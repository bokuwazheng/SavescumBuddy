namespace SavescumBuddy.Services.Interfaces
{
    public interface ISettingsAccess
    {
        int AutobackupInterval { get; set; }
        string AutobackupOverwriteType { get; set; }
        bool AutobackupsEnabled { get; set; }
        string AutobackupSkipType { get; set; }
        int BackupKey { get; set; }
        int BackupModifier { get; set; }
        string CloudAppRootFolderId { get; set; }
        bool HotkeysEnabled { get; set; }
        int OverwriteKey { get; set; }
        int OverwriteModifier { get; set; }
        int RestoreKey { get; set; }
        int RestoreModifier { get; set; }
        bool SoundCuesEnabled { get; set; }
    }
}
