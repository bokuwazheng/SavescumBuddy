using Microsoft.Extensions.DependencyInjection;
using SavescumBuddy.Sqlite;
using SavescumBuddy.ViewModels;
using System;
using System.Configuration;
using System.Windows;

namespace SavescumBuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }

        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddSingleton(x => new SqliteDbService(LoadConnectionString()))
                .AddSingleton<SqliteDataAccess>()
                .AddSingleton<AutobackupManager>()
                .AddSingleton<AboutViewModel>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<SettingsViewModel>()
                .AddTransient<BackupFactory>();
        }

        public static T GetService<T>() => ServiceProvider.GetRequiredService<T>();

        private static string LoadConnectionString()
        {
#if DEBUG
            var id = "Debug";
#else
            var id = "Release";
#endif

            var cnnstr = ConfigurationManager.ConnectionStrings[id].ConnectionString;
            return cnnstr.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }
    }
}
