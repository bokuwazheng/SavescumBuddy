using SavescumBuddy.Lib;
using System;
using System.Collections.Generic;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IDataAccess
    {
        BackupSearchResponse SearchBackups(IBackupSearchRequest request);
        Backup CreateBackup(bool isScheduled);
        void DeleteBackup(int id);
        void DeleteGame(Game game);
        void DeleteGoogleDriveInfo(int backupId);
        Backup GetLatestBackup();
        void UpdateNote(Backup backup);
        void UpdateIsLiked(Backup backup);
        Game GetGame(int id);
        List<Game> GetGames();
        (string SavefileId, string PictureId) GetGoogleDriveInfo(int backupId);
        void CreateGame(Game game);
        void SaveGoogleDriveInfo(int backupId, string savefileId, string pictureId);
        void SetGameAsCurrent(int id);
        void UpdateGame(Game game);
        bool ScheduledBackupMustBeSkipped();
        void OverwriteScheduledBackup(Action<Backup> action);
    }
}
