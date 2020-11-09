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
using System.Diagnostics;

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
            ea.GetEvent<ErrorOccuredEvent>().Subscribe(ex => MessageBox.Show(Current.MainWindow, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            ea.GetEvent<HookChangedEvent>().Subscribe(OnHookChanged);
            ea.GetEvent<HookEnabledChangedEvent>().Subscribe(OnHookEnabledChanged);
            ea.GetEvent<ExecuteRequestedEvent>().Subscribe(OnExecuteRequested);

            var settings = Container.Resolve<ISettingsAccess>();
            var hotkeysEnabled = settings.HotkeysEnabled;
            if (hotkeysEnabled)
            {
                var hook = Container.Resolve<GlobalKeyboardHook>("Application");
                hook.Hook();
                hook.KeyDown += ApplicationHook_KeyDown;
            }
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
                .RegisterInstance(new GlobalKeyboardHook(), "Settings")
                .RegisterInstance(new GlobalKeyboardHook(), "Application")
                .RegisterSingleton<IGoogleDrive, GoogleDrive>();
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

        private void OnExecuteRequested(string path)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo(path)
                    {
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Current.MainWindow, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplicationHook_KeyDown(object sender, KeyEventArgs e)
        {
            var settings = Container.Resolve<ISettingsAccess>();
            var ea = Container.Resolve<IEventAggregator>();
            var factory = Container.Resolve<IBackupFactory>();
            var data = Container.Resolve<IDataAccess>();
            var backuper = Container.Resolve<IBackupService>();

            try
            {
                if (e.KeyValue == settings.BackupKey && (int)e.Modifiers == settings.BackupModifier)
                {
                    var backup = factory.CreateBackup();
                    backuper.BackupSavefile(backup);
                    backuper.SaveScreenshot(backup.PicturePath);
                    data.SaveBackup(backup);
                    ea.GetEvent<BackupListUpdateRequestedEvent>().Publish();
                    return;
                }

                if (e.KeyValue == settings.RestoreKey && (int)e.Modifiers == settings.RestoreModifier)
                {
                    var backup = data.GetLatestBackup();
                    if (backup is object)
                        backuper.RestoreBackup(backup);
                    return;
                }

                if (e.KeyValue == settings.OverwriteKey && (int)e.Modifiers == settings.OverwriteModifier)
                {
                    var latest = data.GetLatestBackup();
                    if (latest is object)
                    {
                        backuper.DeleteFiles(latest);
                        data.RemoveBackup(latest);
                    }
                    var backup = factory.CreateBackup();
                    backuper.BackupSavefile(backup);
                    backuper.SaveScreenshot(backup.PicturePath);
                    data.SaveBackup(backup);
                    ea.GetEvent<BackupListUpdateRequestedEvent>().Publish();
                }
            }
            catch (Exception ex)
            {
                ea.GetEvent<ErrorOccuredEvent>().Publish(ex);
            }
        }

        private void OnHookEnabledChanged(bool isActive)
        {
            var hook = Container.Resolve<GlobalKeyboardHook>("Application");

            if (isActive)
            {
                hook.Hook();
                hook.KeyDown += ApplicationHook_KeyDown;
            }
            else
            {
                hook.Hook();
                hook.KeyDown -= ApplicationHook_KeyDown;
            }
        }

        private void OnHookChanged(bool isActive)
        {
            var hook = Container.Resolve<GlobalKeyboardHook>("Settings");

            if (isActive)
            {
                hook.Hook();
                hook.KeyDown += SettingsHook_KeyDown;
            }
            else
            {
                hook.Unhook();
                hook.KeyDown -= SettingsHook_KeyDown;
            }
        }

        private void SettingsHook_KeyDown(object sender, KeyEventArgs e)
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
    }
}
