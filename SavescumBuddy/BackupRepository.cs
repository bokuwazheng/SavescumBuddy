using System;
using System.Collections.Generic;
using System.Linq;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy
{
    public class BackupRepository
    {
        private List<Backup> _backups;
        public event Action<Backup> BackupAdded;
        public event Action<Backup> BackupDeleted;

        public BackupRepository() => 
            _backups = new List<Backup>();

        public List<Backup> GetBackupList() => _backups;

        public void Add(Backup backup)
        {
            var dateTimeTags = SqliteDataAccess.LoadBackupDateTimeTagList();

            if (!dateTimeTags.Any(tag => tag == backup.DateTimeTag))
            {
                SqliteDataAccess.SaveBackup(backup);
                BackupAdded?.Invoke(backup);
            }    
        }

        public void Remove(Backup backup)
        {
            SqliteDataAccess.RemoveBackup(backup);
            BackupDeleted?.Invoke(backup);
        }

        internal void UpdateBackupList(BackupSearchRequest value) => 
            _backups = SqliteDataAccess.SearchBackups(value);
    }
}
