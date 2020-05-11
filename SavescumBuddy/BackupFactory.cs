using System;
using System.Globalization;
using System.IO;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class BackupFactory
    {
        private static Backup CreateBackup(int isAutobackup)
        {
            var folderName = DateTime.Now.ToString(DateTimeFormat.WindowsFriendly);
            var game = SqliteDataAccess.GetCurrentGame()
                ?? throw new NullReferenceException("Failed to create a backup: no game is set as current yet.");

            var backup = new Backup()
            {
                IsAutobackup = isAutobackup,
                GameId = game.Title,
                Origin = game.SavefilePath,
                DateTimeTag = DateTime.Now.ToString(DateTimeFormat.UserFriendly, CultureInfo.CreateSpecificCulture("en-US")),
                Picture = Path.Combine(game.BackupFolder, folderName, folderName + ".jpg"),
                FilePath = Path.Combine(game.BackupFolder, folderName, Path.GetFileName(game.SavefilePath))
            };

            return backup;
        }

        public static Backup CreateBackup() => CreateBackup(0);

        public static Backup CreateAutobackup() => CreateBackup(1);
    }
}
