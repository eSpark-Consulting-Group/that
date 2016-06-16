using System;
using APTAT.Core.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DeploymentTarget : IComputer
	{
		public Guid Uid { get; set; }
	    public string Name { get; set; }
		public string DnsName { get; set; }

	    public string OrganizationUnit
	    {
	        get
	        {
	            if (String.IsNullOrEmpty(LdapPath))
	                return LdapPath;
	            return TargetsHelper.GetOrganizationUnit(LdapPath);
	        }
	    }

	    public bool IsOnline { get; set; }
        public bool IsAptHost { get; set; }

        public string IPAddress { get; set; }

        public string LdapPath { get; set; }
    }
}
