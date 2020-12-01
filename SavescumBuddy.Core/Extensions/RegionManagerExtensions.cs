using Prism.Regions;
using SavescumBuddy.Core.Enums;
using System;

namespace SavescumBuddy.Core.Extensions
{
    public static class RegionManagerExtensions
    {
        public static void ShowError(this IRegionManager regionManager, string message)
        {
            var parameters = new NavigationParameters
            {
                { "title", "Error" },
                { "message", message }
            };
            regionManager.RequestNavigate(RegionNames.Overlay, "NotificationDialog", parameters);
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
            regionManager.RequestNavigate(RegionNames.Overlay, "NotificationDialog", parameters);
        }
    }
}
