using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Models;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Backing fields
        private BackupRepository _backupRepository;
        private BackupSearchRequest _filter;
        private BackupModel _selectedBackup;
        private int _currentPageIndex;

        // Properties
        public ObservableCollection<BackupModel> Backups => 
            new ObservableCollection<BackupModel>(_backupRepository.GetBackupList().Select(x => new BackupModel(x)));
        public double Interval => Settings.Default.Interval * 60;
        public AutobackupManager Autobackuper { get; }
        public bool CurrentGameIsSet => SqliteDataAccess.GetCurrentGame() is object;
        public int CurrentPageIndex { get => _currentPageIndex; set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public BackupModel SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value); }
        public BackupSearchRequest Filter { get => _filter ?? (_filter = GetFilter()); set => SetProperty(ref _filter, value); }
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

        public MainViewModel(BackupRepository repo, AutobackupManager manager)
        {
            _backupRepository = repo ?? throw new ArgumentNullException(nameof(repo));
            Autobackuper = manager ?? throw new ArgumentNullException(nameof(manager));

            Autobackuper.AdditionRequested += x => Add(x);
            Autobackuper.RemovalRequested += x => Remove(x);

            AddCommand = new DelegateCommand<Backup>(b => Add(b));
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

        private void Add(Backup backup = default)
        {
            var b = backup ?? BackupFactory.CreateBackup();
            _backupRepository.Add(b);
            UpdateBackupList();
        }

        private void Remove(Backup backup)
        {
            if (backup is null) 
                return;
            _backupRepository.Remove(backup);
            UpdateBackupList();
        }

        public void UpdateBackupList()
        {
            _backupRepository.UpdateBackupList(Filter);
            RaisePropertyChanged(nameof(Backups));
            RaisePropertyChanged(nameof(TotalNumberOfBackups));
            RaisePropertyChanged(nameof(From));
            RaisePropertyChanged(nameof(To));
            RaiseNavigateCanExecute();
        }

        public BackupSearchRequest GetFilter()
        {
            var pageSize = Settings.Default.BackupsPerPage;

            return new BackupSearchRequest()
            {
                LikedOnly = Settings.Default.LikedOnly,
                HideAutobackups = Settings.Default.HideAutobackups,
                CurrentOnly = Settings.Default.CurrentOnly,
                Order = Settings.Default.OrderByDesc ? "desc" : "asc",
                Offset = CurrentPageIndex * pageSize,
                Limit = pageSize,
                Note = null
            };
        }

        public DelegateCommand<Backup> RemoveCommand { get; }
        public DelegateCommand<Backup> AddCommand { get; }
        public DelegateCommand<Backup> RestoreCommand { get; }
        public DelegateCommand NavigateForwardCommand { get; }
        public DelegateCommand NavigateBackwardCommand { get; }
        public DelegateCommand NavigateToEndCommand { get; }
        public DelegateCommand NavigateToStartCommand { get; }
    }
}
