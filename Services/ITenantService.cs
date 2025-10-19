using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface ITenantService
    {
        Task<ApiResponse<TenantDto>> CreateTenantAsync(TenantCreateDto dto);
        Task<ApiResponse<TenantDto>> GetTenantByIdAsync(int tenantId);
        Task<ApiResponse<TenantDto>> GetTenantByCodeAsync(string code);
        Task<ApiResponse<TenantDto>> UpdateTenantAsync(int tenantId, TenantUpdateDto dto);
        Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<ApiResponse<TenantStatsDto>> GetTenantStatsAsync(int tenantId);
        Task<ApiResponse<PagedResult<DoctorDto>>> GetTenantDoctorsAsync(int tenantId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<ApiResponse<DoctorDto>> GetTenantDoctorAsync(int tenantId, int doctorId);
        Task<ApiResponse<DoctorDto>> UpdateTenantDoctorAsync(int tenantId, int doctorId, UpdateDoctorDto dto);
    }
}
