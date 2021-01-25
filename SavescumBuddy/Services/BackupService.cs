using SavescumBuddy.Lib;
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
            File.Copy(backup.OriginPath, backup.SavefilePath);
        }

        public void DeleteFiles(Backup backup)
        {
            File.Delete(backup.SavefilePath);
            File.Delete(backup.PicturePath);
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
