using System.ComponentModel;

namespace SavescumBuddy.Lib.Enums
{
    public enum HotkeyAction
    {
        None,
        Backup,
        Restore,
        Overwrite
    }

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

    public enum DialogResult
    {
        None,
        OK,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No
    }
}
