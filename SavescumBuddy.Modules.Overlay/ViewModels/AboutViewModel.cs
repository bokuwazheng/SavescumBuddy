using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
using SavescumBuddy.Core.Events;
using System.Linq;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;

        public AboutViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;

            StartProcessCommand = new DelegateCommand<string>(s => _eventAggregator.GetEvent<StartProcessRequestedEvent>().Publish(s));
            CloseDialogCommand = new DelegateCommand(CloseDialog);
        }

        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private void CloseDialog()
        {
            var activeRegion = _regionManager.Regions[RegionNames.Overlay].ActiveViews.FirstOrDefault();

            if (activeRegion is object)
                _regionManager.Regions[RegionNames.Overlay].Deactivate(activeRegion);
        }

        public DelegateCommand CloseDialogCommand { get; }
        public DelegateCommand<string> StartProcessCommand { get; }
    }
}
