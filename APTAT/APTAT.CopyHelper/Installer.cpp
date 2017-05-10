#include "stdafx.h"
#include "Installer.h"

using namespace AgentCommonTools;
bool g_runFromService = true;



RemoteInstallerService::RemoteInstallerService() : m_ssh(0), m_status(status_error), 
													m_err(0), m_currentPercentage(0)													
{
	::InitializeCriticalSection(&m_cs);
	memset(&m_ss, 0, sizeof(m_ss));
	m_ss.dwServiceType = SERVICE_WIN32_OWN_PROCESS;

	m_ss.dwControlsAccepted = SERVICE_CONTROL_INTERROGATE;
	m_ss.dwControlsAccepted |= SERVICE_ACCEPT_STOP;
}
RemoteInstallerService::~RemoteInstallerService()
{
	::DeleteCriticalSection(&m_cs);
}

bool RemoteInstallerService::InitService(DWORD dwArgc, LPTSTR* lpszArgv)
{
	assert(dwArgc > 0);

	WSADATA WSAData;
	::WSAStartup(MAKEWORD(2, 2), &WSAData);

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
	for (int i = 0; i < m_argc; i++)
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
	DO_LOG("Stopping service");
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
	DO_LOG("CtrlHandler %d", dwControl);
	switch (dwControl)
	{
		case SERVICE_CONTROL_INTERROGATE:
			return NO_ERROR;
		case SERVICE_CONTROL_STOP:
			DO_LOG("Service stop request")
			ReleaseService();
			return NO_ERROR;
		default:
			return ERROR_CALL_NOT_IMPLEMENTED;
	}
}

DWORD WINAPI RemoteInstallerService::HandlerEx(DWORD dwControl, DWORD dwEventType, LPVOID lpEventData, LPVOID lpContext)
{
    return ((RemoteInstallerService*)lpContext)->Handler(dwControl);
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

	auto ret = process_response_action(m_argc, m_argv);
	return ret;

    // Error
    setStatus(status_error);
    setNativeError(ERROR_INVALID_FUNCTION);
    return -1;
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

