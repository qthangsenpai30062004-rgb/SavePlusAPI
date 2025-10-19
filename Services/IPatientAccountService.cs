using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public interface IPatientAccountService
    {
        /// <summary>
        /// Đăng ký tài khoản Patient mới (tạo cả Patient và PatientAccount)
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> RegisterAsync(PatientAccountRegisterDto dto);

        /// <summary>
        /// Đăng nhập bằng Email/Password
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> LoginByEmailAsync(PatientAccountLoginDto dto);

        /// <summary>
        /// Đăng nhập bằng Phone/Password
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> LoginByPhoneAsync(PatientAccountLoginByPhoneDto dto);

        /// <summary>
        /// Lấy thông tin PatientAccount theo AccountId
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> GetByAccountIdAsync(int accountId);

        /// <summary>
        /// Lấy thông tin PatientAccount theo PatientId
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> GetByPatientIdAsync(int patientId);

        /// <summary>
        /// Lấy thông tin PatientAccount theo Email
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> GetByEmailAsync(string email);

        /// <summary>
        /// Lấy thông tin PatientAccount theo Phone
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> GetByPhoneAsync(string phoneE164);

        /// <summary>
        /// Cập nhật thông tin PatientAccount
        /// </summary>
        Task<ApiResponse<PatientAccountResponseDto>> UpdateAsync(int accountId, PatientAccountUpdateDto dto);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        Task<ApiResponse<object>> ChangePasswordAsync(int accountId, PatientAccountChangePasswordDto dto);

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        Task<ApiResponse<bool>> VerifyPasswordAsync(int accountId, string password);

        /// <summary>
        /// Reset mật khẩu (dùng khi forgot password)
        /// </summary>
        Task<ApiResponse<object>> ResetPasswordAsync(int accountId, string newPassword);

        /// <summary>
        /// Request OTP để verify email
        /// </summary>
        Task<ApiResponse<object>> RequestEmailVerificationAsync(int accountId);

        /// <summary>
        /// Xác thực email bằng OTP
        /// </summary>
        Task<ApiResponse<object>> VerifyEmailAsync(int accountId, string otpCode);

        /// <summary>
        /// Request OTP để verify phone
        /// </summary>
        Task<ApiResponse<object>> RequestPhoneVerificationAsync(int accountId);

        /// <summary>
        /// Xác thực phone bằng OTP
        /// </summary>
        Task<ApiResponse<object>> VerifyPhoneAsync(int accountId, string otpCode);

        /// <summary>
        /// Cập nhật LastLoginAt
        /// </summary>
        Task<ApiResponse<object>> UpdateLastLoginAsync(int accountId);

        /// <summary>
        /// Vô hiệu hóa tài khoản
        /// </summary>
        Task<ApiResponse<object>> DeactivateAccountAsync(int accountId);

        /// <summary>
        /// Kích hoạt tài khoản
        /// </summary>
        Task<ApiResponse<object>> ActivateAccountAsync(int accountId);

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        Task<bool> IsEmailExistsAsync(string email);

        /// <summary>
        /// Kiểm tra phone đã tồn tại chưa
        /// </summary>
        Task<bool> IsPhoneExistsAsync(string phoneE164);
    }
}


