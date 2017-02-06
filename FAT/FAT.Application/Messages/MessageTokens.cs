using FAT.Application.Views;
using System;

namespace FAT.Application.Messages
{
    public enum ChangeScreenTypes
    {
        StartScan,
        StopScan,
        FilterTarget
    }

    public sealed class ChangeScreen<T>
    {
        public ChangeScreenTypes Message { get; private set; }
        public Action<T> Action { get; private set; }

        public ChangeScreen(ChangeScreenTypes message, Action<T> action)
        {
            Message = message;
            Action = action;
        }
    }

    public static class AppMessages
    {
        public static class ScanMessage
        {
            public static void Send(ChangeScreenTypes message, Action<object> action = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreen<object>(message, action));
            }
        }

        public static class FilterMessage
        {
            public static void Send(ChangeScreenTypes message, Action<SearchUserControl> action = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreen<SearchUserControl>(message, action));
            }
        }
    }
}
