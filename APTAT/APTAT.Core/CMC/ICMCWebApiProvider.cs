using System;

namespace APTAT.Core.CMC
{
    public interface ICmcWebApiProvider
    {
        Uri GetBaseUri();
        void SetBaseUri(Uri uri);
        void SendData(IGeneralInfo request);
    }
}
