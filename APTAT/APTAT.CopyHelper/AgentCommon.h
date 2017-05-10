#pragma once

#define WIN32_FROM_HRESULT(x) (x & 0xffff)

#define NAN std::numeric_limits<double>::quiet_NaN()
#define NANF std::numeric_limits<float>::quiet_NaN()
#define MIN_DATE 0.0
#define MAX_DATE COleDateTime(9999, 12, 31, 23, 59, 59)
// COleDateTime tick which is 1 msec interval
#define DATE_TICK (1.0 / (24.0 * 60 * 60 * 1000))
#define WSTR_NULL ((wchar_t*) 0)

#define MAX_DISCOVERY_XML_SIZE (512UL * 1024UL) // 512KB
#define MAX_DATA_SIZE (4000UL * 1024UL) // 4MB

static LPCWSTR s_DescentFileName = L"CesmHelper.exe";
