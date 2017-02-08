using FAT.Application.Views;
using System;

namespace FAT.Application.Messages
{
    public enum MessageTypes
    {
        StartScan,
        StopScan,
        FilterTarget
    }

    public static class AppMessages
    {
        public class AppMessage<T>
        {
            public MessageTypes Type { get; private set; }
            public Action<T> Action { get; private set; }

            public AppMessage(MessageTypes type, Action<T> action)
            {
                Type = type;
                Action = action;
            }
        }

        public static class ScanMessage
        {
            public static void Send(MessageTypes type, Action<object> action = null)
            {
                MessageManager.Instance.PublishMessage(new AppMessage<object>(type, action));
            }
        }

        public static class FilterMessage
        {
            public static void Send(MessageTypes type, Action<SearchUserControl> action = null)
            {
                MessageManager.Instance.PublishMessage(new AppMessage<SearchUserControl>(type, action));
            }
        }
    }
}
