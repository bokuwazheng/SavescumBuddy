using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class NavigationViewModel : BindableBase
    {
        private IRegionManager _regionManager;

        public NavigationViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(s => _regionManager.RequestNavigate(RegionNames.ContentRegion, s));
            OpenAboutDialogCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.Overlay, "About"));
        }

        public DelegateCommand<string> NavigateCommand { get; }
        public DelegateCommand OpenAboutDialogCommand { get; }
    }
}
