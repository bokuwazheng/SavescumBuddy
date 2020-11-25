namespace SavescumBuddy.Data
{
    public class BackupSearchRequest : IBackupSearchRequest
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public bool Descending { get; set; }
        public string GroupBy { get; set; }
        public string Note { get; set; }
        public bool? IsLiked { get; set; }
        public bool? IsAutobackup { get; set; }
        public bool? IsInGoogleDrive { get; set; }
        public int? GameId { get; set; }
    }
}
