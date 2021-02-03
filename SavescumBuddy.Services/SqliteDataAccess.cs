using SavescumBuddy.Lib;
using SavescumBuddy.Lib.Enums;
using SavescumBuddy.Lib.Extensions;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<string> whereClauses = new();
            List<string> sortClauses = new();

            var order = request.Descending ? "DESC" : "ASC";

            var query = @"
                SELECT 
                    Backup.Id,
                    GameId,
                    Game.Title AS GameTitle,
                    iif(GoogleDrive.Id IS NULL, 0, 1) AS IsInGoogleDrive,
                    Note,
                    IsLiked,
                    IsScheduled,
                    TimeStamp,
                    Game.SavefilePath AS OriginPath,
                    Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                    Game.BackupFolder || '\' || TimeStamp || '.jpg' AS PicturePath
                FROM Backup 
                LEFT JOIN Game ON Backup.GameId = Game.Id
                LEFT JOIN GoogleDrive ON Backup.Id = GoogleDrive.BackupId
                {0} ORDER BY Backup.Id {1} {2}";

            if (request.Id.HasValue)
                whereClauses.Add($"Backup.Id = { request.Id.Value }");

            if (request.IsLiked.HasValue)
                whereClauses.Add($"IsLiked = { request.IsLiked.Value.ToSqliteBoolean() }");

            if (request.IsScheduled.HasValue)
                whereClauses.Add($"IsScheduled = { request.IsScheduled.Value.ToSqliteBoolean() }");

            if (request.CurrentGame)
                whereClauses.Add($"GameId = (SELECT Id FROM Game WHERE IsCurrent = 1)");

            if (request is { CurrentGame: false, GameId: > 0 })
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
                StringBuilder sb = new(" WHERE ");
                sb.AppendJoin(" AND ", whereClauses);
                whereParams = sb.ToString();
            }

            if (sortClauses.Count > 0)
            {
                StringBuilder sb = new();
                sb.AppendJoin(" ", sortClauses);
                sortParams = sb.ToString();
            }

            var sql = string.Format(query, whereParams, order, sortParams);
            var backups = _sqlService.Query<Backup>(sql, args);

            query = "SELECT COUNT(*) FROM Backup {0}";
            sql = string.Format(query, whereParams);
            var count = _sqlService.ExecuteScalar<int>(sql, args);

            return new()
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
                    IsScheduled,
                    TimeStamp,
                    Game.SavefilePath AS OriginPath,
                    Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                    Game.BackupFolder || '\' || TimeStamp || '.jpg' AS PicturePath
                FROM Backup 
                LEFT JOIN Game ON Backup.GameId = Game.Id
                WHERE Backup.Id = @Id";

            return _sqlService.QueryFirstOrDefault<Backup>(sql, new { Id = id });
        }

        public Backup CreateBackup(bool isScheduled)
        {
            object args = new
            {
                TimeStamp = DateTime.Now.Ticks,
                IsScheduled = isScheduled.ToSqliteBoolean()
            };

            var query = @"
                INSERT INTO Backup 
                    (GameId, TimeStamp, IsScheduled) 
                    values 
                    ((SELECT Id FROM Game WHERE IsCurrent = 1), @TimeStamp, @IsScheduled);
                SELECT last_insert_rowid();";

            var id = _sqlService.ExecuteScalar<int>(query, args);

            return SearchBackups(new BackupSearchRequest() { Id = id }).Backups.First();
        }

        public void DeleteBackup(int id)
        {
            _sqlService.Execute("DELETE FROM Backup WHERE Id = @Id;", new { Id = id });
        }

        public Backup GetLatestBackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>(@"
                SELECT 
                Backup.Id,
                Game.BackupFolder || '\' || TimeStamp AS SavefilePath,
                Game.SavefilePath AS OriginPath
                FROM Backup
                LEFT JOIN Game ON Backup.GameId = Game.Id
                WHERE GameId = (SELECT Id FROM Game WHERE IsCurrent = 1)
                AND 
                IsScheduled = 0 
                ORDER BY Backup.Id DESC");
        }

        public bool ScheduledBackupMustBeSkipped()
        {
            var sql = @"
                SELECT 
                TimeStamp
                FROM Backup 
                WHERE 
                GameId = (SELECT Id FROM Game WHERE IsCurrent = 1) 
                AND 
                IsScheduled = 0 
                ORDER BY Id DESC
                LIMIT 1";

            var timeStamp = DateTime.Now - new DateTime(_sqlService.ExecuteScalar<long>(sql));

            sql = "SELECT SchedulerSkipType FROM Settings WHERE Id = 1;";
            var skipOption = (SkipOption)_sqlService.ExecuteScalar<int>(sql);

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
            
            var sql = "SELECT SchedulerOverwriteType FROM Settings WHERE Id = 1;";
            var overwriteOption = (OverwriteOption)_sqlService.ExecuteScalar<int>(sql);

            sql = @"
                SELECT
                Id,
                IsLiked
                FROM Backup 
                WHERE 
                GameId = (SELECT Id from Game WHERE IsCurrent = 1) 
                AND 
                IsScheduled = 1 
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
        #endregion

        #region Game table methods
        public List<Game> GetGames()
        {
            return _sqlService.Query<Game>("SELECT * FROM Game ORDER BY Id ASC");
        }

        public Game GetGame(int id)
        {
            return _sqlService.QueryFirstOrDefault<Game>("SELECT * FROM Game WHERE Id = @Id", new { Id = id });
        }

        public void CreateGame(Game game)
        {
            _sqlService.Execute("INSERT INTO Game (Title, SavefilePath, BackupFolder) values (@Title, @SavefilePath, @BackupFolder);", game);
        }

        public void DeleteGame(Game game)
        {
            _sqlService.Execute("DELETE FROM Game WHERE Id = @Id;", new { Id = game.Id });
        }

        public void SetGameAsCurrent(int id)
        {
            var sql = @"
                UPDATE Game SET IsCurrent = 0 WHERE IsCurrent = 1;
                UPDATE Game SET IsCurrent = 1 WHERE Id = @Id;";

            _sqlService.Execute(sql, new { Id = id });
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

        public void SaveGoogleDriveInfo(int backupId, string savefileId, string pictureId)
        {
            var args = new
            {
                BackupId = backupId,
                Savefile = savefileId,
                Picture = pictureId
            };

            var sql = @"
                INSERT INTO GoogleDrive
                (BackupId, Savefile, Picture)
                VALUES
                (@BackupId, @Savefile, @Picture);";

            _sqlService.Execute(sql, args);
        }

        public (string SavefileId, string PictureId) GetGoogleDriveInfo(int backupId)
        {
            var sql = @"
                SELECT
                Savefile,
                Picture
                FROM GoogleDrive 
                WHERE BackupId = @BackupId;";

            return _sqlService.QueryFirstOrDefault<(string SavefileId, string PictureId)>(sql, new { BackupId = backupId });
        }

        public void DeleteGoogleDriveInfo(int backupId)
        {
            _sqlService.Execute("DELETE FROM GoogleDrive WHERE BackupId = @BackupId;", new { BackupId = backupId });
        }
    }
}
