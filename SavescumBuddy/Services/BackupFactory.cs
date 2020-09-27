using SavescumBuddy.Core.Extensions;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Globalization;
using System.IO;

namespace SavescumBuddy.Services
{
    public class BackupFactory : IBackupFactory
    {
        private IDataAccess _dataAccess;

        public BackupFactory(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        
        public Backup CreateAutobackup() => CreateBackup(1);

        public Backup CreateBackup() => CreateBackup(0);

        private Backup CreateBackup(int isAutobackup)
        {
            var dateTimeTag = DateTime.Now;
            var folderName = dateTimeTag.ToWindowsFriendlyFormat();
            var game = _dataAccess.GetCurrentGame()
                ?? throw new NullReferenceException("Failed to create a backup: no game is set as current yet.");

            var backup = new Backup()
            {
                IsAutobackup = isAutobackup,
                GameId = game.Id,
                OriginPath = game.SavefilePath,
                TimeStamp = dateTimeTag.ToUserFriendlyFormat(CultureInfo.CreateSpecificCulture("en-US")),
                PicturePath = Path.Combine(game.BackupFolder, folderName, folderName + ".jpg"),
                SavefilePath = Path.Combine(game.BackupFolder, folderName, Path.GetFileName(game.SavefilePath))
            };

            return backup;
        }
    }
}
