using SavescumBuddy.ViewModels;
using Prism.Mvvm;
using System;
using System.Windows.Threading;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy
{
    public class AutobackupManager : BindableBase
    {
        private DispatcherTimer _backupTimer = new DispatcherTimer();
        private DispatcherTimer _progressBarTimer = new DispatcherTimer();
        private BackupRepository _backupRepository;
        private BackupFactory _backupFactory;
        public int Progress { get; private set; }

        public AutobackupManager(BackupRepository repo, BackupFactory factory)
        {
            _backupRepository = repo;
            _backupFactory = factory;

            _backupTimer.Interval = TimeSpan.FromMinutes(Settings.Default.Interval);
            _backupTimer.Tick += (s, ea) =>
            {
                SmartAutobackup();
                Progress = 0;
            };
            _backupTimer.Start();
            _backupTimer.IsEnabled = Settings.Default.AutobackupsOn; // puts on pause if false

            _progressBarTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _progressBarTimer.Tick += (s, ea) =>
            {
                Progress++;
                RaisePropertyChanged("Progress");
            };
            _progressBarTimer.Start();
            _progressBarTimer.IsEnabled = Settings.Default.AutobackupsOn;
        }

        private void Start()
        {
            _backupTimer.Interval = TimeSpan.FromMinutes(Settings.Default.Interval);
            _backupTimer.Start();
            _progressBarTimer.Start();
        }

        private void Stop()
        {
            _backupTimer.Stop();
            _progressBarTimer.Stop();
            Progress = 0;
        }

        #region SmartAutobackup implementation
        private void SmartAutobackup()
        {
            if (SqliteDataAccess.GetCurrentGame() == null || ItIsTimeToSkip())
            {
                return;
            }

            var backup = SqliteDataAccess.GetLatestAutobackup();

            if (backup != null)
            {
                if (PreviousAutobackupShouldBeDeleted(backup))
                {
                    _backupRepository.Remove(backup);
                }
            }

            _backupRepository.Add(_backupFactory.CreateAutobackup());
        }

        private bool ItIsTimeToSkip()
        {
            var lastBackup = SqliteDataAccess.GetLatestBackup();

            if (lastBackup != null)
            {
                var timeSinceLastBackup = (DateTime.Now - DateTime.Parse(lastBackup.DateTimeTag)).Minutes;

                if (Settings.Default.Skip.Equals(SettingsViewModel.SkipOptionsEnum.FiveMin))
                {
                    return timeSinceLastBackup > 5;
                }
                else if (Settings.Default.Skip.Equals(SettingsViewModel.SkipOptionsEnum.TenMin))
                {
                    return timeSinceLastBackup > 10;
                }
            }

            return false;
        }

        private bool PreviousAutobackupShouldBeDeleted(Backup previous)
        {
            if (Settings.Default.Overwrite.Equals(SettingsViewModel.OverwriteOptionsEnum.Always))
            {
                return true;
            }
            else if (Settings.Default.Overwrite.Equals(SettingsViewModel.OverwriteOptionsEnum.KeepLiked))
            {
                return previous.IsLiked.Equals(1);
            }

            return false;
        }

        internal void OnEnabledChanged(bool value)
        {
            if (value)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        internal void OnIntervalChanged()
        {
            if (Settings.Default.AutobackupsOn)
            {
                Stop(); Start();
            }
        }
        #endregion
    }
}
