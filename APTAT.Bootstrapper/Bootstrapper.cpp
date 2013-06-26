#include "Bootstrapper.h"
#include <tchar.h>

const wchar_t PATH[] = L"Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client";
const wchar_t KEY[]  = L"Install";
const wchar_t FRAMEWORK_DOWNLOAD_LINK[] = L"http://go.microsoft.com/fwlink/?linkid=182804";
const wchar_t APP[]  = L"APTAT.Application.exe";
const wchar_t FONT1[] = L"Fonts\\segoeui.ttf";
const wchar_t FONT2[] = L"Fonts\\segoeuil.ttf";
const wchar_t FONT3[] = L"Fonts\\seguisb.ttf";


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
}

bool  Bootstrapper::FileExists(LPCTSTR szPath)
{
  DWORD dwAttrib = GetFileAttributes(szPath);

  return (dwAttrib != INVALID_FILE_ATTRIBUTES && 
         !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

bool  Bootstrapper::InstallFont(LPCTSTR font)
{
	TCHAR path[MAX_PATH + MAX_PATH];
	GetWindowsDirectory(path, MAX_PATH);

	LPCWSTR fontpath = lstrcat(path, L"\\");
	fontpath = lstrcat(path, font);

	if(FileExists(fontpath))
	{
		return true;
	}

	 bool result = CopyFileEx(font, fontpath, nullptr, nullptr, FALSE, 0);
	 DWORD err = GetLastError();
	 return result;
}

bool Bootstrapper::PrepareFonts()
{	
	bool font1 = InstallFont(FONT1);
	bool font2 = InstallFont(FONT2);
	bool font3 = InstallFont(FONT3);

	return font1 && font2 && font3;
}

