using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Modules.Main.Models;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Core;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;

        private HotkeyAction? _recordedHotkeyType = null;

        public SettingsModel Settings { get; }
        public HotkeyAction? SelectedHotkeyAction { get => _recordedHotkeyType; set => SetProperty(ref _recordedHotkeyType, value); }

        public SettingsViewModel(IRegionManager regionManager, ISettingsAccess settingsAccess, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<HookKeyDownEvent>().Subscribe(keyboardHook_KeyDown);

            Settings = new SettingsModel(_settingsAccess);
            Settings.PropertyChanged += (s, e) => OnSettingsPropertyChanged(e.PropertyName);

            RegisterHotkeyCommand = new DelegateCommand<HotkeyAction?>(ToggleKeyboardHook);
            NavigateToBackupsCommand = new DelegateCommand(() => _regionManager.RequestNavigate(RegionNames.ContentRegion, "Backups"));
        }

        private void OnSettingsPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(SettingsModel.AutobackupsEnabled))
                _eventAggregator.GetEvent<AutobackupsEnabledChangedEvent>().Publish(Settings.AutobackupsEnabled);

            if (propertyName == nameof(SettingsModel.AutobackupInterval))
                _eventAggregator.GetEvent<AutobackupIntervalChangedEvent>().Publish(Settings.AutobackupsEnabled);
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

        private void keyboardHook_KeyDown((int Key, int Modifier) keys)
        {
            if (keys.Key == 27) // Keys.Escape
            {
                ToggleKeyboardHook(SelectedHotkeyAction);
                return;
            }

            if (keys.Key == 13 || keys.Key == 32) // Keys.Enter or Keys.Space
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

        public DelegateCommand<HotkeyAction?> RegisterHotkeyCommand { get; }
        public DelegateCommand NavigateToBackupsCommand { get; }
    }
}
