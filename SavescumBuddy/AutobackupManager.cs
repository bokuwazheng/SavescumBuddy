using Prism.Mvvm;
using System;
using System.Windows.Threading;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Models;
using SavescumBuddy.MarkupExtensions;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class AutobackupManager : BindableBase
    {
        private int _progress;
        private DispatcherTimer _backupTimer;
        private DispatcherTimer _progressBarTimer;
        public event Action<Backup> DeletionRequested;
        public event Action AdditionRequested;

        public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }

        public AutobackupManager()
        {
            // If false puts the timer on pause.
            var isEnabled = Settings.Default.AutobackupsOn;

            _backupTimer = new DispatcherTimer();
            _backupTimer.Interval = TimeSpan.FromMinutes(Settings.Default.Interval);
            _backupTimer.Tick += (s, ea) => { SmartAutobackup(); Progress = 0; };
            _backupTimer.Start();
            _backupTimer.IsEnabled = isEnabled;

            _progressBarTimer = new DispatcherTimer();
            _progressBarTimer.Interval = TimeSpan.FromSeconds(1);
            _progressBarTimer.Tick += (s, ea) => Progress++;
            _progressBarTimer.Start();
            _progressBarTimer.IsEnabled = isEnabled;
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

        public void OnIsEnabledChanged(bool isEnabled)
        {
            if (isEnabled)
                Start();
            else
                Stop();
        }

        public void OnIntervalChanged(bool isEnabled)
        {
            if (isEnabled)
            {
                Stop();
                Start();
            }
        }

        #region SmartAutobackup implementation
        private void SmartAutobackup()
        {
            if (SqliteDataAccess.GetCurrentGame() is null || ItIsTimeToSkip())
                return;

            var backup = SqliteDataAccess.GetLatestAutobackup();

            if (backup is object)
            {
                if (PreviousAutobackupShouldBeDeleted(backup))
                {
                    DeletionRequested?.Invoke(backup);
                }
            }

            AdditionRequested?.Invoke();
        }

        private bool ItIsTimeToSkip()
        {
            var lastBackup = SqliteDataAccess.GetLatestBackup();

            if (lastBackup is object)
            {
                var timeSinceLastBackup = DateTime.Now - DateTime.Parse(lastBackup.DateTimeTag);

                if (Settings.Default.Skip.Equals(EnumToCollectionExtension.EnumToDescriptionOrString(SkipOption.FiveMin)))
                {
                    return timeSinceLastBackup > TimeSpan.FromMinutes(5d);
                }
                else if (Settings.Default.Skip.Equals(EnumToCollectionExtension.EnumToDescriptionOrString(SkipOption.TenMin)))
                {
                    return timeSinceLastBackup > TimeSpan.FromMinutes(10d);
                }
            }

            return false;
        }

        private bool PreviousAutobackupShouldBeDeleted(Backup previous)
        {
            if (Settings.Default.Overwrite.Equals(EnumToCollectionExtension.EnumToDescriptionOrString(OverwriteOption.Always)))
            {
                return true;
            }
            else if (Settings.Default.Overwrite.Equals(EnumToCollectionExtension.EnumToDescriptionOrString(OverwriteOption.KeepLiked)))
            {
                return !previous.IsLiked.Equals(1);
            }

            return false;
        }
        #endregion
    }
}