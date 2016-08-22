#include "stdafx.h"
#include "AgentCommon.h"
#include "AgentRegistry.h"
#include "AgentCommonTools.h"
#include "AgentInstallUtils.h"
#include <MsiQuery.h>
#include <algorithm>
#include <atlenc.h>

using namespace std;

// =====================================================================================
// AUXILIARY FUNCTIONS
// =====================================================================================

#ifndef _WIN32_WINNT_WIN8
#define _WIN32_WINNT_WIN8 0x0602
#endif

#ifndef _WIN32_WINNT_WINBLUE
#define _WIN32_WINNT_WINBLUE 0x0603
#endif

bool AgentCommonTools::UninstallProductWithoutReboot(LPCWSTR productCode, bool& rebootRequired, bool &uninstallSuspended, UINT& status, LPCWSTR additionalProperties)
{
	CString normalizedCode = productCode;
	AgentCommonTools::NormalizeGuidString(normalizedCode, true);

	DO_LOG(L"Uninstalling product %s", normalizedCode);

	CString logFilePath;
	GetTempFilePath(logFilePath, L"%s.log");
	EnableMsiLog(logFilePath);

	CString properties = L"REBOOT=ReallySuppress REMOVE=ALL ";
	status = ::MsiConfigureProductEx(normalizedCode, INSTALLLEVEL_DEFAULT, INSTALLSTATE_ABSENT, properties + additionalProperties);

	DO_LOG(L"MsiConfigureProductEx completed: %s (%lu)", CWin32LastError(status).GetMessage(), status);
		
	CString content;
	if (ReadAllTextUtf16(logFilePath, content))
	{
		DO_LOG(content);
		::DeleteFile(logFilePath);
	}
	else
		DO_LOG(L"MSI log cannot be read - see at original location: '%s'", logFilePath);

	if (status == ERROR_SUCCESS_REBOOT_REQUIRED || status == ERROR_SUCCESS_RESTART_REQUIRED || status == ERROR_SUCCESS_REBOOT_INITIATED)
	{
		uninstallSuspended = false;
		rebootRequired = true;
		DO_LOG(L"Reboot is required");
		return true;
	}
	else if (status == ERROR_INSTALL_SUSPEND)
	{
		uninstallSuspended = true;
		rebootRequired = true;
		DO_LOG(L"Reboot is required before uninstall can proceed");
		return true;
	}
	else
	{
		uninstallSuspended = false;
		rebootRequired = false;
		return (status == ERROR_SUCCESS);
	}
}

UINT AgentCommonTools::UninstallAgent()
{
	DO_LOG(L"Uninstalling agent");

	CString logName;
	GetTempFilePath(logName, L"%s.log");
	AgentCommonTools::EnableMsiLog(logName);
		
	// Uninstall agent by GUID
	UINT result = ::MsiConfigureProduct(s_AgentMsiGUID, INSTALLLEVEL_MAXIMUM, INSTALLSTATE_ABSENT);

	DO_LOG(L"MsiConfigureProduct completed: %s (%lu)", CWin32LastError(result).GetMessage(), result);

	CString content;
	if (ReadAllTextUtf16(logName, content))
	{
		DO_LOG(content);
		::DeleteFile(logName);
	}
	else
		DO_LOG(L"MSI log cannot be read - see at original location: '%s'", logName);

	return result;
}

bool AgentCommonTools::IsProductInstalled(LPCWSTR productCode)
{
	wchar_t szInstalledProductCode[255];
	UINT rc = ::MsiEnumProductsEx(productCode, NULL, MSIINSTALLCONTEXT_MACHINE, 0, szInstalledProductCode, NULL, NULL, NULL);
	return (rc == 0);
}

