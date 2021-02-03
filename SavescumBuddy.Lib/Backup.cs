namespace SavescumBuddy.Lib
{
    public class Backup : IDbEntity
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public int IsInGoogleDrive { get; set; }
        public string Note { get; set; }
        public int IsLiked { get; set; }
        public int IsScheduled { get; set; }
        public long TimeStamp { get; set; }
        public string OriginPath { get; set; }
        public string SavefilePath { get; set; }
        public string PicturePath { get; set; }
    }
}
