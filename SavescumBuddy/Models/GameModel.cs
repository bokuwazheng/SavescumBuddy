using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy.Models
{
    public class GameModel : BindableBase
    {
        public Game Game { get; }
        public event Action StateChanged;

        public GameModel(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));

            UpdateGameCommand = new DelegateCommand(UpdateGame);
            SetCurrentCommand = new DelegateCommand(SetCurrentGame);
            RemoveGameCommand = new DelegateCommand(RemoveGame);
            OpenFilePickerCommand = new DelegateCommand(OpenFilePicker);
            OpenFolderPickerCommand = new DelegateCommand(OpenFolderPicker);
        }

        private void UpdateGame()
        {
            SqliteDataAccess.UpdateGame(Game);
            StateChanged?.Invoke();
        }

        private void SetCurrentGame()
        {
            SqliteDataAccess.SetGameAsCurrent(Game);
            StateChanged?.Invoke();
        }

        private void RemoveGame()
        {
            SqliteDataAccess.RemoveGame(Game);
            StateChanged?.Invoke();
        }

        private void OpenFilePicker()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.ShowHiddenItems = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Game.SavefilePath = dialog.FileName;
                }
            }
        }

        private void OpenFolderPicker()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.IsFolderPicker = true;
                dialog.ShowHiddenItems = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Game.BackupFolder = dialog.FileName;
                }
            }
        }

        public DelegateCommand UpdateGameCommand { get; }
        public DelegateCommand SetCurrentCommand { get; }
        public DelegateCommand RemoveGameCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }
    }
}