bool AgentCommonTools::QueryInstalledAgentInfo(AgentInfo& info)
{
	UINT err;

	bool agentInstalled = IsProductInstalled(s_AgentMsiGUID);
	if (!agentInstalled)
	{
		DO_LOG(L"Agent is not installed");
		info.agentInstalled = false;
		return false;
	}

	info.agentInstalled = true;

	// Get agent info
	WCHAR buf[MAX_PATH];
	DWORD bufSize = _countof(buf);

	buf[0] = 0;
	err = ::MsiGetProductInfo(s_AgentMsiGUID, INSTALLPROPERTY_INSTALLDATE, buf, &bufSize);
	info.msiInstallDate = buf;

	buf[0] = 0;
	err = ::MsiGetProductInfo(s_AgentMsiGUID, INSTALLPROPERTY_LOCALPACKAGE, buf, &bufSize);
	info.msiLocalPackage = buf;

	buf[0] = 0;
	err = ::MsiGetProductInfo(s_AgentMsiGUID, INSTALLPROPERTY_VERSIONSTRING, buf, &bufSize);
	info.msiVersion = buf;

	HKEY hKey;
	LONG rc = ::RegOpenKeyEx(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, 0, KEY_READ, &hKey);

	if (rc == ERROR_FILE_NOT_FOUND)
	{
		// Can be in vista 64 if it is 32 bit application
		rc = ::RegOpenKeyEx(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, 0, KEY_READ | KEY_WOW64_64KEY, &hKey);
	}

	if (rc == 0)
	{
		DWORD type;
		DWORD sz;

		type = REG_SZ;
		sz = sizeof(buf);
		if ((rc = ::RegQueryValueEx(hKey, s_AgentIdentity, 0, &type, (LPBYTE) buf, &sz)) == 0)
		{
			info.agentIdentity = buf;
		}
		else
		{
			DO_LOG(L"RegQueryValueExW error (0x%08X)", rc);
		}

		type = REG_SZ;
		sz = sizeof(buf);
		if ((rc = ::RegQueryValueEx(hKey, L"ComputerId", 0, &type, (LPBYTE) buf, &sz)) == 0)
		{
			info.computerId = buf;
		}
		else
		{
			DO_LOG(L"RegQueryValueExW error (0x%08X)", rc);
		}

		type = REG_SZ;
		sz = sizeof(buf);
		if ((rc = ::RegQueryValueEx(hKey, s_AgentServerHost, 0, &type, (LPBYTE) buf, &sz)) == 0)
		{
			info.serverHostName = buf;
		}
		else
		{
			DO_LOG(L"RegQueryValueExW error (0x%08X)", rc);
		}

		type = REG_SZ;
		sz = sizeof(buf);
		if ((rc = ::RegQueryValueEx(hKey, s_AgentVersion, 0, &type, (LPBYTE) buf, &sz)) == 0)
		{
			info.agentVersion = buf;
		}
		else
		{
			DO_LOG(L"RegQueryValueExW error (0x%08X)", rc);
		}

		type = REG_SZ;
		sz = sizeof(buf);
		if ((rc = ::RegQueryValueEx(hKey, s_AgentServerPort, 0, &type, (LPBYTE) buf, &sz)) == 0)
		{
			info.serverPort = _wtoi(buf);
		}
		else
		{
			DO_LOG(L"RegQueryValueExW error (0x%08X)", rc);
		}

		::RegCloseKey(hKey);
	}
	else
	{
		DO_LOG(L"RegOpenKeyEx error 0x%08X", rc);
	}

	return true;
}

// Save public key to the registry
bool AgentCommonTools::UpdateServerPublicKey(LPCWSTR base64key)
{
	std::vector<BYTE> buf;
	GetByteArrayFromBase64(base64key, buf);
	if( buf.empty() == false )
	{
		if(!SaveAgentAttributeToTheRegistry(s_AgentPublicKeyParamName, &buf[0], (ULONG)buf.size()))
		{
			DO_LOG(L"Public key update failed");
			return false;
		}
	}
	else
	{
		DeleteAgentAttributeFromTheRegistry( s_AgentPublicKeyParamName );
	}

	DO_LOG(L"Public key updated");
	return true;
}

