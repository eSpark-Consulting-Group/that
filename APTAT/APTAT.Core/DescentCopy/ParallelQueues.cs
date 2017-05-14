using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.DescentCopy
{
    public interface IRawPacket
    {

    }
    public sealed class PacketAction <T> where T : IRawPacket
    {
        public PacketAction(Guid sourceId, Action<T> action, T argument)
        {
            SourceId = sourceId;
            Action = action;
            Argument = argument;
        }

        public Guid SourceId { get; private set; }
        public Action<T> Action { get; private set; }
        public T Argument { get; private set; }

        public void Invoke()
        {
            Action(Argument);
        }
    }

    public class ParallelQueues<T> where T : IRawPacket
    {
        private sealed class CustomQueue<T> : Queue<PacketAction<T>> where T : IRawPacket
        {
            public CustomQueue(Guid id)
            {
                Id = id;
            }
            public Guid Id { get; private set; }
            public bool Frozen { get; set; }
        }

        private readonly Dictionary<Guid, CustomQueue<T>> _queues = new Dictionary<Guid, CustomQueue<T>>(1000);
        private readonly List<CustomQueue<T>> _queuesList = new List<CustomQueue<T>>(1000);
        
        private int _nextQueueIndex = 0;
        private readonly object _lockObject = new object();

        public void EnqueueAction(PacketAction<T> action)
        {
            Guid queueId = action.SourceId;
            lock (_lockObject)
            {
                CustomQueue<T> queue = null;
                if (!_queues.TryGetValue(queueId, out queue))
                {
                    queue = new CustomQueue<T>(queueId);
                    _queues.Add(queueId, queue);
                    _queuesList.Add(queue);
                }
                queue.Enqueue(action);
            }
        }


        public bool TryDeqeueActionAndFreezeQueue(out PacketAction<T> action, out Guid frozenQueueId)
        {
            bool result = false;
            action = null;
            frozenQueueId = Guid.Empty;

            lock (_lockObject)
            {
                int queuesCount = _queuesList.Count;
                if (queuesCount > 0)
                {
                    int queueIndex = _nextQueueIndex;

                    for (int i = 0; i < queuesCount; i++)
                    {
                        if (queueIndex >= queuesCount)
                            queueIndex = 0;

                        var queue = _queuesList[i];
                        if (!queue.Frozen && queue.Count > 0)
                        {
                            frozenQueueId = queue.Id;
                            action = queue.Dequeue();
                            queue.Frozen = true;
                            result = true;
                            _nextQueueIndex = queueIndex + 1;
                            break;
                        }
                        queueIndex++;
                    }
                }

            }

            return result;
        }

        public void UnfreezeQueue(Guid frozenQueueId)
        {
            lock (_lockObject)
            {
                CustomQueue<T> queue = null;
                if (_queues.TryGetValue(frozenQueueId, out queue))
                {
                    queue.Frozen = false;
                }
            }
        }

        public bool HasUnfrozenItems
        {
            get
            {
                lock (_lockObject)
                {
                    return _queuesList.Any(queue => !queue.Frozen && queue.Count > 0);
                }
            }
        }
    }
}
