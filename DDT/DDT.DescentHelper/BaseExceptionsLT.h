#pragma once

#include <comdef.h>
#include <atltime.h>
#include "BaseTypes.h"

namespace AgentCommonTools {

class CCrmException
{
protected:
	DWORD m_code;
	CString m_message;
	CComPtr<IErrorInfo> m_com_error;

	CCrmException();

public:
	CCrmException(DWORD code, LPCWSTR message, bool _ = true);
	CCrmException(LPCWSTR message, bool _ = true);
	explicit CCrmException(const _com_error& x);

	DWORD GetCode() const { return m_code; }
	CString GetMessage() const { return m_message; }

	virtual CString ToString() const;
};

class CWin32Exception: public CCrmException
{
public:
	CWin32Exception(DWORD code);
	CWin32Exception();

	static CString GetErrorString(DWORD code);
};

class CWin32LastError
{
	DWORD m_code;
	CString m_message;
public:
	CWin32LastError(DWORD code);
	CWin32LastError();
	DWORD GetCode() const { return m_code; }
	CString GetMessage() const { return CString(m_message).Trim(L"\r\n"); }
};

class CCrtException: public CCrmException
{
public:
	CCrtException(int code);
	CCrtException();

	static CString GetErrorString(int code);
};

}
