#include "stdafx.h"
#include "sslsocket.h"
#include "IPEndPoint.h"
#include "BaseExceptionsLT.h"
#include <winsock2.h>
#include <mstcpip.h>

using namespace CRM::AgnCore::Transport;

#ifndef LOG_DEBUG
#define LOG_DEBUG DO_LOG
#define LOG_CONTEXT(name)
#endif

CSslSocket::CSslSocket():
	m_Socket(NULL),
	m_bSecureConnection(false),
	m_cbData(0),
	m_uBufferLength(0),
	m_pRecvBuffer(NULL),
	m_pSendBuffer(NULL)
{
	m_pSecurityFuncs = ::InitSecurityInterface();

	memset(&m_BufferSizes, 0, sizeof(m_BufferSizes));
	memset(&m_hCreds, 0, sizeof(m_hCreds));
	memset(&m_hContext, 0, sizeof(m_hContext));
}

CSslSocket::~CSslSocket()
{
	Close();
}

bool CSslSocket::SetKeepAlive(ULONG time, ULONG interval)
{
	DWORD optval = 1;
	socklen_t optlen = sizeof(optval);
	int rc = ::setsockopt(m_Socket, SOL_SOCKET, SO_KEEPALIVE, (char*)&optval, optlen);
	if (rc == SOCKET_ERROR)
	{
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"setsockopt failed to set SO_KEEPALIVE: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	tcp_keepalive keepalive = { };
	keepalive.onoff = 1;
	keepalive.keepalivetime = time;
	keepalive.keepaliveinterval = interval;
	DWORD dwSize = 0;
	rc = ::WSAIoctl(m_Socket, SIO_KEEPALIVE_VALS, &keepalive, sizeof(keepalive), NULL, 0, &dwSize, NULL, NULL);
	if (rc == SOCKET_ERROR)
	{
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"WSAIoctl failed to set SIO_KEEPALIVE_VALS: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	return true;
}

bool CSslSocket::SetLinger(BOOL enabled, USHORT timeout)
{
	linger lin;
	lin.l_onoff = enabled;
	lin.l_linger = timeout;

	int rc = ::setsockopt(m_Socket, SOL_SOCKET, SO_LINGER, (char*)&lin, sizeof(lin));
	if (rc == SOCKET_ERROR)
	{
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"setsockopt failed to set SO_LINGER: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	return true;
}

bool CSslSocket::ConnectIPv4(IN_ADDR uAddress, UINT uPort)
{
	if (!IsClosed())
	{
		Close();
	}

	m_Socket = ::socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (m_Socket == INVALID_SOCKET)
	{
		m_Socket = NULL;
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"socket failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	// Disable inheritance of this socket (See Ticket #3298)
	BOOL b = ::SetHandleInformation((HANDLE) m_Socket, HANDLE_FLAG_INHERIT, 0);
	if (!b)
	{
		AgentCommonTools::CWin32LastError error;
		LOG_DEBUG(L"SetHandleInformation failed: %s (%d)", error.GetMessage(), error.GetCode());
	}

	SOCKADDR_IN SockAddr = { };
	SockAddr.sin_family = AF_INET;
	SockAddr.sin_addr = uAddress;
	SockAddr.sin_port = htons( uPort );

	if (::connect(m_Socket, (sockaddr*) &SockAddr, sizeof(SockAddr)) != 0)
	{
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"connect failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	SetLinger(TRUE, 5);
	m_bSecureConnection = false;

	return true;
}

bool CSslSocket::ConnectIPv6(IN6_ADDR uAddress, UINT uPort)
{
	if (!IsClosed())
	{
		Close();
	}

	m_Socket = ::socket(AF_INET6, SOCK_STREAM, IPPROTO_TCP);
	if (m_Socket == INVALID_SOCKET)
	{
		m_Socket = NULL;
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"socket failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	SOCKADDR_IN6 SockAddr = { };
	SockAddr.sin6_family = AF_INET6;
	SockAddr.sin6_addr = uAddress;
	SockAddr.sin6_port = htons( uPort );
	SockAddr.sin6_flowinfo = 0; 
	SockAddr.sin6_scope_id = 0;

	if (::connect(m_Socket, (sockaddr*) &SockAddr, sizeof(SockAddr)) != 0)
	{
		AgentCommonTools::CWin32LastError error(::WSAGetLastError());
		LOG_DEBUG(L"connect failed: %s (%d)", error.GetMessage(), error.GetCode());
		return false;
	}

	SetLinger(TRUE, 5);
	m_bSecureConnection = false;

	return true;
}

bool CSslSocket::Connect(CIPEndPoint& endPoint)
{
	if (ConnectIPv4(endPoint.GetIPv4(), endPoint.GetPort()))
	{
		endPoint.SetIsIPv4();
		return true;
	}
	if (ConnectIPv6(endPoint.GetIPv6(), endPoint.GetPort()))
	{
		endPoint.SetIsIPv6();
		return true;
	}
	return false;
}

bool CSslSocket::GetLocalEndpoint(CRM::AgnCore::Transport::CIPEndPoint& endpoint)
{
	union
	{
		SOCKADDR_STORAGE storage;
		SOCKADDR_IN addr4;
		SOCKADDR_IN6 addr6;
		SOCKADDR addr;
	};
	int cb = sizeof(storage);
	int rc = getsockname(m_Socket, &addr, &cb);
	if (rc != NO_ERROR)
		return false;
	wchar_t buf[NI_MAXHOST];
	USHORT port = 0;
	switch (storage.ss_family)
	{
		case AF_INET:
			if (::GetNameInfo(&addr, sizeof(addr4), buf, _countof(buf), NULL, 0, NI_NUMERICHOST) == NO_ERROR)
			{
				port = addr4.sin_port;
				endpoint.Init(buf, port);
				endpoint.SetIsIPv4();
				return true;
			}
			break;
		case AF_INET6:
			if (::GetNameInfo(&addr, sizeof(addr6), buf, _countof(buf), NULL, 0, NI_NUMERICHOST) == NO_ERROR)
			{
				port = addr6.sin6_port;
				endpoint.Init(buf, port);
				endpoint.SetIsIPv6();
				return true;
			}
			break;
	}
	return false;
}

bool CSslSocket::SecureConnect(CIPEndPoint& endPoint)
{
	CLocker lock(m_cs);

	if (!Connect(endPoint))
	{
		return false;
	}

	m_serverName = endPoint.GetHost();

	if (!ClientHandshake(m_serverName))
	{
		return false;
	}

	SECURITY_STATUS scRet;

	SecPkgContext_ConnectionInfo connInfo = { };
	scRet = m_pSecurityFuncs->QueryContextAttributes(&m_hContext, SECPKG_ATTR_CONNECTION_INFO, &connInfo);
	if (scRet == SEC_E_OK)
	{
		CString connDetails;
		ParseConnectionDetails(&connInfo, connDetails);
		LOG_DEBUG(L"SSL cipher suite: %s", connDetails);
	}
	else
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"QueryContextAttributes failed for SECPKG_ATTR_CONNECTION_INFO: %s (0x%08x)", error.GetMessage(), error.GetCode());
		return false;
	}

	scRet = m_pSecurityFuncs->QueryContextAttributes(&m_hContext, SECPKG_ATTR_STREAM_SIZES, &m_BufferSizes);
	if (scRet != SEC_E_OK)
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"QueryContextAttributes failed for SECPKG_ATTR_STREAM_SIZES: %s (0x%08x)", error.GetMessage(), error.GetCode());
		return false;
	}

	m_uBufferLength = m_BufferSizes.cbHeader + m_BufferSizes.cbMaximumMessage + m_BufferSizes.cbTrailer;
	m_pRecvBuffer = (PBYTE) calloc( m_uBufferLength, sizeof( BYTE ) );
	if( m_pRecvBuffer==NULL )
	{
		LOG_DEBUG(L"Failed to allocate %d bytes of memory for receive buffer.", m_uBufferLength);
		return false;
	}	

	m_pSendBuffer = (PBYTE) calloc( m_uBufferLength, sizeof( BYTE ) );
	if( m_pSendBuffer==NULL )
	{
		LOG_DEBUG(L"Failed to allocate %d bytes of memory for send buffer.", m_uBufferLength);
		return false;
	}

	m_bSecureConnection = true;

	return true;
}

int CSslSocket::Send(const void* lpBuf, UINT nBufLen, int nFlags) 
{
	DWORD dwTotSent = 0;

	if (IsClosed())
		return SOCKET_ERROR;

	if (m_bSecureConnection) 
	{
		SECURITY_STATUS scRet;

		PBYTE pbMessage;
		DWORD cbMessage;

		DWORD dwAvaLn = 0;
		DWORD dwDataToSend = 0;
		DWORD dwSendInd = 0;
		DWORD dwCurrLn = 0;

		pbMessage = m_pSendBuffer + m_BufferSizes.cbHeader;
		dwAvaLn = m_BufferSizes.cbMaximumMessage;
		dwDataToSend = ((DWORD)nBufLen);

		do
		{
			//ZeroMemory(m_pSendBuffer,m_uBufferLength);
			dwCurrLn = ((DWORD)nBufLen)-dwSendInd > dwAvaLn ? dwAvaLn : (((DWORD)nBufLen)-dwSendInd);
			memcpy_s( pbMessage, m_uBufferLength - m_BufferSizes.cbHeader, ((BYTE*)lpBuf) + dwSendInd, dwCurrLn );

			dwSendInd += dwCurrLn;
			dwDataToSend -= dwCurrLn;

			cbMessage = dwCurrLn;

			PBYTE pSendBuffer = NULL;
			UINT nSendBuffer = 0;
			scRet = Encrypt(pbMessage, cbMessage, pSendBuffer, nSendBuffer);//pSendBuffer is equal to m_pSendBuffer

			if (FAILED(scRet))
			{
				::SetLastError(scRet);
				break;
			}
/*
// To emulate latency issues
#ifdef _DEBUG
	::Sleep(rand() % 10 * 10);
#endif
*/
			LOG_CONTEXT(_T(__FUNCTION__));
			int rc = SendBuffer(pSendBuffer, nSendBuffer);
			if (rc == SOCKET_ERROR)
			{
				::SetLastError(::WSAGetLastError());
				break;
			}
			else
				dwTotSent += rc;
		}
		while (dwDataToSend != 0);

		int result = dwTotSent > nBufLen ? nBufLen : dwTotSent;
		return result;
	} 
	else 
	{
		LOG_CONTEXT(_T(__FUNCTION__));
		int result = SendBuffer(lpBuf, nBufLen);
		return result;
	}
}

int CSslSocket::SendBuffer(const void* buffer, uint32_t bufSize)
{
	if (bufSize != 0 && buffer != NULL)
	{
		uint32_t cbSent = 0;
		while (cbSent < bufSize)
		{
			int count = ::send(m_Socket, ((const char*) buffer) + cbSent, bufSize - cbSent, 0);
			if (count == SOCKET_ERROR)
			{
				AgentCommonTools::CWin32LastError error(::WSAGetLastError());
				LOG_DEBUG(L"send failed: %s (%d)", error.GetMessage(), error.GetCode());
				return SOCKET_ERROR;
			}
			cbSent += count;
		}
		return cbSent;
	}
	else
		return 0;
}

int CSslSocket::RecvBuffer(void* buffer, uint32_t bufSize)
{
	if (bufSize != 0 && buffer != NULL)
	{
		uint32_t cbRcvd = 0;
		while (cbRcvd < bufSize)
		{
			int count = ::recv(m_Socket, ((char*) buffer) + cbRcvd, bufSize - cbRcvd, 0);
			if (count == SOCKET_ERROR)
			{
				AgentCommonTools::CWin32LastError error(::WSAGetLastError());
				LOG_DEBUG(L"recv failed: %s (%d)", error.GetMessage(), error.GetCode());
				return SOCKET_ERROR;
			}
			else if (count == 0)
			{
				LOG_DEBUG(L"recv failed: connection closed");
				return 0;
			}
			cbRcvd += count;
		}
		return cbRcvd;
	}
	else
		return 0;
}

SECURITY_STATUS CSslSocket::Encrypt(byte* pbMessage, UINT cbMessage, PBYTE& pSendBuffer, UINT &nSendBuffer)
{
	CLocker lock(m_cs);

	SecBuffer       Buffers[4] = { };
	SecBufferDesc   Message    = { };
	SECURITY_STATUS scRet      = 0;

	Buffers[0].pvBuffer     = m_pSendBuffer;
	Buffers[0].cbBuffer     = m_BufferSizes.cbHeader;
	Buffers[0].BufferType   = SECBUFFER_STREAM_HEADER;

	Buffers[1].pvBuffer     = pbMessage;
	Buffers[1].cbBuffer     = cbMessage;
	Buffers[1].BufferType   = SECBUFFER_DATA;

	Buffers[2].pvBuffer     = pbMessage + cbMessage;
	Buffers[2].cbBuffer     = m_BufferSizes.cbTrailer;
	Buffers[2].BufferType   = SECBUFFER_STREAM_TRAILER;

	Buffers[3].BufferType   = SECBUFFER_EMPTY;

	Message.ulVersion       = SECBUFFER_VERSION;
	Message.cBuffers        = 4;
	Message.pBuffers        = Buffers;

	scRet = m_pSecurityFuncs->EncryptMessage(&m_hContext, 0, &Message, 0);
	if (scRet != SEC_E_OK)
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"EncryptMessage failed: %s (0x%08x)", error.GetMessage(), error.GetCode());
	}

	nSendBuffer = Buffers[0].cbBuffer + Buffers[1].cbBuffer + Buffers[2].cbBuffer;
	pSendBuffer = m_pSendBuffer;
	return scRet;
}

