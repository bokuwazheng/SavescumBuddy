using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Wpf.Constants;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Extensions;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class AboutViewModel : BindableBase, INavigationAware
    {
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;

        public AboutViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;

            StartProcessCommand = new DelegateCommand<string>(s => _eventAggregator.GetEvent<StartProcessRequestedEvent>().Publish(s));
            CloseDialogCommand = new DelegateCommand(() => _regionManager.Deactivate(RegionNames.Overlay));
        }

        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            navigationContext.NavigationService.Journal.Clear();
        }

        public DelegateCommand CloseDialogCommand { get; }
        public DelegateCommand<string> StartProcessCommand { get; }
    }
}
