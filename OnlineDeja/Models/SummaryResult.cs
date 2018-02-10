using System.Collections.Generic;

namespace OnlineDeja.Models {
    public class SummaryResult : AResult {
        public Summary Summary { get; set; }
        public List<TopUser> TopUsers { get; set; }
        public int CurrentIndex { get; set; }
    }
}