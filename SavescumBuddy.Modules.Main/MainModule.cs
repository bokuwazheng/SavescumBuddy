using SavescumBuddy.Modules.Main.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SavescumBuddy.Core;

namespace SavescumBuddy.Modules.Main
{
    public class MainModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public MainModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "Backups");
            _regionManager.RequestNavigate(RegionNames.Navigation, "Navigation");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Backups>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<Games>();
            containerRegistry.RegisterForNavigation<Navigation>();
        }
    }
}