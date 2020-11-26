using SavescumBuddy.Modules.Overlay.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace SavescumBuddy.Modules.Overlay
{
    public class OverlayModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public OverlayModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NotificationDialog>();
            containerRegistry.RegisterForNavigation<Game>();
            containerRegistry.RegisterForNavigation<About>();
        }
    }
}