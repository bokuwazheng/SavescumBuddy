using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Wpf.Constants;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Extensions;
using System;
using System.Threading.Tasks;

namespace SavescumBuddy.Wpf.Mvvm
{
    public class BaseViewModel : BindableBase
    {
        protected readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;

        public BaseViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
        }

        protected void Handle(Action action, Action onException = null, Action onFinally = null) 
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                onException?.Invoke();
                ShowError(ex);
            }
            finally
            {
                onFinally?.Invoke();
            }
        }

        protected async Task HandleAsync(Func<Task> action, Action onException = null, Action onFinally = null)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                onException?.Invoke();
                ShowError(ex);
            }
            finally
            {
                onFinally?.Invoke();
            }
        }

        protected void ShowDialog(string viewName, NavigationParameters navigationParameters)
        {
            _eventAggregator.GetEvent<DialogIsOpenChangedEvent>().Publish(true);
            _regionManager.RequestNavigate(RegionNames.Overlay, viewName, navigationParameters);
        }

        protected void PromptAction(string title, string message, string okContent, string cancelContent, Action<DialogResult> callback)
        {
            _eventAggregator.GetEvent<DialogIsOpenChangedEvent>().Publish(true);
            _regionManager.PromptAction(title, message, okContent, cancelContent, callback);
        }

        protected void ShowError(Exception ex)
        {
            _eventAggregator.GetEvent<DialogIsOpenChangedEvent>().Publish(true);
            _regionManager.ShowError(ex.Message);
        }
    }
}
