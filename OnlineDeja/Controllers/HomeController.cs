using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineDeja.Helpers;
using OnlineDeja.Models;

namespace OnlineDeja.Controllers {
    public class HomeController : Controller {
        private IHttpContextAccessor accessor;
        private static object copyleft = new {
            Title = "Willkommen, armer Kerl ~",
            ICPS = "",
            Host = "https://page404.top/",
            WebServer = "Apache Hubschrauber : apache.org"
        };
        private JsonResult indexResult = new JsonResult(copyleft);

        public HomeController(IHttpContextAccessor accessor) {
            this.accessor = accessor;
        }

        public JsonResult Index(string ID) {
            return indexResult;
        }

        public ActionResult Error() {
            ViewBag.Title = "Error";
            return View();
        }

        [HttpPost]
        public JsonResult Code(string ID) {
            var ar = DejaHelper.SyncOpenIdFromWeChat(ID);
            ar.Success = false;
            if (!string.IsNullOrEmpty(ar.OpenId)) {
                var ui = accessor.GetCurrentContextUserInfo();
                if (ui != null) {
                    ui.OpenId = ar.OpenId;
                    DBHelper.SyncUserInfo(ui);
                    if (ui != null && !string.IsNullOrEmpty(ui.OpenId)) {
                        DBHelper.GenerateContestForUser(ui.OpenId);
                        ar.IsFinished = (DBHelper.GetTestInfo(ui.OpenId).ContestFinished != "0");
                        if (!ar.IsFinished) {
                            ar.CurrentID = DBHelper.GetCurrentContest(ui.OpenId);
                        } else {
                            ar.CurrentID = -1;
                        }
                        ar.Success = true;
                    } else {
                        ar.ErrMsg = "无法为参加考生分配试卷";
                    }
                } else {
                    ar.ErrMsg = "无法反序列化Sync";
                }
            } else {
                ar.ErrMsg = "无法从微信获得OpenId";
            }

            return new JsonResult(ar);
        }

        [HttpPost]
        public JsonResult Summary() {
            SummaryResult result = new SummaryResult() { Success = false };
            var OpenId = accessor.GetCurrentContextOpenId();
            result.Summary = DBHelper.IsFinished(OpenId);
            result.Success = (result.Summary != null);
            result.TopUsers = DBHelper.GetTopUsers();
            result.CurrentIndex = DBHelper.GetCurrentContest(OpenId);
            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult Reset(string ID) {
            var OpenId = accessor.GetCurrentContextOpenId();
            var result = DBHelper.ResetUser(OpenId);
            result.Success = true;
            return new JsonResult(result);
        }
    }
}