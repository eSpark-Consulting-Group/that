#include "Bootstrapper.h"
#include <tchar.h>
#include <Shlobj.h>

const wchar_t PATH[] = L"Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client";
const wchar_t KEY[]  = L"Install";
const wchar_t FRAMEWORK_DOWNLOAD_LINK[] = L"http://go.microsoft.com/fwlink/?linkid=182804";
const wchar_t APP[]  = L"UnknownFileHunter.exe";

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
	if(GetDWORDRegKey(hKey, KEY, installed, 0) != ERROR_SUCCESS)
	{
		RegCloseKey(hKey);
		return false;
	}

	RegCloseKey(hKey);
	return installed == 1;
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

