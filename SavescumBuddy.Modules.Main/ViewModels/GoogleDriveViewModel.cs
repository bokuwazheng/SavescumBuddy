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

            AuthorizeCommand = new DelegateCommand(() => AuthorizeAsync(Ct).ConfigureAwait(false));
            ReauthorizeCommand = new DelegateCommand(() => ReauthorizeAsync(Ct).ConfigureAwait(false));
            UpdateUserEmailCommand = new DelegateCommand(() => UpdateUserEmailAsync(Ct).ConfigureAwait(false));
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

        private Task AuthorizeAsync(CancellationToken ct) => HandleAsync(async () =>
        {
            var succeeded = await _googleDrive.AuthorizeAsync(ct).ConfigureAwait(false);
            if (!succeeded)
                throw new Exception("Failed to authorize.");

            await UpdateUserEmailAsync(ct).ConfigureAwait(false);
        });

        private Task ReauthorizeAsync(CancellationToken ct) => HandleAsync(async () =>
        {
            await _googleDrive.ReauthorizeAsync(ct).ConfigureAwait(false);
            await UpdateUserEmailAsync(ct).ConfigureAwait(false);
        });

        private Task UpdateUserEmailAsync(CancellationToken ct) => HandleAsync(async () =>
        {
            if (_googleDrive.CredentialExists())
                UserEmail = await _googleDrive.GetUserEmailAsync(ct).ConfigureAwait(false);
        });

        public void OnNavigatedTo(NavigationContext navigationContext) 
        { 
            if (UserEmail is null)
                UpdateUserEmailCommand.Execute(); 
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand ReauthorizeCommand { get; }
        public DelegateCommand UpdateUserEmailCommand { get; }
        public DelegateCommand CancelCommand { get; }
    }
}
