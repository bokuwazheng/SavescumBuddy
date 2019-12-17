using Common;
using Prism.Commands;
using System.Collections.Generic;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        private BackupFactory _backupFactory;
        private BackupRepository _backupRepository;
        private AutobackupManager _autobackupManager;
        private GlobalKeyboardHook _keyboardListener;

        private List<BaseViewModel> _viewModels;
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set { if (_currentViewModel != value) _currentViewModel = value; RaisePropertyChanged("CurrentViewModel"); }
        }

        public ApplicationViewModel()
        {
            _backupFactory = new BackupFactory();
            _backupRepository = new BackupRepository();
            _autobackupManager = new AutobackupManager(_backupRepository, _backupFactory);
            _keyboardListener = new GlobalKeyboardHook();

            _viewModels = new List<BaseViewModel>();
            _viewModels.Add(new MainViewModel(_backupRepository, _backupFactory, _autobackupManager));
            _viewModels.Add(new SettingsViewModel(_backupRepository, _autobackupManager));
            _viewModels.Add(new AboutViewModel());

            CurrentViewModel = _viewModels[0];
            MainViewLoaded();
        }

        public DelegateCommand<string> ChangeViewModelCommand => new DelegateCommand<string>(s =>
        {
            switch (s)
            {
                case (NavigateTo.Main):
                    CurrentViewModel = _viewModels[0]; MainViewLoaded();
                    break;
                case (NavigateTo.Settings):
                    CurrentViewModel = _viewModels[1]; MainViewUnloaded();
                    break;
                case (NavigateTo.About):
                    CurrentViewModel = _viewModels[2];
                    break;
            }
        });

        private void MainViewLoaded()
        {
            if (Settings.Default.HotkeysOn)
            {
                _keyboardListener.Hook();
                _keyboardListener.KeyDown += _keyboardListener_KeyDown;
            }
        }

        private void MainViewUnloaded()
        {
            _keyboardListener.Unhook();
            _keyboardListener.KeyDown -= _keyboardListener_KeyDown;
        }

        private void _keyboardListener_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == Settings.Default.QSKey)
            {
                if ((int)e.Modifiers == Settings.Default.QSMod)
                {
                    try
                    {
                        _backupRepository.Add(_backupFactory.CreateBackup());
                        Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch
                    {
                        Util.PopUp("Failed to create a backup: no game is set as current yet.");
                    }

                    e.Handled = true;
                }
            }

            if (e.KeyValue == Settings.Default.QLKey)
            {
                if ((int)e.Modifiers == Settings.Default.QLMod)
                {
                    try
                    {
                        _backupRepository.RestoreLatest();
                        Util.PlaySound(WavLocator.restore_cue);
                    }
                    catch
                    {
                        Util.PopUp("There is no backup to restore yet.");
                    }

                    e.Handled = true;
                }
            }

            if (e.KeyValue == Settings.Default.SOKey)
            {
                if ((int)e.Modifiers == Settings.Default.SOMod)
                {
                    try
                    {
                        _backupRepository.Remove(SqliteDataAccess.GetLatestBackup());
                        _backupRepository.Add(_backupFactory.CreateBackup());
                        Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch
                    {
                        Util.PopUp("There is no backup to overwrite yet.");
                    }

                    e.Handled = true;
                }
            }
        }

        public static class NavigateTo
        {
            public const string Main = "Main";
            public const string Settings = "Settings";
            public const string About = "About";
        }

        public string ToMain => NavigateTo.Main;
        public string ToSettings => NavigateTo.Settings;
        public string ToAbout => NavigateTo.About;
    }
}
