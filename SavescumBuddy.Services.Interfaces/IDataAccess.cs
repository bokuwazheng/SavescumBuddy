using SavescumBuddy.Lib;
using System;
using System.Collections.Generic;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IDataAccess
    {
        BackupSearchResponse SearchBackups(IBackupSearchRequest request);
        Backup CreateBackup(bool isAutobackup);
        void DeleteBackup(int id);
        Backup GetLatestBackup();
        void UpdateNote(Backup backup);
        void UpdateIsLiked(Backup backup);
        void UpdateGoogleDriveId(Backup backup);
        Game GetGame(int id);
        List<Game> GetGames();
        void CreateGame(Game game);
        void DeleteGame(Game game);
        void SetGameAsCurrent(int id);
        void UpdateGame(Game game);
        bool ScheduledBackupMustBeSkipped();
        void OverwriteScheduledBackup(Action<Backup> action);
    }
}
