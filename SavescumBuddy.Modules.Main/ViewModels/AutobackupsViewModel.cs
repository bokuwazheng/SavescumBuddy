using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Core.Extensions;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Windows.Threading;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class AutobackupsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBackupService _backupService;
        private readonly IBackupFactory _backupFactory;

        private int _progress;
        private DispatcherTimer _backupTimer;
        private DispatcherTimer _progressBarTimer;

        public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }
        public int Interval => _settingsAccess.AutobackupInterval * 60;

        public AutobackupsViewModel(IRegionManager regionManager, IDataAccess dataAccess, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, IBackupService backupService, IBackupFactory backupFactory)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _backupService = backupService;
            _backupFactory = backupFactory;

            _backupTimer = new DispatcherTimer();
            _backupTimer.Interval = TimeSpan.FromMinutes(Interval);
            _backupTimer.Tick += (s, e) => { Autobackup(); Progress = 0; };

            _progressBarTimer = new DispatcherTimer();
            _progressBarTimer.Interval = TimeSpan.FromSeconds(1);
            _progressBarTimer.Tick += (s, e) => Progress++;

            // If false puts the timer on pause.
            var isEnabled = _settingsAccess.AutobackupsEnabled;
            if (isEnabled)
            {
                _backupTimer.Start();
                _progressBarTimer.Start();
            }

            _eventAggregator.GetEvent<AutobackupIntervalChangedEvent>().Subscribe(OnIntervalChanged);
            _eventAggregator.GetEvent<AutobackupsEnabledChangedEvent>().Subscribe(OnIsEnabledChanged);
        }

        private void Start()
        {
            _backupTimer.Interval = TimeSpan.FromMinutes(Interval);
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
            RaisePropertyChanged(nameof(Interval));
        }

        private void Autobackup()
        {
            if (_dataAccess.GetCurrentGame() is null || ItIsTimeToSkip())
                return;

            var backup = _dataAccess.GetLatestAutobackup();

            if (backup is object && PreviousAutobackupShouldBeDeleted(backup))
            {
                _dataAccess.RemoveBackup(backup);
                _backupService.DeleteFiles(backup);
            }

            var newBackup = _backupFactory.CreateBackup();
            _dataAccess.SaveBackup(newBackup);
            _backupService.BackupSavefile(newBackup);
            _backupService.SaveScreenshot(newBackup.PicturePath);

            _eventAggregator.GetEvent<BackupListUpdateRequestedEvent>().Publish();
        }

        private bool ItIsTimeToSkip()
        {
            var lastBackup = _dataAccess.GetLatestBackup();

            if (lastBackup is object)
            {
                var timeSinceLastBackup = DateTime.Now - DateTime.Parse(lastBackup.TimeStamp);

                if (_settingsAccess.AutobackupSkipType.EqualsEnumDescription(SkipOption.FiveMin))
                {
                    return timeSinceLastBackup > TimeSpan.FromMinutes(5d);
                }
                else if (_settingsAccess.AutobackupSkipType.EqualsEnumDescription(SkipOption.TenMin))
                {
                    return timeSinceLastBackup > TimeSpan.FromMinutes(10d);
                }
            }

            return false;
        }

        private bool PreviousAutobackupShouldBeDeleted(Backup previous)
        {
            if (_settingsAccess.AutobackupOverwriteType.EqualsEnumDescription(OverwriteOption.Always))
            {
                return true;
            }
            else if (_settingsAccess.AutobackupOverwriteType.EqualsEnumDescription(OverwriteOption.KeepLiked))
            {
                return !previous.IsLiked.Equals(1);
            }

            return false;
        }
    }
}
