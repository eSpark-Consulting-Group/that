#include "stdafx.h"
#include "IPEndpoint.h"

#include <string>

namespace CRM 
{
    namespace AgnCore 
    {
        namespace Transport
        {
			bool CIPEndPoint::Init(LPCWSTR hostname, UINT uPort)
			{
                bool ok = false;

                m_bIpV4 = false;
                m_bIpV6 = false;
                memset(&m_ipv4, 0, sizeof(m_ipv4));
                memset(&m_ipv6, 0, sizeof(m_ipv6));

                m_port = uPort;
                wcscpy_s(m_strHost, _countof(m_strHost), hostname);

				ADDRINFOW aiHints;
                ADDRINFOW* aiList;

                // IPV4
                memset(&aiHints, 0, sizeof(aiHints));
                aiHints.ai_family = AF_INET;
                aiHints.ai_socktype = SOCK_STREAM;
                aiHints.ai_protocol = IPPROTO_TCP;
				aiList = NULL;

                if (::GetAddrInfo(hostname, NULL, &aiHints, &aiList) == 0)
                {
                    sockaddr_in* _4 = (sockaddr_in*) aiList->ai_addr;
                    m_ipv4 = _4->sin_addr;
                    ::FreeAddrInfo(aiList);
                    m_bIpV4 = true;
                    ok = true;
                }

                // IPV6
                memset(&aiHints, 0, sizeof(aiHints));
                aiHints.ai_family = AF_INET6;
                aiHints.ai_socktype = SOCK_STREAM;
                aiHints.ai_protocol = IPPROTO_TCP;
				aiList = NULL;

                if (::GetAddrInfo(hostname, NULL, &aiHints, &aiList) == 0)
                {
                    sockaddr_in6* _6 = (sockaddr_in6*) aiList->ai_addr;
                    m_ipv6 = _6->sin6_addr;
                    ::FreeAddrInfo(aiList);
                    m_bIpV6 = true;
                    ok = true;
                }

                return ok;
            }

			bool CIPEndPoint::IsLoopback() const
			{
				if (m_bIpV4)
				{
					return m_ipv4.S_un.S_un_b.s_b1 == 127;
				}
				else if (m_bIpV6)
				{
					// Check for ::1
					return m_ipv6.u.Word[0] == 0 &&
						   m_ipv6.u.Word[1] == 0 &&
						   m_ipv6.u.Word[2] == 0 &&
						   m_ipv6.u.Word[3] == 0 &&
						   m_ipv6.u.Word[4] == 0 &&
						   m_ipv6.u.Word[5] == 0 &&
						   m_ipv6.u.Word[6] == 0 &&
						   m_ipv6.u.Word[7] == 0x0100;    // big-endian for 0x0001
				}
				else
					return false;
			}

			bool CIPEndPoint::IsPrivate() const
			{
				if (m_bIpV4)
				{
					return (m_ipv4.S_un.S_un_b.s_b1 == 10) ||
						   (m_ipv4.S_un.S_un_b.s_b1 == 172 && m_ipv4.S_un.S_un_b.s_b2 >= 16 && m_ipv4.S_un.S_un_b.s_b2 <= 31) ||
						   (m_ipv4.S_un.S_un_b.s_b1 == 192 && m_ipv4.S_un.S_un_b.s_b2 == 168) ||
						   (m_ipv4.S_un.S_un_b.s_b1 == 169 && m_ipv4.S_un.S_un_b.s_b2 == 254);
				}
				else if (m_bIpV6)
				{
					return
					// Check if it's either link-local address (fe80::/64)
					   (m_ipv6.u.Word[0] == 0x80fe &&	// big-endian for 0xfe80
						m_ipv6.u.Word[1] == 0      &&
						m_ipv6.u.Word[2] == 0      &&
						m_ipv6.u.Word[3] == 0) ||
					// or unique-local address (fc00::/7)
					   (m_ipv6.u.Byte[0] == 0xfc || m_ipv6.u.Byte[0] == 0xfd);
				}
				else
					return false;
			}

			void CIPEndPoint::Clear()
			{
				memset(m_strHost, 0, sizeof(m_strHost));
				memset(&m_ipv4, 0, sizeof(m_ipv4));
				memset(&m_ipv6, 0, sizeof(m_ipv6));
				m_bIpV4 = false;
				m_bIpV6 = false;
				m_port = 0;
			}

            CIPEndPoint::CIPEndPoint() :
                m_bIpV4(false),
                m_bIpV6(false),
                m_port(0)
            {
                m_strHost[0] = 0;
                memset(&m_ipv4, 0, sizeof(m_ipv4));
                memset(&m_ipv6, 0, sizeof(m_ipv6));
            }

            CIPEndPoint::CIPEndPoint(LPCWSTR strHost, USHORT port)  : 
                m_bIpV4(false),
                m_bIpV6(false),
                m_port(0)
            {
                Init(strHost, port);
            }

            CIPEndPoint::CIPEndPoint(const CIPEndPoint & src)
            {
                m_ipv4 = src.m_ipv4;
                m_ipv6 = src.m_ipv6;
                m_port = src.m_port;
                m_bIpV4 = src.m_bIpV4;
                m_bIpV6 = src.m_bIpV6;
                wcscpy_s(this->m_strHost, _countof(m_strHost), src.m_strHost);
            }
        }
    }
}
