#include "stdafx.h"
#include "AgentCommonTools.h"
#include <atlutil.h>
#include <ATLComTime.h>
#include <LM.h>

CString AgentCommonTools::GetIsoUtcTimeStr(DATE x)
{
	CString result;
	SYSTEMTIME st;
	// Compute seconds since midnight rounded to msec
	double days = (UINT) x;
	double fSecs = (UINT) ((x - days) * 86400000.0 + 0.5) / 1000.0;
	if (fSecs >= 86400.0)
	{
		fSecs = 0;
		days++;
	}
	// Let COleDateTime do date conversion, but convert time manually
	if (COleDateTime(days).GetAsSystemTime(st))
	{		
		UINT nSecs = (UINT) fSecs;			// Integer seconds since midnight
		st.wHour = nSecs / 3600;			// Complete hours
		st.wMinute = nSecs % 3600 / 60;		// Complete minutes
		st.wSecond = nSecs % 60;			// Complete seconds
		st.wMilliseconds = (UINT) ((fSecs - nSecs) * 1000.0 + 0.5);
		result.Format(L"%04hu-%02hu-%02huT%02hu:%02hu:%02hu.%03huZ", st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
	}

	return result;
}

DATE AgentCommonTools::ParseIsoUtcTimeStr(LPCWSTR s)
{
	SYSTEMTIME st = { };
	int count = swscanf_s(s, L"%hu-%hu-%huT%hu:%hu:%hu.%huZ", &st.wYear, &st.wMonth, &st.wDay, &st.wHour, &st.wMinute, &st.wSecond, &st.wMilliseconds);
	if (count < 6)
		return 0.0;
	double timeOfDay = (st.wHour * 3600.0 * 1000.0 + st.wMinute * 60.0 * 1000.0 + st.wSecond * 1000.0 + st.wMilliseconds) / 86400000.0;
	st.wHour = st.wMinute = st.wSecond = st.wMilliseconds = 0;
	double days = (UINT) COleDateTime(st).m_dt;
	return days + timeOfDay;
}

// Exerpt from Win 8.1 SDK
bool AgentCommonTools::IsWindowsVersionOrGreater(WORD wMajorVersion, WORD wMinorVersion, WORD wServicePackMajor)
{
	OSVERSIONINFOEX  osvi = { sizeof(osvi), 0, 0, 0, 0, {0}, 0, 0 };
	DWORDLONG        const dwlConditionMask = VerSetConditionMask(
		VerSetConditionMask(
		VerSetConditionMask(
			0, VER_MAJORVERSION, VER_GREATER_EQUAL),
				VER_MINORVERSION, VER_GREATER_EQUAL),
				VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);

	osvi.dwMajorVersion = wMajorVersion;
	osvi.dwMinorVersion = wMinorVersion;
	osvi.wServicePackMajor = wServicePackMajor;

	bool result = ::VerifyVersionInfo(&osvi, VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR, dwlConditionMask) != FALSE;
	return result;
}

bool AgentCommonTools::IsSystem32bit()
{
	CString platform = GetCurrentSystemProcessorArchitecture();

	if (platform == L"x86")
		return true;

	if (platform == L"AMD64")
		return false;

	DO_LOG(L"Unknown system architecture: %s", platform);
	ATLASSERT(false);

	// Assume it is not 32 bit !
	return false;
}

// Get current system architecture
// Valid results: x86, AMD64
CString AgentCommonTools::GetCurrentSystemProcessorArchitecture()
{
	SYSTEM_INFO si = { };
	GetNativeSystemInfo(&si);
		
	switch (si.wProcessorArchitecture)
	{
		case PROCESSOR_ARCHITECTURE_INTEL:
			return L"x86";
		case PROCESSOR_ARCHITECTURE_AMD64:
			return L"AMD64";
		case PROCESSOR_ARCHITECTURE_IA64:
			return L"IA64";
		default:
			return L"";
	}
}

bool AgentCommonTools::IsWindows8Point1OrGreater()
{
	bool result = IsWindowsVersionOrGreater(HIBYTE(_WIN32_WINNT_WINBLUE), LOBYTE(_WIN32_WINNT_WINBLUE), 0);
	return result;
}

AgentCommonTools::SystemKind AgentCommonTools::GetSystemKind()
{
	uint32_t result = SystemKind::EnSkUnknown;

#ifdef _WINDOWS_
	OSVERSIONINFOEX osver = { };
	osver.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	if (::GetVersionEx((LPOSVERSIONINFO) &osver))
	{
		LPWKSTA_INFO_100 pWks = NULL;
		if (::NetWkstaGetInfo(NULL, 100, (LPBYTE*) &pWks) == NERR_Success)
		{
			// This always results in actual version on Windows 8.1 and Windows 10
			if (pWks->wki100_ver_major > osver.dwMajorVersion || 
				pWks->wki100_ver_major == osver.dwMajorVersion && pWks->wki100_ver_minor > osver.dwMinorVersion)
			{
				osver.dwMajorVersion = pWks->wki100_ver_major;
				osver.dwMinorVersion = pWks->wki100_ver_minor;
				osver.dwBuildNumber = 0;
			}
			::NetApiBufferFree(pWks);
		}
		uint32_t version = (osver.dwMajorVersion << 16) | (uint16_t) osver.dwMinorVersion;
		switch (version)
		{
			case 0x00050001:	// 5.1
				result = SystemKind::EnSkWindowsXP;
				if (GetSystemMetrics(SM_MEDIACENTER))
					result |= SystemKind::EnSkIsWindowsMediaCenter;
				else if (GetSystemMetrics(SM_TABLETPC))
					result |= SystemKind::EnSkIsWindowsTabletPC;
				break;
			case 0x00050002:	// 5.2
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindowsXP;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2003;
					if (osver.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED)
					{
						// Windows Small Business Server 2003
						if (GetSystemMetrics(SM_SERVERR2))
							result |= SystemKind::EnSkIsWindowsSBSR2;
						else
							result |= SystemKind::EnSkIsWindowsSBS;
					}
					else if (osver.wSuiteMask & VER_SUITE_WH_SERVER)
						// Windows Home Server 2003
						result |= SystemKind::EnSkIsWindowsHomeServer;
					else if (GetSystemMetrics(SM_SERVERR2))
						// Windows Server 2003 R2
						result |= SystemKind::EnSkIsWindowsServerR2;
				}
				break;
			case 0x00060000:	// 6.0
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindowsVista;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2008;
					if (osver.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED)
						// Windows Small Business Server 2008
						result |= SystemKind::EnSkIsWindowsSBS;
				}
				break;
			case 0x00060001:	// 6.1
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindows7;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2008R2;
					if (osver.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED)
						// Windows Small Business Server 2011
						result |= SystemKind::EnSkIsWindowsSBS;
					else if (osver.wSuiteMask & VER_SUITE_WH_SERVER)
						// Windows Home Server 2011
						result |= SystemKind::EnSkIsWindowsHomeServer;
				}
				break;
			case 0x00060002:	// 6.2
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindows8;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2012;
				}
				break;
			case 0x00060003:	// 6.3
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindows81;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2012R2;
				}
				break;
			case 0x000A0000:	// 10.0
				if (osver.wProductType == VER_NT_WORKSTATION)
				{
					result = SystemKind::EnSkWindows10;
				}
				else
				{
					result = SystemKind::EnSkWindowsServer2016;
				}
				break;
			default:
				result = SystemKind::EnSkWindows;
				break;
		}
	}
	else
		result = SystemKind::EnSkWindows;

	if (osver.wSuiteMask & VER_SUITE_EMBEDDEDNT)
		result |= SystemKind::EnSkIsWindowsEmbedded;

	SYSTEM_INFO si = { };
	::GetNativeSystemInfo(&si);
	if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
		result |= SystemKind::EnSkIsIntelX64Edition;
