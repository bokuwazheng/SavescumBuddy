using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
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

            _eventAggregator.GetEvent<GoogleDriveUploadRequestedEvent>().Subscribe(async x => await SaveInDriveAsync(x, Ct), ThreadOption.UIThread);
            _eventAggregator.GetEvent<GoogleDriveDeletionRequestedEvent>().Subscribe(async x => await DeleteFromDriveAsync(x, Ct), ThreadOption.UIThread);

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
                var succeeded = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
                if (!succeeded)
                    throw new Exception("Failed to authorize.");

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
                var rootId = await _googleDrive.GetAppRootFolderIdAsync(ct).ConfigureAwait(false);
                Status = "Retrieving game folder id...";
                var gameTitle = _dataAccess.GetGame(backup.GameId).Title;
                var gameFolderId = await _googleDrive.GetIdByNameAsync(gameTitle, rootId, IGoogleDrive.MimeType.Folder, ct).ConfigureAwait(false);
                if (gameFolderId is null)
                {
                    Status = "Creating game folder...";
                    gameFolderId = await _googleDrive.CreateFolderAsync(gameTitle, rootId, ct).ConfigureAwait(false);
                }
                Status = "Creating backup folder...";
                backupCloudFolderId = await _googleDrive.CreateFolderAsync(backup.TimeStamp.ToString(), gameFolderId, ct).ConfigureAwait(false);
                Status = "Uploading backup file...";
                await _googleDrive.UploadFileAsync(backup.SavefilePath, backupCloudFolderId, ct).ConfigureAwait(false);
                Status = "Uploading image...";
                await _googleDrive.UploadFileAsync(backup.PicturePath, backupCloudFolderId, ct).ConfigureAwait(false);

                if (!ct.IsCancellationRequested)
                {
                    backup.GoogleDriveId = backupCloudFolderId;
                    _dataAccess.UpdateGoogleDriveId(backup);
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
                    backup.GoogleDriveId = null;
                    _dataAccess.UpdateGoogleDriveId(backup);
                }
            }
            catch (OperationCanceledException)
            {
                Status = "Checking if cancellation succeeded...";
                var file = await _googleDrive.GetFileById(backup.GoogleDriveId, false).ConfigureAwait(false);
                if (file is null)
                {
                    backup.GoogleDriveId = null;
                    _dataAccess.UpdateGoogleDriveId(backup);
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
