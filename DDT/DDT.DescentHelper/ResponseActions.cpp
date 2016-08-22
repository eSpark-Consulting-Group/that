#include "stdafx.h"
#include <atlstr.h>
#include "Installer.h"
#include "AgentInstallUtils.h"
#include "AgentSslSocketTools.h"
#include "FormattedText.h"
#include <ATLComTime.h>
#include "ImpersonationHelper.h"

using namespace CRM::AgnCore::Transport;
using namespace CRM::Localization;

#include <algorithm>

void RemoteInstallerService::formatResponse(LPCWSTR format, ...)
{
    wchar_t buffer[1024];

    va_list args;
    va_start (args, format);
    vswprintf_s(buffer,sizeof(buffer) / sizeof(buffer[0]), format, args);
    va_end (args);

    m_responseString = buffer;
}


/** 
* process_response_action Executes special action. Response file name (output)
*               must contain DescentStatus;win32error#errorString\nRESPONSE STRING
* 
* @param argc Must be >= 3
* @param argv[0] - ActionName
* @param argv[1] - Output filename
* @param argv[2] - Timeout, in ms
* @param argv[n] - Parameters
* 
* @returns
*/
int RemoteInstallerService::process_response_action(int argc, LPTSTR argv[])
{
    if(argc < 3)
    {
        DO_LOG(L"Invalid parameters count %d", argc);
        return -1;
    }

    m_responseString = L"";
    m_err = ERROR_INVALID_FUNCTION;
    m_status = status_error;
    m_ErrorString = L"";

    CString actionName = argv[0];
    CString responseFile = argv[1];
    int Timeout = _wtoi(argv[2]);
	m_responseFile = responseFile;
    int argc_ = argc - 3;
    LPTSTR* argv_ = argv + 3;
    // Parse ActionName
	m_systemKind = GetSystemKind();

    int ret = -1;
    do
    {
        if (actionName == L"action_install_agent_check")
        {
            ret = action_install_agent_check(argc_, argv_);
        }
        else
        if (actionName == L"action_query_agent_information")
        {
            ret = action_query_agent_information(argc_, argv_);
        }
        else
        if (actionName == L"action_install_agent")
        {
            ret = action_install_agent(argc_, argv_);
        }
		else 
		if(actionName == L"action_install_msi")
		{
			ret = action_install_msi(responseFile,argc_, argv_);
		}
        else
        if (actionName == L"action_uninstall_agent")
        {
            ret = action_uninstall_agent(argc_, argv_);
        }
		else
		if (actionName = L"action_install_exe")
		{
			ret = action_install_exe(argc_, argv_, Timeout);
		}
        else
        {
            DO_LOG(L"Error: unknown action %s", actionName);
            setStatus(status_error);
            formatContextError(L"Error: unknown action %s", actionName);
            ret = -1;
        }

    }
    while(0);

	writeStatusToFile(responseFile, true);

    return ret;
}

