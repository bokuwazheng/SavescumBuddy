namespace SavescumBuddy.Data
{
    public class Backup : IDbEntity
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GoogleDriveId { get; set; }
        public string Note { get; set; }
        public int IsLiked { get; set; }
        public int IsAutobackup { get; set; }
        public string TimeStamp { get; set; }
        public string OriginPath { get; set; }
        public string SavefilePath { get; set; }
        public string PicturePath { get; set; }
    }
}
