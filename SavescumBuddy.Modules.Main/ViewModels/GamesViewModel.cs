using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Wpf.Constants;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Extensions;
using SavescumBuddy.Wpf.Models;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Lib;
using SavescumBuddy.Wpf.Services;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GamesViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBackupService _backupService;

        public GamesViewModel(IRegionManager regionManager, IDataAccess dataAccess, IEventAggregator eventAggregator, IBackupService backupService)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;
            _backupService = backupService;

            Games = new ObservableCollection<GameModel>();

            LoadGamesCommand = new DelegateCommand(LoadGames);
            AddCommand = new DelegateCommand(Add);
            EditCommand = new DelegateCommand<GameModel>(Edit);
            MakeCurrentCommand = new DelegateCommand<GameModel>(MakeCurrent);
            RemoveCommand = new DelegateCommand<GameModel>(Remove);

            LoadGamesCommand.Execute();
        }

        private void Add()
        {
            var parameters = new NavigationParameters
            {
                { "gameId", 0 },
                {
                    "callback", new Action<DialogResult>(result =>
                    {
                        if (result is DialogResult.OK)
                            LoadGamesCommand.Execute();
                    })
                }
            };
            _regionManager.RequestNavigate(RegionNames.Overlay, ViewNames.Game, parameters);
        }

        private void Edit(GameModel game)
        {
            var parameters = new NavigationParameters
            {
                { "gameId", game.Id },
                {
                    "callback", new Action<DialogResult>(result =>
                    {
                        if (result is DialogResult.OK)
                            LoadGamesCommand.Execute();
                    })
                }
            };
            _regionManager.RequestNavigate(RegionNames.Overlay, ViewNames.Game, parameters);
        }

        public ObservableCollection<GameModel> Games { get; private set; }

        private void LoadGames()
        {
            try
            {
                var games = _dataAccess.GetGames();
                var gameModels = games.Select(x => new GameModel(x)).ToList();
                Games.Clear();
                Games.AddRange(gameModels);
                RaisePropertyChanged(nameof(Games));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void Remove(GameModel game)
        {
            try
            {
                if (game.BackupCount != 0)
                {
                    _regionManager.PromptAction(
                        "Are you sure?",
                        "NOTE: all associated backups will also be deleted. Are you sure you wish to proceed?",
                        $"DELETE GAME AND { game.BackupCount } ASSOCIATED BACKUP(S)",
                        "CANCEL",
                        r =>
                        {
                            try
                            {
                                if (r == DialogResult.OK)
                                {
                                    var backups = _dataAccess.SearchBackups(new BackupSearchRequest() { GameId = game.Id });
                                    backups.Backups.ForEach(x => _backupService.DeleteFiles(x));

                                    Games.Remove(game);
                                    _dataAccess.DeleteGame(game.Game);
                                }
                            }
                            catch (Exception ex)
                            {
                                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
                            }
                        });
                }
                else
                {
                    Games.Remove(game);
                    _dataAccess.DeleteGame(game.Game);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void MakeCurrent(GameModel game)
        {
            try
            {
                _dataAccess.SetGameAsCurrent(game.Id);
                LoadGamesCommand.Execute();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext) => LoadGamesCommand.Execute();

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public DelegateCommand LoadGamesCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<GameModel> EditCommand { get; }
        public DelegateCommand<GameModel> MakeCurrentCommand { get; }
        public DelegateCommand<GameModel> RemoveCommand { get; }
    }
}
