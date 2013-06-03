using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class DeployAgentCommand : ServiceCommand<DeployAgentRequest, DeployAgentResponse>
	{
        //private IDirectoryResolver _resolver;
        //private IEntityStorageSubsystem _storage;
        //private ILicenseSubSystem _license;
	    private readonly IDescentSubSystem _descentSubSystem;

		public DeployAgentCommand( DeployAgentRequest request, IDescentSubSystem descentSubSystem )
			: base( request )
		{
		    _descentSubSystem = descentSubSystem;
		    //_resolver = SubSystemFactory.Current.Catalog.Find<IDirectoryResolver>();
		    //_storage = SubSystemFactory.Current.Catalog.Find<IEntityStorageSubsystem>();
		    //_license = SubSystemFactory.Current.Catalog.Find<ILicenseSubSystem>();
		}

		public override void Execute()
		{
			if( Request.Targets.Count == 0 )
				//throw new FaultException( "Targets list is empty." );
                throw new ArgumentException("Targets list is empty.");

            // TODO: Commented
            //var managedComputers = Enumerable.Empty<IComputer>();
            //if (Request.Targets.Except(Request.Targets.OfType<ManagedTarget>()).Any())
            //{
            //    var licensedFilter = _storage.CreateFilterComputersByIsLicensed(true);
            //    managedComputers = _storage.GetObjects<IComputer>(licensedFilter);
            //}

            //var descentSubsystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();

			//var directorySubsystem = SubSystemFactory.Current.GetSubSystem<IDirectorySubSystem>();
			List<TargetUidComputerPair> computers = new List<TargetUidComputerPair>();
			foreach( var target in Request.Targets )
			{
                // TODO: Commented
                //if( target is ManagedTarget )
                //{
                //    var managedTarget = ( ManagedTarget )target;
                //    var directoryComputer = directorySubsystem.GetComputer( managedTarget.ComputerId );
                //    if( directoryComputer != null && directoryComputer.IsOnline )
                //    {
                //        computers.Add( new TargetUidComputerPair( target.TargetUid, directoryComputer ) );
                //    }
                //    else
                //    {
                //        var computer = CreateComputer( target, directorySubsystem );
                //        var pair = new TargetUidComputerPair(target.TargetUid, computer);
                //        if (directoryComputer == null)
                //        {
                //            pair.State = new ObjectNotFoundException("Target computer is no longer managed.");
                //            pair.DescentStatus = DescentStatus.status_managed_target_not_found;
                //        }
                //        computers.Add(pair);
                //    }
                //}
                //else
                //{
                //    IComputer computer = null;
                //    IPAddress targetAddress = null;
                //    if (target.IPAddress != null && IPAddress.TryParse(target.IPAddress, out targetAddress))
                //    {
                //        computer = managedComputers
                //            .Where(c => c.IsOnline)
                //            .FirstOrDefault(c => c.IPAddress.Equals(targetAddress));
                //    }
                //    if (computer == null)
                //    {
                //        computer = new DirectoryComputer
                //        {
                //            Name = target.Name,
                //            DnsName = !String.IsNullOrEmpty(target.DnsName) ? target.DnsName : target.Name,
                //            IPAddress = targetAddress,
                //            Uid = Guid.NewGuid(),
                //        };
                //    }

                //    computers.Add( new TargetUidComputerPair( target.TargetUid, computer ) );
                //}

                // TODO: Added
                IPAddress targetAddress;
			    IPAddress.TryParse(target.IPAddress, out targetAddress);
                var computer = new DirectoryComputer
                {
                    Name = target.Name,
                    DnsName = !String.IsNullOrEmpty(target.DnsName) ? target.DnsName : target.Name,
                    IPAddress = targetAddress,
                    Uid = target.TargetUid, //Guid.NewGuid(),
                };
                computers.Add(new TargetUidComputerPair(target.TargetUid, computer));
			}

            // TODO: Commented
            //CesmLicenseInfo license = _license.LicenseContext.EffectiveLicense;
            //bool serverSupportsCisManagement = license.IsValidOnDate( _license.LicenseContext.ValidationTime ) && license.HasCisProduct;

            //InstallPackageInfo install = null;
            //if( serverSupportsCisManagement )
            //{
            //    if( Request.InstallPackage != null )
            //    {
            //        install = Request.InstallPackage.CisComponents.ToInstallPackage();
            //        install.PackageUids = Request.InstallPackage.PackageUids;
            //        install.SuppressReboot = Request.InstallPackage.SuppressReboot;
            //        install.UninstallIncompatible = Request.InstallPackage.UninstallIncompatibleProducts;
            //        install.Lcid = Request.InstallPackage.Lcid;
            //    }
            //    else if( Request.UpdateEndpoints )
            //    {
            //        var pms = SubSystemFactory.Current.GetSubSystem<IProductManagementSubSystem>();

            //        // Filter by CIS only
            //        RegisteredProduct latest = pms.GetLatestRegisteredManagedProduct(PluginGuids.CisGuid);

            //        // Identify later which components to install which - not for each computer.
            //        install = new InstallPackageInfo
            //        {
            //            InstallFW = true,
            //            InstallAV = true,
            //            Update = true,
            //            SuppressReboot = false,
            //            UninstallIncompatible = false,
            //            PackageUids = latest == null ? null : latest.Packages.Select( pack => pack.Uid ).ToArray()
            //        };
            //    }
            //}

			Guid operationUid;
            InstallPackageInfo install = null;
            _descentSubSystem.DoDescentOperationOnNotExistingComputers(computers, Request.Login, Request.Password, false, install, out operationUid);

			Response.OperationUid = operationUid;
		}

        // TODO: Commented
        //private IComputer CreateComputer( DeploymentTarget target, IDirectorySubSystem directorySubSystem )
        //{
        //    bool isManaged;
        //    Guid? uid = FindExistingUid( target.Name, target.DnsName, out isManaged );

        //    IPAddress targetAddress = null;
        //    if (target.IPAddress != null)
        //    {
        //        IPAddress.TryParse(target.IPAddress, out targetAddress);
        //    }

        //    IComputer result;
        //    if (!isManaged || uid == null)
        //    {
        //        result = new DirectoryComputer
        //        {
        //            Name = target.Name,
        //            DnsName = !String.IsNullOrEmpty(target.DnsName) ? target.DnsName : target.Name,
        //            Uid = uid ?? Guid.NewGuid(),
        //        };
        //    }
        //    else
        //    {
        //        result = directorySubSystem.GetComputer(uid.Value);
        //    }

        //    if (result != null)
        //        result.IPAddress = targetAddress;

        //    return result;
        //}
        
        //private Guid? FindExistingUid( string name, string dnsName, out bool isManaged )
        //{
        //    IComputer computer;
        //    if( !String.IsNullOrEmpty( dnsName ) )
        //    {
        //        computer = _resolver.ResolveByDnsName( dnsName );
        //        if( computer != null && computer.Visible )
        //        {
        //            isManaged = true;
        //            return computer.Uid;
        //        }
        //    }

        //    computer = _resolver.ResolveByDnsName( name );
        //    if( computer != null && computer.Visible )
        //    {
        //        isManaged = true;
        //        return computer.Uid;
        //    }

        //    IPAddress ip;
        //    if( IPAddress.TryParse( name, out ip ) )
        //    {
        //        computer = _resolver.Resolve( ip );
        //        if( computer != null && computer.Visible )
        //        {
        //            isManaged = true;
        //            return computer.Uid;
        //        }
        //    }

        //    isManaged = false;
        //    return null;
        //}
	}
}
