#include "stdafx.h"
#include "Installer.h"
#include "AgentSslSocketTools.h"
#include "InstallReporter.h"

using namespace AgentCommonTools;
bool g_runFromService = true;

/** 
* extractParametersFromPath Extracts Path and Parameters from string.
* string must be in the following format: Path*Parameters
* 
* @param Source Source string
* @param Path result Path
* @param Parameters result Parameters
*/
void RemoteInstallerService::extractParametersFromPath(LPCWSTR source, CString& path, CString& parameters)
{
    parameters = L"";
    path = L"";
    CString s = source;

    int n = s.Find(L"*");
    if (n == -1)
    {
        path = s;
    }
    else
    {
        path = s.Mid(0, n);
        parameters = s.Mid(n + 1);
    }
}

RemoteInstallerService::RemoteInstallerService() : m_ssh(0), m_status(status_error), 
													m_err(0), m_currentPercentage(0),
													m_lastMsiMessage(L"")
{
	::InitializeCriticalSection(&m_cs);
	memset(&m_ss, 0, sizeof(m_ss));
	m_ss.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
	m_ss.dwControlsAccepted = SERVICE_CONTROL_INTERROGATE;
}
RemoteInstallerService::~RemoteInstallerService()
{
	::DeleteCriticalSection(&m_cs);
}

void RemoteInstallerService::OnInstallProgress(int percentage)
{
	DO_LOG(L"OnInstallProgress invoked with %d percentages", percentage);
	if (percentage > m_currentPercentage || m_status != status_install_in_progress)
	{
		setStatus(status_install_in_progress);
		m_currentPercentage = percentage;
		writeStatusToFile(m_responseFile, false);
	}
}

void RemoteInstallerService::OnInstallMessage(LPCWSTR message)
{
	m_lastMsiMessage = message;
	//m_pContext->ReportAboutActionProgress(m_progressCalculator.getTotalProgress(), message);
}

void RemoteInstallerService::OnInstallTerminate()
{

}
bool RemoteInstallerService::InitService(DWORD dwArgc, LPTSTR* lpszArgv)
{
	assert(dwArgc > 0);

    WSADATA WSAData; 
    ::WSAStartup( MAKEWORD( 2, 2 ), &WSAData);

	if (g_runFromService)
	{

		m_ssh = RegisterServiceCtrlHandlerEx(s_DescentFileName, HandlerEx, this);
		if (m_ssh == 0)
			return false;

		m_ss.dwCurrentState = SERVICE_RUNNING;
		SetServiceStatus(m_ssh, &m_ss);
	}

	m_argc = dwArgc - 1;
	m_argv = new LPTSTR[m_argc];
	for (int i = 0 ; i < m_argc ; i++)
	{
		int allocSize = wcslen(lpszArgv[i + 1]) + sizeof(TCHAR);
		m_argv[i] = new TCHAR[allocSize];
		memset(m_argv[i], 0, allocSize);
		wcscpy_s(m_argv[i], allocSize, lpszArgv[i + 1]);
	}

    return true;
}

void RemoteInstallerService::ReleaseService()
{
	for (int i = 0; i < m_argc; i++)
	{
		delete[] m_argv[i];
	}
	delete[] m_argv;

	if (g_runFromService)
		if(m_ssh != 0)
		{
			m_ss.dwCurrentState = SERVICE_STOPPED;
			SetServiceStatus(m_ssh, &m_ss);
		}

    ::WSACleanup();
}


DWORD WINAPI RemoteInstallerService::Handler(DWORD dwControl)
{
	switch (dwControl)
	{
		case SERVICE_CONTROL_INTERROGATE:
			return NO_ERROR;

		default:
			return ERROR_CALL_NOT_IMPLEMENTED;
	}
}

DWORD WINAPI RemoteInstallerService::HandlerEx(DWORD dwControl, DWORD dwEventType, LPVOID lpEventData, LPVOID lpContext)
{
    return ((RemoteInstallerService*)lpContext)->Handler(dwControl);
}

