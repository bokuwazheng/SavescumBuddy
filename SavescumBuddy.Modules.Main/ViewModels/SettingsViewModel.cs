using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SavescumBuddy.Core.Enums;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SavescumBuddy.Modules.Main.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
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
            //if (actionType != SelectedHotkeyAction)
            //{
            //    if (!_keyboardHook.HookActive)
            //    {
            //        _keyboardHook.Hook();
            //        _keyboardHook.KeyDown += keyboardHook_KeyDown;
            //    }

            //    SelectedHotkeyAction = actionType;
            //}
            //else
            //{
            //    if (_keyboardHook.HookActive)
            //    {
            //        _keyboardHook.Unhook();
            //        _keyboardHook.KeyDown -= keyboardHook_KeyDown;
            //    }

            //    SelectedHotkeyAction = null;
            //}
        }

        private void keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Escape)
            //{
            //    ToggleKeyboardHook(SelectedHotkeyAction);
            //    return;
            //}

            //if (e.KeyCode == Keys.Enter || e.KeyCode == Key.Space)
            //    return;

            //var mod = Keys.None;
            //if (e.Alt) mod = Keys.Alt;
            //if (e.Shift) mod = Keys.Shift;
            //if (e.Control) mod = Keys.Control;

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
