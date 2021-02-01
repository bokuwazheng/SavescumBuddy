using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Wpf.Events;
using MaterialDesignThemes.Wpf;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Wpf.Services;
using System.Windows.Forms;
using SavescumBuddy.Wpf.Mvvm;
using SavescumBuddy.Wpf.Constants;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class SettingsViewModel : BaseViewModel, INavigationAware
    {
        private readonly ISettingsAccess _settingsAccess;
        private readonly ISnackbarMessageQueue _messageQueue;
        private readonly IGlobalKeyboardHook _keyboardHook;

        private HotkeyAction _selectedHotkeyAction;

        public SettingsViewModel(ISettingsAccess settingsAccess, IEventAggregator eventAggregator, IRegionManager regionManager, ISnackbarMessageQueue messageQueue, IGlobalKeyboardHook keyboardHook) 
            : base(regionManager, eventAggregator)
        {
            _settingsAccess = settingsAccess;
            _messageQueue = messageQueue;
            _keyboardHook = keyboardHook;

            Settings = new SettingsModel(_settingsAccess);
            Settings.PropertyChanged += (s, e) => OnSettingsPropertyChanged(e.PropertyName);

            RegisterHotkeyCommand = new DelegateCommand<HotkeyAction?>(x => { if (x.HasValue) SelectedHotkeyAction = x.Value; });
            OpenAboutDialogCommand = new DelegateCommand(() => ShowDialog(ViewNames.About, null));
        }

        public SettingsModel Settings { get; }
        public HotkeyAction SelectedHotkeyAction
        {
            get => _selectedHotkeyAction;
            set
            {
                _selectedHotkeyAction = _selectedHotkeyAction == value
                    ? HotkeyAction.None
                    : value;
                RaisePropertyChanged(nameof(SelectedHotkeyAction));

                if (_selectedHotkeyAction is HotkeyAction.None)
                {
                    _keyboardHook.Unhook();
                    _keyboardHook.KeyDown -= OnKeyDown;
                }
                else
                {
                    if (!_keyboardHook.HookActive)
                    {
                        _keyboardHook.Hook();
                        _keyboardHook.KeyDown += OnKeyDown;
                    }
                }
            }
        }

        private void OnSettingsPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(SettingsModel.AutobackupsEnabled))
                _eventAggregator.GetEvent<AutobackupsEnabledChangedEvent>().Publish(Settings.AutobackupsEnabled);

            if (propertyName == nameof(SettingsModel.AutobackupInterval))
                _eventAggregator.GetEvent<AutobackupIntervalChangedEvent>().Publish(Settings.AutobackupsEnabled);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var key = Keys.None;
            if (e.KeyValue > 0) key = e.KeyCode;

            if (key is Keys.Escape)
            {
                SelectedHotkeyAction = HotkeyAction.None;
                return;
            }

            if (key is Keys.Enter or Keys.Space or Keys.Back)
                return;

            var mod = Keys.None;
            if (e.Alt) mod = Keys.Alt;
            if (e.Shift) mod = Keys.Shift;
            if (e.Control) mod = Keys.Control;

            if (key is
                Keys.LMenu or Keys.RMenu or
                Keys.LShiftKey or Keys.RShiftKey or
                Keys.LControlKey or Keys.RControlKey)
                mod = Keys.None;

            if (SelectedHotkeyAction == HotkeyAction.Backup)
            {
                Settings.BackupKey = (int)key;
                Settings.BackupModifier = (int)mod;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Restore)
            {
                Settings.RestoreKey = (int)key;
                Settings.RestoreModifier = (int)mod;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Overwrite)
            {
                Settings.OverwriteKey = (int)key;
                Settings.OverwriteModifier = (int)mod;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _eventAggregator.GetEvent<HookEnabledChangedEvent>().Publish(false);
            _messageQueue.Enqueue("Hotkeys are disabled while this tab is open.");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            SelectedHotkeyAction = HotkeyAction.None;
            _eventAggregator.GetEvent<HookEnabledChangedEvent>().Publish(_settingsAccess.HotkeysEnabled);
        }

        public DelegateCommand<HotkeyAction?> RegisterHotkeyCommand { get; }
        public DelegateCommand OpenAboutDialogCommand { get; }
    }
}
