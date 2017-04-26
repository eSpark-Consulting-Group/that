#pragma once

#include <vector>
#include <string>
#include <functional>

#include "AgentCommon.h"
#include "AgentCommonTools.h"

namespace AgentCommonTools
{
	bool LoadAgentAttributeFromTheRegistry(LPCWSTR name, void* data, ULONG& dataSize);
	bool LoadAgentAttributeFromTheRegistry(LPCWSTR name, std::vector<BYTE>& data);
	bool LoadAgentAttributeFromTheRegistry(LPCWSTR name, CString& result);
	bool LoadAgentAttributeFromTheRegistry(LPCWSTR name, DWORD& result);

	bool LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, DWORD& result);
	bool LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG& result);
	bool LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, CString& result);
	bool EnumAgentAttributesFromTheRegistryKey(LPCWSTR keyName, std::tr1::function<bool(DWORD type, PWCHAR name, LPBYTE value)> handler);

	bool LoadCisOptionAttributeFromTheRegistry(LPCWSTR name, DWORD& result);

	bool SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, const void* data, ULONG dataSize, bool is64);
	bool SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, LPCWSTR value, bool is64);
	bool SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, DWORD value, bool is64);

	bool SaveAgentAttributeToTheRegistry(LPCWSTR name, const void* data, ULONG dataSize);
	bool SaveAgentAttributeToTheRegistry(LPCWSTR name, const std::vector<BYTE>& data);
	bool SaveAgentAttributeToTheRegistry(LPCWSTR name, LPCWSTR value);
	bool SaveAgentAttributeToTheRegistry(LPCWSTR name, DWORD value);

	bool SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, DWORD value);
	bool SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG value);
	bool SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, LPCWSTR value);

	bool SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, DWORD value, bool is64);
	bool SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG value, bool is64);
	bool SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, LPCWSTR value, bool is64);

	bool CreateAgentRegistryKeyInternal(CRegKey& regKey, LPCWSTR keyPath, bool is64);

	bool SaveCisOptionAttributeToTheRegistry(LPCWSTR name, DWORD value);
	bool SaveCisAttributeToTheRegistryInternal(LPCWSTR name, DWORD value, bool is64);

	bool DeleteAgentAttributeFromTheRegistry(LPCWSTR name);
	bool DeleteAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName);
	bool DeleteAgentAttribute(LPCWSTR name);

	bool SaveToRegistry(LPCWSTR Key, LPCWSTR Value);

	bool DeleteKeyFromRegistry(LPCWSTR Key);

	// Load/save agent per-user attribute
	bool LoadAgentAttributeFromTheUserRegistry(LPCWSTR name, DWORD& result);
	bool LoadAgentAttributeFromTheUserRegistry(LPCWSTR name, GUID& result);
	bool SaveAgentAttributeToTheUserRegistry(LPCWSTR name, DWORD value);
	bool SaveAgentAttributeToTheUserRegistry(LPCWSTR name, REFGUID value);

	bool UpdateAgentIndentity(REFGUID newGuid);
	bool CreateAgentRegistryPath();
}
