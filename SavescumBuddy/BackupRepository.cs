using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SavescumBuddy
{
    public class BackupRepository
    {
        public ObservableCollection<Backup> _backups;

        public BackupRepository()
        {
            _backups = new ObservableCollection<Backup>();
        }

        public ObservableCollection<Backup> GetBackupList()
        {
            return _backups;
        }

        public int Count()
        {
            return _backups.Count();
        }

        public void Add(Backup backup)
        {
            var backups = SqliteDataAccess.LoadBackupList();

            if (backups.FirstOrDefault(b => b.DateTimeTag.Equals(backup.DateTimeTag)) == null)
            {
                Util.BackupFiles(backup);
                SqliteDataAccess.SaveBackup(backup);
            }
        }

        public void Remove(Backup backup)
        {
            Util.MoveToTrash(backup.FilePath);
            SqliteDataAccess.RemoveBackup(backup);
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && index < _backups.Count)
            {
                Remove(_backups[index]);
            }
        }

        public void Restore(int index)
        {
            if (index > -1 && index < _backups.Count)
            {
                Util.Restore(_backups[index]);
            }
        }

        public void RestoreLatest()
        {
            Util.Restore(SqliteDataAccess.GetLatestBackup());
        }

        public void LoadSortedByNoteList(string input, string offset)
        {
            if (!String.IsNullOrWhiteSpace(input))
            {
                _backups = new ObservableCollection<Backup>(SqliteDataAccess.LoadBackupsWithNoteLike(input, offset));
            }
            else
            {
                LoadBackupsFromPage(offset);
            }
        }

        public void LoadBackupsFromPage(string page)
        {
            try
            {
                _backups = new ObservableCollection<Backup>(SqliteDataAccess.LoadBackups(page));
            }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
            }
        }
    }
}
