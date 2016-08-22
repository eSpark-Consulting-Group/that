#include "Bootstrapper.h"
#include <tchar.h>
#include <Shlobj.h>
#include <cstdint>
#include <regex>
#include "AutoHandle.h"

const wchar_t PATH[] = L"Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client";
const wchar_t KEY_INSTALL[]  = L"Install";
const wchar_t KEY_VERSION[] = L"Version";
const wchar_t FRAMEWORK_DOWNLOAD_LINK[] = L"https://go.microsoft.com/fwlink/?LinkId=225702";
const wchar_t APP[]  = L"UnknownFileHunter.exe";
const wchar_t ExpectedVersion[] = L"4.5";

const wchar_t FONT_REGISTRY_KEY[] = L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts";
const wchar_t FONT1[] = L"segoeui.ttf";
const wchar_t FONT2[] = L"segoeuil.ttf";
const wchar_t FONT3[] = L"seguisb.ttf";
const wchar_t FONTS_FOLDER[] = L"Fonts";


Bootstrapper::Bootstrapper()
{
}

Bootstrapper::~Bootstrapper()
{
}

bool Bootstrapper::IsMinimumFrameworkVersionInstalled()
{
	HKEY hKey;
	LONG lRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, PATH, 0, KEY_READ, &hKey);
	bool bExistsAndSuccess (lRes == ERROR_SUCCESS);
	
	if(bExistsAndSuccess == false)
	{
		RegCloseKey(hKey);
		return false;
	}

	DWORD installed = 0;
	if(GetDWORDRegKey(hKey, KEY_INSTALL, installed, 0) != ERROR_SUCCESS)
	{
		RegCloseKey(hKey);
		return false;
	}


	std::wstring value;
	if (GetRegSZRegKey(hKey, KEY_VERSION, value) != ERROR_SUCCESS)
	{
		RegCloseKey(hKey);
		return false;
	}
	if (StandardVersionCheck(ExpectedVersion, value.c_str()))
	{
		RegCloseKey(hKey);
		return true;
	}
	RegCloseKey(hKey);
	return false;
}

bool Bootstrapper::StandardVersionCheck(LPCWSTR expectedVersion, LPCWSTR actualVersion)
{
	// expectedVersion is policy version
	// actualVersion is installed version

	uint32_t expectedMajor = 0;
	uint32_t expectedMinor = 0;
	if (expectedVersion != NULL && wcslen(expectedVersion) > 0 && !TryParseVersion(expectedVersion, expectedMajor, expectedMinor))
	{
		return false;
	}

	uint32_t actualMajor = 0;
	uint32_t actualMinor = 0;

	if (!TryParseVersion(actualVersion, actualMajor, actualMinor))
	{
		return false;
	}
	bool result =
		(actualMajor > expectedMajor) ||
		(actualMajor == expectedMajor && actualMinor >= expectedMinor);

	return result;
}

bool Bootstrapper::TryParseVersion(LPCWSTR versionText, uint32_t& major, uint32_t& minor)
{
	if (versionText == NULL || versionText[0] == L'\0')
		return false;

	static std::wregex versionPattern(L"^(\\d+)\\.(\\d+)", std::regex_constants::syntax_option_type::optimize);
	std::wcmatch versionMatch;
	bool parsed = regex_search(versionText, versionMatch, versionPattern);
	if (!parsed)
		return false;

	major = _ttoi(versionMatch[1].str().c_str());
	minor = _ttoi(versionMatch[2].str().c_str());
	return true;
}


LONG Bootstrapper::GetDWORDRegKey(HKEY hKey, const std::wstring &strValueName, DWORD &nValue, DWORD nDefaultValue)
{
    nValue = nDefaultValue;
    DWORD dwBufferSize(sizeof(DWORD));
    DWORD nResult(0);
    LONG nError = ::RegQueryValueEx(hKey,
        strValueName.c_str(),
        0,
        NULL,
        reinterpret_cast<LPBYTE>(&nResult),
        &dwBufferSize);
    if (ERROR_SUCCESS == nError)
    {
        nValue = nResult;
    }
    return nError;
}

