using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Sqlite;
using System.Threading;

namespace SavescumBuddy.Models
{
    public class BackupModel : BindableBase
    {
        private CancellationTokenSource _cts;
        private string _uploadingStatus;

        public Backup Backup { get; }
        public bool? IsInDrive => CanExecuteDriveCommands ? Backup.DriveId is object : (bool?)null;
        public bool CanExecuteDriveCommands => GoogleDrive.Current.UserCredential is object;
        public CancellationToken Ct
        {
            get
            {
                if (_cts is null || _cts.IsCancellationRequested)
                    _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }
        public string DriveStatus { get => _uploadingStatus; set => SetProperty(ref _uploadingStatus, value); }

        public BackupModel(Backup backup)
        {
            Backup = backup ?? throw new ArgumentNullException(nameof(backup));

            _cts = new CancellationTokenSource();

            UpdateNoteCommand = new DelegateCommand(() => SqliteDataAccess.UpdateNote(Backup));
            UpdateIsLikedCommand = new DelegateCommand(() => SqliteDataAccess.UpdateIsLiked(Backup));
            ShowInExplorerCommand = new DelegateCommand(() => Util.ShowFolderInExplorer(Backup.FilePath));
            CancelCommand = new DelegateCommand(_cts.Cancel);
            ExecuteDriveActionCommand = new DelegateCommand(async () => await ExecuteCloudActionAsync(Ct));
            LocateCommand = new DelegateCommand(Locate);
        }

        private void Locate()
        {
            var newBackup = Util.PromptLocateNewFilePaths(Backup);
            if (newBackup is null)
            {
                Util.PopUp("Seems like the chosen folder is not the right one.\n\nUse import function instead.");
            }
            else
            {
                SqliteDataAccess.UpdateFilePaths(newBackup);
                Backup.FilePath = newBackup.FilePath;
                Backup.Picture = newBackup.Picture;
            }
        }

        private async Task ExecuteCloudActionAsync(CancellationToken ct)
        {
            if (!CanExecuteDriveCommands)
            {
                RaisePropertyChanged(nameof(IsInDrive));
                return;
            }

            if (Backup.DriveId is null)
                await SaveInDriveAsync(Backup, ct);
            else
                await DeleteFromDriveAsync(Backup, ct); 
        }

        private async Task SaveInDriveAsync(Backup backup, CancellationToken ct = default)
        {
            var backupCloudFolderId = "";
            try
            {
                DriveStatus = "Retrieving app root folder id...";
                var rootId = Settings.Default.CloudAppRootFolderId;
                if (rootId is null)
                {
                    rootId = await GoogleDrive.Current.GetAppRootFolderIdAsync(ct);
                }
                if (rootId is null)
                {
                    DriveStatus = "Creating app root folder...";
                    rootId = await GoogleDrive.Current.CreateAppRootFolderAsync(ct);
                }
                if (rootId != null)
                {
                    Settings.Default.CloudAppRootFolderId = rootId;
                    Settings.Default.Save();
                }
                DriveStatus = "Retrieving game folder id...";
                var gameFolderId = await GoogleDrive.Current.GetIdByNameAsync(backup.GameId, rootId, MimeType.Folder, ct);
                if (gameFolderId is null)
                {
                    DriveStatus = "Creating game folder...";
                    gameFolderId = await GoogleDrive.Current.CreateFolderAsync(backup.GameId, rootId, ct);
                }
                DriveStatus = "Creating backup folder...";
                backupCloudFolderId = await GoogleDrive.Current.CreateFolderAsync(backup.DateTimeTag, gameFolderId, ct);
                DriveStatus = "Uploading backup file...";
                await GoogleDrive.Current.UploadFileAsync(backup.FilePath, backupCloudFolderId, ct);
                DriveStatus = "Uploading image...";
                await GoogleDrive.Current.UploadFileAsync(backup.Picture, backupCloudFolderId, ct);

                if (!ct.IsCancellationRequested)
                {
                    Backup.DriveId = backupCloudFolderId;
                    SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            catch (OperationCanceledException) 
            {
                if (!string.IsNullOrEmpty(backupCloudFolderId))
                {
                    DriveStatus = "Deleting uploaded files...";
                    await GoogleDrive.Current.DeleteFromCloudAsync(backupCloudFolderId);
                }
            }
            finally
            {
                RaisePropertyChanged(nameof(IsInDrive));
                DriveStatus = null;
            }
        }

        private async Task DeleteFromDriveAsync(Backup backup, CancellationToken ct = default)
        {
            try
            {
                DriveStatus = "Deleting backup folder...";
                await Task.Delay(2000, ct);
                await GoogleDrive.Current.DeleteFromCloudAsync(backup.DriveId, ct);

                if (!ct.IsCancellationRequested)
                {
                    Backup.DriveId = null;
                    SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            catch (OperationCanceledException) 
            {
                DriveStatus = "Checking if succeeded...";
                var file = await GoogleDrive.Current.GetById(Backup.DriveId, false);
                if (file is null)
                {
                    Backup.DriveId = null;
                    SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            finally
            {
                RaisePropertyChanged(nameof(IsInDrive));
                DriveStatus = null;
            }
        }

        public DelegateCommand UpdateNoteCommand { get; }
        public DelegateCommand UpdateIsLikedCommand { get; }
        public DelegateCommand ShowInExplorerCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ExecuteDriveActionCommand { get; }
        public DelegateCommand LocateCommand { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}