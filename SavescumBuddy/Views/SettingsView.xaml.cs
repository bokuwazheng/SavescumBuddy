using SavescumBuddy.ViewModels;
using System.Linq;
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
            var dc = Application.Current.MainWindow.DataContext;
            var appVm = dc as ApplicationViewModel;
            var settingsVm = appVm.GetViewModel<SettingsViewModel>();
            if (settingsVm.SelectedHotkeyAction != null)
                settingsVm.RegisterHotkeyCommand?.Execute(settingsVm.SelectedHotkeyAction);
        }
    }
}
