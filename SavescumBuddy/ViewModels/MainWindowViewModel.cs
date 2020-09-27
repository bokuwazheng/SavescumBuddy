using Prism.Mvvm;

namespace SavescumBuddy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Savescum Buddy";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
