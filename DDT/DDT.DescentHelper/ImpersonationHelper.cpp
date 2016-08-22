#include "stdafx.h"
#include <WtsApi32.h>
#include <Userenv.h>
#include <Ntsecapi.h>
#include <Tlhelp32.h>
#include "ImpersonationHelper.h"

#define INVALID_SESSION_VALUE 0xFFFFFFFF

LPCWSTR ImpersonationHelper::AgentCallbackString = L"AgentCallback";
LPCWSTR ImpersonationHelper::AgentCallbackCommand_executeProcess = L"executeProcess";

LSA_HANDLE ImpersonationHelper::GetPolicyHandle()
{
    LSA_OBJECT_ATTRIBUTES ObjectAttributes;
    NTSTATUS ntsResult;
    LSA_HANDLE lsahPolicyHandle;

    // Object attributes are reserved, so initialize to zeros.
    ZeroMemory(&ObjectAttributes, sizeof(ObjectAttributes));

    // Get a handle to the Policy object.
    ntsResult = ::LsaOpenPolicy(
        NULL,    //Name of the target system.
        &ObjectAttributes, //Object attributes.
        POLICY_ALL_ACCESS, //Desired access permissions.
        &lsahPolicyHandle  //Receives the policy handle.
        );

    if (ntsResult != ERROR_SUCCESS)
    {
		CWin32LastError error(::LsaNtStatusToWinError(ntsResult));
        DO_LOG(L"LsaOpenPolicy failed: %s (%d)", error.GetMessage(), error.GetCode());
        return NULL;
    } 
    return lsahPolicyHandle;
}

void ImpersonationHelper::InitLsaString(PLSA_UNICODE_STRING LsaString, LPWSTR String)
{
    DWORD StringLength;

    if (String == NULL) {
        LsaString->Buffer = NULL;
        LsaString->Length = 0;
        LsaString->MaximumLength = 0;
        return;
    }

    StringLength = (DWORD)wcslen(String);
    LsaString->Buffer = String;
    LsaString->Length = (USHORT) StringLength * sizeof(WCHAR);
    LsaString->MaximumLength=(USHORT)(StringLength+1) * sizeof(WCHAR);
}

bool ImpersonationHelper::AddPrivilegesToAccount(PSID AccountSID, LPWSTR PolicyName)
{
    LSA_HANDLE PolicyHandle = GetPolicyHandle();

    LSA_UNICODE_STRING lucPrivilege;
    NTSTATUS ntsResult;

    InitLsaString(&lucPrivilege, PolicyName);
    ntsResult = ::LsaAddAccountRights(
        PolicyHandle,  // An open policy handle.
        AccountSID,    // The target SID.
        &lucPrivilege, // The privileges.
        1              // Number of privileges.
        );
	if (ntsResult != ERROR_SUCCESS)
	{
		CWin32LastError error(::LsaNtStatusToWinError(ntsResult));
        DO_LOG(L"LsaAddAccountRights failed: %s (%d)", error.GetMessage(), error.GetCode());
	}

    LsaClose(PolicyHandle);

    return (ntsResult == ERROR_SUCCESS);
}

BOOL ImpersonationHelper::IsPrivilegeSet(HANDLE hToken, LPCTSTR lpszPrivilege)
{
    PRIVILEGE_SET ps;
    LUID luid;

    if ( !::LookupPrivilegeValue(
        NULL, // lookup privilege on local system
        lpszPrivilege, // privilege to lookup
        &luid ) ) // receives LUID of privilege
    {
		CWin32LastError error;
        DO_LOG(L"LookupPrivilegeValue failed: %s (%d)", error.GetMessage(), error.GetCode());
        return FALSE;
    }

    ps.Control = PRIVILEGE_SET_ALL_NECESSARY;
    ps.PrivilegeCount = 1;
    ps.Privilege[0].Luid = luid;
    ps.Privilege[0].Attributes = 0;

    BOOL b, bResult;
    b = PrivilegeCheck(hToken, &ps, &bResult);

    if(b == FALSE)
    {
        return FALSE;
    }

    return bResult;
}

