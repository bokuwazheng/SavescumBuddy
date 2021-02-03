using System.Threading;
using System.Threading.Tasks;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IGoogleDrive
    {
        bool IsAuthorized { get; }
        Task<bool> AuthorizeAsync(CancellationToken ct);
        bool CredentialExists();
        Task DeleteAsync(string id, CancellationToken ct);
        Task DownloadAsync(string id, string path, CancellationToken ct);
        Task<string> GetUserEmailAsync(CancellationToken ct);
        Task ReauthorizeAsync(CancellationToken ct);
        Task<string> UploadAsync(string path, string mimeType, CancellationToken ct);

        class MimeType
        {
            public const string File = "application/unknown";
            public const string Folder = "application/vnd.google-apps.folder";
            public const string Binary = "application/octet-stream";
            public const string Image = "image/jpeg";
        }
    }
}
