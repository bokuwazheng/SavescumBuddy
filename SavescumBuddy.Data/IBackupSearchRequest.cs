namespace SavescumBuddy.Data
{
    public interface IBackupSearchRequest
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public bool Descending { get; set; }
        public string GroupBy { get; set; }
        public bool? IsLiked { get; set; }
        public bool? IsAutobackup { get; set; }
        public int GameId { get; set; }
        public bool? IsInGoogleDrive { get; set; }
        public string Note { get; set; }
    }
}
