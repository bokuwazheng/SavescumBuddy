using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class BackupsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly IFileHelper _fileHelper;
        private readonly IEventAggregator _eventAggregator;

        public BackupsViewModel(IRegionManager regionManager, IDataAccess dataAccess, IFileHelper fileHelper, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;

            AddCommand = new DelegateCommand(Add);
            RemoveCommand = new DelegateCommand<Backup>(Remove);
            RestoreCommand = new DelegateCommand<Backup>(_eventAggregator.GetEvent<BackupRestoredEvent>().Publish);

            //NavigateForwardCommand = new DelegateCommand(() => ++CurrentPageIndex, () => To < TotalNumberOfBackups);
            //NavigateBackwardCommand = new DelegateCommand(() => --CurrentPageIndex, () => From > 1);
            //NavigateToStartCommand = new DelegateCommand(() => CurrentPageIndex = 0, () => From > 1);
            //NavigateToEndCommand = new DelegateCommand(() => CurrentPageIndex = TotalNumberOfBackups / PageSize, () => To < TotalNumberOfBackups);

            //Filter.PropertyChanged += (s, e) => OnFilterPropertyChanged(e.PropertyName);
            UpdateBackupList();
        }

        // Backing fields
        private BackupSearchRequest _filter;
        private Backup _selectedBackup;
        private int _currentPageIndex;

        // Properties
        public List<Backup> Backups { get; private set; }
        public DateTime TimeSinceLastBackup { get; private set; }
        //public double Interval => Settings.Default.Interval * 60;
        public bool CurrentGameIsSet => _dataAccess.GetCurrentGame() is object;
        //public int CurrentPageIndex { get => _currentPageIndex; private set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public Backup SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value); }
        public BackupSearchRequest Filter { get => _filter ?? (_filter = new BackupSearchRequest()); private set => SetProperty(ref _filter, value); }
        public int TotalNumberOfBackups => _dataAccess.GetTotalNumberOfBackups(Filter);
        //public int PageSize => Settings.Default.BackupsPerPage;
        public int From => Backups.Count > 0 ? Filter.Offset.Value + 1 : 0;
        public int To => Filter.Offset.Value + Backups.Count;

        //#region Sorting options
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
        //#endregion

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
            var now = DateTime.Now;
            if (now - TimeSinceLastBackup > TimeSpan.FromSeconds(1d))
            {
                TimeSinceLastBackup = now;
                var backup = new Backup();
                _dataAccess.SaveBackup(backup);
                //_fileHelper.BackupSavefile(backup);
                //_fileHelper.SaveScreenshot(backup.Picture);
                _eventAggregator.GetEvent<BackupCreatedEvent>().Publish(backup);
                UpdateBackupList();
            }
        }

        private void Remove(Backup backup)
        {
            if (backup is null)
                return;
            _dataAccess.RemoveBackup(backup);
            //_fileHelper.DeleteFiles(backup);
            _eventAggregator.GetEvent<BackupDeletedEvent>().Publish(backup);
            UpdateBackupList();
        }

        public void UpdateBackupList()
        {
            Backups = _dataAccess.SearchBackups(Filter);
            RaisePropertyChanged(nameof(Backups));
            RaisePropertyChanged(nameof(TotalNumberOfBackups));
            RaisePropertyChanged(nameof(From));
            RaisePropertyChanged(nameof(To));
            RaiseNavigateCanExecute();
        }

        public DelegateCommand<Backup> RemoveCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<Backup> RestoreCommand { get; }
        public DelegateCommand NavigateForwardCommand { get; }
        public DelegateCommand NavigateBackwardCommand { get; }
        public DelegateCommand NavigateToEndCommand { get; }
        public DelegateCommand NavigateToStartCommand { get; }
    }
}