int CSslSocket::Receive(void* lpBuf, UINT nBufLen, int nFlags)
{
	SecBufferDesc   Message    = { };
	SecBuffer       Buffers[4] = { };

	if (IsClosed())
		return SOCKET_ERROR;

	if (m_bSecureConnection) 
	{
		if (m_cbData <= 0)
		{
			fd_set fd = { };
			fd.fd_array[0] = m_Socket;
			fd.fd_count = 1;

			timeval timeout;
			timeout.tv_sec = 1;
			timeout.tv_usec = 0;

			int nready = 0;
			while (!IsClosed())
			{
				nready = ::select(0, &fd, NULL, NULL, &timeout);
				if (nready == SOCKET_ERROR)
				{
					m_cbData = SOCKET_ERROR;
					AgentCommonTools::CWin32LastError error(::WSAGetLastError());
					LOG_DEBUG(L"select failed: %s (%d)", error.GetMessage(), error.GetCode());
				}
				else if (nready == 0) // timeout
				{
					fd.fd_array[0] = m_Socket;
					fd.fd_count = 1;
					continue;
				}
				break;
			}

			if (nready > 0)
			{
				m_cbData = ::recv( m_Socket, (char *)m_pRecvBuffer, m_uBufferLength, 0 );
				if (m_cbData == SOCKET_ERROR)
				{
					AgentCommonTools::CWin32LastError error(::WSAGetLastError());
					LOG_DEBUG(L"recv failed: %s (%d)", error.GetMessage(), error.GetCode());
				}
				else if (m_cbData == 0)
				{
					LOG_DEBUG(L"recv failed: connection closed");
				}
			}
		}

		while (m_cbData > 0)
		{
			SECURITY_STATUS rc = Decrypt(m_pRecvBuffer, m_cbData, Message, (SecBuffer*)Buffers);
			if (rc != SEC_E_OK)
			{
				while (rc == SEC_E_INCOMPLETE_MESSAGE)
				{
					int count = ::recv(m_Socket, (char*) m_pRecvBuffer + m_cbData, m_uBufferLength - m_cbData, 0);
					if (count == SOCKET_ERROR)
					{
						AgentCommonTools::CWin32LastError error(::WSAGetLastError());
						LOG_DEBUG(L"recv failed: %s (%d)", error.GetMessage(), error.GetCode());
						m_cbData = SOCKET_ERROR;
						break;
					}
					else if (count == 0)
					{
						LOG_DEBUG(L"recv failed: connection closed");
						m_cbData = 0;
						break;
					}
					m_cbData += count;
					rc = Decrypt(m_pRecvBuffer, m_cbData, Message, (SecBuffer*) Buffers);
				}
				if (rc != SEC_E_OK)
					break;
			}

			m_cbData = 0;

			/*none result*//*SplitData(Message, (SecBuffer*)Buffers, SECBUFFER_EXTRA, m_pRecvBuffer, nBufLen);
						   int nResult = SplitData(Message, (SecBuffer*)Buffers, SECBUFFER_DATA, lpBuf, nBufLen);*/

			int nResult = 0;
			for( UINT index=0; index<Message.cBuffers; index++ )
			{
				if( (Buffers[index].BufferType == SECBUFFER_DATA) )
				{
					nResult = Buffers[index].cbBuffer<nBufLen ? Buffers[index].cbBuffer : nBufLen;
					memcpy_s( lpBuf, nBufLen, Buffers[index].pvBuffer, nResult );
				}
				else if( (Buffers[index].BufferType == SECBUFFER_EXTRA) )
				{
					m_cbData = Buffers[index].cbBuffer;
					memcpy_s( m_pRecvBuffer, m_uBufferLength, Buffers[index].pvBuffer, Buffers[index].cbBuffer );
				}
			};

			return nResult;
		}
		return m_cbData;
	}
	else
	{
		int count = ::recv(m_Socket, (char*) lpBuf, nBufLen, 0);
		if (count == SOCKET_ERROR)
		{
			AgentCommonTools::CWin32LastError error(::WSAGetLastError());
			LOG_DEBUG(L"recv failed: %s (%d)", error.GetMessage(), error.GetCode());
		}
		else if (count == 0)
		{
			LOG_DEBUG(L"recv failed: connection closed");
		}
		return count;
	}
}

