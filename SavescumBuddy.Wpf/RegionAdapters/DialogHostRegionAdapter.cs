using MaterialDesignThemes.Wpf;
using Prism.Regions;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace SavescumBuddy.Wpf.RegionAdapters
{
    public class DialogHostRegionAdapter : RegionAdapterBase<DialogHost>
    {
        public DialogHostRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, DialogHost regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            bool contentIsSet = regionTarget.DialogContent is object;

            if (contentIsSet)
                throw new InvalidOperationException("Content is already set");

            region.ActiveViews.CollectionChanged += (s, e) =>
            {
                regionTarget.DialogContent = region.ActiveViews.FirstOrDefault();
            };

            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Add && !region.ActiveViews.Any())
                {
                    region.Activate(e.NewItems[0]);
                }
            };
        }

        protected override IRegion CreateRegion() => new SingleActiveRegion();
    }
}