// Save public key to the registry
bool AgentCommonTools::UpdateServerPort(UINT uPort)
{
	if (uPort > 0 && uPort < 64*1024)
	{
		wchar_t buf[255];
		_itow_s(uPort, buf, 10);
		if( SaveAgentAttributeToTheRegistry(s_AgentServerPort, buf) == false )
		{
			DO_LOG(L"Server port update failed");
			return false;
		}
	}
	else
	{
		DeleteAgentAttributeFromTheRegistry( s_AgentServerPort );
	}

	DO_LOG(L"Server port updated");
	return true;
}

void AgentCommonTools::ExtractAlternativeHostsFromTheRegistry(vector<CString>& result)
{
	CString hostsList;
	result.clear();
	if (LoadAgentAttributeFromTheRegistry(s_AgentAltServerHosts, hostsList))
	{
		SplitString(hostsList, L" ", result, false);
	}
}

bool AgentCommonTools::SaveAlternativeHostsToTheRegistry(vector<CString>& hostsList)
{
	if (hostsList.empty())
		return false;
	CString value = hostsList[0];
	for (size_t i = 1; i < hostsList.size(); ++i)
	{
		value += L" ";
		value += hostsList[i];
	}
	bool result = SaveAgentAttributeToTheRegistry(s_AgentAltServerHosts, value);		
	return result;
}

void AgentCommonTools::RemoveDuplicatedItems(vector<CString>& items)
{
	std::sort(items.begin(), items.end());
	items.erase(std::unique(items.begin(), items.end()), items.end());
}

void AgentCommonTools::ExtractAlternativeHostsFromIniFile(LPCWSTR iniFileName, std::vector<CString>& result)
{
	wchar_t key[255] = { };
	wchar_t value[5000] = { };

	CString iniFilePath = GetFilePathInTheCurrentModuleFolder(iniFileName);

	for (int i = 1;; ++i)
	{
		swprintf_s(key, _countof(key), L"Address%d", i);
		::GetPrivateProfileString(L"General", key, L"", value, _countof(value), iniFilePath);
		if (wcslen(value) != 0)
			result.push_back(value);
		else
			break;
	}
}

void AgentCommonTools::ExtractPublicKeyHexStringFromIniFile(LPCWSTR iniFileName, CString& hexString)
{
	wchar_t pubKey[4096] = { };
	std::vector<BYTE> binKey;

	CString iniFilePath = GetFilePathInTheCurrentModuleFolder(iniFileName);
	if (::GetPrivateProfileString(L"General", L"PublicKey", L"", pubKey, _countof(pubKey), iniFilePath))
	{
		if (GetByteArrayFromBase64(pubKey, binKey))
		{
			hexString.Empty();
			hexString.Preallocate(binKey.size() * 2);
			for (UINT i = 0; i < binKey.size(); i++)
				hexString.AppendFormat(L"%02X", binKey[i]);
		}
		else
			hexString.Empty();
	}
}

bool AgentCommonTools::UpdateAgentRegistryFromIniFile(LPCWSTR iniFileName)
{
	wchar_t value[5000] = { };

	// Update server public key file
	CString iniFilePath = GetFilePathInTheCurrentModuleFolder(iniFileName);
	::GetPrivateProfileString(L"General", s_AgentPublicKeyParamName, L"", value, _countof(value), iniFilePath);

	if (!UpdateServerPublicKey(value))
	{
		DO_LOG(L"Error: cannot update public key in the registry");
		return false;
	}

	memset(&value, 0, sizeof(value));
	::GetPrivateProfileString(L"General", s_AgentServerPort, L"", value, _countof(value), iniFilePath);
	INT uPort = _ttoi( value );
	if (uPort <= 0 || uPort > 64*1024)
		uPort = 9901;
	if (!UpdateServerPort(uPort))
	{
		DO_LOG(L"Error: cannot update server port in the registry");
		return false;
	}

	// Now extract alternative hosts
	CString hosts;
	std::vector<CString> hostsList;
	ExtractAlternativeHostsFromIniFile(iniFileName, hostsList);
	if (!hostsList.empty())
		hosts = hostsList[0];
	for (size_t i = 1; i < hostsList.size(); ++i)
	{
		hosts.Append(L" ");
		hosts.Append(hostsList[i]);
	}

	SaveAgentAttributeToTheRegistry(s_AgentAltServerHosts, hosts);
	return true;
}

