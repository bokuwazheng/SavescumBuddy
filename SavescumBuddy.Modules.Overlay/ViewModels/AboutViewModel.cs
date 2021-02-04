using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class AboutViewModel : OverlayBaseViewModel, INavigationAware
    {
        public AboutViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            StartProcessCommand = new DelegateCommand<string>(s => _eventAggregator.GetEvent<StartProcessRequestedEvent>().Publish(s));
            CloseDialogCommand = new DelegateCommand(CloseDialog);
        }

        public string Version => System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

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
