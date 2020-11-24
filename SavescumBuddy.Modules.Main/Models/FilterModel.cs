using Prism.Mvvm;
using SavescumBuddy.Data;

namespace SavescumBuddy.Modules.Main.Models
{
    public class FilterModel : BindableBase, IBackupSearchRequest
    {
        public int? _offset;
        public int? _limit;
        public bool _order;
        public string _groupBy;
        public bool? _liked;
        public bool? _autobackups;
        public bool? _isInGoogleDrive;
        public bool? _current;
        public string _note;

        public FilterModel()
        {
            _offset = 0;
            _limit = 10;
        }

        public int? Offset { get => _offset; set => SetProperty(ref _offset, value); }
        public int? Limit { get => _limit; set => SetProperty(ref _limit, value); }
        public bool Order { get => _order ; set => SetProperty(ref _order, value); }
        public string GroupBy { get => _groupBy; set => SetProperty(ref _groupBy, value); }
        public bool? Liked { get => _liked; set => SetProperty(ref _liked, value); }
        public bool? Autobackups { get => _autobackups; set => SetProperty(ref _autobackups, value); }
        public bool? IsInGoogleDrive { get => _autobackups; set => SetProperty(ref _autobackups, value); }
        public bool? Current { get => _current; set => SetProperty(ref _current, value); }
        public string Note { get => _note; set => SetProperty(ref _note, value); }
    }
}
