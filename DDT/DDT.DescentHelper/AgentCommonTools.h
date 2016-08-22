#pragma once
#include <string>
#include <vector>
#include <fstream>
#include <locale>
#include <codecvt>
#include <atlstr.h>
#include <atlpath.h>

#ifndef _WIN32_WINNT_WIN8
#define _WIN32_WINNT_WIN8 0x0602
#endif

#ifndef _WIN32_WINNT_WINBLUE
#define _WIN32_WINNT_WINBLUE 0x0603
#endif

namespace AgentCommonTools
{
	enum SystemKind
	{
		EnSkUnknown               = 0,

		EnSkWindows               = 0x01000000,
		EnSkWindowsXP             = 0x01000501,
		EnSkWindowsXPMediaCenter  = 0x01030501,
		EnSkWindowsXPTabletPC     = 0x01040501,
		EnSkWindowsServer2003     = 0x01800502,
		EnSkWindowsServer2003R2   = 0x01810502,
		EnSkWindowsHomeServer2003 = 0x01820502,
		EnSkWindowsSBS2003        = 0x01850502,
		EnSkWindowsSBS2003R2      = 0x01860502,
		EnSkWindowsVista          = 0x01000600,
		EnSkWindowsServer2008     = 0x01800600,
		EnSkWindowsSBS2008        = 0x01850600,
		EnSkWindows7              = 0x01000601,
		EnSkWindowsServer2008R2   = 0x01800601,
		EnSkWindowsHomeServer2011 = 0x01820601,
		EnSkWindowsSBS2011        = 0x01850601,
		EnSkWindows8              = 0x01000602,
		EnSkWindowsServer2012     = 0x01800602,
		EnSkWindows81             = 0x01000603,
		EnSkWindowsServer2012R2   = 0x01810603,
		EnSkWindows10             = 0x01000A00,
		EnSkWindowsServer2016     = 0x01800A00,

		EnSkLinux                 = 0x02000000,
		EnSkLinuxDebian           = 0x02000100,
		EnSkLinuxUbuntu           = 0x02000101,
		EnSkLinuxRedhat           = 0x02000200,
		EnSkLinuxFedora           = 0x02000201,

		EnSkMac                   = 0x04000000,
		EnSkMacOSX                = 0x04000100,

		EnSkAndroid               = 0x08000000,

		// Bits 23-21 contain bit field
		EnSkIsIntelX64Edition     = 0x00400000,
		EnSkIsWindowsEmbedded     = 0x00200000,
		EnSkIsWindowsServer       = 0x00800000,

		// Bits 20-16 contain integer in a range 0-31
		EnSkIsWindowsServerR2     = 0x00010000,
		EnSkIsWindowsHomeServer   = 0x00020000,
		EnSkIsWindowsMediaCenter  = 0x00030000,
		EnSkIsWindowsTabletPC     = 0x00040000,
		EnSkIsWindowsSBS          = 0x00050000,
		EnSkIsWindowsSBSR2        = 0x00060000,

		EnSkFamilyMask            = 0xFF000000,
		EnSkEditionMask			  = 0x00FF0000,
		EnSkSubKindMask           = 0x0000FF00,
		EnSkFlavourMask			  = 0x000000FF,
	};

	CString GetIsoUtcTimeStr(DATE x);
	DATE ParseIsoUtcTimeStr(LPCWSTR s);
	bool IsWindowsVersionOrGreater(WORD wMajorVersion, WORD wMinorVersion, WORD wServicePackMajor);
	bool IsWindows8Point1OrGreater();
	bool IsSystem32bit();
	CString GetCurrentSystemProcessorArchitecture();
	SystemKind GetSystemKind();
	CString& NormalizeGuidString(CString& guid, bool withBraces);
	CString GuidToString(REFGUID guid);
	CString GuidToStringWithBraces(REFGUID guid);
	bool GuidFromString(LPCWSTR guidString, GUID& guid);
	bool AreGuidsEqual(LPCWSTR guid1, LPCWSTR guid2);
	bool IsValidGuid(LPCWSTR guidString);
	CString TryToExtractGuidFromString(LPCWSTR guidString);
	CString GetModuleFolder(HMODULE hModule);
	CString GetCurrentModuleFolder();
	CString GetFilePathWithoutExtension(LPCWSTR filePath);
	CString GetFilePathInTheModuleFolder(HMODULE hModule, LPCWSTR fileName);
	CString GetFilePathInTheCurrentModuleFolder(LPCWSTR fileName);
	void SplitString(LPCWSTR value, LPCWSTR separators, std::vector<CString>& result, bool allowEmptyStrings = true);
	void EnumerateFiles(LPCWSTR whereToFind, std::vector<WIN32_FIND_DATA>& result);
	bool ExtractHostPortFromUri(LPCWSTR uri, CString& host, short& uPort);
	template<class TString>
	bool GetTempFilePath(TString& path, LPCWSTR fmt = NULL);
	CString QueryDefaultSystemLocale();
	bool ReadAllText(LPCWSTR path, std::string& text);
	bool ReadAllTextUtf8(LPCWSTR path, CString& text);
	bool ReadAllTextUtf16(LPCWSTR path, CString& text);
	bool WriteAllText(LPCWSTR path, const std::string& text);
	bool WriteAllTextUtf8(LPCWSTR path, const CString& text);
	bool WriteAllTextUtf16(LPCWSTR path, const CString& text);
	unsigned GetCrc32(void* buffer, size_t len);
	unsigned GetCrc32(CString text);
};

