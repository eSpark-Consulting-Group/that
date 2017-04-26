#ifndef _S_SAGENTVERSION_H_
#define _S_SAGENTVERSION_H_
//#include "stdafx.h"

struct SAgentVersion
{
	SAgentVersion(): m_ulMajorVersion(0), m_ulMinorVersion(0), m_ulBuild(0), m_ulRevision(0)
	{
	}

	SAgentVersion(uint32_t ulMajorVersion, uint32_t ulMinorVersion):
		m_ulMajorVersion(ulMajorVersion), m_ulMinorVersion(ulMinorVersion), m_ulBuild(0), m_ulRevision(0)
	{
	}

	SAgentVersion(uint32_t ulMajorVersion, uint32_t ulMinorVersion, uint32_t ulBuild, uint32_t ulRevision):
		m_ulMajorVersion(ulMajorVersion), m_ulMinorVersion(ulMinorVersion), m_ulBuild(ulBuild), m_ulRevision(ulRevision)
	{
	}

	SAgentVersion(const SAgentVersion& src)
	{
		*this = src;
	}

	SAgentVersion(LPCWSTR versionString)
	{
		swscanf_s(versionString, L"%lu.%lu.%lu.%lu", &m_ulMajorVersion, &m_ulMinorVersion, &m_ulBuild, &m_ulRevision);
	}

	SAgentVersion& operator=(const SAgentVersion& src)
	{
		this->m_ulMajorVersion = src.m_ulMajorVersion;
		this->m_ulMinorVersion = src.m_ulMinorVersion;
		this->m_ulBuild = src.m_ulBuild;
		this->m_ulRevision = src.m_ulRevision;
		return *this;
	}

	bool IsEmpty() const { return (m_ulMajorVersion | m_ulMinorVersion | m_ulBuild | m_ulRevision) == 0; }

	bool IsInvalid() const { return (m_ulMajorVersion & m_ulMinorVersion & m_ulBuild & m_ulRevision) == 0xFFFFFFFF; }

	void Invalidate() { m_ulMajorVersion = m_ulMinorVersion = m_ulBuild = m_ulRevision = 0xFFFFFFFF; }

	CString ToString()
	{
		CString result;
		result.Format(L"%d.%d.%d.%d", m_ulMajorVersion, m_ulMinorVersion, m_ulBuild, m_ulRevision);
		return result;
	}

	uint32_t m_ulMajorVersion, m_ulMinorVersion, m_ulBuild, m_ulRevision;

	bool operator==(const SAgentVersion& other) const
	{
		return memcmp(this, &other, sizeof(SAgentVersion)) == 0;
	}

	bool operator!=(const SAgentVersion& other) const
	{
		return memcmp(this, &other, sizeof(SAgentVersion)) != 0;
	}

	bool operator>(const SAgentVersion& other) const
	{
		return
			m_ulMajorVersion > other.m_ulMajorVersion ||
			m_ulMajorVersion == other.m_ulMajorVersion && (m_ulMinorVersion > other.m_ulMinorVersion ||
                                                           m_ulMinorVersion == other.m_ulMinorVersion && (m_ulBuild > other.m_ulBuild ||
			                                                                                              m_ulBuild == other.m_ulBuild && m_ulRevision > other.m_ulRevision));
	}

	bool operator>=(const SAgentVersion& other) const
	{
		return *this == other || *this > other;
	}

	bool operator<(const SAgentVersion& other) const
	{
		return
			m_ulMajorVersion < other.m_ulMajorVersion ||
			m_ulMajorVersion == other.m_ulMajorVersion && (m_ulMinorVersion < other.m_ulMinorVersion ||
                                                           m_ulMinorVersion == other.m_ulMinorVersion && (m_ulBuild < other.m_ulBuild ||
			                                                                                              m_ulBuild == other.m_ulBuild && m_ulRevision < other.m_ulRevision));
	}

	bool operator<=(const SAgentVersion& other) const
	{
		return *this == other || *this < other;
	}
};

#endif