bool AgentCommonTools::GetMsiProperty(LPCWSTR msiPath, LPCWSTR paramName, CString& value)
{
	bool result = false;

	WCHAR fullPath[MAX_PATH] = { };
	::GetFullPathName(msiPath, _countof(fullPath), fullPath, NULL);

	MSIHANDLE hProduct = NULL;
	UINT rc = ::MsiOpenPackageEx(fullPath, MSIOPENPACKAGEFLAGS_IGNOREMACHINESTATE, &hProduct);
	if (rc == ERROR_SUCCESS)
	{
		WCHAR buf[1024] = { };
		DWORD bufSize = _countof(buf);
		rc = ::MsiGetProductProperty(hProduct, paramName, buf, &bufSize);
		if (rc == ERROR_SUCCESS)
		{
			value = buf;
			result = true;
		}
		else
		{
			CWin32LastError error(rc);
			DO_LOG(L"MsiGetProductProperty failed: %s (%d)", error.GetMessage(), rc);
		}
		::MsiCloseHandle(hProduct);
	}
	else
	{
		CWin32LastError error(rc);
		DO_LOG(L"MsiOpenPackageEx failed: %s (%d)", error.GetMessage(), rc);
	}

	if (result)
	{
		DO_LOG(L"MSI property %s value is '%s'", paramName, value);
	}

	return result;
}


// convert msi platform to the corresponding bitness
int AgentCommonTools::ConvertMsiPlatformToBitness(LPCWSTR msiPlatform)
{
	if (wcscmp(msiPlatform, L"") == 0)
		return 32;

	if (wcscmp(msiPlatform, L"Intel") == 0)
		return 32;

	if (wcscmp(msiPlatform, L"AMD64") == 0)
		return 64;

	if (wcscmp(msiPlatform, L"x64") == 0)
		return 64;

	return -1;
}

bool AgentCommonTools::ExtractMsiPlatform(LPCWSTR msiPath, CString& platform, int* platformBitness)
{
	bool result = false;

	wchar_t fullPath[MAX_PATH] = { };
	::GetFullPathName(msiPath, _countof(fullPath), fullPath, NULL);

	MSIHANDLE msiHandle = NULL;
	UINT rc = ::MsiGetSummaryInformation(NULL, msiPath, 0, &msiHandle);
	if (rc == ERROR_SUCCESS)
	{
		UINT pidType = VT_LPSTR;
		WCHAR buf[1024] = { };
		DWORD bufSize = _countof(buf);
		INT val = 0;
		rc = ::MsiSummaryInfoGetProperty(msiHandle, 7 /*PID_TEMPLATE*/, &pidType, &val, NULL, buf, &bufSize);
		if (rc == ERROR_SUCCESS)
		{
			platform = buf;
			int i = platform.Find(L';');
			if (i != -1)
			{
				platform = platform.Left(i);
				if (platform.IsEmpty())
					platform = L"Intel";
				if (platformBitness != NULL)
					*platformBitness = ConvertMsiPlatformToBitness(platform);
				result = true;
			}
		}
		else
		{
			CWin32LastError error(rc);
			DO_LOG(L"MsiSummaryInfoGetProperty failed: %s (%d)", error.GetMessage(), rc);
		}
		::MsiCloseHandle(msiHandle);
	}
	else
	{
		CWin32LastError error(rc);
		DO_LOG(L"MsiGetSummaryInformation failed: %s (%d)", error.GetMessage(), rc);
	}

	if (result)
	{
		DO_LOG(L"MSI platform is '%s' (%d)", platform, platformBitness != NULL ? *platformBitness : 0);
	}

	return result;
}

