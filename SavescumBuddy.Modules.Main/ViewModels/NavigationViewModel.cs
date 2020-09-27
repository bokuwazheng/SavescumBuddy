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

            NavigateToBackupsCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "Backups"));
            NavigateToSettingsCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "Settings"));
            NavigateToGamesCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "Games"));
            NavigateToGoogleDriveCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "GoogleDrive"));
            NavigateToAboutCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "About"));
        }

        public DelegateCommand NavigateToBackupsCommand { get; }
        public DelegateCommand NavigateToSettingsCommand { get; }
        public DelegateCommand NavigateToGamesCommand { get; }
        public DelegateCommand NavigateToGoogleDriveCommand { get; }
        public DelegateCommand NavigateToAboutCommand { get; }
    }
}