BOOL ImpersonationHelper::SetTokenPrivilege(
    HANDLE hToken, // access token handle
    LPCTSTR lpszPrivilege, // name of privilege to enable/disable
    BOOL bEnablePrivilege, // to enable or disable privilege
	_Out_opt_ PBOOL pbPreviousState /* = NULL */)
{
    TOKEN_PRIVILEGES tp;
	LUID luid;

    if ( !::LookupPrivilegeValue(
        NULL, // lookup privilege on local system
        lpszPrivilege, // privilege to lookup
        &luid ) ) // receives LUID of privilege
    {
		CWin32LastError error;
        DO_LOG(L"LookupPrivilegeValue failed: %s (%d)", error.GetMessage(), error.GetCode());
        return FALSE;
    }

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    if (bEnablePrivilege)
        tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
    else
        tp.Privileges[0].Attributes = 0;

	TOKEN_PRIVILEGES ptp;
	DWORD ptpLen = sizeof(ptp);

    if ( !AdjustTokenPrivileges(
        hToken,
        FALSE,
        &tp,
        sizeof(TOKEN_PRIVILEGES),
        &ptp,
        &ptpLen) )
    {
        return FALSE;
    }

    if (GetLastError() == ERROR_NOT_ALL_ASSIGNED)
    {
        return FALSE;
    }

	if (pbPreviousState != NULL)
	{
		*pbPreviousState = ptp.PrivilegeCount == 0 ?
			bEnablePrivilege :
			ptp.Privileges[0].Attributes == SE_PRIVILEGE_ENABLED || ptp.Privileges[0].Attributes == SE_PRIVILEGE_ENABLED_BY_DEFAULT;
	}
    return TRUE;
}