SECURITY_STATUS CSslSocket::Decrypt(/*[in]*/ PVOID pbMessage,/*[in]*/ UINT nBufLen,/*[out]*/ SecBufferDesc& Message,/*[out]*/ SecBuffer* Buffers, UINT nBuffersCount)
{
	CLocker lock(m_cs);

	Buffers[0].pvBuffer     = pbMessage;
	Buffers[0].cbBuffer     = m_cbData;
	Buffers[0].BufferType   = SECBUFFER_DATA;

	Buffers[1].BufferType   = SECBUFFER_EMPTY;
	Buffers[2].BufferType   = SECBUFFER_EMPTY;
	Buffers[3].BufferType   = SECBUFFER_EMPTY;

	Message.ulVersion       = SECBUFFER_VERSION;
	Message.cBuffers        = nBuffersCount;
	Message.pBuffers        = Buffers;

	SECURITY_STATUS scRet = m_pSecurityFuncs->DecryptMessage(&m_hContext, &Message, 0, NULL);
	if (scRet != SEC_E_OK && scRet != SEC_E_INCOMPLETE_MESSAGE)
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"DecryptMessage failed: %s (0x%08x)", error.GetMessage(), error.GetCode());
	}

	return scRet;
}

UINT CSslSocket::SplitData(/*[in]*/SecBufferDesc& Message,/*[in]*/ SecBuffer* Buffers,/*[in]*/ ULONG BufferType,/*[out]*/ PVOID &lpBuffer, UINT nBufLen)
{
	int nResult = 0;
	for( UINT index=0; index<Message.cBuffers; index++ )
	{
		if( (Buffers[index].BufferType == BufferType) && (BufferType == SECBUFFER_DATA) )
		{
			nResult = Buffers[index].cbBuffer<nBufLen ? Buffers[index].cbBuffer : nBufLen;
			memcpy_s( lpBuffer, nBufLen, Buffers[index].pvBuffer, nResult );
		}
		else if( (Buffers[index].BufferType == BufferType)&&(BufferType == SECBUFFER_EXTRA) )
		{
			m_cbData = Buffers[index].cbBuffer;
			memcpy_s( lpBuffer, nBufLen, Buffers[index].pvBuffer, Buffers[index].cbBuffer );
		}
	};
	return nResult;
}

