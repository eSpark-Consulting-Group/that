using System;
using System.Net;

namespace APTAT.Core.CMC
{
    public interface ICmcWebApiProvider
    {
        Uri GetBaseUri();
        void SetBaseUri(Uri uri);
        void SendData(IGeneralInfo request, NetworkCredential networkCredential);
    }
}
