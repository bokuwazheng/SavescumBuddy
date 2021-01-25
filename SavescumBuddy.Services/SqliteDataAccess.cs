using SavescumBuddy.Lib;
using SavescumBuddy.Lib.Enums;
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
        public BackupSearchResponse SearchBackups(IBackupSearchRequest request)
        {
            var whereParams = "";
            var sortParams = "";
            var whereClauses = new List<string>();
            var sortClauses = new List<string>();

            var order = request.Descending ? "DESC" : "ASC";

            var query = @"
                SELECT 
                    Backup.Id,
                    GameId,
                    Game.Title AS GameTitle,
                    GoogleDriveId,
                    Note,
                    IsLiked,
                    IsAutobackup,
                    TimeStamp,
                    Game.SavefilePath AS OriginPath,
                    Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                    Game.BackupFolder || '\' || TimeStamp || '.jpg' AS PicturePath
                FROM Backup 
                LEFT JOIN Game ON Backup.GameId = Game.Id
                {0} ORDER BY Backup.Id {1} {2}";

            if (request.IsLiked.HasValue)
                whereClauses.Add($"IsLiked = { request.IsLiked.Value.ToSqliteBoolean() }");

            if (request.IsAutobackup.HasValue)
                whereClauses.Add($"IsAutobackup = { request.IsAutobackup.Value.ToSqliteBoolean() }");

            if (request.GameId != 0)
                whereClauses.Add($"GameId = { request.GameId }");

            if (request.IsInGoogleDrive.HasValue)
                whereClauses.Add($"GoogleDriveId { request.IsInGoogleDrive.Value.ToSqliteNullCheck() }");

            if (request.Limit.HasValue)
                sortClauses.Add($"LIMIT { request.Limit.Value }");

            if (request.Offset.HasValue)
                sortClauses.Add($"OFFSET { request.Offset.Value }");

            object args = null;

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                whereClauses.Add("(Note like @LC or Note like @UC)");

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
            var backups = _sqlService.Query<Backup>(sql, args);

            query = "SELECT COUNT(*) FROM Backup {0}";
            sql = string.Format(query, whereParams);
            var count = _sqlService.ExecuteScalar<int>(sql, args);

            return new BackupSearchResponse()
            {
                Backups = backups,
                TotalCount = count
            };
        }

        public Backup GetBackup(int id)
        {
            var sql = @"
                SELECT 
                    Backup.Id,
                    GameId,
                    Game.Title AS GameTitle,
                    GoogleDriveId,
                    Note,
                    IsLiked,
                    IsAutobackup,
                    TimeStamp,
                    Game.SavefilePath AS OriginPath,
                    Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                    Game.BackupFolder || '\' || TimeStamp || '.jpg' AS PicturePath
                FROM Backup 
                LEFT JOIN Game ON Backup.GameId = Game.Id
                WHERE Backup.Id = @Id";

            return _sqlService.QueryFirstOrDefault<Backup>(sql, new { Id = id });
        }

        public Backup CreateBackup(bool isAutobackup)
        {
            object args = new
            {
                TimeStamp = DateTime.Now.Ticks,
                IsAutobackup = isAutobackup.ToSqliteBoolean()
            };

            var query = @"
                INSERT INTO Backup 
                    (GameId, TimeStamp, IsAutobackup) 
                    values 
                    ((SELECT Id FROM Game WHERE IsCurrent = 1), @TimeStamp, @IsAutobackup);
                SELECT last_insert_rowid();";

            var id = _sqlService.ExecuteScalar<int>(query, args);

            return GetBackup(id);
        }

        public void DeleteBackup(int id)
        {
            _sqlService.Execute("DELETE FROM Backup WHERE Id = @Id;", new { Id = id });
        }

        public Backup GetLatestBackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Id from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 0 order by b.Id desc");
        }

        public Backup GetLatestAutobackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Id from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 1 order by b.Id desc");
        }

        public bool ScheduledBackupMustBeSkipped()
        {
            var sql = @"
                SELECT 
                TimeStamp
                FROM Backup 
                WHERE 
                GameId = (SELECT Id from Game WHERE IsCurrent = 1) 
                AND 
                IsAutobackup = 0 
                ORDER BY Id DESC
                LIMIT 1";

            var timeStamp = DateTime.Now - new DateTime(_sqlService.ExecuteScalar<long>(sql));

            sql = "SELECT AutobackupSkipType FROM Settings WHERE Id = 1;";
            var skipOption = (SkipOption)_sqlService.ExecuteScalar<long>(sql);

            return skipOption switch
            {
                SkipOption.Never => false,
                SkipOption.FiveMin => timeStamp > TimeSpan.FromMinutes(5d),
                SkipOption.TenMin => timeStamp > TimeSpan.FromMinutes(10d),
                _ => throw new NotImplementedException("Unsupported skip option"),
            };
        }

        public void OverwriteScheduledBackup(Action<Backup> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            
            var sql = "SELECT AutobackupOverwriteType FROM Settings WHERE Id = 1;";
            var overwriteOption = (OverwriteOption)_sqlService.ExecuteScalar<long>(sql);

            sql = @"
                SELECT
                Id,
                IsLiked
                FROM Backup 
                WHERE 
                GameId = (SELECT Id from Game WHERE IsCurrent = 1) 
                AND 
                IsAutobackup = 1 
                ORDER BY Id DESC
                LIMIT 1";

            var latestScheduledBackup = _sqlService.QueryFirstOrDefault<Backup>(sql);

            var mustBeDeleted = overwriteOption switch
            {
                OverwriteOption.Always => true,
                OverwriteOption.Never => false,
                OverwriteOption.KeepLiked => latestScheduledBackup.IsLiked.SqliteToBoolean(),
                _ => throw new NotImplementedException("Unsupported overwrite option"),
            };

            if (mustBeDeleted)
            {
                sql = @"
                    SELECT
                    Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                    Game.BackupFolder || '\' || TimeStamp || '.jpg' AS PicturePath
                    FROM Backup
                    LEFT JOIN Game ON Backup.GameId = Game.Id
                    WHERE 
                    Backup.Id = @Id;

                    DELETE FROM Backup WHERE Id = @Id;";

                var backup =_sqlService.QueryFirstOrDefault<Backup>(sql, new { Id = latestScheduledBackup.Id });
                action(backup);
            }
        }

        public void UpdateNote(Backup backup)
        {
            _sqlService.Execute("UPDATE Backup SET Note = @Note WHERE Id = @Id;", new { Id = backup.Id, Note = backup.Note });
        }

        public void UpdateIsLiked(Backup backup)
        {
            _sqlService.Execute("UPDATE Backup SET IsLiked = @IsLiked WHERE Id = @Id;", new { Id = backup.Id, IsLiked = backup.IsLiked });
        }

        public void UpdateGoogleDriveId(Backup backup)
        {
            _sqlService.Execute("UPDATE Backup SET GoogleDriveId = @GoogleDriveId WHERE Id = @Id;", new { Id = backup.Id, GoogleDriveId = backup.GoogleDriveId });
        }
        #endregion

        #region Game table methods
        public List<Game> GetGames()
        {
            return _sqlService.Query<Game>("SELECT * FROM Game ORDER BY Id ASC");
        }

        public void CreateGame(Game game)
        {
            _sqlService.Execute("INSERT INTO Game (Title, SavefilePath, BackupFolder) values (@Title, @SavefilePath, @BackupFolder);", game);
        }

        public void DeleteGame(Game game)
        {
            _sqlService.Execute("delete from Game where Id = @Id;", new { Id = game.Id });
        }

        public void SetGameAsCurrent(Game game)
        {
            var sql = @"
                UPDATE Game SET IsCurrent = 0 WHERE IsCurrent = 1;
                UPDATE Game SET IsCurrent = 1 WHERE Id = @Id;";

            _sqlService.Execute(sql, new { Id = game.Id });
        }

        public void UpdateGame(Game game)
        {
            var sql = @"
                UPDATE Game SET 
                    Title = @Title, 
                    SavefilePath = @SavefilePath, 
                    BackupFolder = @BackupFolder 
                WHERE Id = @Id;";

            _sqlService.Execute(sql, new { Id = game.Id, Title = game.Title, SavefilePath = game.SavefilePath, BackupFolder = game.BackupFolder});
        }
        #endregion
    }

    public static class SqliteExtensions
    {
        public static int ToSqliteBoolean(this bool value) => value ? 1 : 0;
        public static string ToSqliteNullCheck(this bool value) => value ? "IS NOT NULL" : "IS NULL";
        public static bool SqliteToBoolean(this int value) => value == 0 ? false : value == 1 ? true : throw new InvalidOperationException("Only 0 and 1 can be converted to bool");
    }
}
