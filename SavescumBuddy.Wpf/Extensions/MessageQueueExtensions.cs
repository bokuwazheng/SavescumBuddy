using MaterialDesignThemes.Wpf;
using System;

namespace SavescumBuddy.Wpf.Extensions
{
    public static class MessageQueueExtensions
    {
        public static void PromptAction(this ISnackbarMessageQueue queue, string content, string actionContent, Action action, TimeSpan? duration = null)
        {
            queue.Enqueue(content, actionContent, _ => action(), null, true, true, duration);
        }

        public static void Notify(this ISnackbarMessageQueue queue, string content)
        {
            queue.Enqueue(content, "OK", () => { }, true);
        }
    }
}
