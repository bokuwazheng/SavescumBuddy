using Prism.Commands;
using System;
using System.Linq;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Models;
using SavescumBuddy.Sqlite;
using System.Collections.Generic;

namespace SavescumBuddy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Backing fields
        private BackupSearchRequest _filter;
        private BackupModel _selectedBackup;
        private int _currentPageIndex;

        // Properties
        public List<BackupModel> Backups { get; private set; }
        public DateTime TimeSinceLastBackup { get; private set; }
        public double Interval => Settings.Default.Interval * 60;
        public bool CurrentGameIsSet => SqliteDataAccess.GetCurrentGame() is object;
        public int CurrentPageIndex { get => _currentPageIndex; private set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public BackupModel SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value); }
        public BackupSearchRequest Filter { get => _filter ?? (_filter = GetFilter()); private set => SetProperty(ref _filter, value); }
        public int TotalNumberOfBackups => SqliteDataAccess.GetTotalNumberOfBackups(Filter);
        public int PageSize => Settings.Default.BackupsPerPage;
        public int From => Backups.Count > 0 ? Filter.Offset.Value + 1 : 0;
        public int To => Filter.Offset.Value + Backups.Count;

        #region Sorting options
        public bool LikedOnly
        {
            get => Settings.Default.LikedOnly;
            set
            {
                Settings.Default.LikedOnly = value;
                Filter.LikedOnly = value;
                RaisePropertyChanged(nameof(LikedOnly));
            }
        }

        public bool HideAutobackups
        {
            get => Settings.Default.HideAutobackups;
            set
            {
                Settings.Default.HideAutobackups = value;
                Filter.HideAutobackups = value;
                RaisePropertyChanged(nameof(HideAutobackups));
            }
        }

        public bool CurrentOnly
        {
            get => Settings.Default.CurrentOnly;
            set
            {
                Settings.Default.CurrentOnly = value;
                Filter.CurrentOnly = value;
                RaisePropertyChanged(nameof(CurrentOnly));
            }
        }

        public bool OrderByDesc
        {
            get => Settings.Default.OrderByDesc;
            set
            {
                Settings.Default.OrderByDesc = value;
                Filter.Order = value ? "desc" : "asc";
                RaisePropertyChanged(nameof(OrderByDesc));
            }
        }
        #endregion

        public MainViewModel()
        {
            AddCommand = new DelegateCommand(Add);
            RemoveCommand = new DelegateCommand<Backup>(b => Remove(b));
            RestoreCommand = new DelegateCommand<Backup>(b => Util.Restore(b));

            NavigateForwardCommand = new DelegateCommand(() => ++CurrentPageIndex, () => To < TotalNumberOfBackups);
            NavigateBackwardCommand = new DelegateCommand(() => --CurrentPageIndex, () => From > 1);
            NavigateToStartCommand = new DelegateCommand(() => CurrentPageIndex = 0, () => From > 1);
            NavigateToEndCommand = new DelegateCommand(() => CurrentPageIndex = TotalNumberOfBackups / PageSize, () => To < TotalNumberOfBackups);

            Filter.PropertyChanged += (s, e) => OnFilterPropertyChanged(e.PropertyName);
            UpdateBackupList();
        }

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
                var backup = BackupFactory.CreateBackup();
                SqliteDataAccess.SaveBackup(backup);
                Util.BackupFiles(backup);
                UpdateBackupList();
            }
        }

        private void Remove(Backup backup)
        {
            if (backup is null)
                return;
            SqliteDataAccess.RemoveBackup(backup);
            Util.MoveToTrash(backup);
            UpdateBackupList();
        }

        public void UpdateBackupList()
        {
            Backups = SqliteDataAccess.SearchBackups(Filter).Select(x => new BackupModel(x)).ToList();
            RaisePropertyChanged(nameof(Backups));
            RaisePropertyChanged(nameof(TotalNumberOfBackups));
            RaisePropertyChanged(nameof(From));
            RaisePropertyChanged(nameof(To));
            RaiseNavigateCanExecute();
        }

        private BackupSearchRequest GetFilter()
        {
            return new BackupSearchRequest()
            {
                LikedOnly = Settings.Default.LikedOnly,
                HideAutobackups = Settings.Default.HideAutobackups,
                CurrentOnly = Settings.Default.CurrentOnly,
                Order = Settings.Default.OrderByDesc ? "desc" : "asc",
                Offset = CurrentPageIndex * PageSize,
                Limit = PageSize,
                Note = null
            };
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