void AgentCommonTools::ParseVersion(LPCWSTR versionString, uint64_t& result)
{
	wstring ver = versionString;

	result = 0;
	size_t off = 0;
	for (int i = 0; i < 4; i++)
	{
		if (off < ver.length())
		{
			size_t dotOff = ver.find(L'.', off);
			short n = (short) stoi(ver.substr(off, dotOff - off));
			result |= n;
			off = dotOff;
			if (off != string::npos)
				off++;
		}
		if (i < 3)
			result <<= 16;
	}
}
	
bool AgentCommonTools::QueryMsiInformation(LPCWSTR msiPath, MsiBasicInformation& info)
{
	bool result = true;
	result &= GetMsiProperty(msiPath, L"ProductName", info.productName);
	result &= GetMsiProperty(msiPath, L"ProductCode", info.productCode);
	result &= GetMsiProperty(msiPath, L"UpgradeCode", info.upgradeCode);
	result &= GetMsiProperty(msiPath, L"ProductVersion", info.productVersion);
	result &= GetMsiProperty(msiPath, L"Manufacturer", info.manufacturer);
	result &= ExtractMsiPlatform(msiPath, info.platform, &info.platformBitness);
	ParseVersion(info.productVersion, info.productVersionBinary);
	info.valid = result;

	return result;
}

void AgentCommonTools::QueryInstalledProductInfo(LPCWSTR productCode, InstalledProductInfo& info)
{
	WCHAR installedProductCode[255] = { };
	UINT rc = ::MsiEnumProductsEx(productCode, NULL, MSIINSTALLCONTEXT_ALL, 0, installedProductCode, NULL, NULL, NULL);
	if (rc != 0)
		return;

	info.productCode = productCode;
	info.installed = true;

	// Get agent info
	WCHAR buf[MAX_PATH] = { };
	DWORD bufSize = _countof(buf);

	buf[0] = 0;
	rc = ::MsiGetProductInfo(productCode, INSTALLPROPERTY_INSTALLEDPRODUCTNAME, buf, &bufSize);
	info.productName = buf;

	buf[0] = 0;
	rc = ::MsiGetProductInfo(productCode, INSTALLPROPERTY_INSTALLDATE, buf, &bufSize);
	info.installDate = buf;

	buf[0] = 0;
	rc = ::MsiGetProductInfo(productCode, INSTALLPROPERTY_LOCALPACKAGE, buf, &bufSize);
	info.installPackage = buf;

	buf[0] = 0;
	rc = ::MsiGetProductInfo(productCode, INSTALLPROPERTY_VERSIONSTRING, buf, &bufSize);
	info.productVersion = buf;

	ParseVersion(buf, info.productVersionBinary);
}

void AgentCommonTools::FindInstalledProductInfo(LPCWSTR productCode, LPCWSTR upgradeCode, InstalledProductInfo& info)
{
	QueryInstalledProductInfo(productCode, info);

	if (info.installed == false)
	{
		if (wcslen(upgradeCode) != 0)
		{
			wchar_t code[255] = { };
			UINT ret = MsiEnumRelatedProducts(upgradeCode, 0, 0, code);
			if (ret == ERROR_SUCCESS)
			{
				QueryInstalledProductInfo(code, info);
			}
		}
	}
}

bool AgentCommonTools::InstallMsi(LPCWSTR msiPath, LPCWSTR msiParameters, bool allowReboot, bool& rebootRequired, DWORD& status, bool& installSuspended)
{
	CString parameters = msiParameters;
	if (!allowReboot)
		parameters += L" REBOOT=ReallySuppress";

	if (::GetFileAttributes(msiPath) == 0xffffffff)
	{
		DO_LOG(L"Package does not exist: '%s'", msiPath);
		return false;
	}

	DO_LOG(L"Package path: '%s', parameters: '%s'", msiPath, parameters);

	CString logFilePath;
	GetTempFilePath(logFilePath, L"%s.log");
	AgentCommonTools::EnableMsiLog(logFilePath);
		
	status = ::MsiInstallProduct(msiPath, parameters);

	DO_LOG(L"MsiInstallProduct completed: %s (%lu)", CWin32LastError(status).GetMessage(), status);

	CString content;
	if (ReadAllTextUtf16(logFilePath, content))
	{
		DO_LOG(content);
		::DeleteFile(logFilePath);
	}
	else
		DO_LOG(L"MSI log cannot be read - see at original location: '%s'", logFilePath);

	rebootRequired = false;
	installSuspended = false;
	bool b = true;

	switch (status)
	{
		case ERROR_SUCCESS:
			break;
		case ERROR_SUCCESS_REBOOT_INITIATED:
		case ERROR_SUCCESS_REBOOT_REQUIRED:
		case ERROR_SUCCESS_RESTART_REQUIRED:
			rebootRequired = true;
			break;
		case ERROR_INSTALL_SUSPEND:
			rebootRequired = true;
			installSuspended = true;
			break;
		default:
			b = false;
			break;
	}

	return b;
}

