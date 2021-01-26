using Prism.Mvvm;
using SavescumBuddy.Lib;

namespace SavescumBuddy.Wpf.Models
{
    public class FilterModel : BindableBase, IBackupSearchRequest
    {
        private int? _id;
        private int? _offset;
        private int? _limit;
        private bool _descending;
        private string _groupBy;
        private bool? _isLiked;
        private bool? _isAutobackup;
        private bool? _isInGoogleDrive;
        private string _note;
        private int _gameId;

        public FilterModel()
        {
            _offset = 0;
            _limit = 10;
        }

        public int? Id { get => _id; set => SetProperty(ref _id, value); }
        public int? Offset { get => _offset; set => SetProperty(ref _offset, value); }
        public int? Limit { get => _limit; set => SetProperty(ref _limit, value); }
        public bool Descending { get => _descending ; set => SetProperty(ref _descending, value); }
        public string GroupBy { get => _groupBy; set => SetProperty(ref _groupBy, value); }
        public bool? IsLiked { get => _isLiked; set => SetProperty(ref _isLiked, value); }
        public bool? IsAutobackup { get => _isAutobackup; set => SetProperty(ref _isAutobackup, value); }
        public bool? IsInGoogleDrive { get => _isInGoogleDrive; set => SetProperty(ref _isInGoogleDrive, value); }
        public string Note { get => _note; set => SetProperty(ref _note, value); }
        public int GameId { get => _gameId; set => SetProperty(ref _gameId, value); }
    }
}
