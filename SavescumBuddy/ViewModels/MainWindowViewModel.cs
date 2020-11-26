using MaterialDesignThemes.Wpf;
using Prism.Mvvm;

namespace SavescumBuddy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Savescum Buddy";

        public MainWindowViewModel(ISnackbarMessageQueue messageQueue)
        {
            MessageQueue = messageQueue;
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); } 

        public ISnackbarMessageQueue MessageQueue { get; }
    }
}
