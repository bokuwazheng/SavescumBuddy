using System;
using System.Globalization;
using System.IO;

namespace SavescumBuddy
{
    public class BackupFactory
    {
        private Backup CreateBackup(int isAutobackup, Game game)
        {
            var dateTimeTag = DateTime.Now;
            var folderName = dateTimeTag.ToString(DateTimeFormat.WindowsFriendly);

            var backup = new Backup()
            {
                IsAutobackup = isAutobackup,
                GameId = game.Title,
                Origin = game.SavefilePath,
                DateTimeTag = dateTimeTag.ToString(DateTimeFormat.UserFriendly, CultureInfo.CreateSpecificCulture("en-US")),
                Picture = Path.Combine(game.BackupFolder, folderName, folderName + ".jpg"),
                FilePath = Path.Combine(game.BackupFolder, folderName, Path.GetFileName(game.SavefilePath))
            };

            return backup;
        }

        public Backup CreateBackup(Game game) => CreateBackup(0, game);
        public Backup CreateAutobackup(Game game) => CreateBackup(1, game);
    }
}