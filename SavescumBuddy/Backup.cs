using Prism.Mvvm;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class Backup : BindableBase, IDbEntity
    {
        private string _note;
        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }
        public int IsLiked { get; set; }
        public int InCloud { get; set; }
        public int IsAutobackup { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string DateTimeTag { get; set; }
        public string Picture { get; set; }
        public string Origin { get; set; }
        public string FilePath { get; set; }
    }
}
