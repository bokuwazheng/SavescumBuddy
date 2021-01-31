using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Wpf.Constants;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        public NavigationViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            NavigateCommand = new DelegateCommand<string>(s => _regionManager.RequestNavigate(RegionNames.Content, s));
            OpenAboutDialogCommand = new DelegateCommand(() => ShowDialog(ViewNames.About, null));
        }

        public DelegateCommand<string> NavigateCommand { get; }
        public DelegateCommand OpenAboutDialogCommand { get; }
    }
}
