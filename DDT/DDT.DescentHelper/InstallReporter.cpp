#include "stdafx.h"
#include "InstallReporter.h"

CInstallReporter::CInstallReporter(bool preserveInternalUILevel) : m_pListener(NULL)
{
    m_oldPercentage = 0;
    g_wLANGID = LANG_NEUTRAL; // initialize to neutral language
    g_iProgressTotal = 0; // total ticks on progress bar
    g_iProgress = 0;      // amount of progress
    iCurPos = 0;
    bFirstTime = preserveInternalUILevel ? FALSE : TRUE;
    g_bForwardProgress = TRUE; //TRUE if the progress bar control should be incremented in a forward direction
    g_bScriptInProgress = FALSE;
    g_bEnableActionData; //TRUE if INSTALLOGMODE_ACTIONDATA messages are sending progress information
    g_bCancelInstall = FALSE; //Should be set to TRUE if the user clicks Cancel button.
}

CInstallReporter::~CInstallReporter()
{
    ::MsiSetExternalUI(NULL, NULL, NULL);
}


//
//  FUNCTION: FGetInteger(char*& pch)
//
//  PURPOSE:  Converts the string (from current pos. to next whitespace or '\0')
//            to an integer.
//
//  COMMENTS: Assumes correct syntax.  Ptr is updated to new position at whitespace
//            or null terminator.
//
int CInstallReporter::FGetInteger(wchar_t*& rpch)
{
    wchar_t* pchPrev = rpch; 
    while (*rpch && *rpch != ' ')
        rpch++;
    *rpch = '\0';
    int i = _wtoi(pchPrev);
    return i;
}



//
//  FUNCTION: ParseCommonDataString(wchar_t* sz)
//
//  PURPOSE:  Parses the common data message sent to the INSTALLUI_HANDLER callback
//
//  COMMENTS: Ignores the 3rd field and the caption common data message. Assumes correct syntax.
//
BOOL CInstallReporter::ParseCommonDataString(wchar_t* sz)
{
    wchar_t *pch = sz;
    if (0 == *pch)
        return FALSE; // no msg

    while (*pch != 0)
    {
        wchar_t chField = *pch++;
        pch++; // for ':'
        pch++; // for sp
        switch (chField)
        {
        case L'1': // field 1
            {
                // common data message type
                g_rgiField[0] = *pch++ - '0';
                if (g_rgiField[0] == 1)
                    return FALSE; // we are ignoring caption messages
                break;
            }
        case '2': // field 2
            {
                // because we are ignoring caption msg, these are all ints
                g_rgiField[1] = FGetInteger(pch);
                return TRUE; // done processing
            }
        default: // unknown field
            {
                return FALSE;
            }
        }
        pch++; // for space (' ') between fields
    }

    return TRUE;
}

//
//  FUNCTION: ParseProgressString(wchar_t* sz)
//
//  PURPOSE:  Parses the progress data message sent to the INSTALLUI_HANDLER callback
//
//  COMMENTS: Assumes correct syntax.
//
BOOL CInstallReporter::ParseProgressString(wchar_t* sz)
{
    wchar_t *pch = sz;
    if (0 == *pch)
        return FALSE; // no msg

    while (*pch != 0)
    {
        wchar_t chField = *pch++;
        pch++; // for ':'
        pch++; // for sp
        switch (chField)
        {
        case '1': // field 1
            {
                // progress message type
                if (0 == isdigit(*pch))
                    return FALSE; // blank record
                iField[0] = *pch++ - '0';
                break;
            }
        case '2': // field 2
            {
                iField[1] = FGetInteger(pch);
                if (iField[0] == 2 || iField[0] == 3)
                    return TRUE; // done processing
                break;
            }
        case '3': // field 3
            {
                iField[2] = FGetInteger(pch);
                if (iField[0] == 1)
                    return TRUE; // done processing
                break;
            }
        case '4': // field 4
            {
                iField[3] = FGetInteger(pch);
                return TRUE; // done processing
            }
        default: // unknown field
            {
                return FALSE;
            }
        }
        pch++; // for space (' ') between fields
    }

    return TRUE;
}





