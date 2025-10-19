using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IConsultationService
    {
        /// <summary>
        /// Tạo consultation mới
        /// </summary>
        Task<ApiResponse<ConsultationDto>> CreateConsultationAsync(ConsultationCreateDto dto);

        /// <summary>
        /// Lấy consultation theo ID
        /// </summary>
        Task<ApiResponse<ConsultationDto>> GetConsultationByIdAsync(int consultationId);

        /// <summary>
        /// Cập nhật consultation
        /// </summary>
        Task<ApiResponse<ConsultationDto>> UpdateConsultationAsync(int consultationId, ConsultationUpdateDto dto);

        /// <summary>
        /// Xóa consultation
        /// </summary>
        Task<ApiResponse<bool>> DeleteConsultationAsync(int consultationId);

        /// <summary>
        /// Lấy danh sách consultation với filter và phân trang
        /// </summary>
        Task<ApiResponse<PagedResult<ConsultationDto>>> GetConsultationsAsync(ConsultationFilterDto filter);

        /// <summary>
        /// Lấy consultation theo appointment ID
        /// </summary>
        Task<ApiResponse<ConsultationDto>> GetConsultationByAppointmentIdAsync(int appointmentId);

        /// <summary>
        /// Lấy danh sách consultation của bệnh nhân
        /// </summary>
        Task<ApiResponse<List<ConsultationDto>>> GetPatientConsultationsAsync(int patientId, int? tenantId = null);

        /// <summary>
        /// Lấy danh sách consultation của bác sĩ
        /// </summary>
        Task<ApiResponse<List<ConsultationDto>>> GetDoctorConsultationsAsync(int doctorId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Lấy danh sách consultation của phòng khám
        /// </summary>
        Task<ApiResponse<List<ConsultationDto>>> GetTenantConsultationsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Lấy báo cáo consultation
        /// </summary>
        Task<ApiResponse<ConsultationReportDto>> GetConsultationReportAsync(int? tenantId = null, int? doctorId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Tìm kiếm consultation theo từ khóa
        /// </summary>
        Task<ApiResponse<List<ConsultationDto>>> SearchConsultationsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10);
    }
}
