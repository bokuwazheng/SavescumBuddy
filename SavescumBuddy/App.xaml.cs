using Prism.Ioc;
using SavescumBuddy.Views;
using System.Windows;
using Prism.Modularity;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Services;
using SavescumBuddy.Modules.Main;
using SavescumBuddy.Modules.Overlay;
using System;
using DryIoc;
using Prism.Events;
using SavescumBuddy.Wpf.Events;
using System.Windows.Forms;
using System.Diagnostics;
using Prism.Regions;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Threading;
using SavescumBuddy.Wpf.Extensions;
using SavescumBuddy.Wpf.Services;

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
            ea.GetEvent<ErrorOccuredEvent>().Subscribe(OnErrorOccured, ThreadOption.UIThread);
            ea.GetEvent<HookEnabledChangedEvent>().Subscribe(OnHookEnabledChanged);
            ea.GetEvent<StartProcessRequestedEvent>().Subscribe(OnStartProcessRequested);

            var settings = Container.Resolve<ISettingsAccess>();
            var hotkeysEnabled = settings.HotkeysEnabled;
            if (hotkeysEnabled)
                OnHookEnabledChanged(hotkeysEnabled);

            Task.Run(async () => 
            {
                var gd = Container.Resolve<IGoogleDrive>();
                if (gd.CredentialExists())
                    await gd.AuthorizeAsync(CancellationToken.None);
            });
        }

        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry
                .RegisterInstance<IDataAccess>(new SqliteDataAccess(new SqliteDbService(LoadConnectionString())))
                .RegisterInstance<ISettingsAccess>(new SqliteSettingsAccess(new SqliteDbService(LoadConnectionString())))
                .Register<IOpenFileService, OpenFileService>()
                .Register<IBackupService, BackupService>()
                .RegisterSingleton<IGlobalKeyboardHook, GlobalKeyboardHook>()
                .RegisterSingleton<IGoogleDrive, GoogleDrive>()
                .RegisterSingleton<ISnackbarMessageQueue, SnackbarMessageQueue>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<OverlayModule>();
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

        private void OnErrorOccured(Exception ex)
        {
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.ShowError(ex.Message);
        }

        private void OnStartProcessRequested(string path)
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
                OnErrorOccured(ex);
            }
        }

        private void ApplicationHook_KeyDown(object sender, KeyEventArgs e)
        {
            var settings = Container.Resolve<ISettingsAccess>();
            var ea = Container.Resolve<IEventAggregator>();
            var data = Container.Resolve<IDataAccess>();
            var backuper = Container.Resolve<IBackupService>();

            try
            {
                if (e.KeyValue == settings.BackupKey && (int)e.Modifiers == settings.BackupModifier)
                {
                    var backup = data.CreateBackup(isAutobackup: false);
                    backuper.BackupSavefile(backup);
                    backuper.SaveScreenshot(backup.PicturePath);
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
                        data.DeleteBackup(latest.Id);
                    }

                    var backup = data.CreateBackup(isAutobackup: false);
                    backuper.BackupSavefile(backup);
                    backuper.SaveScreenshot(backup.PicturePath);
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
            var hook = Container.Resolve<IGlobalKeyboardHook>();

            if (isActive && !hook.HookActive)
            {
                hook.Hook();
                hook.KeyDown += ApplicationHook_KeyDown;
            }
            else
            {
                hook.Unhook();
                hook.KeyDown -= ApplicationHook_KeyDown;
            }
        }
    }
}
