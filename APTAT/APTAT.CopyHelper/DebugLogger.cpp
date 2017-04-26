#include "stdafx.h"
#include <windows.h>
#include "DebugLogger.h"
#include <assert.h>
#include "AgentRegistry.h"

#define CHECK_LOGGING_ENABLED if(!m_bLoggingEnabled && !m_bLoggingSourceEnabled)return;
//#define LOG_ALWAYS

CDebugLogger::CDebugLogger() : 
    m_flags(0),
    m_bLoggingEnabled(false),
    m_hFile(INVALID_HANDLE_VALUE),
	m_pListener(NULL),
	m_bLoggingSourceEnabled(false)
{
    ::InitializeCriticalSection(&m_cs);
}

CDebugLogger::~CDebugLogger()
{
    ::DeleteCriticalSection(&m_cs);
}

void CDebugLogger::EnableLoggingSource(bool enabled)
{
	if(!m_bLoggingEnabled)
		init(false, NULL);

	m_bLoggingSourceEnabled = enabled;
}

void CDebugLogger::registerLogSource()
{
	typedef void (*RegisterLogSourcePtr)(ILogSource* source);
	RegisterLogSourcePtr pRegisterLogSource = NULL;
	HMODULE h = ::GetModuleHandle(L"AgnCore.dll");
	if(h != NULL)
	{
		pRegisterLogSource = (RegisterLogSourcePtr)::GetProcAddress(h, "RegisterLogSource");
		if(pRegisterLogSource != NULL)
		{
			pRegisterLogSource(this);
		}
	}
}

int CDebugLogger::init( bool bLoggingEnabled, const wchar_t* logPath, 
                           const wchar_t* debugPrefix, unsigned flags)
{
    m_flags = flags;
    m_bLoggingEnabled = bLoggingEnabled;

    setDebugPrefix(debugPrefix);

    if( logPath != NULL)
    {
		CString targetPath(logPath);
		if(targetPath.Find(L"\\") == -1 && targetPath.Find(L"/") == -1)
		{
			targetPath = "";
			char szCurDir[MAX_PATH] = {0};
			GetModuleFileNameA( NULL, szCurDir, MAX_PATH - 1 );
			memset( strrchr( szCurDir, '\\' ) + 1, 0, strlen( strrchr( szCurDir, '\\' ) ) );
			targetPath += szCurDir;
			targetPath += L"CESM_AGENT_LOGS\\";
			::CreateDirectory(LPCTSTR(targetPath), NULL);
			targetPath += logPath;
		}
		if( bLoggingEnabled == true )
		{
			m_hFile = ::CreateFile(LPCTSTR(targetPath), GENERIC_WRITE,FILE_SHARE_READ,0,OPEN_ALWAYS, 0,0);
			if(m_hFile == INVALID_HANDLE_VALUE)
			{
				UINT nTryCountMax = 100;
				for( UINT i = 0 ; i < nTryCountMax ; i++ )
				{
					DWORD dwError = ::GetLastError();

					int dotPos = targetPath.Find( L"." );
					wchar_t szInt[ 32 ] = { 0 };
					_itow_s( i, szInt, 10 );

					if( dotPos != -1 )
					{
						targetPath.Insert(dotPos, szInt);
					}
					else
					{
						targetPath += szInt;
					}

					m_hFile = ::CreateFile(LPCTSTR(targetPath), GENERIC_WRITE,FILE_SHARE_READ,0,OPEN_ALWAYS, 0,0);

					if(m_hFile != INVALID_HANDLE_VALUE)
					{
						break;
					}
				}
			}
			else
				::SetFilePointer(m_hFile, NULL, NULL, FILE_END);
		}
		m_logFilePath = targetPath;
    }

	if( m_bLoggingEnabled == false )
	{
		if( m_hFile != INVALID_HANDLE_VALUE )
		{
			CloseHandle( m_hFile );
			m_hFile = INVALID_HANDLE_VALUE;
		}
	}

	if( m_hFile != INVALID_HANDLE_VALUE )
	{
		DWORD dwFileSizeLow = 0;
		DWORD dwFileSizeHigh = 0;
		dwFileSizeLow = GetFileSize( m_hFile, &dwFileSizeHigh );
		if(  dwFileSizeLow == 0  )
		{
			prepareUnicodeHeaderFile( m_hFile );
		}

		final_message( L"-------------- LOG SESSION HAS BEEN STARTED ---------------\r\n" );
	}

    return ERROR_SUCCESS;
}

void CDebugLogger::prepareUnicodeHeaderFile( HANDLE& hFile )
{
	unsigned char header[] = { 0xFF, 0xFE };
	DWORD written = 0;

	::EnterCriticalSection(&m_cs);
	::WriteFile( hFile, &header, 2, &written, NULL );
	::LeaveCriticalSection(&m_cs);
}

void CDebugLogger::setDebugPrefix(const wchar_t* prefix)
{
    m_debugPrefix = prefix;

    // if debug prefix is not empty, append additional space
	if(!m_debugPrefix.IsEmpty())
    {
        m_debugPrefix += L" ";
    }
}

void CDebugLogger::dumpFile(const char* filePath, const void* data, size_t len)
{
	HANDLE hFile = ::CreateFileA(filePath, GENERIC_WRITE, 0, 0, CREATE_ALWAYS, 0, 0);
	if(hFile == INVALID_HANDLE_VALUE)
		return;

	DWORD dw = 0;
	::WriteFile(hFile, data, (DWORD)len, &dw, NULL);
	::CloseHandle(hFile);
}


