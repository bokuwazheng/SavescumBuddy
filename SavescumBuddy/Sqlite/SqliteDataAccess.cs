using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace SavescumBuddy.Sqlite
{
    interface IDbEntity { }

    class SqliteDataAccess
    {
#if DEBUG
        private static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Debug"].ConnectionString;
        }
#else
        private static string LoadConnectionString()
        {
            var cnnstr = ConfigurationManager.ConnectionStrings["Release"].ConnectionString;
            return cnnstr.Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }
#endif

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

        private static T ExecuteScalar<T>(string sql, object args = null)
        {
            lock (_locker)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    return cnn.ExecuteScalar<T>(sql, args);
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
        public static int GetTotalNumberOfBackups(BackupSearchRequest request)
        {
            var isLiked = request.LikedOnly ? "1" : "0";
            var isAutobackup = request.HideAutobackups ? "0" : "1";
            var note = request.Note;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("select count(*) from [Backup] b where ");
            if (!string.IsNullOrWhiteSpace(note))
                queryBuilder.Append("(b.Note like @LC or b.Note like @UC) and ");
            queryBuilder.Append("b.IsLiked >= @IsLiked and b.IsAutobackup <= @IsAutobackup");
            if (request.CurrentOnly)
                queryBuilder.Append(" and b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1)");

            var sql = queryBuilder.ToString();

            object args;

            if (string.IsNullOrWhiteSpace(note))
            {
                args = new
                {
                    IsLiked = isLiked,
                    IsAutobackup = isAutobackup
                };
            }
            else
            {
                args = new
                {
                    LC = "%" + note.ToLower() + "%",
                    UC = "%" + note.ToUpper() + "%",
                    IsLiked = isLiked,
                    IsAutobackup = isAutobackup
                };
            }

            return ExecuteScalar<int>(sql, args);
        }

        public static List<Backup> SearchBackups(BackupSearchRequest request)
        {
            var isLiked = request.LikedOnly ? "1" : "0";
            var isAutobackup = request.HideAutobackups ? "0" : "1";
            var order = request.Order;
            var limit = request.Limit;
            var offset = request.Offset;
            var note = request.Note;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("select * from [Backup] b where ");
            if (!string.IsNullOrWhiteSpace(note))
                queryBuilder.Append("(b.Note like @LC or b.Note like @UC) and ");
            queryBuilder.Append("b.IsLiked >= @IsLiked and b.IsAutobackup <= @IsAutobackup ");
            if (request.CurrentOnly)
                queryBuilder.Append("and b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) ");
            queryBuilder.Append($"order by b.Id { order } limit { limit } offset { offset }");

            var sql = queryBuilder.ToString();

            object args;

            if (string.IsNullOrWhiteSpace(note))
            {
                args = new
                {
                    IsLiked = isLiked,
                    IsAutobackup = isAutobackup
                };
            }
            else
            {
                args = new
                {
                    LC = "%" + note.ToLower() + "%",
                    UC = "%" + note.ToUpper() + "%",
                    IsLiked = isLiked,
                    IsAutobackup = isAutobackup
                };
            }

            return Query<Backup>(sql, args);
        }

        public static void SaveBackup(Backup backup)
        {
            Execute("insert into Backup (GameId, DateTimeTag, Picture, Origin, FilePath, IsAutobackup) values (@GameId, @DateTimeTag, @Picture, @Origin, @FilePath, @IsAutobackup)", backup);
        }

        public static void RemoveBackup(Backup backup)
        {
            Execute("delete from Backup where Id = @Id;", new { Id = backup.Id });
        }

        public static Backup GetBackup(Backup backup)
        {
            return QueryFirstOrDefault<Backup>($"select * from Backup where Id = @Id;", new { Id = backup.Id });
        }

        public static Backup GetLatestBackup()
        {
            return QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 0 order by b.Id desc");
        }

        public static Backup GetLatestAutobackup()
        {
            return QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 1 order by b.Id desc");
        }

        public static void UpdateNote(Backup backup)
        {
            Execute("update Backup set Note = @Note where Id = @Id;", new { Id = backup.Id, Note = backup.Note });
        }

        public static void UpdateIsLiked(Backup backup)
        {
            Execute("update Backup set IsLiked = @IsLiked where Id = @Id;", new { Id = backup.Id, IsLiked = backup.IsLiked });
        }

        public static void UpdateDriveId(Backup backup)
        {
            Execute("update Backup set DriveId = @DriveId where Id = @Id;", new { Id = backup.Id, DriveId = backup.DriveId });
        }

        public static void UpdateFilePaths(Backup backup)
        {
            Execute("update Backup set FilePath = @FilePath, Picture = @Picture where Id = @Id;", new { FilePath = backup.FilePath, Picture = backup.Picture, Id = backup.Id });
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