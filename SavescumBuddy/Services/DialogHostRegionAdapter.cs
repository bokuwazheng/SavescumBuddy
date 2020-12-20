using MaterialDesignThemes.Wpf;
using Prism.Regions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;

namespace SavescumBuddy.Services
{
    public class DialogHostRegionAdapter : RegionAdapterBase<DialogHost>
    {
        public DialogHostRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory) { }

        protected override void Adapt(IRegion region, DialogHost regionTarget)
        {
            if (regionTarget == null)
            {
                throw new ArgumentNullException("regionTarget");
            }
            if (regionTarget.Content != null || BindingOperations.GetBinding(regionTarget, DialogHost.DialogContentProperty) != null)
            {
                throw new InvalidOperationException("DialogContent can not be set");
            }

            region.ActiveViews.CollectionChanged += (sender, e) => regionTarget.DialogContent = region.ActiveViews.FirstOrDefault();

            region.Views.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add && !region.ActiveViews.Any())
                {
                    region.Activate(e.NewItems[0]);
                }
            };
        }

        protected override IRegion CreateRegion() => new SingleActiveRegion();
    }
}
