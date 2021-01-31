using Prism.Mvvm;
using SavescumBuddy.Lib;
using System;

namespace SavescumBuddy.Wpf.Models
{
    public class GameModel : BindableBase
    {
        public GameModel(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public Game Game { get; }

        public int Id => Game.Id;
        public string Title { get => Game.Title; set { Game.Title = value; RaisePropertyChanged(nameof(Title)); } }
        public string SavefilePath { get => Game.SavefilePath; set { Game.SavefilePath = value; RaisePropertyChanged(nameof(SavefilePath)); } }
        public string BackupFolder { get => Game.BackupFolder; set { Game.BackupFolder = value; RaisePropertyChanged(nameof(BackupFolder)); } }
        public int IsCurrent => Game.IsCurrent;
        public int BackupCount=> Game.BackupCount;
    }
}
