using Prism.Commands;
using System.Collections.Generic;

namespace SavescumBuddy.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        public ApplicationViewModel()
        {
            ViewModels.Add(new MainViewModel());
            ViewModels.Add(new SettingsViewModel());
            ViewModels.Add(new AboutViewModel());

            CurrentViewModel = ViewModels[0];

            ChangeViewModelCommand = new DelegateCommand<string>(s =>
            {
                switch (s)
                {
                    case (NavigateTo.Main):
                        CurrentViewModel = ViewModels[0];
                        break;
                    case (NavigateTo.Settings):
                        CurrentViewModel = ViewModels[1];
                        break;
                    case (NavigateTo.About):
                        CurrentViewModel = ViewModels[2];
                        break;
                }
            });
        }

        private List<BaseViewModel> _viewModels;
        public List<BaseViewModel> ViewModels
        {
            get
            {
                if (_viewModels == null) _viewModels = new List<BaseViewModel>();
                return _viewModels;
            }
        }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set { if (_currentViewModel != value) _currentViewModel = value; RaisePropertyChanged("CurrentViewModel"); }
        }

        public DelegateCommand<string> ChangeViewModelCommand { get; }

        public static class NavigateTo
        {
            public const string Main = "Main";
            public const string Settings = "Settings";
            public const string About = "About";
        }

        public string ToMain => NavigateTo.Main;
        public string ToSettings => NavigateTo.Settings;
        public string ToAbout => NavigateTo.About;

    }
}
