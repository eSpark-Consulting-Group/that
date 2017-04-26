#pragma once

enum DescentStatus
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
    status_agent_is_already_installed_and_server_differs = 13,
    status_agent_is_not_installed = 14,

    status_installer_unknown_product = 15,
    status_installer_fatal_error = 16,
    status_installer_product_already_installed = 17,
    status_installer_install_already_running = 18,

	status_uninstall_incompatible_products_failure = 19,
	status_uninstall_incompatible_products_not_supported = 20,
	status_uninstall_incompatible_products_partially_not_supported = 21,

    status_host_is_not_accessible_from_remote_pc = 22,
    status_no_compatible_msi_found = 23,

    status_bad_admin_netpath = 24,
    status_success_agent_updated = 25,

	status_unsupported_extension = 26,

	status_install_in_progress = 27,
	status_cannot_create_process = 28,
	status_prepairing_for_create_process = 29,
	status_installing_timeout = 30,
	status_msi_error = 31,
	status_exe_error = 32,
	status_before_msi_started = 33,

};
