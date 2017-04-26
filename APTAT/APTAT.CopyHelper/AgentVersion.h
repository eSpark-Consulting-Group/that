#pragma once 
#include "../CommonVersionInfo.h"
#include "SAgentVersion.h" 
#define GLOBAL_AGENT_VERSION SAgentVersion(rcMajor, rcMinor, rcBuild, rcRevision) 

#define AGENT_VERSION szMajorMinorBuildRevision
#define AGENT_COPYRIGHT szCopyright

#define DWORDAGENT_VERSION rcMajor, rcMinor, rcBuild, rcRevision
