using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Wpf.Events;

namespace SavescumBuddy.Wpf.Mvvm
{
    public class OverlayBaseViewModel : BaseViewModel
    {
        public OverlayBaseViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
        }

        protected void CloseDialog() => _eventAggregator.GetEvent<DialogIsOpenChangedEvent>().Publish(false);
    }
}
