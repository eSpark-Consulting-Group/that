#include "stdafx.h"
#include "AgentRegistry.h"
#include "BaseExceptionsLT.h"

#ifndef LOG_DEBUG
#define LOG_DEBUG(...) DO_LOG(__VA_ARGS__)
#endif

#ifndef LOG_VERBOSE
#define LOG_VERBOSE(...) DO_LOG(__VA_ARGS__)
#endif

// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistry(LPCWSTR name, void* data, ULONG& dataSize)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.QueryBinaryValue(name, data, &dataSize)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistry(LPCWSTR name, std::vector<BYTE>& data)
{
	CRegKey RegKey;
	ULONG dataSize = 0;
	LoadAgentAttributeFromTheRegistry(name, NULL, dataSize);

	data.resize(dataSize);
	if(dataSize != 0)
	{
		return LoadAgentAttributeFromTheRegistry(name, &data[0], dataSize);
	}
	return false;
}


// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistry(LPCWSTR name, CString& result)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	ULONG dataSize = 0;
	if (0 != (rc = RegKey.QueryStringValue(name, NULL, &dataSize)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}
	std::vector<wchar_t> buf(dataSize + 1, 0);
	if (0 != (rc = RegKey.QueryStringValue(name, &buf[0], &dataSize)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	result = &buf[0];
	buf.clear();
	return true;
}


// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistry(LPCWSTR name, DWORD& result)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.QueryDWORDValue(name, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}
	return true;
}

// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, DWORD& result)
{
	LONG rc = 0;
	CRegKey regKey;
	CString keyPath = CString(s_AgentRegistryKey) + L'\\' + keyName;
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.QueryDWORDValue(valueName, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Load agent attribute
bool AgentCommonTools::LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG& result)
{
	LONG rc = 0;
	CRegKey regKey;
	CString keyPath = CString(s_AgentRegistryKey) + L'\\' + keyName;
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.QueryQWORDValue(valueName, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::LoadAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, CString& result)
{
	LONG rc = 0;
	CRegKey RegKey;
	CString keyPath = CString(s_AgentRegistryKey) + L'\\' + keyName;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	ULONG dataSize = 0;
	if (0 != (rc = RegKey.QueryStringValue(valueName, NULL, &dataSize)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	std::vector<wchar_t> buf(dataSize + 1, 0);
	if (0 != (rc = RegKey.QueryStringValue(valueName, &buf[0], &dataSize)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	result = &buf[0];
	buf.clear();
	return true;
}

bool AgentCommonTools::EnumAgentAttributesFromTheRegistryKey(LPCWSTR keyName, std::tr1::function<bool(DWORD type, PWCHAR name, LPBYTE value)> handler)
{
	LONG rc = 0;
	CRegKey regKey;
	CString keyPath = CString(s_AgentRegistryKey) + L'\\' + keyName;
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	WCHAR nameBuf[1024];
	BYTE valueBuf[1024];
	DWORD i = 0;
	do
	{
		DWORD type = 0;
		DWORD ccName = _countof(nameBuf) - 2;
		DWORD szValue = _countof(valueBuf) - 4;
		memset(nameBuf, 0, sizeof(nameBuf));
		memset(valueBuf, 0, sizeof(valueBuf));
		rc = RegEnumValue(regKey, i, nameBuf, &ccName, NULL, &type, valueBuf, &szValue);
		if (rc == ERROR_SUCCESS)
		{
			bool proceed = handler(type, nameBuf, valueBuf);
			if (!proceed)
				break;
		}
		if (rc == ERROR_NO_MORE_ITEMS)
			break;
		i++;
	}
	while (1);

	return true;
}

// Load agent attribute
bool AgentCommonTools::LoadCisOptionAttributeFromTheRegistry(LPCWSTR name, DWORD& result)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_Cis_OptionRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_Cis_OptionRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", s_Cis_OptionRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.QueryDWORDValue(name, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}
	return true;
}


bool AgentCommonTools::SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, const void* data, ULONG dataSize, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, s_AgentRegistryKey, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetBinaryValue(name, data, dataSize)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, LPCWSTR value, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, s_AgentRegistryKey, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetStringValue(name, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryInternal(LPCWSTR name, DWORD value, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, s_AgentRegistryKey, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetDWORDValue(name, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Save CIS option attribute
bool AgentCommonTools::SaveCisOptionAttributeToTheRegistry(LPCWSTR name, DWORD value)
{
	if (!SaveCisAttributeToTheRegistryInternal(name, value, true))
		return SaveCisAttributeToTheRegistryInternal(name, value, false);
	return true;
}

bool AgentCommonTools::SaveCisAttributeToTheRegistryInternal(LPCWSTR name, DWORD value, bool is64)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_Cis_OptionRegistryKey, KEY_WRITE | (is64 ? KEY_WOW64_64KEY : 0))))
	if (0 != (rc = RegKey.Create(HKEY_LOCAL_MACHINE, s_Cis_OptionRegistryKey, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE | (is64 ? KEY_WOW64_64KEY : 0))))
	{
		LOG_DEBUG(L"Cannot create registry key 'HKLM\\%s': %s (%ld)", s_Cis_OptionRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.SetDWORDValue(name, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Save agent attribute
bool AgentCommonTools::SaveAgentAttributeToTheRegistry(LPCWSTR name, const void* data, ULONG dataSize)
{
	if( data == NULL )
		return true;
	if(!SaveAgentAttributeToTheRegistryInternal(name, data, dataSize, true))
		return SaveAgentAttributeToTheRegistryInternal(name, data, dataSize, false);
	return true;
}

// Save agent attribute
bool AgentCommonTools::SaveAgentAttributeToTheRegistry(LPCWSTR name, const std::vector<BYTE>& data)
{
	if(!SaveAgentAttributeToTheRegistryInternal(name, &data[0], (ULONG)data.size(), true))
		return SaveAgentAttributeToTheRegistryInternal(name, &data[0], (ULONG)data.size(), false);
	return true;
}

// Save agent attribute
bool AgentCommonTools::SaveAgentAttributeToTheRegistry(LPCWSTR name, LPCWSTR value)
{
	if (!SaveAgentAttributeToTheRegistryInternal(name, value, true))
		return SaveAgentAttributeToTheRegistryInternal(name, value, false);
	return true;
}

// Save agent attribute
bool AgentCommonTools::SaveAgentAttributeToTheRegistry(LPCWSTR name, DWORD value)
{
	if(!SaveAgentAttributeToTheRegistryInternal(name, value, true))
		return SaveAgentAttributeToTheRegistryInternal(name, value, false);
	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, DWORD value)
{
	if (!SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, true))
		return SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, false);
	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG value)
{
	if (!SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, true))
		return SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, false);
	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName, LPCWSTR value)
{
	if (!SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, true))
		return SaveAgentAttributeToTheRegistryKeyInternal(keyName, valueName, value, false);
	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, DWORD value, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, CString(s_AgentRegistryKey) + L'\\' + keyName, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetDWORDValue(valueName, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, ULONGLONG value, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, CString(s_AgentRegistryKey) + L'\\' + keyName, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetQWORDValue(valueName, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheRegistryKeyInternal(LPCWSTR keyName, LPCWSTR valueName, LPCWSTR value, bool is64)
{
	CRegKey regKey;
	if (!CreateAgentRegistryKeyInternal(regKey, CString(s_AgentRegistryKey) + L'\\' + keyName, is64))
		return false;

	LONG rc = 0;
	if (0 != (rc = regKey.SetStringValue(valueName, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::CreateAgentRegistryKeyInternal(CRegKey& regKey, LPCWSTR keyPath, bool is64)
{
	LONG rc = 0;
	if (0 != (rc = regKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_WRITE | (is64 ? KEY_WOW64_64KEY : 0))))
	if (0 != (rc = regKey.Create(HKEY_LOCAL_MACHINE, keyPath, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE | (is64 ? KEY_WOW64_64KEY : 0))))
	{
		LOG_DEBUG(L"Cannot create registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}
	return true;
}

// Delete agent attribute
bool AgentCommonTools::DeleteAgentAttributeFromTheRegistry(LPCWSTR name)
{
	LONG rc = 0;
	CRegKey RegKey;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, s_AgentRegistryKey, KEY_WRITE)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.DeleteValue(name)))
	{
		LOG_VERBOSE(L"Cannot delete registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Delete attribute from the agent subkey
bool AgentCommonTools::DeleteAgentAttributeFromTheRegistryKey(LPCWSTR keyName, LPCWSTR valueName)
{
	LONG rc = 0;
	CRegKey RegKey;
	CString keyPath = CString(s_AgentRegistryKey) + L'\\' + keyName;
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = RegKey.Open(HKEY_LOCAL_MACHINE, keyPath, KEY_WRITE)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKLM\\%s': %s (%ld)", keyPath, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = RegKey.DeleteValue(valueName)))
	{
		LOG_VERBOSE(L"Cannot delete registry value '%s': %s (%ld)", valueName, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

// Delete agent attribute
bool AgentCommonTools::DeleteAgentAttribute(LPCWSTR name)
{
	return DeleteAgentAttributeFromTheRegistry( name );
}

bool AgentCommonTools::SaveToRegistry(LPCWSTR key, LPCWSTR value)
{
	return SaveAgentAttributeToTheRegistry(key, value);
}

bool AgentCommonTools::DeleteKeyFromRegistry(LPCWSTR Key)
{
	return DeleteAgentAttributeFromTheRegistry(Key);
}

bool AgentCommonTools::LoadAgentAttributeFromTheUserRegistry(LPCWSTR name, DWORD& result)
{
	LONG rc = 0;
	CRegKey regKey;
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKCU\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.QueryDWORDValue(name, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::LoadAgentAttributeFromTheUserRegistry(LPCWSTR name, GUID& result)
{
	LONG rc = 0;
	CRegKey regKey;
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_READ | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_READ)))
	{
		LOG_VERBOSE(L"Cannot open registry key 'HKCU\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.QueryGUIDValue(name, result)))
	{
		LOG_VERBOSE(L"Cannot query registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheUserRegistry(LPCWSTR name, DWORD value)
{
	CRegKey regKey;
	LONG rc = 0;
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_WRITE)))
	if (0 != (rc = regKey.Create(HKEY_CURRENT_USER, s_AgentRegistryKey, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Create(HKEY_CURRENT_USER, s_AgentRegistryKey, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE)))
	{
		LOG_DEBUG(L"Cannot create registry key 'HKCU\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.SetDWORDValue(name, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::SaveAgentAttributeToTheUserRegistry(LPCWSTR name, REFGUID value)
{
	CRegKey regKey;
	LONG rc = 0;
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Open(HKEY_CURRENT_USER, s_AgentRegistryKey, KEY_WRITE)))
	if (0 != (rc = regKey.Create(HKEY_CURRENT_USER, s_AgentRegistryKey, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE | KEY_WOW64_64KEY)))
	if (0 != (rc = regKey.Create(HKEY_CURRENT_USER, s_AgentRegistryKey, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_WRITE)))
	{
		LOG_DEBUG(L"Cannot create registry key 'HKCU\\%s': %s (%ld)", s_AgentRegistryKey, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	if (0 != (rc = regKey.SetGUIDValue(name, value)))
	{
		LOG_DEBUG(L"Cannot set registry value '%s': %s (%ld)", name, AgentCommonTools::CWin32LastError(rc).GetMessage(), rc);
		return false;
	}

	return true;
}

bool AgentCommonTools::UpdateAgentIndentity(REFGUID newGuid)
{
	CString guidString = AgentCommonTools::GuidToString(newGuid);
	if (!SaveToRegistry(L"identity", guidString))
	{
		return false;
	}

	return true;
}

bool AgentCommonTools::CreateAgentRegistryPath()
{
	std::vector<CString> result;
	SplitString(s_AgentRegistryKey, L"\\", result, false);
	if (result.empty())
		return false;

	CString path;
	for (size_t i = 0; i < result.size(); ++i)
	{
		CRegKey regKey;
		path += result[i];
		if (0 != regKey.Create(HKEY_LOCAL_MACHINE, path, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_READ | KEY_WOW64_64KEY))
 		if (0 != regKey.Create(HKEY_LOCAL_MACHINE, path, REG_NONE, REG_OPTION_NON_VOLATILE, KEY_READ))
			return false;

		path += L"\\";
	}
		
	return true;
}
