#ifndef _C_IPENDPOINT_H_
#define _C_IPENDPOINT_H_

#include "BaseTypes.h"

#include <WS2tcpip.h>
#include <string>

namespace CRM 
{
	namespace AgnCore 
	{
		namespace Transport
		{
			class DllExport CIPEndPoint
			{
			private:
			    IN_ADDR m_ipv4;
				IN6_ADDR m_ipv6;
                bool m_bIpV4, m_bIpV6;

                wchar_t m_strHost[NI_MAXHOST];
				UINT m_port;

            public:
                CIPEndPoint();
				CIPEndPoint(LPCWSTR strHost, USHORT port = 0);
				CIPEndPoint(const CIPEndPoint & src);
				bool operator == (const CIPEndPoint &src) const
                {
					return (m_port == src.m_port && !wcsncmp(m_strHost, src.m_strHost, NI_MAXHOST));
				}
                CIPEndPoint& operator = (const CIPEndPoint& src)
                {
                    wcscpy_s(m_strHost, _countof(m_strHost), src.m_strHost);
                    m_ipv4 = src.m_ipv4;
                    m_ipv6 = src.m_ipv6;
                    m_port = src.m_port;
					m_bIpV4 = src.m_bIpV4;
					m_bIpV6 = src.m_bIpV6;

                    return *this;
                }

                // Initialize endpoint (resolve addresses)
				bool Init(LPCWSTR hostname, UINT port);

                // Resolve using current hostName and port
				bool Resolve()
				{
					return Init( m_strHost, m_port );
				}

				IN_ADDR GetIPv4() const { return m_ipv4; }
				IN6_ADDR GetIPv6() const { return m_ipv6; }

                bool IsIpv4() const {return m_bIpV4;}
                bool IsIpv6() const {return m_bIpV6;}

				UINT GetPort() const { return m_port; }
                LPCWSTR GetHost() const { return m_strHost; }

				void SetIsIPv4()
				{
					m_bIpV4 = true;
					m_bIpV6 = false;
				}

				void SetIsIPv6()
				{
					m_bIpV4 = false;
					m_bIpV6 = true;
				}

				bool IsLoopback() const;
				bool IsPrivate() const;

				void Clear();
			};
		}
	}
}
#endif