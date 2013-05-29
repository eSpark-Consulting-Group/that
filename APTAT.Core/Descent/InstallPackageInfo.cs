using System;
using System.Text;

namespace CRM.SrvCore.Descent
{
	public class InstallPackageInfo
	{
		public const string InstallFWParam = "INSTALLFIREWALL";
		public const string InstallAVParam = "INSTALLANTIVIRUS";
		public const string RestoreConfigParam = "RESTOREACTIVECONFIG";
	//	public const string TransformsParam = "TRANSFORMS";
		public const string CisLanguageParam = "CIS_LANGUAGE_ID";
		public const string AvForServersParam = "AV_FOR_SERVERS";

		private const string ParamsFormat = "{0}={1}";
	//	private const string TransformsFormat = ":{0}.mst";
		private const string TrueValue = "1";
		private const string FalseValue = "0";
		private const string Divider = " ";

		public Guid[] PackageUids { get; set; }

		public bool SuppressReboot { get; set; }
	
		public bool UninstallIncompatible { get; set; }
		
		public bool Update { get; set; }

		public bool InstallAV { get; set; }

		public bool InstallFW { get; set; }

		public bool IsServerInstall { get; set; }

		public bool? RestoreConfig { get; set; }

		public int Lcid { get; set; }

		public string GetParams()
		{
			if (!InstallFW && !InstallAV)
				throw new ArgumentException("Impossible to install CES without both AV and FW.");

			var result = String.Join(Divider,
				IsServerInstall ? CreateComponentParam(AvForServersParam, true) : String.Empty,
				CreateComponentParam(InstallFWParam, IsServerInstall ? false : InstallFW),
				CreateComponentParam(InstallAVParam, InstallAV),
			//	Lcid != 0 ? CreateParam(TransformsParam, String.Format(TransformsFormat, Lcid)) : String.Empty,
				Lcid != 0 ? CreateParam(CisLanguageParam, Lcid.ToString()) : String.Empty,
				RestoreConfig != null ? CreateComponentParam(RestoreConfigParam, RestoreConfig.Value) : String.Empty);

			return result;
		}

		private static string CreateComponentParam( string componentParam, bool isComponentIncluded )
		{
			return String.Format( ParamsFormat, componentParam, isComponentIncluded ? TrueValue : FalseValue );
		}

		private static string CreateParam(string paramName, string paramValue)
		{
			return String.Format(ParamsFormat, paramName, paramValue);
		}

		internal InstallPackageInfo Clone()
		{
			return new InstallPackageInfo
			{
				PackageUids = PackageUids,
				SuppressReboot = SuppressReboot,
				UninstallIncompatible = UninstallIncompatible,
				InstallAV = InstallAV,
				InstallFW = InstallFW,
				RestoreConfig = RestoreConfig,
				Update = Update,
				Lcid = Lcid,
			};
		}
	}
}
