using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
using SavescumBuddy.Core.Events;
using System.Linq;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class AboutViewModel : BindableBase, INavigationAware
    {
        private IRegionNavigationService _navigationService;
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

        // TODO: figure out why navigation stack is not empty when trying to close for first time
        private void CloseDialog()
        {
            if (_navigationService.Journal.CanGoBack)
                _navigationService.Journal.GoBack();
            else
            {
                var activeRegion = _regionManager.Regions[RegionNames.Overlay].ActiveViews.FirstOrDefault();

                if (activeRegion is object)
                    _regionManager.Regions[RegionNames.Overlay].Deactivate(activeRegion);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public DelegateCommand CloseDialogCommand { get; }

        public DelegateCommand<string> StartProcessCommand { get; }
    }
}
