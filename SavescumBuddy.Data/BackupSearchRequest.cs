namespace SavescumBuddy.Data
{
    public class BackupSearchRequest : IBackupSearchRequest
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public bool Order { get; set; }
        public string GroupBy { get; set; }
        public string Note { get; set; }
        public bool? Liked { get; set; }
        public bool? Autobackups { get; set; }
        public bool? Current { get; set; }
        public bool? IsInGoogleDrive { get; set; }
    }
}
