#pragma once
#include "ResourceText.h"

namespace CRM {
namespace Localization {

class CFormattedText
{
	typedef std::pair<CString, CString> param_t;
	#pragma warning(suppress:4251)
	std::vector<param_t> _params;

	CResourceText _format;
	mutable LPWSTR _pText;

	CString FormatText(LPCWSTR format) const;

public:
	CFormattedText():
	  _pText(NULL)
	{ }

	~CFormattedText()
	{
		free(_pText);
	}

	CFormattedText(LPCWSTR format):
		_format(L"", format),
		_pText(NULL)
	{ }

	CFormattedText(const CString& format):
		_format(L"", format),
		_pText(NULL)
	{ }

	CFormattedText(const CResourceText& format):
		_format(format),
		_pText(NULL)
	{ }

	CFormattedText(const CFormattedText& other):
		_format(other._format),
		_params(other._params)
	{
		_pText = _wcsdup(other._pText);
	}

	CFormattedText(CFormattedText&& other):
		_format(std::move(other._format)),
		_params(std::move(other._params))
	{
		_pText = other._pText;
		other._pText = NULL;
	}

	CFormattedText& operator=(const CFormattedText& other)
	{
		if (this != &other)
		{
			_format = other._format;
			_params = other._params;
			free(_pText);
			_pText = _wcsdup(other._pText);
		}
		return *this;
	}

	CFormattedText& operator=(CFormattedText&& other)
	{
		std::swap(_format, other._format);
		std::swap(_params, other._params);
		free(_pText);
		_pText = other._pText;
		return *this;
	}

	static CFormattedText FromXml(LPCWSTR xml, bool acceptAsText = false);

	CFormattedText& AddParameter(LPCWSTR value)
	{
		_params.push_back(param_t(L"string", value));
		free(_pText);
		_pText = NULL;
		return *this;
	}

	CFormattedText& AddParameter(int value)
	{
		_params.push_back(param_t(L"int", _variant_t(value)));
		free(_pText);
		_pText = NULL;
		return *this;
	}

	operator LPCWSTR() const
	{
		if (_pText == NULL)
			_pText = _wcsdup(FormatText(_format));
		return _pText;
	}

	CString ToString()
	{
		return FormatText(_format);
	}

	CString ToInvariantString() const
	{
		return FormatText(_format.GetDefaultText());
	}

	CResourceText GetFormat() const { return _format; }

	bool HasResourceId() const { return !_format._resourceId.IsEmpty(); }

	CString& GetXml(CString& result, LPCWSTR rootName = L"FormattedText") const;

	CString GetXml(LPCWSTR rootName = L"FormattedText") const
	{
		CString result;
		GetXml(result, rootName);
		return result;
	}

	MSXML2::IXMLDOMElementPtr GetXmlElement(LPCWSTR rootName = L"FormattedText") const;
};

}}
