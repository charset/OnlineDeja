using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineDeja.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using log4net;

namespace OnlineDeja.Helpers {
    public static class DejaHelper {
        static string Appid;
        static string Secret;
        static ILog iLog;

        static DejaHelper() {
            Appid = Startup.Config.AppSettings.Appid;
            Secret = Startup.Config.AppSettings.Secret;
            iLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public static UserInfo DeserializeUserInfo(string json) {
            if (string.IsNullOrEmpty(json)) return null;

            UserInfo _ui = JsonConvert.DeserializeObject<UserInfo>(json);
            return _ui;
        }

        public static UploadRequest DeserializeBite(string json) {
            UploadRequest obj = null;
            try {
                obj = JsonConvert.DeserializeObject<UploadRequest>(json);
            } catch (Exception) {

            }
            return obj;
        }

        private static T Deserialize<T>(string json) where T : class {
            T t = default(T);
            try {
                t = JsonConvert.DeserializeObject<T>(json);
            } catch (Exception) {

            }
            return t;
        }

        public static SubmitChoice DeserializeSubmitChoice(string json) {
            return Deserialize<SubmitChoice>(json);
        }

        public static AuthenticateResult SyncOpenIdFromWeChat(string code) {//, string iv, string encryptedData) {
            AuthenticateResult ar = new AuthenticateResult();
            string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={Appid}&secret={Secret}&js_code={code}&grant_type=authorization_code";
            string content;
            try {
                WebRequest wReq = WebRequest.Create(url);
                WebResponse wResp = wReq.GetResponse();
                Stream respStream = wResp.GetResponseStream();
                using (var reader = new StreamReader(respStream, Encoding.UTF8)) {
                    content = reader.ReadToEnd();
                }
            } catch (Exception ex) {
                ar.ErrCode = ex.Message; ar.ErrMsg = ex.StackTrace;
                return ar;
            }

            JObject @object = (JObject)JsonConvert.DeserializeObject(content);
            try {
                ar.OpenId = @object["openid"].ToString();
                ar.SessionKey = @object["session_key"].ToString();
            } catch (Exception) {
                ar.ErrCode = @object["errcode"].ToString();
                ar.ErrMsg = @object["errmsg"].ToString();
                return ar;
            }
            ar.Success = true;
            return ar;
        }

        public static T GetCurrentContextObject<T>(this IHttpContextAccessor accessor) where T : class {
            var context = accessor.HttpContext;
            using (StreamReader sr = new StreamReader(context.Request.Body)) {
                string content = sr.ReadToEnd();
                T obj = Deserialize<T>(content);
                return obj;
            }
        }

        public static UserInfo GetCurrentContextUserInfo(this IHttpContextAccessor accessor) {
            var context = accessor.HttpContext;
            using (StreamReader sr = new StreamReader(context.Request.Body)) {
                string content = sr.ReadToEnd();
                System.Diagnostics.Debug.WriteLine(content);
                var userInfo = (JObject)JsonConvert.DeserializeObject(content);
                var _ = userInfo["userInfo"];
                if(_ != null && _.HasValues) {
                    return JsonConvert.DeserializeObject<UserInfo>(_.ToString());
                }
                return null;
            }
        }

        public static string GetCurrentContextOpenId(this IHttpContextAccessor accessor) {
            var context = accessor.HttpContext;
            using (StreamReader sr = new StreamReader(context.Request.Body)) {
                string content = sr.ReadToEnd();
                if (string.IsNullOrEmpty(content)) return null;
                JObject _ = (JObject)JsonConvert.DeserializeObject(content);
                if (_ == null) return null;
                var token = _.GetValue("openId");
                if (token == null) return null;
                return token.ToString();
            }
        }
    }
}