/** 
* process_script Executes script on the local machine
* 
* @param argc must be 2
* @param argv argv[0] = Script file name, argv[1] = Out file name
* 
* @returns 0 if ok
*/
int RemoteInstallerService::process_script(int argc, LPTSTR argv[])
{
    if(argc != 3)
    {
        assert(false);
        return 1;
    }

    int timeoutInMs = _wtoi(argv[2]);

    DO_LOG(L"Process_script: executing %s > %s (timeout=%d)",
        argv[0],
        argv[1],
        timeoutInMs
    );

    wchar_t cmdLine[1024];
    wchar_t currentFolder[MAX_PATH];
    wchar_t outputFile[MAX_PATH];

    ::GetWindowsDirectoryW(currentFolder, sizeof(currentFolder) / sizeof(currentFolder[0]));
    swprintf_s(outputFile, L"%s\\%s", currentFolder, argv[1]);
    swprintf_s(cmdLine, L" > %s", argv[1]);

    ::DeleteFile(outputFile);

    SHELLEXECUTEINFO sei = {0}; 
    ZeroMemory (&sei, sizeof (SHELLEXECUTEINFO)); 
    sei.cbSize = sizeof (SHELLEXECUTEINFO); 
    sei.lpVerb = L"OPEN";
    sei.lpFile = argv[0],
    sei.lpParameters = cmdLine,
    sei.nShow = SW_HIDE;
    sei.lpDirectory = currentFolder;
    sei.fMask = SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI;

    BOOL b = ::ShellExecuteEx(&sei); 
    if(!b)
    {
        DO_LOG(L"Error");
        return 1;
    }

    DO_LOG(L"Waiting for exiting...");

    // Wait
    if (::WaitForSingleObject(sei.hProcess, timeoutInMs ? timeoutInMs : INFINITE) == WAIT_TIMEOUT)
    {
        ::TerminateProcess(sei.hProcess,0xffffffff);
        ::CloseHandle(sei.hProcess);

        DO_LOG(L"ERROR: TIMEOUT. Deleting file %s...", outputFile);

        if(!::DeleteFileW(outputFile))
        {
            DO_LOG(L"Cannot delete file!");
        }
        return 1;
    }

    ::CloseHandle(sei.hProcess);
    DO_LOG(L"OK");

    return 0;
}


/** 
* process_msi Installs list of msi files
* 
* @param argc number of msi files
* @param argv msi strings (FileName*Parameters)
* 
* @returns 0 if ok
*/
int RemoteInstallerService::process_msi(int argc, LPTSTR argv[])
{
    // Parse MSIs
    // Msi_path*parameters
    int i, iCount = argc;
    std::vector<tMSI> arMSI;

    arMSI.resize(iCount);

    for(i = 0; i < argc;++i)
    {
        tMSI &msi = arMSI[i];
        extractParametersFromPath(argv[i], msi.path, msi.parameters);
    }

    // Primary initialization completed.
    // All necessary parameters are initialized.
    // Msi files with arguments are placed in 
    // the arMSI vector.


    // Now, we can start the installation.
    // Iterate through all MSI packages
    // and install each of them independently
    iCount = (int)arMSI.size();

    bool b;
    for(i = 0;i<iCount;++i)
    {
        const tMSI &msi = arMSI[i];
        b = InstallMsi(msi);
        if(b == false)
        {
            // Error occured
            break;
        }
    }

    return 0;
}

int RemoteInstallerService::ServiceMain()
{
    setStatus(status_error);
    setNativeError(ERROR_INVALID_FUNCTION);

    DO_LOG(L"ServiceMain: Params=%d", m_argc);
    for(int i = 0; i < m_argc; ++i)
    {
        DO_LOG(L"Param %d = %s", i, m_argv[i]);
    }

    do
    {
        if (m_argc < 4)
        {
            DO_LOG(L"Error: argc = %d", m_argc);
            break;
        }

        MakeWindowsInstallerQuiet();

        // Parse basic parameters
        m_Command = m_argv[1];
		//m_ServerHostName = GetServerHostNameFromIniFile(L"server.ini");

        int argc_ = m_argc - 2;
        LPTSTR* argv_ = m_argv + 2;

        //DO_LOG(L"Command = %s, ServerHostName = %s", m_Command, m_ServerHostName);

        int ret = -1;
        if(m_Command == L"process_script")
        {
            ret = process_script(argc_, argv_);
        }
        else
        if(m_Command == L"process_response_action")
        {
            ret = process_response_action(argc_, argv_);
        }
        else
        {
            DO_LOG(L"Error: unknown command %s", m_Command);
            break;
        }

        return ret;

    }
    while(0);

    // Error
    setStatus(status_error);
    setNativeError(ERROR_INVALID_FUNCTION);
    return -1;
}

bool RemoteInstallerService::InstallMsi(int argc, LPTSTR argv[])
{
	tMSI msi;
	CString strPath, parameters;
	extractParametersFromPath(argv[0], strPath, parameters);
	msi.path = strPath;
	CPath path(strPath);
	CString strExt = path.GetExtension();
	if (strExt.CompareNoCase(L".msi") != 0)
	{
		setStatus(DescentStatus::status_unsupported_extension);
		return false;
	}
	if (parameters.Find(L"REBOOT") == -1)
		parameters += L" REBOOT=ReallySuppress";
	msi.parameters = parameters;
	bool rebootRequered = false;
	bool installationSuspended = false;
	DWORD status = 0;
	CInstallReporter msiReporter;
	msiReporter.init(this);
	AgentCommonTools::MakeWindowsInstallerQuiet();
	auto finish = AgentCommonTools::InstallMsi(path, parameters, true, rebootRequered, status, installationSuspended);
	m_err = status;
	updateStatusByMsiError();
	return m_err == ERROR_SUCCESS;
}

