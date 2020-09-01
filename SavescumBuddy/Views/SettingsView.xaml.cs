using SavescumBuddy.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SavescumBuddy.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Disable hotkey recorder.
            var settingsVm = App.GetService<SettingsViewModel>();
            if (settingsVm.HookIsEnabled)
            {
                settingsVm.SaveHookIsEnabled = false;
                settingsVm.RestoreHookIsEnabled = false;
                settingsVm.OverwriteHookIsEnabled = false;
                settingsVm.RegisterHotkeyCommand?.Execute();
            }
        }
    }
}
