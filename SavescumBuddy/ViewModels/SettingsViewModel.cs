using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavescumBuddy.Models;
using Common;
using System.Windows.Forms;
using SavescumBuddy.Sqlite;
using System.Collections.Generic;
using SavescumBuddy.Enums;

namespace SavescumBuddy.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private bool _importInProgress;
        private int _importProgress;
        private GlobalKeyboardHook _keyboardHook;
        private HotkeyAction? _recordedHotkeyType = null;

        public SettingsModel Settings { get; }
        public HotkeyAction? SelectedHotkeyAction { get => _recordedHotkeyType; set => SetProperty(ref _recordedHotkeyType, value); }
        public string AuthorizedAs => GetUserEmail();
        public int ImportProgress { get => _importProgress; private set => SetProperty(ref _importProgress, value); }
        public bool ImportInProgress { get => _importInProgress; private set => SetProperty(ref _importInProgress, value); }
        public string CurrentGameTitle => SqliteDataAccess.GetCurrentGame()?.Title ?? "none";
        public ObservableCollection<GameModel> Games { get; private set; }

        public SettingsViewModel()
        {
            _keyboardHook = new GlobalKeyboardHook();
            Settings = new SettingsModel();
            Settings.PropertyChanged += (s, e) => Properties.Settings.Default.Save();

            AddGameCommand = new DelegateCommand(AddEmptyGame);
            UploadCustomCommand = new DelegateCommand(async () => await UploadBackups().ConfigureAwait(false));
            RegisterHotkeyCommand = new DelegateCommand<HotkeyAction?>(x => ToggleKeyboardHook(x));
            AuthorizeCommand = new DelegateCommand(async () => await AuthorizeAsync().ConfigureAwait(false));
            ReauthorizeCommand = new DelegateCommand(async () => await ReauthorizeAsync().ConfigureAwait(false));

            UpdateGameList();
            if (Games.Count == 0)
                AddEmptyGame();

            TryAuthorize();
        }

        private void TryAuthorize()
        {
            var tokenFolder = GoogleDrive.TokenFolderName;
            var folderExists = Directory.Exists(tokenFolder);
            if (folderExists)
            {
                var tokenExists = Directory.GetFiles(tokenFolder).Any(x => x.Contains("Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
                if (tokenExists)
                    AuthorizeCommand?.Execute();
            }
        }

        private void ToggleKeyboardHook(HotkeyAction? actionType)
        {
            if (actionType != SelectedHotkeyAction)
            {
                if (!_keyboardHook.HookActive)
                {
                    _keyboardHook.Hook();
                    _keyboardHook.KeyDown += keyboardHook_KeyDown;
                }

                SelectedHotkeyAction = actionType;
            }
            else 
            {
                if (_keyboardHook.HookActive)
                {
                    _keyboardHook.Unhook();
                    _keyboardHook.KeyDown -= keyboardHook_KeyDown;
                }

                SelectedHotkeyAction = null;
            }
        }

        private void keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ToggleKeyboardHook(SelectedHotkeyAction);
                return;
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                return;

            var mod = Keys.None;
            if (e.Alt) mod = Keys.Alt;
            if (e.Shift) mod = Keys.Shift;
            if (e.Control) mod = Keys.Control;

            var key = Keys.None;
            if (e.KeyValue > 0) key = e.KeyCode;

            if (key == Keys.LMenu || key == Keys.RMenu ||
                key == Keys.LShiftKey || key == Keys.RShiftKey ||
                key == Keys.LControlKey || key == Keys.RControlKey)
                mod = Keys.None;

            if (SelectedHotkeyAction == HotkeyAction.Save)
            {
                Settings.SelectedQSKey = key;
                Settings.SelectedQSMod = mod;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Restore)
            {
                Settings.SelectedQLKey = key;
                Settings.SelectedQLMod = mod;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Overwrite)
            {
                Settings.SelectedSOKey = key;
                Settings.SelectedSOMod = mod;
            }
        }

        private async Task UploadBackups()
        {
            var game = SqliteDataAccess.GetCurrentGame();

            if (game is null)
            {
                Util.PopUp("No game is set as current yet.");
                return;
            }

            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.IsFolderPicker = true;
                dialog.ShowHiddenItems = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var folders = dialog.FileNames;
                    var savefileName = Path.GetFileName(game.SavefilePath);
                    var fileList = new List<string>();
                    foreach(var folder in folders)
                    {
                        var files = Directory.GetFiles(folder, savefileName, SearchOption.AllDirectories);
                        fileList.AddRange(files);
                    }

                    var sb = new StringBuilder();

                    var importQCount = fileList.Count();
                    var importedCount = 0;

                    ImportInProgress = true;

                    await Task.Run(() =>
                    {
                        foreach (string file in fileList)
                        {
                            // Report progress.
                            ImportProgress = (++importedCount * 100) / importQCount;

                            var parentFolder = Path.GetDirectoryName(file);
                            var folderItems = Directory.GetFiles(parentFolder);
                            var picture = folderItems.FirstOrDefault(s => s.EndsWith(".jpg"));

                            try
                            {
                                SqliteDataAccess.SaveBackup(new Backup()
                                {
                                    IsAutobackup = 0,
                                    GameId = game.Title,
                                    Origin = game.SavefilePath,
                                    DateTimeTag = "N/A",
                                    Picture = picture ?? "",
                                    FilePath = file
                                });
                            }
                            catch
                            {
                                sb.Append($"Error: {file} is already in the list.\n");
                                continue;
                            }
                        }
                    });

                    // Reset progress.
                    ImportInProgress = false;
                    ImportProgress = 0;

                    if (sb.Length != 0)
                        Util.PopUp(sb.ToString());
                }
            }
        }

        private async Task CreateAppRootFolderAsync()
        {
            var rootId = await GoogleDrive.Current.GetAppRootFolderIdAsync().ConfigureAwait(false);
            if (rootId is null)
                rootId = await GoogleDrive.Current.CreateAppRootFolderAsync().ConfigureAwait(false);

            Settings.CloudAppRootFolderId = rootId;
            Settings.Save();
        }

        private async Task AuthorizeAsync()
        {
            var credentials = GoogleDrive.CredentialsFileName;
            var token = GoogleDrive.TokenFolderName;
            var userCredential = await GoogleDrive.Current.AuthorizeAsync(credentials, token).ConfigureAwait(false);
            if (userCredential is null)
                return;
            GoogleDrive.Current.UserCredential = userCredential;
            await CreateAppRootFolderAsync().ConfigureAwait(false);

            RaisePropertyChanged(nameof(AuthorizedAs));
        }

        private async Task ReauthorizeAsync()
        {
            var userCredential = GoogleDrive.Current.UserCredential;
            if (userCredential is object)
            {
                await GoogleDrive.Current.ReauthorizeAsync(userCredential).ConfigureAwait(false);
                await CreateAppRootFolderAsync().ConfigureAwait(false);
                RaisePropertyChanged(nameof(AuthorizedAs));
            }
            else
            {
                await AuthorizeAsync().ConfigureAwait(false);
                RaisePropertyChanged(nameof(AuthorizedAs));
            }
        }

        private string GetUserEmail()
        {
            try
            {
                var service = GoogleDrive.Current.GetDriveApiService();
                var request = service.About.Get();
                request.Fields = "user(emailAddress)";
                var result = request.Execute();
                return result.User.EmailAddress;
            }
            catch
            {
                return null;
            }
        }

        public void UpdateGameList()
        {
            var games = SqliteDataAccess.LoadGames();
            var gameModels = games.Select(x => new GameModel(x)).ToList();
            gameModels.ForEach(x => x.StateChanged += () => UpdateGameList());
            Games = new ObservableCollection<GameModel>(gameModels);
            RaisePropertyChanged(nameof(Games));
            RaisePropertyChanged(nameof(CurrentGameTitle));
        }

        public void AddEmptyGame()
        {
            var game = new Game()
            {
                SavefilePath = "",
                BackupFolder = "",
                Title = ""
            };

            SqliteDataAccess.SaveGame(game);
            UpdateGameList();
        }

        public DelegateCommand AddGameCommand { get; }
        public DelegateCommand UploadCustomCommand { get; }
        public DelegateCommand<HotkeyAction?> RegisterHotkeyCommand { get; }
        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand ReauthorizeCommand { get; }
    }
}