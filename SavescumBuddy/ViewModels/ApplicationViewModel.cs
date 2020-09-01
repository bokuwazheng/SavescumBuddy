using Common;
using Prism.Commands;
using System;
using Settings = SavescumBuddy.Properties.Settings;
using SavescumBuddy.Sqlite;

namespace SavescumBuddy.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private GlobalKeyboardHook _keyboardListener;

        public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetProperty(ref _currentViewModel, value); }

        public ApplicationViewModel(IDataAccess dataAccess) : base(dataAccess)
        {
            _keyboardListener = new GlobalKeyboardHook();

            NavigateToMainCommand = new DelegateCommand(() => CurrentViewModel = App.GetService<MainViewModel>());
            NavigateToSettingsCommand = new DelegateCommand(() => CurrentViewModel = App.GetService<SettingsViewModel>());
            NavigateToAboutCommand = new DelegateCommand(() => CurrentViewModel = App.GetService<AboutViewModel>());

            NavigateToMainCommand.Execute();
        }

        public DelegateCommand NavigateToMainCommand { get; }
        public DelegateCommand NavigateToSettingsCommand { get; }
        public DelegateCommand NavigateToAboutCommand { get; }

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
                        var mainVm = App.GetService<MainViewModel>();
                        mainVm.AddCommand.Execute();
                        if (Settings.Default.SoundCuesOn)
                            Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch (Exception ex)
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
                        if (Settings.Default.SoundCuesOn)
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
                        var mainVm = App.GetService<MainViewModel>();
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

                    e.Handled = true;
                    return;
                }
            }
        }
    }
}