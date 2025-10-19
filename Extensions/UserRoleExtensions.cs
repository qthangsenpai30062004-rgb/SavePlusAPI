using SavePlus_API.Constants;

namespace SavePlus_API.Extensions
{
    /// <summary>
    /// Extension methods cho User Roles
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Kiểm tra xem role có phải là staff không
        /// </summary>
        public static bool IsStaffRole(this string role)
        {
            return UserRoles.StaffRoles.Contains(role);
        }

        /// <summary>
        /// Kiểm tra xem role có quyền xem dữ liệu y tế không
        /// </summary>
        public static bool CanAccessMedicalData(this string role)
        {
            return UserRoles.MedicalDataRoles.Contains(role);
        }

        /// <summary>
        /// Kiểm tra xem role có quyền kê đơn thuốc không
        /// </summary>
        public static bool CanPrescribe(this string role)
        {
            return UserRoles.PrescriptionRoles.Contains(role);
        }

        /// <summary>
        /// Kiểm tra xem role có quyền quản lý lịch hẹn không
        /// </summary>
        public static bool CanManageAppointments(this string role)
        {
            return UserRoles.AppointmentManagementRoles.Contains(role);
        }

        /// <summary>
        /// Kiểm tra xem role có phải admin cấp hệ thống không
        /// </summary>
        public static bool IsSystemAdmin(this string role)
        {
            return role == UserRoles.SystemAdmin;
        }

        /// <summary>
        /// Kiểm tra xem role có phải admin cấp phòng khám không
        /// </summary>
        public static bool IsClinicAdmin(this string role)
        {
            return role == UserRoles.ClinicAdmin;
        }

        /// <summary>
        /// Kiểm tra xem role có quyền admin (system hoặc clinic) không
        /// </summary>
        public static bool IsAnyAdmin(this string role)
        {
            return role.IsSystemAdmin() || role.IsClinicAdmin();
        }

        /// <summary>
        /// Lấy mô tả role
        /// </summary>
        public static string GetRoleDescription(this string role)
        {
            return role switch
            {
                UserRoles.SystemAdmin => "Quản trị viên hệ thống",
                UserRoles.ClinicAdmin => "Quản trị viên phòng khám",
                UserRoles.Doctor => "Bác sĩ",
                UserRoles.Nurse => "Điều dưỡng",
                UserRoles.Receptionist => "Lễ tân",
                UserRoles.Patient => "Bệnh nhân",
                UserRoles.CareCoordinator => "Điều phối chăm sóc",
                UserRoles.Scheduler => "Chuyên viên lịch hẹn",
                UserRoles.Billing => "Kế toán",
                UserRoles.Caregiver => "Người chăm sóc",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Kiểm tra permission level giữa 2 roles
        /// </summary>
        public static bool HasHigherOrEqualPermission(this string currentRole, string targetRole)
        {
            // SystemAdmin có quyền cao nhất
            if (currentRole.IsSystemAdmin())
                return true;

            // ClinicAdmin có quyền trong tenant
            if (currentRole.IsClinicAdmin() && !targetRole.IsSystemAdmin())
                return true;

            // Cùng level
            if (currentRole == targetRole)
                return true;

            return false;
        }
    }
}