bool RemoteInstallerService::writeStatusToFile(CString responseFile, bool forceWrite) const
{
	DO_LOG(L"writeStatusToFile");
	static bool firstTime = true;
	static clock_t  last;
	clock_t now = clock();
	if (!firstTime && !forceWrite)
	{
		clock_t d = now - last;
		double seconds = static_cast<double>(d) / CLOCKS_PER_SEC;
		if(seconds < 2)
		{
			DO_LOG(L"Previous write have been less than 2 seconds ago (%f)", seconds);
			return false;
		}
	}
	last = now;
	firstTime = false;

	if(::TryEnterCriticalSection(&m_cs))
	{
		static clock_t time = clock();
		// Write result
		wchar_t currentFolder[MAX_PATH];
		wchar_t outputFile[MAX_PATH];

		if(responseFile.Find(L'\\') != -1)
		{
			swprintf_s(outputFile, L"%s", responseFile);
		}
		else
		{
			GetModuleFileNameW(NULL, currentFolder, MAX_PATH - 1);
			memset(wcsrchr(currentFolder, '\\') + 1, 0, wcslen(wcsrchr(currentFolder, '\\')));
			//::GetWindowsDirectoryW(currentFolder, _countof(currentFolder));
			swprintf_s(outputFile, L"%s%s", currentFolder, responseFile);
		}
		::DeleteFile(outputFile);

		HANDLE hFile = ::CreateFile(outputFile, GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, NULL, NULL);

		if (hFile == INVALID_HANDLE_VALUE)
		{
			DO_LOG(L"Error: can not create %s", outputFile);
			return false;
		}

		// Write in the response file
		wchar_t buf[1024] = { 0 };
		wchar_t buf2[1024] = { 0 };
		DWORD dw;
		swprintf_s(buf, L"%d;%d#%s", (UINT)m_status, (UINT)m_err, m_ErrorString);
		swprintf_s(buf2, L"%d;%s", (UINT)m_currentPercentage, m_lastMsiMessage);


		DO_LOG(L"ResponseErr: %s", buf);
		DO_LOG(L"ResponseString: %s", m_responseString);
		DO_LOG(L"LastMessage: %s", m_lastMsiMessage);
		DO_LOG(L"Current pecentage: %d", m_currentPercentage);

		::WriteFile(hFile, buf, wcslen(buf) * sizeof(buf[0]), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		::WriteFile(hFile, m_responseString, m_responseString.GetLength() * sizeof(WCHAR), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		::WriteFile(hFile, buf2, wcslen(buf2) * sizeof(buf2[0]), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		swprintf_s(buf2, L"%d", (UINT)m_systemKind);
		::WriteFile(hFile, buf2, wcslen(buf2) * sizeof(buf2[0]), &dw, NULL);
		::CloseHandle(hFile);
		::LeaveCriticalSection(&m_cs);
		return true;

	}
	return false;
}


bool RemoteInstallerService::checkConnectionToRemoteServer_(int hostPort)
{
	CString err;

	DO_LOG(L"Checking connection to the remote server");
	CString validHost;
	if(!ValidateCesmServerSSLFromIniFile(L"server.ini", hostPort, err, validHost))
	{
		CFormattedText msg(Resources::Descent_ConnectionError);
		msg.AddParameter(m_ServerHostName);
		msg.AddParameter(err);

		formatContextError(L"%s", LPCWSTR(msg));
		return false;
	}
	m_ServerHostName = validHost;

    // All is ok
    setStatus(status_success);
    return true;
}

bool RemoteInstallerService::checkInstalledAgentCompatibilityToThisServer_(AgentInfo info, int hostPort)
{
    // Already installed
    // Now check remote server info
    if (!info.serverHostName.IsEmpty()) // old versions may not give information about server host and port
    {
		info.serverHostName.MakeLower();
        m_ServerHostName.MakeLower();

        if (info.serverHostName != m_ServerHostName)
        {
            formatContextError(L"ServerName %s != %s", info.serverHostName, m_ServerHostName);
            setStatus(status_agent_is_already_installed_and_server_differs);
            return false;
        }
        if (info.serverPort != hostPort)
        {
            formatContextError(L"ServerPort %d != %d", info.serverPort, hostPort);
            setStatus(status_agent_is_already_installed_and_server_differs);
            return false;
        }
    }

    setStatus(status_agent_is_already_installed);
    return true;
}




/** 
* action_install_agent_check Check connection to the server and returns target platform.
*                                                           Also checks whether agent is installed.
* 
* @param argc 1
* @param argv[0] - HostPort
* 
* @returns 0 if ok
*/
int RemoteInstallerService::action_install_agent_check(int argc, LPTSTR argv[])
{
    if(argc != 1)
    {
        setStatus(status_error);
        formatContextError(L"Arguments count error: %d != 1", argc);
        assert(false);
        return -1;
    }


    AgentInfo info;
    bool bInstalled = QueryInstalledAgentInfo(info);

    // Now get target processor architecture
    bool is32 = IsSystem32bit();

    // Auxiliary parameters
    unsigned versionMajor = 0, versionMinor = 0, serverPort = 0;
    CString serverHostName(L"");

    swscanf_s(info.agentVersion, L"%d.%d", &versionMajor, &versionMinor);
    serverHostName = info.serverHostName;
    serverPort = info.serverPort;

    // format response
    formatResponse(L"%d,%d,%d,%d,%s", 
        is32 ? 32 : 64,
        versionMajor,
        versionMinor,
        serverPort,
        serverHostName);

    DO_LOG(L"Agent details: Platform=%d Version=%d.%d ServerHostName=%s ServerPort=%d",
        is32 ? 32 : 64, versionMajor, versionMinor, serverHostName, serverPort);


    // Now perform additional tests
    UINT hostPort = _wtoi(argv[0]);

    // Check whether agent is already installed
    if (bInstalled)
    {
        // DO NOT check compatibility to the current server now
        if(checkInstalledAgentCompatibilityToThisServer_(info, hostPort))
            return 0;
        return -1;
    }

    if(checkConnectionToRemoteServer_(hostPort))
        return -1;

    return 0;
}


// param 1: server port
int RemoteInstallerService::action_query_agent_information(int argc, LPTSTR argv[])
{
    if(argc != 1)
    {
        setStatus(status_error);
        formatContextError(L"Arguments count error: %d != 1", argc);
        assert(false);
        return -1;
    }

    unsigned versionMajor = 0, versionMinor = 0, serverPort = 0;
    CString serverHostName;

    AgentInfo info;
    bool bInstalled = QueryInstalledAgentInfo(info);
    if (bInstalled)
    {
        swscanf_s(info.agentVersion, L"%d.%d",  &versionMajor, &versionMinor);
        serverHostName = info.serverHostName;
        serverPort = info.serverPort;
    }

    // Now perform additional tests
    UINT hostPort = _wtoi(argv[0]);
    bool connectionOK = checkConnectionToRemoteServer_(hostPort);

    formatResponse(L"%d,%d,%d,%d,%s,%d", 
        bInstalled ? 1 : 0,
        versionMajor,
        versionMinor,
        serverPort,
        serverHostName,
        connectionOK ? 1 : 0
        );

    DO_LOG(L"Agent details: Installed=%d Version=%d.%d ServerHostName=%s ServerPort=%d Connection=%d",
        bInstalled ? 1 : 0, versionMajor, versionMinor, serverHostName, serverPort,
        connectionOK ? 1 : 0);

    // All is ok
    setStatus(status_success);
    return 0;
}

int RemoteInstallerService::action_install_exe(int argc, LPTSTR argv[], int timeOut)
{
	DWORD dwExitCode;
	DO_LOG(L"Creating process token..."); 
	wchar_t loggedStr[100];
	swprintf_s(loggedStr, L"%d arguments", argc);
	DO_LOG(loggedStr);
	CString parameters;
	CString strPath;
	extractParametersFromPath(argv[0], strPath, parameters);
	PROCESS_INFORMATION pi = { 0 };
	STARTUPINFO pStartUp = { 0 };
	pStartUp.cb = sizeof(STARTUPINFO);
	pStartUp.dwFlags = STARTF_FORCEOFFFEEDBACK; // disable waiting cursor when launched the programwchar_t outputFile[MAX_PATH];
	wchar_t parametersStr[MAX_PATH];
	swprintf_s(parametersStr, L"%s %s", strPath, parameters);
	
	setStatus(status_prepairing_for_create_process);
	writeStatusToFile(m_responseFile, true);
	auto impersonate = false;
	
	if (argc > 2)
	{
		swprintf_s(loggedStr, L"%s %s", argv[1], argv[2]);
		DO_LOG(loggedStr);
		CString argument = argv[1];
		DO_LOG(argument);
		if (argument == L"-impersonate")
		{
			argument = argv[2];
			DO_LOG(argument);
			if (argument == L"LoggedUserExecutionContext")
			{
				DO_LOG(L"Impersonation context: LoggedUserExecutionContext");
				impersonate = true;
			}
		}
	}
	BOOLEAN b = false;
	CHandle hImpersonationHandle;
	if (impersonate)
	{
		do
		{
			DWORD err;
			auto hImpersonationHandleInternal = ImpersonationHelper::GetActiveSessionUserToken(err);
			if (hImpersonationHandleInternal == NULL)
			{
				DO_LOG(L"Impersonation to current logged user failed");
				impersonate = false;
				break;
			}
			hImpersonationHandle.Attach(hImpersonationHandleInternal);
			DO_LOG(L"Start process as user");
			b = ::CreateProcessAsUser(hImpersonationHandle, NULL, parametersStr,
					NULL, NULL, FALSE, CREATE_UNICODE_ENVIRONMENT | CREATE_SUSPENDED,
					NULL, NULL, &pStartUp, &pi);

		} while (false);
	}
	if (!impersonate)
	{
		DO_LOG(L"Start process");
		b = ::CreateProcess(NULL, parametersStr,
			NULL, NULL, FALSE, CREATE_UNICODE_ENVIRONMENT | CREATE_SUSPENDED,
			NULL, NULL, &pStartUp, &pi);
	}
	if (!b)
	{
		CWin32LastError error;
		DO_LOG(L"CreateProcess failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
		m_responseString = error.GetMessage();
		m_err = error.GetCode();
		setStatus(status_cannot_create_process);
		return m_err;
	}
	else
	{
		setStatus(status_install_in_progress);
		writeStatusToFile(m_responseFile, true);
	}
	DO_LOG(L"Process information: PID=%d, TID=%d, PHND=%08X, THND=%08X", pi.dwProcessId, pi.dwThreadId, pi.hProcess, pi.hThread);
	DO_LOG(L"Resuming main thread...");
	::ResumeThread(pi.hThread);
	DO_LOG(L"Waiting for quit..");
	auto waitCode = ::WaitForSingleObject(pi.hProcess, timeOut? timeOut:INFINITE);
	if (waitCode == WAIT_TIMEOUT)
	{
		DO_LOG("time out");
		setStatus(status_installing_timeout);
	}
	else
	{
		::GetExitCodeProcess(pi.hProcess, &dwExitCode);
		DO_LOG(L"Completed, exit code = %u", dwExitCode);
		if (dwExitCode == ERROR_INVALID_FUNCTION)
			dwExitCode = ERROR_INSTALL_FAILURE;

		if (dwExitCode == ERROR_INSTALL_FAILURE)
		{
			m_err = dwExitCode;
			m_responseString = CWin32LastError(dwExitCode).GetMessage();
			setStatus(status_installer_fatal_error);
		}
		else if (dwExitCode == 0)
			setStatus(status_success);
		else
		{
			setStatus(status_exe_error);
			m_err = dwExitCode;
			m_responseString = CWin32LastError(dwExitCode).GetMessage();
		}
	}
	::CloseHandle(pi.hThread);
	::CloseHandle(pi.hProcess);


	// Many executables return error 1, so we just modify it
	
	if (impersonate)
		hImpersonationHandle.Close();
	return dwExitCode;
}


int RemoteInstallerService::action_install_msi(CString responseFile, int argc, LPTSTR argv[])
{
	if (argc != 1)
	{
		setStatus(status_error);
		formatContextError(L"Arguments count error: %d != 0", argc);
		assert(false);
		return -1;
	}
	DO_LOG(L"Installing msi");
	setStatus(status_before_msi_started);
	writeStatusToFile(responseFile, true);
	bool b = InstallMsi(argc, argv);

	formatResponse(L"%d",
		b ? 0 : -1
	);

	return b ? 0 : -1;
}

int RemoteInstallerService::action_install_agent(int argc, LPTSTR argv[])
{
    if(argc != 1)
    {
        setStatus(status_error);
        formatContextError(L"Arguments count error: %d != 0", argc);
        assert(false);
        return -1;
    }

    int result = process_agent_install(argc, argv);

    formatResponse(L"%d", 
        result
        );


    return 0;
}

int RemoteInstallerService::action_uninstall_agent(int argc, LPTSTR argv[])
{
    if(argc != 0)
    {
        setStatus(status_error);
        formatContextError(L"Arguments count error: %d != 0", argc);
        assert(false);
        return -1;
    }



    int result = process_agent_uninstall(argc, argv);

    formatResponse(L"%d", 
        result
        );


    return 0;
}

