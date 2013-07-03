using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using APTAT.Core.Properties;
using APTAT.Core.Descent;
using APTAT.Core.Directory;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;
using ThreadingTask = System.Threading.Tasks.Task;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class GetTargetsSummaryCommand:
		ServiceCommand<GetTargetsSummaryRequest, GetTargetsSummaryResponse>,
		IEqualityComparer<IComputer>,
		IEqualityComparer<DeploymentTarget>
	{
        // TODO: Probably should be moved to some Initialization part
        private readonly IActiveDirectoryObserver _activeDirectoryObserver = new ActiveDirectoryObserver();
        private readonly IDirectorySubSystem _directorySubSystem = new DirectorySubSystem();

		private static readonly int MaxConcurrentTasks = Settings.Default.MaxEndpointsAvailabilityResolverConcurrentTasks;

		// Must be in sync with the Esm.Shared.Modules.Computers.Wrappers.IpAddressValidator.MaxTargetsCount and MaxTargetsNetworkBits constants!
		private const int MaxDeploymentTargets = 1024;

		private INetworkObserver _networkObserver;
		private TaskFactory _taskFactory;

		public GetTargetsSummaryCommand(GetTargetsSummaryRequest request): base(request)
		{
            _networkObserver = _directorySubSystem as INetworkObserver;//SubSystemFactory.Current.GetSubSystem<IDirectorySubSystem>() as INetworkObserver;
			_taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(MaxConcurrentTasks));
		}

		public override void Execute()
		{
			if (Request.Targets.Count == 0)
                //throw new FaultException<TargetsNotFoundFault>(new TargetsNotFoundFault(),"Targets list is empty.");
                throw new InvalidOperationException("Targets list is empty.");

			Func<string, IPAddress> ParseIP = source =>
			{
				IPAddress result;
				return source != null && IPAddress.TryParse(source, out result) ? result : null;
			};

			var targetComputers = from target in Request.Targets
								  let domainTarget = target as DomainTarget
								  where domainTarget == null || domainTarget.EntryType == DomainEntryType.Computer
								  select new DirectoryComputer
								  {
									  Uid = target.TargetUid, // Use DirectoryComputer.Uid as a temp storage for TargetUid
									  Name = target.Name,
									  DnsName = target.DnsName,
									  IPAddress = domainTarget == null ? ParseIP(target.Name) : null,
                                      IsOnline = target.Online,
                                      LdapPath = domainTarget != null ? domainTarget.LdapPath : null,
								  };

			var computers = new List<IComputer>(targetComputers);
			//var directory = SubSystemFactory.Current.GetSubSystem<IDirectorySubSystem>();
            var searcher = new ActiveDirectorySearcher(_activeDirectoryObserver, Request.DomainAccessLogin, Request.DomainAccessPassword);

			var searchTasks = from dt in Request.Targets.OfType<DomainTarget>()
							  where dt.EntryType != DomainEntryType.Computer
							  let asyncResult = searcher.BeginSearch(dt.LdapPath, foundComputers =>
							  {
								  lock (computers)
								  {
									  computers.AddRange(from fc in foundComputers select new DirectoryComputer
									  {
										  Uid = Guid.NewGuid(), // Comp was not found on console, give it a new GUID as a temp storage for TargetUid
										  Name = fc.Name,
										  DnsName = fc.DnsName,
                                          LdapPath = fc.LdapPath
									  });
								  }
							  })
							  select _taskFactory.FromAsync(asyncResult, searcher.EndSearch);

			searchTasks = searchTasks.ToArray();
			if (searchTasks.Count() > 0)
			{
				using (var completeTask = _taskFactory.ContinueWhenAll((Task[]) searchTasks, completedTasks => ResolveAndTest(computers)))
				{
					try
					{
						completeTask.Wait();
					}
					catch (AggregateException x)
					{
                        // TODO: Commented
						//if (x.InnerException is FaultException)
						//	throw x.InnerException;
						//else
							throw;
					}
				}
			}
			else
			{
				ResolveAndTest(computers);
			}

			Response.IsCompleted = true;
		}

        private void ResolveAndTest(IEnumerable<IComputer> sourceComputers)
        {
            //sourceComputers = sourceComputers.Distinct(this);

            IComputer[] uniqueComputers = sourceComputers.Distinct(this).ToArray();

            if (uniqueComputers.Count() > MaxDeploymentTargets)
                //throw new FaultException<TooManyTargetsFault>(new TooManyTargetsFault { MaxAllowableTargets = MaxDeploymentTargets });
                throw new InvalidOperationException("Too many targets");

            // Tasks to resolve unknown IPs by DNS name
            var ipTasks = from target in uniqueComputers
                          where target.IPAddress == null
                          let name = target.DnsName ?? target.Name
                          select _taskFactory.StartNew(() =>
                          {
                              var IPs = _networkObserver.ResolveIpByDns(name);
                              if (IPs != null)
                              {
                                  var extv4 = IPs.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)).ToArray();
                                  if (extv4.Count() == 0)
                                      extv4 = IPs.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
                                  if (extv4.Count() == 0)
                                      extv4 = IPs;
                                  target.IPAddress = extv4.First();
                              }
                          });

            ipTasks = ipTasks.ToArray();
            if (ipTasks.Count() > 0)
            {
                using (var completeTask = _taskFactory.ContinueWhenAll((ThreadingTask[])ipTasks, completedTasks => Array.ForEach(completedTasks, t => t.Dispose())))
                {
                    completeTask.Wait();
                }
            }

            // Tasks to resolve unknown DNS names
            var dnsTasks = from target in uniqueComputers
                           where String.IsNullOrEmpty(target.DnsName) && target.IPAddress != null
                           select _taskFactory.StartNew(() => target.DnsName = _networkObserver.ResolveDnsByIp(target.IPAddress));

            dnsTasks = dnsTasks.ToArray();
            if (dnsTasks.Count() > 0)
            {
                using (var completeTask = _taskFactory.ContinueWhenAll((ThreadingTask[])dnsTasks, completedTasks =>
                                                                                                      {
                                                                                                          foreach (var completedTask in completedTasks)
                                                                                                          {
                                                                                                              completedTask.Dispose();
                                                                                                          }
                                                                                                      }))
                {
                    completeTask.Wait();
                }
            }

            var deploymentTargets = new List<DeploymentTarget>(uniqueComputers.Count());

            // Tasks to test targets are online and have port 445 open
            var testTasks = from target in uniqueComputers
                            where target.IPAddress != null
                            select _taskFactory.StartNew(() => Tuple.Create(target, _networkObserver.TestMsDs(target.IPAddress.ToString())));

            testTasks = testTasks.ToArray();
            if (testTasks.Count() > 0)
            {
                var completeTask = _taskFactory.ContinueWhenAll(testTasks.ToArray(), completedTasks =>
                {
                    var updatedTargets = from target in uniqueComputers
                                         join task in completedTasks on target equals task.Result.Item1 into relatedTask
                                         from testTask in relatedTask.DefaultIfEmpty()
                                         let ipAddress = target.IPAddress.ToStringOrEmpty()
                                         let name = target.Name != ipAddress ? target.Name : target.DnsName.ToStringOrEmpty()
                                         select new DeploymentTarget
                                         {
                                             TargetUid = target.Uid, // IComputer.Uid was used as a temp storage for TargetUid
                                             Name = name,
                                             DnsName = target.DnsName,
                                             IPAddress = ipAddress,
                                             Online = testTask != null ? testTask.Result.Item2 : false,
                                             LdapPath = target.LdapPath
                                         };

                    deploymentTargets.AddRange(updatedTargets);
                    Array.ForEach(completedTasks, t => t.Dispose());
                });

                using (completeTask)
                {
                    completeTask.Wait();
                }
            }

            var results = from c in uniqueComputers
                          join t in deploymentTargets on c.Uid equals t.TargetUid into relatedTargets
                          from rt in relatedTargets.DefaultIfEmpty(new DeploymentTarget
                          {
                              TargetUid = c.Uid,
                              Name = c.Name,
                              DnsName = c.DnsName ?? c.Name,
                              LdapPath = c.LdapPath
                              // c.IPAddress is always null here
                          })
                          select rt.WithSideEffect(t =>
                          {
                              if (t.Name == t.DnsName && t.DnsName != t.IPAddress.ToStringOrEmpty())
                                  t.Name = t.DnsName.Split('.').First().ToUpper();
                          });

            results = results.Distinct(this).OrderBy(t => t.Name).ToArray();
            //TargetsHelper.FindManagedTargetsAndEsmServerHost(results);
            Response.Targets.AddRange(results);
        }

		public bool Equals(IComputer x, IComputer y)
		{
			return x.Name == y.Name && x.DnsName == y.DnsName &&
				(x.IPAddress != null ? x.IPAddress.Equals(y.IPAddress) : 
				 y.IPAddress != null ? y.IPAddress.Equals(x.IPAddress) :
				 true);
		}

		public int GetHashCode(IComputer obj)
		{
			return 327 ^
				(obj.Name ?? String.Empty).GetHashCode() ^
				(obj.DnsName ?? String.Empty).GetHashCode() ^
				(obj.IPAddress != null ? obj.GetHashCode() : 0);
		}

		public bool Equals(DeploymentTarget x, DeploymentTarget y)
		{
			return x.IPAddress == y.IPAddress || (String.IsNullOrEmpty(x.IPAddress) && String.IsNullOrEmpty(y.IPAddress) && x.Name == y.Name);
		}

		public int GetHashCode(DeploymentTarget obj)
		{
			return !String.IsNullOrEmpty(obj.IPAddress) ? obj.IPAddress.GetHashCode() : obj.Name.GetHashCode();
		}
	}

    public static partial class MapperHelper
    {
        [DebuggerStepThrough]
        public static T WithSideEffect<T>(this T @object, Action<T> sideEffect)
        {
            sideEffect(@object);
            return @object;
        }
    }
}
