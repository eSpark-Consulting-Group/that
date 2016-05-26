using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Properties;

namespace APTAT.Core.Descent
{
    public abstract class Scanner : IScanner
    {
        protected readonly TaskFactory TaskFactory;
        protected readonly IChangeTrackingSubSystem Tracker;
        protected readonly string Login;
        protected readonly string Password;
        protected readonly TargetUidComputerPair[] Targets;
        protected readonly TargetUidComputerPair[] OnlineTargets;
        protected readonly CancellationToken CancellationToken;
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        protected Scanner(ISubSystemCatalog catalog, IEnumerable<DeploymentTarget> targets, string login, string password)
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            TaskFactory = new TaskFactory(
                CancellationToken,
                TaskCreationOptions.LongRunning,
                TaskContinuationOptions.None,
                new LimitedConcurrencyLevelTaskScheduler(Settings.Default.DescentMaximumConcurrencyLevel));
            Tracker = catalog.Find<IChangeTrackingSubSystem>();
            TargetUidComputerPair[] onlineTargets;
            Targets = GetTargetUidComputerPairs(targets, out onlineTargets);
            OnlineTargets = onlineTargets;
            Login = login;
            Password = password;
        }

        protected static TargetUidComputerPair[] GetTargetUidComputerPairs(IEnumerable<DeploymentTarget> targets,
            out TargetUidComputerPair[] onlineTargets)
        {
            List<TargetUidComputerPair> onlineTargetsInternal = new List<TargetUidComputerPair>();
            var deploymentTargets = targets as DeploymentTarget[] ?? targets.ToArray();
            TargetUidComputerPair[] computers = new TargetUidComputerPair[deploymentTargets.Length];
            for (int index = 0; index < deploymentTargets.Length; index++)
            {
                var target = deploymentTargets[index];
                IPAddress targetAddress;
                IPAddress.TryParse(target.IPAddress, out targetAddress);
                computers[index] = new TargetUidComputerPair(target.Uid, target);
                if (target.IsOnline)
                    onlineTargetsInternal.Add(computers[index]);
            }
            onlineTargets = onlineTargetsInternal.ToArray();
            return computers;
        }

        public virtual DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result)
        {
            return new DescentEventArgs() {OperationUid = result.OperationUid};
        }

        public const string TimeoutAbortString = "OH SHI--";

        public abstract IDescentQueryResult ValidateAndGetResult();
        public abstract void StartScan(IDescentQueryResult result);

        public virtual void AbortAllThread()
        {
            CancellationTokenSource.Cancel();
        }
        public virtual void Dispose()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }
        }
    }
}
