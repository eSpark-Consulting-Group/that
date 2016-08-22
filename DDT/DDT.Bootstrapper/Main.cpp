#include "Bootstrapper.h"
#include <windows.h>


int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE crap, LPSTR lpCmdLine, int nShowCmd)
{
	Bootstrapper bootstrapper;
	bool isFrameworkInstalled = bootstrapper.IsMinimumFrameworkVersionInstalled();
	if (isFrameworkInstalled == false)
	{
		if (MessageBoxExW(nullptr,
			L"To run APTAT tool you must install Microsoft .NET Framework 4.6.\n"
			L"Launch application again after the installation.\n\n"
			L"To download installation package press OK.\n", L".NET framework 4.6 is not installed", MB_OKCANCEL, 0) == IDOK)
		{
			bootstrapper.DownloadPackage();
		}
	}
	else
	{
		bool ok = bootstrapper.PrepareFonts();
		if (ok == false)
		{
			MessageBoxExW(nullptr, L"Fonts are not installed", L"Fonts", MB_OK, 0);
		}
		bootstrapper.LaunchApplication();
	}

	return 0;
}