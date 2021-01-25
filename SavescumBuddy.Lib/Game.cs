namespace SavescumBuddy.Lib
{
    public class Game : IDbEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SavefilePath { get; set; }
        public string BackupFolder { get; set; }
        public int IsCurrent { get; set; }
        public int BackupCount { get; set; }
    }
}
