using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IGoogleDrive
    {
        UserCredential UserCredential { get; set; }

        Task<UserCredential> AuthorizeAsync(string credentials, string token);
        Task<string> CreateAppRootFolderAsync(CancellationToken ct = default);
        Task<string> CreateFolderAsync(string folderName, string parentId, CancellationToken ct = default);
        Task DeleteFromCloudAsync(string id, CancellationToken ct = default);
        void Dispose();
        Task<string> GetAppRootFolderIdAsync(CancellationToken ct = default);
        Task<DriveFile> GetById(string id, bool throwIfFails, CancellationToken ct = default);
        DriveService GetDriveApiService();
        Task<List<DriveFile>> GetFileListAsync(string parentId, string mimeType, CancellationToken ct = default);
        Task<string> GetIdByNameAsync(string name, string parentId, string mimeType, CancellationToken ct = default);
        Task<string> GetUserEmailAsync(CancellationToken ct = default);
        Task ReauthorizeAsync(UserCredential userCredential);
        Task UploadFileAsync(string path, string parentId, CancellationToken ct = default);
    }
}
