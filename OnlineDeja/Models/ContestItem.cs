using Newtonsoft.Json;

namespace OnlineDeja.Models {
    public class ContestItem {
        public int BranchID { get; set; }
        public string Content { get; set; }
        public string TestType { get; set; }
        public int BranchCount { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string H { get; set; }
        [JsonIgnore]
        public int Correct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Chosen { get; set; }
    }
}