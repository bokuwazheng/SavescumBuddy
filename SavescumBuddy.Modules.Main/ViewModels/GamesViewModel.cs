using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Data;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GamesViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenFileService _openFileService;

        public GamesViewModel(IRegionManager regionManager, IDataAccess dataAccess, IEventAggregator eventAggregator, IOpenFileService openFileService)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;
            _openFileService = openFileService;

            Games = new ObservableCollection<GameModel>();

            LoadGamesCommand = new DelegateCommand(LoadGames);
            AddGameCommand = new DelegateCommand(() => Games.Add(new GameModel(new Game())));
            UpdateGameCommand = new DelegateCommand<GameModel>(UpdateGame);
            SetCurrentCommand = new DelegateCommand<GameModel>(x => _dataAccess.SetGameAsCurrent(x.Game));
            RemoveGameCommand = new DelegateCommand<GameModel>(RemoveGame);
            OpenFilePickerCommand = new DelegateCommand<GameModel>(GetSavefilePath);
            OpenFolderPickerCommand = new DelegateCommand<GameModel>(GetBackupFolderPath);

            LoadGamesCommand.Execute();
        }
        
        public ObservableCollection<GameModel> Games { get; private set; }

        private void GetSavefilePath(GameModel game)
        {
            _openFileService.IsFolderPicker = false;
            _openFileService.Multiselect = false;
            _openFileService.ShowHiddenItems = true;

            if (_openFileService.OpenFile())
            {
                game.SavefilePath = _openFileService.FileName;
            }
        }

        private void GetBackupFolderPath(GameModel game)
        {
            _openFileService.IsFolderPicker = true;
            _openFileService.Multiselect = false;
            _openFileService.ShowHiddenItems = true;

            if (_openFileService.OpenFile())
            {
                game.BackupFolder = _openFileService.FileName;
            }
        }

        private void LoadGames()
        {
            var games = _dataAccess.LoadGames();
            var gameModels = games.Select(x => new GameModel(x));
            Games = new ObservableCollection<GameModel>(gameModels);
        }

        private void UpdateGame(GameModel game)
        {
            try
            {
                if (game.Id == 0)
                    _dataAccess.SaveGame(game.Game);
                else
                    _dataAccess.UpdateGame(game.Game);
            }
            catch
            {

            }
        }

        private void RemoveGame(GameModel game)
        {
            try
            {
                Games.Remove(game);
                _dataAccess.RemoveGame(game.Game);
            }
            catch
            {

            }
        }

        public DelegateCommand LoadGamesCommand { get; }
        public DelegateCommand AddGameCommand { get; }
        public DelegateCommand<GameModel> SaveGameCommand { get; }
        public DelegateCommand<GameModel> UpdateGameCommand { get; }
        public DelegateCommand<GameModel> SetCurrentCommand { get; }
        public DelegateCommand<GameModel> RemoveGameCommand { get; }
        public DelegateCommand<GameModel> OpenFilePickerCommand { get; }
        public DelegateCommand<GameModel> OpenFolderPickerCommand { get; }
    }
}
