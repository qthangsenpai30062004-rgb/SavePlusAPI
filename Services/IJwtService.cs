using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IJwtService
    {
        string GeneratePatientToken(PatientDto patient);
        string GeneratePatientToken(UserInfoDto userInfo); // Overload for UserInfoDto
        string GenerateUserToken(UserInfoDto user);
        Task<bool> ValidateTokenAsync(string token);
        string? GetUserIdFromToken(string token);
        string? GetPatientIdFromToken(string token);
    }
}
