using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Modules.Main.Models;
using System.Windows.Input;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Core;

namespace SavescumBuddy.Modules.Main.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ISettingsAccess _settingsAccess;
        private readonly IEventAggregator _eventAggregator;
        private readonly IHotkeyListener<Key> _hotkeyListener;

        private HotkeyAction? _recordedHotkeyType = null;

        public SettingsModel Settings { get; }
        public HotkeyAction? SelectedHotkeyAction { get => _recordedHotkeyType; set => SetProperty(ref _recordedHotkeyType, value); }

        public SettingsViewModel(IRegionManager regionManager, ISettingsAccess settingsAccess, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _settingsAccess = settingsAccess;
            _eventAggregator = eventAggregator;

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
            {
                if (!_hotkeyListener.HookActive)
                {
                    _hotkeyListener.Hook();
                    _hotkeyListener.KeyDown += keyboardHook_KeyDown;
                }

                SelectedHotkeyAction = actionType;
            }
            else
            {
                if (_hotkeyListener.HookActive)
                {
                    _hotkeyListener.Unhook();
                    _hotkeyListener.KeyDown -= keyboardHook_KeyDown;
                }

                SelectedHotkeyAction = null;
            }
        }

        private void keyboardHook_KeyDown(object sender, IKeyEventArgs<Key> e)
        {
            //if (e.KeyCode == Key.Escape)
            //{
            //    ToggleKeyboardHook(SelectedHotkeyAction);
            //    return;
            //}

            //if (e.KeyCode == Key.Enter || e.KeyCode == Key.Space)
            //    return;

            //var mod = Key.None;
            //if (e.Alt) mod = Key.Alt;
            //if (e.Shift) mod = Key.Shift;
            //if (e.Control) mod = Key.Control;

            //var key = Keys.None;
            //if (e.KeyValue > 0) key = e.KeyCode;

            //if (key == Keys.LMenu || key == Keys.RMenu ||
            //    key == Keys.LShiftKey || key == Keys.RShiftKey ||
            //    key == Keys.LControlKey || key == Keys.RControlKey)
            //    mod = Keys.None;

            //if (SelectedHotkeyAction == HotkeyAction.Save)
            //{
            //    Settings.SelectedQSKey = key;
            //    Settings.SelectedQSMod = mod;
            //}
            //else if (SelectedHotkeyAction == HotkeyAction.Restore)
            //{
            //    Settings.SelectedQLKey = key;
            //    Settings.SelectedQLMod = mod;
            //}
            //else if (SelectedHotkeyAction == HotkeyAction.Overwrite)
            //{
            //    Settings.SelectedSOKey = key;
            //    Settings.SelectedSOMod = mod;
            //}
        }

        public DelegateCommand<HotkeyAction?> RegisterHotkeyCommand { get; }
        public DelegateCommand NavigateToBackupsCommand { get; }
    }
}
