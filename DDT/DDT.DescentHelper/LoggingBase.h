#ifndef _LOGGINGBASE_H_
#define _LOGGINGBASE_H_

#pragma warning( disable : 4786)
#pragma warning( disable : 4793)
#pragma warning( disable : 4251)

#include "BaseTypes.h"
#include "trace_enum.h"
#include <atlcoll.h>

namespace trace 
{
	DllExport const TCHAR* SeverityString(eSeverity threshold);

	class DllExport CTraceFilterBase
	{
	public:
		virtual ~CTraceFilterBase(){}
		virtual bool Pass(eSeverity severity, long code, const TCHAR* message) const = 0;
	};

	class DllExport CTraceFilterSeverity:
		public CTraceFilterBase
	{
		static const int all=debug|info|warning|error|fatal;
		int m_severities;
	public:
		CTraceFilterSeverity():m_severities(all)
		{}
		CTraceFilterSeverity(int severities)
		{
			SetSeverities(severities);
		}

		void SetSeverities(int severities)
		{
			//assert(0==(filter & ~all));
			m_severities=severities;
		}
		bool Pass(eSeverity severity, long code, const TCHAR* message) const 
		{
			return 0!=(severity & m_severities);
		}
	};


	class DllExport CTraceListener
	{
	protected:
		CTraceFilterBase* m_filter;
	public:
		CTraceListener(): m_filter(NULL) {}
		void SetFilter(CTraceFilterBase* filter)
		{
			m_filter=filter;
		}
		virtual ~CTraceListener(){}
		virtual void Write(eSeverity severity, long code, const TCHAR* message) = 0;
	};

	class DllExport CTraceManager 
	{
	public:
		void AddListener(CTraceListener* listener);
		void RemoveListeners();
		void Write(eSeverity severity, const TCHAR* message)
		{
			Write(severity, 0, message);
		}
		void Write(eSeverity severity, long code, const TCHAR* message);
		void Write(eSeverity severity, long code, const TCHAR* format, va_list argptr);
	private:
#pragma warning(suppress:4251)
		CAtlArray<CTraceListener*> m_listeners;
	};


	class DllExport Trace
	{
	protected:
		static CTraceManager m_manager;
		static void Write(eSeverity severity, long code, const TCHAR* message);
		static void Write(eSeverity severity, long code, const TCHAR* format, va_list argptr);
	public:
		static void AddListener(CTraceListener* listener);
		static void RemoveListeners();

		//static void Debug(long code, const TCHAR* message)
		//{
		//	Write(trace::debug, code, message);
		//}
		//static void Debug(const TCHAR* message)
		//{
		//	Write(trace::debug, 0, message);
		//}
		static void Debug(long code, const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::debug, code, format, args);
		}
		static void Debug(const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::debug, 0, format, args);
		}

		//static void Info(long code, const TCHAR* message)
		//{
		//	Write(trace::info, code, message);
		//}
		//static void Info(const TCHAR* message)
		//{
		//	Write(trace::info, 0, message);
		//}
		static void Info(long code, const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::info, code, format, args);
		}
		static void Info(const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::info, 0, format, args);
		}

		//static void Warning(long code, const TCHAR* message)
		//{
		//	Write(trace::warning, code, message);
		//}
		//static void Warning(const TCHAR* message)
		//{
		//	Write(trace::warning, 0, message);
		//}
		static void Warning(long code, const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::warning, code, format, args);
		}
		static void Warning(const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::warning, 0, format, args);
		}

		//static void Error(long code, const TCHAR* message)
		//{
		//	Write(trace::error, code, message);
		//}
		//static void Error(const TCHAR* message)
		//{
		//	Write(trace::error, 0, message);
		//}
		static void Error(long code, const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::error, code, format, args);
		}
		static void Error(const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::error, 0, format, args);
		}

		//static void Fatal(long code, const TCHAR* message)
		//{
		//	Write(trace::fatal, code, message);
		//}
		//static void Fatal(const TCHAR* message)
		//{
		//	Write(trace::fatal, 0, message);
		//}
		static void Fatal(long code, const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::fatal, code, format, args);
		}
		static void Fatal(const TCHAR* format, ...)
		{
			va_list	args;
			va_start(args, format);
			Write(trace::fatal, 0, format, args);
		}
	};

};

#pragma warning( default : 4793)

#endif //_LOGGINGBASE_H_