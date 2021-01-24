using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Core.Events;
using MaterialDesignThemes.Wpf;
using SavescumBuddy.Core.Constants;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class SettingsViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISnackbarMessageQueue _messageQueue;

        private HotkeyAction? _recordedHotkeyType = null;

        public SettingsModel Settings { get; }
        public HotkeyAction? SelectedHotkeyAction { get => _recordedHotkeyType; set => SetProperty(ref _recordedHotkeyType, value); }

        public SettingsViewModel(IRegionManager regionManager, ISettingsAccess settingsAccess, IEventAggregator eventAggregator, ISnackbarMessageQueue messageQueue)
        {
            _regionManager = regionManager;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;
            _messageQueue = messageQueue;

            _eventAggregator.GetEvent<HookKeyDownEvent>().Subscribe(keyboardHook_KeyDown);

            Settings = new SettingsModel(_settingsAccess);
            Settings.PropertyChanged += (s, e) => OnSettingsPropertyChanged(e.PropertyName);

            RegisterHotkeyCommand = new DelegateCommand<HotkeyAction?>(ToggleKeyboardHook);
            NavigateToBackupsCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.Content, ViewNames.Backups));
        }

        private void OnSettingsPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(SettingsModel.AutobackupsEnabled))
                _eventAggregator.GetEvent<AutobackupsEnabledChangedEvent>().Publish(Settings.AutobackupsEnabled);

            if (propertyName == nameof(SettingsModel.AutobackupInterval))
                _eventAggregator.GetEvent<AutobackupIntervalChangedEvent>().Publish(Settings.AutobackupsEnabled);

            if (propertyName == nameof(SettingsModel.HotkeysEnabled))
                _eventAggregator.GetEvent<HookEnabledChangedEvent>().Publish(Settings.HotkeysEnabled);
        }

        // TODO: better way to unhook cuz passing SelectedHotkeyAction is kinda tricky
        private void ToggleKeyboardHook(HotkeyAction? actionType)
        {
            if (actionType != SelectedHotkeyAction)
                SelectedHotkeyAction = actionType;
            else
                SelectedHotkeyAction = null;

            _eventAggregator.GetEvent<HookChangedEvent>().Publish(SelectedHotkeyAction.HasValue);
        }

        private void keyboardHook_KeyDown((int Key, int Modifier) keys) // TODO: SHOULDN'T KNOW ANYTHING ABOUT KEYS
        {
            if (keys.Key == 27) // Keys.Escape
            {
                ToggleKeyboardHook(SelectedHotkeyAction);
                return;
            }

            if (keys.Key == 13 || keys.Key == 32 || keys.Key == 8) // Keys.Enter or Keys.Space or Keys.Back
                return;

            if (SelectedHotkeyAction == HotkeyAction.Backup)
            {
                Settings.BackupKey = keys.Key;
                Settings.BackupModifier = keys.Modifier;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Restore)
            {
                Settings.RestoreKey = keys.Key;
                Settings.RestoreModifier = keys.Modifier;
            }
            else if (SelectedHotkeyAction == HotkeyAction.Overwrite)
            {
                Settings.OverwriteKey = keys.Key;
                Settings.OverwriteModifier = keys.Modifier;
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
            _eventAggregator.GetEvent<HookEnabledChangedEvent>().Publish(_settingsAccess.HotkeysEnabled);
        }

        public DelegateCommand<HotkeyAction?> RegisterHotkeyCommand { get; }
        public DelegateCommand NavigateToBackupsCommand { get; }
    }
}
