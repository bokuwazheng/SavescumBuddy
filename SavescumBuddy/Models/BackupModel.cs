using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Threading.Tasks;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Sqlite;
using System.Threading;

namespace SavescumBuddy.Models
{
    public class BackupModel : BindableBase
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Backup Backup { get; }

        public BackupModel(Backup backup)
        {
            Backup = backup ?? throw new ArgumentNullException(nameof(backup));
            
            UpdateNoteCommand = new DelegateCommand(() => SqliteDataAccess.UpdateNote(Backup));
            UpdateIsLikedCommand = new DelegateCommand(() => SqliteDataAccess.UpdateIsLiked(Backup));
            UpdateInCloudCommand = new DelegateCommand(async () => await UpdateInCloudAsync(Backup));
            ShowInExplorerCommand = new DelegateCommand(() => Util.ShowInExplorer(Backup.FilePath));
        }

        private async Task SaveInCloudAsync(Backup backup)
        {
            var rootId = Settings.Default.CloudAppRootFolderId;
            // Get game folder id.
            var gameFolderId = await GoogleDrive.Current.GetIdByNameAsync(backup.GameId, rootId, MimeType.Folder);
            // Create game folder if it doesn't exist.
            if (gameFolderId is null)
                gameFolderId = await GoogleDrive.Current.CreateFolderAsync(backup.GameId, rootId);
            // Create backup folder inside game folder.
            var backupFolderId = await GoogleDrive.Current.CreateFolderAsync(backup.DateTimeTag, gameFolderId);
            // Upload files inside backup folder.
            await GoogleDrive.Current.UploadFileAsync(backup.FilePath, backupFolderId);
            await GoogleDrive.Current.UploadFileAsync(backup.Picture, backupFolderId);
        }

        private async Task DeleteFromCloudAsync(Backup backup)
        {
            await GoogleDrive.Current.DeleteFromCloudAsync(backup.DateTimeTag, MimeType.Folder);
        }

        private async Task UpdateInCloudAsync(Backup backup)
        {
            if (GoogleDrive.Current.UserCredential is null)
            {
                Util.PopUp("You are not autorized. Please go to Settings to authorize in Google Drive.");
                return;
            }

            var handled = false;

            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (backup.InCloud == 1)
                        await SaveInCloudAsync(backup);
                    if (backup.InCloud == 0)
                        await DeleteFromCloudAsync(backup);

                    handled = true;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Util.PopUp($"Error: { ex.Message }");
            }

            // Update DB if uploading/deleting succeeded.
            if (handled)
                SqliteDataAccess.UpdateInCloud(backup);
        }

        public DelegateCommand UpdateNoteCommand { get; }
        public DelegateCommand UpdateIsLikedCommand { get; }
        public DelegateCommand UpdateInCloudCommand { get; }
        public DelegateCommand ShowInExplorerCommand { get; }
    }
}
