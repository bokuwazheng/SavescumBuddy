using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.FileIO;
using System.Media;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

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

        public static Backup PromptLocateNewFilePaths(Backup backup)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.IsFolderPicker = true;
                dialog.ShowHiddenItems = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var newFolder = dialog.FileName;
                    var folderName = Path.GetFileNameWithoutExtension(backup.Picture);
                    var isRightFolder = newFolder.Contains(folderName);
                    if (!isRightFolder)
                        return null;

                    var backupFile = Path.GetFileName(backup.FilePath);
                    var newFilePath = Path.Combine(newFolder, backupFile);
                    var fileExists = File.Exists(newFilePath);
                    if (fileExists)
                        backup.FilePath = newFilePath;

                    var imageFile = Path.GetFileName(backup.Picture);
                    var newPicture = Path.Combine(newFolder, imageFile);
                    var imageExists = File.Exists(newPicture);
                    if (imageExists)
                        backup.Picture = newPicture;

                    return backup;
                }
            }

            return null;
        }

        public static void MoveToTrash(Backup backup)
        {
            var dirName = Path.GetDirectoryName(backup.FilePath);

            if (Directory.Exists(dirName))
                FileSystem.DeleteDirectory(dirName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        public static void ShowFolderInExplorer(string path)
        {
            try
            {
                var extension = Path.GetExtension(path);
                if (!string.IsNullOrEmpty(extension))
                    path = Path.GetDirectoryName(path);

                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                PopUp("The folder could not be found.");
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

        public static DialogResult PopUp(string message)
        {
            return MessageBox.Show(message,
                "Savescum Buddy",
                MessageBoxButtons.OK);
        }
        #endregion
    }
}