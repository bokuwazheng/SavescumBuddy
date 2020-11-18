using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using System;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class NotificationDialogViewModel : BindableBase, INavigationAware
    {
        private string _title;
        private string _message;
        private event Action<DialogResult> _requestClose;

        public NotificationDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        
        private void CloseDialog(DialogResult? result)
        {
            if (result.HasValue)
                _requestClose?.Invoke(result.Value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.Count == 0)
                return;
            Message = navigationContext.Parameters["message"].ToString();
            Title = navigationContext.Parameters["title"].ToString();
            _requestClose = (Action<DialogResult>)navigationContext.Parameters["callback"];
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _requestClose = null;
        }

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
    }
}