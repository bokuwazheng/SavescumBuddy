using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy.Models
{
    public enum OverwriteOption
    {
        [Description("Never")]
        Never,
        [Description("Always")]
        Always,
        [Description("Keep liked autobackups")]
        KeepLiked
    }

    public enum SkipOption
    {
        [Description("Never")]
        Never,
        [Description("If any backup was created <5 min ago")]
        FiveMin,
        [Description("If any backup was created <10 min ago")]
        TenMin
    }

    public class SettingsModel : BindableBase
    {
        public event Action<bool> AutobackupsIsEnabledChanged;
        public event Action<bool> SelectedInvervalChanged;

        public void Save() => Settings.Default.Save();

        public bool AutobackupsOn
        {
            get => Settings.Default.AutobackupsOn;
            set
            {
                Settings.Default.AutobackupsOn = value;
                AutobackupsIsEnabledChanged?.Invoke(value);
                RaisePropertyChanged(nameof(AutobackupsOn));
            }
        }

        public string SelectedSkipOption
        {
            get => Settings.Default.Skip;
            set { Settings.Default.Skip = value; RaisePropertyChanged(nameof(SelectedSkipOption)); }
        }

        public string SelectedInterval
        {
            get => Settings.Default.Interval + " min";
            set
            {
                Settings.Default.Interval = int.Parse(Regex.Match(value, @"\d+").Value);
                SelectedInvervalChanged?.Invoke(AutobackupsOn);
                RaisePropertyChanged(nameof(SelectedInterval));
            }
        }

        public string SelectedOverwriteOption
        {
            get => Settings.Default.Overwrite;
            set { Settings.Default.Overwrite = value; RaisePropertyChanged(nameof(SelectedOverwriteOption)); }
        }

        public bool HotkeysOn
        {
            get => Settings.Default.HotkeysOn;
            set { Settings.Default.HotkeysOn = value; RaisePropertyChanged(nameof(HotkeysOn)); }
        }

        public Keys SelectedQLKey
        {
            get => (Keys)Settings.Default.QLKey;
            set { Settings.Default.QLKey = (int)value; RaisePropertyChanged(nameof(SelectedQLKey)); }
        }
        public Keys SelectedQSKey
        {
            get => (Keys)Settings.Default.QSKey;
            set { Settings.Default.QSKey = (int)value; RaisePropertyChanged(nameof(SelectedQSKey)); }
        }
        public Keys SelectedSOKey
        {
            get => (Keys)Settings.Default.SOKey;
            set { Settings.Default.SOKey = (int)value; RaisePropertyChanged(nameof(SelectedSOKey)); }
        }

        public Keys SelectedQLMod
        {
            get => (Keys)Settings.Default.QLMod;
            set { Settings.Default.QLMod = (int)value; RaisePropertyChanged(nameof(SelectedQLMod)); }
        }
        public Keys SelectedQSMod
        {
            get => (Keys)Settings.Default.QSMod;
            set { Settings.Default.QSMod = (int)value; RaisePropertyChanged(nameof(SelectedQSMod)); }
        }
        public Keys SelectedSOMod
        {
            get => (Keys)Settings.Default.SOMod;
            set { Settings.Default.SOMod = (int)value; RaisePropertyChanged(nameof(SelectedSOMod)); }
        }

        public bool SoundCuesOn
        {
            get => Settings.Default.SoundCuesOn;
            set { Settings.Default.SoundCuesOn = value; RaisePropertyChanged(nameof(SoundCuesOn)); }
        }

        public string CloudAppRootFolderId
        {
            get => Settings.Default.CloudAppRootFolderId;
            set { Settings.Default.CloudAppRootFolderId = value; RaisePropertyChanged(nameof(CloudAppRootFolderId)); }
        }
    }
}
