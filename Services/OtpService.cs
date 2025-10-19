using Microsoft.Extensions.Caching.Memory;

namespace SavePlus_API.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly Random _random;

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
            _random = new Random();
        }

        public async Task<string> GenerateOtpAsync(string phoneNumber, string purpose = "login")
        {
            // Generate 6-digit OTP
            var otpCode = _random.Next(100000, 999999).ToString();
            var cacheKey = GetCacheKey(phoneNumber, purpose);
            
            // Cache OTP for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, otpCode, cacheOptions);
            
            _logger.LogInformation("OTP generated for {PhoneNumber} with purpose {Purpose}", phoneNumber, purpose);
            
            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode, string purpose = "login")
        {
            var cacheKey = GetCacheKey(phoneNumber, purpose);
            
            if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                if (storedOtp == otpCode)
                {
                    // Remove OTP after successful validation
                    _cache.Remove(cacheKey);
                    _logger.LogInformation("OTP validated successfully for {PhoneNumber}", phoneNumber);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Invalid OTP provided for {PhoneNumber}", phoneNumber);
                    return false;
                }
            }
            
            _logger.LogWarning("OTP not found or expired for {PhoneNumber}", phoneNumber);
            return false;
        }

        public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode)
        {
            try
            {
                // TODO: Implement real SMS service (Twilio, AWS SNS, etc.)
                // For now, we'll just log the OTP (for development)
                
                _logger.LogInformation("📱 SMS OTP: {OtpCode} sent to {PhoneNumber}", otpCode, phoneNumber);
                
                // Simulate SMS sending delay
                await Task.Delay(100);
                
                // For development, always return true
                // In production, return actual SMS service result
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS OTP to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task InvalidateOtpAsync(string phoneNumber, string purpose = "login")
        {
            var cacheKey = GetCacheKey(phoneNumber, purpose);
            _cache.Remove(cacheKey);
            _logger.LogInformation("OTP invalidated for {PhoneNumber} with purpose {Purpose}", phoneNumber, purpose);
        }

        private string GetCacheKey(string phoneNumber, string purpose)
        {
            return $"otp:{phoneNumber}:{purpose}";
        }
    }
}
