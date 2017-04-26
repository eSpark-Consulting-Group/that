#pragma once

#define WIN32_FROM_HRESULT(x) (x & 0xffff)

#define NAN std::numeric_limits<double>::quiet_NaN()
#define NANF std::numeric_limits<float>::quiet_NaN()
#define MIN_DATE 0.0
#define MAX_DATE COleDateTime(9999, 12, 31, 23, 59, 59)
// COleDateTime tick which is 1 msec interval
#define DATE_TICK (1.0 / (24.0 * 60 * 60 * 1000))
#define WSTR_NULL ((wchar_t*) 0)

#define MAX_DISCOVERY_XML_SIZE (512UL * 1024UL) // 512KB
#define MAX_DATA_SIZE (4000UL * 1024UL) // 4MB

static LPCWSTR s_PolicyFileName = L"policy";
static LPCWSTR s_DefaultPolicyFileName = L"defaultpolicy";
static LPCWSTR s_DeferredActionsFileName = L"deferred.bin";
static LPCWSTR s_PowerBackupFileName = L"power_backup";
static LPCWSTR s_DescentFileName = L"CesmHelper.exe";
static LPCWSTR s_AgentMsiGUID = L"{ED7608DB-B426-4A61-9E1F-120A9810B291}";
static LPCWSTR s_CisUpgradeCode = L"{B9D3F17C-97D2-4285-9964-BFD6ADFFE38F}";
static LPCWSTR s_ZeroIdentity = L"00000000-0000-0000-0000-000000000000";
static LPCWSTR s_AgentRegistryKey = L"Software\\COMODO\\CESM\\CESM Agent";
static LPCWSTR s_AgentOldRegistryKey = L"Software\\COMODO\\CESM\\CESM Service";
static LPCWSTR s_AgentPublicKeyParamName = L"PublicKey";
static LPCWSTR s_AgentAltServerHosts = L"AltServerHosts";
static LPCWSTR s_AgentIdentity = L"identity";
static LPCWSTR s_AgentServerHost = L"serverhost";
static LPCWSTR s_AgentServerPort = L"serverport";
static LPCWSTR s_AgentVersion = L"version";
static LPCWSTR s_AgentUILanguage = L"UILanguage";
static LPCWSTR s_AgentAssistanceSessionUid = L"AssistanceSessionUid";
static LPCWSTR s_RebootPending = L"RebootPending";

static LPCWSTR s_Cis_OptionRegistryKey = L"System\\Software\\COMODO\\Firewall Pro\\Options";
static LPCWSTR s_Cis_MsiProductKey = L"System\\Software\\COMODO\\Firewall Pro\\";
static LPCWSTR s_Cis_MsiProductCode = L"MsiProductCode";