void CDebugLogger::message(const char* context, const wchar_t* format, ...)
{
    CHECK_LOGGING_ENABLED;

	CString s;

	try
	{
	    va_list args;
	    va_start (args, format);
		s.FormatV(format, args);
	    va_end (args);
	}
	catch(...)
	{
		proceed_message("LOGGING EXCEPTION 1", L"CString format exception\r\n");
		return;
	}

	s += L"\r\n";

	proceed_message(context, LPCTSTR(s));
}

void CDebugLogger::message(const char* context, const char* format, ...)
{
	try
	{
		CHECK_LOGGING_ENABLED;

		CStringA s;
	
		try
		{
			va_list args;
			va_start (args, format);
			s.FormatV(format, args);
			va_end (args);
		}
		catch(...)
		{
			proceed_message("LOGGING EXCEPTION 2:", L"CString format exception\r\n");
			return;
		}

		s += L"\r\n";
		CString result(s);

		if (result.GetLength() > 1024)
		{
			proceed_message("LOGGING WARNING:", L"Message is pretty long. Consider using DO_LOG_HUGE instead\r\n");
		}

		proceed_message(context, LPCTSTR(result));
	}
	catch( CAtlException& ex )
	{
		DO_LOG( "ATL through CAtlException exception:code=%d", ex.m_hr );
		return;
	}
}

void CDebugLogger::proceed_message(const char* context, const wchar_t* buffer)
{
	CHECK_LOGGING_ENABLED;

	CString result = m_flags & flag_prefix ? m_debugPrefix : L"";

	if( m_flags & flag_time )
	{
		SYSTEMTIME st = {0};
		::GetLocalTime( &st );

		result.AppendFormat( L"[%02d:%02d:%02d:%03d] ", 
			(int)st.wHour, (int)st.wMinute, (int)st.wSecond, (int)st.wMilliseconds);
	}

	if( m_flags & flag_login_context )
	{
		wchar_t UserName[512] = {0};
		DWORD cbUserName = sizeof(UserName) / sizeof(UserName[0]);
		::GetUserNameW( UserName, &cbUserName );

		result.AppendFormat(L"[%s] ", UserName);
	}


	if(m_flags & flag_context)
	{
		result.AppendFormat( L"[%S] ", context );
	}

	if(m_flags & flag_thread_id)
	{
		result.AppendFormat(L"[THREAD 0x%08X] ", ::GetCurrentThreadId() );
	}

	result += buffer;

	final_message( result );
}

void CDebugLogger::appendLine()
{
	final_message(L"\r\n");
}

void CDebugLogger::final_message( const wchar_t* buffer )
{
	if(m_pListener != NULL && m_bLoggingSourceEnabled)
	{
		m_pListener->OnLogMessage(this, buffer);
	}

	if(m_bLoggingEnabled)
	{
		DWORD dw = 0;

		::EnterCriticalSection(&m_cs);

		DWORD limitLogFileSize = 100*1024*1024;

		DWORD dwFileSizeLow = 0;
		DWORD dwFileSizeHigh = 0;
		if( ( dwFileSizeLow = GetFileSize( m_hFile, &dwFileSizeHigh ) ) != INVALID_FILE_SIZE )
		{
			if( dwFileSizeLow > limitLogFileSize )
			{
				SetFilePointer( m_hFile, 0, NULL, FILE_BEGIN );
				SetEndOfFile( m_hFile );
			}
		}

		if(m_hFile != INVALID_HANDLE_VALUE)
		{
			size_t size = wcslen( buffer ) * sizeof( wchar_t );
			::WriteFile( m_hFile, buffer, (DWORD)size, &dw, NULL);
			::FlushFileBuffers(m_hFile);
		}

		::LeaveCriticalSection(&m_cs);
	}
}


void CDebugLogger::SetLogListener(ILogSourceListener* listener)
{
	m_pListener = listener;
}

tLoggerAutoInit::~tLoggerAutoInit()
{
	g_pLoggerAutoInit = NULL;
}

void GetErrorString(HRESULT code, CString& message)
{
	TCHAR* lpMsgBuf;
	FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER|FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		code,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0, NULL );
	message = lpMsgBuf;
	LocalFree(lpMsgBuf);
}

// =============================================
tLoggerAutoInit* g_pLoggerAutoInit = NULL;
tLoggerAutoInit::tLoggerAutoInit(const wchar_t* logPath, const wchar_t* prefix, bool bAllowEnableLoggingKey )
{
    g_pLoggerAutoInit = this;

	DWORD loggingEnabled = 0;
	if( ( bAllowEnableLoggingKey == false ) || ( !LoadAgentAttributeFromTheRegistry(L"EnableLogging", loggingEnabled) || !loggingEnabled ))
    {
        // No file created... Logging is disabled
        m_logger.enableLogging(false);
    }
    else
    {
        // Logging enabled
		DWORD dwError = m_logger.init(true, logPath);
        if( dwError == ERROR_SUCCESS )
		{
			m_logger.setDebugPrefix(prefix);
			m_logger.enableFlags(CDebugLogger::flag_context | CDebugLogger::flag_thread_id | CDebugLogger::flag_login_context | CDebugLogger::flag_prefix );
		}
		else
		{
			trace::CEventLogListener m_traceListener( _T( "CesmAgentService.DebugLogger" ) );
			
			TCHAR szInt[16] = { 0 };
			_itot_s( dwError, szInt, 10 );
			
			CString strError;
			CString strMessage;
			GetErrorString( dwError, strError );
			strMessage = _T( "CesmAgentService can not write log file:\r\n Error = " ) + CString( szInt ) + _T( "\r\nMessage:" ) + strError;
			m_traceListener.Write( trace::error, -1, strMessage );
		}
    }
}
