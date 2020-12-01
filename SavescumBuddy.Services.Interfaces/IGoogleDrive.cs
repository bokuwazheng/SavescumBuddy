﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using SavescumBuddy.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace SavescumBuddy.Services.Interfaces
{
    // TODO: separate classes? base and backup?
    public interface IGoogleDrive
    {
        UserCredential UserCredential { get; }

        Task<bool> AuthorizeAsync(CancellationToken ct);
        bool CredentialExists();
        Task<string> CreateAppRootFolderAsync(CancellationToken ct = default);
        Task<string> CreateFolderAsync(string folderName, string parentId, CancellationToken ct = default);
        Task DeleteBackupAsync(Backup backup, CancellationToken ct = default);
        Task DeleteFromCloudAsync(string id, CancellationToken ct = default);
        Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default);
        Task<DriveFile> GetAsync(string id, CancellationToken ct = default);
        DriveService GetDriveApiService();
        Task<List<DriveFile>> GetFilesAsync(string parentId, CancellationToken ct = default);
        Task ExportAsync(DriveFile file, string mimeType, Stream stream, Action<long?, IDownloadProgress> onProgressChanged, CancellationToken ct = default);
        Task<string> GetIdByNameAsync(string name, string parentId, string mimeType, CancellationToken ct = default);
        Task<string> GetUserEmailAsync(CancellationToken ct = default);
        Task RecoverAsync(Backup backup, Action callback, CancellationToken ct = default);
        Task ReauthorizeAsync(CancellationToken ct);
        Task<string> UploadBackupAsync(Backup backup, string gameTitle, CancellationToken ct = default);
        Task UploadFileAsync(string path, string parentId, CancellationToken ct = default);
        Task UploadFilesAsync(string[] paths, string parentId, CancellationToken ct = default);

        class MimeType
        {
            public const string File = "application/unknown";
            public const string Folder = "application/vnd.google-apps.folder";
            public const string Binary = "application/octet-stream";
            public const string Image = "image/jpeg";
        }
    }
}
