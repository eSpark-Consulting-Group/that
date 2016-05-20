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
        private static readonly int MaxConcurrentTasks = Settings.Default.MaxEndpointsAvailabilityResolverConcurrentTasks;

		// Must be in sync with the Esm.Shared.Modules.Computers.Wrappers.IpAddressValidator.MaxTargetsCount and MaxTargetsNetworkBits constants!
		private const int MaxDeploymentTargets = 1024;
		private readonly INetworkObserver _networkObserver = new DirectorySubSystem();
	    private readonly TaskFactory _taskFactory;

        public GetTargetsSummaryCommand(GetTargetsSummaryRequest request): base(request)
		{
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

		    var targetComputers = Request.Targets.Where(target => !(target is DomainTarget) ||
                ((DomainTarget) target).EntryType ==DomainEntryType.Computer)
		        .Select(t =>
		        {
                    if (String.IsNullOrEmpty(t.LdapPath))
                    {
                        var ipAddress = ParseIP(t.Name);
                        if (ipAddress != null)
                            t.IPAddress = ipAddress.ToString();
                    }
                    return t;
		        });

            var computers = new List<IComputer>(targetComputers);
            var searcher = new ActiveDirectorySearcher(_activeDirectoryObserver, Request.DomainAccessLogin, Request.DomainAccessPassword);

            var searchTasks = from dt in Request.Targets.OfType<DomainTarget>()
                where dt.EntryType != DomainEntryType.Computer
                let asyncResult = searcher.BeginSearch(dt.LdapPath, foundComputers =>
                {
                    lock (computers)
                    {
                        computers.AddRange(from fc in foundComputers
                                            select new DomainTarget
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

            if (searchTasks.Any())
            {
                using (var completeTask = _taskFactory.ContinueWhenAll((Task[])searchTasks, completedTasks => ResolveAndTest(computers)))
                {
                    try
                    {
                        completeTask.Wait();

                        Exception temp = null;
                        // Looking for exceptions...
                        foreach (var searchTask in searchTasks)
                        {
                            if (searchTask.Exception != null && searchTask.Exception.InnerExceptions != null)
                            {
                                foreach (var innerException in searchTask.Exception.InnerExceptions)
                                {
                                    if(innerException != null)
                                        temp = innerException;
                                }
                            }
                        }

                        // Throw one of exceptions (no matter which)
                        if (temp != null)
                            throw temp;
                    }
                    catch (AggregateException x)
                    {
                        LogHelper.Trace(TraceEventType.Error, "GetTargetsSummaryCommand AggregateException: {0}. Rethrowing the exception...", x.Message);
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
                                  if (!extv4.Any())
                                      extv4 = IPs.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
                                  if (!extv4.Any())
                                      extv4 = IPs;
                                  var firstOrDefault = extv4.FirstOrDefault();
                                  if (firstOrDefault != null)
                                      target.IPAddress = firstOrDefault.ToString();
                              }
                          });

            ipTasks = ipTasks.ToArray();
            if (ipTasks.Any())
            {
                using (var completeTask = _taskFactory.ContinueWhenAll((ThreadingTask[])ipTasks, completedTasks => Array.ForEach(completedTasks, t => t.Dispose())))
                {
                    completeTask.Wait();
                }
            }

            // Tasks to resolve unknown DNS names
            var dnsTasks = from target in uniqueComputers
                           where String.IsNullOrEmpty(target.DnsName) && target.IPAddress != null
                           select _taskFactory.StartNew(() => target.DnsName = _networkObserver.ResolveDnsByIp(IPAddress.Parse(target.IPAddress)));

            dnsTasks = dnsTasks.ToArray();
            if (dnsTasks.Any())
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
            if (testTasks.Any())
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
                                             Uid = target.Uid, // IComputer.Uid was used as a temp storage for TargetUid
                                             Name = name,
                                             DnsName = target.DnsName,
                                             IPAddress = ipAddress,
                                             IsOnline = testTask != null ? testTask.Result.Item2 : false,
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
                          join t in deploymentTargets on c.Uid equals t.Uid into relatedTargets
                          from rt in relatedTargets.DefaultIfEmpty(new DeploymentTarget
                          {
                              Uid = c.Uid,
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
        public static string ToStringOrEmpty<T>(this T source) where T : class
        {
            return source != null ? source.ToString() : String.Empty;
        }
    }
}
