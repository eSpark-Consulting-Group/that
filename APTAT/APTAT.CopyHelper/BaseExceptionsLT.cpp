#include "stdafx.h"
#include "BaseExceptionsLT.h"

namespace AgentCommonTools {

CCrmException::CCrmException(DWORD code, LPCWSTR message, bool _):
	m_code(code),
	m_message(message)
{ }

CCrmException::CCrmException(LPCWSTR message, bool _):
	m_code(-1),
	m_message(message)
{ }

CCrmException::CCrmException(const _com_error& x)
{
	m_code = (DWORD) x.Error();
	m_message = x.ErrorMessage();
	m_com_error = x.ErrorInfo();
}

CCrmException::CCrmException():
	m_code(0)
{ }

CString CCrmException::ToString() const
{
	CString message = m_message;
	if (!message.IsEmpty())
		message = L": " + message.TrimRight(L"\r\n");
	message.Append(L"\n");

	CString result;
	if (m_com_error != NULL)
	{
		result.Format(L"COM error 0x%08x%s", m_code, message);
		{
			_bstr_t source;
			m_com_error->GetSource(source.GetAddress());
			if (source.length() != 0)
				result.AppendFormat(L"Source: %s\n", source);
		}
		{
			_bstr_t description;
			m_com_error->GetDescription(description.GetAddress());
			if (description.length() != 0)
				result.AppendFormat(L"Description: %s\n", description);
		}
	}
	else
	{
		result.Format(L"Exception code %d (0x%08x)%s", m_code, m_code, message);
	}
	return result;
}

CString CWin32Exception::GetErrorString(DWORD code)
{
	CString m;
    WCHAR* lpMsgBuf = NULL;
	// First try English resources
    DWORD nChars = ::FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER|FORMAT_MESSAGE_FROM_SYSTEM, NULL, code, 1033, (LPWSTR) &lpMsgBuf, 0, NULL);
	// Then default
	if (nChars == 0)
		nChars = ::FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER|FORMAT_MESSAGE_FROM_SYSTEM, NULL, code, 0, (LPWSTR) &lpMsgBuf, 0, NULL);
	if (nChars != 0)
		m = lpMsgBuf;
    ::LocalFree(lpMsgBuf);
	return m;
}

CWin32Exception::CWin32Exception(DWORD code)
{
	m_code = code;
	m_message = GetErrorString(m_code);
}

CWin32Exception::CWin32Exception()
{
	m_code = ::GetLastError();
	m_message = GetErrorString(m_code);
}

CWin32LastError::CWin32LastError(DWORD code)
{
	m_code = code;
	m_message = CWin32Exception::GetErrorString(m_code);
}

CWin32LastError::CWin32LastError()
{
	m_code = ::GetLastError();
	m_message = CWin32Exception::GetErrorString(m_code);
}

CCrtException::CCrtException(int code)
{
	m_code = code;
	m_message = GetErrorString(m_code);
}
CCrtException::CCrtException()
{
	m_code = errno;
	m_message = GetErrorString(m_code);
}

CString CCrtException::GetErrorString(int code)
{
	wchar_t szBuf[1024] = { };
	_wcserror_s(szBuf, code);
	return szBuf;
}

}
