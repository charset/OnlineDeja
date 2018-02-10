namespace OnlineDeja.Models {
    /// <summary>
    /// 
    /// </summary>
    public abstract class AResult {
        public string ErrCode { get; set; }
        public string ErrMsg { get; set; }
        public bool Success { get; set; }
        public int RowsAffected { get; set; }
    }
}