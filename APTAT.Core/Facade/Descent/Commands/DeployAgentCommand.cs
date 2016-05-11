using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class DeployAgentCommand : ServiceCommand<DeployAgentRequest, DeployAgentResponse>
	{
	    private readonly IDescentSubSystem _descentSubSystem;

		public DeployAgentCommand( DeployAgentRequest request, IDescentSubSystem descentSubSystem )
			: base( request )
		{
		    _descentSubSystem = descentSubSystem;
		}

		public override void Execute()
		{
			if( Request.Targets.Count == 0 )
                throw new ArgumentException("Targets list is empty.");
			List<TargetUidComputerPair> computers = new List<TargetUidComputerPair>();
			foreach( var target in Request.Targets )
			{

                // TODO: Added
                IPAddress targetAddress;
                IPAddress.TryParse(target.IPAddress, out targetAddress);
                var computer = new DirectoryComputer
                {
                    Name = target.Name,
                    DnsName = !String.IsNullOrEmpty(target.DnsName) ? target.DnsName : target.Name,
                    IPAddress = targetAddress,
                    Uid = target.TargetUid, //Guid.NewGuid(),
                    IsOnline = target.Online,
                    LdapPath = target.LdapPath,
                    OrganizationUnit = GetOrganizationUnit(target.LdapPath),
                };
                computers.Add(new TargetUidComputerPair(target.TargetUid, computer));
			}
            

			Guid operationUid;
            _descentSubSystem.DoDescentOperationOnNotExistingComputers(computers, Request.Login, Request.Password,
                out operationUid, Request.ScanScope, Request.Domain, Request.DevicesSourceDescription);

			Response.OperationUid = operationUid;
		}

        

        public static string GetOrganizationUnit(string ldapPath /*, out string firstParent*/ )
        {
            string firstParent;
            if (String.IsNullOrEmpty(ldapPath))
                return null;

            string[] parts = ldapPath.Split(',');
            List<string> units = new List<string>();
            List<string> containers = new List<string>();
            foreach (var part in parts)
            {
                if (part.StartsWith("CN=", true, CultureInfo.InvariantCulture) && part.Length > 3)
                    containers.Add(part.Substring(3));

                if (part.StartsWith("OU=", true, CultureInfo.InvariantCulture) && part.Length > 3)
                    units.Add(part.Substring(3));
            }

            if (units.Count > 0)
            {
                firstParent = units[0];
            }
            else
            {
                if (containers.Count > 0)
                {
                    firstParent = containers[0];
                    return firstParent;
                }
            }

            units.Reverse();

            StringBuilder sb = new StringBuilder();
            foreach (var unit in units)
            {
                sb.Append(String.Concat(unit, "\\"));
            }

            return sb.ToString().TrimEnd('\\');
        }
	}
}
