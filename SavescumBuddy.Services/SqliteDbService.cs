using Dapper;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace SavescumBuddy.Services
{
    public class SqliteDbService : ISqliteDbService
    {
        public string _connectionString;
        private static readonly object _locker = new object();

        public SqliteDbService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void Execute(string sql, object args = null)
        {
            lock (_locker)
            {
                using IDbConnection cnn = new SQLiteConnection(_connectionString);
                cnn.Execute(sql, args);
            }
        }

        public T ExecuteScalar<T>(string sql, object args = null)
        {
            lock (_locker)
            {
                using IDbConnection cnn = new SQLiteConnection(_connectionString);
                return cnn.ExecuteScalar<T>(sql, args);
            }
        }

        public IDbEntity QueryFirstOrDefault<IDbEntity>(string sql, object args = null)
        {
            lock (_locker)
            {
                using IDbConnection cnn = new SQLiteConnection(_connectionString);
                return cnn.QueryFirstOrDefault<IDbEntity>(sql, args);
            }
        }

        public List<IDbEntity> Query<IDbEntity>(string sql, object args = null)
        {
            lock (_locker)
            {
                using IDbConnection cnn = new SQLiteConnection(_connectionString);
                var output = cnn.Query<IDbEntity>(sql, args);
                return output.AsList();
            }
        }
    }
}
