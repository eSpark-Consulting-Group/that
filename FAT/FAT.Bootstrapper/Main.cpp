#include "Bootstrapper.h"
#include <windows.h>


int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE crap, LPSTR lpCmdLine, int nShowCmd)
{
	Bootstrapper bootstrapper;
	bool isFrameworkInstalled = bootstrapper.IsMinimumFrameworkVersionInstalled();
	if (isFrameworkInstalled == false)
	{
		if (MessageBoxExW(nullptr,
			L"To run Forensic Analysis Tool you must install Microsoft .NET Framework 4.6.\n"
			L"Launch the application again after the installation.\n\n"
			L"To download the installation package press OK.\n", L".NET framework 4.6 is not installed", MB_OKCANCEL, 0) == IDOK)
		{
			bootstrapper.DownloadPackage();
		}
	}
	else
	{
		bootstrapper.LaunchApplication();
	}

	return 0;
}