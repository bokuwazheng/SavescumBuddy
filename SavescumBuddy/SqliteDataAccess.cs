using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System;

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

        public static List<Backup> LoadBackupList()
        {
            return Query<Backup>("select * from Backup order by Id desc");
        }
        #endregion

        #region Backup table sorting
        public static List<Backup> LoadBackupsWithNoteLike(string input)
        {
            return Query<Backup>("select * from Backup where (Note like @lc or Note like @uc) order by Id desc;", new { lc = "%" + input.ToLower() + "%", uc = "%" + input.ToUpper() + "%" });
        }

        public static List<Backup> LoadSortedBackupList()
        {
            var game = GetCurrentGame();

            var isLiked = Properties.Settings.Default.LikedOnly ? "1" : "0";
            var isAutobackup = Properties.Settings.Default.HideAutobackups ? "0" : "1";
            var gameId = Properties.Settings.Default.CurrentOnly && game != null ? $"{ game.Title }" : "";
            var limit = Properties.Settings.Default.LimitTen ? "limit 10" : "";
            var order = Properties.Settings.Default.OrderByDesc ? "desc" : "asc";

            var sql = $"select * from Backup where IsLiked >= @IsLiked and IsAutobackup <= @IsAutobackup and GameId like @GameId order by Id { order } { limit }";
            var args = new
            {
                IsLiked = isLiked,
                IsAutobackup = isAutobackup,
                GameId = "%" + gameId + "%"
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
