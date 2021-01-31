using MaterialDesignThemes.Wpf;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Wpf.Events;

namespace SavescumBuddy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Savescum Buddy";
        private bool _dialogHostIsOpen;
        private readonly IEventAggregator _eventAggregator;

        public MainWindowViewModel(ISnackbarMessageQueue messageQueue, IEventAggregator eventAggregator)
        {
            MessageQueue = messageQueue;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<DialogIsOpenChangedEvent>().Subscribe(x => DialogHostIsOpen = x, ThreadOption.UIThread);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool DialogHostIsOpen { get => _dialogHostIsOpen; set => SetProperty(ref _dialogHostIsOpen, value); }

        public ISnackbarMessageQueue MessageQueue { get; }
    }
}
