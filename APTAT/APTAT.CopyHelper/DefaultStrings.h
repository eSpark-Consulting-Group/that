pragma once
#include "ResourceText.h"

using namespace CRM::Localization;

class Resources
{
public:
	static CResourceText Descent_ConnectionError;

private:
	static struct SInit { SInit(); } s_init;
};
