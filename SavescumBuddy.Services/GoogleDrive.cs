using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using SavescumBuddy.Services.Interfaces;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace SavescumBuddy.Services
{
    public class GoogleDrive : IGoogleDrive
    {
        // If modifying these scopes, delete your previously saved credentials.
        private readonly string[] _scopes;
        private readonly string _applicationName = "Savescum Buddy";
        private readonly string _credentialsFileName = "sb_credentials.json";
        private readonly string _tokenFolderName = "token.json";

#if DEBUG
        protected string CredentialsFileName => _credentialsFileName;
        protected string TokenFolderName => _tokenFolderName;
#else
        public string CredentialsFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", _credentialsFileName);
        public string TokenFolderName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", _tokenFolderName);
#endif

        public GoogleDrive(IServiceProvider serviceProvider)
        {
            HttpClientFactory = (IHttpClientFactory)serviceProvider.GetService(typeof(IHttpClientFactory));
            HttpClient = HttpClientFactory.CreateClient(nameof(GoogleDrive));

            _scopes = new string[] { DriveService.Scope.DriveFile, DriveService.Scope.DriveAppdata };
        }

        public bool IsAuthorized => UserCredential is object;
        protected UserCredential UserCredential { get; private set; }
        protected HttpClient HttpClient { get; }
        protected IHttpClientFactory HttpClientFactory { get; }

        protected DriveService DriveService => 
            new(new()
            {
                HttpClientInitializer = UserCredential,
                ApplicationName = _applicationName,
            });

        // The file token.json stores the user's access and refresh tokens, and is created
        // automatically when the authorization flow completes for the first time.
        public async Task<bool> AuthorizeAsync(CancellationToken ct = default)
        {
            var credentials = CredentialsFileName;
            var token = TokenFolderName;

            using FileStream stream = new(credentials, FileMode.Open, FileAccess.Read);
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

        public async Task ReauthorizeAsync(CancellationToken ct = default)
        {
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(UserCredential, ct).ConfigureAwait(false);
        }

        protected async Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default) =>
            await GetIdByNameAsync(_applicationName, "root", IGoogleDrive.MimeType.Folder, ct).ConfigureAwait(false)
            ?? await CreateAppRootFolderAsync(ct).ConfigureAwait(false);

        protected async Task<string> CreateAppRootFolderAsync(CancellationToken ct = default)
        {
            DriveFile body = new()
            {
                Name = _applicationName,
                MimeType = IGoogleDrive.MimeType.Folder
            };
            var request = DriveService.Files.Create(body);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(ct).ConfigureAwait(false);

            Permission permission = new() { Type = "anyone", Role = "reader" };
            DriveService.Permissions.Create(permission, folder.Id).Execute();

            return folder.Id;
        }

        protected async Task UploadFileAsync(string path, string parentId, string mimeType, CancellationToken ct = default)
        {
            DriveFile body = new()
            {
                Name = Path.GetFileName(path),
                Parents = new List<string> { parentId }
            };
            using FileStream fs = new(path, FileMode.Open);
            var upload = DriveService.Files.Create(body, fs, mimeType);
            await upload.UploadAsync(ct).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            await DriveService.Files.Delete(id).ExecuteAsync(ct).ConfigureAwait(false);
        }

        public async Task<string> GetUserEmailAsync(CancellationToken ct = default)
        {
            var request = DriveService.About.Get();
            request.Fields = "user(emailAddress)";
            var result = await request.ExecuteAsync(ct).ConfigureAwait(false);
            return result?.User.EmailAddress;
        }

        protected async Task<List<DriveFile>> GetFilesAsync(string parentId, CancellationToken ct = default)
        {
            var request = DriveService.Files.List();
            request.PageSize = 100;
            request.Fields = "nextPageToken, files(id, name, mimeType)";
            request.Spaces = "drive";
            request.Q = $"'{ parentId }' in parents and trashed = false";
            List<DriveFile> result = new();
            do
            {
                var files = await request.ExecuteAsync(ct).ConfigureAwait(false);
                result.AddRange(files.Files);
                request.PageToken = files.NextPageToken;
            }
            while (!string.IsNullOrEmpty(request.PageToken));

            return result;
        }

        protected async Task<string> GetIdByNameAsync(string name, string parentId, string mimeType, CancellationToken ct = default)
        {
            var request = DriveService.Files.List();
            request.Fields = "nextPageToken, files(id, name)";
            request.Spaces = "drive";
            request.Q = $"mimeType = '{ mimeType }' and '{ parentId }' in parents and trashed = false";

            var list = await request.ExecuteAsync(ct).ConfigureAwait(false);
            var result = list.Files.FirstOrDefault(x => x.Name == name);
            return result?.Id;
        }

        public async Task<string> UploadAsync(string path, string mimeType, CancellationToken ct = default)
        {
            var appRootId = await GetAppRootFolderIdAsync(ct).ConfigureAwait(false);

            await UploadFileAsync(path, appRootId, mimeType, ct).ConfigureAwait(false);

            var fileName = Path.GetFileName(path);
            var result = await GetIdByNameAsync(fileName, appRootId, mimeType, ct).ConfigureAwait(false);

            return result;
        }

        public async Task DownloadAsync(string id, string path, CancellationToken ct = default)
        {
            FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, FileOptions.SequentialScan | FileOptions.Asynchronous);
            await using (fs.ConfigureAwait(false))
            {
                var response = await HttpClient.GetAsync($"https://drive.google.com/uc?export=download&id={ id }", ct).ConfigureAwait(false);
                await response.Content.CopyToAsync(fs, ct).ConfigureAwait(false);
            }
        }

        public async Task ExportAsync(DriveFile file, string mimeType, Stream stream, Action<long?, IDownloadProgress> onProgressChanged, CancellationToken ct = default)
        {
            var request = DriveService.Files.Export(file.Id, mimeType);
            request.MediaDownloader.ProgressChanged += p => onProgressChanged?.Invoke(file.Size, p);
            await request.DownloadAsync(stream, ct);
        }
    }
}
