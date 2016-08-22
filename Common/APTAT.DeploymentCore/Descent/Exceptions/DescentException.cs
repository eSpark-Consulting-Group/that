using System;
using System.ComponentModel;
using APTAT.DeploymentCore.Descent.Helpers.Transport;

namespace APTAT.DeploymentCore.Descent.Exceptions
{
    namespace Transport
    {
		/// <summary>
		/// IMPORTANT! For those who is going to add/update enum's elements!
		/// The enumeration is a copy of DescentStatus from DescentAgn/DescentStatus.h file.
		/// Low elements should be the same as in DescentAgn's file (0-999) with the same numbers.
		/// Server-defined elements that don't have corresponding elements in DescentAgn's file 
		/// should have numver from 1000 and higher.
		/// </summary>
        public enum DescentStatus
        {
			status_error = 0,
			status_success = 1,
			status_cannot_resolve_remote_hostname = 2,

			status_login_problem = 3,
			status_login_invalid_username_or_bad_password = 4,
			status_login_invalid_username = 5,
			status_login_trusted_relationship_failure = 6,

			status_remote_cannot_copy_file = 7,
			status_remote_cannot_setup_descent_service = 8,
			status_remote_cannot_cleanup_descent_service = 9,
			status_remote_cannot_run_descent_service = 10,
			status_remote_invalid_response = 11,

			status_agent_is_already_installed = 12,
			status_agent_is_not_installed = 14,

			status_installer_unknown_product = 15,
			status_installer_fatal_error = 16,
			status_installer_product_already_installed = 17,
			status_installer_install_already_running = 18,
            

			status_bad_admin_netpath = 24,
            status_unsupported_extension = 26,
            status_install_in_progress = 27,
            status_cannot_create_process = 28,
            status_prepairing_for_create_process = 29,
            status_installing_timeout = 30,
            status_msi_error = 31,
            status_exe_error = 32,
            status_before_msi_started = 33,

            status_agent_timeout = 1002,

            status_invalid_command_line = 1003,
            status_operation_canceled = 1004,
            status_error_product_version = 1005,
            status_success_reboot_required = 1006,
            status_agent_was_crashed = 1007
        }

        public class DescentException : Exception
        {
            public DescentStatus _status;
            public int _win32error;
            public string _errorString;

            private const int ERROR_LOGON_FAILURE = 1326;
            private const int ERROR_NO_SUCH_LOGON_SESSION = 1312;
            private const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;
            private const int ERROR_NO_LOGON_SERVERS = 1311;
            private const int ERROR_ACCOUNT_DISABLED = 1331;
            private const int ERROR_INVALID_COMMAND_LINE = 1639;
            private const int ERROR_BAD_USERNAME = 2202;
            private const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;
            private const int ERROR_BAD_NETPATH = 53;
            private const int ERROR_NO_NET_OR_BAD_PATH = 1203;
            private const int ERROR_ACCESS_DENIED = 5;
            private const int ERROR_WRONG_TARGET_NAME = 1396;
            private const int ERROR_SUCCESS_REBOOT_REQUIRED = 3010;

            public DescentException(DescentHelper.RemoteResponse response)
            {
                _status = response.Status;
                _win32error = response.Win32Error;
                _errorString = response.ErrorString;
                switch (_win32error)
                {
                    case ERROR_INVALID_COMMAND_LINE:
                        _status = DescentStatus.status_invalid_command_line;
                        break;
                    case ERROR_SUCCESS_REBOOT_REQUIRED:
                        _status = DescentStatus.status_success_reboot_required;
                        break;
                }
            }

            public DescentException(DescentStatus status, string errorString, int win32error)
            {
                _status = status;
                _win32error = win32error;
                _errorString = errorString;
            }

            public DescentException(Win32Exception ex)
            {
                _win32error = ex.NativeErrorCode;
                _errorString = ex.Message;
                _status = DescentStatus.status_error;

                switch (_win32error)
                {
                    case ERROR_LOGON_FAILURE:
                    case ERROR_NO_SUCH_LOGON_SESSION:
                    case ERROR_ACCOUNT_DISABLED: // can occur on vista x64 with wrong login/password
                        _status = DescentStatus.status_login_invalid_username_or_bad_password;
                        break;
                    case ERROR_BAD_USERNAME:
                        _status = DescentStatus.status_login_invalid_username;
                        break;
                    case ERROR_TRUSTED_RELATIONSHIP_FAILURE:
                        _status = DescentStatus.status_login_trusted_relationship_failure;
                        break;
                    case ERROR_BAD_NETPATH:
                    case ERROR_NO_NET_OR_BAD_PATH:
                        _status = DescentStatus.status_bad_admin_netpath;
                        break;
                    

                    // -----   COMMON ERRORS PACK   -----
                    case ERROR_ACCESS_DENIED:
                    case ERROR_SESSION_CREDENTIAL_CONFLICT:
                    // Multiple connections to a server or shared resource by the same user, 
                    // using more than one user name, are not allowed. 
                    // Disconnect all previous connections to the server or shared resource and try again.
                    case ERROR_WRONG_TARGET_NAME:
                    // May occur when IP of target computer name in etc/hosts is incorrect (in m_adminPath)
                        _status = DescentStatus.status_login_problem;
                        break;
                }

            }
        }
    }
}