using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public interface ITenantSettingService
    {
        Task<string?> GetSettingValueAsync(int tenantId, string settingKey);
        Task<T?> GetSettingValueAsync<T>(int tenantId, string settingKey);
        Task<List<TenantSettingDto>> GetTenantSettingsAsync(int tenantId, string? category = null);
        Task<BookingConfigDto> GetBookingConfigAsync(int tenantId);
        Task<PaymentConfigDto> GetPaymentConfigAsync(int tenantId);
        Task<TenantSettingDto> UpdateSettingAsync(int tenantId, string settingKey, string settingValue);
        Task<List<TenantSettingDto>> UpdateSettingsBulkAsync(int tenantId, Dictionary<string, string> settings);
        Task InitializeDefaultSettingsAsync(int tenantId);
    }

    public class TenantSettingService : ITenantSettingService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<TenantSettingService> _logger;

        // Default settings to initialize for new tenants
        private readonly Dictionary<string, (string Value, string Type, string Category, string Description)> _defaultSettings = new()
        {
            ["Booking.MaxAdvanceBookingDays"] = ("90", "Integer", "Booking", "Số ngày tối đa có thể đặt lịch trước"),
            ["Booking.DefaultSlotDurationMinutes"] = ("30", "Integer", "Booking", "Thời lượng mặc định của mỗi khung giờ khám (phút)"),
            ["Booking.MinAdvanceBookingHours"] = ("1", "Integer", "Booking", "Số giờ tối thiểu phải đặt lịch trước"),
            ["Booking.MaxCancellationHours"] = ("24", "Integer", "Booking", "Số giờ tối đa có thể hủy lịch trước khi khám"),
            ["Booking.AllowWeekendBooking"] = ("true", "Boolean", "Booking", "Cho phép đặt lịch vào cuối tuần"),
            ["Payment.BankTransferEnabled"] = ("true", "Boolean", "Payment", "Cho phép thanh toán qua chuyển khoản ngân hàng"),
            ["Payment.EWalletEnabled"] = ("false", "Boolean", "Payment", "Cho phép thanh toán qua ví điện tử")
        };

        public TenantSettingService(SavePlusDbContext context, ILogger<TenantSettingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetSettingValueAsync(int tenantId, string settingKey)
        {
            var setting = await _context.TenantSettings
                .Where(s => s.TenantId == tenantId && s.SettingKey == settingKey)
                .FirstOrDefaultAsync();

            if (setting == null && _defaultSettings.ContainsKey(settingKey))
            {
                // Return default value if setting doesn't exist
                return _defaultSettings[settingKey].Value;
            }

            return setting?.SettingValue;
        }

        public async Task<T?> GetSettingValueAsync<T>(int tenantId, string settingKey)
        {
            var value = await GetSettingValueAsync(tenantId, settingKey);
            if (value == null) return default;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                _logger.LogWarning("Failed to convert setting {Key} value '{Value}' to type {Type}", 
                    settingKey, value, typeof(T).Name);
                return default;
            }
        }

        public async Task<List<TenantSettingDto>> GetTenantSettingsAsync(int tenantId, string? category = null)
        {
            var query = _context.TenantSettings.Where(s => s.TenantId == tenantId);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.Category == category);
            }

            var settings = await query
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SettingKey)
                .Select(s => new TenantSettingDto
                {
                    TenantSettingId = s.TenantSettingId,
                    TenantId = s.TenantId,
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue,
                    SettingType = s.SettingType,
                    Category = s.Category,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return settings;
        }

        public async Task<BookingConfigDto> GetBookingConfigAsync(int tenantId)
        {
            var maxDaysValue = await GetSettingValueAsync(tenantId, "Booking.MaxAdvanceBookingDays");
            var maxDays = maxDaysValue != null ? int.Parse(maxDaysValue) : 90;
            
            var slotDurationValue = await GetSettingValueAsync(tenantId, "Booking.DefaultSlotDurationMinutes");
            var slotDuration = slotDurationValue != null ? int.Parse(slotDurationValue) : 30;
            
            var minHoursValue = await GetSettingValueAsync(tenantId, "Booking.MinAdvanceBookingHours");
            var minHours = minHoursValue != null ? int.Parse(minHoursValue) : 1;
            
            var maxCancelHoursValue = await GetSettingValueAsync(tenantId, "Booking.MaxCancellationHours");
            var maxCancelHours = maxCancelHoursValue != null ? int.Parse(maxCancelHoursValue) : 24;
            
            var allowWeekendValue = await GetSettingValueAsync(tenantId, "Booking.AllowWeekendBooking");
            var allowWeekend = allowWeekendValue != null ? bool.Parse(allowWeekendValue) : true;

            return new BookingConfigDto
            {
                MaxAdvanceBookingDays = maxDays,
                DefaultSlotDurationMinutes = slotDuration,
                MinAdvanceBookingHours = minHours,
                MaxCancellationHours = maxCancelHours,
                AllowWeekendBooking = allowWeekend
            };
        }

        public async Task<PaymentConfigDto> GetPaymentConfigAsync(int tenantId)
        {
            var bankTransfer = await GetSettingValueAsync<bool>(tenantId, "Payment.BankTransferEnabled");
            var eWallet = await GetSettingValueAsync<bool>(tenantId, "Payment.EWalletEnabled");

            return new PaymentConfigDto
            {
                BankTransferEnabled = bankTransfer,
                EWalletEnabled = eWallet,
                CashEnabled = true // Always enabled
            };
        }

        public async Task<TenantSettingDto> UpdateSettingAsync(int tenantId, string settingKey, string settingValue)
        {
            var setting = await _context.TenantSettings
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SettingKey == settingKey);

            if (setting == null)
            {
                // Create new setting if it doesn't exist
                if (!_defaultSettings.ContainsKey(settingKey))
                {
                    throw new KeyNotFoundException($"Unknown setting key: {settingKey}");
                }

                var defaultSetting = _defaultSettings[settingKey];
                setting = new TenantSetting
                {
                    TenantId = tenantId,
                    SettingKey = settingKey,
                    SettingValue = settingValue,
                    SettingType = defaultSetting.Type,
                    Category = defaultSetting.Category,
                    Description = defaultSetting.Description
                };
                _context.TenantSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = settingValue;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new TenantSettingDto
            {
                TenantSettingId = setting.TenantSettingId,
                TenantId = setting.TenantId,
                SettingKey = setting.SettingKey,
                SettingValue = setting.SettingValue,
                SettingType = setting.SettingType,
                Category = setting.Category,
                Description = setting.Description,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
        }

        public async Task<List<TenantSettingDto>> UpdateSettingsBulkAsync(int tenantId, Dictionary<string, string> settings)
        {
            var results = new List<TenantSettingDto>();

            foreach (var kvp in settings)
            {
                var result = await UpdateSettingAsync(tenantId, kvp.Key, kvp.Value);
                results.Add(result);
            }

            return results;
        }

        public async Task InitializeDefaultSettingsAsync(int tenantId)
        {
            foreach (var defaultSetting in _defaultSettings)
            {
                var exists = await _context.TenantSettings
                    .AnyAsync(s => s.TenantId == tenantId && s.SettingKey == defaultSetting.Key);

                if (!exists)
                {
                    _context.TenantSettings.Add(new TenantSetting
                    {
                        TenantId = tenantId,
                        SettingKey = defaultSetting.Key,
                        SettingValue = defaultSetting.Value.Value,
                        SettingType = defaultSetting.Value.Type,
                        Category = defaultSetting.Value.Category,
                        Description = defaultSetting.Value.Description
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
