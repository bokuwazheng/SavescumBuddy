using Prism.Mvvm;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class Backup : BindableBase, IDbEntity
    {
        private string _note;
        private string _driveId;
        private string _filePath;
        private string _picture;

        public string Note { get => _note; set => SetProperty(ref _note, value); }
        public int IsLiked { get; set; }
        public int IsAutobackup { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string DriveId { get => _driveId; set => SetProperty(ref _driveId, value); }
        public string DateTimeTag { get; set; }
        public string Picture { get => _picture; set => SetProperty(ref _picture, value); }
        public string Origin { get; set; }
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
    }
}