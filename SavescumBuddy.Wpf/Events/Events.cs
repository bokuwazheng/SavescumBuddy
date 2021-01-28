using Prism.Events;
using SavescumBuddy.Lib;
using System;

namespace SavescumBuddy.Wpf.Events
{
    public class BackupCreatedEvent : PubSubEvent<Backup> { }
    public class BackupDeletedEvent : PubSubEvent<Backup> { }
    public class BackupRestoredEvent : PubSubEvent<Backup> { }
    public class SelectedBackupChangedEvent : PubSubEvent<Backup> { }
    public class BackupListChangedEvent : PubSubEvent { }
    public class BackupIsLikedChangedEvent : PubSubEvent<Backup> { }
    public class BackupNoteChangedEvent : PubSubEvent<Backup> { }
    public class BackupInGoogleDriveChangedEvent : PubSubEvent<Backup> { }
    public class BackupListSortedEvent : PubSubEvent<IBackupSearchRequest> { }
    public class NavigatedToSettingsEvent : PubSubEvent { }
    public class NextPageRequestedEvent : PubSubEvent<IBackupSearchRequest> { }
    public class PreviousPageRequestedEvent : PubSubEvent<IBackupSearchRequest> { }
    public class FirstPageRequestedEvent : PubSubEvent<IBackupSearchRequest> { }
    public class LastPageRequestedEvent : PubSubEvent<IBackupSearchRequest> { }
    public class AutobackupsEnabledChangedEvent : PubSubEvent<bool> { }
    public class AutobackupIntervalChangedEvent : PubSubEvent<bool> { }
    public class BackupCreationRequestedEvent : PubSubEvent<Backup> { }
    public class BackupDeletionRequestedEvent : PubSubEvent<Backup> { }
    public class BackupListUpdateRequestedEvent : PubSubEvent { }
    public class ErrorOccuredEvent : PubSubEvent<Exception> { }
    public class HookChangedEvent : PubSubEvent<bool> { }
    public class HookEnabledChangedEvent : PubSubEvent<bool> { }
    public class StartProcessRequestedEvent : PubSubEvent<string> { }
    public class GoogleDriveUploadRequestedEvent : PubSubEvent<Backup> { }
    public class GoogleDriveDeletionRequestedEvent : PubSubEvent<Backup> { }
    public class GoogleDriveCancellationRequestedEvent : PubSubEvent { }
}
