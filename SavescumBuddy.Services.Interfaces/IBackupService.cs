using SavescumBuddy.Lib;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IBackupService
    {
        void BackupSavefile(Backup backup);
        void SaveScreenshot(string filePath);
        void RestoreBackup(Backup backup);
        void DeleteFiles(Backup backup);
    }
}
