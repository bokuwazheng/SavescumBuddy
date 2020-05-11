using Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private BackupRepository _backupRepository;
        private AutobackupManager _autobackupManager;
        private GlobalKeyboardHook _keyboardListener;
        
        public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetProperty(ref _currentViewModel, value); }
        public readonly List<BaseViewModel> ViewModels;

        public ApplicationViewModel()
        {
            _backupRepository = new BackupRepository();
            _backupRepository.BackupAdded += b => Util.BackupFiles(b);
            _backupRepository.BackupDeleted += b => Util.MoveToTrash(b);

            _autobackupManager = new AutobackupManager();
            _keyboardListener = new GlobalKeyboardHook();

            var mainVm = new MainViewModel(_backupRepository, _autobackupManager);
            var settingsVm = new SettingsViewModel();
            var aboutVm = new AboutViewModel();

            settingsVm.Settings.AutobackupsIsEnabledChanged += isEnabled => _autobackupManager.OnIsEnabledChanged(isEnabled);
            settingsVm.Settings.SelectedInvervalChanged += isEnabled => _autobackupManager.OnIntervalChanged(isEnabled);

            ViewModels = new List<BaseViewModel>()
            {
                mainVm, settingsVm, aboutVm
            };

            NavigateToMainCommand = new DelegateCommand(() => CurrentViewModel = GetViewModel<MainViewModel>());
            NavigateToSettingsCommand = new DelegateCommand(() => CurrentViewModel = GetViewModel<SettingsViewModel>());
            NavigateToAboutCommand = new DelegateCommand(() => CurrentViewModel = GetViewModel<AboutViewModel>());

            NavigateToMainCommand.Execute();
        }

        public DelegateCommand NavigateToMainCommand { get; }
        public DelegateCommand NavigateToSettingsCommand { get; }
        public DelegateCommand NavigateToAboutCommand { get; }

        public T GetViewModel<T>() => ViewModels.OfType<T>().First();

        public void Hook()
        {
            if (Settings.Default.HotkeysOn)
            {
                _keyboardListener.Hook();
                _keyboardListener.KeyDown += _keyboardListener_KeyDown;
            }
        }

        public void Unhook()
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
                        var backup = BackupFactory.CreateBackup();
                        var mainVm = ViewModels.OfType<MainViewModel>().First();
                        mainVm.AddCommand.Execute(backup);
                        Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch (NullReferenceException ex)
                    {
                        Util.PopUp(ex.Message);
                    }

                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyValue == Settings.Default.QLKey)
            {
                if ((int)e.Modifiers == Settings.Default.QLMod)
                {
                    try
                    {
                        var latest = SqliteDataAccess.GetLatestBackup();
                        if (latest is null)
                            return;
                        Util.Restore(latest);
                        Util.PlaySound(WavLocator.restore_cue);
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp(ex.Message);
                    }

                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyValue == Settings.Default.SOKey)
            {
                if ((int)e.Modifiers == Settings.Default.SOMod)
                {
                    try
                    {
                        var latest = SqliteDataAccess.GetLatestBackup();
                        if (latest is object)
                            _backupRepository.Remove(latest);
                        var backup = BackupFactory.CreateBackup();
                        var mainVm = ViewModels.OfType<MainViewModel>().First();
                        mainVm.AddCommand.Execute(backup);
                        Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch (NullReferenceException ex)
                    {
                        Util.PopUp(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp(ex.Message);
                    }

                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
