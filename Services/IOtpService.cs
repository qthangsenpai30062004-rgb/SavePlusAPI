namespace SavePlus_API.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string phoneNumber, string purpose = "login");
        Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode, string purpose = "login");
        Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode);
        Task InvalidateOtpAsync(string phoneNumber, string purpose = "login");
    }
}
