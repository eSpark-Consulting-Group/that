using System;
using APTAT.DeploymentCore.Facade.Descent.Commands;

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
            if (this.IPAddress == null) return null;
            System.Net.IPAddress address = System.Net.IPAddress.Parse(this.IPAddress);
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
