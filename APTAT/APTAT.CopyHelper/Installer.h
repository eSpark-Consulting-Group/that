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

struct RemoteInstallerService
{
	CString m_Command;
	int m_currentPercentage;
	mutable CRITICAL_SECTION m_cs;
	SERVICE_STATUS m_ss;
	SERVICE_STATUS_HANDLE m_ssh;

	int m_argc;
	LPTSTR* m_argv;

	// Error handling
	UINT m_err;
	vector<UINT> m_fileStatus;
	vector<CString> m_filePath;
	

	DescentStatus m_status;
	void setStatus(DescentStatus status)
	{
		DO_LOG(L"Change Status from %d to %d", m_status, status);
		m_status = status;
	}
	void setNativeError(UINT err) { m_err = err; }

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
	bool InitService(DWORD dwArgc, LPTSTR* lpszArgv);
	void ReleaseService();
	DWORD WINAPI Handler(DWORD dwControl);

	static DWORD WINAPI HandlerEx(DWORD dwControl, DWORD dwEventType, LPVOID lpEventData, LPVOID lpContext);


	int ServiceMain();

	// Process commands
	int process_response_action(int argc, LPTSTR argv[]);


	// ----------------------------
	// Response actions
	// ----------------------------


	bool writeStatusToFile(CString responseFile, bool forceWrite) const;


	int action_copy_file(int argc, LPTSTR argv[]);
	int action_copy_files(int argc, LPTSTR argv[]);
};