LONG Bootstrapper::GetRegSZRegKey(HKEY hKey, const std::wstring &strValueName, std::wstring &buf)
{
	DWORD BufferSize = 8192;

	CAutoHandle<PWCHAR> value(BufferSize * sizeof(WCHAR));

	DWORD dwRet;
	DWORD dwType = REG_SZ;
	
	dwRet = RegQueryValueEx(hKey,
		strValueName.c_str(),
		NULL,
		&dwType,
		(LPBYTE)(PWCHAR)value,
		&BufferSize);
	if (dwRet == ERROR_SUCCESS)
		buf = std::wstring(value);
	return dwRet;
}

void Bootstrapper::DownloadPackage()
{
	ShellExecute(NULL, L"open", FRAMEWORK_DOWNLOAD_LINK, NULL, NULL, SW_SHOWNORMAL);
}

void Bootstrapper::LaunchApplication()
{
	STARTUPINFO si = {sizeof(si)};
    PROCESS_INFORMATION pi;

	LPWSTR szCmdline = _tcsdup(APP);
	
	CreateProcess(NULL,szCmdline , NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);

	free(szCmdline);

	WaitForSingleObject(pi.hProcess, INFINITE);
}

bool  Bootstrapper::FileExists(LPCTSTR szPath)
{
  DWORD dwAttrib = GetFileAttributes(szPath);

  return (dwAttrib != INVALID_FILE_ATTRIBUTES && 
         !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

bool  Bootstrapper::InstallFont(LPCTSTR font, LPCTSTR registryKey)
{
	TCHAR fontFolderPathBuffer[MAX_PATH];
	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_FONTS, NULL, 0,  fontFolderPathBuffer)) == false) 
	{
		return false;
	}

	TCHAR fontPathBuffer[MAX_PATH];
	wcscpy_s(fontPathBuffer, fontFolderPathBuffer);

	LPCWSTR fontTargetPath = lstrcat(fontPathBuffer, L"\\");
	fontTargetPath = lstrcat(fontPathBuffer, font);
	if(FileExists(fontTargetPath))
	{
		return true;
	}

	TCHAR sourcePathBuffer[MAX_PATH * 2];
	wcscpy_s(sourcePathBuffer, FONTS_FOLDER);

	LPCWSTR xcopyCmdLine = lstrcat(sourcePathBuffer, L"\\");
	xcopyCmdLine = lstrcat(sourcePathBuffer, font);
	xcopyCmdLine = lstrcat(sourcePathBuffer, L" ");
	xcopyCmdLine = lstrcat(sourcePathBuffer, fontFolderPathBuffer);
	
	ShellExecute( NULL, L"open", L"xcopy", xcopyCmdLine, NULL, SW_HIDE);

	HKEY   hkey;
	DWORD  dwDisposition;
	DWORD dwType, dwSize;
	LONG r = RegOpenKeyEx(HKEY_LOCAL_MACHINE, FONT_REGISTRY_KEY, 0, KEY_WRITE | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS, &hkey);
	if( r == ERROR_SUCCESS)
	{	
		dwType = REG_SZ;
		dwSize = (_tcslen(font) + 1) * sizeof(TCHAR);

		RegSetValueEx(hkey, registryKey, 0, dwType,  (LPBYTE)font, dwSize);
		RegCloseKey(hkey);

		return true;
	}

	return false;
}

bool Bootstrapper::PrepareFonts()
{	
	bool font1 = InstallFont(FONT1, L"Segoe UI (TrueType)");
	bool font2 = InstallFont(FONT2, L"Segoe UI Light (TrueType)");
	bool font3 = InstallFont(FONT3, L"Segoe UI SemiBold (TrueType)");

	return font1 && font2 && font3;
}