int CInstallReporter::Custom_UIHandler(UINT iMessageType, LPCWSTR szMessage)
{
//	DO_LOG(L"Custom_UIHandler (%d) (%s)", iMessageType, szMessage ? szMessage : L"");

    // File costing is skipped when applying Patch(es) and INSTALLUILEVEL is NONE.
    // Workaround: Set INSTALLUILEVEL to anything but NONE only once.
    if (bFirstTime == TRUE)
    {
		// However, this can break LaunchConditions which depend on INSTALLUILEVEL_NONE,
		// so use parameter in CInstallReporter::ctor() where appropriate.
        UINT r1 = MsiSetInternalUI(INSTALLUILEVEL_BASIC, NULL);
        bFirstTime = FALSE;
    }

    if (!szMessage)
        return 0;

    INSTALLMESSAGE mt;
    UINT uiFlags;

    mt = (INSTALLMESSAGE)(0xFF000000 & (UINT)iMessageType);
    uiFlags = 0x00FFFFFF & iMessageType;

    switch (mt)
    {
    case INSTALLMESSAGE_INFO:
        return IDOK;
        break;

    case INSTALLMESSAGE_FATALEXIT:
    case INSTALLMESSAGE_ERROR:
    case INSTALLMESSAGE_WARNING:
    case INSTALLMESSAGE_USER:
        m_pListener->OnInstallMessage(szMessage);
        return IDOK;

    case INSTALLMESSAGE_FILESINUSE:
        /* Display FilesInUse dialog */
        // parse the message text to provide the names of the 
        // applications that the user can close so that the 
        // files are no longer in use.
        return 0;

    case INSTALLMESSAGE_RESOLVESOURCE:
        /* ALWAYS return 0 for ResolveSource */
        return 0;

    case INSTALLMESSAGE_OUTOFDISKSPACE:
        /* Get user message here */
        return IDOK;

    case INSTALLMESSAGE_ACTIONSTART:
        /* New action started, any action data is sent by this new action */
        g_bEnableActionData = FALSE;
        return IDOK;

    case INSTALLMESSAGE_ACTIONDATA:
        // only act if progress total has been initialized
        if (0 == g_iProgressTotal)
            return IDOK;
        if(g_bEnableActionData)
        {
        }
        return IDOK;

    case INSTALLMESSAGE_PROGRESS:
        {
            if(ParseProgressString(const_cast<wchar_t*>(szMessage)))
            {
                // all fields off by 1 due to c array notation
                switch(iField[0])
                {
                case 0: // Reset progress bar
                    {
                        //field 1 = 0, field 2 = total number of ticks, field 3 = direction, field 4 = in progress

                        /* get total number of ticks in progress bar */
                        g_iProgressTotal = iField[1];

                        /* determine direction */
                        if (iField[2] == 0)
                            g_bForwardProgress = TRUE;
                        else // iField[2] == 1
                            g_bForwardProgress = FALSE;

                        /* get current position of progress bar, depends on direction */
                        // if Forward direction, current position is 0
                        // if Backward direction, current position is Total # ticks
                        g_iProgress = g_bForwardProgress ? 0 : g_iProgressTotal;

                        // if g_bScriptInProgress, finish progress bar, else reset (and set up according to direction)

                        iCurPos = 0;

                        /* determine new state */
                        // if new state = 1 (script in progress), could send a "Please wait..." msg
                        // new state = 1 means the total # of progress ticks is an estimate, and may not add up correctly
                        g_bScriptInProgress = (iField[3] == 1) ? TRUE : FALSE;

                        m_pListener->OnInstallProgress(0);

                        break;
                    }
                case 1:  // ActionInfo
                    {
                        //field 1 = 1, field 2 will contain the number of ticks to increment the bar
                        //ignore if field 3 is zero
                        if(iField[2])
                        {
                            // movement direction determined by g_bForwardProgress set by reset progress msg
                            g_bEnableActionData = TRUE;
                        }
                        else
                        {
                            g_bEnableActionData = FALSE;
                        }

                        break;
                    }
                case 2: //ProgressReport
                    {
                        // only act if progress total has been initialized
                        if (0 == g_iProgressTotal)
                            break;

                        iCurPos += iField[1];

                        //field 1 = 2,field 2 will contain the number of ticks the bar has moved
                        // movement direction determined by g_bForwardProgress set by reset progress msg

                        LONGLONG percentage = iCurPos;
                        percentage *= 100;
                        percentage /= g_iProgressTotal;

                        if(percentage > m_oldPercentage)
                        {
                            if(percentage > 100)
                                percentage = 100;

                            m_pListener->OnInstallProgress((int)percentage);
                            m_oldPercentage = (int)percentage;
                        }

                        break;
                    }
                case 3: // ProgressAddition - fall through (we don't care to handle it -- total tick count adjustment)
                default:
                    {
                        break;
                    }
                }
            }

            if(g_bCancelInstall == TRUE)
            {
                return IDCANCEL;
            }
            else
                return IDOK;
        }


    case INSTALLMESSAGE_COMMONDATA:
        {
            if (ParseCommonDataString(const_cast<wchar_t*>(szMessage)))
            {
                // all fields off by 1 due to c array notation
                switch (g_rgiField[0])
                {
                case 0:
                    // field 1 = 0, field 2 = LANGID, field 3 = CodePage
                    g_wLANGID = g_rgiField[1];
                    break;
                case 1:
                    // field 1 = 1, field 2 = CAPTION
                    /* you could use this as the caption for MessageBoxes */
                    break;
                case 2:
                    // field 1 = 2, field 2 = 0 (hide cancel button) OR 1 (show cancel button)
                    break;
                default:
                    break;
                }
            }
            return IDOK;
        }

        // this message is received prior to internal UI initialization, no string data
    case INSTALLMESSAGE_INITIALIZE:
        return IDOK;

        // Sent after UI termination, no string data
    case INSTALLMESSAGE_TERMINATE:
        m_pListener->OnInstallTerminate();
        return IDOK;

        //Sent prior to display of authored dialog or wizard
    case INSTALLMESSAGE_SHOWDIALOG:
        return IDOK;

    default:
        return 0;
    }
}



