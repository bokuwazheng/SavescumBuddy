using Prism.Mvvm;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class Game : BindableBase, IDbEntity
    {
        private string _savefilePath;
        private string _backupFolder;
        
        public int Id { get; set; }
        public string Title { get; set; }
        public string SavefilePath { get => _savefilePath; set => SetProperty(ref _savefilePath, value); }
        public string BackupFolder { get => _backupFolder; set => SetProperty(ref _backupFolder, value); }
        public int CanBeSetCurrent { get; set; }
        public int IsCurrent { get; set; }
    }
}
