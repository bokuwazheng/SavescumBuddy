﻿using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
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
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly IEventAggregator _eventAggregator;

        public GamesViewModel(IRegionManager regionManager, IDataAccess dataAccess, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;

            Games = new ObservableCollection<GameModel>();

            LoadGamesCommand = new DelegateCommand(LoadGames);
            AddCommand = new DelegateCommand(() => Edit(new GameModel(new Game())));
            EditCommand = new DelegateCommand<GameModel>(Edit);
            MakeCurrentCommand = new DelegateCommand<GameModel>(MakeCurrent);
            RemoveCommand = new DelegateCommand<GameModel>(Remove);

            LoadGamesCommand.Execute();
        }

        private void Edit(GameModel game)
        {
            var parameters = new NavigationParameters
            {
                { "game", game.Game },
                {
                    "callback", new Action<Game>(result =>
                    {
                        var activeRegion = _regionManager.Regions[RegionNames.Overlay].ActiveViews.FirstOrDefault();
                     
                        if (activeRegion is object)
                            _regionManager.Regions[RegionNames.Overlay].Deactivate(activeRegion);

                        if (result is null)
                        {
                            return;
                        }

                        result.Id = game.Id;
                        result.IsCurrent = game.IsCurrent;

                        if (result.Id == 0)
                        {
                            _dataAccess.SaveGame(result);
                            Games.Add(new GameModel(result));
                        }
                        else
                        {
                            _dataAccess.UpdateGame(result);
                            var g = Games.IndexOf(game);
                            Games[g] = new GameModel(result);
                        }
                    })
                }
            };
            _regionManager.RequestNavigate(RegionNames.Overlay, "Game", parameters);
        }

        public ObservableCollection<GameModel> Games { get; private set; }

        private void LoadGames()
        {
            try
            {
                var games = _dataAccess.LoadGames();
                var gameModels = games.Select(x => new GameModel(x));
                Games = new ObservableCollection<GameModel>(gameModels);
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
                Games.Remove(game);
                _dataAccess.RemoveGame(game.Game);
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
                _dataAccess.SetGameAsCurrent(game.Game);
                LoadGamesCommand.Execute();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public DelegateCommand LoadGamesCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<GameModel> EditCommand { get; }
        public DelegateCommand<GameModel> MakeCurrentCommand { get; }
        public DelegateCommand<GameModel> RemoveCommand { get; }
    }
}
