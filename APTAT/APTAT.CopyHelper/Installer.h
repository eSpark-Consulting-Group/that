#pragma once

#include <windows.h>
#include <vector>
#include <string>
#include "DescentStatus.h"
#include "InstallReporter.h"

using namespace AgentCommonTools;


/*
    Arguments:
    
    0 - executable name (GUID) - automatic assigning
    1 - command
    2 - host name (for a mailslot or a pipe)
    3 - key (unsigned int) - ignored
    ... additional parameters

    Commands:
    process_msi (msi1, msi2, ...). Msi format: fileName*arguments - (** Using Mailslot)
    process_script (inFile, outFile, timeoutInSeconds).
    agent_uninstall - (** Using Mailslot)
    agent_install (msiFile) - (** Using Mailslot)
    process_response_action (ActionName, OutputFileName, Timeout (in ms), Parameter1, Parameter2 ... ParameterN)
*/
extern bool g_runFromService;

struct RemoteInstallerService : CInstallReporter::IInstallationListener
{
    struct tMSI
    {
        CString path;
        CString parameters;
    };

    CString m_ServerHostName; // server host name extracted from server.ini
    CString m_Command;
	CString m_lastMsiMessage;
	int m_currentPercentage;
	mutable CRITICAL_SECTION m_cs;
	SERVICE_STATUS m_ss;
	SERVICE_STATUS_HANDLE m_ssh;

	int m_argc;
	LPTSTR* m_argv;

    // Error handling
    UINT m_err;
    DescentStatus m_status;
    void setStatus(DescentStatus status)
    {
		DO_LOG(L"Change Status from %d to %d", m_status, status);
	    m_status = status;
    }
    void setNativeError(UINT err){m_err = err;}
    void updateStatusByMsiError();

    void formatContextError(LPCWSTR format, ...);
    void setContextError(LPCWSTR error);
    CString m_ErrorString;
    CString m_responseString;
	CString m_responseFile;
	SystemKind m_systemKind;
    void formatResponse(LPCWSTR format, ...);

    // Methods
    RemoteInstallerService();
	~RemoteInstallerService();
	static void extractParametersFromPath(LPCWSTR source, CString& path, CString& parameters);
    bool InitService(DWORD dwArgc, LPTSTR* lpszArgv);
    void ReleaseService();
	DWORD WINAPI Handler(DWORD dwControl);

	static DWORD WINAPI HandlerEx(DWORD dwControl, DWORD dwEventType, LPVOID lpEventData, LPVOID lpContext);

    bool InstallMsi(const tMSI &msi);
	bool InstallMsi(int argc, LPTSTR argv[]);
    int ServiceMain();

    // Process commands
    int process_response_action(int argc, LPTSTR argv[]);
    int process_msi(int argc, LPTSTR argv[]);
    int process_script(int argc, LPTSTR argv[]);
    int process_agent_install(int argc, LPTSTR argv[]);
    int process_agent_uninstall(int argc, LPTSTR argv[]);

    // ----------------------------
    // Response actions
    // ----------------------------

    // Pre-install agent checking. Output: platform (32 or 64)
    int action_install_agent_check(int argc, LPTSTR argv[]);

	bool writeStatusToFile(CString responseFile, bool forceWrite) const;
    
    // Query installed agent information. 
    // Output: isInstalled, VersionMajor, VersionMinor, ServerPort, ServerHostName, CurrentServerAccessible
    int action_query_agent_information(int argc, LPTSTR argv[]);

    // Install agent. Input: msiPath
	int action_install_msi(CString responseFile, int argc, LPTSTR argv[]);
    int action_install_agent(int argc, LPTSTR argv[]);
	int action_install_exe(int argc, LPTSTR argv[], int timeOut);
	int action_copy_files(int argc, LPTSTR argv[]);
    int action_uninstall_agent(int argc, LPTSTR argv[]);


    bool checkConnectionToRemoteServer_(int HostPort);
    bool checkInstalledAgentCompatibilityToThisServer_(AgentInfo info, int HostPort);
	virtual void OnInstallProgress(int percentage);
	virtual void OnInstallMessage(LPCWSTR message);
	virtual void OnInstallTerminate();
};
