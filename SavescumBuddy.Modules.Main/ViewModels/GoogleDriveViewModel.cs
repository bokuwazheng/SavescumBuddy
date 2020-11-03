using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Services;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private IGoogleDrive _googleDrive;
        private ISettingsAccess _settingsAccess;
        private IEventAggregator _eventAggregator;
        private IDataAccess _dataAccess;

        private CancellationTokenSource _cts;
        private string _status;
        private string _authorizedAs;

        public GoogleDriveViewModel(IGoogleDrive googleDrive, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, IDataAccess dataAccess)
        {
            _googleDrive = googleDrive;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _dataAccess = dataAccess;

            _eventAggregator.GetEvent<GoogleDriveUploadRequestedEvent>().Subscribe(async x => await SaveInDriveAsync(x, Ct));
            _eventAggregator.GetEvent<GoogleDriveDeletionRequestedEvent>().Subscribe(async x => await DeleteFromDriveAsync(x, Ct));

            AuthorizeCommand = new DelegateCommand(async () => await AuthorizeAsync(Ct).ConfigureAwait(false));
            CancelCommand = new DelegateCommand(() => _cts?.Cancel());

            if (_googleDrive.CredentialExists())
                AuthorizeCommand.Execute();
        }

        public CancellationToken Ct
        {
            get
            {
                if (_cts is null || _cts.IsCancellationRequested)
                    _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public string AuthorizedAs { get => _authorizedAs; set => SetProperty(ref _authorizedAs, value); }

        private async Task AuthorizeAsync(CancellationToken ct)
        {
            try
            {
                var userCredential = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
                if (userCredential is null)
                    throw new Exception("Failed to authorize.");

                var rootId = await _googleDrive.GetAppRootFolderIdAsync().ConfigureAwait(false);
                if (rootId is null)
                {
                    rootId = await _googleDrive.CreateAppRootFolderAsync().ConfigureAwait(false);
                    _settingsAccess.CloudAppRootFolderId = rootId;
                }

                AuthorizedAs = await _googleDrive.GetUserEmailAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private async Task SaveInDriveAsync(Backup backup, CancellationToken ct)
        {
            var backupCloudFolderId = "";
            try
            {
                Status = "Retrieving app root folder id...";
                var rootId = _settingsAccess.CloudAppRootFolderId;
                if (rootId is null)
                {
                    rootId = await _googleDrive.GetAppRootFolderIdAsync(ct).ConfigureAwait(false);
                }
                if (rootId is null)
                {
                    Status = "Creating app root folder...";
                    rootId = await _googleDrive.CreateAppRootFolderAsync(ct).ConfigureAwait(false);
                }
                if (rootId != null)
                {
                    _settingsAccess.CloudAppRootFolderId = rootId;
                }
                Status = "Retrieving game folder id...";
                var gameTitle = _dataAccess.GetGame(backup.GameId).Title;
                var gameFolderId = await _googleDrive.GetIdByNameAsync(gameTitle, rootId, MimeType.Folder, ct).ConfigureAwait(false);
                if (gameFolderId is null)
                {
                    Status = "Creating game folder...";
                    gameFolderId = await _googleDrive.CreateFolderAsync(gameTitle, rootId, ct).ConfigureAwait(false);
                }
                Status = "Creating backup folder...";
                backupCloudFolderId = await _googleDrive.CreateFolderAsync(backup.TimeStamp, gameFolderId, ct).ConfigureAwait(false);
                Status = "Uploading backup file...";
                await _googleDrive.UploadFileAsync(backup.SavefilePath, backupCloudFolderId, ct).ConfigureAwait(false);
                Status = "Uploading image...";
                await _googleDrive.UploadFileAsync(backup.PicturePath, backupCloudFolderId, ct).ConfigureAwait(false);

                if (!ct.IsCancellationRequested)
                {
                    //Backup.DriveId = backupCloudFolderId;
                    //SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            catch (OperationCanceledException)
            {
                if (!string.IsNullOrEmpty(backupCloudFolderId))
                {
                    Status = "Deleting uploaded files...";
                    await _googleDrive.DeleteFromCloudAsync(backupCloudFolderId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
            finally
            {
                Status = null;
            }
        }

        private async Task DeleteFromDriveAsync(Backup backup, CancellationToken ct)
        {
            try
            {
                Status = "Deleting backup folder...";
                await Task.Delay(2000, ct).ConfigureAwait(false);
                await _googleDrive.DeleteFromCloudAsync(backup.GoogleDriveId, ct).ConfigureAwait(false);

                if (!ct.IsCancellationRequested)
                {
                    //Backup.DriveId = null;
                    //SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            catch (OperationCanceledException)
            {
                Status = "Checking if cancellation succeeded...";
                var file = await _googleDrive.GetById(backup.GoogleDriveId, false).ConfigureAwait(false);
                if (file is null)
                {
                    //Backup.DriveId = null;
                    //SqliteDataAccess.UpdateDriveId(Backup);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
            finally
            {
                Status = null;
            }
        }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand CancelCommand { get; }
    }
}
