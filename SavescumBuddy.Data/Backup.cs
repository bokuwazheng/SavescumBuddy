namespace SavescumBuddy.Data
{
    public class Backup : IDbEntity
    {
        public string Note { get; set; }
        public int IsLiked { get; set; }
        public int IsAutobackup { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string DriveId { get; set; }
        public string DateTimeTag { get; set; }
        public string Picture { get; set; }
        public string Origin { get; set; }
        public string FilePath { get; set; }
    }
}
