using System.ComponentModel;

namespace SavescumBuddy.Core.Enums
{
    public enum HotkeyAction
    {
        Save,
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
}
