using Prism.Mvvm;
using SavescumBuddy.Data;
using System;

namespace SavescumBuddy.Modules.Main.Models
{
    public class GameModel : BindableBase
    {
        private int _id;
        private string _title;
        private string _savefilePath;
        private string _backupFolder;
        private int _canBeSetCurrent;
        private int _isCurrent;

        public GameModel(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public Game Game { get; }

        public int Id { get => Game.Id; set { Game.Id = value; RaisePropertyChanged(nameof(Id)); } }
        public string Title { get => Game.Title; set { Game.Title = value; RaisePropertyChanged(nameof(Title)); } }
        public string SavefilePath { get => Game.SavefilePath; set { Game.SavefilePath = value; RaisePropertyChanged(nameof(SavefilePath)); } }
        public string BackupFolder { get => Game.BackupFolder; set { Game.BackupFolder = value; RaisePropertyChanged(nameof(BackupFolder)); } }
        public int IsCurrent { get => Game.IsCurrent; set { Game.IsCurrent = value; RaisePropertyChanged(nameof(IsCurrent)); } }
    }
}
