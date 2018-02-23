#include "Extractor.h"
#include <TlHelp32.h>
#include <fstream>
#include <Psapi.h>
#include <regex>
#include <codecvt>
#include <shtypes.h>
#include <Shlobj.h>


Extractor::Extractor()
{
}

void Extractor::ExtractId()
{
	auto pid = GetCurrentProcessId();

	auto ppid = getParentPID(pid);
	DWORD size = MAX_PATH;
	WCHAR processName[MAX_PATH];// = L"c:\\UnknownFileHunter_MQ==.exe";
	auto m = std::wcmatch{};
	getProcessName(ppid, processName, size);
	wchar_t buf[MAX_PATH];
	::GetModuleFileName(NULL, buf, _countof(buf));
	std::wstring fileName(buf);
	std::wstring directory;
	std::wstring extractedDirectory;
	size_t last_slash_idx = fileName.rfind('\\');
	if (std::string::npos != last_slash_idx)
	{
		directory = fileName.substr(0, last_slash_idx);
	}

	last_slash_idx = directory.rfind('\\');
	if (std::string::npos != last_slash_idx)
	{
		extractedDirectory = directory.substr(0, last_slash_idx);
	}
	auto regex = std::wregex{ L"UnknownFileHunter_[a-zA-Z0-9]+[=]{0,2}" };
	if (std::regex_search(processName, m, regex))
	{
		WriteAllTextUtf8((directory + L"\\originalfile").c_str(), m[0].str());
		last_slash_idx = directory.rfind('\\');
		if (std::string::npos != last_slash_idx)
		{
			directory = directory.substr(0, last_slash_idx);
		}
	}
	ShellExecute(NULL, NULL, extractedDirectory.c_str(), NULL, NULL, SW_SHOWNORMAL);
}

bool Extractor::WriteAllTextUtf8(LPCWSTR path, const std::wstring& text)
{
	std::wofstream ofs (path, std::ios::binary);
	std::locale loc(std::locale(""), new std::codecvt_utf8<wchar_t, 0x10ffff, std::generate_header>());
	ofs.imbue(loc);
	if (ofs.is_open())
	{
		ofs.write(text.c_str(), text.length());
		return true;
	}
	else
		return false;
}


DWORD Extractor::getParentPID(DWORD pid)
{
	HANDLE h = NULL;
	PROCESSENTRY32W pe = { 0 };
	DWORD ppid = 0;
	pe.dwSize = sizeof(PROCESSENTRY32W);
	h = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (Process32First(h, &pe))
	{
		do
		{
			if (pe.th32ProcessID == pid)
			{
				ppid = pe.th32ParentProcessID;
				break;
			}
		} while (Process32Next(h, &pe));
	}
	CloseHandle(h);
	return (ppid);
}

int Extractor::getProcessName(DWORD pid, LPWSTR fname, DWORD sz)
{
	HANDLE h = NULL;
	int e = 0;
	h = OpenProcess
	(
		PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
		FALSE,
		pid
	);
	if (h)
	{
		if (GetModuleFileNameEx(h, NULL, fname, sz) == 0)
			e = GetLastError();
		CloseHandle(h);
	}
	else
	{
		e = GetLastError();
	}
	return (e);
}


Extractor::~Extractor()
{
}