BOOL ImpersonationHelper::SetProcessPrivilege(
    HANDLE hProcess,
    LPCTSTR lpszPrivilege, // name of privilege to enable/disable
    BOOL bEnablePrivilege, // to enable or disable privilege
	_Out_opt_ PBOOL pbPreviousState /* = NULL */)
{
    HANDLE hToken;
    BOOL b = FALSE;
    if (!::OpenProcessToken(hProcess, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
    {
		CWin32LastError error;
        DO_LOG(L"OpenProcessToken failed: %s (%d)", error.GetMessage(), error.GetCode());
        return FALSE;
    }

    b = SetTokenPrivilege(hToken, lpszPrivilege, bEnablePrivilege, pbPreviousState);
    ::CloseHandle(hToken);
    return b;
}

BOOL ImpersonationHelper::SetProcessPrivilege(
    DWORD ProcessId,
    LPCTSTR lpszPrivilege, // name of privilege to enable/disable
    BOOL bEnablePrivilege, // to enable or disable privilege
	_Out_opt_ PBOOL pbPreviousState /* = NULL */)
{
    BOOL b = FALSE;

    HANDLE hProcess = ::OpenProcess(MAXIMUM_ALLOWED,FALSE,ProcessId);
    if (hProcess == INVALID_HANDLE_VALUE)
    {
		CWin32LastError error;
        DO_LOG(L"OpenProcess failed: %s (%d)", error.GetMessage(), error.GetCode());
        return FALSE;
    }

    b = SetProcessPrivilege(hProcess, lpszPrivilege, bEnablePrivilege, pbPreviousState);

    ::CloseHandle(hProcess);
    return b;
}

BOOL ImpersonationHelper::SetCurrentProcessPrivilege(
    LPCTSTR lpszPrivilege, // name of privilege to enable/disable
    BOOL bEnablePrivilege, // to enable or disable privilege
	_Out_opt_ PBOOL pbPreviousState /* = NULL */)
{
    return SetProcessPrivilege(GetCurrentProcess(), lpszPrivilege, bEnablePrivilege, pbPreviousState);
}

/** 
* GetSessionUserToken Get session user id and return token
* GetLastError() can be ERROR_PRIVILEGE_NOT_HELD on error
* 
* @returns result or NULL
*/
HANDLE ImpersonationHelper::GetSessionUserToken(DWORD dwSessionId, DWORD& err)
{
    // Enable SE_TCB_NAME privilege for this process in order to get access to WTSQueryUserToken
    ImpersonationHelper::SetCurrentProcessPrivilege(SE_TCB_NAME, TRUE);

    HANDLE hToken    = NULL;
    HANDLE hDupToken = NULL;

    if (!::WTSQueryUserToken(dwSessionId, &hToken))
    {
		CWin32LastError error;
        DO_LOG(L"WTSQueryUserToken failed: %s (%d)", error.GetMessage(), error.GetCode());
        return NULL;
    }

    if (!::DuplicateTokenEx(hToken, TOKEN_ALL_ACCESS, NULL, 
        /*SecurityIdentification*/
        SecurityImpersonation,
        TokenPrimary,
		&hDupToken))
    {
		CWin32LastError error;
        DO_LOG(L"DuplicateTokenEx failed: %s (%d)", error.GetMessage(), error.GetCode());
        ::CloseHandle(hToken);
        return NULL;
    }

    ::CloseHandle(hToken);
    return hDupToken;
}

/** 
* GetActiveSessionUserToken Get active session user id and return token
* GetLastError() can be ERROR_PRIVILEGE_NOT_HELD on error
* Do not forget to call ::CloseHandle !!!!
* 
* @returns result or NULL
*/
HANDLE ImpersonationHelper::GetActiveSessionUserToken(DWORD& err)
{
	DWORD dwSessionId = INVALID_SESSION_VALUE;
	CString stationName;
	do 
	{
		PWTS_SESSION_INFO sessions = NULL;
		DWORD sessionsCount = 0;
		if (::WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &sessions, &sessionsCount))
		{
			for (DWORD i = 0; i < sessionsCount; i++)
			{
				WTS_SESSION_INFO& si = sessions[i];
				if (si.State == WTSActive)
				{
					dwSessionId = si.SessionId;
					stationName = si.pWinStationName;
					break;
				}
			}
			::WTSFreeMemory(sessions);
		}
		else
		{
			dwSessionId = ::WTSGetActiveConsoleSessionId();
			if (dwSessionId != INVALID_SESSION_VALUE)
				stationName = L"console";
		}
	}
	while (0);

	if (dwSessionId == INVALID_SESSION_VALUE)
	{
        return NULL;
    }

    DO_LOG(L"Active session id = %d, station name = '%s'", dwSessionId, stationName);

    return GetSessionUserToken(dwSessionId, err);
}

// Do not forget to call CloseHandles !!!
void ImpersonationHelper::GetSessionsUserTokens(std::vector<CHandle>& handles)
{
    PHANDLE currentToken = 0;
    PHANDLE primaryToken = 0;

    int dwSessionId = 0;
    PHANDLE hUserToken = 0;
    PHANDLE hTokenDup = 0;

    PWTS_SESSION_INFO pSessionInfo = 0;
    DWORD dwCount = 0;

    // Get the list of all terminal sessions 
    ::WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &pSessionInfo, &dwCount);

    int dataSize = sizeof(WTS_SESSION_INFO);

    // look over obtained list in search of the active session
    for (DWORD i = 0; i < dwCount; ++i)
    {
        WTS_SESSION_INFO si = pSessionInfo[i];
        if (WTSActive == si.State)
        { 
            DWORD err;
            HANDLE h = GetSessionUserToken(si.SessionId, err);
            if (h != NULL)
            {
                handles.push_back(CHandle(h));
            }
        }
    } 	

    ::WTSFreeMemory(pSessionInfo);
}

// Checks if logged user exists
bool ImpersonationHelper::IsLoggedUserExisting()
{
	bool logged = false;
    PWTS_SESSION_INFO pSessionInfo = 0;
    DWORD dwCount = 0;

    ::WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &pSessionInfo, &dwCount);

    for (DWORD i = 0; i < dwCount; ++i)
    {
        WTS_SESSION_INFO si = pSessionInfo[i];
        if (WTSActive == si.State)
        {
			logged = true;
			break;
        }
    } 	
		
	::WTSFreeMemory(pSessionInfo);
	return logged;
}

BOOL ImpersonationHelper::LogonUser(LPCWSTR UserName, LPCWSTR Domain, LPCWSTR Password, HANDLE& hToken)
{
    if (!::LogonUser(UserName, Domain, Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, &hToken))
    {
		CWin32LastError error;
        DO_LOG(L"LogonUser failed: %s (%d)", error.GetMessage(), error.GetCode());
        return FALSE;
    }

    HANDLE duplicatedToken;
    if (!::DuplicateTokenEx(hToken, MAXIMUM_ALLOWED, NULL, SecurityDelegation, TokenPrimary, &duplicatedToken))
    {
		CWin32LastError error;
        DO_LOG(L"DuplicateTokenEx failed: %s (%d)", error.GetMessage(), error.GetCode());
        ::CloseHandle(hToken);
        return FALSE;
    }

    ::CloseHandle(hToken);
    hToken = duplicatedToken;

    return TRUE;
}

