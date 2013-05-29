using System;

namespace APTAT.Core.Facade.Descent.DataContracts
{
	public class InstallPackageInfo
	{
		public Guid[] PackageUids { get; set; }

		public bool SuppressReboot { get; set; }

		public bool UninstallIncompatibleProducts { get; set; }

		public CisComponents CisComponents { get; set; }

		public int Lcid { get; set; }
	}
}
