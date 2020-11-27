using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private IGoogleDrive _googleDrive;
        private IEventAggregator _eventAggregator;

        private CancellationTokenSource _cts;
        private string _userEmail;

        public GoogleDriveViewModel(IGoogleDrive googleDrive, IEventAggregator eventAggregator)
        {
            _googleDrive = googleDrive;
            _eventAggregator = eventAggregator;

            AuthorizeCommand = new DelegateCommand(async () => await AuthorizeAsync(Ct).ConfigureAwait(false));
            ReauthorizeCommand = new DelegateCommand(async () => await ReauthorizeAsync(Ct).ConfigureAwait(false));
            UpdateUserEmailCommand = new DelegateCommand(async () => await UpdateUserEmailAsync(Ct).ConfigureAwait(false));
            CancelCommand = new DelegateCommand(() => _cts?.Cancel());
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
        public string UserEmail { get => _userEmail; set => SetProperty(ref _userEmail, value); }

        private async Task AuthorizeAsync(CancellationToken ct)
        {
            try
            {
                var succeeded = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
                if (!succeeded)
                    throw new Exception("Failed to authorize.");

                await UpdateUserEmailAsync(ct).ConfigureAwait(false);
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

                await UpdateUserEmailAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private async Task UpdateUserEmailAsync(CancellationToken ct)
        {
            try
            {
                UserEmail = await _googleDrive.GetUserEmailAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand ReauthorizeCommand { get; }
        public DelegateCommand UpdateUserEmailCommand { get; }
        public DelegateCommand CancelCommand { get; }
    }
}