void ImpersonationHelper::EnumChildProcesses( DWORD ProcessId, vector<DWORD>& ProcessesIds )
{
	PROCESSENTRY32 pe32;
	HANDLE hProcessesSnap;

	// Take a snapshot of all processes in the system.
	hProcessesSnap = CreateToolhelp32Snapshot( TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD, 0 );
	if( hProcessesSnap == INVALID_HANDLE_VALUE )
	{
		return;
	}

	pe32.dwSize = sizeof( PROCESSENTRY32 );

	BOOL bProcessing = TRUE;

	bProcessing = Process32First( hProcessesSnap, &pe32 );

	do
	{
		if( pe32.th32ParentProcessID == ProcessId )
		{
			ProcessesIds.push_back( pe32.th32ProcessID );
		}

	} while( (bProcessing = Process32Next( hProcessesSnap, &pe32 )) );

	CloseHandle( hProcessesSnap );
}

HANDLE ImpersonationHelper::OpenProcess( DWORD ProcessID )
{
	HANDLE hProcess = NULL;
	if( ( hProcess = ::OpenProcess( PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, ProcessID ) ) == NULL )
	if( ( hProcess = ::OpenProcess( PROCESS_QUERY_INFORMATION, FALSE, ProcessID ) ) == NULL )
		  hProcess = ::OpenProcess( PROCESS_QUERY_LIMITED_INFORMATION, FALSE, ProcessID );
	return hProcess;
}

HANDLE ImpersonationHelper::OpenProcess( DWORD ProcessID, bool& VistaStyle )
{
	HANDLE hProcess = NULL;
	VistaStyle = false;
	if( ( hProcess = ::OpenProcess( PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, ProcessID ) ) == NULL )
	if( ( hProcess = ::OpenProcess( PROCESS_QUERY_INFORMATION, FALSE, ProcessID ) ) == NULL )
	if( ( hProcess = ::OpenProcess( PROCESS_QUERY_LIMITED_INFORMATION, FALSE, ProcessID ) ) != NULL)
		VistaStyle = true;
	return hProcess;
}

void ImpersonationHelper::WaitForMultipleProcesses( vector<DWORD>& processesIds, vector<DWORD>& dwExitCodes )
{
	size_t size = processesIds.size();
	vector<HANDLE> hOpenedProcesses;
	bool dummy = false;
	for( size_t i = 0 ; i < size ; i++ )
	{
		HANDLE hProcess = OpenProcess( processesIds[i], dummy );
		if( hProcess == NULL )
		{
			continue;
		}
		hOpenedProcesses.push_back(hProcess);
	}
	dwExitCodes.clear();
	DWORD count = (DWORD) hOpenedProcesses.size();
	DWORD waitResult = ::WaitForMultipleObjects(count, &hOpenedProcesses[0], TRUE, INFINITE);		
	if (waitResult != WAIT_FAILED)
	{
		for (DWORD i = 0 ; i < count; i++)
		{
			DWORD dwExitCode = 0;
			::GetExitCodeProcess(hOpenedProcesses[i], &dwExitCode);
			::CloseHandle(hOpenedProcesses[i]);
			dwExitCodes.push_back(dwExitCode);
		}
	}
}

void ImpersonationHelper::GetBiggestDWORD( vector<DWORD>& arr, DWORD& value )
{
	size_t size = arr.size();
	value = 0;
	for( size_t i = 0 ; i < size ; i++ )
	{
		DWORD& newValue = arr[i];
		if( newValue > value )
		{
			value = newValue;
		}
	}
}

BOOL ImpersonationHelper::ExecuteCustomAgentCommandInAnotherProcess(
	LPCWSTR command,
	LPCWSTR params, 
    DWORD& dwExitCode,
    HANDLE hImpersonationToken /* = NULL */,
	BOOL waitForQuit /* = TRUE */,
	HANDLE* phProcessHandle /* = NULL */,
    DWORD creationFlags /* = 0 */)
{
	wchar_t modulePath[MAX_PATH] = { };
	wchar_t moduleFolder[MAX_PATH] = { };
    wchar_t* filePath = NULL;

    ::GetModuleFileName(NULL, modulePath, _countof(modulePath));
    ::GetFullPathName(modulePath, _countof(moduleFolder), moduleFolder, &filePath);
    *(filePath-1) = 0;

	wchar_t cmdLine[1024] = { };
    swprintf_s(cmdLine, L"\"%s\" %s %s %s", modulePath, AgentCallbackString, command, params);

    BOOL b = ImpersonationHelper::ExecuteProcess(
        cmdLine,
        moduleFolder,
        waitForQuit,
        dwExitCode,
        hImpersonationToken,
        phProcessHandle,
        creationFlags);

    return b;
}

