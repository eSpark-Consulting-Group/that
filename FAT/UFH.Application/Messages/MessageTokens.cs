
using APTAT.Core.CMC;
using UFH.Application.Views;
using System;

namespace UFH.Application.Messages
{
    public enum MessageTypes
    {
        StartScan,
        StopScan,
        FilterTarget,
        CMCStatistics
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

        public static class ScanMessages
        {
            public static void Send(MessageTypes type, Action<object> action = null)
            {
                MessageManager.Instance.PublishMessage(new AppMessage<object>(type, action));
            }
        }

        public static class FilterMessages
        {
            public static void Send(Action<SearchUserControl> action = null)
            {
                MessageManager.Instance.PublishMessage(new AppMessage<SearchUserControl>(MessageTypes.FilterTarget, action));
            }
        }

        public static class CMCStatisticsMessages<T> where T: IRequest
        {
            public static void Send(Action<T> request = null)
            {
                MessageManager.Instance.PublishMessage(new AppMessage<T>(MessageTypes.CMCStatistics, request));
            }
        }
    }
}
