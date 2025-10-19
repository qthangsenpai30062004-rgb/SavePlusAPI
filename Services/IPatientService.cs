using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public interface IPatientService
    {
        // Quản lý bệnh nhân
        Task<ApiResponse<PatientDto>> RegisterPatientAsync(PatientRegistrationDto dto);
        Task<ApiResponse<PatientDto>> GetPatientByIdAsync(int patientId);
        Task<ApiResponse<PatientDto>> GetPatientByPhoneAsync(string phoneNumber);
        Task<ApiResponse<PatientDto>> UpdatePatientAsync(int patientId, PatientUpdateDto dto);
        Task<ApiResponse<PagedResult<PatientDto>>> GetPatientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        // Quản lý bệnh nhân trong phòng khám (ClinicPatient)
        Task<ApiResponse<ClinicPatientDto>> EnrollPatientToClinicAsync(int tenantId, int patientId, string? mrn = null, int? primaryDoctorId = null);
        Task<ApiResponse<PagedResult<ClinicPatientDto>>> GetClinicPatientsAsync(int tenantId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<ApiResponse<ClinicPatientDto>> GetClinicPatientAsync(int tenantId, int patientId);
        Task<ApiResponse<ClinicPatientDto>> UpdateClinicPatientAsync(int tenantId, int patientId, string? mrn = null, int? primaryDoctorId = null, byte? status = null);

        // Authentication
        Task<ApiResponse<AuthResponseDto>> AuthenticatePatientAsync(string phoneNumber, string verificationCode);
        
        // Tìm kiếm bệnh nhân theo số điện thoại (cho multi-tenant)
        Task<ApiResponse<List<ClinicPatientDto>>> FindPatientInClinicsAsync(string phoneNumber);
        
        // Search bệnh nhân trong tenant cụ thể cho autocomplete
        Task<ApiResponse<List<PatientSearchDto>>> SearchPatientsInTenantAsync(int tenantId, string searchTerm, int limit = 10);
    }
}
