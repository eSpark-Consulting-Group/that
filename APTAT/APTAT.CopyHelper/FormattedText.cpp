#include "stdafx.h"
#include "ResourceText.h"
#include "FormattedText.h"

namespace CRM {
namespace Localization {

CFormattedText CFormattedText::FromXml(LPCWSTR xml, bool acceptAsText /* = false */)
{
	CFormattedText result;

	MSXML2::IXMLDOMDocumentPtr doc;
	ATLENSURE_SUCCEEDED(doc.CreateInstance(CLSID_DOMDocument));
	do
	{
		doc->loadXML(xml);
		if (doc->documentElement == NULL)
		{
			if (acceptAsText)
			{
				result._format._defaultText = xml;
				result._pText = _wcsdup(xml);
				break;
			}
			else
				throw CCrmException(L"Couldn't load XML document", false);
		}

		MSXML2::IXMLDOMElementPtr formatElem = doc->documentElement->selectSingleNode(L"Format");
		if (formatElem == NULL)
			throw CCrmException(L"<Format> element is missing", false);

		result._format._defaultText = LPCWSTR(formatElem->text);
		auto resourceIdAttr = formatElem->getAttribute(L"ResourceId");
		if (resourceIdAttr.vt != VT_NULL)
			result._format._resourceId = resourceIdAttr.bstrVal;

		MSXML2::IXMLDOMElementPtr argsElem = doc->documentElement->selectSingleNode(L"Arguments");
		if (argsElem == NULL)
			break;

		auto argsList = argsElem->childNodes;
		while (MSXML2::IXMLDOMElementPtr arg = argsList->nextNode())
		{
			auto type = arg->nodeName;
			auto value = arg->text;
			result._params.push_back(param_t(LPCWSTR(type), LPCWSTR(value)));
		}
	}
	while (0);

	return result;
}

CString CFormattedText::FormatText(LPCWSTR format) const
{
	CString result = format;
	WCHAR pholder[5] = L"{0}";

	int lEsc = result.Replace(L"{{", L"\x02");
	int rEsc = result.Replace(L"}}", L"\x03");

	for (unsigned i = 0; i <= 9; i++)
	{
		if (result.Find(L'{') == -1)
			break;
		pholder[1] = L'0' + i;
		if (i < _params.size())
			result.Replace(pholder, _params[i].second);
	}

	if (lEsc)
		result.Replace(L'\x02', L'{');
	if (rEsc)
		result.Replace(L'\x03', L'}');

	return result;
}

CString& CFormattedText::GetXml(CString& result, LPCWSTR rootName) const
{
	if (_format._resourceId == L"" && _format._defaultText == L"")
	{
		result.Format(L"<%s><Format ResourceId=\"\"/></%s>", rootName, rootName);
	}
	else
	{
		result = LPCWSTR(GetXmlElement(rootName)->xml);
	}
	return result;
}

MSXML2::IXMLDOMElementPtr CFormattedText::GetXmlElement(LPCWSTR rootName) const
{
	MSXML2::IXMLDOMDocumentPtr doc;
	ATLENSURE_SUCCEEDED(doc.CreateInstance(CLSID_DOMDocument));

	if (_format._resourceId == L"" && _format._defaultText == L"")
	{
		CString rootXml;
		rootXml.Format(L"<%s><Format ResourceId=\"\"/></%s>", rootName, rootName);
		doc->loadXML(LPCWSTR(rootXml));
		return doc->documentElement;
	}
	else
	{
		auto formatElem = doc->createElement(L"Format");
		formatElem->setAttribute(L"ResourceId", _variant_t(_format.GetResourceId()));
		formatElem->text = _format.GetDefaultText();

		auto rootElem = doc->createElement(rootName);
		rootElem->appendChild(formatElem);

		if (_params.size() != 0)
		{
			auto argumentsElem = doc->createElement(L"Arguments");
			for (auto p = _params.cbegin(); p != _params.cend(); p++)
			{
				auto argElem = doc->createElement(LPCWSTR(p->first));
				argumentsElem->appendChild(argElem)->text = LPCWSTR(p->second);
			}
			rootElem->appendChild(argumentsElem);
		}
		return rootElem;
	}
}

}}