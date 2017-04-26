#pragma once

namespace CRM {
namespace Localization {

class CResourceText
{	
	friend class CFormattedText;

	typedef std::map<CString, UINT> mappings_t;

	#pragma warning(suppress:4251)
	static mappings_t& s_resourceMappings();
	static HMODULE s_instance;

	CString _resourceId;
	CString _defaultText;
	mutable CString _textBuf;

public:
	static bool LoadMappings(UINT resourceId);
	static CResourceText FromXml(LPCWSTR xml);
	static CString LoadString(HMODULE hModule, WORD resourceId, WORD resourceType = 512 /* RTEXT */);

	CResourceText()
	{ }

	CResourceText(LPCWSTR text):
		_defaultText(text)
	{ }

	CResourceText(const CString& text):
		_defaultText(text)
	{ }

	CResourceText(UINT resourceId, LPCWSTR defaultText = L""):
		_defaultText(defaultText)
	{
		_resourceId.Format(L"#%u", resourceId);
	}

	CResourceText(LPCWSTR resourceId, LPCWSTR defaultText = L""):
		_defaultText(defaultText)
	{
		_resourceId = resourceId;
		ATLASSERT(_resourceId.FindOneOf(L"\"\'&<>") == -1);
	}

	CResourceText(const CResourceText& other):
		_resourceId(other._resourceId),
		_defaultText(other._defaultText)
	{ }

	CResourceText& operator=(const CResourceText& other)
	{
		if (this != &other)
		{
			_resourceId = other._resourceId;
			_defaultText = other._defaultText;
		}
		return *this;
	}

	LPCWSTR GetResourceId() const { return _resourceId; }

	LPCWSTR GetDefaultText() const { return _defaultText; }

	operator LPCWSTR() const;

//	CString GetXml() const;
//	CString& GetXml(CString& result) const;
/*
	CResourceText& Append(LPCWSTR text)
	{
		__super::Append(text);
		return *this;
	}

	CResourceText& AppendFormat(LPCWSTR pszFormat, ...)
	{
		va_list argList;
		va_start(argList, pszFormat);
		__super::AppendFormatV(pszFormat, argList);
		va_end(argList);
		return *this;
	}
 */
};

}}
