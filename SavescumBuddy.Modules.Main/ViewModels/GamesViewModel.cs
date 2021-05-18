using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Wpf.Constants;
using SavescumBuddy.Wpf.Extensions;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Lib;
using SavescumBuddy.Wpf.Services;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GamesViewModel : BaseViewModel, INavigationAware
    {
        private readonly IDataAccess _dataAccess;
        private readonly IBackupService _backupService;

        public GamesViewModel(IRegionManager regionManager, IDataAccess dataAccess, IEventAggregator eventAggregator, IBackupService backupService) : base(regionManager, eventAggregator)
        {
            _dataAccess = dataAccess;
            _backupService = backupService;

            RefreshCommand = new DelegateCommand(LoadGames);
            AddCommand = new DelegateCommand(() => OpenGameDialog(0));
            EditCommand = new DelegateCommand<Game>(x => OpenGameDialog(x.Id));
            MakeCurrentCommand = new DelegateCommand<Game>(MakeCurrent);
            RemoveCommand = new DelegateCommand<Game>(Remove);
        }

        public ObservableCollection<Game> Games { get; } = new();

        private void OpenGameDialog(int id) => Handle(() =>
        {
            var parameters = new NavigationParameters
            {
                { "gameId", id },
                {
                    "callback", new Action<DialogResult>(result =>
                    {
                        if (result is DialogResult.OK)
                            RefreshCommand.Execute();
                    })
                }
            };
            ShowDialog(ViewNames.Game, parameters);
        });

        private void LoadGames() => Handle(() =>
        {
            var games = _dataAccess.GetGames();
            Games.ReplaceRange(games);
        });

        private void Remove(Game game) => Handle(() =>
        {
            var anyBackups = _dataAccess.GetGame(game.Id).BackupCount > 0; // Check backup count cuz it doesn't update when a backup is created via hotkey or when a scheduled backup happen.

            void removeGame() // Backups are removed via FK constraint. 
            {
                Games.Remove(game);
                _dataAccess.DeleteGame(game);
            }
            
            if (anyBackups)
            {
                PromptAction(
                    "Are you sure?",
                    "NOTE: all associated backups will also be deleted. Are you sure you wish to proceed?",
                    $"DELETE GAME AND { game.BackupCount } ASSOCIATED BACKUP(S)",
                    "CANCEL",
                    r =>
                    {
                        if (r is DialogResult.OK)
                        {
                            var response = _dataAccess.SearchBackups(new BackupSearchRequest() { GameId = game.Id });
                            response.Backups.ForEach(x => _backupService.DeleteFiles(x));

                            removeGame();
                        }
                    });
            }
            else
            {
                removeGame();
            }
        });

        private void MakeCurrent(Game game) => Handle(() =>
        {
            _dataAccess.SetGameAsCurrent(game.Id);
            RefreshCommand.Execute();
        });

        public void OnNavigatedTo(NavigationContext navigationContext) => RefreshCommand.Execute();

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<Game> EditCommand { get; }
        public DelegateCommand<Game> MakeCurrentCommand { get; }
        public DelegateCommand<Game> RemoveCommand { get; }
    }
}
