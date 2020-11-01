using Prism.Mvvm;
using SavescumBuddy.Data;

namespace SavescumBuddy.Modules.Main.Models
{
    public class FilterModel : BindableBase, IBackupSearchRequest
    {
        public int? _offset;
        public int? _limit;
        public string _order;
        public string _groupBy;
        public bool _likedOnly;
        public bool _hideAutobackups;
        public bool _currentOnly;
        public string _note;

        public FilterModel()
        {
            _offset = 0;
            _limit = 10;
        }

        public int? Offset { get => _offset; set => SetProperty(ref _offset, value); }
        public int? Limit { get => _limit; set => SetProperty(ref _limit, value); }
        public string Order { get => _order ; set => SetProperty(ref _order, value); }
        public string GroupBy { get => _groupBy; set => SetProperty(ref _groupBy, value); }
        public bool LikedOnly { get => _likedOnly; set => SetProperty(ref _likedOnly, value); }
        public bool HideAutobackups { get => _hideAutobackups; set => SetProperty(ref _hideAutobackups, value); }
        public bool CurrentOnly { get => _currentOnly; set => SetProperty(ref _currentOnly, value); }
        public string Note { get => _note; set => SetProperty(ref _note, value); }

        public bool OrderByDesc
        {
            get => Order == "desc";
            set
            {
                Order = value switch
                {
                    true => "desc",
                    false => "asc"
                };
            }
        }
    }
}
