using System.Windows;
using Common;
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
            _klistener = new GlobalKeyboardHook();
        }

        private GlobalKeyboardHook _klistener;

        private void _klistener_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == Properties.Settings.Default.QSKey)
            {
                if ((int)e.Modifiers == Properties.Settings.Default.QSMod)
                {
                    try
                    {
                        BackupRepository.Current.Add();
                        Util.PlaySound(WavLocator.backup_cue);
                    }
                    catch
                    {
                        Util.PopUp("Failed to create a backup: no game is set as current yet.");
                    }

                    e.Handled = true;
                }
            }

            if (e.KeyValue == Properties.Settings.Default.QLKey)
            {
                if ((int)e.Modifiers == Properties.Settings.Default.QLMod)
                {
                    try
                    {
                        BackupRepository.Current.RestoreLatest();
                        Util.PlaySound(WavLocator.restore_cue);
                    }
                    catch
                    {
                        Util.PopUp("There is no backup to restore yet.");
                    }

                    e.Handled = true;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.HotkeysOn)
            {
                _klistener.Hook();
                _klistener.KeyDown += _klistener_KeyDown;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _klistener.Unhook();
            _klistener.KeyDown -= _klistener_KeyDown;
        }
    }
}
