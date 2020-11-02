using Prism.Ioc;
using SavescumBuddy.Views;
using System.Windows;
using Prism.Modularity;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Services;
using SavescumBuddy.Modules.Main;
using System;
using DryIoc;
using Prism.Events;
using SavescumBuddy.Core.Events;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace SavescumBuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var ea = Container.Resolve<IEventAggregator>();
            ea.GetEvent<ErrorOccuredEvent>().Subscribe(ex => MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            ea.GetEvent<HookChangedEvent>().Subscribe(OnHookChanged);
        }

        private void OnHookChanged(bool x)
        {
            var hook = Container.Resolve<GlobalKeyboardHook>();

            if (x)
            {
                hook.Hook();
                hook.KeyDown += Hook_KeyDown;
            }
            else
            {
                hook.Unhook();
                hook.KeyDown -= Hook_KeyDown;
            }
        }

        private void Hook_KeyDown(object sender, KeyEventArgs e)
        {
            var mod = Keys.None;
            if (e.Alt) mod = Keys.Alt;
            if (e.Shift) mod = Keys.Shift;
            if (e.Control) mod = Keys.Control;

            var key = Keys.None;
            if (e.KeyValue > 0) key = e.KeyCode;

            if (key == Keys.LMenu || key == Keys.RMenu ||
                key == Keys.LShiftKey || key == Keys.RShiftKey ||
                key == Keys.LControlKey || key == Keys.RControlKey)
                mod = Keys.None;

            var ea = Container.Resolve<IEventAggregator>();
            ea.GetEvent<HookKeyDownEvent>().Publish(((int)key, (int)mod));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry
                .RegisterInstance<IDataAccess>(new SqliteDataAccess(new SqliteDbService(LoadConnectionString())))
                .RegisterInstance<ISettingsAccess>(new SqliteSettingsAccess(new SqliteDbService(LoadConnectionString())))
                .Register<IOpenFileService, OpenFileService>()
                .Register<IBackupService, BackupService>()
                .Register<IBackupFactory, BackupFactory>()
                .RegisterSingleton<GlobalKeyboardHook>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
        }

        private string LoadConnectionString()
        {
#if DEBUG
            var id = "Data Source=.\\DB.db;Version=3;";
#else
            var id = "Data Source=%APPDATA%\\bokuwazheng\\DB.db;Version=3;";
#endif
            return id.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }
    }
}
