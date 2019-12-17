using System.Windows;
using System.Windows.Forms;


namespace SavescumBuddy.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : System.Windows.Controls.UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void PageBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(PageBox.Text, "[^0-9]"))
            {
                PageBox.Text = PageBox.Text.Remove(PageBox.Text.Length - 1);
                PageBox.SelectionStart = PageBox.Text.Length;
            }
        }
    }
}
