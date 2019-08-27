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
    public class BackupRepository : BindableBase
    {
        public ObservableCollection<Backup> Backups = new ObservableCollection<Backup>();

        #region Singleton implementaion
        private static readonly BackupRepository _instance = new BackupRepository();
        public static BackupRepository Current => _instance;

        static BackupRepository()
        {

        } 
        #endregion

        public void Add(int isAutobackup = 0)
        {
            var backups = SqliteDataAccess.LoadBackupList();
            var game = SqliteDataAccess.GetCurrentGame();
            var folderName = DateTime.Now.ToString(DateTimeFormat.WindowsFriendly);

            var backup = new Backup()
            {
                IsAutobackup = isAutobackup,
                GameId = game.Title,
                Origin = game.SavefilePath,
                DateTimeTag = DateTime.Now.ToString(DateTimeFormat.UserFriendly, CultureInfo.CreateSpecificCulture("en-US")),
                Picture = Path.Combine(game.BackupFolder, folderName, folderName + ".jpg"),
                FilePath = Path.Combine(game.BackupFolder, folderName, Path.GetFileName(game.SavefilePath))
            };

            if (backups.FirstOrDefault(b => b.DateTimeTag.Equals(backup.DateTimeTag)) == null)
            {
                Util.BackupFiles(backup);
                SqliteDataAccess.SaveBackup(backup);
                RaisePropertyChanged("Backups");
            }
        }

        public void Remove(Backup backup)
        {
            Util.MoveToTrash(backup.FilePath);
            SqliteDataAccess.RemoveBackup(backup);
            RaisePropertyChanged("Backups");
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && index < Backups.Count)
            {
                Remove(Backups[index]);
            }
        }

        public void Restore(int index)
        {
            if (index > -1 && index < Backups.Count)
            {
                Util.Restore(Backups[index]);
            }
        }

        public void RestoreLatest()
        {
            Util.Restore(SqliteDataAccess.GetLatestBackup());
        }

        public void SortByNote(string input, string offset)
        {
            if (!String.IsNullOrWhiteSpace(input))
            {
                Backups = new ObservableCollection<Backup>(SqliteDataAccess.LoadBackupsWithNoteLike(input));
            }
            else
            {
                LoadSortedList(offset);
            }
        }

        public void LoadSortedList(string offset)
        {
            try
            {
                Backups = new ObservableCollection<Backup>(SqliteDataAccess.LoadSortedBackupList(offset));
            }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
            }
        }
    }
}
