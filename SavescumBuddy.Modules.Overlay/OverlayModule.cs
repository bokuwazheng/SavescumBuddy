using SavescumBuddy.Modules.Overlay.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SavescumBuddy.Core;
using System;

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
            //var callback = new Action(() => { });
            //var pars = new NavigationParameters
            //{
            //    { "title", "123" },
            //    { "message", "ASDF" },
            //    { "callback", callback }
            //};
            //_regionManager.RequestNavigate(RegionNames.Overlay, "NotificationDialog", pars);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NotificationDialog>();
        }
    }
}