bool CSslSocket::Listen(int nNumber)
{
	return false;
}

CSslSocket* CSslSocket::Accept()
{
	return NULL;
}

void CSslSocket::Close()
{
	CLocker lock(m_cs);

	if (m_Socket != NULL)
	{
		closesocket(m_Socket);
		m_Socket = NULL;
	}

	if (m_pRecvBuffer != NULL)
	{
		free( m_pRecvBuffer );
		m_pRecvBuffer = NULL;
	}

	if (m_pSendBuffer != NULL)
	{
		free( m_pSendBuffer );
		m_pSendBuffer = NULL;
	}

	if( m_pSecurityFuncs != NULL )
	{
		if (!IsNull(&m_hContext))
		{
			m_pSecurityFuncs->DeleteSecurityContext(&m_hContext);
			memset(&m_hContext, 0, sizeof(m_hContext));
		}
		if (!IsNull(&m_hCreds))
		{
			m_pSecurityFuncs->FreeCredentialsHandle(&m_hCreds);
			memset(&m_hCreds, 0, sizeof(m_hCreds));
		}
	}

	m_cbData = 0;
	m_uBufferLength = 0;
	m_bSecureConnection = false;
}

//
// Private
//

bool CSslSocket::ClientHandshake(LPCWSTR szServerName)
{
	SecBufferDesc   OutBuffer      = { };
	SecBuffer       OutBuffers[1]  = { };
	DWORD           dwSSPIFlags    = 0;
	DWORD           dwSSPIOutFlags = 0;
	TimeStamp       tsExpiry       = { };
	SECURITY_STATUS scRet          = 0;
	DWORD           cbData         = 0;

	scRet = m_pSecurityFuncs->AcquireCredentialsHandle(
		NULL,
		UNISP_NAME,
		SECPKG_CRED_OUTBOUND,
		NULL,
		NULL,
		NULL,
		NULL,
		&m_hCreds,
		NULL);
	if (scRet != SEC_E_OK)
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"AcquireCredentialsHandle failed: %s (0x%08x)", error.GetMessage(), error.GetCode());
		return false;
	}

	dwSSPIFlags =
		ISC_REQ_SEQUENCE_DETECT   |
		ISC_REQ_REPLAY_DETECT     |
		ISC_REQ_CONFIDENTIALITY   |
		ISC_RET_EXTENDED_ERROR    |
		ISC_REQ_ALLOCATE_MEMORY   |
		ISC_REQ_STREAM			  |
		ISC_REQ_MANUAL_CRED_VALIDATION;

	OutBuffers[0].pvBuffer   = NULL;
	OutBuffers[0].BufferType = SECBUFFER_TOKEN;
	OutBuffers[0].cbBuffer   = 0;

	OutBuffer.cBuffers = 1;
	OutBuffer.pBuffers = OutBuffers;
	OutBuffer.ulVersion = SECBUFFER_VERSION;

	scRet = m_pSecurityFuncs->InitializeSecurityContext(
		&m_hCreds,					/* phCredential */
		NULL,						/* phContext */
		(SEC_WCHAR*) szServerName,	/* pszTargetName */
		dwSSPIFlags,				/* fContextReq */
		0,							/* Reserved1 */
		0,							/* TargetDataRep */
		NULL,						/* pInput */
		0,							/* Reserved2 */
		&m_hContext,				/* phNewContext */
		&OutBuffer,					/* pOutput */
		&dwSSPIOutFlags,			/* pfContextAttr */
		&tsExpiry);					/* ptsExpiry */

	if (scRet != SEC_I_CONTINUE_NEEDED)
	{
		AgentCommonTools::CWin32LastError error(scRet);
		LOG_DEBUG(L"InitializeSecurityContext result unexpected: %s (0x%08x)", error.GetMessage(), error.GetCode());
		cbData = SOCKET_ERROR;
	}
	else
		cbData = SendBuffer(OutBuffers[0].pvBuffer, OutBuffers[0].cbBuffer);

	if (SUCCEEDED(scRet))
	{
		m_pSecurityFuncs->FreeContextBuffer(OutBuffers[0].pvBuffer);
		OutBuffers[0].pvBuffer = NULL;
	}

	if (cbData == SOCKET_ERROR)
	{
		scRet = SEC_E_INTERNAL_ERROR;
	}
	else
	{
		scRet = ClientHandshakeLoop(TRUE);
	}

	return !!SUCCEEDED(scRet);
}