#endif

	return (SystemKind) result;
}

CString AgentCommonTools::TryToExtractGuidFromString(LPCWSTR guidString)
{
	// Check if valid GUID
	if (IsValidGuid(guidString))
		return guidString;

	CString s = guidString;
	int i1 = s.Find('{');
	int i2 = s.ReverseFind('}');
	if (i1 != -1 && i2 != -1 && i1 < i2)
	{
		s = s.Mid(i1, i2 - i1 + 1);
		if (s != guidString)
			return TryToExtractGuidFromString(s);
	}

	return L"";
}

CString AgentCommonTools::GetFilePathInTheModuleFolder(HMODULE hModule, LPCWSTR fileName)
{
	wchar_t buf[MAX_PATH];
	CString filePath = fileName;

	if (::GetFileAttributes(fileName) == 0xffffffff)
	{
		CPath path(GetModuleFolder(hModule));
		path.Append(fileName);
		filePath = LPCWSTR(path);
	}

	::GetFullPathName(filePath, MAX_PATH, buf, NULL);
	filePath = buf;
	return filePath;
}

void AgentCommonTools::SplitString(LPCWSTR value, LPCWSTR separators, std::vector<CString>& result, bool allowEmptyStrings /* = true */)
{
	CString value_(value);
	result.clear();
	if (value_.IsEmpty())
		return;

	for (int i = 0;;)
	{
		CString token = value_.Tokenize(separators, i);
		if (i == -1)
			break;
		if (!token.IsEmpty() || allowEmptyStrings)
			result.push_back(token);
	}

	if (result.empty())
		result.push_back(value_);
}

