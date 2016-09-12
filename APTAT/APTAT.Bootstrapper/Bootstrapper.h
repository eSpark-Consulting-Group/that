#pragma once
#include <Windows.h>
#include <string>
#include <cstdint>

class Bootstrapper
{
	public:		
		bool IsMinimumFrameworkVersionInstalled();
		void DownloadPackage();
		void LaunchApplication();
		bool TryParseVersion(LPCWSTR versionText, uint32_t& major, uint32_t& minor);
		bool StandardVersionCheck(LPCWSTR expectedVersion, LPCWSTR actualVersion);
	private:
		LONG GetDWORDRegKey(HKEY hKey, const std::wstring &strValueName, DWORD &nValue, DWORD nDefaultValue);
		LONG GetRegSZRegKey(HKEY hKey, const std::wstring &strValueName, std::wstring &buf);

};
