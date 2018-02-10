using Dapper;
using MySql.Data.MySqlClient;
using OnlineDeja.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OnlineDeja.Helpers {
    public class DBHelper {
        private static string connectionString;

        static DBHelper() {
            connectionString = Startup.Config.ConnectionString;
        }

        public static IDbConnection GetConnection() {
            MySqlConnection connection = new MySqlConnection {
                ConnectionString = connectionString
            };
            connection.Open();
            return connection;
        }
        /// <summary>
        /// 获取考试试卷的总题数
        /// </summary>
        /// <returns></returns>
        public static int GetAdminTestInfo() {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<int>("SELECT Score FROM TestInfo WHERE OpenId='Admin'").First();
            }
        }

        public static TestInfo GetTestInfo(string OpenId) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<TestInfo>("SELECT * FROM TestInfo WHERE OpenId=@OpenId", new { OpenId }).FirstOrDefault();
            }
        }

        public static UserInfo GetUserInfo(string OpenId) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<UserInfo>("SELECT * FROM UserInfo WHERE OpenId=@OpenId", new { OpenId }).FirstOrDefault();
            }
        }

        public static ItemAction GetItemAction(string OpenId, string BranchID) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<ItemAction>("SELECT * FROM ItemAction WHERE OpenId=@OpenId AND BranchID=@BranchID", new { OpenId, BranchID }).FirstOrDefault();
            }
        }

        public static List<ItemAction> GetItemActions(string OpenId) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<ItemAction>("SELECT * FROM ItemAction WHERE OpenId=@OpenId", new { OpenId }).ToList();
            }
        }

        public static ContestItem GetContestItem(int BranchID) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<ContestItem>("SELECT * FROM ContestItem WHERE BranchID=@BranchID", new { BranchID }).FirstOrDefault();
            }
        }

        private static string FlushedOptions(string options) {
            char[] _ = options.ToCharArray(); for (int i = 0; i < _.Length; i++) _[i] += ((char)('A'-'0')); return string.Join(',', _);
        }

        public static ContestItem GetContestItem(string OpenId, int BranchID) {
            using (IDbConnection conn = GetConnection()) {
                var itemAction = conn.Query<ItemAction>("SELECT * FROM ItemAction WHERE OpenId=@OpenId AND BranchID=@BranchID", new { OpenId, BranchID }).FirstOrDefault();
                if (itemAction == null) return null;
                var contestItem = conn.Query<ContestItem>("SELECT * FROM ContestItem WHERE BranchID=@BranchID", new { BranchID }).FirstOrDefault();
                if (contestItem == null) return null;
                contestItem.Chosen = itemAction.Choice;
                string[] _ = { contestItem.A, contestItem.B, contestItem.C, contestItem.D };
                int pos = int.Parse(itemAction.Options.Substring(0, 1)); contestItem.A = _[pos];
                pos = int.Parse(itemAction.Options.Substring(1, 1)); contestItem.B = _[pos];
                pos = int.Parse(itemAction.Options.Substring(2, 1)); contestItem.C = _[pos];
                pos = int.Parse(itemAction.Options.Substring(3, 1)); contestItem.D = _[pos];
                return contestItem;
            }
        }

        /// <summary>
        /// 为第一次登录的用户产生对应的考题, 题目的选项顺序是被打乱的.
        /// </summary>
        /// <param name="OpenId">用户微信OpenId, 理论上是经由Deja从腾讯服务器获得</param>
        /// <returns>被影响的行数. 如果比0小则数据库出错, 大于0成功(一定是等于总考题数量的);等于0表示不需要重新生成考题.</returns>
        public static int GenerateContestForUser(string OpenId) {
            object o = new { OpenId };
            using (IDbConnection conn = GetConnection()) {
                int UserCount = conn.Query<int>("SELECT COUNT(*) FROM ItemAction WHERE OpenId=@OpenId", o).First();
                int TestCount = GetAdminTestInfo();
                int rowsAffected = 0;
                List<ContestItem> contestItems = conn.Query<ContestItem>("SELECT * FROM ContestItem ORDER BY BranchID").ToList();
                if(UserCount != TestCount) {
                    conn.Execute("DELETE FROM ItemAction WHERE OpenId=@OpenId", o);
                    using (var transaction = conn.BeginTransaction()) {
                        try {
                            for (int i = 0; i < TestCount; i++) {
                                string Options = FlushData();
                                int CorrectID = -1;
                                switch (contestItems[i].TestType) {
                                    case "0":
                                        CorrectID = Options.IndexOf(contestItems[i].Correct.ToString());
                                        break;
                                    case "1":
                                        int[] correct = {
                                            Options.IndexOf((contestItems[i].Correct & 0b1_0_0_0) >> 3 > 0 ? "3" : "A"), //D
                                            Options.IndexOf((contestItems[i].Correct & 0b0_1_0_0) >> 2 > 0 ? "2" : "A"), //C
                                            Options.IndexOf((contestItems[i].Correct & 0b0_0_1_0) >> 1 > 0 ? "1" : "A"), //B
                                            Options.IndexOf((contestItems[i].Correct & 0b0_0_0_1) > 0 ? "0" : "A")       //A
                                        };
                                        int sum = 0;
                                        for(int j = correct.Length - 1; j >= 0; j--) {
                                            if (correct[j] >= 0) sum += (1 << correct[j]);
                                        }
                                        CorrectID = sum;
                                        break;
                                }
                                rowsAffected += conn.Execute("INSERT INTO ItemAction(OpenId,BranchID,CorrectID,Options) VALUES(@OpenId,@BranchID,@CorrectID,@Options)",
                                            new { OpenId, BranchID = i, CorrectID , Options });
                            }
                            transaction.Commit();
                        }catch(Exception e) {
                            transaction.Rollback();
                            return -1;
                        }
                    }
                    conn.Execute("DELETE FROM TestInfo WHERE OpenID=@OpenId", o);
                    conn.Execute("INSERT INTO TestInfo(OpenId,Score,ContestFinished,CurrentIndex) VALUES(@OpenId,0,0,0)", o);
                }
                return rowsAffected;
            }
        }

        public static int SubmitChoice(string OpenId, int BranchID, int Choice, decimal Latitude, decimal Longitude) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Execute("UPDATE ItemAction SET Choice=@Choice,Lat=@Latitude,Lon=@Longitude,SubmitTime=now() WHERE OpenId=@OpenId AND BranchID=@BranchID",
                    new { Choice, Latitude, Longitude, OpenId, BranchID });
            }
        }

        public static int SubmitChoice(int BranchID, UploadRequest bite) {
            return SubmitChoice(bite.OpenId, BranchID, bite.Choice, bite.Latitude, bite.Longitude);
        }

        public static int SyncUserInfo(UserInfo info) {
            if (info == null) return -1;

            using (IDbConnection conn = GetConnection()) {
                int count = conn.Query<int>("SELECT COUNT(*) FROM UserInfo WHERE OpenId=@OpenId", new { info.OpenId }).First();
                var o = new { info.OpenId, info.NickName, info.Gender, info.City, info.Province, info.Country, info.AvatarUrl, info.UnionId };

                return conn.Execute(count == 0 ?
                    "INSERT INTO UserInfo(OpenID,NickName,Gender,City,Province,Country,AvatarUrl,UnionID) VALUES(@OpenID,@NickName,@Gender,@City,@Province,@Country,@AvatarUrl,@UnionID)" :
                    "UPDATE UserInfo SET NickName=@NickName,Gender=@Gender,City=@City,Province=@Province,Country=@Country,AvatarUrl=@AvatarUrl,UnionID=@UnionID WHERE OpenId=@OpenId"
                    , o);
            }
        }

        private static string FlushData(int flushTimes = 64) {
            char[] cards = { '0', '1', '2', '3' }; char swap; Random r = new Random();
            for(int i = 0; i < flushTimes; i++) {
                int r0 = r.Next(3), r1 = r.Next(3);
                if (r0 != r1) { swap = cards[r0]; cards[r0] = cards[r1]; cards[r1] = swap; }
            }
            return new string(cards);
        }

        public static Summary IsFinished(string OpenId) {
            Summary summary = null; object o = new { OpenId };
            using (IDbConnection conn = GetConnection()) {
                var finished = conn.Query<string>("SELECT ContestFinished FROM TestInfo WHERE OpenId=@OpenId", o).FirstOrDefault();
                if (string.IsNullOrEmpty(finished)) return null;
                summary = new Summary();
                var total = conn.Query<int>("SELECT Score FROM TestInfo WHERE OpenId='Admin'").First();
                var right = conn.Query<int>("SELECT COUNT(*) FROM ItemAction WHERE OpenId=@OpenId AND Choice=CorrectID", o).First();
                summary.Total = total;
                summary.Right = right;
            }
            return summary;
        }

        public static int GetCurrentContest(string OpenId) {
            using (IDbConnection conn = GetConnection()) {
                return conn.Query<int>("SELECT IFNULL(MIN(BranchID),-1) FROM ItemAction WHERE OpenId=@OpenId AND Choice IS NULL", new { OpenId }).First();
            }
        }

        public static List<TopUser> GetTopUsers(int count = 5) {
            var total = GetAdminTestInfo();
            if (total == 0) return null;

            using (IDbConnection conn = GetConnection()) {
                var users = conn.Query<TopUser>($"SELECT B.Nickname,B.AvatarUrl,CONCAT(CONVERT(COUNT(*)/{total}*100,DECIMAL),'%') AS Score,A.OpenId FROM ItemAction A JOIN UserInfo B ON A.OpenId=B.OpenId WHERE Choice=CorrectID GROUP BY OpenId ORDER BY 3 DESC");
                return users.Take(count).ToList();
            }
        }

        public static int DealConfess(string OpenId) {
            var o = new { OpenId };
            using (IDbConnection conn = GetConnection()) {
                int choices = conn.Query<int>("SELECT IFNULL(COUNT(*), -1) FROM ItemAction WHERE OpenId=@OpenId AND Choice IS NOT NULL", o).First();
                int total = GetAdminTestInfo();
                if (choices != total) return -1;
                return conn.Execute("UPDATE TestInfo SET ContestFinished='1' WHERE OpenId=@OpenId", o);
            }
        }

        public static ResetResult ResetUser(string OpenId) {
            ResetResult result = new ResetResult() { Success = false };
            object o = new { OpenId };
            string template = "DELETE FROM ### WHERE OpenId=@OpenId";
            string[] tables = { "UserInfo", "TestInfo", "ItemAction" };
            using (IDbConnection conn = GetConnection()) {
                var sql = template.Replace("###", tables[0]);
                result.UserInfoDeleted = conn.Execute(sql, o);
                sql = template.Replace("###", tables[1]);
                result.TestInfoDeleted = conn.Execute(sql, o);
                sql = template.Replace("###", tables[2]);
                result.ItemActionDeleted = conn.Execute(sql, o);
                result.Success = true;
            }
            return result;
        }
    }
}
