namespace OnlineDeja.Models {
    public class ResetResult : AResult {
        public int UserInfoDeleted { get; set; }
        public int TestInfoDeleted { get; set; }
        public int ItemActionDeleted { get; set; }
    }
}