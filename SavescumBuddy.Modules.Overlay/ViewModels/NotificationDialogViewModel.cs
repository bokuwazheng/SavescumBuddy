﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
using SavescumBuddy.Core.Enums;
using System.Linq;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class NotificationDialogViewModel : BindableBase, INavigationAware
    {
        private string _title;
        private string _message;
        private IRegionNavigationService _navigationService;
        private IRegionManager _regionManager;

        public NotificationDialogViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            
            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        
        private void CloseDialog(DialogResult? result)
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

            if (navigationContext.Parameters.Count == 0)
                return;
            Message = navigationContext.Parameters["message"].ToString();
            Title = navigationContext.Parameters["title"].ToString();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
    }
}