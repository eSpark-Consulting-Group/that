#include "stdafx.h"
#include "ResourceText.h"
#include <sstream>

using namespace std;

namespace CRM {
namespace Localization {

CResourceText::mappings_t& CResourceText::s_resourceMappings()
{
	static mappings_t& x = mappings_t();
	return x;
}
HMODULE CResourceText::s_instance = NULL;

bool CResourceText::LoadMappings(UINT resourceId)
{
		::GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR) &s_instance, &s_instance);
	HRSRC hRes = ::FindResource(s_instance, MAKEINTRESOURCE(resourceId), MAKEINTRESOURCE(RTEXT));
	DWORD dwSize = ::SizeofResource(s_instance, hRes);
	if (dwSize == 0)
		return false;

	HGLOBAL hGlob = ::LoadResource(s_instance, hRes);
	wistringstream input((LPCWSTR) ::LockResource(hGlob));
	wstring line;
	s_resourceMappings().clear();
	while (getline(input, line))
	{
		if (line.size() == 0)
			continue;
		if (line[0] == L';')
			continue;
		UINT id = 0;
		wstring name;
		wchar_t delim;
		wistringstream lineInput(line);
		if (lineInput >> id >> delim >> name)
			s_resourceMappings().emplace(mappings_t::value_type(name.c_str(), id));
	}

	return true;
}

CResourceText CResourceText::FromXml(LPCWSTR xml)
{
	CResourceText result;
	MSXML2::IXMLDOMDocument2Ptr doc;
	ATLENSURE_SUCCEEDED(doc.CreateInstance(CLSID_DOMDocument));
	doc->loadXML(LPCWSTR(xml));
	auto root = doc->documentElement;
	if (root != NULL)
	{
		result._defaultText = LPCWSTR(root->text);
		auto resourceIdAttr = root->getAttribute(L"ResourceId");
		if (resourceIdAttr.vt != VT_NULL)
			result._resourceId = resourceIdAttr.bstrVal;
	}
	return result;
}

CString CResourceText::LoadString(HMODULE hModule, WORD resourceId, WORD resourceType)
{
	CString result;
	HRSRC hRes = ::FindResource(hModule, MAKEINTRESOURCE(resourceId), MAKEINTRESOURCE(resourceType));
	DWORD dwSize = ::SizeofResource(hModule, hRes);
	HGLOBAL hGlob = ::LoadResource(hModule, hRes);
	result = CString((LPCWSTR) ::LockResource(hGlob), dwSize / sizeof(WCHAR));
	if (result.Left(1) == L'\xFEFF')
		result.TrimLeft(L'\xFEFF');
	return result;
}

CResourceText::operator LPCWSTR() const
{
	mappings_t::const_iterator i = s_resourceMappings().find(_resourceId);
	bool hasText = (i != s_resourceMappings().cend());
	if (hasText)
		hasText = !!_textBuf.LoadString(s_instance, i->second);
	LPCWSTR result = hasText ? _textBuf : _defaultText;
	return result;
}

}}
