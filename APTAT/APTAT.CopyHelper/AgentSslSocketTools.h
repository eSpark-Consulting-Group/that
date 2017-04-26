#pragma once

#include "SSLSocket.h"
#include "IpEndpoint.h"
#include "AgentInstallUtils.h"

namespace AgentCommonTools
{
	bool ValidateCesmServerSSLPort(LPCWSTR hostName, int hostPort, CString& errorMessage);
	bool ValidateCesmServerSSLPortArray(const std::vector<CString>& hosts, int hostPort, CString& errorMessage, CString& validHost);
	bool ValidateCesmServerSSLFromIniFile(LPCWSTR iniFileName, int hostPort, CString& errorMessage, CString& validHost);
	CString GetServerHostNameFromIniFile(LPCWSTR iniFileName);
}
