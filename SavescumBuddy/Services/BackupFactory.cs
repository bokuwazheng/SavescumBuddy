using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
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
            var game = _dataAccess.GetCurrentGame()
                ?? throw new NullReferenceException("Failed to create a backup: no game is set as current yet.");
            var timeStamp = DateTime.Now.Ticks;
            var fileName = timeStamp.ToString();

            var backup = new Backup()
            {
                IsAutobackup = isAutobackup,
                GameId = game.Id,
                OriginPath = game.SavefilePath,
                TimeStamp = timeStamp,
                PicturePath = Path.Combine(game.BackupFolder, fileName + ".jpg"),
                SavefilePath = Path.Combine(game.BackupFolder, fileName + Path.GetExtension(game.SavefilePath))
            };

            return backup;
        }
    }
}
