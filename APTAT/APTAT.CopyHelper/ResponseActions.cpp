#include "stdafx.h"
#include <atlstr.h>
#include "Installer.h"
#include "AgentInstallUtils.h"
#include "AgentSslSocketTools.h"
#include "FormattedText.h"
#include <ATLComTime.h>
#include <sstream>
#include "ImpersonationHelper.h"

using namespace CRM::AgnCore::Transport;
using namespace CRM::Localization;

#include <algorithm>

void RemoteInstallerService::formatResponse(LPCWSTR format, ...)
{
	wchar_t buffer[1024];

	va_list args;
	va_start(args, format);
	vswprintf_s(buffer, sizeof(buffer) / sizeof(buffer[0]), format, args);
	va_end(args);

	m_responseString = buffer;
}


/**
* process_response_action Executes special action. Response file name (output)
*               must contain DescentStatus;win32error#errorString\nRESPONSE STRING
*
* @param argc Must be >= 3
* @param argv[0] - ActionName
* @param argv[1] - Output filename
* @param argv[2] - Timeout, in ms
* @param argv[n] - Parameters
*
* @returns
*/
int RemoteInstallerService::process_response_action(int argc, LPTSTR argv[])
{
	if (argc != 2)
	{
		DO_LOG(L"Invalid parameters count %d", argc);
		return -1;
	}

	m_responseString = L"";
	m_err = ERROR_INVALID_FUNCTION;
	m_status = status_error;
	m_ErrorString = L"";

	CString responseFile = argv[0];
	m_responseFile = responseFile;
	int argc_ = argc - 1;
	LPTSTR* argv_ = argv + 1;


	auto ret = action_copy_files(argc_, argv_);


	writeStatusToFile(responseFile, true);

	return ret;
}

