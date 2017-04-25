#pragma once
#include <windows.h>
#include <string>

class Extractor
{
	int getProcessName(DWORD pid, LPWSTR fname, DWORD sz);
	DWORD getParentPID(DWORD pid);
public:
	Extractor();
	void ExtractId();
	bool WriteAllTextUtf8(LPCWSTR path, const std::wstring& text);
	~Extractor();
};

