using SavescumBuddy.Services.Interfaces;
using System;

namespace SavescumBuddy.Services
{
    public class SqliteSettingsAccess : ISettingsAccess
    {
        private ISqliteDbService _sqlService;

        public SqliteSettingsAccess(ISqliteDbService sqlService)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        }

        public int AutobackupInterval 
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(AutobackupInterval) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(AutobackupInterval) } = @{ nameof(AutobackupInterval) } WHERE Id = 1;", new { AutobackupInterval = value });
        }

        public int AutobackupOverwriteType
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(AutobackupOverwriteType) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(AutobackupOverwriteType) } = @{ nameof(AutobackupOverwriteType) } WHERE Id = 1;", new { AutobackupOverwriteType = value });
        }

        public bool AutobackupsEnabled
        {
            get => _sqlService.ExecuteScalar<bool>($"SELECT { nameof(AutobackupsEnabled) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(AutobackupsEnabled) } = @{ nameof(AutobackupsEnabled) } WHERE Id = 1;", new { AutobackupsEnabled = value });
        }

        public int AutobackupSkipType
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(AutobackupSkipType) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(AutobackupSkipType) } = @{ nameof(AutobackupSkipType) } WHERE Id = 1;", new { AutobackupSkipType = value });
        }

        public int BackupKey
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(BackupKey) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(BackupKey) } = @{ nameof(BackupKey) } WHERE Id = 1;", new { BackupKey = value });
        }

        public int BackupModifier
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(BackupModifier) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(BackupModifier) } = @{ nameof(BackupModifier) } WHERE Id = 1;", new { BackupModifier = value });
        }

        public string CloudAppRootFolderId
        {
            get => _sqlService.ExecuteScalar<string>($"SELECT { nameof(CloudAppRootFolderId) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(CloudAppRootFolderId) } = @{ nameof(CloudAppRootFolderId) } WHERE Id = 1;", new { CloudAppRootFolderId = value });
        }

        public bool HotkeysEnabled
        {
            get => _sqlService.ExecuteScalar<bool>($"SELECT { nameof(HotkeysEnabled) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(HotkeysEnabled) } = @{ nameof(HotkeysEnabled) } WHERE Id = 1;", new { HotkeysEnabled = value });
        }

        public int OverwriteKey
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(OverwriteKey) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(OverwriteKey) } = @{ nameof(OverwriteKey) } WHERE Id = 1;", new { OverwriteKey = value });
        }

        public int OverwriteModifier
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(OverwriteModifier) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(OverwriteModifier) } = @{ nameof(OverwriteModifier) } WHERE Id = 1;", new { OverwriteModifier = value });
        }

        public int RestoreKey
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(RestoreKey) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(RestoreKey) } = @{ nameof(RestoreKey) } WHERE Id = 1;", new { RestoreKey = value });
        }

        public int RestoreModifier
        {
            get => _sqlService.ExecuteScalar<int>($"SELECT { nameof(RestoreModifier) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(RestoreModifier) } = @{ nameof(RestoreModifier) } WHERE Id = 1;", new { RestoreModifier = value });
        }

        public bool SoundCuesEnabled
        {
            get => _sqlService.ExecuteScalar<bool>($"SELECT { nameof(SoundCuesEnabled) } FROM Settings WHERE Id = 1;");
            set => _sqlService.Execute($"UPDATE Settings SET { nameof(SoundCuesEnabled) } = @{ nameof(SoundCuesEnabled) } WHERE Id = 1;", new { SoundCuesEnabled = value });
        }
    }
}
