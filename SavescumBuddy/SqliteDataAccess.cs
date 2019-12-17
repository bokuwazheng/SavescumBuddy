using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System;
using System.Threading.Tasks;
using Settings = SavescumBuddy.Properties.Settings;

namespace SavescumBuddy
{
    interface IDbEntity { }

    class SqliteDataAccess
    {
        private static string LoadConnectionString(string id = "Debug") // string id = "Debug" for debugging, otherwise "Default"
        {
            var cnnstr = ConfigurationManager.ConnectionStrings[id].ConnectionString;
            return cnnstr.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)); 
        }

        private static readonly object _locker = new object();

        private static void Execute(string sql, object args = null)
        {
            lock (_locker)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute(sql, args);
                }
            }
        }

        private static IDbEntity QueryFirstOrDefault<IDbEntity>(string sql, object args = null)
        {
            lock (_locker)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.QueryFirstOrDefault<IDbEntity>(sql, args);
                    return output;
                }
            }
        }

        private static List<IDbEntity> Query<IDbEntity>(string sql, object args = null)
        {
            lock (_locker)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<IDbEntity>(sql, args);
                    return output.ToList();
                }
            }
        }

        private static async Task<List<IDbEntity>> QueryAsync<IDbEntity>(string sql, object args = null)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<IDbEntity>(sql, args);
                return output.ToList();
            }
        }

        #region Backup table
        public static void SaveBackup(Backup backup)
        {
            Execute("insert into Backup (GameId, DateTimeTag, Picture, Origin, FilePath, IsAutobackup) values (@GameId, @DateTimeTag, @Picture, @Origin, @FilePath, @IsAutobackup)", backup);
        }

        public static void RemoveBackup(Backup backup)
        {
            Execute("delete from Backup where Id = @Id;", new { Id = backup.Id });
        }

        public static Backup GetLatestBackup()
        {
            return QueryFirstOrDefault<Backup>("select * from Backup where GameId = @GameId and IsAutobackup = 0 order by Id desc", new { GameId = GetCurrentGame().Title });
        }

        public static Backup GetLatestAutobackup()
        {
            return QueryFirstOrDefault<Backup>("select * from Backup where IsAutobackup = 1 order by Id desc");
        }

        public static void UpdateNote(Backup backup)
        {
            Execute("update Backup set Note = @Note where Id = @Id;", new { Id = backup.Id, Note = backup.Note });
        }

        public static void UpdateIsLiked(Backup backup)
        {
            Execute("update Backup set IsLiked = @IsLiked where Id = @Id;", new { Id = backup.Id, IsLiked = backup.IsLiked });
        }

        public static void UpdateInCloud(Backup backup)
        {
            Execute("update Backup set InCloud = @InCloud where Id = @Id;", new { Id = backup.Id, InCloud = backup.InCloud });
        }

        public static List<Backup> LoadBackupList()
        {
            return Query<Backup>("select * from Backup order by Id desc");
        }
        #endregion

        #region Backup table sorting
        public static List<Backup> LoadBackupsWithNoteLike(string input, string offset)
        {
            var order = Settings.Default.OrderByDesc ? "desc" : "asc";
            var limit = Settings.Default.BackupsPerPage.ToString();

            var sql = "";
            if (Settings.Default.CurrentOnly)
            {
                sql = $"select * from [Backup] b where (b.Note like @lc or b.Note like @uc) and b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) order by b.Id { order } limit { limit } offset { offset }";
            }
            else
            {
                sql = $"select * from Backup where (Note like @lc or Note like @uc) order by Id { order } limit { limit } offset { offset }";
            }
                
            var args = new
            {
                lc = "%" + input.ToLower() + "%",
                uc = "%" + input.ToUpper() + "%"
            };

            return Query<Backup>(sql, args);
        }

        public static List<Backup> LoadBackups(string offset)
        {
            var isLiked = Settings.Default.LikedOnly ? "1" : "0";
            var isAutobackup = Settings.Default.HideAutobackups ? "0" : "1";
            var order = Settings.Default.OrderByDesc ? "desc" : "asc";
            var limit = Settings.Default.BackupsPerPage.ToString();

            var sql = "";
            if (Settings.Default.CurrentOnly)
            {
                sql = $"select * from [Backup] b where b.IsLiked >= @IsLiked and b.IsAutobackup <= @IsAutobackup and b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) order by Id { order } limit { limit } offset { offset }";
            }
            else
            {
                sql = $"select * from Backup where IsLiked >= @IsLiked and IsAutobackup <= @IsAutobackup order by Id { order } limit { limit } offset { offset }";
            }
            var args = new
            {
                IsLiked = isLiked,
                IsAutobackup = isAutobackup
            };

            return Query<Backup>(sql, args);
        }
        #endregion

        #region Game table
        public static List<Game> LoadGames()
        {
            return Query<Game>("select * from Game order by Id asc");
        }

        public static void SaveGame(Game game)
        {
            Execute("insert into Game (Title, SavefilePath, BackupFolder) values (@Title, @SavefilePath, @BackupFolder)", game);
        }

        public static void RemoveGame(Game game)
        {
            Execute("delete from Game where Id = @Id;", new { Id = game.Id });
        }

        public static Game GetCurrentGame()
        {
            return QueryFirstOrDefault<Game>("select * from Game where IsCurrent = 1");
        }

        public static void SetGameAsCurrent(Game game)
        {
            Execute("update Game set IsCurrent = 0;");
            Execute("update Game set IsCurrent = @IsCurrent where Id = @Id;", new { Id = game.Id, IsCurrent = 1 });
        }

        public static void UpdateGame(Game game)
        {
            Execute("update Game set Title = @Title, SavefilePath = @SavefilePath, BackupFolder = @BackupFolder, CanBeSetCurrent = @CanBeSetCurrent where Id = @Id;",
                new { Id = game.Id, Title = game.Title, SavefilePath = game.SavefilePath, BackupFolder = game.BackupFolder, CanBeSetCurrent = 1 });
        }
        #endregion
    }
}
