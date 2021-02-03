using Prism.Mvvm;
using SavescumBuddy.Services.Interfaces;
using System;

namespace SavescumBuddy.Modules.Main.Models
{
    public class SettingsModel : BindableBase, ISettingsAccess
    {
        private ISettingsAccess _settingsAccess;

        public SettingsModel(ISettingsAccess settingsAccess)
        {
            _settingsAccess = settingsAccess ?? throw new ArgumentNullException(nameof(settingsAccess));
        }
        
        public bool SchedulerEnabled
        {
            get => _settingsAccess.SchedulerEnabled;
            set { _settingsAccess.SchedulerEnabled = value; RaisePropertyChanged(nameof(SchedulerEnabled)); }
        }

        public int SchedulerSkipType
        {
            get => _settingsAccess.SchedulerSkipType;
            set { _settingsAccess.SchedulerSkipType = value; RaisePropertyChanged(nameof(SchedulerSkipType)); }
        }

        public int SchedulerInterval
        {
            get => _settingsAccess.SchedulerInterval;
            set { _settingsAccess.SchedulerInterval = value; RaisePropertyChanged(nameof(SchedulerInterval)); }
        }

        public int SchedulerOverwriteType
        {
            get => _settingsAccess.SchedulerOverwriteType;
            set { _settingsAccess.SchedulerOverwriteType = value; RaisePropertyChanged(nameof(SchedulerOverwriteType)); }
        }

        public bool HotkeysEnabled
        {
            get => _settingsAccess.HotkeysEnabled;
            set { _settingsAccess.HotkeysEnabled = value; RaisePropertyChanged(nameof(HotkeysEnabled)); }
        }

        public int RestoreKey
        {
            get => _settingsAccess.RestoreKey;
            set { _settingsAccess.RestoreKey = value; RaisePropertyChanged(nameof(RestoreKey)); }
        }

        public int BackupKey
        {
            get => _settingsAccess.BackupKey;
            set { _settingsAccess.BackupKey = value; RaisePropertyChanged(nameof(BackupKey)); }
        }

        public int OverwriteKey
        {
            get => _settingsAccess.OverwriteKey;
            set { _settingsAccess.OverwriteKey = value; RaisePropertyChanged(nameof(OverwriteKey)); }
        }

        public int RestoreModifier
        {
            get => _settingsAccess.RestoreModifier;
            set { _settingsAccess.RestoreModifier = value; RaisePropertyChanged(nameof(RestoreModifier)); }
        }

        public int BackupModifier
        {
            get => _settingsAccess.BackupModifier;
            set { _settingsAccess.BackupModifier = value; RaisePropertyChanged(nameof(BackupModifier)); }
        }

        public int OverwriteModifier
        {
            get => _settingsAccess.OverwriteModifier;
            set { _settingsAccess.OverwriteModifier = value; RaisePropertyChanged(nameof(OverwriteModifier)); }
        }

        public bool SoundCuesEnabled
        {
            get => _settingsAccess.SoundCuesEnabled;
            set { _settingsAccess.SoundCuesEnabled = value; RaisePropertyChanged(nameof(SoundCuesEnabled)); }
        }
    }
}