SECURITY_STATUS CSslSocket::ClientHandshakeLoop(BOOL fDoInitialRead)
{
	SecBufferDesc   InBuffer       = { };
	SecBuffer       InBuffers[2]   = { };
	SecBufferDesc   OutBuffer      = { };
	SecBuffer       OutBuffers[1]  = { };
	DWORD           dwSSPIFlags    = 0;
	DWORD           dwSSPIOutFlags = 0;
	TimeStamp       tsExpiry       = { };
	SECURITY_STATUS scRet          = 0;
	DWORD           cbData         = 0;
	PUCHAR          IoBuffer       = NULL;
	DWORD           cbIoBuffer     = 0;
	BOOL            fDoRead        = FALSE;

	dwSSPIFlags =
		ISC_REQ_SEQUENCE_DETECT   |
		ISC_REQ_REPLAY_DETECT     |
		ISC_REQ_CONFIDENTIALITY   |
		ISC_RET_EXTENDED_ERROR    |
		ISC_REQ_ALLOCATE_MEMORY   |
		ISC_REQ_STREAM			  |
		ISC_REQ_MANUAL_CRED_VALIDATION;

	IoBuffer = (PUCHAR) calloc(IO_BUFFER_SIZE, 1);
	if (IoBuffer == NULL)
	{
		LOG_DEBUG(L"Failed to allocate %d bytes of memory", IO_BUFFER_SIZE);
		return SEC_E_INTERNAL_ERROR;
	}

	cbIoBuffer = 0;
	fDoRead = fDoInitialRead;
	scRet = SEC_I_CONTINUE_NEEDED;

	while (scRet == SEC_I_CONTINUE_NEEDED        ||
		   scRet == SEC_E_INCOMPLETE_MESSAGE     ||
		   scRet == SEC_I_INCOMPLETE_CREDENTIALS)
	{
		if (0 == cbIoBuffer || scRet == SEC_E_INCOMPLETE_MESSAGE)
		{
			if (fDoRead)
			{
				cbData = ::recv(m_Socket, (char*) (IoBuffer + cbIoBuffer), IO_BUFFER_SIZE - cbIoBuffer, 0);
				if (cbData == SOCKET_ERROR)
				{
					AgentCommonTools::CWin32LastError error(::WSAGetLastError());
					LOG_DEBUG(L"recv failed: %s (%d)", error.GetMessage(), error.GetCode());
					scRet = SEC_E_INTERNAL_ERROR;
					break;
				}
				else if (cbData == 0)
				{
					LOG_DEBUG(L"recv failed: connection closed");
					scRet = SEC_E_INTERNAL_ERROR;
					break;
				}

				cbIoBuffer += cbData;
			}
			else
			{
				fDoRead = TRUE;
			}
		}

		InBuffers[0].pvBuffer   = IoBuffer;
		InBuffers[0].cbBuffer   = cbIoBuffer;
		InBuffers[0].BufferType = SECBUFFER_TOKEN;

		InBuffers[1].pvBuffer   = NULL;
		InBuffers[1].cbBuffer   = 0;
		InBuffers[1].BufferType = SECBUFFER_EMPTY;

		InBuffer.cBuffers       = 2;
		InBuffer.pBuffers       = InBuffers;
		InBuffer.ulVersion      = SECBUFFER_VERSION;

		OutBuffers[0].pvBuffer  = NULL;
		OutBuffers[0].BufferType= SECBUFFER_TOKEN;
		OutBuffers[0].cbBuffer  = 0;

		OutBuffer.cBuffers      = 1;
		OutBuffer.pBuffers      = OutBuffers;
		OutBuffer.ulVersion     = SECBUFFER_VERSION;

		scRet = m_pSecurityFuncs->InitializeSecurityContext(
			&m_hCreds,			/* phCredential */
			&m_hContext,		/* phContext */
			NULL,				/* pszTargetName */
			dwSSPIFlags,		/* fContextReq */
			0,					/* Reserved1 */
			0,					/* TargetDataRep */
			&InBuffer,			/* pInput */
			0,					/* Reserved2 */
			NULL,				/* phNewContext */
			&OutBuffer,			/* pOutput */
			&dwSSPIOutFlags,	/* pfContextAttr */
			&tsExpiry);			/* ptsExpiry */

		if (scRet == SEC_E_OK ||
			scRet == SEC_I_CONTINUE_NEEDED ||
			FAILED(scRet) && (dwSSPIOutFlags & ISC_RET_EXTENDED_ERROR))
		{
			cbData = SendBuffer(OutBuffers[0].pvBuffer, OutBuffers[0].cbBuffer);
		}
		if (SUCCEEDED(scRet))
		{
			m_pSecurityFuncs->FreeContextBuffer(OutBuffers[0].pvBuffer);
			OutBuffers[0].pvBuffer = NULL;
		}
		if (cbData == SOCKET_ERROR)
		{
			scRet = SEC_E_INTERNAL_ERROR;
			break;
		}
		if (scRet == SEC_E_INCOMPLETE_MESSAGE)
			continue;

		if (scRet == SEC_E_OK)
			break;

		if (FAILED(scRet) || scRet == SEC_I_INCOMPLETE_CREDENTIALS)
		{
			AgentCommonTools::CWin32LastError error(scRet);
			LOG_DEBUG(L"InitializeSecurityContext failed: %s (0x%08x)", error.GetMessage(), error.GetCode());
			break;
		}

		if (InBuffers[1].BufferType == SECBUFFER_EXTRA)
		{
			::MoveMemory(IoBuffer, IoBuffer + (cbIoBuffer - InBuffers[1].cbBuffer), InBuffers[1].cbBuffer);
			cbIoBuffer = InBuffers[1].cbBuffer;
		}
		else
		{
			cbIoBuffer = 0;
		}
	}

	if (IoBuffer != NULL)
	{
		free(IoBuffer);
	}

	return scRet;
}

