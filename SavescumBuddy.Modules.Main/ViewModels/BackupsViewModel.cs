using Google.Apis.Download;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Core.Extensions;
using SavescumBuddy.Data;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class BackupsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataAccess _dataAccess;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBackupService _backupService;
        private readonly IBackupFactory _backupFactory;
        private readonly IGoogleDrive _googleDrive;
        private readonly ISnackbarMessageQueue _messageQueue;

        // Backing fields
        private CancellationTokenSource _cts;
        private FilterModel _filter;
        private BackupModel _selectedBackup;
        private int _currentPageIndex;

        public BackupsViewModel(IRegionManager regionManager, IDataAccess dataAccess, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, 
            IBackupService backupService, IBackupFactory backupFactory, IGoogleDrive googleDrive, ISnackbarMessageQueue messageQueue)
        {
            _regionManager = regionManager;
            _dataAccess = dataAccess;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _backupService = backupService;
            _backupFactory = backupFactory;
            _googleDrive = googleDrive;
            _messageQueue = messageQueue;

            _eventAggregator.GetEvent<BackupListUpdateRequestedEvent>().Subscribe(UpdateBackups);

            AddCommand = new DelegateCommand(Add);
            RemoveCommand = new DelegateCommand<BackupModel>(Remove);
            RemoveSelectedCommand = new DelegateCommand(RemoveSelected, () => IsAllItemsSelected.HasValue ? IsAllItemsSelected.Value != false : true);
            RestoreCommand = new DelegateCommand<BackupModel>(x => _backupService.RestoreBackup(x.Backup));

            NavigateForwardCommand = new DelegateCommand(() => ++CurrentPageIndex, () => To < TotalNumberOfBackups);
            NavigateBackwardCommand = new DelegateCommand(() => --CurrentPageIndex, () => From > 1);
            NavigateToStartCommand = new DelegateCommand(() => CurrentPageIndex = 0, () => From > 1);
            NavigateToEndCommand = new DelegateCommand(() => CurrentPageIndex = TotalNumberOfBackups / PageSize, () => To < TotalNumberOfBackups);

            ShowInExplorerCommand = new DelegateCommand<BackupModel>(x => _eventAggregator.GetEvent<StartProcessRequestedEvent>().Publish(Path.GetDirectoryName(x.SavefilePath)));
            UpdateNoteCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateNote(x.Backup));
            UpdateIsLikedCommand = new DelegateCommand<BackupModel>(x => _dataAccess.UpdateIsLiked(x.Backup));
            ExecuteDriveActionCommand = new DelegateCommand<BackupModel>(async x => await ExecuteCloudAction(x, Ct).ConfigureAwait(false), x => _googleDrive.UserCredential is object);
            RecoverCommand = new DelegateCommand<BackupModel>(async x => await RecoverAsync(x, Ct).ConfigureAwait(false), x => x.GoogleDriveId is object && !File.Exists(x.SavefilePath) && _googleDrive.UserCredential is object);

            Filter.PropertyChanged += (s, e) => OnFilterPropertyChanged(e.PropertyName);
            UpdateBackups();
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
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var backup in Backups)
                    {
                        backup.IsSelected = value.Value;
                    }
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<BackupModel> Backups { get; private set; }
        public ObservableCollection<Game> Games => GetGames();
        public bool CurrentGameIsSet => _dataAccess.GetCurrentGame() is object;
        public int CurrentPageIndex { get => _currentPageIndex; private set => SetProperty(ref _currentPageIndex, value, () => Filter.Offset = value * PageSize); }
        public BackupModel SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value, RaiseDriveActionCanExecute); }
        public FilterModel Filter { get => _filter ??= new FilterModel(); private set => SetProperty(ref _filter, value); }
        public int TotalNumberOfBackups => _dataAccess.GetTotalNumberOfBackups(Filter);
        public int PageSize => 10; // _settingsAccess.BackupsPerPage
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

        private void Add()
        {
            try
            {
                var backup = _backupFactory.CreateBackup();
                _dataAccess.SaveBackup(backup);
                _backupService.BackupSavefile(backup);
                _backupService.SaveScreenshot(backup.PicturePath);
                UpdateBackups();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        // TODO: If in Google Drive -- promtp user to delete files from clould (leave the backup in DB and adjust context menu accordingly)
        private void Remove(BackupModel backup)
        {
            try
            {
                if (backup is null)
                    return;
                else if (backup.GoogleDriveId is object)
                {
                    _regionManager.PromptAction(
                        "Would you also like to delete the backup from Google Drive? If you leave the backup in Google Drive you'll be able to recover it later.",
                        "DELETE",
                        "LEAVE BACKUP IN GOOGLE DRIVE",
                        r =>
                        {
                            if (r == DialogResult.None)
                                return;

                            if (r == DialogResult.OK)
                            {
                                _googleDrive.DeleteBackupAsync(backup.Backup);
                                _dataAccess.RemoveBackup(backup.Backup);
                                UpdateBackups();
                            }

                            _backupService.DeleteFiles(backup.Backup);
                        });
                }
                else
                {
                    _dataAccess.RemoveBackup(backup.Backup);
                    _backupService.DeleteFiles(backup.Backup);
                    UpdateBackups();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        public void UpdateBackups()
        {
            try
            {
                var backups = _dataAccess.SearchBackups(Filter);
                var backupModels = backups.Select(x => new BackupModel(x)).ToList();

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

                    model.GameTitle = _dataAccess.GetGame(model.GameId).Title;
                }

                Backups = new ObservableCollection<BackupModel>(backupModels);
                RaisePropertyChanged(nameof(Backups));
                RaisePropertyChanged(nameof(TotalNumberOfBackups));
                RaisePropertyChanged(nameof(From));
                RaisePropertyChanged(nameof(To));
                RaiseNavigateCanExecute();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        // TODO: list doesn't not get updated after a new game is added
        public ObservableCollection<Game> GetGames()
        {
            try
            {
                var games = _dataAccess.LoadGames();
                games.Add(new Game() { Title = "ALL" });
                return new ObservableCollection<Game>(games);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
            return null;
        }

        // TODO: lock?
        // TODO: check if succeeded in finally?
        // TODO: check if this solution is ok for multiple files/operations
        private async Task ExecuteCloudAction(BackupModel backupModel, CancellationToken ct = default)
        {
            var backup = backupModel.Backup;
            try
            {
                if (backup.GoogleDriveId is null)
                {
                    _messageQueue.Enqueue("Upload started...", "CANCEL", x => _cts?.Cancel(), null, true, true);

                    var gameTitle = _dataAccess.GetGame(backup.GameId).Title;
                    var backupCloudFolderId = await _googleDrive.UploadBackupAsync(backup, gameTitle, ct).ConfigureAwait(true);

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

                    var result = await _googleDrive.DeleteBackupAsync(backup, ct).ConfigureAwait(false);

                    backupModel.GoogleDriveId = null;
                    _dataAccess.UpdateGoogleDriveId(backup);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void RemoveSelected()
        {
            var selected = Backups.Where(item => item.IsSelected);

            if (selected.Any())
            {
                foreach (var backup in selected)
                {
                    RemoveCommand.Execute(backup);
                }
            }
        }

        private async Task RecoverAsync(BackupModel backup, CancellationToken ct)
        {
            try
            {
                var files = await _googleDrive.GetFilesAsync(backup.GoogleDriveId, IGoogleDrive.MimeType.Backup, ct).ConfigureAwait(false);
                if (files.Count > 0)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(backup.SavefilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(backup.SavefilePath));
                    }

                    //using var savefile = new FileStream(backup.SavefilePath, FileMode.Create, FileAccess.Write);
                    //using var picture = new FileStream(backup.PicturePath, FileMode.Create, FileAccess.Write);
                    //using var savefile = File.Create(backup.SavefilePath);
                    using var picture = File.Create(backup.PicturePath);

                    Action<long?, IDownloadProgress> callback = (l, p) =>
                    {
                        var message = p.Status switch
                        {
                            DownloadStatus.NotStarted => "Not Started",
                            DownloadStatus.Downloading when l.HasValue => $"Downloading ({ p.BytesDownloaded } out of { l.Value } bytes)",
                            DownloadStatus.Downloading => $"Downloading",
                            DownloadStatus.Completed => "Completed",
                            DownloadStatus.Failed => "Failed",
                            _ => ""
                        };
                        _messageQueue.Enqueue(message, "", () => { }, true);
                    };

                    //await _googleDrive.ExportAsync(files[0], IGoogleDrive.MimeType.File, savefile, callback, ct).ConfigureAwait(false);
                    await _googleDrive.ExportAsync(files[0], IGoogleDrive.MimeType.Image, picture, callback, ct).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

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
