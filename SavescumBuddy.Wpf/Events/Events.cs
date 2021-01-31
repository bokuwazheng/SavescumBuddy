using Prism.Events;
using System;

namespace SavescumBuddy.Wpf.Events
{
    public class AutobackupsEnabledChangedEvent : PubSubEvent<bool> { }
    public class AutobackupIntervalChangedEvent : PubSubEvent<bool> { }
    public class BackupListUpdateRequestedEvent : PubSubEvent { }
    public class ErrorOccuredEvent : PubSubEvent<Exception> { }
    public class HookEnabledChangedEvent : PubSubEvent<bool> { }
    public class StartProcessRequestedEvent : PubSubEvent<string> { }
    public class DialogIsOpenChangedEvent : PubSubEvent<bool> { }
}
