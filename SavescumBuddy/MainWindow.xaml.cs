using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace SavescumBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var token = GoogleDrive.GetToken(GoogleDrive.CurrentMode);

            try
            {
                if (Directory.GetFiles(token).Length > 0)
                {
                    Task.Run(async() =>
                    {
                        await GoogleDrive.Current.AuthorizeAsync();
                    });
                }
            }
            catch (System.Exception) { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
