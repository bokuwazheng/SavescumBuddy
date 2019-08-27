using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace SavescumBuddy
{
    public class GoogleDrive
    {
        #region Singleton implementation
        private static readonly GoogleDrive _instance = new GoogleDrive();
        public static GoogleDrive Current => _instance;

        static GoogleDrive() { }
        #endregion

        public UserCredential credential { get; private set; }

        // If modifying these scopes, delete your previously saved credentials
        private string[] Scopes = { DriveService.Scope.DriveFile, DriveService.Scope.DriveAppdata };
        private string ApplicationName = "Savescum Buddy";
        public const string credentials = "sb_credentials.json";
        public const string token = "token.json";

        private CancellationTokenSource cts;

        public enum Mode { Debug, Release }
        public const Mode CurrentMode = Mode.Release;

        public static string GetCredentials(Mode mode)
        {
            switch (mode)
            {
                case (Mode.Debug):
                    return credentials;
                case (Mode.Release):
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", credentials);      
            }

            return credentials;
        }

        public static string GetToken(Mode mode)
        {
            switch (mode)
            {
                case (Mode.Debug):
                    return token;
                case (Mode.Release):
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bokuwazheng", token);
            }

            return token;
        }

        public async Task AuthorizeAsync()
        {
            cts = new CancellationTokenSource();

            await Task.Run(async() => 
            {
                // Reauthorize.
                if (credential != null)
                {
                    try
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(180.0));

                        using (var stream = new FileStream(GetCredentials(CurrentMode), FileMode.Open, FileAccess.Read))
                        {
                            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credential, cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp("Error: timeout. Authorization canceled after 180 seconds. \n \n" +
                            "Please click 'Authorize' again to complete authorization. \n \n" +
                            $"Exception message: { ex.GetBaseException().ToString() }");
                    }
                }

                try
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(180.0));

                    using (var stream = new FileStream(GetCredentials(CurrentMode), FileMode.Open, FileAccess.Read))
                    {
                        // The file token.json stores the user's access and refresh tokens, and is created
                        // automatically when the authorization flow completes for the first time.
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            cts.Token,
                            new FileDataStore(GetToken(CurrentMode), true)).Result;
                    }
                }
                catch (Exception ex)
                {
                    Util.PopUp("Error: timeout. Authorization canceled after 180 seconds. \n \n" +
                        "Please click 'Authorize' again to complete authorization. \n \n" +
                        $"Exception message: { ex.GetBaseException().ToString() }");
                }

                cts = null;

                // Returning to avoid unauthorized use.
                if (credential == null) return;

                // Create app root folder in drive root and save its id.
                if (!NameExists(ApplicationName, "root", MimeType.Folder))
                {
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = ApplicationName,
                        MimeType = "application/vnd.google-apps.folder",
                    };
                    var request = CreateDriveApiService().Files.Create(fileMetadata);
                    request.Fields = "id";
                    var folder = request.Execute();
                    Properties.Settings.Default.CloudRootId = folder.Id;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    var folderId = GetIdByName(ApplicationName, MimeType.Folder);
                    Properties.Settings.Default.CloudRootId = folderId;
                    Properties.Settings.Default.Save();
                }

            });
        }

        public DriveService CreateDriveApiService()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        public string CreateFolder(string folderName, string rootId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { rootId }
            };
            var request = CreateDriveApiService().Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();

            return file.Id;
        }

        public void UploadFile(string path, string rootId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(path),
                Parents = new List<string> { rootId }
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                request = CreateDriveApiService().Files.Create(fileMetadata, stream, GetMimeType(path));
                request.Fields = "id";
                request.Upload();
            }
        }

        public bool DirectoryExists(string path)
        {
            var dir = string.IsNullOrEmpty(Path.GetExtension(path)) ? path : Path.GetDirectoryName(path);

            var subfolders = dir.Split(new[] { "\\" }, StringSplitOptions.None);
            var sub = subfolders[subfolders.Count() - 1];
            var main = subfolders[subfolders.Count() - 2];

            FilesResource.ListRequest listRequest = CreateDriveApiService().Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = "mimeType = 'application/vnd.google-apps.folder'";

            var files = listRequest.Execute().Files.Select(x => x.Name).ToList();
  
            var subExists = files.Contains(sub);
            var parExists = files.Contains(main);

            return subExists & parExists;
        }

        public bool NameExists(string name, string parentId, string mimeType)
        {
            FilesResource.ListRequest listRequest = CreateDriveApiService().Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = mimeType + $" and '{ parentId }' in parents and trashed = false";

            var exists = listRequest.Execute().Files.Any(x => x.Name == name);
 
            return exists;
        }

        public string GetIdByName(string name, string mimeType)
        {
            FilesResource.ListRequest listRequest = CreateDriveApiService().Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = mimeType + " and trashed = false";

            var result = listRequest.Execute().Files.FirstOrDefault(x => x.Name == name);

            return result == null ? null : result.Id;
        }

        public string GetIdByName(string name, string parentId, string mimeType)
        {
            FilesResource.ListRequest listRequest = CreateDriveApiService().Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Spaces = "drive";
            listRequest.Q = mimeType + $" and '{ parentId }' in parents and trashed = false";

            var result = listRequest.Execute().Files.FirstOrDefault(x => x.Name == name);

            return result == null ? null : result.Id;
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            return regKey != null && regKey.GetValue("Content Type") != null ? regKey.GetValue("Content Type").ToString() : mimeType;
        }
    }

    public class MimeType
    {
        public const string File = "mimeType = 'application/unknown'";
        public const string Folder = "mimeType = 'application/vnd.google-apps.folder'";
    }
}
