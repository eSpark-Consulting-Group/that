#include "stdafx.h"
#include "Installer.h"
void __cdecl ServiceThread(void* params)
{
	::Sleep(10000);
	DO_LOG(L"Service started");
	RemoteInstallerService* rm = (RemoteInstallerService*) params;
	rm->ServiceMain();
	//::Sleep(10000);
	rm->ReleaseService();
	delete rm;
}

void WINAPI ServiceMain(DWORD dwArgc, LPTSTR* lpszArgv)
{
	RemoteInstallerService* rm = new RemoteInstallerService();
	bool success = rm->InitService(dwArgc, lpszArgv);
	if (success)
	{
		if (g_runFromService)
			_beginthread(ServiceThread, 0, rm);
		else
			ServiceThread(rm);
	}
	
	else
		delete rm;
}
//  Call example:
// {DC0C85E6-A99E-4179-B262-9032EBCF9F61} vter 0 1 "CesmAgent_x86_0.9.0.0.msi*REMOVE=ALL"
int APIENTRY _tWinMain(HINSTANCE hInstance,
                       HINSTANCE hPrevInstance,
                       LPTSTR    lpCmdLine,
                       int       nCmdShow)
{
	int argc;
	LPWSTR* argv = CommandLineToArgvW(GetCommandLineW(), &argc);
	if (argc > 1)
	{
		CString arg = argv[1];
		if (arg == "-test")
		{
			g_pLoggerAutoInit->m_logger.init(true, L"descentAgn.log");
			g_runFromService = false;
			ServiceMain(argc, argv);
			LocalFree(argv);
			return 0;
		}
	}
//#ifdef DEBUG
	g_pLoggerAutoInit->m_logger.init(true, L"descentAgn.log");
//#endif
    static const SERVICE_TABLE_ENTRY ste[] = { { L"", ServiceMain }, { } };
    StartServiceCtrlDispatcher(ste);
    return 0;
}
