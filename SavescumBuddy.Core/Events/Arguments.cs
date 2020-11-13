using SavescumBuddy.Data;
using System;

namespace SavescumBuddy.Core.Events
{
    public class CallbackArgs
    {
        public Backup Backup { get; set; }
        public Action<string> Callback { get; set; }
    }
}
