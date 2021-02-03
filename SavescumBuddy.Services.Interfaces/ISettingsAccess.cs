namespace SavescumBuddy.Services.Interfaces
{
    public interface ISettingsAccess
    {
        int SchedulerInterval { get; set; }
        int SchedulerOverwriteType { get; set; }
        bool SchedulerEnabled { get; set; }
        int SchedulerSkipType { get; set; }
        int BackupKey { get; set; }
        int BackupModifier { get; set; }
        bool HotkeysEnabled { get; set; }
        int OverwriteKey { get; set; }
        int OverwriteModifier { get; set; }
        int RestoreKey { get; set; }
        int RestoreModifier { get; set; }
        bool SoundCuesEnabled { get; set; }
    }
}
