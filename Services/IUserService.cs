using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IUserService
    {
        // Quản lý users
        Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateDto dto);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
        Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
        Task<ApiResponse<UserDto>> GetUserByPhoneAsync(string phoneNumber);
        Task<ApiResponse<UserDto>> UpdateUserAsync(int userId, UserUpdateDto dto);
        Task<ApiResponse<bool>> DeactivateUserAsync(int userId);
        Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(int? tenantId = null, int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        // Authentication
        Task<ApiResponse<AuthResponseDto>> AuthenticateAsync(StaffLoginDto dto);
        Task<ApiResponse<bool>> ValidatePasswordAsync(int userId, string password);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        
        // Password Reset
        Task<ApiResponse<bool>> ResetPasswordAsync(int userId, string newPassword);
        Task<ApiResponse<bool>> VerifyPasswordAsync(int userId, string password);

        // User với Doctor info
        Task<ApiResponse<UserWithDoctorDto>> GetUserWithDoctorInfoAsync(int userId);
        Task<ApiResponse<List<UserDto>>> GetUsersByTenantAsync(int tenantId, string? role = null);
        
        // Utility
        Task<ApiResponse<bool>> EmailExistsAsync(string email, int? excludeUserId = null);
        Task<ApiResponse<bool>> PhoneExistsAsync(string phoneNumber, int? excludeUserId = null);
        
        // Doctor management
        Task<ApiResponse<object>> CreateDoctorRecordAsync(int userId, string? specialty, string? licenseNumber);
        
        // Search doctors trong tenant cụ thể cho autocomplete
        Task<ApiResponse<List<DoctorSearchDto>>> SearchDoctorsInTenantAsync(int tenantId, string searchTerm, int limit = 10);
    }
}
