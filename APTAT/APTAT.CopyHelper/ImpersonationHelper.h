#pragma once

#include <WtsApi32.h>
#include <Shellapi.h>
#include <Userenv.h>
#include <atlsecurity.h>
#include <Ntsecapi.h>
#include <Tlhelp32.h>
#include <algorithm>

#define INVALID_SESSION_VALUE 0xFFFFFFFF

namespace ImpersonationHelper
{
    extern LPCWSTR AgentCallbackString;
    extern LPCWSTR AgentCallbackCommand_executeProcess;

    LSA_HANDLE GetPolicyHandle();
    void InitLsaString(PLSA_UNICODE_STRING LsaString, LPWSTR String);
    bool AddPrivilegesToAccount(PSID AccountSID, LPWSTR PolicyName);
    BOOL IsPrivilegeSet(HANDLE hToken, LPCTSTR lpszPrivilege);

    BOOL SetTokenPrivilege(
        HANDLE hToken, // access token handle
        LPCTSTR lpszPrivilege, // name of privilege to enable/disable
        BOOL bEnablePrivilege, // to enable or disable privilege
		_Out_opt_ PBOOL pbPreviousState = NULL);

    BOOL SetProcessPrivilege(
        HANDLE hProcess,
        LPCTSTR lpszPrivilege, // name of privilege to enable/disable
        BOOL bEnablePrivilege, // to enable or disable privilege
		_Out_opt_ PBOOL pbPreviousState = NULL);

    BOOL SetProcessPrivilege(
        DWORD ProcessId,
        LPCTSTR lpszPrivilege, // name of privilege to enable/disable
        BOOL bEnablePrivilege, // to enable or disable privilege
		_Out_opt_ PBOOL pbPreviousState = NULL);

    BOOL SetCurrentProcessPrivilege(
        LPCTSTR lpszPrivilege, // name of privilege to enable/disable
        BOOL bEnablePrivilege, // to enable or disable privilege
		_Out_opt_ PBOOL pbPreviousState = NULL);

    /** 
    * GetSessionUserToken Get session user id and return token
    * GetLastError() can be ERROR_PRIVILEGE_NOT_HELD on error
    * 
    * @returns result or NULL
    */
    HANDLE GetSessionUserToken(DWORD dwSessionId, DWORD& err);

    /** 
    * GetActiveSessionUserToken Get active session user id and return token
    * GetLastError() can be ERROR_PRIVILEGE_NOT_HELD on error
	* Do not forget to call ::CloseHandle !!!!
    * 
    * @returns result or NULL
    */
    HANDLE GetActiveSessionUserToken(DWORD& err);

    void GetSessionsUserTokens(std::vector<CHandle>& handles);    
	bool IsLoggedUserExisting();
    BOOL LogonUser(LPCWSTR UserName, LPCWSTR Domain, LPCWSTR Password, HANDLE& hToken);
	void EnumChildProcesses(DWORD ProcessId, std::vector<DWORD>& ProcessesIds);
	HANDLE OpenProcess(DWORD ProcessID);
	HANDLE OpenProcess(DWORD ProcessID, bool& VistaStyle);
	void WaitForMultipleProcesses(std::vector<DWORD>& processesIds, std::vector<DWORD>& dwExitCodes);
	void GetBiggestDWORD( std::vector<DWORD>& arr, DWORD& value );

    // Execute current agent executable with custom parameters with impersonation
    BOOL ExecuteCustomAgentCommandInAnotherProcess(
		LPCWSTR command,
		LPCWSTR params, 
        DWORD& dwExitCode,
        HANDLE hImpersonationToken = NULL, 
        BOOL waitForQuit = TRUE,
        HANDLE* phProcessHandle = NULL,
        DWORD creationFlags = 0);

    // Execute current agent executable with custom parameters with impersonation
    // outString - output string (optional, can be null). Must be released by the caller
    BOOL ExecuteCustomAgentCommandInAnotherProcess(
		LPCWSTR command,
		LPCWSTR params, 
        CAtlString& outString,
        DWORD& dwExitCode,
        HANDLE hImpersonationToken = NULL, 
        BOOL waitForQuit = TRUE,
        HANDLE* phProcessHandle = NULL,
        DWORD creationFlags = 0);

	DWORD ExecuteProcess(int argc, LPCWSTR* argv);
                
    // Executes process and if ERROR_ELEVATION_REQUIRED error occurres, tries to fix it
    BOOL ExecuteProcessEx(
		LPCWSTR commandLine,
		LPCWSTR executeFolder, 
        BOOL waitForQuit, 
        DWORD &dwExitCode,
        HANDLE hImpersonationToken = NULL);

    BOOL ExecuteProcess(
		LPCWSTR commandLine, 
        LPCWSTR executeFolder, 
        BOOL waitForQuit, 
        DWORD &dwExitCode,
        HANDLE hImpersonationToken = NULL,
        HANDLE* phProcessHandle = NULL,
        DWORD creationFlags = 0);

    struct ImpersonationContext
    {
        ImpersonationContext() : m_bImpersonated(FALSE)
        {
        }

        ImpersonationContext(HANDLE h) : m_bImpersonated(FALSE)
        {
            Attach(h);
        }

        ~ImpersonationContext()
        {
            Revert();
        }

        void Attach(HANDLE h)
        {
            if (h != NULL)
                m_bImpersonated = ::ImpersonateLoggedOnUser(h);
        }

        void Revert()
        {
            if(m_bImpersonated)
            {
                ::RevertToSelf();
                m_bImpersonated = FALSE;
            }
        }

        BOOL m_bImpersonated;
    };
}