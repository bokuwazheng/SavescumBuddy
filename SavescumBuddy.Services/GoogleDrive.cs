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
using SavescumBuddy.Services.Interfaces;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace SavescumBuddy.Services
{
    public class MimeType
    {
        public const string File = "mimeType = 'application/unknown'";
        public const string Folder = "mimeType = 'application/vnd.google-apps.folder'";
    }

    public class GoogleDrive : IGoogleDrive
    {
        // If modifying these scopes, delete your previously saved credentials.
        private readonly string[] _scopes;
        private readonly string _applicationName;
        private readonly TimeSpan _timeoutDelay;
        private readonly string _timeoutError;

        private readonly string _credentialsFileName = "sb_credentials.json";
        private readonly string _tokenFolderName = "token.json";

#if DEBUG
        public string CredentialsFileName => _credentialsFileName;
        public string TokenFolderName => _tokenFolderName;
#else
        public string CredentialsFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", _credentialsFileName);
        public string TokenFolderName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", _tokenFolderName);
#endif

        public UserCredential UserCredential { get; private set; }

        public GoogleDrive()
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

        public async Task<UserCredential> AuthorizeAsync(CancellationToken ct)
        {
            var credentials = CredentialsFileName;
            var token = TokenFolderName;

            using (var stream = new FileStream(credentials, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    ct,
                    new FileDataStore(token, true)).ConfigureAwait(false);

                UserCredential = credential;

                return credential;
            }
        }

        public bool CredentialExists()
        {
            var credentials = CredentialsFileName;
            var token = TokenFolderName;

            var folderExists = Directory.Exists(token);
            if (folderExists)
                return Directory.GetFiles(token).Any(x => x.Contains("Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
            else
                return false;
        }

        public async Task ReauthorizeAsync(UserCredential userCredential, CancellationToken ct)
        {
            if (userCredential is null)
                throw new ArgumentNullException(nameof(userCredential));

            await GoogleWebAuthorizationBroker.ReauthorizeAsync(userCredential, ct).ConfigureAwait(false);
        }

        public async Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default) =>
            await GetIdByNameAsync(_applicationName, "root", MimeType.Folder, ct).ConfigureAwait(false);

        public async Task<string> CreateAppRootFolderAsync(CancellationToken ct = default)
        {
            var fileMetadata = new DriveFile()
            {
                Name = _applicationName,
                MimeType = "application/vnd.google-apps.folder",
            };
            var request = GetDriveApiService().Files.Create(fileMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(ct).ConfigureAwait(false);
            var rootId = folder.Id;
            return rootId;
        }

        public async Task<string> CreateFolderAsync(string folderName, string parentId, CancellationToken ct = default)
        {
            var fileMetadata = new DriveFile()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentId }
            };
            var request = GetDriveApiService().Files.Create(fileMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(ct).ConfigureAwait(false);
            return folder.Id;
        }

        public async Task UploadFileAsync(string path, string parentId, CancellationToken ct = default)
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
                await request.UploadAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task DeleteFromCloudAsync(string id, CancellationToken ct = default)
        {
            var service = GetDriveApiService();
            await service.Files.Delete(id).ExecuteAsync(ct).ConfigureAwait(false);
        }

        public async Task<string> GetUserEmailAsync(CancellationToken ct = default)
        {
            var service = GetDriveApiService();
            var request = service.About.Get();
            request.Fields = "user(emailAddress)";
            var result = await request.ExecuteAsync(ct).ConfigureAwait(false);
            return result?.User.EmailAddress;
        }

        public async Task<DriveFile> GetById(string id, bool throwIfFails, CancellationToken ct = default)
        {
            try
            {
                var service = GetDriveApiService();
                var request = service.Files.Get(id);
                request.Fields = "*";
                var result = await request.ExecuteAsync().ConfigureAwait(false);
                return result;
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

            var result = await listRequest.ExecuteAsync(ct).ConfigureAwait(false);
            var files = result.Files.ToList();
            while (result.NextPageToken is object)
            {
                listRequest.PageToken = result.NextPageToken;
                result = await listRequest.ExecuteAsync(ct).ConfigureAwait(false);
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

            var list = await listRequest.ExecuteAsync(ct).ConfigureAwait(false);
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
    }
}
