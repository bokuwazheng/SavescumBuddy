﻿using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Core.Events;
using System;
using System.Linq;

namespace SavescumBuddy.Modules.Overlay.ViewModels
{
    public class NotificationDialogViewModel : BindableBase, INavigationAware, IJournalAware
    {
        private string _title;
        private string _message;
        private string _okContent;
        private string _cancelContent;
        private event Action<DialogResult> _requestClose;
        private IRegionNavigationService _navigationService;
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;

        public NotificationDialogViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            CloseDialogCommand = new DelegateCommand<DialogResult?>(CloseDialog);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public string OkContent { get => _okContent; set => SetProperty(ref _okContent, value); }
        public string CancelContent { get => _cancelContent; set => SetProperty(ref _cancelContent, value); }

        private void CloseDialog(DialogResult? result)
        {
            try
            {
                if (result.HasValue)
                {
                    _requestClose?.Invoke(result.Value);
                }

                if (_navigationService.Journal.CanGoBack)
                    _navigationService.Journal.GoBack();
                else
                {
                    var activeRegion = _regionManager.Regions[RegionNames.Overlay].ActiveViews.FirstOrDefault();

                    if (activeRegion is object)
                        _regionManager.Regions[RegionNames.Overlay].Deactivate(activeRegion);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;

            if (navigationContext.Parameters.Count == 0)
                return;
            Message = navigationContext.Parameters["message"].ToString();
            Title = navigationContext.Parameters["title"].ToString();
            OkContent = navigationContext.Parameters["okContent"]?.ToString() ?? "OK";
            CancelContent = navigationContext.Parameters["cancelContent"]?.ToString();
            _requestClose = (Action<DialogResult>)navigationContext.Parameters["callback"];
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public bool PersistInHistory() => false;

        public DelegateCommand<DialogResult?> CloseDialogCommand { get; }
    }
}