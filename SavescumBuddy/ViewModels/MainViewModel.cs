using Prism.Commands;
using System.Collections.ObjectModel;


namespace SavescumBuddy.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private AutobackupManager _mngr = new AutobackupManager();
        public ObservableCollection<Backup> Backups => BackupRepository.Current.Backups;
        public double Interval => Properties.Settings.Default.Interval * 60;
        public int Progress => _mngr.Progress;
        public bool CurrentGameIsSet => SqliteDataAccess.GetCurrentGame() != null;
        public string SearchQuery { get; set; }

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

        public bool TopTen
        {
            get { return Properties.Settings.Default.LimitTen; }
            set { Properties.Settings.Default.LimitTen = value; RaisePropertyChanged(); }
        }

        public bool OrderByDesc
        {
            get { return Properties.Settings.Default.OrderByDesc; }
            set { Properties.Settings.Default.OrderByDesc = value; RaisePropertyChanged(); }
        }
        #endregion

        public MainViewModel()
        {
            BackupRepository.Current.LoadSortedList();

            _mngr.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
            };

            BackupRepository.Current.PropertyChanged += (s, e) =>
            {
                BackupRepository.Current.LoadSortedList();
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
                BackupRepository.Current.SortByNote(s);
                RaisePropertyChanged("Backups");
            });

            SortCommand = new DelegateCommand(() =>
            {
                BackupRepository.Current.Backups.Clear();
                BackupRepository.Current.LoadSortedList();
                RaisePropertyChanged("Backups");
            });
        }

        public DelegateCommand<int?> RestoreCommand { get; }
        public DelegateCommand<int?> RemoveCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand SortCommand { get; }
        public DelegateCommand<string> SortByNoteCommand { get; }
    }
}
