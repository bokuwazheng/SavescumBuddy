using Prism.Events;
using SavescumBuddy.Data;

namespace SavescumBuddy.Core.Events
{
    public class BackupCreatedEvent : PubSubEvent<Backup> { }
    public class BackupDeletedEvent : PubSubEvent<Backup> { }
    public class BackupRestoredEvent : PubSubEvent<Backup> { }
    public class SelectedBackupChangedEvent : PubSubEvent<Backup> { }
    public class BackupListChangedEvent : PubSubEvent { }
    public class BackupIsLikedChangedEvent : PubSubEvent<Backup> { }
    public class BackupNoteChangedEvent : PubSubEvent<Backup> { }
    public class BackupInGoogleDriveChangedEvent : PubSubEvent<Backup> { }
    public class BackupListSortedEvent : PubSubEvent<BackupSearchRequest> { }
    public class NavigatedToSettingsEvent : PubSubEvent { }
    public class NextPageRequestedEvent : PubSubEvent<BackupSearchRequest> { }
    public class PreviousPageRequestedEvent : PubSubEvent<BackupSearchRequest> { }
    public class FirstPageRequestedEvent : PubSubEvent<BackupSearchRequest> { }
    public class LastPageRequestedEvent : PubSubEvent<BackupSearchRequest> { }
}
