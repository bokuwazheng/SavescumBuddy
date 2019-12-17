using SavescumBuddy.ValueConverters;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy.ViewModels
{
    public class SettingsViewModel : BaseViewModel, Game.IListItemEventListener
    {
        public string AuthorizedAs
        {
            get
            {
                try
                {
                    var service = GoogleDrive.Current.CreateDriveApiService();
                    var request = service.About.Get();
                    request.Fields = "*";
                    return request.Execute().User.EmailAddress;
                }
                catch
                {
                    return "not authorized";
                }
            }
        }

        public int ImportProgress { get; set; } = 0;
        public bool ImportInProgress { get; set; }
        private BackupRepository _backupRepository;
        private AutobackupManager _autobackupManager;

        public SettingsViewModel(BackupRepository repo, AutobackupManager manager)
        {
            _backupRepository = repo;
            _autobackupManager = manager;

            this.PropertyChanged += (s, e) =>
            {
                CheckHotkeysValidity(e.PropertyName);
                Properties.Settings.Default.Save();
            };

            AddGameCommand = new DelegateCommand(() =>
            {
                AddGame();
            });

            UploadCustomCommand = new DelegateCommand(async() =>
            {
                var game = SqliteDataAccess.GetCurrentGame();

                if (game == null)
                {
                    Util.PopUp("No game is set as current yet."); return;
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
                        var dateTimeNow = DateTime.Now;
                        var sec = 0;
                        var sb = new StringBuilder();
                        var handled = false;

                        var importQ = folders.Count();
                        var itemsImported = 0;

                        ImportInProgress = true;
                        RaisePropertyChanged("ImportInProgress");

                        await Task.Run(() => 
                        {
                            foreach (string folder in folders)
                            {
                                // Report progress.
                                itemsImported++;
                                ImportProgress = (itemsImported * 100) / importQ;
                                RaisePropertyChanged("ImportProgress");

                                var folderItems = Directory.GetFiles(folder);
                                if (folderItems.Length == 0)
                                {
                                    sb.Append($"Error: {folder} is empty.\n");
                                    continue;
                                }
                                var filePath = folderItems.FirstOrDefault(s => s.EndsWith(savefileName));
                                if (filePath == null)
                                {
                                    sb.Append($"Error: {savefileName} expected.\n");
                                    continue;
                                }
                                var picture = folderItems.FirstOrDefault(s => s.EndsWith(".jpg"));
                                var dateTimeTag = dateTimeNow + TimeSpan.FromSeconds(sec);
                                sec++;

                                try
                                {
                                    SqliteDataAccess.SaveBackup(new Backup()
                                    {
                                        IsAutobackup = 0,
                                        GameId = game.Title,
                                        Origin = game.SavefilePath,
                                        DateTimeTag = dateTimeTag.ToString(DateTimeFormat.UserFriendly, CultureInfo.CreateSpecificCulture("en-US")),
                                        Picture = picture != null ? picture : "",
                                        FilePath = filePath
                                    });

                                    handled = true;
                                }
                                catch
                                {
                                    sb.Append($"Error: {filePath} is already in the list.\n");
                                    continue;
                                }
                            }
                        });

                        // Reset progress.
                        ImportInProgress = false;
                        ImportProgress = 0;
                        RaisePropertyChanged("ImportInProgress");
                        RaisePropertyChanged("ImportProgress");

                        if (sb.Length != 0)
                        {
                            sb.Append("\nTip: Savefiles must be located in separate folders. " +
                                "To attach an image put it in the folder next to the savefile.");

                            Util.PopUp(sb.ToString());
                        }

                        if (handled)
                        {
                            _backupRepository.LoadBackupsFromPage("1");
                        }
                    }
                }
            });

            AuthorizeCommand = new DelegateCommand(async() =>
            {
                try
                {
                    await GoogleDrive.Current.AuthorizeAsync();
                }
                catch (Exception ex)
                {
                    Util.PopUp($"Error from SettingsViewModel: { ex.Message }");
                }

                RaisePropertyChanged("AuthorizedAs");
            });

            if (Games.Count == 0) AddGame();

            UpdateGameList();
        }

        public DelegateCommand AddGameCommand { get; }
        public DelegateCommand UploadCustomCommand { get; }
        public DelegateCommand AuthorizeCommand { get; }

        #region Game MGMT
        public string CurrentGameTitle => SqliteDataAccess.GetCurrentGame() != null ? SqliteDataAccess.GetCurrentGame().Title : "none";

        public ObservableCollection<Game> Games { get; private set; } = new ObservableCollection<Game>(SqliteDataAccess.LoadGames());

        public void UpdateGameList()
        {
            Games = new ObservableCollection<Game>(SqliteDataAccess.LoadGames());
            foreach (Game game in Games) game.SetListener(this);
            RaisePropertyChanged("Games");
            RaisePropertyChanged("CurrentGameTitle");
        }

        public void AddGame()
        {
            Game game = new Game()
            {
                SavefilePath = "",
                BackupFolder = "",
                Title = ""
            };

            SqliteDataAccess.SaveGame(game);
            UpdateGameList();
        }
        #endregion

        #region Hotkeys
        //Keys
        public List<Keys> HotKeys => new List<Keys>
        {
            Keys.NumPad0,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.O,
            Keys.J,
            Keys.C,
            Keys.S,
            Keys.V,
            Keys.F6,
            Keys.F7,
            Keys.F8
        };

        public Keys SelectedQLKey
        {
            get { return (Keys)Settings.Default.QLKey; }
            set { Settings.Default.QLKey = (int)value; RaisePropertyChanged("SelectedQLKey"); }
        }
        public Keys SelectedQSKey
        {
            get { return (Keys)Settings.Default.QSKey; }
            set { Settings.Default.QSKey = (int)value; RaisePropertyChanged("SelectedQSKey"); }
        }
        public Keys SelectedSOKey
        {
            get { return (Keys)Settings.Default.SOKey; }
            set { Settings.Default.SOKey = (int)value; RaisePropertyChanged("SelectedSOKey"); }
        }

        //Modifiers 
        public List<string> Modifiers => UserFriendlyModifiers.AsList;

        public Keys SelectedQLMod
        {
            get { return (Keys)Settings.Default.QLMod; }
            set { Settings.Default.QLMod = (int)value; RaisePropertyChanged("SelectedQLMod"); }
        }
        public Keys SelectedQSMod
        {
            get { return (Keys)Settings.Default.QSMod; }
            set { Settings.Default.QSMod = (int)value; RaisePropertyChanged("SelectedQSMod"); }
        }
        public Keys SelectedSOMod
        {
            get { return (Keys)Settings.Default.SOMod; }
            set { Settings.Default.SOMod = (int)value; RaisePropertyChanged("SelectedSOMod"); }
        }

        // On/off
        public bool HotkeysOn
        {
            get { return Settings.Default.HotkeysOn; }
            set { Settings.Default.HotkeysOn = value; RaisePropertyChanged("HotkeysOn"); }
        }

        // Selected keys validation method
        private void CheckHotkeysValidity(string key)
        {
            // compare QL and QS
            if (SelectedQLKey == SelectedQSKey && SelectedQLMod == SelectedQSMod)
            {
                if (key.Equals("SelectedQLKey") || key.Equals("SelectedQLMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedQLKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedQSKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedQSKey = HotKeys[0];
                    }
                }

                if (key.Equals("SelectedQSKey") || key.Equals("SelectedQSMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedQSKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedQLKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedQLKey = HotKeys[0];
                    }
                }
            }

            // compare QL and SO
            if (SelectedQLKey == SelectedSOKey && SelectedQLMod == SelectedSOMod)
            {
                if (key.Equals("SelectedQLKey") || key.Equals("SelectedQLMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedQLKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedSOKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedSOKey = HotKeys[0];
                    }
                }

                if (key.Equals("SelectedSOKey") || key.Equals("SelectedSOMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedSOKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedQLKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedQLKey = HotKeys[0];
                    }
                }
            }

            // compare QS and SO
            if (SelectedQSKey == SelectedSOKey && SelectedQSMod == SelectedSOMod)
            {
                if (key.Equals("SelectedQSKey") || key.Equals("SelectedQSMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedQSKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedSOKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedSOKey = HotKeys[0];
                    }
                }

                if (key.Equals("SelectedSOKey") || key.Equals("SelectedSOMod"))
                {
                    var keyIndex = HotKeys.FindIndex(k => k.Equals(SelectedSOKey));

                    if (keyIndex > -1 && keyIndex < HotKeys.Count - 1)
                    {
                        SelectedQSKey = HotKeys[keyIndex + 1];
                    }
                    else
                    {
                        SelectedQSKey = HotKeys[0];
                    }
                }
            }
        }
        #endregion

        #region Autobackup options
        public List<string> SkipOptions => SkipOptionsEnum.AsList();

        public static class SkipOptionsEnum
        {
            public const string Never = "Never";
            public const string FiveMin = "If any backup was created <5 min ago";
            public const string TenMin = "If any backup was created <10 min ago";

            public static List<string> AsList()
            {
                return new List<string>()
                {
                    Never, FiveMin, TenMin
                };
            }
        }

        public List<string> IntervalList => Intervals.AsList();

        public class Intervals
        {
            private static int[] _intervals = new int[] { 5, 10, 15, 20, 30, 40, 50, 60 };

            public static List<string> AsList()
            {
                var output = new List<string>();
                foreach (int i in _intervals) output.Add(i + " min");
                return output;
            }
        }

        public List<string> OverwriteOptions => OverwriteOptionsEnum.AsList();

        public static class OverwriteOptionsEnum
        {
            public const string Never = "Never";
            public const string Always = "Always";
            public const string KeepLiked = "Keep liked autobackups";

            public static List<string> AsList()
            {
                return new List<string>()
                {
                    Never, Always, KeepLiked
                };
            }
        }

        public bool AutobackupsOn
        {
            get { return Settings.Default.AutobackupsOn; }
            set
            {
                Settings.Default.AutobackupsOn = value;
                _autobackupManager.OnEnabledChanged(value);
                RaisePropertyChanged("AutobackupsOn");
            }
        }
        public string SelectedSkipOption
        {
            get { return Settings.Default.Skip; }
            set { Settings.Default.Skip = value; RaisePropertyChanged("SelectedSkipOption"); }
        }

        public string SelectedInterval
        {
            get { return Settings.Default.Interval + " min"; }
            set
            {
                Settings.Default.Interval = int.Parse(Regex.Match(value, @"\d+").Value);
                _autobackupManager.OnIntervalChanged();
                RaisePropertyChanged("SelectedInterval");
            }
        }

        public string SelectedOverwriteOption
        {
            get { return Settings.Default.Overwrite; }
            set { Settings.Default.Overwrite = value; RaisePropertyChanged("SelectedOverwriteOption"); }
        }
        #endregion

        #region Sound cues
        public bool SoundCuesOn
        {
            get { return Settings.Default.SoundCuesOn; }
            set { Settings.Default.SoundCuesOn = value; RaisePropertyChanged("SoundCuesOn"); }
        }
        #endregion

        public void StateChanged()
        {
            UpdateGameList();
        }
    }
}
