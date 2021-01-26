using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Extensions;
using SavescumBuddy.Lib;
using SavescumBuddy.Services.Interfaces;
using System;
using SavescumBuddy.Wpf.Models;
using SavescumBuddy.Wpf.Constants;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class GameViewModel : BindableBase, INavigationAware
    {
        private GameModel _game;
        private Action<DialogResult> _requestClose;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenFileService _openFileService;
        private readonly IDataAccess _dataAccess;
        private IRegionNavigationService _navigationService;

        public GameViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IOpenFileService openFileService, IDataAccess dataAccess)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _openFileService = openFileService;
            _dataAccess = dataAccess;

            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
            OpenFilePickerCommand = new DelegateCommand(GetSavefilePath);
            OpenFolderPickerCommand = new DelegateCommand(GetBackupFolderPath);
        }

        public GameModel Game { get => _game; set => SetProperty(ref _game, value); }

        private void CloseDialog(DialogResult? result)
        {
            if (result.HasValue)
            {
                if (result.Value == DialogResult.OK)
                {
                    if (Game.Id == 0)
                        _dataAccess.CreateGame(Game.Game);
                    else
                        _dataAccess.UpdateGame(Game.Game);
                }

                _regionManager.Deactivate(RegionNames.Overlay);
                _navigationService.Journal.Clear();
                _requestClose?.Invoke(result.Value);
            }
        }

        private void GetSavefilePath()
        {
            try
            {
                _openFileService.IsFolderPicker = false;
                _openFileService.Multiselect = false;
                _openFileService.ShowHiddenItems = true;

                if (_openFileService.OpenFile())
                    Game.SavefilePath = _openFileService.FileName;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void GetBackupFolderPath()
        {
            try
            {
                _openFileService.IsFolderPicker = true;
                _openFileService.Multiselect = false;
                _openFileService.ShowHiddenItems = true;

                if (_openFileService.OpenFile())
                    Game.BackupFolder = _openFileService.FileName;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //_requestClose = null;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;

            if (navigationContext.Parameters.Count == 0)
                return;

            var id = (int)navigationContext.Parameters["gameId"];

            if (id == 0)
                Game = new GameModel(new Game());
            else
            {
                var game = _dataAccess.GetGame(id) ?? throw new Exception($"Couldn't file game with id { id }");
                Game = new GameModel(game);
            }

            _requestClose = (Action<DialogResult>)navigationContext.Parameters["callback"];
        }

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }
    }
}
