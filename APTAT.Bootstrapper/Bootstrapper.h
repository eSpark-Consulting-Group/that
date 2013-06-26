#pragma once
#include <Windows.h>
#include <string>

class Bootstrapper
{
	public:
		Bootstrapper();
		~Bootstrapper();

		
		bool IsMinimumFrameworkVersionInstalled();
		void DownloadPackage();
		void LaunchApplication();
		bool PrepareFonts();
	private:
		bool FileExists(LPCTSTR szPath);
		bool InstallFont(LPCTSTR font);
		LONG GetDWORDRegKey(HKEY hKey, const std::wstring &strValueName, DWORD &nValue, DWORD nDefaultValue);

};