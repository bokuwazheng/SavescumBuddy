using Prism.Mvvm;
using Settings = SavescumBuddy.Modules.Main.Properties.Settings;

namespace SavescumBuddy.Modules.Main.Models
{
    public class SettingsModel : BindableBase
    {
        public bool AutobackupsEnabled
        {
            get => Settings.Default.AutobackupsEnabled;
            set { Settings.Default.AutobackupsEnabled = value; RaisePropertyChanged(nameof(AutobackupsEnabled)); }
        }

        public string AutobackupSkipType
        {
            get => Settings.Default.AutobackupSkipType;
            set { Settings.Default.AutobackupSkipType = value; RaisePropertyChanged(nameof(AutobackupSkipType)); }
        }

        public int AutobackupInterval
        {
            get => Settings.Default.AutobackupInterval;
            set { Settings.Default.AutobackupInterval = value; RaisePropertyChanged(nameof(AutobackupInterval)); }
        }

        public string AutobackupOverwriteType
        {
            get => Settings.Default.AutobackupOverwriteType;
            set { Settings.Default.AutobackupOverwriteType = value; RaisePropertyChanged(nameof(AutobackupOverwriteType)); }
        }

        public bool HotkeysEnabled
        {
            get => Settings.Default.HotkeysEnabled;
            set { Settings.Default.HotkeysEnabled = value; RaisePropertyChanged(nameof(HotkeysEnabled)); }
        }

        public int RestoreKey
        {
            get => Settings.Default.RestoreKey;
            set { Settings.Default.RestoreKey = value; RaisePropertyChanged(nameof(RestoreKey)); }
        }

        public int BackupKey
        {
            get => Settings.Default.BackupKey;
            set { Settings.Default.BackupKey = value; RaisePropertyChanged(nameof(BackupKey)); }
        }

        public int OverwriteKey
        {
            get => Settings.Default.OverwriteKey;
            set { Settings.Default.OverwriteKey = value; RaisePropertyChanged(nameof(OverwriteKey)); }
        }

        public int RestoreModifier
        {
            get => Settings.Default.RestoreModifier;
            set { Settings.Default.RestoreModifier = value; RaisePropertyChanged(nameof(RestoreModifier)); }
        }

        public int BackupModifier
        {
            get => Settings.Default.BackupModifier;
            set { Settings.Default.BackupModifier = value; RaisePropertyChanged(nameof(BackupModifier)); }
        }

        public int OverwriteModifier
        {
            get => Settings.Default.OverwriteModifier;
            set { Settings.Default.OverwriteModifier = value; RaisePropertyChanged(nameof(OverwriteModifier)); }
        }

        public bool SoundCuesEnabled
        {
            get => Settings.Default.SoundCuesEnabled;
            set { Settings.Default.SoundCuesEnabled = value; RaisePropertyChanged(nameof(SoundCuesEnabled)); }
        }

        public string CloudAppRootFolderId
        {
            get => Settings.Default.CloudAppRootFolderId;
            set { Settings.Default.CloudAppRootFolderId = value; RaisePropertyChanged(nameof(CloudAppRootFolderId)); }
        }
    }
}
