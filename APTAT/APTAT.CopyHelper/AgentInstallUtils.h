#pragma once
#include <PowrProf.h>
#include <Msi.h>
#include <atltime.h>
#include "BaseTypes.h"


// =====================================================================================
// AUXILIARY FUNCTIONS
// =====================================================================================

namespace AgentCommonTools
{
	struct AgentInfo
	{
		CString msiInstallDate;
		CString msiLocalPackage;
		CString msiVersion;
		CString agentIdentity;
		CString serverHostName;
		CString agentVersion;
		CString computerId;
		UINT serverPort;
		bool agentInstalled;

		AgentInfo():
			serverPort(0),
			agentInstalled(false)
		{ }
	};

	struct InstalledProductInfo
	{
		CString productVersion;
		CString installDate;
		CString installPackage;
		CString productCode;
		CString productName;
		uint64_t productVersionBinary;
		bool installed;

		InstalledProductInfo():
			productVersionBinary(0),
			installed(false)
		{ }
	};

	struct MsiBasicInformation
	{
		CString productCode;
		CString upgradeCode;
		CString productVersion;
		CString manufacturer;
		CString productName;
		CString platform;
		uint64_t productVersionBinary;
		int platformBitness;
		bool valid;

		MsiBasicInformation():
			productVersionBinary(0),
			platformBitness(-1),
			valid(false)
		{ }
	};

}