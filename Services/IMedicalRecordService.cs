using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IMedicalRecordService
    {
        /// <summary>
        /// Tạo medical record mới
        /// </summary>
        Task<ApiResponse<MedicalRecordDto>> CreateMedicalRecordAsync(MedicalRecordCreateDto dto);

        /// <summary>
        /// Lấy medical record theo ID
        /// </summary>
        Task<ApiResponse<MedicalRecordDto>> GetMedicalRecordByIdAsync(long recordId);

        /// <summary>
        /// Cập nhật medical record
        /// </summary>
        Task<ApiResponse<MedicalRecordDto>> UpdateMedicalRecordAsync(long recordId, MedicalRecordUpdateDto dto);

        /// <summary>
        /// Xóa medical record
        /// </summary>
        Task<ApiResponse<bool>> DeleteMedicalRecordAsync(long recordId);

        /// <summary>
        /// Lấy danh sách medical record với filter và phân trang
        /// </summary>
        Task<ApiResponse<PagedResult<MedicalRecordDto>>> GetMedicalRecordsAsync(MedicalRecordFilterDto filter);

        /// <summary>
        /// Lấy danh sách medical record của bệnh nhân
        /// </summary>
        Task<ApiResponse<List<MedicalRecordDto>>> GetPatientMedicalRecordsAsync(int patientId, int? tenantId = null);

        /// <summary>
        /// Lấy danh sách medical record của phòng khám
        /// </summary>
        Task<ApiResponse<List<MedicalRecordDto>>> GetTenantMedicalRecordsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Lấy danh sách medical record theo loại
        /// </summary>
        Task<ApiResponse<List<MedicalRecordDto>>> GetMedicalRecordsByTypeAsync(string recordType, int? tenantId = null, int? patientId = null);

        /// <summary>
        /// Lấy báo cáo medical record
        /// </summary>
        Task<ApiResponse<MedicalRecordReportDto>> GetMedicalRecordReportAsync(int? tenantId = null, int? patientId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Tìm kiếm medical record theo từ khóa
        /// </summary>
        Task<ApiResponse<List<MedicalRecordDto>>> SearchMedicalRecordsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Upload file và tạo medical record
        /// </summary>
        Task<ApiResponse<MedicalRecordDto>> UploadMedicalRecordAsync(MedicalRecordUploadDto dto);

        /// <summary>
        /// Tải file medical record
        /// </summary>
        Task<ApiResponse<byte[]>> DownloadMedicalRecordFileAsync(long recordId);

        /// <summary>
        /// Lấy thống kê medical record của bệnh nhân
        /// </summary>
        Task<ApiResponse<PatientMedicalRecordSummaryDto>> GetPatientMedicalRecordSummaryAsync(int patientId, int? tenantId = null);

        /// <summary>
        /// Lấy danh sách loại medical record được sử dụng
        /// </summary>
        Task<ApiResponse<List<string>>> GetUsedRecordTypesAsync(int? tenantId = null);

        /// <summary>
        /// Kiểm tra quyền truy cập medical record
        /// </summary>
        Task<ApiResponse<bool>> CheckMedicalRecordAccessAsync(long recordId, int userId);

        /// <summary>
        /// Lấy medical record gần đây nhất của bệnh nhân
        /// </summary>
        Task<ApiResponse<MedicalRecordDto>> GetLatestPatientMedicalRecordAsync(int patientId, int? tenantId = null, string? recordType = null);
    }
}
