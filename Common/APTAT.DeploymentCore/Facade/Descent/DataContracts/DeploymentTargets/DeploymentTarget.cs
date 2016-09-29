using System;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.Common;

namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{      
    public class DeploymentTarget : IComputer
	{
        public DeploymentTarget()
        {

        }
		public Guid Uid { get; set; }
	    public string Name { get; set; }
		public string DnsName { get; set; }

	    public string OrganizationUnit
	    {
	        get
	        {
	            if (string.IsNullOrEmpty(LdapPath))
	                return LdapPath;
	            return TargetsHelper.GetOrganizationUnit(LdapPath);
	        }
	    }

	    public bool IsOnline { get; set; }
        public bool IsLocalComputer { get; set; }

        public string IPAddress { get; set; }

        public System.Net.IPAddress GetIPAddress()
        {
            LogHelper.Trace(System.Diagnostics.TraceEventType.Information, "GetIPAddress was called  IP" + (String.IsNullOrWhiteSpace(this.IPAddress)? "Empty": this.IPAddress));

            if (String.IsNullOrWhiteSpace(this.IPAddress))
            {
                LogHelper.Trace(System.Diagnostics.TraceEventType.Warning, this.IPAddress + " IP Address is empty");
                return null;
            }
            System.Net.IPAddress address;
            System.Net.IPAddress.TryParse( this.IPAddress, out address);
            if (address == null)
            {
                LogHelper.Trace(System.Diagnostics.TraceEventType.Warning, this.IPAddress + " is not correct IP Address format");
            }
            return address;
        }
        public SystemKind? SystemKind { get; set; }

        public string LdapPath { get; set; }
        public virtual DeploymentTargetSource Source
        {
            get; set;
        }
    }
}
