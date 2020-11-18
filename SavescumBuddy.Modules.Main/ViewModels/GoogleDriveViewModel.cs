using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private IGoogleDrive _googleDrive;
        private ISettingsAccess _settingsAccess;
        private IEventAggregator _eventAggregator;
        private IDataAccess _dataAccess;

        private CancellationTokenSource _cts;
        private string _authorizedAs;

        public GoogleDriveViewModel(IGoogleDrive googleDrive, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, IDataAccess dataAccess)
        {
            _googleDrive = googleDrive;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _dataAccess = dataAccess;

            AuthorizeCommand = new DelegateCommand(async () => await AuthorizeAsync(Ct).ConfigureAwait(false));
            ReauthorizeCommand = new DelegateCommand(async () => await ReauthorizeAsync(Ct).ConfigureAwait(false));
            CancelCommand = new DelegateCommand(() => _cts?.Cancel());

            if (_googleDrive.CredentialExists())
                AuthorizeCommand.Execute();
        }

        public CancellationToken Ct
        {
            get
            {
                if (_cts is null || _cts.IsCancellationRequested)
                    _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }
        public string AuthorizedAs { get => _authorizedAs; set => SetProperty(ref _authorizedAs, value); }

        private async Task AuthorizeAsync(CancellationToken ct)
        {
            try
            {
                var succeeded = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
                if (!succeeded)
                    throw new Exception("Failed to authorize.");

                AuthorizedAs = await _googleDrive.GetUserEmailAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private async Task ReauthorizeAsync(CancellationToken ct)
        {
            try
            {
                var succeeded = await _googleDrive.ReauthorizeAsync(ct).ConfigureAwait(false);
                if (!succeeded)
                    throw new Exception("Failed to reauthorize.");

                AuthorizedAs = await _googleDrive.GetUserEmailAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand ReauthorizeCommand { get; }
        public DelegateCommand CancelCommand { get; }
    }
}
