#include "stdafx.h"
#include "AgentCommon.h"
#include "AgentRegistry.h"
#include "AgentCommonTools.h"
#include "AgentInstallUtils.h"
#include <MsiQuery.h>
#include <algorithm>
#include <atlenc.h>

using namespace std;

// =====================================================================================
// AUXILIARY FUNCTIONS
// =====================================================================================

#ifndef _WIN32_WINNT_WIN8
#define _WIN32_WINNT_WIN8 0x0602
#endif

#ifndef _WIN32_WINNT_WINBLUE
#define _WIN32_WINNT_WINBLUE 0x0603
#endif
