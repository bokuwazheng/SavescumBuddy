namespace SavescumBuddy.Data
{
    public interface IBackupSearchRequest
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public string Order { get; set; }
        public string GroupBy { get; set; }
        public bool LikedOnly { get; set; }
        public bool HideAutobackups { get; set; }
        public bool CurrentOnly { get; set; }
        public string Note { get; set; }
    }
}
