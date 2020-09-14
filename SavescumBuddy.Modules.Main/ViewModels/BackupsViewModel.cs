using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class BackupsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public BackupsViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            GoToSettingsCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "Settings"));
        }

        public DelegateCommand GoToSettingsCommand { get; }
    }
}
