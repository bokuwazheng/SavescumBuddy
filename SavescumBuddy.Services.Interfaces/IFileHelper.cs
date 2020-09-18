using SavescumBuddy.Data;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IFileHelper
    {
        void BackupSavefile(Backup backup);
        void SaveScreenshot(string filePath);
        void RestoreBackup(Backup backup);
        void DeleteFiles(Backup backup);
    }
}