/** 
* InstallMsi Install single msi
* 
* @param msi MSI description structure
* 
* @returns true if ok
*/
bool RemoteInstallerService::InstallMsi(const tMSI &msi)
{
    // Install MSI with parameters
    CString path = msi.path;
    if(::GetFileAttributes(path) == 0xffffffff)
    {
		wchar_t buf[MAX_PATH];
		::GetWindowsDirectory(buf, sizeof(buf) / sizeof(buf[0]));

        path = buf;
		path += L"\\";
        path += msi.path;
    }

    if(::GetFileAttributes(path) == 0xffffffff)
    {
        // File does not exist
        DO_LOG(L"File %s does not exist\n",path);
        return false;
    }

    DO_LOG(L"Installing %s (%s)",path, msi.parameters);
	g_pLoggerAutoInit->m_logger.init( false );

    m_err = ::MsiInstallProductW(path, msi.parameters);
	g_pLoggerAutoInit->m_logger.init( true, g_pLoggerAutoInit->m_logger.getLogFilePath());

    updateStatusByMsiError();
    DO_LOG(L"Installed %s (%s), result=0x%08X",path, msi.parameters, m_err);
    return m_err == ERROR_SUCCESS;
}

/** 
* process_agent_install Installs agent
* 
* @param argc must be 1
* @param argv argv[0] = MsiPath*Parameters
* 
* @returns 0 if ok
*/
int RemoteInstallerService::process_agent_install(int argc, LPTSTR argv[])
{
    DO_LOG(L"Installing agent");

    if(argc != 1)
    {
        DO_LOG(L"Error: argc = %d",argc);
        assert(false);
        return -1;
    }

    bool bUpdatingAgent = false;

    AgentInfo info;
    bool bInstalled = QueryInstalledAgentInfo(info);
    if(bInstalled)
    {
        // Already installed.
        // NOT FATAL - just uninstall previous version!
        DO_LOG(L"Warning: agent is already installed... Uninstalling...");
        bUpdatingAgent = true;
        m_err = UninstallAgent();
        updateStatusByMsiError();
        if(m_err != ERROR_SUCCESS)
        {
            formatContextError(L"Error 0x%08X occured while uninstalling", m_err);
            return 0;
        }
        else
        {
            DO_LOG(L"Uninstallation complete. Resuming installation...");
        }
    }

	CString hexString;
	ExtractPublicKeyHexStringFromIniFile(L"server.ini", hexString);
	if (hexString.IsEmpty())
	{
		DO_LOG(L"Failed to read PublicKey value from server.ini");
		return -1;
	}

    tMSI msi;
    CString path, parameters;
    extractParametersFromPath(argv[0], path, parameters);
	if (parameters.Find(L"PUBLICKEY=") == -1)
		parameters.AppendFormat(L" PUBLICKEY=%s", hexString.GetString());
    msi.path = path;
	msi.parameters = parameters;

    bool b = InstallMsi(msi);

    if(b && m_status == status_success && bUpdatingAgent)
    {
        // Agent has been updated successfully !
        DO_LOG(L"Updated successfully");
        setStatus(status_success_agent_updated);
    }

    return b ? 0 : -1;
}

/** 
* process_agent_uninstall Uninstalls agent by known GUID
* 
* @param argc 0
* @param argv
* 
* @returns 0 if ok
*/
int RemoteInstallerService::process_agent_uninstall(int argc, LPTSTR argv[])
{
    DO_LOG(L"Uninstalling agent");

    if(argc != 0)
    {
        DO_LOG(L"Error: argc = %d",argc);
        assert(false);
        return -1;
    }

    AgentInfo info;
    bool bInstalled = QueryInstalledAgentInfo(info);
    if (!bInstalled)
    {
        // Not installed
        setStatus(status_agent_is_not_installed);
        return 0;
    }

    m_err = UninstallAgent();
    updateStatusByMsiError();

    DO_LOG(L"Uninstallation completed. Result = 0x%08X",m_err);
    return 0;
}



/** 
* updateStatusByMsiError update m_status by m_err
* 
*/
void RemoteInstallerService::updateStatusByMsiError()
{
    switch(m_err)
    {
    case 0:
        setStatus(status_success);
        break;
    case ERROR_UNKNOWN_PRODUCT:
        {
            setStatus(status_installer_unknown_product);
        }
        break;
    case ERROR_INSTALL_FAILURE:
        {
            setStatus(status_installer_fatal_error);
        }
        break;
    case ERROR_INSTALL_ALREADY_RUNNING:
        {
            setStatus(status_installer_install_already_running);
        }
        break;
    default:
        DO_LOG(L"ERROR: UNKNOWN MSI ERROR 0x%08X", m_err);
        setStatus(status_msi_error);
        break;
    }
}

void RemoteInstallerService::formatContextError(LPCWSTR format, ...)
{
    wchar_t buffer[1024];

    va_list args;
    va_start (args, format);
    vswprintf_s(buffer,sizeof(buffer) / sizeof(buffer[0]), format, args);
    va_end (args);

    setContextError(buffer);
}


void RemoteInstallerService::setContextError(LPCWSTR error)
{
    // Just append it
    if (m_ErrorString.IsEmpty())
    {
        m_ErrorString = error;
    }
    else
    {
        m_ErrorString += L"\r\n";
        m_ErrorString += error;
    }
    DO_LOG(L"ContextStatus: %s", error);
}

