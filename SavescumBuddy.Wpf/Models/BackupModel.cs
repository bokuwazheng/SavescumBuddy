using Prism.Mvvm;
using SavescumBuddy.Lib;
using SavescumBuddy.Lib.Extensions;
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

        public int Id => Backup.Id;
        public string GameTitle => Backup.GameTitle;
        public bool IsInGoogleDrive => Backup.IsInGoogleDrive.SqliteToBoolean();
        public string Note { get => Backup.Note; set { Backup.Note = value; RaisePropertyChanged(); } }
        public int IsLiked { get => Backup.IsLiked; set { Backup.IsLiked = value; RaisePropertyChanged(); } }
        public int IsScheduled => Backup.IsScheduled;
        public long TimeStamp => Backup.TimeStamp;
        public string PicturePath => Backup.PicturePath;
    }
}
