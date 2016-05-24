using System;

namespace APTAT.Application.Messages
{
    public enum ChangeScreenTypes
    {
        StartScan,
        StartValkyrieScan,
        Scan,
        ValkyrieScan,
        StopScan
    }

    public sealed class ChangeScreen
    {
        public ChangeScreenTypes Message { get; private set; }
        public Action<object> Action { get; private set; }

        public ChangeScreen(ChangeScreenTypes message, Action<object> action)
        {
            Message = message;
            Action = action;
        }
    }

    public static class AppMessages
    {
        public static class ChangeScreenMessage
        {
            public static void Send(ChangeScreenTypes message, Action<object> action = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreen(message, action));
            }
        }
    }
}
