using SavescumBuddy.Data;
using System.Collections.Generic;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IDataAccess
    {
        int GetTotalNumberOfBackups(BackupSearchRequest request);
        List<Backup> SearchBackups(BackupSearchRequest request);
        void SaveBackup(Backup backup);
        void RemoveBackup(Backup backup);
        Backup GetBackup(Backup backup);
        Backup GetLatestBackup();
        Backup GetLatestAutobackup();
        void UpdateNote(Backup backup);
        void UpdateIsLiked(Backup backup);
        void UpdateDriveId(Backup backup);
        void UpdateFilePaths(Backup backup);
        List<Game> LoadGames();
        void SaveGame(Game game);
        void RemoveGame(Game game);
        Game GetCurrentGame();
        void SetGameAsCurrent(Game game);
        void UpdateGame(Game game);
    }
}
