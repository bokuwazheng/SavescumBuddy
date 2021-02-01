using SavescumBuddy.Modules.Main.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SavescumBuddy.Wpf.Constants;

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
            _regionManager.RequestNavigate(RegionNames.Content, ViewNames.Backups);
            _regionManager.RequestNavigate(RegionNames.Navigation, ViewNames.Navigation);
            _regionManager.RequestNavigate(RegionNames.Scheduler, ViewNames.Scheduler);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Backups>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<Games>();
            containerRegistry.RegisterForNavigation<Navigation>();
            containerRegistry.RegisterForNavigation<Scheduler>();
            containerRegistry.RegisterForNavigation<GoogleDrive>();
        }
    }
}