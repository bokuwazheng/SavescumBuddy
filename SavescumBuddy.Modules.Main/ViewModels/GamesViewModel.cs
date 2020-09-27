using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GamesViewModel : BindableBase
    {
        private readonly IDataAccess _dataAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenFileService _openFileService;

        public GamesViewModel(IDataAccess dataAccess, IEventAggregator eventAggregator, IOpenFileService openFileService)
        {
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
            try
            {
                _openFileService.IsFolderPicker = false;
                _openFileService.Multiselect = false;
                _openFileService.ShowHiddenItems = true;

                if (_openFileService.OpenFile())
                    game.SavefilePath = _openFileService.FileName;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void GetBackupFolderPath(GameModel game)
        {
            try
            {
                _openFileService.IsFolderPicker = true;
                _openFileService.Multiselect = false;
                _openFileService.ShowHiddenItems = true;

                if (_openFileService.OpenFile())
                {
                    game.BackupFolder = _openFileService.FileName;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void LoadGames()
        {
            try
            {
                var games = _dataAccess.LoadGames();
                var gameModels = games.Select(x => new GameModel(x));
                Games = new ObservableCollection<GameModel>(gameModels);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
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
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void RemoveGame(GameModel game)
        {
            try
            {
                Games.Remove(game);
                _dataAccess.RemoveGame(game.Game);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public DelegateCommand LoadGamesCommand { get; }
        public DelegateCommand AddGameCommand { get; }
        public DelegateCommand<GameModel> UpdateGameCommand { get; }
        public DelegateCommand<GameModel> SetCurrentCommand { get; }
        public DelegateCommand<GameModel> RemoveGameCommand { get; }
        public DelegateCommand<GameModel> OpenFilePickerCommand { get; }
        public DelegateCommand<GameModel> OpenFolderPickerCommand { get; }
    }
}