void AgentCommonTools::EnumerateFiles(LPCWSTR whereToFind, std::vector<WIN32_FIND_DATA>& result)
{
	result.clear();

	WIN32_FIND_DATA findFileData = { };
	HANDLE hFind = INVALID_HANDLE_VALUE;
	hFind = ::FindFirstFile(whereToFind, &findFileData);

	if (hFind != INVALID_HANDLE_VALUE) 
	{
		result.push_back(findFileData);
		while (::FindNextFile(hFind, &findFileData) != 0) 
		{
			result.push_back(findFileData);
		}
		::FindClose(hFind);
	}
}

bool AgentCommonTools::ExtractHostPortFromUri(LPCWSTR uri, CString& host, short& uPort)
{
	CUrl url;
	if (url.CrackUrl(uri))
	{
		host = url.GetHostName();
		uPort = url.GetPortNumber();
		return true;
	}
	else
		return false;
}

CString AgentCommonTools::QueryDefaultSystemLocale()
{
	// Here we use API methods that are available on all Windows platforms
	CString result;
	wchar_t buf[64] = { };

	::GetLocaleInfo(LOCALE_SYSTEM_DEFAULT, LOCALE_SISO639LANGNAME, buf, _countof(buf));
	result += buf;
	result += L"-";

	::GetLocaleInfo(LOCALE_SYSTEM_DEFAULT, LOCALE_SISO3166CTRYNAME, buf, _countof(buf));
	result += buf;
	return result;
}

bool AgentCommonTools::ReadAllText(LPCWSTR path, std::string& text)
{
	std::ifstream ifs(path);
	if (ifs.is_open())
	{
		ifs.seekg(0, std::ios::end);
		text.resize((size_t) ifs.tellg());
		ifs.seekg(0, std::ios::beg);
		ifs.read(&text[0], text.size());
		return true;
	}
	else
		return false;
}

bool AgentCommonTools::ReadAllTextUtf8(LPCWSTR path, CString& text)
{
	std::wifstream ifs(path);
	std::locale loc(std::locale(""), new std::codecvt_utf8<wchar_t, 0x10ffff, std::consume_header>());
	ifs.imbue(loc);
	if (ifs.is_open())
	{
		ifs.seekg(0, std::ios::end);
		text.Empty();
		text.Preallocate((int) ifs.tellg());
		ifs.seekg(0, std::ios::beg);
		std::wstring line;
		line.reserve(1024);
		while (std::getline(ifs, line))
		{
			text += line.c_str();
			text += L'\n';
		}
		return true;
	}
	else
		return false;
}

