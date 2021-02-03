using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Services.Interfaces;
using System;
using SavescumBuddy.Wpf.Models;
using SavescumBuddy.Wpf.Services;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class GameViewModel : OverlayBaseViewModel, INavigationAware
    {
        public Action<DialogResult> _requestClose;
        private GameModel _game;
        private readonly IOpenFileService _openFileService;
        private readonly IDataAccess _dataAccess;
        private IRegionNavigationService _navigationService;

        public GameViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IOpenFileService openFileService, IDataAccess dataAccess) : base(regionManager, eventAggregator)
        {
            _openFileService = openFileService;
            _dataAccess = dataAccess;

            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
            OpenFilePickerCommand = new DelegateCommand(GetSavefilePath);
            OpenFolderPickerCommand = new DelegateCommand(GetBackupFolderPath);
        }

        public GameModel Game { get => _game; set => SetProperty(ref _game, value); }

        private void CloseDialog(DialogResult? result) => Handle(() =>
        {
            if (result.HasValue)
            {
                if (result.Value is DialogResult.OK)
                {
                    if (Game.Id is 0)
                        _dataAccess.CreateGame(Game.Game);
                    else
                        _dataAccess.UpdateGame(Game.Game);
                }

                _navigationService.Journal.Clear();
                _requestClose?.Invoke(result.Value);
                CloseDialog();
            }
        });

        private void GetSavefilePath() => Handle(() =>
        {
            _openFileService.IsFolderPicker = false;
            _openFileService.Multiselect = false;
            _openFileService.ShowHiddenItems = true;

            if (_openFileService.OpenFile())
                Game.SavefilePath = _openFileService.FileName;
        });

        private void GetBackupFolderPath() => Handle(() =>
        {
            _openFileService.IsFolderPicker = true;
            _openFileService.Multiselect = false;
            _openFileService.ShowHiddenItems = true;

            if (_openFileService.OpenFile())
                Game.BackupFolder = _openFileService.FileName;
        });

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;

            if (navigationContext.Parameters.Count == 0)
                return;

            var id = (int)navigationContext.Parameters["gameId"];

            if (id is 0)
                Game = new(new());
            else
            {
                var game = _dataAccess.GetGame(id) ?? throw new($"Couldn't file game with id { id }");
                Game = new(game);
            }

            _requestClose = (Action<DialogResult>)navigationContext.Parameters["callback"];
        }

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }
    }
}
