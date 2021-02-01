using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Wpf.Events;
using SavescumBuddy.Wpf.Extensions;
using SavescumBuddy.Lib;
using SavescumBuddy.Wpf.Models;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SavescumBuddy.Wpf.Services;
using SavescumBuddy.Wpf.Mvvm;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class BackupsViewModel : BaseViewModel, INavigationAware
    {
        private readonly IDataAccess _dataAccess;
        private readonly IBackupService _backupService;
        private readonly IGoogleDrive _googleDrive;
        private readonly ISnackbarMessageQueue _messageQueue;

        // Backing fields
        private CancellationTokenSource _cts;
        private FilterModel _filter;
        private BackupModel _selectedBackup;
        private int _currentPageIndex;
        private int _totalCount;

        public BackupsViewModel(IRegionManager regionManager, IDataAccess dataAccess, IEventAggregator eventAggregator, IBackupService backupService, 
            IGoogleDrive googleDrive, ISnackbarMessageQueue messageQueue) : base(regionManager, eventAggregator)
        {
            _dataAccess = dataAccess;
            _backupService = backupService;
            _googleDrive = googleDrive;
            _messageQueue = messageQueue;

            _eventAggregator.GetEvent<BackupListUpdateRequestedEvent>().Subscribe(UpdateBackups);

            RefreshCommand = new DelegateCommand(UpdateBackups);
            AddCommand = new DelegateCommand(Add);
            RemoveCommand = new DelegateCommand<BackupModel>(Remove, x => x is object).ObservesProperty(() => SelectedBackup);
            RemoveSelectedCommand = new DelegateCommand(RemoveSelected, () => IsAllItemsSelected ?? true).ObservesProperty(() => IsAllItemsSelected);
            RestoreCommand = new DelegateCommand<BackupModel>(Restore, x => x is object).ObservesProperty(() => SelectedBackup);

            NavigateForwardCommand = new DelegateCommand(() => ++CurrentPageIndex, () => To < TotalNumberOfBackups);
            NavigateBackwardCommand = new DelegateCommand(() => --CurrentPageIndex, () => From > 1);
            NavigateToStartCommand = new DelegateCommand(() => CurrentPageIndex = 0, () => From > 1);
            NavigateToEndCommand = new DelegateCommand(() => CurrentPageIndex = TotalNumberOfBackups / PageSize, () => To < TotalNumberOfBackups);

            ShowInExplorerCommand = new DelegateCommand<BackupModel>(ShowInExplorer, x => x is object).ObservesProperty(() => SelectedBackup);
            UpdateNoteCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateNote(x.Backup));
            UpdateIsLikedCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateIsLiked(x.Backup));
            ExecuteDriveActionCommand = new DelegateCommand<BackupModel>(async x => await ExecuteCloudAction(x, Ct).ConfigureAwait(false), 
                x => x is object && _googleDrive.UserCredential is object).ObservesProperty(() => SelectedBackup);
            RecoverCommand = new DelegateCommand<BackupModel>(async x => await RecoverAsync(x, Ct).ConfigureAwait(false), 
                x => x is object && x.GoogleDriveId is object && !File.Exists(x.Backup.SavefilePath) && _googleDrive.UserCredential is object).ObservesProperty(() => SelectedBackup);

            Filter.PropertyChanged += (s, e) => OnFilterPropertyChanged(e.PropertyName);
        }

        // Properties
        public CancellationToken Ct
        {
            get
            {
                if (_cts is null || _cts.IsCancellationRequested)
                    _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }
        public bool? IsAllItemsSelected
        {
            get
            {
                var selected = Backups.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count is 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var backup in Backups)
                    {
                        backup.IsSelected = value.Value;
                    }
                }
            }
        }
        public ObservableCollection<BackupModel> Backups { get; } = new();
        public ObservableCollection<Game> Games { get; } = new();
        public int CurrentPageIndex { get => _currentPageIndex; private set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public BackupModel SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value); }
        public FilterModel Filter { get => _filter ??= new(); private set => SetProperty(ref _filter, value); }
        public int TotalNumberOfBackups { get => _totalCount; set => SetProperty(ref _totalCount, value); }
        public int PageSize => 10; 
        public int From => Backups.Count > 0 ? Filter.Offset.Value + 1 : 0;
        public int To => Filter.Offset.Value + Backups.Count;

        private void OnFilterPropertyChanged(string propName)
        {
            UpdateBackups();
            if (!propName.Equals(nameof(Filter.Offset)))
                NavigateToStartCommand?.Execute();
        }

        private void RaiseNavigateCanExecute()
        {
            NavigateForwardCommand.RaiseCanExecuteChanged();
            NavigateBackwardCommand.RaiseCanExecuteChanged();
            NavigateToStartCommand.RaiseCanExecuteChanged();
            NavigateToEndCommand.RaiseCanExecuteChanged();
        }

        private void RaiseDriveActionCanExecute()
        {
            ExecuteDriveActionCommand.RaiseCanExecuteChanged();
            RecoverCommand.RaiseCanExecuteChanged();
        }

        private void Add() => Handle(() =>
        {
            var backup = _dataAccess.CreateBackup(isScheduled: false);
            _backupService.BackupSavefile(backup);
            _backupService.SaveScreenshot(backup.PicturePath);
            UpdateBackups();
        });

        private void Remove(BackupModel backup) => Handle(() =>
        {
            if (backup is null)
                return;

            if (backup.GoogleDriveId is object) // TODO: TEST UI UPDATES
            {
                _regionManager.PromptAction(
                    "Delete from Google Drive too?",
                    "If you leave the backup in Google Drive you'll be able to recover it later.",
                    "DELETE",
                    "LEAVE BACKUP IN GOOGLE DRIVE",
                    async r =>
                    {
                        if (r is DialogResult.Abort)
                            return;

                        await HandleAsync(async () =>
                        {
                            if (r is DialogResult.OK)
                            {
                                await _googleDrive.DeleteBackupAsync(backup.Backup);
                                _dataAccess.DeleteBackup(backup.Backup.Id);
                                RaiseDriveActionCanExecute();
                            }
                            else if (r is DialogResult.Cancel)
                            {
                                _backupService.DeleteFiles(backup.Backup);

                            }
                            UpdateBackups();
                        });
                    });
            }
            else
            {
                _dataAccess.DeleteBackup(backup.Backup.Id);
                _backupService.DeleteFiles(backup.Backup);
                UpdateBackups();
            }
        });

        public void UpdateBackups() => Handle(() =>
        {
            var response = _dataAccess.SearchBackups(Filter);
            var backupModels = response.Backups.Select(x => new BackupModel(x)).ToList();

            foreach (var model in backupModels)
            {
                model.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BackupModel.IsSelected))
                    {
                        RaisePropertyChanged(nameof(IsAllItemsSelected));
                        RemoveSelectedCommand.RaiseCanExecuteChanged();
                    }
                };
            }

            Backups.ReplaceRange(backupModels);
            TotalNumberOfBackups = response.TotalCount;

            RaisePropertyChanged(nameof(From));
            RaisePropertyChanged(nameof(To));
            RaiseNavigateCanExecute();
        });

        private void ShowInExplorer(BackupModel backup) => Handle(() =>
        {
            var dirName = Path.GetDirectoryName(backup.Backup.SavefilePath);
            _eventAggregator.GetEvent<StartProcessRequestedEvent>().Publish(dirName);
        });

        private void Restore(BackupModel backup) => Handle(() =>
        {
            _backupService.RestoreBackup(backup.Backup);
            _messageQueue.Enqueue("Backup was restored!");
        });

        public void GetGames() => Handle(() =>
        {
            var selected = Filter.GameId;

            var games = _dataAccess.GetGames();
            games.Add(new Game() { Title = "ALL" });
            Games.Clear();
            Games.AddRange(games);

            Filter.GameId = -1;
            Filter.GameId = selected;
        });

        // TODO: lock?
        // TODO: check if succeeded in finally?
        // TODO: check if this solution is ok for multiple files/operations
        private async Task ExecuteCloudAction(BackupModel backupModel, CancellationToken ct = default) => await HandleAsync(async () =>
        {
            var backup = backupModel.Backup;
            if (backup.GoogleDriveId is null)
            {
                _messageQueue.Enqueue("Upload started...", "CANCEL", x => _cts?.Cancel(), null, true, true);

                var backupCloudFolderId = await _googleDrive.UploadBackupAsync(backup, ct).ConfigureAwait(true);

                _messageQueue.Enqueue("Upload finished!", "", () => { }, true);

                if (backupCloudFolderId is object)
                {
                    backupModel.GoogleDriveId = backupCloudFolderId;
                    _dataAccess.UpdateGoogleDriveId(backup);
                }
            }
            else
            {
                var countdown = TimeSpan.FromSeconds(3d);
                _messageQueue.Enqueue("Undo deletion?", "UNDO", x => _cts?.Cancel(), null, true, true, countdown);
                await Task.Delay(countdown);

                await _googleDrive.DeleteBackupAsync(backup, ct).ConfigureAwait(false);

                backupModel.GoogleDriveId = null;
                _dataAccess.UpdateGoogleDriveId(backup);
            }
        });

        private void RemoveSelected() => Handle(() =>
        {
            var selected = Backups.Where(item => item.IsSelected).ToList();

            if (selected.Any())
            {
                foreach (var backup in selected)
                {
                    RemoveCommand.Execute(backup);
                }
            }
        });

        private async Task RecoverAsync(BackupModel backup, CancellationToken ct) => await HandleAsync(async () =>
        {
            await _googleDrive.RecoverAsync(backup.Backup, () => _messageQueue.Enqueue($"Download completed!"), ct).ConfigureAwait(false);
            RefreshCommand.Execute();
        });

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            GetGames();
            UpdateBackups();
            RaisePropertyChanged(nameof(IsAllItemsSelected));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<BackupModel> RemoveCommand { get; }
        public DelegateCommand RemoveSelectedCommand { get; }
        public DelegateCommand<BackupModel> RestoreCommand { get; }
        public DelegateCommand NavigateForwardCommand { get; }
        public DelegateCommand NavigateBackwardCommand { get; }
        public DelegateCommand NavigateToEndCommand { get; }
        public DelegateCommand NavigateToStartCommand { get; }
        public DelegateCommand<BackupModel> ShowInExplorerCommand { get; }
        public DelegateCommand<BackupModel> UpdateNoteCommand { get; }
        public DelegateCommand<BackupModel> UpdateIsLikedCommand { get; }
        public DelegateCommand<BackupModel> ExecuteDriveActionCommand { get; }
        public DelegateCommand<BackupModel> RecoverCommand { get; }
    }
}