bool AgentCommonTools::ReadAllTextUtf16(LPCWSTR path, CString& text)
{
	std::wifstream ifs(path);
	std::locale loc(std::locale(""), new std::codecvt_utf16<wchar_t, 0x10ffff, std::codecvt_mode(std::consume_header | std::little_endian)>());
	ifs.imbue(loc);
	if (ifs.is_open())
	{
		ifs.seekg(0, std::ios::end);
		text.Empty();
		text.Preallocate((int) ifs.tellg());
		ifs.seekg(0, std::ios::beg);
		std::wstring line;
		line.reserve(1024);
		while (std::getline(ifs, line))
		{
			text += line.c_str();
			text += L'\n';
		}
		return true;
	}
	else
		return false;
}

bool AgentCommonTools::WriteAllText(LPCWSTR path, const std::string& text)
{
	std::ofstream ofs(path);
	if (ofs.is_open())
	{
		ofs.write(&text[0], text.size());
		return true;
	}
	else
		return false;
}

bool AgentCommonTools::WriteAllTextUtf8(LPCWSTR path, const CString& text)
{
	std::wofstream ofs(path, std::ios::binary);
	std::locale loc(std::locale(""), new std::codecvt_utf8<wchar_t, 0x10ffff, std::generate_header>());
	ofs.imbue(loc);
	if (ofs.is_open())
	{
		ofs.write(LPCWSTR(text), text.GetLength());
		return true;
	}
	else
		return false;
}

bool AgentCommonTools::WriteAllTextUtf16(LPCWSTR path, const CString& text)
{
	std::wofstream ofs(path, std::ios::binary);
	std::locale loc(std::locale(""), new std::codecvt_utf16<wchar_t, 0x10ffff, std::codecvt_mode(std::generate_header | std::little_endian)>());
	ofs.imbue(loc);
	if (ofs.is_open())
	{
		ofs.write(LPCWSTR(text), text.GetLength());
		return true;
	}
	else
		return false;
}

unsigned AgentCommonTools::GetCrc32(void* buffer, size_t len)
{
	// CRC parameters
    const unsigned InitialRemainder = 0xFFFFFFFF;
    const unsigned FinalXorValue = 0xFFFFFFFF;

    static unsigned CrcTable[] =
    {
          0x00000000, 0x77073096, 0xee0e612c, 0x990951ba
        , 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3
        , 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988
        , 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91
        , 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de
        , 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7
        , 0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec
        , 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5
        , 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172
        , 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b
        , 0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940
        , 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59
        , 0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116
        , 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f
        , 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924
        , 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d
        , 0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a
        , 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433
        , 0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818
        , 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01
        , 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e
        , 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457
        , 0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c
        , 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65
        , 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2
        , 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb
        , 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0
        , 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9
        , 0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086
        , 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f
        , 0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4
        , 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad
        , 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a
        , 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683
        , 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8
        , 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1
        , 0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe
        , 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7
        , 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc
        , 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5
        , 0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252
        , 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b
        , 0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60
        , 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79
        , 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236
        , 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f
        , 0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04
        , 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d
        , 0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a
        , 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713
        , 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38
        , 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21
        , 0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e
        , 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777
        , 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c
        , 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45
        , 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2
        , 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db
        , 0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0
        , 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9
        , 0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6
        , 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf
        , 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94
        , 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
    };

    unsigned sum = InitialRemainder;

    for (size_t i = 0; i < len; i++)
    {
        sum = (sum >> 8) ^ CrcTable[(sum & 0xff) ^ ((char*) buffer)[i]];
    }

    return sum ^ FinalXorValue;
}

unsigned AgentCommonTools::GetCrc32(CString text)
{
	unsigned value = 0;
	int bufLen = ::WideCharToMultiByte(CP_UTF8, 0, text, text.GetLength(), NULL, 0, NULL, NULL);
	if (bufLen != 0)
	{
		CAutoPtr<char> buf(new char[bufLen]);
		::WideCharToMultiByte(CP_UTF8, 0, text, text.GetLength(), buf.m_p, bufLen, NULL, NULL);
		value = GetCrc32(buf, bufLen);
	}
	return value;
}
