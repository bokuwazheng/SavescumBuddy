using System;
using System.Collections.ObjectModel;
using System.IO;
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
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;

            // Update the checked state of the menu items.
            var grid = this.skinner as Button;
            foreach (MenuItem mi in grid.ContextMenu.Items)
                mi.IsChecked = mi == item;

            // Get a relative path to the ResourceDictionary which
            // contains the selected skin.
            string skinDictPath = item.Tag as string;
            Uri skinDictUri = new Uri(skinDictPath, UriKind.Relative);

            // Tell the Application to load the skin resources.
            var skinDict = Application.LoadComponent(skinDictUri) as ResourceDictionary;

            Collection<ResourceDictionary> mergedDicts = App.Current.Resources.MergedDictionaries;

            // Remove the existing skin dictionary, if one exists.
            // NOTE: In a real application, this logic might need
            // to be more complex, because there might be dictionaries
            // which should not be removed.
            if (mergedDicts.Count > 0)
                mergedDicts.Clear();

            // Apply the selected skin so that all elements in the
            // application will honor the new look and feel.
            mergedDicts.Add(skinDict);
        }
    }
}
