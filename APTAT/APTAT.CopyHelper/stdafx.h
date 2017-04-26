// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once
#pragma warning(disable: 4482) // nonstandard extension used: enum 'Enum' used in qualified name

#ifndef WINVER                          // Specifies that the minimum required platform is Windows Vista.
#define WINVER 0x0600           // Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINNT            // Specifies that the minimum required platform is Windows Vista.
#define _WIN32_WINNT 0x0600     // Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINDOWS          // Specifies that the minimum required platform is Windows 98.
#define _WIN32_WINDOWS 0x0410 // Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE                       // Specifies that the minimum required platform is Internet Explorer 7.0.
#define _WIN32_IE 0x0700        // Change this to the appropriate value to target other versions of IE.
#endif


#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <winsock2.h>

// C RunTime Header Files
#define _CRT_RAND_S
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>

#include <msi.h>
#include <shellapi.h>
#include <assert.h>
#include <comdef.h>

#include <string>
#include <vector>
#include <map>

#include <atlbase.h>


typedef ATL::CComAutoCriticalSection CAutoCriticalSection;
typedef ATL::CComCritSecLock<ATL::CComCriticalSection> CLocker;

#import <msxml6.dll>
using namespace std;

#include "resource.h"

// Comment the following define to have an ability to 
// run/debug application from the command line (without service)
//#define _RUN_FROM_SERVICE_ don't use anymore (use g_runFromService instead)

#include "DebugLogger.h"
#include "BaseExceptionsLT.h"
#include "AgentInstallUtils.h"
//#include "FormattedText.h"
#include "DefaultStrings.h"

using namespace AgentCommonTools;
