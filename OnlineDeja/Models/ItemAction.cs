using System;

namespace OnlineDeja.Models {
    public class ItemAction {
        /// <summary>
        /// 维度
        /// </summary>
        public decimal Lat { get; set; }
        /// <summary>
        /// 精度
        /// </summary>
        public decimal Lon { get; set; }

        public DateTime SubmitTime { get; set; }

        public string OpenId { get; set; }

        public int BranchID { get; set; }
        public string Choice { get; set; }

        private int CorrectID { get; set; }
        public string Options { get; set; }
    }
}