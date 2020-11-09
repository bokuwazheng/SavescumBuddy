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

            return _sqlService.ExecuteScalar<int>(sql, args);
        }

        public List<Backup> SearchBackups(IBackupSearchRequest request)
        {
            var isLiked = request.LikedOnly ? "1" : "0";
            var isAutobackup = request.HideAutobackups ? "0" : "1";
            var note = request.Note;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("select * from [Backup] b where ");
            if (!string.IsNullOrWhiteSpace(note))
                queryBuilder.Append("(b.Note like @LC or b.Note like @UC) and ");
            queryBuilder.Append("b.IsLiked >= @IsLiked and b.IsAutobackup <= @IsAutobackup ");
            if (request.CurrentOnly)
                queryBuilder.Append("and b.GameId = (select g.Id from [Game] g where g.IsCurrent = 1) ");
            queryBuilder.Append($"order by b.Id { request.Order } limit { request.Limit } offset { request.Offset }");

            var sql = queryBuilder.ToString();

            object args;

            if (string.IsNullOrWhiteSpace(request.Note))
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

        public Backup GetBackup(Backup backup)
        {
            return _sqlService.QueryFirstOrDefault<Backup>($"select * from Backup where Id = @Id;", new { Id = backup.Id });
        }

        public Backup GetLatestBackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 0 order by b.Id desc");
        }

        public Backup GetLatestAutobackup()
        {
            return _sqlService.QueryFirstOrDefault<Backup>("select * from [Backup] b where b.GameId = (select g.Title from [Game] g where g.IsCurrent = 1) and b.IsAutobackup = 1 order by b.Id desc");
        }

        public void UpdateNote(Backup backup)
        {
            _sqlService.Execute("update Backup set Note = @Note where Id = @Id;", new { Id = backup.Id, Note = backup.Note });
        }

        public void UpdateIsLiked(Backup backup)
        {
            _sqlService.Execute("update Backup set IsLiked = @IsLiked where Id = @Id;", new { Id = backup.Id, IsLiked = backup.IsLiked });
        }

        public void UpdateDriveId(Backup backup)
        {
            _sqlService.Execute("update Backup set GoogleDriveId = @GoogleDriveId where Id = @Id;", new { Id = backup.Id, DriveId = backup.GoogleDriveId });
        }

        public void UpdateFilePaths(Backup backup)
        {
            _sqlService.Execute("update Backup set FilePath = @FilePath, Picture = @Picture where Id = @Id;", new { FilePath = backup.SavefilePath, Picture = backup.PicturePath, Id = backup.Id });
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
            _sqlService.Execute("update Game set IsCurrent = 0;");
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
}
