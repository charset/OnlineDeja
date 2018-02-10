namespace OnlineDeja.Models {
    public class AuthenticateResult : AResult {
        public string OpenId { get; set; }
        public string SessionKey { get; set; }

        public bool IsFinished { get; set; }
        public int CurrentID { get; set; }
    }
}