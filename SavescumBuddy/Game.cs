using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;

namespace SavescumBuddy
{
    public class Game : BindableBase, IDbEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SavefilePath { get; set; }
        public string BackupFolder { get; set; }
        public int CanBeSetCurrent { get; set; }
        public int IsCurrent { get; set; }

        public Game()
        {
            UpdateGameCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateGame(this);
                RaiseStateChanged();
            });

            SetCurrentCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.SetGameAsCurrent(this);
                RaiseStateChanged();
            });

            RemoveGameCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.RemoveGame(this);
                RaiseStateChanged();
            });

            OpenFilePickerCommand = new DelegateCommand(() =>
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.ShowHiddenItems = true;

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        SavefilePath = dialog.FileName;
                        RaisePropertyChanged("SavefilePath");
                    }
                }
            });

            OpenFolderPickerCommand = new DelegateCommand(() =>
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.IsFolderPicker = true;
                    dialog.ShowHiddenItems = true;

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        BackupFolder = dialog.FileName;
                        RaisePropertyChanged("BackupFolder");
                    }
                }
            });
        }

        public DelegateCommand UpdateGameCommand { get; }
        public DelegateCommand SetCurrentCommand { get; }
        public DelegateCommand RemoveGameCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }

        public interface IListItemEventListener
        {
            void StateChanged();
        }

        private IListItemEventListener listener;

        public void SetListener(IListItemEventListener listener)
        {
            this.listener = listener;
        }

        private void RaiseStateChanged()
        {
            if (listener != null) listener.StateChanged();
        }
    }
}
