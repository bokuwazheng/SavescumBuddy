﻿using SavescumBuddy.Services.Interfaces;
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

        public int SchedulerInterval
        {
            get => Get<int>(nameof(SchedulerInterval));
            set => Set(nameof(SchedulerInterval), ref value);
        }

        public int SchedulerOverwriteType
        {
            get => Get<int>(nameof(SchedulerOverwriteType));
            set => Set(nameof(SchedulerOverwriteType), ref value);
        }

        public bool SchedulerEnabled
        {
            get => Get<bool>(nameof(SchedulerEnabled));
            set => Set(nameof(SchedulerEnabled), ref value);
        }

        public int SchedulerSkipType
        {
            get => Get<int>(nameof(SchedulerSkipType));
            set => Set(nameof(SchedulerSkipType), ref value);
        }

        public int BackupKey
        {
            get => Get<int>(nameof(BackupKey));
            set => Set(nameof(BackupKey), ref value);
        }

        public int BackupModifier
        {
            get => Get<int>(nameof(BackupModifier));
            set => Set(nameof(BackupModifier), ref value);
        }

        public bool HotkeysEnabled
        {
            get => Get<bool>(nameof(HotkeysEnabled));
            set => Set(nameof(HotkeysEnabled), ref value);
        }

        public int OverwriteKey
        {
            get => Get<int>(nameof(OverwriteKey));
            set => Set(nameof(OverwriteKey), ref value);
        }

        public int OverwriteModifier
        {
            get => Get<int>(nameof(OverwriteModifier));
            set => Set(nameof(OverwriteModifier), ref value);
        }

        public int RestoreKey
        {
            get => Get<int>(nameof(RestoreKey));
            set => Set(nameof(RestoreKey), ref value);
        }

        public int RestoreModifier
        {
            get => Get<int>(nameof(RestoreModifier));
            set => Set(nameof(RestoreModifier), ref value);
        }

        public bool SoundCuesEnabled
        {
            get => Get<bool>(nameof(SoundCuesEnabled));
            set => Set(nameof(SoundCuesEnabled), ref value);
        }

        private T Get<T>(string propertyName)
        {
            return _sqlService.ExecuteScalar<T>($"SELECT { propertyName } FROM Settings WHERE Id = 1;");
        }

        private void Set<T>(string propertyName, ref T value)
        {
            _sqlService.Execute($"UPDATE Settings SET { propertyName } = @Value WHERE Id = 1;", new { Value = value });
        }
    }
}
