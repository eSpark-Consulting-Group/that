#pragma once

#include "BaseTypes.h"
#include <Winsock2.h>
#include <wincrypt.h>
#include <wintrust.h>
#include <schannel.h>

#define SECURITY_WIN32
#include <security.h>
#include <sspi.h>

#define IO_BUFFER_SIZE  0x10000

#include "IpEndpoint.h"

class DllExport CSslSocket
{
private:
	volatile SOCKET m_Socket;
	CAutoCriticalSection m_cs;
	CString m_serverName;

	SecurityFunctionTable *m_pSecurityFuncs;
	bool m_bSecureConnection;
	
	CredHandle m_hCreds;
	CtxtHandle m_hContext;

	SecPkgContext_StreamSizes m_BufferSizes;
	ULONG m_uBufferLength;
	PVOID m_pRecvBuffer;
	PBYTE m_pSendBuffer;
	
	int m_cbData;

private:
	bool ClientHandshake(LPCWSTR szServerName = NULL);
	SECURITY_STATUS ClientHandshakeLoop(BOOL fDoInitialRead);

    bool ConnectIPv4(IN_ADDR IPv4Address, UINT uPort);
    bool ConnectIPv6(IN6_ADDR IPv6Address, UINT uPort);
	int SendBuffer(const void* buffer, uint32_t bufSize);
	int RecvBuffer(void* buffer, uint32_t bufSize);

	static void ParseConnectionDetails(const void* pConnInfo, CString& text);
	static bool IsNull(PCredHandle pHandle);

public:
	CSslSocket();
	~CSslSocket();

	bool SetKeepAlive(ULONG time, ULONG interval);
	bool SetLinger(BOOL enabled, USHORT timeout);
	bool SecureConnect(CRM::AgnCore::Transport::CIPEndPoint& endPoint);
    bool Connect(CRM::AgnCore::Transport::CIPEndPoint& endPoint);
	bool GetLocalEndpoint(CRM::AgnCore::Transport::CIPEndPoint& endpoint);

	int Send(const void* lpBuf, UINT nBufLen, int nFlags);
	int Receive(void* lpBuf, UINT nBufLen, int nFlags);
	bool Listen(int nNumber);
	CSslSocket* Accept();
	void Close();

	SECURITY_STATUS Encrypt(/*[in]*/ byte* pbMessage,/*[in]*/ UINT nBufLen,/*[out]*/ PBYTE& pSendBuffer,/*[out]*/ UINT &nSendBuffer);
	SECURITY_STATUS Decrypt(/*[in]*/ PVOID pbMessage,/*[in]*/ UINT nBufLen,/*[out]*/ SecBufferDesc& Message,/*[out]*/ SecBuffer* Buffers, UINT nBuffersCount = 4);

	UINT SplitData(/*[in]*/SecBufferDesc& Message,/*[in]*/ SecBuffer* Buffers,/*[in]*/ ULONG BufferType,/*[out]*/ PVOID &lpBuffer, UINT nBuffer);

	bool IsClosed() const { return m_Socket == NULL; }
};