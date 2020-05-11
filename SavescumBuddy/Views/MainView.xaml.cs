using SavescumBuddy.ViewModels;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void MainViewLoaded(object sender, RoutedEventArgs e)
        {
            var dc = Application.Current.MainWindow.DataContext;
            var appVm = dc as ApplicationViewModel;
            
            // Enable keyboard listener.
            appVm.Hook();

            // Update backup list.
            var mainVm = appVm.ViewModels.OfType<MainViewModel>().First();
            mainVm.UpdateBackupList();
            mainVm.NavigateToStartCommand?.Execute();
        }

        private void MainViewUnloaded(object sender, RoutedEventArgs e)
        {
            var dc = Application.Current.MainWindow.DataContext;
            var appVm = dc as ApplicationViewModel;

            // Disable keyboard listener.
            appVm.Unhook();

            // Save filtering settings.
            Settings.Default.Save();
        }
    }
}
