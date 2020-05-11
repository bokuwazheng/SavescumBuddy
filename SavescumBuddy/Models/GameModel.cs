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
            
            UpdateGameCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateGame(Game);
                StateChanged?.Invoke();
            });

            SetCurrentCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.SetGameAsCurrent(Game);
                StateChanged?.Invoke();
            });

            RemoveGameCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.RemoveGame(Game);
                StateChanged?.Invoke();
            });

            OpenFilePickerCommand = new DelegateCommand(() =>
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
                        Game.BackupFolder = dialog.FileName;
                    }
                }
            });
        }

        public DelegateCommand UpdateGameCommand { get; }
        public DelegateCommand SetCurrentCommand { get; }
        public DelegateCommand RemoveGameCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }
    }
}
