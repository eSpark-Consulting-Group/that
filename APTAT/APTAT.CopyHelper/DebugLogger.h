#ifndef _DEBUG_LOGGER_H_
#define _DEBUG_LOGGER_H_

/*
    Simple logger.

    Initialization:

    ==============================
    *** Usage 1 (With DebugLogger.cpp included in the project):
    Disable Precompiled header for DebugLogger.cpp

    stdafx.h:
        #define _ENABLE_DEBUG_LOGGER_
        #include "DebugLogger.h"
    stdafx.cpp:
        #ifdef _ENABLE_DEBUG_LOGGER_
            tLoggerAutoInit g_loggerAutoInit(L"descentAgn.log", L"*** DESCENT AGENT ***");
        #endif

    ==============================
    *** Usage 2 (DebugLogger.cpp is NOT included in the project):
    stdafx.h:
        #define _ENABLE_DEBUG_LOGGER_
        #include "DebugLogger.h"

    stdafx.cpp:

        #define _INLINE_INCLUDE_
        #include "DebugLogger.cpp"

        #ifdef _ENABLE_DEBUG_LOGGER_
            tLoggerAutoInit g_loggerAutoInit("descentMod.log", "*** DESCENT MOD ***");
        #endif


        Usage: 
        DO_LOG("Simple message %d",123);


    Output example:
    *** CESM AGENT *** [18:30:56:390] [TestConsole.cpp:254 (wmain)] [THREAD 0x00000F9C] Simple message 123

    In release mode to enable logging just create file c:\do_log or c:\do_log.txt, where C=System root disk
    In the final version, this macros will be disabled.
*/

#include <string>
#include "LoggingImpl.h"


struct ILogSource;
struct ILogSourceListener
{
	virtual void OnLogMessage(ILogSource* source, const wchar_t* message) = 0;
};

struct ILogSource
{
	virtual void SetLogListener(ILogSourceListener* listener) = 0;
	virtual void EnableLoggingSource(bool enabled) = 0;
};


class CDebugLogger : public ILogSource
{
public:
    enum 
    {
        flag_time = 1 << 0,
        flag_prefix = 1 << 1,
        flag_thread_id = 1 << 2,
        flag_context = 1 << 3,
        flag_login_context = 1 << 4,
		
    };


    CDebugLogger();
    ~CDebugLogger();

    int init(
        bool bLoggingEnabled = true,
        const wchar_t * logPath = L"debug.log", 
        const wchar_t * debugPrefix = L"",
        unsigned flags = flag_time | flag_prefix | flag_thread_id | flag_context | flag_login_context
		);
    void enableLogging(bool b){m_bLoggingEnabled = b;}
    bool isLoggingEnabled() const {return m_bLoggingEnabled || m_bLoggingSourceEnabled;}

    // flags
    void setFlags(unsigned flags){m_flags = flags;}
    unsigned getFlags() const {return m_flags;}
    inline void enableFlags(unsigned flags){setFlags(getFlags() | flags);}
    inline void disableFlags(unsigned flags){setFlags(getFlags() & ~flags);}

    void setDebugPrefix(const wchar_t* prefix);

    void message(const char* context, const char* format, ...);
    void message(const char* context, const wchar_t* format, ...);
	void appendLine();
	void proceed_message(const char* context, const wchar_t* buffer);
	void final_message( const wchar_t* buffer); // wide chars final
	static void dumpFile(const char* filePath, const void* data, size_t len);

	virtual void SetLogListener(ILogSourceListener* listener);

	CString getLogFilePath() const { return m_logFilePath; }

private:
	void registerLogSource();
	void EnableLoggingSource(bool enabled);

	void prepareUnicodeHeaderFile( HANDLE& hFile );
private:
    CString m_debugPrefix; // special debug prefix

    bool m_bLoggingEnabled;
	bool m_bLoggingSourceEnabled; // as logging source only

