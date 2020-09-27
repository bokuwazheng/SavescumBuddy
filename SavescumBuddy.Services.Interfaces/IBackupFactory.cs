using SavescumBuddy.Data;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IBackupFactory
    {
        Backup CreateBackup();
        Backup CreateAutobackup();
    }
}
