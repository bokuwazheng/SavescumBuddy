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
        private AutobackupManager _autobackupManager;
        private GlobalKeyboardHook _keyboardListener;

        public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetProperty(ref _currentViewModel, value); }
        public readonly List<BaseViewModel> ViewModels;

        public ApplicationViewModel()
        {
            _autobackupManager = new AutobackupManager();
            _keyboardListener = new GlobalKeyboardHook();

            var mainVm = new MainViewModel();
            var settingsVm = new SettingsViewModel();
            var aboutVm = new AboutViewModel();

            settingsVm.Settings.AutobackupsIsEnabledChanged += isEnabled => _autobackupManager.OnIsEnabledChanged(isEnabled);
            settingsVm.Settings.SelectedInvervalChanged += isEnabled => _autobackupManager.OnIntervalChanged(isEnabled);

            _autobackupManager.AdditionRequested += () => mainVm.AddCommand.Execute();
            _autobackupManager.DeletionRequested += x => mainVm.RemoveCommand.Execute(x);

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
                        var mainVm = ViewModels.OfType<MainViewModel>().First();
                        mainVm.AddCommand.Execute();
                        if (Settings.Default.SoundCuesOn)
                            Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp(ex.Message);
                    }

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
                        if (Settings.Default.SoundCuesOn)
                            Util.PlaySound(WavLocator.restore_cue);
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp(ex.Message);
                    }

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
                        var mainVm = ViewModels.OfType<MainViewModel>().First();
                        mainVm.AddCommand.Execute();
                        if (Settings.Default.SoundCuesOn)
                            Util.PlaySound(WavLocator.backup_cue);
                        if (latest is object)
                            mainVm.RemoveCommand.Execute(latest);
                    }
                    catch (Exception ex)
                    {
                        Util.PopUp(ex.Message);
                    }

                    return;
                }
            }
        }
    }
}