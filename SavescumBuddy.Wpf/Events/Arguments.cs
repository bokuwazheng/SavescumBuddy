using SavescumBuddy.Lib;
using System;

namespace SavescumBuddy.Wpf.Events
{
    public class CallbackArgs
    {
        public Backup Backup { get; set; }
        public Action<string> Callback { get; set; }
    }
}
