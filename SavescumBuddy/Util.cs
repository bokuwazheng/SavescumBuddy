using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.FileIO;
using System.Media;
using System.Windows.Forms;

namespace SavescumBuddy
{
    public static class DateTimeFormat
    {
        public const string WindowsFriendly = "dd.MM.yyyy--HH.mm.ss";
        public const string UserFriendly = "MMM dd, yyyy h:mm:ss tt";
    }
    public class WavLocator
    {
        public const string restore_cue = @"Resources\restore_cue.wav";
        public const string backup_cue = @"Resources\backup_cue.wav";
    }

    class Util
    {
        #region File-related methods 
        public static void BackupSavefile(Backup backup)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(backup.FilePath));
            File.Copy(backup.Origin, backup.FilePath);
        }

        public static void SaveImage(string filePath)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                bitmap.Save((filePath), ImageFormat.Jpeg);
            }
        }

        public static void BackupFiles(Backup backup)
        {
            BackupSavefile(backup);
            SaveImage(backup.Picture);
        }

        public static void Restore(Backup backup)
        {
            File.Copy(backup.FilePath, backup.Origin, true);
        }

        public static void MoveToTrash(Backup backup)
        {
            var dirName = Path.GetDirectoryName(backup.FilePath);

            if (Directory.Exists(dirName))
                FileSystem.DeleteDirectory(dirName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        public static void ShowInExplorer(string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);

            try
            {
                System.Diagnostics.Process.Start(folder);
            }
            catch
            {
                PopUp($"Folder ({folder}) doesn't exist.");
            }
        }
        #endregion

        #region Misc
        public static void PlaySound(string path)
        {
            if (!Properties.Settings.Default.SoundCuesOn)
                return;

            try
            {
                using (var sound = new SoundPlayer(path))
                {
                    sound.Play();
                }
            }
            catch
            {
                return;
            }
        }

        public static bool IsDirectoryWritable(string directory, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(directory, Path.GetRandomFileName()),
                    1, 
                    FileOptions.DeleteOnClose))

                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw new Exception($"Selected directory ({directory}) is read-only. Please select other directory.");
                else
                    return false;
            }
        }

        public static void PopUp(string message)
        {
            MessageBox.Show(message,
                "Savescum Buddy",
                MessageBoxButtons.OK);
        }
        #endregion
    }
}