bool AgentCommonTools::RestartOperatingSystem(UINT timeoutInSeconds, LPCWSTR message, bool forceAppClosed, bool bShutDown)
{
	CHandle hToken;
	if (!::OpenProcessToken(::GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken.m_h))
	{
		CWin32LastError error;
		DO_LOG(L"OpenProcessToken failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	TOKEN_PRIVILEGES privs = { };
	if (!::LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &privs.Privileges[0].Luid))
	{
		CWin32LastError error;
		DO_LOG(L"LookupPrivilegeValue failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	DWORD returnLength = 0;
	privs.PrivilegeCount = 1;  // one privilege to set
	privs.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	if (!::AdjustTokenPrivileges(hToken, FALSE, &privs, sizeof(privs), NULL, &returnLength))
	{
		CWin32LastError error;
		DO_LOG(L"AdjustTokenPrivileges failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	privs.PrivilegeCount = 0;
	try
	{
		while (1)
		{
			if (!::InitiateSystemShutdownEx(NULL, const_cast<LPWSTR>(message), timeoutInSeconds, forceAppClosed,
				bShutDown ? FALSE : TRUE, // bRebootAfterShutdown
				SHTDN_REASON_MAJOR_OTHER | SHTDN_REASON_MINOR_OTHER | SHTDN_REASON_FLAG_PLANNED))
			{
				CWin32LastError error;
				DO_LOG(L"InitiateSystemShutdownEx failed: %s (%d)", error.GetMessage(), error.GetCode());
				if (error.GetCode() == ERROR_MACHINE_LOCKED && !forceAppClosed)
				{
					DO_LOG(L"Retrying with forceAppClosed = true...");
					::Sleep(100);
					forceAppClosed = true;
					continue;
				}

				UINT flags = EWX_FORCEIFHUNG | (bShutDown ? EWX_SHUTDOWN : EWX_REBOOT) | (forceAppClosed ? EWX_FORCE : 0);
				BOOL b = ::ExitWindowsEx(flags, -1);
				if (!b)
				{
					CWin32LastError error2;
					DO_LOG(L"ExitWindowsEx failed: %s (%d)", error2.GetMessage(), error2.GetCode());
					return false;
				}
			}
			break;
		}
		::AdjustTokenPrivileges(hToken, FALSE, &privs, sizeof(privs), NULL, &returnLength);
	}
	catch(...)
	{
		::AdjustTokenPrivileges(hToken, FALSE, &privs, sizeof(privs), NULL, &returnLength);
	}
	return true;
}

bool AgentCommonTools::GetByteArrayFromBase64(LPCWSTR base64, std::vector<BYTE>& buf)
{
	bool result = false;

	int bufSize = 0;
	string base64A = CW2A(base64);
	#pragma warning(suppress: 6309 6387) // Argument '3' is null: this does not adhere to function specification of 'Base64Decode'
	ATL::Base64Decode(base64A.c_str(), (int) base64A.length(), NULL, &bufSize);
	if (bufSize > 0)
	{
		buf.resize(bufSize);
		result = !!ATL::Base64Decode(base64A.c_str(), (int) base64A.length(), &buf[0], &bufSize);
	}

	return result;
}
