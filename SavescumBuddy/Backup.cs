using System.IO;
using Prism.Mvvm;
using Prism.Commands;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace SavescumBuddy
{
    public class Backup : BindableBase, IDbEntity
    {
        private static readonly object _locker = new object();
        private string _note;
        public string Note
        {
            get { return _note; }
            set { _note = value; RaisePropertyChanged("Note"); }
        }
        public int IsLiked { get; set; }
        public int InCloud { get; set; }
        public int IsAutobackup { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string DateTimeTag { get; set; }
        public string Picture { get; set; }
        public string Origin { get; set; }
        public string FilePath { get; set; }

        public Backup()
        {
            UpdateNoteCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateNote(this);
            });

            UpdateIsLikedCommand = new DelegateCommand(() =>
            {
                SqliteDataAccess.UpdateIsLiked(this);
            });

            UpdateInCloudCommand = new DelegateCommand(async() =>
            {

                if (GoogleDrive.Current.credential == null)
                {
                    Util.PopUp("You are not autorized. Please go to settings to authorize in Google drive.");
                    return;
                }

                var handled = false;

                // Upload to cloud 
                if (InCloud == 1)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            lock (_locker)
                            {
                                // Get game folder id
                                var gameFolderId = GoogleDrive.Current.GetIdByName(GameId, Properties.Settings.Default.CloudRootId, MimeType.Folder);
                                // Create game folder id if it doesn't exist
                                if (gameFolderId == null)
                                {
                                    gameFolderId = GoogleDrive.Current.CreateFolder(GameId, Properties.Settings.Default.CloudRootId);
                                }
                                // Create backup folder inside game folder
                                var backupFolderId = GoogleDrive.Current.CreateFolder(DateTimeTag, gameFolderId);
                                // Upload files inside backup folder
                                GoogleDrive.Current.UploadFile(FilePath, backupFolderId);
                                GoogleDrive.Current.UploadFile(Picture, backupFolderId);
                            }

                            handled = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp($"Exception message: { ex.Message }");
                    }
                }

                // Delete backup folder from cloud
                if (InCloud == 0)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            lock (_locker)
                            {
                                var service = GoogleDrive.Current.CreateDriveApiService();
                                var folderId = GoogleDrive.Current.GetIdByName(DateTimeTag, MimeType.Folder);
                                service.Files.Delete(folderId).Execute(); 
                            }

                            handled = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp($"Exception message: { ex.Message }");
                    }
                }

                // Update DB if uploading/deleting succeeded
                if (handled == true) SqliteDataAccess.UpdateInCloud(this);
            });

            ShowInExplorerCommand = new DelegateCommand(() =>
            {
                var folder = Path.GetDirectoryName(FilePath);

                try
                {
                    System.Diagnostics.Process.Start(folder);
                }
                catch
                {
                    Util.PopUp($"Folder ({folder}) doesn't exist.");
                }
            });
        }

        public DelegateCommand UpdateNoteCommand { get; }
        public DelegateCommand UpdateIsLikedCommand { get; }
        public DelegateCommand UpdateInCloudCommand { get; }
        public DelegateCommand ShowInExplorerCommand { get; }
    }
}
