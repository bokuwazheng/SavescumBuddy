using Moq;
using SavescumBuddy.Services.Interfaces;
using SavescumBuddy.Services;
using System;
using System.Collections.Generic;
using System.Text;

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

            _dataAccess = new SqliteDataAccess(new SqliteDbService("Data Source=.\\DB.db;Version=3;"));
        }
    }
}
