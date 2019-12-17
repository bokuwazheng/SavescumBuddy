using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Backing fields
        private string _currentPage = "1";
        private AutobackupManager _autobackupManager;
        private BackupFactory _backupFactory;
        private BackupRepository _backupRepository;

        // Properties
        public ObservableCollection<Backup> Backups
        {
            get { return _backupRepository.GetBackupList(); }
        }
        public double Interval => Settings.Default.Interval * 60;
        public int Progress => _autobackupManager.Progress;
        public bool CurrentGameIsSet => SqliteDataAccess.GetCurrentGame() != null;
        public string SearchQuery { get; set; }
        private int BackupsPerPage => Settings.Default.BackupsPerPage;
        public string CurrentPage
        {
            get { return _currentPage; }
            set { if (value != _currentPage) _currentPage = value; RaisePropertyChanged("CurrentPage"); }
        }

        #region Sorting options
        public bool LikedOnly
        {
            get { return Properties.Settings.Default.LikedOnly; }
            set { Properties.Settings.Default.LikedOnly = value; RaisePropertyChanged(); }
        }

        public bool HideAutobackups
        {
            get { return Properties.Settings.Default.HideAutobackups; }
            set { Properties.Settings.Default.HideAutobackups = value; RaisePropertyChanged(); }
        }

        public bool CurrentOnly
        {
            get { return Properties.Settings.Default.CurrentOnly; }
            set { Properties.Settings.Default.CurrentOnly = value; RaisePropertyChanged(); }
        }

        public bool GroupByGame
        {
            get { return Properties.Settings.Default.GroupByGame; }
            set { Properties.Settings.Default.GroupByGame = value; RaisePropertyChanged(); }
        }

        public bool OrderByDesc
        {
            get { return Properties.Settings.Default.OrderByDesc; }
            set { Properties.Settings.Default.OrderByDesc = value; RaisePropertyChanged(); }
        }
        #endregion

        public MainViewModel(BackupRepository repo, BackupFactory factory, AutobackupManager manager)
        {
            _backupRepository = repo;
            _backupFactory = factory;
            _autobackupManager = manager;
            _backupRepository.LoadBackupsFromPage(((int.Parse(CurrentPage) - 1) * BackupsPerPage).ToString());

            // TODO figure out what happens here!!
            _autobackupManager.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
            };

            AddCommand = new DelegateCommand(() =>
            {
                _backupRepository.Add(_backupFactory.CreateBackup());
                UpdateBackupList(int.Parse(CurrentPage));
            });

            RemoveCommand = new DelegateCommand<int?>(i =>
            {
                if (i.HasValue) _backupRepository.RemoveAt(i.Value);
                UpdateBackupList(int.Parse(CurrentPage));
            });

            RestoreCommand = new DelegateCommand<int?>(i =>
            {
                if (i.HasValue) _backupRepository.Restore(i.Value);
            });

            SortByNoteCommand = new DelegateCommand<string>(s =>
            {
                UpdateBackupList(int.Parse(CurrentPage));
            });

            SortCommand = new DelegateCommand(() =>
            {
                UpdateBackupList(int.Parse(CurrentPage));
            });

            FirstPageCommand = new DelegateCommand(() => 
            {
                CurrentPage = "1";
                UpdateBackupList(1);
            });

            PreviousPageCommand = new DelegateCommand(() => 
            {
                var i = int.Parse(CurrentPage) - 1;
                if (i == 0) i = 99;
                CurrentPage = i.ToString();
                UpdateBackupList(i);
            });

            NextPageCommand = new DelegateCommand(() => 
            {
                var i = int.Parse(CurrentPage) + 1;
                if (i == 100) i = 1;
                CurrentPage = i.ToString();
                UpdateBackupList(i);
            });

            LastPageCommand = new DelegateCommand(() => 
            {
                var offset = 0;
                var backups = 0;
                do
                {
                    _backupRepository.LoadBackupsFromPage(((offset) * BackupsPerPage).ToString());
                    backups = _backupRepository.Count();
                    offset++;
                }
                while (backups == BackupsPerPage);
                CurrentPage = offset.ToString();
                UpdateBackupList(int.Parse(CurrentPage));
            });

            SetCurrentPageCommand = new DelegateCommand<string>((s) =>
            {
                if (string.IsNullOrWhiteSpace(s)) return;
                CurrentPage = s;
                var i = int.Parse(CurrentPage);
                UpdateBackupList(i);
            });
        }

        private void UpdateBackupList(int page)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                _backupRepository.LoadBackupsFromPage(((page - 1) * BackupsPerPage).ToString());
            }
            else
            {
                _backupRepository.LoadSortedByNoteList(SearchQuery, ((page - 1) * BackupsPerPage).ToString());
            }

            RaisePropertyChanged("Backups");
        }

        public DelegateCommand<int?> RestoreCommand { get; }
        public DelegateCommand<int?> RemoveCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<string> SortByNoteCommand { get; }
        public DelegateCommand SortCommand { get; }
        public DelegateCommand FirstPageCommand { get; }
        public DelegateCommand PreviousPageCommand { get; }
        public DelegateCommand NextPageCommand { get; }
        public DelegateCommand LastPageCommand { get; }
        public DelegateCommand<string> SetCurrentPageCommand { get; }
    }
}