int _stdcall Custom_UIHandler(LPVOID pvContext, UINT iMessageType, LPCWSTR szMessage)
{
    return ((CInstallReporter*)pvContext)->Custom_UIHandler(iMessageType, szMessage);
}


void CInstallReporter::init(IInstallationListener* pListener)
{
    m_pListener = pListener;

    ::MsiSetInternalUI(INSTALLUILEVEL(INSTALLUILEVEL_NONE|INSTALLUILEVEL_SOURCERESONLY), NULL);
    ::MsiSetExternalUI (::Custom_UIHandler,
        INSTALLLOGMODE_PROGRESS|INSTALLLOGMODE_FATALEXIT|INSTALLLOGMODE_ERROR
        |INSTALLLOGMODE_WARNING|INSTALLLOGMODE_USER|INSTALLLOGMODE_INFO
        |INSTALLLOGMODE_RESOLVESOURCE|INSTALLLOGMODE_OUTOFDISKSPACE
        |INSTALLLOGMODE_ACTIONSTART|INSTALLLOGMODE_ACTIONDATA
        |INSTALLLOGMODE_COMMONDATA|INSTALLLOGMODE_PROGRESS|INSTALLLOGMODE_INITIALIZE
        |INSTALLLOGMODE_TERMINATE|INSTALLLOGMODE_SHOWDIALOG,
        this);
}
