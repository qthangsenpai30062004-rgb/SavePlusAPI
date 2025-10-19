using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<DoctorEditDto>> GetDoctorByUserIdAsync(int userId);
        Task<ApiResponse<DoctorEditDto>> GetDoctorByIdAsync(int doctorId);
        Task<ApiResponse<DoctorEditDto>> UpdateDoctorSelfAsync(int userId, DoctorSelfUpdateDto dto);
        Task<ApiResponse<DoctorEditDto>> UpdateDoctorByAdminAsync(int doctorId, DoctorAdminUpdateDto dto);
    }
}
