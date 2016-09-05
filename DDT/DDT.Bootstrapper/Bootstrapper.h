#pragma once
#include <Windows.h>
#include <string>
#include <cstdint>

class Bootstrapper
{
public:
	bool IsMinimumFrameworkVersionInstalled() const;
	static void DownloadPackage();
	static void LaunchApplication();
private:
	static LONG GetDWORDRegKey(HKEY hKey, const std::wstring &strValueName, DWORD &nValue, DWORD nDefaultValue);
	static LONG GetRegSZRegKey(HKEY hKey, const std::wstring &strValueName, std::wstring &buf);
	bool TryParseVersion(LPCWSTR versionText, uint32_t& major, uint32_t& minor) const;
	bool StandardVersionCheck(LPCWSTR expectedVersion, LPCWSTR actualVersion) const;

};
