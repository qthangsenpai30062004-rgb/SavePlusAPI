using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IAppointmentService
    {
        // Quản lý cuộc hẹn
        Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(AppointmentCreateDto dto);
        Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(int appointmentId);
        Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(int appointmentId, AppointmentUpdateDto dto);
        Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, string reason = "");
        
        // Tìm kiếm và lọc cuộc hẹn
        Task<ApiResponse<PagedResult<AppointmentDto>>> GetAppointmentsAsync(AppointmentFilterDto filter);
        Task<ApiResponse<List<AppointmentDto>>> GetPatientAppointmentsAsync(int patientId, int? tenantId = null);
        Task<ApiResponse<List<AppointmentDto>>> GetDoctorAppointmentsAsync(int doctorId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<AppointmentDto>>> GetTenantAppointmentsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Kiểm tra availability
        Task<ApiResponse<bool>> CheckDoctorAvailabilityAsync(int doctorId, DateTime startTime, DateTime endTime);
        Task<ApiResponse<List<DateTime>>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date, int durationMinutes = 30);
        
        // Xác nhận và cập nhật trạng thái
        Task<ApiResponse<AppointmentDto>> ConfirmAppointmentAsync(int appointmentId);
        Task<ApiResponse<AppointmentDto>> StartAppointmentAsync(int appointmentId);
        Task<ApiResponse<AppointmentDto>> CompleteAppointmentAsync(int appointmentId, string? notes = null);
        
        // Lấy lịch hẹn hôm nay
        Task<ApiResponse<List<AppointmentDto>>> GetTodayAppointmentsAsync(int? tenantId = null);
    }
}
