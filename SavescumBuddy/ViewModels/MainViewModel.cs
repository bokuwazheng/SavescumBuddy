using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace SavescumBuddy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Backing fields
        private string _currentPage = "1";
        private AutobackupManager _mngr = new AutobackupManager();

        // Properties
        public ObservableCollection<Backup> Backups => BackupRepository.Current.Backups;
        public double Interval => Properties.Settings.Default.Interval * 60;
        public int Progress => _mngr.Progress;
        public bool CurrentGameIsSet => SqliteDataAccess.GetCurrentGame() != null;
        public string SearchQuery { get; set; }
        private int PageLimit => Properties.Settings.Default.PageLimit;
        public string CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (value != _currentPage)
                {
                    _currentPage = value;
                }
            }
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

        public MainViewModel()
        {
            BackupRepository.Current.LoadSortedList(((int.Parse(CurrentPage) - 1) * PageLimit).ToString());

            _mngr.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
            };

            BackupRepository.Current.PropertyChanged += (s, e) =>
            {
                BackupRepository.Current.LoadSortedList(((int.Parse(CurrentPage) - 1) * PageLimit).ToString());
                RaisePropertyChanged(e.PropertyName);
            };

            AddCommand = new DelegateCommand(() =>
            {
                BackupRepository.Current.Add();
            });

            RemoveCommand = new DelegateCommand<int?>(i =>
            {
                if (i.HasValue) BackupRepository.Current.RemoveAt(i.Value);
            });

            RestoreCommand = new DelegateCommand<int?>(i =>
            {
                if (i.HasValue) BackupRepository.Current.Restore(i.Value);
            });

            SortByNoteCommand = new DelegateCommand<string>(s =>
            {
                BackupRepository.Current.Backups.Clear();
                BackupRepository.Current.SortByNote(s, ((int.Parse(CurrentPage) - 1) * PageLimit).ToString());
                RaisePropertyChanged("Backups");
            });

            SortCommand = new DelegateCommand(() =>
            {
                BackupRepository.Current.Backups.Clear();
                BackupRepository.Current.LoadSortedList(((int.Parse(CurrentPage) - 1) * PageLimit).ToString());
                RaisePropertyChanged("Backups");
            });

            FirstPageCommand = new DelegateCommand(() => 
            {
                CurrentPage = "1";
                RaisePropertyChanged("CurrentPage");
                BackupRepository.Current.LoadSortedList("0");
                RaisePropertyChanged("Backups");
            });

            PreviousPageCommand = new DelegateCommand(() => 
            {
                var i = int.Parse(CurrentPage) - 1;
                if (i == 0) i = 99;
                CurrentPage = i.ToString();
                RaisePropertyChanged("CurrentPage");
                BackupRepository.Current.LoadSortedList(((i - 1) * PageLimit).ToString());
                RaisePropertyChanged("Backups");
            });

            NextPageCommand = new DelegateCommand(() => 
            {
                var i = int.Parse(CurrentPage) + 1;
                if (i == 100) i = 1;
                CurrentPage = i.ToString();
                RaisePropertyChanged("CurrentPage");
                BackupRepository.Current.LoadSortedList(((i - 1) * PageLimit).ToString());
                RaisePropertyChanged("Backups");
            });

            LastPageCommand = new DelegateCommand(() => 
            {
                var offset = 0;
                var backups = 0;
                do
                {
                    BackupRepository.Current.LoadSortedList(((offset) * PageLimit).ToString());
                    backups = BackupRepository.Current.Backups.Count();
                    offset++;
                }
                while (backups == PageLimit);
                CurrentPage = offset.ToString();
                RaisePropertyChanged("CurrentPage");
                RaisePropertyChanged("Backups");
            });

            SetCurrentPageCommand = new DelegateCommand<string>((s) =>
            {
                if (string.IsNullOrWhiteSpace(s)) return;
                CurrentPage = s;
                RaisePropertyChanged("CurrentPage");
                var i = int.Parse(CurrentPage);
                BackupRepository.Current.LoadSortedList(((i - 1) * PageLimit).ToString());
                RaisePropertyChanged("Backups");
            });
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