BOOL ImpersonationHelper::ExecuteCustomAgentCommandInAnotherProcess(
	LPCWSTR command,
	LPCWSTR params, 
    CAtlString& outString,
    DWORD& dwExitCode,
    HANDLE hImpersonationToken /* = NULL */,
	BOOL waitForQuit /* = TRUE */,
	HANDLE* phProcessHandle /* = NULL */,
    DWORD creationFlags /* = 0 */)
{
	wchar_t modulePath[MAX_PATH] = { };
	wchar_t moduleFolder[MAX_PATH] = { };
    wchar_t* filePath = NULL;

    ::GetModuleFileName(NULL, modulePath, _countof(modulePath));
    ::GetFullPathName(modulePath, _countof(moduleFolder), moduleFolder, &filePath);
    *(filePath-1) = 0;

	wchar_t buf[MAX_PATH] = { };
	wchar_t tempPath[MAX_PATH] = { };
	::GetTempPath(_countof(buf), buf);
	::GetTempFileName(buf, L"esm_____", 0, tempPath);

	wchar_t cmdLine[1024] = { };
    swprintf_s(cmdLine, L"\"%s\" %s %s %s \"%s\"", modulePath, AgentCallbackString, command, params, tempPath);

    BOOL b = ImpersonationHelper::ExecuteProcess(
        cmdLine,
        moduleFolder,
        waitForQuit,
        dwExitCode,
        hImpersonationToken,
        phProcessHandle,
        creationFlags);

    if (b)
    {
        CHandle hFile(::CreateFile(tempPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_DELETE_ON_CLOSE, NULL));
        if (hFile == INVALID_HANDLE_VALUE)
		{
			CWin32LastError error;
			DO_LOG(L"Cannot open response file '%s': %s (%d)", tempPath, error.GetMessage(), error.GetCode());
			return false;
		}
         
		DWORD memSize = ::GetFileSize(hFile, NULL);
        LPWSTR mem = (LPWSTR) _alloca(memSize + sizeof(WCHAR));
		memset(mem, 0, memSize + sizeof(WCHAR));

        if (!::ReadFile(hFile, mem, memSize, &memSize, NULL))
		{
			CWin32LastError error;
			DO_LOG(L"Cannot read from response file: %s (%d)", error.GetMessage(), error.GetCode());
			return false;
		}
        outString = mem;
    }

    return b;
}

DWORD ImpersonationHelper::ExecuteProcess(int argc, LPCWSTR* argv)
{
    wchar_t parameters[1024] = { };
    for (int i = 0; i < argc; ++i)
    {
        wcscat_s(parameters, argv[i]);
        wcscat_s(parameters, L" ");
    }

	DWORD dwError = 0;
	HANDLE hUserToken = ImpersonationHelper::GetActiveSessionUserToken( dwError );

    DWORD dwExitCode = 0;
    BOOL b = ImpersonationHelper::ExecuteProcess(parameters, NULL, TRUE, dwExitCode, hUserToken);

    return dwExitCode;
}

BOOL ImpersonationHelper::ExecuteProcessEx(
	LPCWSTR commandLine, 
    LPCWSTR executeFolder, 
    BOOL waitForQuit, 
    DWORD &dwExitCode,
	HANDLE hImpersonationToken /* = NULL */)
{
    BOOL b = ImpersonationHelper::ExecuteProcess(commandLine, executeFolder, waitForQuit, dwExitCode, hImpersonationToken);

    if (b == FALSE)
    {
        if (dwExitCode == ERROR_ELEVATION_REQUIRED)
        {
            DO_LOG(L"ERROR_ELEVATION_REQUIRED detected...");
            b = ExecuteCustomAgentCommandInAnotherProcess(AgentCallbackCommand_executeProcess, commandLine, dwExitCode, hImpersonationToken);
        }
    }

    DO_LOG(L"Result = %d, ExitCode=0x%08X",  b ? 1 : 0, dwExitCode);

    return b;
}

