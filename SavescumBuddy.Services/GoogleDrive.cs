using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using File = System.IO.File;

namespace SavescumBuddy.Services
{
    public class GoogleDrive : IGoogleDrive
    {
        // If modifying these scopes, delete your previously saved credentials.
        private readonly string[] _scopes;
        private readonly string _applicationName;
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

        // The file token.json stores the user's access and refresh tokens, and is created
        // automatically when the authorization flow completes for the first time.
        public async Task<bool> AuthorizeAsync(CancellationToken ct)
        {
            var credentials = CredentialsFileName;
            var token = TokenFolderName;

            using var stream = new FileStream(credentials, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                _scopes,
                "user",
                ct,
                new FileDataStore(token, true)).ConfigureAwait(false);

            if (credential is object)
            {
                UserCredential = credential;
                return true;
            }
            else
                return false;
        }

        public bool CredentialExists()
        {
            var token = TokenFolderName;

            var folderExists = Directory.Exists(token);
            if (folderExists)
                return Directory.GetFiles(token).Any(x => x.Contains("Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
            else
                return false;
        }

        public async Task ReauthorizeAsync(CancellationToken ct)
        {
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(UserCredential, ct).ConfigureAwait(false);
        }

        public async Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default) =>
            await GetIdByNameAsync(_applicationName, "root", IGoogleDrive.MimeType.Folder, ct).ConfigureAwait(false)
            ?? await CreateAppRootFolderAsync().ConfigureAwait(false);

        public async Task<string> CreateAppRootFolderAsync(CancellationToken ct = default)
        {
            var fileMetadata = new DriveFile()
            {
                Name = _applicationName,
                MimeType = IGoogleDrive.MimeType.Folder
            };
            var request = GetDriveApiService().Files.Create(fileMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(ct).ConfigureAwait(false);

            var permission = new Permission() { Type = "anyone", Role = "reader" };
            var service = GetDriveApiService();
            service.Permissions.Create(permission, folder.Id).Execute();

            return folder.Id;
        }

        public async Task<string> CreateFolderAsync(string folderName, string parentId, CancellationToken ct = default)
        {
            var fileMetadata = new DriveFile()
            {
                Name = folderName,
                MimeType = IGoogleDrive.MimeType.Folder,
                Parents = new List<string> { parentId },
            };
            var request = GetDriveApiService().Files.Create(fileMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(ct).ConfigureAwait(false);
            return folder.Id;
        }

        public async Task UploadFileAsync(string path, string parentId, CancellationToken ct = default)
        {
            var fileMetadata = new DriveFile()
            {
                Name = Path.GetFileName(path),
                Parents = new List<string> { parentId }
            };
            using var stream = new FileStream(path, FileMode.Open);
            var request = GetDriveApiService().Files.Create(fileMetadata, stream, IGoogleDrive.MimeType.File);
            await request.UploadAsync(ct).ConfigureAwait(false);
        }

        public async Task UploadFilesAsync(string[] paths, string parentId, CancellationToken ct = default)
        {
            var tasks = new List<Task>();

            foreach (var path in paths)
            {
                tasks.Add(UploadFileAsync(path, parentId, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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

        public async Task<DriveFile> GetAsync(string id, CancellationToken ct = default)
        {
            var service = GetDriveApiService();
            var request = service.Files.Get(id);
            request.Fields = "*";
            var result = await request.ExecuteAsync().ConfigureAwait(false);
            return result;
        }

        public async Task<List<DriveFile>> GetFilesAsync(string parentId, CancellationToken ct = default)
        {
            var request = GetDriveApiService().Files.List();
            request.PageSize = 100;
            request.Fields = "nextPageToken, files(id, name, mimeType)";
            request.Spaces = "drive";
            request.Q = $"'{ parentId }' in parents and trashed = false";
            var result = new List<DriveFile>();
            do
            {
                var files = await request.ExecuteAsync(ct).ConfigureAwait(false);
                result.AddRange(files.Files);
                request.PageToken = files.NextPageToken;
            }
            while (!string.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public async Task<string> GetIdByNameAsync(string name, string parentId, string mimeType, CancellationToken ct = default)
        {
            var request = GetDriveApiService().Files.List();
            request.Fields = "nextPageToken, files(id, name)";
            request.Spaces = "drive";
            request.Q = $"mimeType = '{ mimeType }' and '{ parentId }' in parents and trashed = false";

            var list = await request.ExecuteAsync(ct).ConfigureAwait(false);
            var result = list.Files.FirstOrDefault(x => x.Name == name);
            return result?.Id;
        }

        // TODO: Transaction-like method?
        public async Task<string> UploadBackupAsync(Backup backup, string gameTitle, CancellationToken ct = default)
        {
            if (!File.Exists(backup.SavefilePath))
                throw new ArgumentException("Savefile does not exist.");

            var rootId = await GetAppRootFolderIdAsync(ct).ConfigureAwait(false);
            var gameFolderId = await GetIdByNameAsync(gameTitle, rootId, IGoogleDrive.MimeType.Folder, ct).ConfigureAwait(false);
            if (gameFolderId is null)
                gameFolderId = await CreateFolderAsync(gameTitle, rootId, ct).ConfigureAwait(false);
            var backupCloudFolderId = await CreateFolderAsync(backup.TimeStamp.ToString(), gameFolderId, ct).ConfigureAwait(false);
            await UploadFileAsync(backup.SavefilePath, backupCloudFolderId, ct).ConfigureAwait(false);
            await UploadFileAsync(backup.PicturePath, backupCloudFolderId, ct).ConfigureAwait(false);
            return backupCloudFolderId;
        }

        public async Task DeleteBackupAsync(Backup backup, CancellationToken ct = default)
        {
            await DeleteFromCloudAsync(backup.GoogleDriveId, ct).ConfigureAwait(false);
        }

        // TODO: only works for DOCS
        public async Task ExportAsync(DriveFile file, string mimeType, Stream stream, Action<long?, IDownloadProgress> onProgressChanged, CancellationToken ct = default)
        {
            var request = GetDriveApiService().Files.Export(file.Id, mimeType);
            request.MediaDownloader.ProgressChanged += p => onProgressChanged?.Invoke(file.Size, p);
            await request.DownloadAsync(stream, ct);
        }

        public async Task RecoverAsync(Backup backup, Action callback, CancellationToken ct = default)
        {
            var files = await GetFilesAsync(backup.GoogleDriveId, ct).ConfigureAwait(false);
            if (files.Count == 2)
            {
                var dirName = Path.GetDirectoryName(backup.SavefilePath);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                using var client = new WebClient();
                client.DownloadFileCompleted += (s, e) => callback.Invoke();

                foreach (var file in files)
                {
                    var path = file.MimeType switch
                    {
                        IGoogleDrive.MimeType.Image => backup.PicturePath,
                        IGoogleDrive.MimeType.Binary => backup.SavefilePath,
                        IGoogleDrive.MimeType.File => backup.SavefilePath,
                        _ => throw new Exception("Unsupported MIME type")
                    };

                    await client.DownloadFileTaskAsync($"https://drive.google.com/uc?export=download&id={ file.Id }", path);
                }
            }
        }
    }
}
