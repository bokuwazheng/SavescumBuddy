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

namespace SavescumBuddy.ViewModels
{
    public class SettingsViewModel : BaseViewModel, Game.IListItemEventListener
    {
        public SettingsViewModel()
        {
            this.PropertyChanged += (s, e) =>
            {
                CheckIfValid(e.PropertyName);
                Properties.Settings.Default.Save();
            };

            AddGameCommand = new DelegateCommand(() =>
            {
                AddGame();
            });

            UploadCustomCommand = new DelegateCommand(() =>
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

                        foreach (string folder in folders)
                        {
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

                        if (sb.Length != 0)
                        {
                            sb.Append("\nTip: Savefiles must be located in separate folders. " +
                                "To attach an image put it in the folder next to savefile.");

                            Util.PopUp(sb.ToString());
                        }

                        if (handled)
                        {
                            BackupRepository.Current.Backups.Clear();
                            BackupRepository.Current.LoadSortedList();
                        }
                    }
                }
            });

            if (Games.Count == 0) AddGame();

            LoadGameList();
        }

        public DelegateCommand AddGameCommand { get; }
        public DelegateCommand UploadCustomCommand { get; }

        #region Game MGMT
        public string CurrentGameTitle => SqliteDataAccess.GetCurrentGame() != null ? SqliteDataAccess.GetCurrentGame().Title : "none";

        public ObservableCollection<Game> Games { get; private set; } = new ObservableCollection<Game>(SqliteDataAccess.LoadGames());

        public void LoadGameList()
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
            LoadGameList();
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
            Keys.F7,
            Keys.F8
        };

        public Keys SelectedQLKey
        {
            get { return (Keys)Properties.Settings.Default.QLKey; }
            set { Properties.Settings.Default.QLKey = (int)value; RaisePropertyChanged("SelectedQLKey"); }
        }
        public Keys SelectedQSKey
        {
            get { return (Keys)Properties.Settings.Default.QSKey; }
            set { Properties.Settings.Default.QSKey = (int)value; RaisePropertyChanged("SelectedQSKey"); }
        }

        //Modifiers 
        public List<string> Modifiers => UserFriendlyModifiers.AsList;

        public Keys SelectedQLMod
        {
            get { return (Keys)Properties.Settings.Default.QLMod; }
            set { Properties.Settings.Default.QLMod = (int)value; RaisePropertyChanged("SelectedQLMod"); }
        }
        public Keys SelectedQSMod
        {
            get { return (Keys)Properties.Settings.Default.QSMod; }
            set { Properties.Settings.Default.QSMod = (int)value; RaisePropertyChanged("SelectedQSMod"); }
        }

        // On/off
        public bool HotkeysOn
        {
            get { return Properties.Settings.Default.HotkeysOn; }
            set { Properties.Settings.Default.HotkeysOn = value; RaisePropertyChanged("HotkeysOn"); }
        }

        // Selected keys validation method
        private void CheckIfValid(string key)
        {
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
            get { return Properties.Settings.Default.AutobackupsOn; }
            set
            {
                Properties.Settings.Default.AutobackupsOn = value;
                OnTimerStateUpdated(new TimerEventArgs(true));
                RaisePropertyChanged("AutobackupsOn");
            }
        }
        public string SelectedSkipOption
        {
            get { return Properties.Settings.Default.Skip; }
            set { Properties.Settings.Default.Skip = value; RaisePropertyChanged("SelectedSkipOption"); }
        }

        public string SelectedInterval
        {
            get { return Properties.Settings.Default.Interval + " min"; }
            set
            {
                Properties.Settings.Default.Interval = int.Parse(Regex.Match(value, @"\d+").Value);
                OnTimerStateUpdated(new TimerEventArgs(false));
                RaisePropertyChanged("SelectedInterval");
            }
        }

        public string SelectedOverwriteOption
        {
            get { return Properties.Settings.Default.Overwrite; }
            set { Properties.Settings.Default.Overwrite = value; RaisePropertyChanged("SelectedOverwriteOption"); }
        }
        #endregion

        #region Sound cues
        public bool SoundCuesOn
        {
            get { return Properties.Settings.Default.SoundCuesOn; }
            set { Properties.Settings.Default.SoundCuesOn = value; RaisePropertyChanged("SoundCuesOn"); }
        }
        #endregion

        public delegate void TimerHandler(object sender, TimerEventArgs e);
        public static event TimerHandler TimerStateUpdated;
        protected virtual void OnTimerStateUpdated(TimerEventArgs e)
        {
            TimerHandler handler = TimerStateUpdated;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public static void Subscribe(TimerHandler handler)
        {
            TimerStateUpdated += handler;
        }

        public void StateChanged()
        {
            LoadGameList();
        }
    }
}
