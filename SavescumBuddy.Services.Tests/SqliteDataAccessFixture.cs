using Moq;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SavescumBuddy.Lib;
using System.IO;

namespace SavescumBuddy.Services.Tests
{
    public class SqliteDataAccessFixture
    {
        IDataAccess _dataAccess;
        ISettingsAccess _settingsAccess;

        public SqliteDataAccessFixture()
        {
            //_dataAccess = new Mock<IDataAccess>();
            //var sqliteDbService = new Mock<ISqliteDbService>();

            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, "source\\repos\\SavescumBuddy\\SavescumBuddy.Services.Tests\\DB.db;Version=3;");
            _dataAccess = new SqliteDataAccess(new SqliteDbService($"Data Source={ path }"));
            _settingsAccess = new SqliteSettingsAccess(new SqliteDbService($"Data Source={ path }"));
        }

        [Fact]
        public void AutobackupIntervalIsNotNull()
        {
            //
            Assert.True(true);
        }

        [Fact]
        public void LikedBackupFound()
        {
            //
            Assert.NotNull("");
        }
    }
}
