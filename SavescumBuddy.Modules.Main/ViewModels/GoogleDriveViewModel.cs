using Prism.Commands;
using Prism.Events;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using SavescumBuddy.Wpf.Mvvm;
using Prism.Regions;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class GoogleDriveViewModel : BaseViewModel, INavigationAware
    {
        private IGoogleDrive _googleDrive;

        private CancellationTokenSource _cts;
        private string _userEmail;

        public GoogleDriveViewModel(IGoogleDrive googleDrive, IEventAggregator eventAggregator, IRegionManager regionManager) : base(regionManager, eventAggregator)
        {
            _googleDrive = googleDrive;

            AuthorizeCommand = new DelegateCommand(async () => await AuthorizeAsync(Ct).ConfigureAwait(false));
            ReauthorizeCommand = new DelegateCommand(async () => await ReauthorizeAsync(Ct).ConfigureAwait(false));
            UpdateUserEmailCommand = new DelegateCommand(async () => await UpdateUserEmailAsync(Ct).ConfigureAwait(false));
            CancelCommand = new DelegateCommand(() => _cts?.Cancel()); // TODO: NEVER USED!
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

        private async Task AuthorizeAsync(CancellationToken ct) => await HandleAsync(async () =>
        {
            var succeeded = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
            if (!succeeded)
                throw new Exception("Failed to authorize.");

            await UpdateUserEmailAsync(ct).ConfigureAwait(false);
        });

        private async Task ReauthorizeAsync(CancellationToken ct) => await HandleAsync(async () =>
        {
            await _googleDrive.ReauthorizeAsync(ct).ConfigureAwait(false);
            await UpdateUserEmailAsync(ct).ConfigureAwait(false);
        });

        private async Task UpdateUserEmailAsync(CancellationToken ct) => await HandleAsync(async () =>
        {
            if (_googleDrive.CredentialExists())
                UserEmail = await _googleDrive.GetUserEmailAsync(ct).ConfigureAwait(false);
        });

        public void OnNavigatedTo(NavigationContext navigationContext) => UpdateUserEmailCommand.Execute();

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand ReauthorizeCommand { get; }
        public DelegateCommand UpdateUserEmailCommand { get; }
        public DelegateCommand CancelCommand { get; }
    }
}
