using APTAT.DeploymentCore.Descent;
using APTAT.Core.Descent.Copy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using APTAT.Common;
using System.Diagnostics;
using APTAT.Core.Valkyrie;

namespace APTAT.Core.DescentCopy
{
    public class QueueItem : IRawPacket
    {
        public Guid ActionUid { get; }
        public string Login { get; }
        public string Password { get; }
        public IFileInfo[] InstancesInfo { get; }

        public QueueItem(IFileInfo[] instances, string login, string password, Guid actionUid)
        {
            InstancesInfo = instances;
            Login = login;
            Password = password;
            ActionUid = actionUid;
        }
    }

    public class CopyHelper : ICopyHelper
    {
        private readonly ParallelQueues<QueueItem> _queue = new ParallelQueues<QueueItem>();
        Object _lockObj = new Object();
        bool _isNotStarted = true;
        private readonly List<ICopyFileResult> _remoteFilePathes = new List<ICopyFileResult>();
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly ConcurrentDictionary<Guid, List<ICopyFileResult>> _dict
            = new ConcurrentDictionary<Guid, List<ICopyFileResult>>();
        private readonly ConcurrentDictionary<Guid, IFileInfo[]> _inputData
            = new ConcurrentDictionary<Guid, IFileInfo[]>();
        private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _uidAggregatedToDescent
            = new ConcurrentDictionary<Guid, HashSet<Guid>>();
        private readonly ConcurrentDictionary<Guid, Guid> _uidDescentToAggregated
            = new ConcurrentDictionary<Guid, Guid>();
        public IReadOnlyCollection<ICopyFileResult> RemoteFilePathes { get; } 

        public bool CheckResults(Guid operationUid, out ICopyFileResult[] results)
        {
            results = new ICopyFileResult[0];
            List<ICopyFileResult> files;
            if (!_dict.TryGetValue(operationUid, out files))
                return false;
            results = files.ToArray();
            return true;
        }

        public event EventHandler<CopyDescentCompletedEventArgs> CopyCompleted;

        private void RaiseCopyCompleted(CopyDescentCompletedEventArgs e)
        {
            var tmp = CopyCompleted;
            if (tmp != null)
            {
                Task.Run(() =>
                {
                    tmp(this, e);
                });
            }
        }


        private Guid QueueAction(IFileInfo[] files, string login, string password)
        {
            Guid aggregatedUid = Guid.NewGuid();
            var grouppedFiles = files.GroupBy(f => f.Computer.Uid);
            HashSet<Guid> descentUids = new HashSet<Guid>();
            foreach (var group in grouppedFiles)
            {
                var package = new QueueItem(group.ToArray(), login, password, aggregatedUid);

                Action<QueueItem> action = (arg) =>
                {
                    Guid operationUid;
                    var scanner = new CopyDescentScanner(new[] { arg.InstancesInfo.First().Computer }, arg.Login, arg.Password
                        , null, arg.InstancesInfo.Select(f => f.Path).ToArray());
                    _descentSubSystem.StartScan(scanner, out operationUid);
                    _inputData.TryAdd(operationUid, arg.InstancesInfo);
                    descentUids.Add(operationUid);
                    _uidDescentToAggregated.TryAdd(operationUid, arg.ActionUid);
                };
                _queue.EnqueueAction(new PacketAction<QueueItem>(group.Key, action, package));
            }

            bool shouldStart;
            lock (_lockObj)
            {
                shouldStart = _isNotStarted;
            }
            if (shouldStart)
            {
                Task.Factory.StartNew(() => TryDequeueAction());
            }

            //_uidAggregatedToDescent.TryAdd(aggregatedUid, descentUids);
            return aggregatedUid;
        }

        private void TryDequeueAction()
        {
            Guid aggregatedUid = Guid.NewGuid();
            lock (_lockObj)
            {
                if (!_isNotStarted)
                    return;
                _isNotStarted = false;
            }

            PacketAction<QueueItem> action;
            Guid queueId;
            while (_queue.TryDeqeueActionAndFreezeQueue(out action, out queueId))
            {
                try
                {
                    action.Invoke();
                    break;
                }
                catch (Exception ex)
                {

                    LogHelper.Trace(TraceEventType.Error,
                        $"Statistics service warning: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            bool shouldStart = false;
            lock (_lockObj)
            {
                _isNotStarted = true;
                if (_queue.HasUnfrozenItems)
                    shouldStart = true;
            }
            if (shouldStart)
                Task.Factory.StartNew(() => TryDequeueAction());

        }

        public Guid RequestFiles(IFileInfo[] filePathes, string login, string password)
        {
            return QueueAction(filePathes, login, password);
        }

        public CopyHelper(IDescentSubSystem descent)
        {
            _descentSubSystem = descent;
            _descentSubSystem.DescentCompleted += _descentSubSystem_DescentCompleted;
            RemoteFilePathes = new ReadOnlyCollection<ICopyFileResult>(_remoteFilePathes);   
        }

        private void _descentSubSystem_DescentCompleted(object sender, DescentEventArgs e)
        {
            Guid aggregatedUid;
            if (_uidDescentToAggregated.TryRemove(e.OperationUid, out aggregatedUid))
            {
                var results = _descentSubSystem.GetDescentResultsByOperationUid(e.OperationUid);
                var copyResult =  results.ComputerDescentResults.OfType<ICopyResult>().First();
                IFileInfo[] input;
                _inputData.TryRemove(e.OperationUid, out input);
                for (int i = 0;i < copyResult.FilePathes.Length;i++)
                {
                    var path = copyResult.FilePathes[i];
                    var split = path.Split(new[] { ';' });
                    if (split[0] == "0")
                        input[i].CopyResult = new CopyFileResult(split[1]);
                }
                RaiseCopyCompleted(new CopyDescentCompletedEventArgs(input));
                _queue.UnfreezeQueue(copyResult.TargetUid);

                bool shouldStart = false;
                lock (_lockObj)
                {
                    if (_isNotStarted && _queue.HasUnfrozenItems)
                        shouldStart = true;
                }
                if (shouldStart)
                    Task.Factory.StartNew(() => TryDequeueAction());
            }
        }

        public void Dispose()
        {
            RemoveRemoteContentSafe();
            _descentSubSystem.DescentCompleted -= _descentSubSystem_DescentCompleted;

        }

        public void RemoveRemoteContentSafe()
        {
            foreach (var path in _remoteFilePathes)
            {
                path.Dispose();
            }
            _remoteFilePathes.Clear();
        }
    }
}
