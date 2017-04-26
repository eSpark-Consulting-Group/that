#include "stdafx.h"
#include "AgentSslSocketTools.h"

bool AgentCommonTools::ValidateCesmServerSSLPort(LPCWSTR hostName, int hostPort, CString& errorMessage)
{
	// ==============================================
	// Check HostName is valid
	// ==============================================
	CRM::AgnCore::Transport::CIPEndPoint endPoint;
	if (!endPoint.Init(hostName, hostPort))
	{
		errorMessage.Format(L"Cannot resolve host name '%s'", hostName);
		return false;
	};

	// ==============================================
	// Try to connect via SSL socket
	// ==============================================
	CSslSocket ssl;
	bool isConnected = ssl.SecureConnect(endPoint);
	if (!isConnected)
	{
		errorMessage.Format(L"Can not establish SSL connection to the server %s", LPCTSTR(hostName));
		return false;
	}

	return true;
}

bool AgentCommonTools::ValidateCesmServerSSLPortArray(const std::vector<CString>& hosts, int hostPort, CString& errorMessage, CString& validHost)
{
	for (int i = 0; i < (int) hosts.size(); ++i)
	{
		if (ValidateCesmServerSSLPort(hosts[i], hostPort, errorMessage))
		{
			validHost = hosts[i];
			return true;
		}
	}
	validHost = L"";
	return false;
}

CString AgentCommonTools::GetServerHostNameFromIniFile(LPCWSTR iniFileName)
{
	std::vector<CString> hosts;
	CString iniFilePath = AgentCommonTools::GetFilePathInTheCurrentModuleFolder(iniFileName);
	AgentCommonTools::ExtractAlternativeHostsFromIniFile(iniFilePath, hosts);	
	return !hosts.empty() ? hosts[0] : L"";
}

bool AgentCommonTools::ValidateCesmServerSSLFromIniFile(LPCWSTR iniFileName, int hostPort, CString& errorMessage, CString& validHost)
{
	std::vector<CString> hosts;
	CString iniFilePath = AgentCommonTools::GetFilePathInTheCurrentModuleFolder(iniFileName);
	AgentCommonTools::ExtractAlternativeHostsFromIniFile(iniFilePath, hosts);
	CString serverHost = GetServerHostNameFromIniFile(iniFilePath);
	hosts.push_back(serverHost);

	bool result = ValidateCesmServerSSLPortArray(hosts, hostPort, errorMessage, validHost);
	return result;
}