BOOL ImpersonationHelper::ExecuteProcess(
	LPCWSTR commandLine, 
    LPCWSTR executeFolder, 
    BOOL waitForQuit, 
    DWORD &dwExitCode,
    HANDLE hImpersonationToken /* = NULL */,
    HANDLE* phProcessHandle /* = NULL */,
    DWORD creationFlags /* = 0 */)
{
    DO_LOG(L"ExecuteProcess (%s), impersonationToken=0x%08X", commandLine, hImpersonationToken);

    if (phProcessHandle)
        *phProcessHandle = NULL;

    BOOL b;
    PROCESS_INFORMATION pi = {0};
    STARTUPINFO pStartUp = {0};
    pStartUp.cb = sizeof(STARTUPINFO);
    pStartUp.dwFlags = STARTF_FORCEOFFFEEDBACK; // disable waiting cursor when launched the program
    LPVOID pEnvironment = NULL;
    LPWSTR parameters = _wcsdup(commandLine);

	DO_LOG(L"Creating environment block...");
    b = ::CreateEnvironmentBlock(&pEnvironment, hImpersonationToken, FALSE);
    if (!b)
	{
		CWin32LastError error;
		DO_LOG(L"CreateEnvironmentBlock failed: %s (%d)", error.GetMessage(), error.GetCode());
	}

    if (hImpersonationToken != NULL)
    {
		DO_LOG(L"Creating process by impersonation token..");
        b = ::CreateProcessAsUser(hImpersonationToken, NULL, parameters, 
            NULL, NULL, FALSE, CREATE_UNICODE_ENVIRONMENT | CREATE_SUSPENDED | creationFlags, 
            pEnvironment, executeFolder, &pStartUp, &pi);
		if (!b)
		{
			CWin32LastError error;
			DO_LOG(L"CreateProcessAsUser failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
		}
    }
    else
    {
		DO_LOG(L"Creating process without impersonation token...");
        b = ::CreateProcess(NULL, parameters, 
            NULL, NULL, FALSE, CREATE_UNICODE_ENVIRONMENT | CREATE_SUSPENDED | creationFlags, 
            pEnvironment, executeFolder, &pStartUp, &pi);
		if (!b)
		{
			CWin32LastError error;
			DO_LOG(L"CreateProcess failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
		}
    }

    if (b)
    {
        DO_LOG(L"Process information: PID=%d, TID=%d, PHND=%08X, THND=%08X", pi.dwProcessId, pi.dwThreadId, pi.hProcess, pi.hThread);
		DO_LOG(L"Setting privileges...");
        b = ImpersonationHelper::SetProcessPrivilege(pi.dwProcessId, SE_SHUTDOWN_NAME, TRUE);
        if (!b)
        {
			CWin32LastError error;
			DO_LOG(L"SetProcessPrivilege for SE_SHUTDOWN_NAME failed: %s (%d)", error.GetMessage(), error.GetCode());
        }

        b = TRUE;

		DO_LOG(L"Resuming main thread...");
        ::ResumeThread(pi.hThread);

        if (waitForQuit)
        {
            DO_LOG(L"Waiting for quit..");
            ::WaitForSingleObject(pi.hProcess, INFINITE);
            ::GetExitCodeProcess(pi.hProcess, &dwExitCode);
			vector<DWORD> dwExitCodes;
			do
			{
				vector<DWORD> processesIds;
				EnumChildProcesses( pi.dwProcessId, processesIds );
				if( processesIds.empty() )
				{
					break;
				}
					
				WaitForMultipleProcesses( processesIds, dwExitCodes );
				GetBiggestDWORD( dwExitCodes, dwExitCode );
			}
			while( 0 );

            ::CloseHandle(pi.hThread);
            ::CloseHandle(pi.hProcess);
	        DO_LOG(L"Completed, exit code = %u", dwExitCode);
        }
        else
        {
            ::CloseHandle(pi.hThread);
            if (phProcessHandle)
                *phProcessHandle = pi.hProcess;
	        DO_LOG(L"Completed");
        }
    }

    if (pEnvironment)
    {
        DestroyEnvironmentBlock(pEnvironment);
    }


	if (parameters)
	{
		free(parameters);
	}

    return (b == TRUE);
}
