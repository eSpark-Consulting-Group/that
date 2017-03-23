using System;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.Common;
using System.ComponentModel;

namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{      
    public class DeploymentTarget : IComputer, INotifyPropertyChanged
	{
		public Guid Uid { get; set; }

        private string _name;

        public string Name
        {
            get { return (_name == IPAddress && !string.IsNullOrEmpty(DnsName)) ? DnsName : _name; }
            set
            {
                _name = value;
            }
        }
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

        public string IPAddressSorting
        {
            get
            {
                if (string.IsNullOrEmpty(IPAddress))
                    return string.Empty;

                var ver = Version.Parse(IPAddress);
                var str = string.Format("C{0:000}.{1:000}.{2:000}.{3:000}",
                    ver.Major, ver.Minor, ver.Build, ver.Revision);
                return str;
            }
        }

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

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
