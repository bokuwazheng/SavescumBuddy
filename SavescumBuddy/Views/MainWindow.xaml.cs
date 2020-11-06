using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SavescumBuddy.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as MenuItem;

            var grid = this.SkinButton as Button;
            foreach (MenuItem mi in grid.ContextMenu.Items)
                mi.IsChecked = mi == item;

            var skinDictPath = item.Tag as string;
            var skinDictUri = new Uri(skinDictPath, UriKind.Relative);
            var skinDict = Application.LoadComponent(skinDictUri) as ResourceDictionary;

            var mergedDicts = App.Current.Resources.MergedDictionaries;

            if (mergedDicts.Count > 0)
                mergedDicts.Clear();

            mergedDicts.Add(skinDict);
        }
    }
}
