using Moq;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SavescumBuddy.Data;
using System.IO;

namespace SavescumBuddy.Services.Tests
{
    public class SqliteDataAccessFixture
    {
        //Mock<IDataAccess> _dataAccess;
        //Mock<ISqliteDbService> _sqliteDbService;
        IDataAccess _dataAccess;

        public SqliteDataAccessFixture()
        {
            //_dataAccess = new Mock<IDataAccess>();
            //var sqliteDbService = new Mock<ISqliteDbService>();

            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, "source\\repos\\SavescumBuddy\\SavescumBuddy.Services.Tests\\DB.db;Version=3;");
            _dataAccess = new SqliteDataAccess(new SqliteDbService($"Data Source={ path }"));
        }

        [Fact]
        public void LikedBackupFound()
        {
            //_dataAccess.SaveBackup(new Backup() 
            //{ 
            //    IsLiked = 1, 
            //    GameId = "1",
            //    DateTimeTag = "kyowa",
            //    IsAutobackup = 0,
            //    FilePath = "asdf",
            //    Picture = "dgdfg",
            //    Origin = "fgjk54"
            //});
            var hui = _dataAccess.SearchBackups(new BackupSearchRequest()
            {
                LikedOnly = true,
                Offset = 0,
                Limit = 1,
                CurrentOnly = true,
                Order = "desc"
            });
            Assert.NotNull(hui[0]);
        }
    }
}