bool RemoteInstallerService::writeStatusToFile(CString responseFile, bool forceWrite) const
{
	DO_LOG(L"writeStatusToFile");
	static bool firstTime = true;
	static clock_t  last;
	clock_t now = clock();
	if (!firstTime && !forceWrite)
	{
		clock_t d = now - last;
		double seconds = static_cast<double>(d) / CLOCKS_PER_SEC;
		if (seconds < 2)
		{
			DO_LOG(L"Previous write have been less than 2 seconds ago (%f)", seconds);
			return false;
		}
	}
	last = now;
	firstTime = false;

	if (::TryEnterCriticalSection(&m_cs))
	{
		static clock_t time = clock();
		// Write result
		wchar_t currentFolder[MAX_PATH];
		wchar_t outputFile[MAX_PATH];

		if (responseFile.Find(L'\\') != -1)
		{
			swprintf_s(outputFile, L"%s", responseFile.GetBuffer());
		}
		else
		{
			GetModuleFileNameW(NULL, currentFolder, MAX_PATH - 1);
			memset(wcsrchr(currentFolder, '\\') + 1, 0, wcslen(wcsrchr(currentFolder, '\\')));
			//::GetWindowsDirectoryW(currentFolder, _countof(currentFolder));
			swprintf_s(outputFile, L"%s%s", currentFolder, responseFile.GetBuffer());
		}
		::DeleteFile(outputFile);

		HANDLE hFile = ::CreateFile(outputFile, GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, NULL, NULL);

		if (hFile == INVALID_HANDLE_VALUE)
		{
			DO_LOG(L"Error: can not create %s", outputFile);
			return false;
		}

		// Write in the response file
		wchar_t buf[1024] = { 0 };
		wchar_t buf2[1024] = { 0 };
		DWORD dw;
		swprintf_s(buf, L"%d;%d#%s", (UINT)m_status, (UINT)m_err, m_ErrorString);
		swprintf_s(buf2, L"%d", (UINT)m_currentPercentage);


		DO_LOG(L"ResponseErr: %s", buf);
		DO_LOG(L"ResponseString: %s", m_responseString);
		DO_LOG(L"Current pecentage: %d", m_currentPercentage);

		::WriteFile(hFile, buf, wcslen(buf) * sizeof(buf[0]), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		::WriteFile(hFile, m_responseString, m_responseString.GetLength() * sizeof(WCHAR), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		::WriteFile(hFile, buf2, wcslen(buf2) * sizeof(buf2[0]), &dw, NULL);
		::WriteFile(hFile, L"\r\n", 4, &dw, NULL);

		///////////////////////////////////////////////////////////////////////////////////////////////////
		for (int i = 0; i < m_fileStatus.size(); i++)
		{
			wchar_t buf3[MAX_PATH + 3] = { 0 };
			swprintf_s(buf3, L"%d;%s", (UINT)m_fileStatus[i], m_filePath[i]);
			::WriteFile(hFile, buf3, wcslen(buf3) * sizeof(buf3[0]), &dw, NULL);
			::WriteFile(hFile, L"\r\n", 4, &dw, NULL);
		}

		::CloseHandle(hFile);
		::LeaveCriticalSection(&m_cs);
		return true;

	}
	return false;
}



int RemoteInstallerService::action_copy_file(int argc, LPTSTR argv[])
{
	if (argc != 1)
	{
		setStatus(status_error);
		formatContextError(L"Arguments count error: %d != 1", argc);
		assert(false);
		return -1;
	}
	DWORD dwExitCode = 0;
	wchar_t strPath[MAX_PATH];
	wchar_t fileName[MAX_PATH];
	wchar_t extension[MAX_PATH];
	CString originalStrPath = argv[0];
	wchar_t buf[MAX_PATH] = {};
	::GetTempPath(_countof(buf), buf);
	_wsplitpath(originalStrPath, NULL, NULL, fileName, extension);
	swprintf_s(strPath, L"%s\\%s%s", buf, fileName, extension);
	wchar_t messageToLog[MAX_PATH] = {};
	swprintf_s(messageToLog, L"%s%s", "Copying exe file to %s", strPath);
	DO_LOG(messageToLog);
	if (!::CopyFile(originalStrPath, strPath, false))
	{
		CWin32LastError error;
		DO_LOG(L"CopyFile Failed failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
		if (error.GetCode() == ERROR_SHARING_VIOLATION)
		{
			wchar_t tempPath[MAX_PATH] = {};
			::GetTempFileName(buf, L"ddt_____", 0, tempPath);
			swprintf_s(strPath, L"%s%s", tempPath, extension);
			swprintf_s(messageToLog, L"%s%s", "Copying exe file to %s", strPath);
			DO_LOG(messageToLog);
			if (!::CopyFile(originalStrPath, strPath, false))
			{
				CWin32LastError error;
				DO_LOG(L"CopyFile Failed failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
				m_responseString = error.GetMessage();
				m_err = error.GetCode();
				setStatus(status_installer_fatal_error);
				return m_err;
			}
		}
	}
	m_responseString = strPath;
	setStatus(status_success);
	return dwExitCode;
}

int RemoteInstallerService::action_copy_files(int argc, LPTSTR argv[])
{
	if (argc != 1)
	{
		setStatus(status_error);
		formatContextError(L"Arguments count error: %d != 1", argc);
		assert(false);
		return -1;
	}
	DWORD dwExitCode = 0;
	wstring filePathes = argv[0];
	std::vector<CString> pathes;
	{
		std::wifstream ifs(filePathes);
		std::wstring content((std::istreambuf_iterator<wchar_t>(ifs)),
			(std::istreambuf_iterator<wchar_t>()));
		AgentCommonTools::SplitString(content.c_str(), L";", pathes, false);
	}


	for (int i = 0; i < pathes.size(); i++)
	{
		DWORD err = 0;
		wchar_t strPath[MAX_PATH];
		wchar_t fileName[MAX_PATH];
		wchar_t extension[MAX_PATH];
		CString originalStrPath = pathes[i];
		wchar_t buf[MAX_PATH] = {};
		::GetTempPath(_countof(buf), buf);
		_wsplitpath(originalStrPath, NULL, NULL, fileName, extension);
		swprintf_s(strPath, L"%s%s%s", buf, fileName, extension);
		wchar_t messageToLog[MAX_PATH] = {};
		swprintf_s(messageToLog, L"%s%s", "Copying exe file to %s", strPath);
		DO_LOG(messageToLog);
		if (!::CopyFile(originalStrPath, strPath, false))
		{
			CWin32LastError error;
			DO_LOG(L"CopyFile Failed failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
			if (error.GetCode() == ERROR_SHARING_VIOLATION)
			{
				wchar_t tempPath[MAX_PATH] = {};
				::GetTempFileName(buf, L"fat_____", 0, tempPath);
				swprintf_s(strPath, L"%s%s", tempPath, extension);
				swprintf_s(messageToLog, L"%s%s", "Copying exe file to %s", strPath);
				DO_LOG(messageToLog);
				if (!::CopyFile(originalStrPath, strPath, false))
				{
					CWin32LastError error;
					DO_LOG(L"CopyFile Failed failed: %s (%d)", error.GetMessage(), (dwExitCode = error.GetCode()));
					m_responseString = error.GetMessage();
					err = error.GetCode();
					m_filePath.push_back(strPath);
					m_fileStatus.push_back(err);
				}
			}
		}
		if (err == 0)
		{
			TCHAR Path[MAX_PATH];
			HANDLE hFile;
			DWORD dwRet;

			hFile = CreateFile(strPath,               // file to open
				GENERIC_READ,          // open for reading
				FILE_SHARE_READ,       // share for reading
				NULL,                  // default security
				OPEN_EXISTING,         // existing file only
				FILE_ATTRIBUTE_NORMAL, // normal file
				NULL);                 // no attr. template

			if (hFile == INVALID_HANDLE_VALUE)
			{
				m_filePath.push_back(strPath);
				m_fileStatus.push_back(m_err);
			}

			dwRet = GetFinalPathNameByHandle(hFile, Path, MAX_PATH, VOLUME_NAME_NT);
			CloseHandle(hFile);
			m_filePath.push_back(strPath);
			m_fileStatus.push_back(0);
		}
	}
	setStatus(status_success);
	return dwExitCode;
}

