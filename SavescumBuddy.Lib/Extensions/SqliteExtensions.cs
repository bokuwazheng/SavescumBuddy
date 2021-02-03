using System;

namespace SavescumBuddy.Lib.Extensions
{
    public static class SqliteExtensions
    {
        public static int ToSqliteBoolean(this bool value) => value ? 1 : 0;
        public static string ToSqliteNullCheck(this bool value) => value ? "IS NOT NULL" : "IS NULL";
        public static bool SqliteToBoolean(this int value) => value switch
        {
            0 => false,
            1 => true,
            _ => throw new InvalidOperationException("Only 0 and 1 can be converted to bool"),
        };
    }
}
