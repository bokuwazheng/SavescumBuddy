using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace SavescumBuddy
{
    public class MimeType
    {
        public const string File = "mimeType = 'application/unknown'";
        public const string Folder = "mimeType = 'application/vnd.google-apps.folder'";
    }

    public class GoogleDrive : IDisposable
    {
        #region Singleton implementation
        private static readonly GoogleDrive _instance = new GoogleDrive();
        public static GoogleDrive Current => _instance;

        static GoogleDrive() { }
        #endregion

        private CancellationTokenSource _cts;
        // If modifying these scopes, delete your previously saved credentials.
        private readonly string[] _scopes;
        private readonly string _applicationName;
        private readonly TimeSpan _timeoutDelay;
        private readonly string _timeoutError;

#if DEBUG
        public const string CredentialsFileName = "sb_credentials.json";
        public const string TokenFolderName = "token.json";
#else
        public const string CredentialsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", CredentialsFileName);
        public const string TokenFolderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", TokenFolderName);
#endif

        public UserCredential UserCredential { get; set; }

        private GoogleDrive()
        {
            _scopes = new string[] { DriveService.Scope.DriveFile, DriveService.Scope.DriveAppdata };
            _applicationName = "Savescum Buddy";
            _timeoutDelay = TimeSpan.FromSeconds(180d);
            _timeoutError = $"Error: Timeout. Authorization canceled after { _timeoutDelay.TotalSeconds } seconds.";
        }

        public DriveService GetDriveApiService()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = UserCredential,
                ApplicationName = _applicationName,
            });
            return service;
        }

        public async Task<UserCredential> AuthorizeAsync(string credentials, string token)
        {
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(_timeoutDelay);

            try
            {
                using (var stream = new FileStream(credentials, FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        _scopes,
                        "user",
                        _cts.Token,
                        new FileDataStore(token, true));

                    return credential;
                }
            }
            catch (OperationCanceledException)
            {
                Util.PopUp(_timeoutError);
            }
            catch (Exception ex)
            {
                Util.PopUp($"Error: { ex.Message }");
            }

            return null;
        }

        public async Task ReauthorizeAsync(UserCredential userCredential)
        {
            if (userCredential is null)
                return;

            _cts = new CancellationTokenSource();
            _cts.CancelAfter(_timeoutDelay);

            try
            {
                await GoogleWebAuthorizationBroker.ReauthorizeAsync(userCredential, _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Util.PopUp(_timeoutError);
            }
            catch (Exception ex)
            {
                Util.PopUp($"Error: { ex.Message }");
            }
        }

        public async Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default) =>
            await GetIdByNameAsync(_applicationName, "root", MimeType.Folder, ct).ConfigureAwait(false);

        public async Task<string> CreateAppRootFolderAsync(CancellationToken ct = default)
        {
            try
            {
                var fileMetadata = new DriveFile()
                {
                    Name = _applicationName,
                    MimeType = "application/vnd.google-apps.folder",
                };
                var request = GetDriveApiService().Files.Create(fileMetadata);
                request.Fields = "id";
                var folder = await request.ExecuteAsync(ct);
                var rootId = folder.Id;
                return rootId;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
                return null;
            }
        }

        public async Task<string> CreateFolderAsync(string folderName, string parentId, CancellationToken ct = default)
        {
            try
            {
                var fileMetadata = new DriveFile()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { parentId }
                };
                var request = GetDriveApiService().Files.Create(fileMetadata);
                request.Fields = "id";
                var folder = await request.ExecuteAsync(ct);
                return folder.Id;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
                return null;
            }
        }

        public async Task UploadFileAsync(string path, string parentId, CancellationToken ct = default)
        {
            try
            {
                var name = Path.GetFileName(path);

                var fileMetadata = new DriveFile()
                {
                    Name = name,
                    Parents = new List<string> { parentId }
                };
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    var request = GetDriveApiService().Files.Create(fileMetadata, stream, GetMimeType(name));
                    request.Fields = "id";
                    await request.UploadAsync(ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
            }
        }

        public async Task DeleteFromCloudAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var service = GetDriveApiService();
                await service.Files.Delete(id).ExecuteAsync(ct);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
            }
        }

        public async Task<string> GetUserEmailAsync(CancellationToken ct = default)
        {
            try
            {
                var service = GetDriveApiService();
                var request = service.About.Get();
                request.Fields = "user(emailAddress)";
                var result = await request.ExecuteAsync(ct);
                return result?.User.EmailAddress;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Util.PopUp(ex.Message);
                return null;
            }
        }

        public async Task<DriveFile> GetById(string id, bool throwIfFails, CancellationToken ct = default)
        {
            try
            {
                var service = GetDriveApiService();
                var request = service.Files.Get(id);
                request.Fields = "*";
                var result = await request.ExecuteAsync();
                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                if (throwIfFails)
                    throw ex;
                else
                    return null;
            }
        }

        public async Task<List<DriveFile>> GetFileListAsync(string parentId, string mimeType, CancellationToken ct = default)
        {
            var listRequest = GetDriveApiService().Files.List();
            listRequest.PageSize = 100;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = mimeType + $" and '{ parentId }' in parents and trashed = false";

            var result = await listRequest.ExecuteAsync(ct);
            var files = result.Files.ToList();
            if (result.NextPageToken is object)
            {
                listRequest.PageToken = result.NextPageToken;
                result = await listRequest.ExecuteAsync(ct);
                files.AddRange(result.Files);
            }

            return files;
        }

        public async Task<string> GetIdByNameAsync(string name, string parentId, string mimeType, CancellationToken ct = default)
        {
            var listRequest = GetDriveApiService().Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = mimeType + $" and '{ parentId }' in parents and trashed = false";

            var list = await listRequest.ExecuteAsync(ct);
            var result = list.Files.FirstOrDefault(x => x.Name == name);
            return result?.Id;
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            var regKey = Registry.ClassesRoot.OpenSubKey(ext);
            return (regKey != null && regKey.GetValue("Content Type") != null)
                ? regKey.GetValue("Content Type").ToString()
                : mimeType;
        }

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