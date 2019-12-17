using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavescumBuddy
{
    public class BackupFactory
    {
        private Backup CreateBackup(int isAutobackup)
        {
            var game = SqliteDataAccess.GetCurrentGame();
            var folderName = DateTime.Now.ToString(DateTimeFormat.WindowsFriendly);

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

        public Backup CreateBackup()
        {
            return CreateBackup(0);
        }

        public Backup CreateAutobackup()
        {
            return CreateBackup(1);
        }
    }
}
