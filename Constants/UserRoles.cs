namespace SavePlus_API.Constants
{
    /// <summary>
    /// Định nghĩa các role trong hệ thống Neosix
    /// </summary>
    public static class UserRoles
    {
        // Phase 1 - Core Roles
        /// <summary>
        /// Quản trị viên toàn hệ thống - quản lý tenant, hạ tầng, cấu hình global
        /// </summary>
        public const string SystemAdmin = "SystemAdmin";

        /// <summary>
        /// Quản trị viên phòng khám - toàn quyền trong phạm vi tenant
        /// </summary>
        public const string ClinicAdmin = "ClinicAdmin";

        /// <summary>
        /// Bác sĩ - tạo hồ sơ, chẩn đoán, kê đơn, tư vấn
        /// </summary>
        public const string Doctor = "Doctor";

        /// <summary>
        /// Điều dưỡng - theo dõi CarePlan, nhập measurement, gửi reminder
        /// </summary>
        public const string Nurse = "Nurse";

        /// <summary>
        /// Lễ tân - quản lý lịch hẹn, check-in, thu phí cơ bản
        /// </summary>
        public const string Receptionist = "Receptionist";

        /// <summary>
        /// Bệnh nhân - sử dụng app mobile
        /// </summary>
        public const string Patient = "Patient";

        // Phase 2 - Advanced Roles (Future)
        /// <summary>
        /// Điều phối chăm sóc - chuyên về quản lý quy trình chăm sóc
        /// </summary>
        public const string CareCoordinator = "CareCoordinator";

        /// <summary>
        /// Chuyên viên lịch hẹn - quản lý lịch phức tạp
        /// </summary>
        public const string Scheduler = "Scheduler";

        /// <summary>
        /// Kế toán - quản lý tài chính, hóa đơn
        /// </summary>
        public const string Billing = "Billing";

        /// <summary>
        /// Người chăm sóc - được bệnh nhân ủy quyền
        /// </summary>
        public const string Caregiver = "Caregiver";

        /// <summary>
        /// Danh sách tất cả roles Phase 1 (đang sử dụng)
        /// </summary>
        public static readonly string[] CoreRoles = 
        {
            SystemAdmin,
            ClinicAdmin, 
            Doctor,
            Nurse,
            Receptionist,
            Patient
        };

        /// <summary>
        /// Danh sách tất cả roles Phase 2 (future)
        /// </summary>
        public static readonly string[] AdvancedRoles = 
        {
            CareCoordinator,
            Scheduler,
            Billing,
            Caregiver
        };

        /// <summary>
        /// Tất cả roles có thể có trong hệ thống
        /// </summary>
        public static readonly string[] AllRoles = CoreRoles.Concat(AdvancedRoles).ToArray();

        /// <summary>
        /// Roles dành cho staff (không phải patient)
        /// </summary>
        public static readonly string[] StaffRoles = 
        {
            SystemAdmin,
            ClinicAdmin,
            Doctor,
            Nurse,
            Receptionist,
            CareCoordinator,
            Scheduler,
            Billing
        };

        /// <summary>
        /// Roles có quyền xem dữ liệu y tế
        /// </summary>
        public static readonly string[] MedicalDataRoles = 
        {
            SystemAdmin, // với ủy quyền
            ClinicAdmin,
            Doctor,
            Nurse,
            CareCoordinator
        };

        /// <summary>
        /// Roles có quyền tạo/sửa prescription
        /// </summary>
        public static readonly string[] PrescriptionRoles = 
        {
            Doctor // Chỉ bác sĩ mới được kê đơn
        };

        /// <summary>
        /// Roles có quyền quản lý lịch hẹn
        /// </summary>
        public static readonly string[] AppointmentManagementRoles = 
        {
            ClinicAdmin,
            Doctor,
            Nurse,
            Receptionist,
            Scheduler
        };
    }
}
