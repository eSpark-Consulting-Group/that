#ifndef _LOGGINGIMPL_H_
#define _LOGGINGIMPL_H_

#include "LoggingBase.h"

//#include <iostream>
//#include <fstream>

namespace trace 
{
	class DllExport CTextListener: 
		public CTraceListener 
	{
	protected:
		virtual void WriteLine(const TCHAR* line)=0;
	public:
		void Write(eSeverity severity, long code, const TCHAR* message);
	};

	class DllExport COutputDebugStringListener: 
		public CTextListener 
	{
		CString m_prefix;
	protected:
		void WriteLine(const TCHAR* line);
	public:
		COutputDebugStringListener(const TCHAR* prefix);
	};

	class DllExport CConsoleListener: 
		public CTextListener 
	{
	protected:
		void WriteLine(const TCHAR* line);
	};


	class DllExport CFileListener: 
		public CTextListener 
	{
		HANDLE m_hfile;
	protected:
		void WriteLine(const TCHAR* line);
	public:
		CFileListener(const TCHAR* filename);
		~CFileListener();
	};

	class DllExport CEventLogListener: 
		public CTraceListener 
	{
		HANDLE m_handle;
	public:
		CEventLogListener(const TCHAR* source);
		bool RegisterSource(CString lpszApp, CString lpszEventMessageDll);
		~CEventLogListener();

		void Write(eSeverity severity, long code, const TCHAR* message);
	};
}
#endif //_LOGGINGIMPL_H_