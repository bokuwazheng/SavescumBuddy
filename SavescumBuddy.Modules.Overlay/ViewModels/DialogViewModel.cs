using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using System;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class DialogViewModel : OverlayBaseViewModel, INavigationAware, IJournalAware
    {
        private IRegionNavigationService _navigationService;

        private string _title;
        private string _message;
        private string _okContent;
        private string _cancelContent;
        private Action<DialogResult> _requestClose;

        public DialogViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            CloseDialogCommand = new(CloseDialog);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public string OkContent { get => _okContent; set => SetProperty(ref _okContent, value); }
        public string CancelContent { get => _cancelContent; set => SetProperty(ref _cancelContent, value); }

        private void CloseDialog(DialogResult? result) => Handle(() =>
        {
            if (result.HasValue)
            {
                if (_navigationService.Journal.CanGoBack)
                    _navigationService.Journal.GoBack();
                else
                {
                    _navigationService.Journal.Clear();
                    CloseDialog();
                }

                _requestClose?.Invoke(result.Value);
            }
        });

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;

            if (navigationContext.Parameters.Count == 0)
                return;

            Message = navigationContext.Parameters["message"].ToString();
            Title = navigationContext.Parameters["title"].ToString();
            OkContent = navigationContext.Parameters["okContent"]?.ToString();
            CancelContent = navigationContext.Parameters["cancelContent"]?.ToString();
            _requestClose = (Action<DialogResult>)navigationContext.Parameters["callback"];
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public bool PersistInHistory() => false;

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
    }
}