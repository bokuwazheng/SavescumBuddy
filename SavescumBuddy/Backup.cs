using System.IO;
using Prism.Mvvm;
using Prism.Commands;

namespace SavescumBuddy
{
    public class Backup : BindableBase, IDbEntity
    {
        private string _note;
        public string Note
        {
            get { return _note; }
            set { _note = value; RaisePropertyChanged("Note"); }
        }
        public int IsLiked { get; set; }
        public int IsAutobackup { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string DateTimeTag { get; set; }
        public string Picture { get; set; }
        public string Origin { get; set; }
        public string FilePath { get; set; }

        public Backup()
        {
            UpdateNoteCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateNote(this);
            });

            UpdateIsLikedCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateIsLiked(this);
            });

            ShowInExplorerCommand = new DelegateCommand(() =>
            {
                var folder = Path.GetDirectoryName(FilePath);

                try
                {
                    System.Diagnostics.Process.Start(folder);
                }
                catch
                {
                    Util.PopUp($"Folder ({folder}) doesn't exist.");
                }
            });
        }

        public DelegateCommand UpdateNoteCommand { get; }
        public DelegateCommand UpdateIsLikedCommand { get; }
        public DelegateCommand ShowInExplorerCommand { get; }
    }
}
