#include "stdafx.h"
#include "DefaultStrings.h"

CResourceText Resources::Descent_ConnectionError(L"Descent_ConnectionError", L"Cannot establish network connection to the server {0} ({1})");

Resources::SInit Resources::s_init;

Resources::SInit::SInit()
{
	CResourceText::LoadMappings(IDT_RESOURCE_MAPPINGS);
}
