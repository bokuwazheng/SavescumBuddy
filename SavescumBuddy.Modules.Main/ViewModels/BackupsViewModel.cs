using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class BackupsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBackupService _backupService;
        private readonly IBackupFactory _backupFactory;
        private readonly IGoogleDrive _googleDrive;

        public BackupsViewModel(IRegionManager regionManager, IDataAccess dataAccess, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, IBackupService backupService, IBackupFactory backupFactory, IGoogleDrive googleDrive)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _backupService = backupService;
            _backupFactory = backupFactory;
            _googleDrive = googleDrive;

            _eventAggregator.GetEvent<BackupListUpdateRequestedEvent>().Subscribe(UpdateBackupList);

            AddCommand = new DelegateCommand(Add);
            RemoveCommand = new DelegateCommand<BackupModel>(Remove);
            RestoreCommand = new DelegateCommand<BackupModel>(x => _backupService.RestoreBackup(x.Backup));

            NavigateForwardCommand = new DelegateCommand(() => ++CurrentPageIndex, () => To < TotalNumberOfBackups);
            NavigateBackwardCommand = new DelegateCommand(() => --CurrentPageIndex, () => From > 1);
            NavigateToStartCommand = new DelegateCommand(() => CurrentPageIndex = 0, () => From > 1);
            NavigateToEndCommand = new DelegateCommand(() => CurrentPageIndex = TotalNumberOfBackups / PageSize, () => To < TotalNumberOfBackups);

            ShowInExplorerCommand = new DelegateCommand<BackupModel>(x => _eventAggregator.GetEvent<ExecuteRequestedEvent>().Publish(Path.GetDirectoryName(x.SavefilePath)));
            UpdateNoteCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateNote(x.Backup));
            UpdateIsLikedCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateIsLiked(x.Backup));
            ExecuteDriveActionCommand = new DelegateCommand<BackupModel>(ExecuteCloudAction, x => _googleDrive.UserCredential is object);

            Filter.PropertyChanged += (s, e) => OnFilterPropertyChanged(e.PropertyName);
            UpdateBackupList();
        }

        // Backing fields
        private FilterModel _filter;
        private BackupModel _selectedBackup;
        private int _currentPageIndex;

        // Properties
        public ObservableCollection<BackupModel> Backups { get; private set; }
        public bool CurrentGameIsSet => _dataAccess.GetCurrentGame() is object;
        public int CurrentPageIndex { get => _currentPageIndex; private set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public BackupModel SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value); }
        public FilterModel Filter { get => _filter ??= new FilterModel(); private set => SetProperty(ref _filter, value); }
        public int TotalNumberOfBackups => _dataAccess.GetTotalNumberOfBackups(Filter);
        public int PageSize => 10; // _settingsAccess.BackupsPerPage
        public int From => Backups.Count > 0 ? Filter.Offset.Value + 1 : 0;
        public int To => Filter.Offset.Value + Backups.Count;

        #region Sorting options
        //public bool LikedOnly
        //{
        //    get => Settings.Default.LikedOnly;
        //    set
        //    {
        //        Settings.Default.LikedOnly = value;
        //        Filter.LikedOnly = value;
        //        RaisePropertyChanged(nameof(LikedOnly));
        //    }
        //}

        //public bool HideAutobackups
        //{
        //    get => Settings.Default.HideAutobackups;
        //    set
        //    {
        //        Settings.Default.HideAutobackups = value;
        //        Filter.HideAutobackups = value;
        //        RaisePropertyChanged(nameof(HideAutobackups));
        //    }
        //}

        //public bool CurrentOnly
        //{
        //    get => Settings.Default.CurrentOnly;
        //    set
        //    {
        //        Settings.Default.CurrentOnly = value;
        //        Filter.CurrentOnly = value;
        //        RaisePropertyChanged(nameof(CurrentOnly));
        //    }
        //}

        //public bool OrderByDesc
        //{
        //    get => Settings.Default.OrderByDesc;
        //    set
        //    {
        //        Settings.Default.OrderByDesc = value;
        //        Filter.Order = value ? "desc" : "asc";
        //        RaisePropertyChanged(nameof(OrderByDesc));
        //    }
        //}
        #endregion

        private void OnFilterPropertyChanged(string propName)
        {
            UpdateBackupList();
            if (!propName.Equals(nameof(Filter.Offset)))
                NavigateToStartCommand?.Execute();
        }

        private void RaiseNavigateCanExecute()
        {
            NavigateForwardCommand.RaiseCanExecuteChanged();
            NavigateBackwardCommand.RaiseCanExecuteChanged();
            NavigateToStartCommand.RaiseCanExecuteChanged();
            NavigateToEndCommand.RaiseCanExecuteChanged();
        }

        private void Add()
        {
            try
            {
                var backup = _backupFactory.CreateBackup();
                _dataAccess.SaveBackup(backup);
                _backupService.BackupSavefile(backup);
                _backupService.SaveScreenshot(backup.PicturePath);
                UpdateBackupList();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void Remove(BackupModel backup)
        {
            try
            {
                if (backup is null)
                    return;
                _dataAccess.RemoveBackup(backup.Backup);
                _backupService.DeleteFiles(backup.Backup);
                UpdateBackupList();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public void UpdateBackupList()
        {
            try
            {
                var backups = _dataAccess.SearchBackups(Filter);
                var backupModels = backups.Select(x => new BackupModel(x)).ToList();
                Backups = new ObservableCollection<BackupModel>(backupModels);
                RaisePropertyChanged(nameof(Backups));
                RaisePropertyChanged(nameof(TotalNumberOfBackups));
                RaisePropertyChanged(nameof(From));
                RaisePropertyChanged(nameof(To));
                RaiseNavigateCanExecute();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void ExecuteCloudAction(BackupModel backup)
        {
            var b = backup.Backup;

            //backup.GoogleDriveId = "HUSDF";
            if (backup.GoogleDriveId is null)
                _eventAggregator.GetEvent<GoogleDriveUploadRequestedEvent>().Publish(b);
            else
                _eventAggregator.GetEvent<GoogleDriveDeletionRequestedEvent>().Publish(b);

            backup.GoogleDriveId = b.GoogleDriveId;
        }

        public DelegateCommand<BackupModel> RemoveCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<BackupModel> RestoreCommand { get; }
        public DelegateCommand NavigateForwardCommand { get; }
        public DelegateCommand NavigateBackwardCommand { get; }
        public DelegateCommand NavigateToEndCommand { get; }
        public DelegateCommand NavigateToStartCommand { get; }
        public DelegateCommand<BackupModel> ShowInExplorerCommand { get; }
        public DelegateCommand<BackupModel> UpdateNoteCommand { get; }
        public DelegateCommand<BackupModel> UpdateIsLikedCommand { get; }
        public DelegateCommand<BackupModel> ExecuteDriveActionCommand { get; }
    }
}
