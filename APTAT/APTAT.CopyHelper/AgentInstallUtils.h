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

	void EnableMsiLog(LPCWSTR logFilePath);
	void EnableMsiLog();
	void MakeWindowsInstallerQuiet();
	bool IsProductInstalled(LPCWSTR productCode);
	bool UninstallProductWithoutReboot(LPCWSTR productCode_, bool& rebootRequired, bool &uninstallSuspended, UINT& status, LPCWSTR additionalPropertis = L"");
	UINT UninstallAgent();

	bool QueryInstalledAgentInfo(AgentInfo& info);
	bool GetByteArrayFromBase64(LPCWSTR base64, std::vector<BYTE>& result);
	bool UpdateServerPublicKey(LPCWSTR base64key);
	bool UpdateServerPort(UINT uPort);

	void ExtractAlternativeHostsFromTheRegistry(std::vector<CString>& result);
	bool SaveAlternativeHostsToTheRegistry(std::vector<CString>& altHosts);
	void RemoveDuplicatedItems( std::vector<CString>& vec );
	void ExtractAlternativeHostsFromIniFile(LPCWSTR iniFileName, std::vector<CString>& result);
	void ExtractPublicKeyHexStringFromIniFile(LPCWSTR iniFileName, CString& hexString);
	bool UpdateAgentRegistryFromIniFile(LPCWSTR iniFileName);

	bool GetMsiProperty(LPCWSTR msiPath, LPCWSTR paramName, CString& value);
	int ConvertMsiPlatformToBitness(LPCWSTR msiPlatform);
	bool ExtractMsiPlatform(LPCWSTR msiPath, CString& platform, int* platformBitness);
	bool QueryMsiInformation(LPCWSTR msiPath, MsiBasicInformation& info);
	void QueryInstalledProductInfo(LPCWSTR productCode, InstalledProductInfo& productInfo);
	void FindInstalledProductInfo(LPCWSTR productCode, LPCWSTR upgradeCode, InstalledProductInfo& info);
	void ParseVersion(LPCWSTR versionString, uint64_t& result);

	bool InstallMsi(LPCWSTR msiPath, LPCWSTR msiParameters, bool allowReboot, bool& rebootRequired, DWORD& status, bool& installSuspended);
	bool RestartOperatingSystem(UINT timeoutInSeconds, LPCWSTR message, bool forceAppClosed, bool bShutDown);
	bool SuspendComputer();
	bool HibernateComputer();
}

inline void AgentCommonTools::EnableMsiLog(LPCWSTR logFilePath)
{
	::MsiEnableLog(INSTALLLOGMODE_VERBOSE, logFilePath, INSTALLLOGATTRIBUTES_APPEND);
}

inline void AgentCommonTools::EnableMsiLog()
{
	wchar_t buf[MAX_PATH] = { };
	GetTempPath(_countof(buf), buf);
	CString path(buf);
	path += L"\\cesmagent_msi.log";
	EnableMsiLog(path);
}

inline void AgentCommonTools::MakeWindowsInstallerQuiet()
{
	::MsiSetInternalUI(INSTALLUILEVEL_NONE, 0);
	EnableMsiLog();
}

inline bool AgentCommonTools::SuspendComputer()
{
	bool result = !!::SetSuspendState(FALSE, TRUE, FALSE);
	return result;
}

inline bool AgentCommonTools::HibernateComputer()
{
	bool result = !!::SetSuspendState(TRUE, TRUE, FALSE);
	return result;
}
