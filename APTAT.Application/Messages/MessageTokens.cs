using System;

namespace APTAT.Application.Messages
{
    public interface IMessage<out T>
    {
        T Message { get; }
    }

    public enum ViewFormTypes
    {
        Scan,
        ValkyrieScan
    }

    public sealed class ChangeScreenMessageType : IMessage<ViewFormTypes>
    {
        public ViewFormTypes Message { get; internal set; }
        public bool ToClose { get; internal set; }
    }

    public sealed class ExceptionMessage
    {
        public ExceptionMessage(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get;  set; }

        public void ShowError()
        {
        }
    }

    public static class AppMessages
    {
        public static class ChangeScreenMessage
        {
            public static void Send(ViewFormTypes messageType, bool toClose = false)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreenMessageType { Message = messageType, ToClose = toClose });
            }
        }
    }
}
