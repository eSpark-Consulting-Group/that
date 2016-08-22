#ifndef _C_INSTALLREPORTER_H_
#define _C_INSTALLREPORTER_H_

#include <msi.h>

class DllExport CInstallReporter
{
    friend int _stdcall Custom_UIHandler(LPVOID pvContext, UINT iMessageType, LPCWSTR szMessage);
public:
    struct IInstallationListener
    {
        virtual void OnInstallProgress(int percentage) = 0;
        virtual void OnInstallMessage(LPCWSTR message) = 0;
        virtual void OnInstallTerminate() = 0;
    };

    CInstallReporter(bool preserveInternalUILevel = false);
    ~CInstallReporter();

    void init(IInstallationListener* pListener);
private:
    IInstallationListener* m_pListener;
    int Custom_UIHandler(UINT iMessageType, LPCWSTR szMessage);
    int FGetInteger(wchar_t*& rpch);
    BOOL ParseCommonDataString(wchar_t* sz);
    BOOL ParseProgressString(wchar_t* sz);


    int m_oldPercentage;
    int g_rgiField[3];
    WORD g_wLANGID;
    int iField[4];
    int  g_iProgressTotal;
    int  g_iProgress;
    int  iCurPos;
    BOOL bFirstTime;
    BOOL g_bForwardProgress;
    BOOL g_bScriptInProgress;
    BOOL g_bEnableActionData;
    BOOL g_bCancelInstall;
};

#endif