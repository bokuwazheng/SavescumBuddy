using Prism.Mvvm;
using SavescumBuddy.Lib;
using System;

namespace SavescumBuddy.Wpf.Models
{
    public class BackupModel : BindableBase
    {
        private bool _isSelected;

        public BackupModel(Backup backup)
        {
            Backup = backup ?? throw new ArgumentNullException(nameof(backup));
        }

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public Backup Backup { get; }

        public int Id { get => Backup.Id; set { Backup.Id = value; RaisePropertyChanged(); } }
        public string GameTitle => Backup.GameTitle;
        public string GoogleDriveId { get => Backup.GoogleDriveId; set { Backup.GoogleDriveId = value; RaisePropertyChanged(); } }
        public string Note { get => Backup.Note; set { Backup.Note = value; RaisePropertyChanged(); } }
        public int IsLiked { get => Backup.IsLiked; set { Backup.IsLiked = value; RaisePropertyChanged(); } }
        public int IsAutobackup => Backup.IsAutobackup;
        public long TimeStamp => Backup.TimeStamp;
        public string PicturePath => Backup.PicturePath;
    }
}
