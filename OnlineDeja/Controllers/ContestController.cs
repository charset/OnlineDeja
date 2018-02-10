using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineDeja.Models;
using System.IO;
using OnlineDeja.Helpers;

namespace OnlineDeja.Controllers {
    public class ContestController : Controller {
        private IHttpContextAccessor accessor;
        public ContestController(IHttpContextAccessor accessor) {
            this.accessor = accessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult View(int ID) {
            var ci = DBHelper.GetContestItem(ID);
            return new JsonResult(ci);
        }

        [HttpPost]
        public JsonResult Submit(int ID) {
            SubmitResult result = new SubmitResult() { Success = false };
            var uploadRequest = accessor.GetCurrentContextObject<UploadRequest>();
            var testInfo = DBHelper.GetTestInfo(uploadRequest.OpenId);

            if (testInfo == null) {
                result.ErrMsg = "无法找到测试者的信息对应的试题";
            } else {
                if (testInfo.ContestFinished != "0") {
                    result.NoMore = true; result.Success = true;
                } else {
                    int raw = DBHelper.SubmitChoice(ID, uploadRequest);
                    result.Success = true; result.NoMore = DBHelper.GetAdminTestInfo() <= ID + 1; result.RowsAffected = raw;
                }
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult Test(int ID) {
            string OpenId = accessor.GetCurrentContextOpenId();
            var contestItem = DBHelper.GetContestItem(OpenId, ID);
            return new JsonResult(contestItem);
        }

        [HttpPost]
        public JsonResult Upload(int ID) {
            var sc = accessor.GetCurrentContextObject<SubmitChoice>();
            var raw = DBHelper.SubmitChoice(sc.OpenId, sc.BranchID, sc.Choice, sc.Latitude, sc.Longitude);
            return new JsonResult(raw);
        }

        [HttpPost]
        public JsonResult Deal() {
            string OpenId = accessor.GetCurrentContextOpenId();
            var raw = DBHelper.DealConfess(OpenId);
            return new JsonResult(raw);
        }
    }
}
