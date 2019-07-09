using SavescumBuddy.ViewModels;
using Prism.Mvvm;
using System;
using System.Windows.Threading;

namespace SavescumBuddy
{
    public class AutobackupManager : BindableBase
    {
        private DispatcherTimer _backupTimer = new DispatcherTimer();
        private DispatcherTimer _progressBarTimer = new DispatcherTimer();
        public int Progress { get; private set; }

        public AutobackupManager()
        {
            SettingsViewModel.Subscribe((object sender, TimerEventArgs e) =>
            {
                if (e.EnabledChanged)
                {
                    if (Properties.Settings.Default.AutobackupsOn)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
                else // meaning that interval has changed
                {
                    if (Properties.Settings.Default.AutobackupsOn)
                    {
                        Stop(); Start();
                    }
                }
            });

            _backupTimer.Interval = TimeSpan.FromMinutes(Properties.Settings.Default.Interval);
            _backupTimer.Tick += (s, ea) =>
            {
                SmartAutobackup();
                Progress = 0;
            };
            _backupTimer.Start();
            _backupTimer.IsEnabled = Properties.Settings.Default.AutobackupsOn; // puts on pause if false

            _progressBarTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _progressBarTimer.Tick += (s, ea) =>
            {
                Progress++;
                RaisePropertyChanged("Progress");
            };
            _progressBarTimer.Start();
            _progressBarTimer.IsEnabled = Properties.Settings.Default.AutobackupsOn;
        }

        private void Start()
        {
            _backupTimer.Interval = TimeSpan.FromMinutes(Properties.Settings.Default.Interval);
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
                    BackupRepository.Current.Remove(backup);
                }
            }

            BackupRepository.Current.Add(1);
        }

        private bool ItIsTimeToSkip()
        {
            var lastBackup = SqliteDataAccess.GetLatestBackup();

            if (lastBackup != null)
            {
                var timeSinceLastBackup = (DateTime.Now - DateTime.Parse(lastBackup.DateTimeTag)).Minutes;

                if (Properties.Settings.Default.Skip.Equals(SettingsViewModel.SkipOptionsEnum.FiveMin))
                {
                    return timeSinceLastBackup > 5;
                }
                else if (Properties.Settings.Default.Skip.Equals(SettingsViewModel.SkipOptionsEnum.TenMin))
                {
                    return timeSinceLastBackup > 10;
                }
            }

            return false;
        }

        private bool PreviousAutobackupShouldBeDeleted(Backup previous)
        {
            if (Properties.Settings.Default.Overwrite.Equals(SettingsViewModel.OverwriteOptionsEnum.Always))
            {
                return true;
            }
            else if (Properties.Settings.Default.Overwrite.Equals(SettingsViewModel.OverwriteOptionsEnum.KeepLiked))
            {
                return previous.IsLiked.Equals(1);
            }

            return false;
        }
        #endregion
    }
}
