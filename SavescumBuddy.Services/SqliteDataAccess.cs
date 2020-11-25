using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SavescumBuddy.Services
{
    public class SqliteDataAccess : IDataAccess
    {
        private ISqliteDbService _sqlService;

        public SqliteDataAccess(ISqliteDbService sqlService)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        }

        #region Backup table methods
        public int GetTotalNumberOfBackups(IBackupSearchRequest request)
        {
            var whereParams = "";
            var whereClauses = new List<string>();

            var query = "SELECT COUNT(*) FROM [Backup] b {0}";

            if (request.IsLiked.HasValue)
                whereClauses.Add($"b.IsLiked = { request.IsLiked.Value.ToSqliteIntParameter() }");

            if (request.IsAutobackup.HasValue)
                whereClauses.Add($"b.IsAutobackup = { request.IsAutobackup.Value.ToSqliteIntParameter() }");

            if (request.GameId.HasValue && request.GameId.Value != 0)
                whereClauses.Add($"b.GameId = (SELECT g.Id FROM [Game] g WHERE g.Id = { request.GameId })");

            if (request.IsInGoogleDrive.HasValue)
                whereClauses.Add($"b.GoogleDriveId { request.IsInGoogleDrive.Value.ToSqliteStringParameter() }");

            object args = null;

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                whereClauses.Add("(b.Note like @LC or b.Note like @UC)");

                args = new
                {
                    LC = "%" + request.Note.ToLower() + "%",
                    UC = "%" + request.Note.ToUpper() + "%",
                };
            }

            if (whereClauses.Count > 0)
            {
                var sb = new StringBuilder(" WHERE ");
                sb.AppendJoin(" AND ", whereClauses);
                whereParams = sb.ToString();
            }

            var sql = string.Format(query, whereParams);

            return _sqlService.ExecuteScalar<int>(sql, args);
        }

        public List<Backup> SearchBackups(IBackupSearchRequest request)
        {
            var whereParams = "";
            var sortParams = "";
            var whereClauses = new List<string>();
            var sortClauses = new List<string>();

            var order = request.Descending ? "DESC" : "ASC";

            var query = "SELECT * FROM [Backup] b {0} ORDER BY Id {1} {2}";

            if (request.IsLiked.HasValue)
                whereClauses.Add($"b.IsLiked = { request.IsLiked.Value.ToSqliteIntParameter() }");

            if (request.IsAutobackup.HasValue)
                whereClauses.Add($"b.IsAutobackup = { request.IsAutobackup.Value.ToSqliteIntParameter() }");

            if (request.GameId.HasValue && request.GameId.Value != 0)
                whereClauses.Add($"b.GameId = (SELECT g.Id FROM [Game] g WHERE g.Id = { request.GameId })");

            if (request.IsInGoogleDrive.HasValue)
                whereClauses.Add($"b.GoogleDriveId { request.IsInGoogleDrive.Value.ToSqliteStringParameter() }");

            if (request.Limit.HasValue)
                sortClauses.Add($"LIMIT { request.Limit.Value }");

            if (request.Offset.HasValue)
                sortClauses.Add($"OFFSET { request.Offset.Value }");

            object args = null;

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                whereClauses.Add("(b.Note like @LC or b.Note like @UC)");

                args = new
                {
                    LC = $"%{ request.Note.ToLower() }%",
                    UC = $"%{ request.Note.ToUpper() }%",
                };
            }

            if (whereClauses.Count > 0)
            {
                var sb = new StringBuilder(" WHERE ");
                sb.AppendJoin(" AND ", whereClauses);
                whereParams = sb.ToString();
            }

            if (sortClauses.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendJoin(" ", sortClauses);
                sortParams = sb.ToString();
            }

            var sql = string.Format(query, whereParams, order, sortParams);

            return _sqlService.Query<Backup>(sql, args);
        }

        public void SaveBackup(Backup backup)
        {
            _sqlService.Execute("insert into Backup (GameId, TimeStamp, PicturePath, OriginPath, SavefilePath, IsAutobackup) values (@GameId, @TimeStamp, @PicturePath, @OriginPath, @SavefilePath, @IsAutobackup)", backup);
        }

        public void RemoveBackup(Backup backup)
        {
            _sqlService.Execute("delete from Backup where Id = @Id;", new { Id = backup.Id });
        }

        public Backup GetLatestBackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Id from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 0 order by b.Id desc");
        }

        public Backup GetLatestAutobackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Id from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 1 order by b.Id desc");
        }

        public void UpdateNote(Backup backup)
        {
            _sqlService.Execute("update Backup set Note = @Note where Id = @Id;", new { Id = backup.Id, Note = backup.Note });
        }

        public void UpdateIsLiked(Backup backup)
        {
            _sqlService.Execute("update Backup set IsLiked = @IsLiked where Id = @Id;", new { Id = backup.Id, IsLiked = backup.IsLiked });
        }

        public void UpdateGoogleDriveId(Backup backup)
        {
            _sqlService.Execute("update Backup set GoogleDriveId = @GoogleDriveId where Id = @Id;", new { Id = backup.Id, GoogleDriveId = backup.GoogleDriveId });
        }

        public void UpdateFilePaths(Backup backup)
        {
            _sqlService.Execute("update Backup set SavefilePath = @SavefilePath, PicturePath = @PicturePath where Id = @Id;", new { SavefilePath = backup.SavefilePath, PicturePath = backup.PicturePath, Id = backup.Id });
        }
        #endregion

        #region Game table methods
        public List<Game> LoadGames()
        {
            return _sqlService.Query<Game>("select * from Game order by Id asc");
        }

        public int SaveGame(Game game)
        {
            return _sqlService.ExecuteScalar<int>("insert into Game (Title, SavefilePath, BackupFolder) values (@Title, @SavefilePath, @BackupFolder); select last_insert_rowid();", game);
        }

        public void RemoveGame(Game game)
        {
            _sqlService.Execute("delete from Game where Id = @Id;", new { Id = game.Id });
        }

        public Game GetCurrentGame()
        {
            return _sqlService.QueryFirstOrDefault<Game>("select * from Game where IsCurrent = 1");
        }

        public void SetGameAsCurrent(Game game)
        {
            _sqlService.Execute("update Game set IsCurrent = 0 where IsCurrent = 1;");
            _sqlService.Execute("update Game set IsCurrent = @IsCurrent where Id = @Id;", new { Id = game.Id, IsCurrent = 1 });
        }

        public void UpdateGame(Game game)
        {
            _sqlService.Execute("update Game set Title = @Title, SavefilePath = @SavefilePath, BackupFolder = @BackupFolder where Id = @Id;",
                new { Id = game.Id, Title = game.Title, SavefilePath = game.SavefilePath, BackupFolder = game.BackupFolder});
        }

        public Game GetGame(int id)
        {
            return _sqlService.QueryFirstOrDefault<Game>("select * from Game where Id = @Id", new { Id = id });
        }
        #endregion
    }

    public static class SqliteExtensions
    {
        public static int ToSqliteIntParameter(this bool value) => value ? 1 : 0;
        public static string ToSqliteStringParameter(this bool value) => value ? "IS NOT NULL" : "IS NULL";
    }
}