bool CSslSocket::IsNull(PCredHandle pHandle)
{
	return pHandle == NULL || (pHandle->dwLower == NULL && pHandle->dwUpper == NULL);
}

void CSslSocket::ParseConnectionDetails(const void* pConnInfo, CString& text)
{
	auto ci = (SecPkgContext_ConnectionInfo*) pConnInfo;

	text.Empty();
	if ((ci->dwProtocol & SP_PROT_SSL2_CLIENT) || (ci->dwProtocol & SP_PROT_SSL2_SERVER))
	{
		if (!text.IsEmpty())
			text.Append(L"-");
		text.Append(L"SSL2");
	}
	if ((ci->dwProtocol & SP_PROT_SSL3_CLIENT) || (ci->dwProtocol & SP_PROT_SSL3_SERVER))
	{
		if (!text.IsEmpty())
			text.Append(L"-");
		text.Append(L"SSL3");
	}
	if ((ci->dwProtocol & SP_PROT_TLS1_CLIENT) || (ci->dwProtocol & SP_PROT_TLS1_SERVER))
	{
		if (!text.IsEmpty())
			text.Append(L"-");
		text.Append(L"TLS1");
	}
	if ((ci->dwProtocol & SP_PROT_TLS1_1_CLIENT) || (ci->dwProtocol & SP_PROT_TLS1_1_SERVER))
	{
		if (!text.IsEmpty())
			text.Append(L"-");
		text.Append(L"TLS11");
	}
	if ((ci->dwProtocol & SP_PROT_TLS1_2_CLIENT) || (ci->dwProtocol & SP_PROT_TLS1_2_SERVER))
	{
		if (!text.IsEmpty())
			text.Append(L"-");
		text.Append(L"TLS12");
	}

	if (!text.IsEmpty())
		text.Append(L"-");
	switch (ci->aiExch)
	{
		case 0x0000A400: // CALG_RSA_KEYX
			text.AppendFormat(L"RSA_%d", ci->dwExchStrength);
			break;
		case 0x0000AA02: // CALG_DH_EPHEM
			text.AppendFormat(L"DHE_%d", ci->dwExchStrength);
			break;
		case 0x0000AE06: // CALG_ECDH_EPHEM
			text.AppendFormat(L"ECDHE_%d", ci->dwExchStrength);
			break;
		case 0x00002203: // CALG_ECDSA
			text.AppendFormat(L"ECDSA_%d", ci->dwExchStrength);
			break;
		default:
			text.AppendFormat(L"K_%08X_%d", ci->aiExch, ci->dwExchStrength);
			break;
	}

	if (!text.IsEmpty())
		text.Append(L"-");
	switch (ci->aiCipher)
	{
		case 0:
			text.Append(L"E_NULL");
			break;
		case CALG_3DES:
			text.AppendFormat(L"3DES_%d", ci->dwCipherStrength);
			break;
		case CALG_AES:
		case CALG_AES_128:
		case CALG_AES_192:
		case CALG_AES_256:
			text.AppendFormat(L"AES_%d", ci->dwCipherStrength);
			break;
		case CALG_DES:
			text.AppendFormat(L"DES_%d", ci->dwCipherStrength);
			break;
		case CALG_RC2:
			text.AppendFormat(L"RC2_%d", ci->dwCipherStrength);
			break;
		case CALG_RC4:
			text.AppendFormat(L"RC4_%d", ci->dwCipherStrength);
			break;
		default:
			text.AppendFormat(L"E_%08X_%d", ci->aiCipher, ci->dwCipherStrength);
			break;
	}

	if (!text.IsEmpty())
		text.Append(L"-");
	switch (ci->aiHash)
	{
		case CALG_MD5:
			text.Append(L"MD5");
			break;
		case CALG_SHA1:
			text.Append(L"SHA1");
			break;
		case CALG_SHA_256:
		case CALG_SHA_384:
		case CALG_SHA_512:
			text.AppendFormat(L"SHA%d", ci->dwHashStrength);
			break;
		default:
			text.AppendFormat(L"D_%08X", ci->aiHash);
			break;
	}
}