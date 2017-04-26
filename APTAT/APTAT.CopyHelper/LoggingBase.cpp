#include "stdafx.h"

#include "LoggingBase.h"
#include <string>

namespace trace 
{
	const TCHAR* strDebug =		_T("debug");
	const TCHAR* strInfo =		_T("info");
	const TCHAR* strWarn =		_T("warn");
	const TCHAR* strError =		_T("error");
	const TCHAR* strFatal =		_T("fatal");

	const TCHAR* SeverityString(eSeverity severity) {
		switch(severity) {
			case trace::debug:
				return strDebug;
			case trace::info:
				return strInfo;
			case trace::warning:
				return strWarn;
			case trace::error:
				return strError;
			case trace::fatal:
				return strFatal;
			default:
				return _T("unknown");
		}
	}


	//method to add appenders to the vector
	void CTraceManager::AddListener(CTraceListener* listener) 
	{
		m_listeners.Add(listener);
	}

	void CTraceManager::RemoveListeners()
	{
		m_listeners.RemoveAll();
	}

	void CTraceManager::Write(eSeverity severity, long code, const TCHAR* message) {
		UINT count = (UINT)m_listeners.GetCount();
		for(UINT i = 0; i<count; i++) 
		{
			m_listeners[i]->Write(severity, code, message);
		}
	}

	void CTraceManager::Write(eSeverity severity, long code, const TCHAR* format, va_list argptr)
	{
		int len = _vsctprintf(format, argptr)+1;
		TCHAR* message =(TCHAR*)malloc(len * sizeof(TCHAR));
		ATLASSUME(message);
		_vstprintf_s(message, len, format, argptr); // C4996
		Write(severity, code, message);
		free(message);
	}

	CTraceManager Trace::m_manager;

	void Trace::AddListener(CTraceListener* listener)
	{
		m_manager.AddListener(listener);
	}

	void Trace::RemoveListeners()
	{
		m_manager.RemoveListeners();
	}

	void Trace::Write(eSeverity severity, long code, const TCHAR* message)
	{
		m_manager.Write(severity, code, message);
	}

	void Trace::Write(eSeverity severity, long code, const TCHAR* format, va_list argptr)
	{
		m_manager.Write(severity, code, format, argptr);
	}
}