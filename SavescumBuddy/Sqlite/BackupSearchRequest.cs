using Prism.Mvvm;

namespace SavescumBuddy.Sqlite
{
    public class BackupSearchRequest : BindableBase, ISearchRequest
    {
        private int? _offset;
        private int? _limit;
        private string _order;
        private string _groupBy;
        private bool _likedOnly;
        private bool _hideAutobackups;
        private bool _currentOnly;
        private string _searchQuery;
        
        public int? Offset { get => _offset; set => SetProperty(ref _offset, value); }
        public int? Limit { get => _limit; set => SetProperty(ref _limit, value); }
        public string Order { get => _order; set => SetProperty(ref _order, value); }
        public string GroupBy { get => _groupBy; set => SetProperty(ref _groupBy, value); }
        public bool LikedOnly { get => _likedOnly; set => SetProperty(ref _likedOnly, value); }
        public bool HideAutobackups { get => _hideAutobackups; set => SetProperty(ref _hideAutobackups, value); }
        public bool CurrentOnly { get => _currentOnly; set => SetProperty(ref _currentOnly, value); }
        public string Note { get => _searchQuery; set => SetProperty(ref _searchQuery, value); }
    }
}