    HANDLE m_hFile;
	CString m_logFilePath;
    CRITICAL_SECTION m_cs;
    unsigned m_flags;
	ILogSourceListener* m_pListener;
};



struct tLoggerAutoInit
{
    tLoggerAutoInit(const wchar_t* logPath, const wchar_t *prefix, bool bAllowEnableLoggingKey = true );
	~tLoggerAutoInit();
    CDebugLogger m_logger;
};

extern tLoggerAutoInit* g_pLoggerAutoInit;
static bool IS_LOGGING_ENABLED(){ if( g_pLoggerAutoInit == NULL ) return false; if( IsBadCodePtr( (FARPROC)&g_pLoggerAutoInit->m_logger) ) return false; return g_pLoggerAutoInit->m_logger.isLoggingEnabled();}

// This macro returns the actual number of variadic arguments
#define __VA_COUNT(...) __VA_COUNT_IMPL_((__VA_ARGS__,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0))
#define __VA_COUNT_IMPL_(tuple) __VA_COUNT_IMPL tuple
#define __VA_COUNT_IMPL(_0,_1,_2,_3,_4,_5,_6,_7,_8,_9,_10,_11,_12,_13,_14,_15,N,...) N

// For small amount of log data
#define DO_LOG(...)																	\
{																					\
	if (IS_LOGGING_ENABLED())														\
	{																				\
		char __tmp[1024];															\
		char __filePath[MAX_PATH];													\
		char* __FileName;															\
		::GetFullPathNameA(__FILE__,sizeof(__filePath),__filePath, &__FileName);	\
		sprintf_s(__tmp, sizeof(__tmp), "%s:%d", __FileName, __LINE__);				\
		if (__VA_COUNT(__VA_ARGS__))												\
			g_pLoggerAutoInit->m_logger.message(__tmp,__VA_ARGS__);					\
		else																		\
			g_pLoggerAutoInit->m_logger.message(__tmp,_T("%s"),__VA_ARGS__);		\
	}																				\
}

// For large amount of data
#define DO_LOG_HUGE(data) {if(IS_LOGGING_ENABLED()){ char __tmp[1024]; char __filePath[MAX_PATH]; char* __FileName;::GetFullPathNameA(__FILE__,sizeof(__filePath),__filePath, &__FileName); sprintf_s(__tmp,sizeof(__tmp),"%s:%d",__FileName,__LINE__); g_pLoggerAutoInit->m_logger.proceed_message(__tmp, data);g_pLoggerAutoInit->m_logger.appendLine();}}

// Dumping data to separate file
#define DO_DUMP(filePath, data, len) {if(IS_LOGGING_ENABLED()){ g_pLoggerAutoInit->m_logger.dumpFile(filePath, data, len);}}

// DEPRECATED: DO NOT USE
#define LOG_OPERATION(input, output, hr) ___LogOperation(__FUNCTION__, input, output, hr)
static void ___LogOperation(const char* operation, const wchar_t* input, const wchar_t* output, HRESULT hr)
{
	if(IS_LOGGING_ENABLED())
	{
		DO_LOG("<Operation> %s <input>: %S <output> %S <HRESULT> 0x%08X",
			operation,
			input ? input : L"",
			output ? output : L"",
			hr
			);
	}
}

static void ___LogOperation(const char* operation, unsigned long input, const wchar_t* output, HRESULT hr)
{
	if(IS_LOGGING_ENABLED())
	{
		wchar_t buf[255];
		swprintf_s(buf, _countof(buf), L"%d", input);
		___LogOperation(operation, buf, output, hr);
	}
}

static void ___LogOperation(const char* operation, const wchar_t* input, unsigned long output, HRESULT hr)
{
	if(IS_LOGGING_ENABLED())
	{
		wchar_t buf[255];
		swprintf_s(buf, _countof(buf), L"%d", output);
		___LogOperation(operation, input, buf, hr);
	}
}





#endif
