using Prism.Regions;
using SavescumBuddy.Core.Constants;
using SavescumBuddy.Core.Enums;
using System;
using System.Linq;

namespace SavescumBuddy.Core.Extensions
{
    public static class RegionManagerExtensions
    {
        public static void ShowError(this IRegionManager regionManager, string message)
        {
            var parameters = new NavigationParameters
            {
                { "title", "Error" },
                { "message", message },
                { "okContent", "OK" }
            };
            regionManager.RequestNavigate(RegionNames.Overlay, ViewNames.NotificationDialog, parameters);
        }

        public static void PromptAction(this IRegionManager regionManager, string title, string message, string okContent, string cancelContent, Action<DialogResult> callback)
        {
            var parameters = new NavigationParameters
            {
                { "title", title },
                { "message", message },
                { "okContent", okContent },
                { "cancelContent", cancelContent },
                { "callback", callback }
            };
            regionManager.RequestNavigate(RegionNames.Overlay, ViewNames.NotificationDialog, parameters);
        }

        public static void Deactivate(this IRegionManager regionManager, string regionName)
        {
            var region = regionManager.Regions[regionName];
            var activeView = region.ActiveViews.FirstOrDefault();

            if (activeView is object)
                region.Deactivate(activeView);
        }
    }
}
