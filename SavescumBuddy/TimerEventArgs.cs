using System;

namespace SavescumBuddy
{
    public class TimerEventArgs : EventArgs
    {
        public bool EnabledChanged { get; set; }

        public TimerEventArgs(bool enabledChanged)
        {
            this.EnabledChanged = enabledChanged;
        }
    }
}
