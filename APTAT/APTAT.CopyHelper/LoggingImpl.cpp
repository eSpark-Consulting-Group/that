#include "stdafx.h"

#include "LoggingImpl.h"

//#include <iostream>
//#include <fstream>
#include <time.h>

namespace trace 
{
	void CTextListener::Write(eSeverity severity, long code, const TCHAR* message) 
	{
		if(m_filter && !m_filter->Pass(severity, code, message))
			return;
		TCHAR buff[4096];

		struct tm today;
        __time64_t long_time;
        _time64( &long_time ); 
        _localtime64_s( &today, &long_time ); 

		_stprintf_s(buff, sizeof(buff)/sizeof(buff[0]), 
			_T("%02d/%02d/%04d %02d:%02d:%02d| %-5s | %d | %s"), 
			today.tm_mday,
			today.tm_mon + 1,
			today.tm_year + 1900,
			today.tm_hour,
			today.tm_min,
			today.tm_sec,
			SeverityString(severity), code, message);
		WriteLine(buff);
	}

	void CConsoleListener::WriteLine(const TCHAR* line)
	{
		_tprintf(_T("%s\n"), line);
	}

	COutputDebugStringListener::COutputDebugStringListener(const TCHAR* prefix)
	{
		ATLASSERT(prefix);
		m_prefix=prefix;
	}


	void COutputDebugStringListener::WriteLine(const TCHAR* line)
	{
		CString str(m_prefix);
		str.Append(_T("=>"));
		str.Append(line);
		str.Append(_T("\n"));
		::OutputDebugString(str.GetString());
	}

	CFileListener::CFileListener(const TCHAR* filename)
	{
		m_hfile=::CreateFile(filename, 
			GENERIC_WRITE, 
			FILE_SHARE_READ, 
			NULL, 
			OPEN_ALWAYS, 
			FILE_ATTRIBUTE_NORMAL,
			NULL);
	}

	CFileListener::~CFileListener() 
	{
		if(INVALID_HANDLE_VALUE!=m_hfile)
		{
			CloseHandle(m_hfile);
			m_hfile=INVALID_HANDLE_VALUE;
		}
	}

	void CFileListener::WriteLine(const TCHAR* line) 
	{
		if(INVALID_HANDLE_VALUE!=m_hfile)
		{
			CString str(line);
			str.Append(_T("\n"));
			DWORD dwBytes=sizeof(TCHAR)*str.GetLength();
			DWORD written=0;
			DWORD dwPos = SetFilePointer(m_hfile, 0, NULL, FILE_END);
			LockFile(m_hfile, dwPos, 0, dwBytes, 0);
			DWORD res=WriteFile(m_hfile, str.GetString(), dwBytes, &written, NULL);
			UnlockFile(m_hfile, dwPos, 0, dwBytes, 0);
			FlushFileBuffers(m_hfile);
		}
	}

	CEventLogListener::CEventLogListener(const TCHAR* source)
	{
		RegisterSource( (LPCTSTR)source, _T("AgnEvt.dll"/*"XEventMessage.dll"*/) );
		m_handle = ::RegisterEventSource(NULL, source);
	}

	bool CEventLogListener::RegisterSource(CString lpszApp, CString lpszEventMessageDll)
	{
		if (lpszApp.IsEmpty())
			return FALSE;

		CString szRegPath(_T("SYSTEM\\CurrentControlSet\\Services\\Eventlog\\Comodo\\"));

		// open the registry event source key
		DWORD dwResult = 0;
		HKEY hKey = NULL;
		LONG lRet = ::RegCreateKeyEx(HKEY_LOCAL_MACHINE, szRegPath, 0, NULL,
						REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, &dwResult);

		szRegPath += lpszApp;
		
		lRet = ::RegCreateKeyEx(HKEY_LOCAL_MACHINE, szRegPath, 0, NULL,
						REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, &dwResult);

		if (lRet == ERROR_SUCCESS)
		{
			CString szPathName;
			
			wchar_t szPath[MAX_PATH*2] = {0};
			// use app's directory + "XEventMessage.dll"
			::GetModuleFileName(NULL, szPath, MAX_PATH*2-2);
			szPathName = szPath;

			int SlashPos = szPathName.ReverseFind(_T('\\'));
			int Len = szPathName.GetLength();
			szPathName.Delete(SlashPos, Len - SlashPos);
			
			szPathName += "\\";
			szPathName += lpszEventMessageDll;

			Len = szPathName.GetLength();
			wchar_t szPath1[MAX_PATH*2] = {0};
			wcscpy_s(szPath1, sizeof(szPath1)/sizeof(szPath1[0]), szPathName.GetBuffer());
			//_tcscpy(szPath, szPathName.GetBuffer());
			lRet = ::RegSetValueEx(hKey,  _T("EventMessageFile"), 0, REG_SZ,
				(const BYTE*)szPath1, Len);

			// === write TypesSupported key ===

			// message DLL supports all types
			DWORD dwSupportedTypes = EVENTLOG_SUCCESS			|
									 EVENTLOG_ERROR_TYPE		|
									 EVENTLOG_WARNING_TYPE		|
									 EVENTLOG_INFORMATION_TYPE  |
									 EVENTLOG_AUDIT_SUCCESS		|
									 EVENTLOG_AUDIT_FAILURE;

			lRet = ::RegSetValueEx(hKey, _T("TypesSupported"), 0, REG_DWORD,
				(const BYTE *) &dwSupportedTypes, sizeof(DWORD));

			lRet = ::RegCloseKey(hKey);

			return TRUE;
		}

		return FALSE;
	}

	CEventLogListener::~CEventLogListener() {
		if(NULL != m_handle) {
			DeregisterEventSource(m_handle);
			m_handle = NULL;
		}
	}

	void CEventLogListener::Write(eSeverity severity, long code, const TCHAR* message) 
	{
		if(m_filter && !m_filter->Pass(severity, code, message))
			return;
		WORD etype=EVENTLOG_INFORMATION_TYPE;
		switch(severity) {
			case trace::warning:
				etype=EVENTLOG_WARNING_TYPE;
				break;
			case trace::error:
			case trace::fatal:
				etype=EVENTLOG_ERROR_TYPE;
				break;
		}

		if(NULL != m_handle) {
			::ReportEvent(m_handle,
				etype,
				0,
				0x20000001L,
				NULL,
				1,
				0,
				(LPCTSTR*)&message,
				NULL);
		}
	}

}