inline CString& AgentCommonTools::NormalizeGuidString(CString& guid, bool withBraces)
{
	if (guid.IsEmpty())
		return guid;
	if (withBraces)
	{
		if (guid[0] != L'{')
			guid.Insert(0, L'{');
		if (guid[guid.GetLength() - 1] != L'}')
			guid += L'}';
	}
	else
	{
		guid.TrimLeft(L'{').TrimRight(L'}');
	}
	return guid;
}

inline CString AgentCommonTools::GuidToString(REFGUID guid)
{
	CString result;

	RPC_WSTR buf = NULL;
	if (::UuidToString(&guid, (RPC_WSTR*)&buf) == RPC_S_OK)
	{
		result = (LPCWSTR) buf;
		::RpcStringFree(&buf);
	}

	return result;
}

inline CString AgentCommonTools::GuidToStringWithBraces(REFGUID guid)
{
	WCHAR s[39];
	::StringFromGUID2(guid, s, _countof(s));
	return s;
}

inline bool AgentCommonTools::GuidFromString(LPCWSTR guidString, GUID& guid)
{
	CString s = guidString;
	s.TrimLeft(L'{').TrimRight(L'}');
	return ::UuidFromString((RPC_WSTR) LPCWSTR(s), &guid) == RPC_S_OK;
}

inline bool AgentCommonTools::AreGuidsEqual(LPCWSTR guid1, LPCWSTR guid2)
{
	GUID g1, g2;
	bool result =
		GuidFromString(guid1, g1) &&
		GuidFromString(guid2, g2) &&
		g1 == g2;
	return result;
}

inline bool AgentCommonTools::IsValidGuid(LPCWSTR guidString)
{
	GUID guid;
	CString s = guidString;
	s.TrimLeft(L'{').TrimRight(L'}');
	bool result = (RPC_S_OK == ::UuidFromString((RPC_WSTR) LPCWSTR(s), &guid));
	return result;
}

inline CString AgentCommonTools::GetModuleFolder(HMODULE hModule)
{
	wchar_t buf[MAX_PATH];
	::GetModuleFileName(hModule, buf, _countof(buf));
	CPath path(buf);
	path.RemoveFileSpec();
	return LPCWSTR(path);
}

inline CString AgentCommonTools::GetCurrentModuleFolder()
{
	return GetModuleFolder(NULL);
}

inline CString AgentCommonTools::GetFilePathWithoutExtension(LPCWSTR filePath)
{
	CPath path(filePath);
	path.RemoveExtension();
	return LPCWSTR(path);
}

inline CString AgentCommonTools::GetFilePathInTheCurrentModuleFolder(LPCWSTR fileName)
{
	return GetFilePathInTheModuleFolder(NULL, fileName);
}

template<class TString>
inline bool AgentCommonTools::GetTempFilePath(TString& path, LPCWSTR fmt)
{
	GUID uuid;
	RPC_WSTR szUuid;
	WCHAR tempDir[MAX_PATH + 1];
	if (::GetTempPath(_countof(tempDir), tempDir) != 0)
	if (::UuidCreate(&uuid) == RPC_S_OK)
	if (::UuidToString(&uuid, (RPC_WSTR*) &szUuid) == RPC_S_OK)
	{
		WCHAR szName[MAX_PATH];
		WCHAR szPath[MAX_PATH];
		_snwprintf_s(szName, MAX_PATH + 1, fmt ? fmt : L"%s", szUuid);
		::RpcStringFree(&szUuid);
		if (_wmakepath_s(szPath, NULL, tempDir, szName, NULL) == 0)
		{
			path = szPath;
			return true;
		}
	}
	return false;
}
