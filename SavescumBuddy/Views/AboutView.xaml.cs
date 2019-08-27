using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;

namespace SavescumBuddy.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (System.Exception ex)
            {
                Util.PopUp($"Exception message: { ex.Message }");
            }
        }
    }
}
