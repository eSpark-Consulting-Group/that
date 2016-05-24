using System;

namespace APTAT.Application.Messages
{
    public enum ViewFormTypes
    {
        StartScan,
        StartValkyrieScan,
        Scan,
        ValkyrieScan
    }

    public sealed class ChangeScreenMessageType
    {
        public ViewFormTypes Message { get; internal set; }
        public Action<object> Action { get; internal set; }
    }

    public static class AppMessages
    {
        public static class ChangeScreenMessage
        {
            public static void Send(ViewFormTypes messageType, Action<object> action = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreenMessageType { Message = messageType, Action = action });
            }
        }
    }
}
