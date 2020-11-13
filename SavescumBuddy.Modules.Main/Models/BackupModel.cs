using Prism.Mvvm;
using SavescumBuddy.Data;
using System;

namespace SavescumBuddy.Modules.Main.Models
{
    public class BackupModel : BindableBase
    {
        public BackupModel(Backup backup)
        {
            Backup = backup ?? throw new ArgumentNullException(nameof(backup));
        }

        public Backup Backup { get; }

        public int Id { get => Backup.Id; set { Backup.Id = value; RaisePropertyChanged(); } }
        public int GameId { get => Backup.GameId; set { Backup.GameId = value; RaisePropertyChanged(); } }
        public string GoogleDriveId { get => Backup.GoogleDriveId; set { Backup.GoogleDriveId = value; RaisePropertyChanged(); } }
        public string Note { get => Backup.Note; set { Backup.Note = value; RaisePropertyChanged(); } }
        public int IsLiked { get => Backup.IsLiked; set { Backup.IsLiked = value; RaisePropertyChanged(); } }
        public int IsAutobackup { get => Backup.IsAutobackup; set { Backup.IsAutobackup = value; RaisePropertyChanged(); } }
        public long TimeStamp { get => Backup.TimeStamp; set { Backup.TimeStamp = value; RaisePropertyChanged(); } }
        public string OriginPath { get => Backup.OriginPath; set { Backup.OriginPath = value; RaisePropertyChanged(); } }
        public string SavefilePath { get => Backup.SavefilePath; set { Backup.SavefilePath = value; RaisePropertyChanged(); } }
        public string PicturePath { get => Backup.PicturePath; set { Backup.PicturePath = value; RaisePropertyChanged(); } }
    }
}
