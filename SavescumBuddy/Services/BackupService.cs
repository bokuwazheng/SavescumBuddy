using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SavescumBuddy.Services
{
    public class BackupService : IBackupService
    {
        public void BackupSavefile(Backup backup)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(backup.SavefilePath));
            File.Copy(backup.OriginPath, backup.SavefilePath);
        }

        public void DeleteFiles(Backup backup)
        {
            var dirName = Path.GetDirectoryName(backup.SavefilePath);
            Directory.Delete(dirName);
        }

        public void RestoreBackup(Backup backup)
        {
            File.Copy(backup.SavefilePath, backup.OriginPath, true);
        }

        public void SaveScreenshot(string filePath)
        {
            var bounds = Screen.GetBounds(Point.Empty);
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            bitmap.Save(filePath, ImageFormat.Jpeg);
        }
    }
}
