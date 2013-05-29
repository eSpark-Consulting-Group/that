using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using APTAT.Core;

namespace CRM.SrvCore
{
	public static class MsiTools
	{
		const int LOCALE_SENGLANGUAGE = 0x00001001;
		[DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
		private static extern int GetLocaleInfo( int Locale, int LCType, StringBuilder lpLCData, int cchData );

		private class MsiInfo
		{
			public Guid productCode;
			public Guid upgradeCode;
			public string productVersion;
			public Dictionary<string /*param*/, string /*value*/> _cachedValues = new Dictionary<string, string>();
		}

		private static Dictionary<string /*MsiPath*/, MsiInfo> _cachedPackages = new Dictionary<string, MsiInfo>();

		private static IntPtr OpenMsiPackage( string FullPath )
		{
			IntPtr hProduct;
			uint res;
			IntPtr hWnd = IntPtr.Zero;
			MsiNative.MsiSetInternalUI( MsiNative.UILevel.INSTALLUILEVEL_NONE, ref hWnd );

			if ( 0 != ( res = MsiNative.MsiOpenPackageEx( FullPath,
				MsiNative.OpenFlags.MSIOPENPACKAGEFLAGS_IGNOREMACHINESTATE,
				out hProduct ) ) )
			{
				throw new Win32Exception( ( int )res );
			}
			return hProduct;
		}

		private static string ExtractMsiProperty( IntPtr hProduct, string property )
		{
			string value = null;
			uint len = 512;
			uint res;
			do
			{
				StringBuilder buff = new StringBuilder( ( int )len );
				res = MsiNative.MsiGetProductProperty( hProduct, property, buff, ref len );
				if ( 0 == res )
				{
					value = buff.ToString();
					break;
				}
				len *= 2;
			} while ( res == MsiNative.ERROR_MORE_DATA );
			if ( 0 != res )
			{
				throw new Win32Exception( ( int )res );
			}
			return value;
		}

		private static void CloseMsiPackage( IntPtr hProduct )
		{
			MsiNative.MsiCloseHandle( hProduct );
		}

		private static void ExtractMsiPropertiesUncached( string path,
			string[] queryProperties,

			out string[] queryValues,
			out Guid productCode,
			out Guid upgradeCode,
			out string productVersion,
			MsiInfo infoToUpdate
			)
		{
			IntPtr hProduct = OpenMsiPackage( path );
			try
			{
				productCode = new Guid( ExtractMsiProperty( hProduct, MsiNative.INSTALLPROPERTY_PRODUCTCODE ) );
				string strUpgradeCode = ExtractMsiProperty( hProduct, MsiNative.INSTALLPROPERTY_UPGRADECODE );
				if ( !string.IsNullOrEmpty( strUpgradeCode ) )
					upgradeCode = new Guid( strUpgradeCode );
				else upgradeCode = Guid.Empty;
				productVersion = ExtractMsiProperty( hProduct, MsiNative.INSTALLPROPERTY_PRODUCTVERSION );

				// cache values
				infoToUpdate.productCode = productCode;
				infoToUpdate.productVersion = productVersion;
				infoToUpdate.upgradeCode = upgradeCode;

				List<string> values = new List<string>();
				foreach ( string prop in queryProperties )
				{
					string result = ExtractMsiProperty( hProduct, prop );
					values.Add( result );

					// cache
					infoToUpdate._cachedValues[prop] = result;
				}

				queryValues = values.ToArray();
			}
			finally
			{
				MsiNative.MsiCloseHandle( hProduct );
			}
		}

		public static string GetMsiProperty(string path, string propertyName)
		{
			IntPtr hProduct = OpenMsiPackage(path);
			try
			{
				return ExtractMsiProperty(hProduct, propertyName);
			}
			finally
			{
				MsiNative.MsiCloseHandle(hProduct);
			}
		}

		public static void ExtractMsiProperties( string path,
			string[] queryProperties,
			out string[] queryValues,
			out Guid productCode,
			out Guid upgradeCode,
			out string productVersion
			)
		{
			string FullPath = Path.GetFullPath( path );
			MsiInfo infoToUpdate;

			lock ( _cachedPackages )
			{
				// First, try to find package
				if ( !_cachedPackages.TryGetValue( FullPath, out infoToUpdate ) )
				{
					// Not found. Create it.
					infoToUpdate = new MsiInfo();
					_cachedPackages[FullPath] = infoToUpdate;
					ExtractMsiPropertiesUncached( FullPath, queryProperties, out queryValues, out productCode,
						 out upgradeCode, out productVersion, infoToUpdate );
				}
				else
				{
					// OK, package was found.
					// Try to search for all the requested values
					List<string> values = new List<string>();
					foreach ( string prop in queryProperties )
					{
						string result;
						if ( !infoToUpdate._cachedValues.TryGetValue( prop, out result ) )
						{
							// Property not found.. 
							break;
						}
						values.Add( result );
					}

					// OK, all was found
					queryValues = values.ToArray();
					productCode = infoToUpdate.productCode;
					upgradeCode = infoToUpdate.upgradeCode;
					productVersion = infoToUpdate.productVersion;
				}
			}
		}

		public struct MsiSummary
		{
			public string[] Platforms;
			public int[] LanguageIdentifiers; // LCIDs
		}

		public static string ConvertLcidToString( int lcid )
		{
			StringBuilder sb = new StringBuilder();
			sb.EnsureCapacity( 200 );
			GetLocaleInfo( lcid, LOCALE_SENGLANGUAGE, sb, sb.Capacity );
			return sb.ToString();
		}

		public static bool GetPackageSummary( string path, out MsiSummary summary )
		{
			summary = new MsiSummary();

			IntPtr infoHandle;
			IntPtr dbHandle = new IntPtr( 0 );
			uint result = MsiNative.MsiGetSummaryInformation( dbHandle, path, 0, out infoHandle );

			if ( result != 0 )
				return false;

			try
			{
				IntPtr pidType = new IntPtr( 30 ); // VT_LPSTR
				IntPtr intVal = new IntPtr( 0 );
				IntPtr nullVal = new IntPtr( 0 );
				StringBuilder sb = new StringBuilder();
				var fileTime = new System.Runtime.InteropServices.ComTypes.FILETIME();

				sb.EnsureCapacity( 500 );
				int summarySize = sb.Capacity;

				result = MsiNative.MsiSummaryInfoGetProperty( infoHandle, 7 /*PID_TEMPLATE*/,
					out pidType, out intVal, out fileTime,
					sb, ref summarySize );

				if ( result != 0 )
					return false;

				string summaryString = sb.ToString();
				string[] splitted = summaryString.Split( ';' );
				string msiPlatforms = splitted[0];
				string msiLanguages = splitted[1];

				// Extract platforms
				splitted = msiPlatforms.Split( ',' );
				List<string> platforms = new List<string>();
				foreach ( string token in splitted )
				{
					platforms.Add( token );
				}
				summary.Platforms = platforms.ToArray();

				var lcids = new List<int>();

				// Extract languages
				splitted = msiLanguages.Split( ',' );

				for ( int i = 0; i < splitted.Length; ++i )
				{
					var token = splitted[i];
					var value = Int32.Parse(token);
					if (!lcids.Contains(value))
						lcids.Add(value);
				}

				sb.EnsureCapacity(500);
				summarySize = sb.Capacity;

				// Try to fetch LCIDs from the Keywords property, which is expected to be in format:
				// Installer;<comma-separated-list-of-lcids>
				result = MsiNative.MsiSummaryInfoGetProperty(infoHandle, 5 /* PID_KEYWORDS */,
					out pidType, out intVal, out fileTime,
					sb, ref summarySize);
				do 
				{
					if (result != 0)
						break;
					var keywordsContent = sb.ToString();
					var keywords = keywordsContent.Split(';');
					if (keywords.Length != 2 || keywords[0] != "Installer")
						break;
					var expectedLcids = keywords[1].Split(',');
					foreach (var lcid in expectedLcids)
					{
						int value = 0;
						if (Int32.TryParse(lcid, out value) && !lcids.Contains(value))
							lcids.Add(value);
					}
				}
				while (false);

				summary.LanguageIdentifiers = lcids.ToArray();

				return true;
			}
			finally
			{
				MsiNative.MsiCloseHandle( infoHandle );
			}
		}
	}
}
