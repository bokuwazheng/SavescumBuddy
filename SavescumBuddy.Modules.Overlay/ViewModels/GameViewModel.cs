using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class GameViewModel : BindableBase, INavigationAware
    {
        private event Action<Game> _requestClose;
        private string _title;
        private string _savefilePath;
        private string _backupFolder;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenFileService _openFileService;
        private IRegionNavigationService _navigationService;

        public GameViewModel(IEventAggregator eventAggregator, IOpenFileService openFileService)
        {
            _eventAggregator = eventAggregator;
            _openFileService = openFileService;

            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
            OpenFilePickerCommand = new DelegateCommand(GetSavefilePath);
            OpenFolderPickerCommand = new DelegateCommand(GetBackupFolderPath);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string SavefilePath { get => _savefilePath; set => SetProperty(ref _savefilePath, value); }
        public string BackupFolder { get => _backupFolder; set => SetProperty(ref _backupFolder, value); }

        private void CloseDialog(DialogResult? result)
        {
            if (result.HasValue)
            {
                Game game = null;
                if (result == DialogResult.OK)
                {
                    game = new Game
                    {
                        Title = Title,
                        SavefilePath = SavefilePath,
                        BackupFolder = BackupFolder
                    };
                }
                _navigationService.Journal.Clear();
                _requestClose?.Invoke(game);
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
                    SavefilePath = _openFileService.FileName;
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
                    BackupFolder = _openFileService.FileName;
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
            var game = (Game)navigationContext.Parameters["game"];
            Title = game.Title;
            SavefilePath = game.SavefilePath;
            BackupFolder = game.BackupFolder;
            _requestClose = (Action<Game>)navigationContext.Parameters["callback"];
        }

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
        public DelegateCommand OpenFilePickerCommand { get; }
        public DelegateCommand OpenFolderPickerCommand { get; }
    }
}
