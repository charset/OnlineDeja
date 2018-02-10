namespace OnlineDeja.Models {
    public class UploadRequest {
        public decimal Longitude;
        public decimal Latitude;
        public int Choice;
        public string OpenId;
    }

    public class CurrentResult : AResult {
        public int Current;
    }
}
