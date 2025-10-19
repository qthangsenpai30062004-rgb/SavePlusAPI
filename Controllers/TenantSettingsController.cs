using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId}")]
    public class TenantSettingsController : ControllerBase
    {
        private readonly ITenantSettingService _settingService;
        private readonly ILogger<TenantSettingsController> _logger;

        public TenantSettingsController(
            ITenantSettingService settingService,
            ILogger<TenantSettingsController> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all settings for a tenant
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<ApiResponse<List<TenantSettingDto>>>> GetTenantSettings(
            int tenantId,
            [FromQuery] string? category = null)
        {
            try
            {
                var settings = await _settingService.GetTenantSettingsAsync(tenantId, category);
                return Ok(ApiResponse<List<TenantSettingDto>>.SuccessResult(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<List<TenantSettingDto>>.ErrorResult("Lỗi khi tải cài đặt"));
            }
        }

        /// <summary>
        /// Get booking configuration for a tenant (client-friendly format)
        /// </summary>
        [HttpGet("booking-config")]
        public async Task<ActionResult<ApiResponse<BookingConfigDto>>> GetBookingConfig(int tenantId)
        {
            try
            {
                var config = await _settingService.GetBookingConfigAsync(tenantId);
                return Ok(ApiResponse<BookingConfigDto>.SuccessResult(config, "Lấy cấu hình đặt lịch thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking config for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<BookingConfigDto>.ErrorResult("Lỗi khi tải cấu hình đặt lịch"));
            }
        }

        /// <summary>
        /// Get payment configuration for a tenant
        /// </summary>
        [HttpGet("payment-config")]
        public async Task<ActionResult<ApiResponse<PaymentConfigDto>>> GetPaymentConfig(int tenantId)
        {
            try
            {
                var config = await _settingService.GetPaymentConfigAsync(tenantId);
                return Ok(ApiResponse<PaymentConfigDto>.SuccessResult(config, "Lấy cấu hình thanh toán thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment config for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<PaymentConfigDto>.ErrorResult("Lỗi khi tải cấu hình thanh toán"));
            }
        }

        /// <summary>
        /// Update a single setting
        /// </summary>
        [HttpPut("settings/{settingKey}")]
        public async Task<ActionResult<ApiResponse<TenantSettingDto>>> UpdateSetting(
            int tenantId,
            string settingKey,
            [FromBody] TenantSettingUpdateDto dto)
        {
            try
            {
                if (dto.SettingKey != settingKey)
                {
                    return BadRequest(ApiResponse<TenantSettingDto>.ErrorResult("Setting key không khớp"));
                }

                var setting = await _settingService.UpdateSettingAsync(tenantId, settingKey, dto.SettingValue);
                return Ok(ApiResponse<TenantSettingDto>.SuccessResult(setting, "Cập nhật cài đặt thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TenantSettingDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {Key} for tenant {TenantId}", settingKey, tenantId);
                return StatusCode(500, ApiResponse<TenantSettingDto>.ErrorResult("Lỗi khi cập nhật cài đặt"));
            }
        }

        /// <summary>
        /// Bulk update multiple settings
        /// </summary>
        [HttpPut("settings")]
        public async Task<ActionResult<ApiResponse<List<TenantSettingDto>>>> UpdateSettings(
            int tenantId,
            [FromBody] TenantSettingsBulkUpdateDto dto)
        {
            try
            {
                var settings = await _settingService.UpdateSettingsBulkAsync(tenantId, dto.Settings);
                return Ok(ApiResponse<List<TenantSettingDto>>.SuccessResult(settings, "Cập nhật cài đặt thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating settings for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<List<TenantSettingDto>>.ErrorResult("Lỗi khi cập nhật cài đặt"));
            }
        }

        /// <summary>
        /// Initialize default settings for a tenant
        /// </summary>
        [HttpPost("settings/initialize")]
        public async Task<ActionResult<ApiResponse<string>>> InitializeSettings(int tenantId)
        {
            try
            {
                await _settingService.InitializeDefaultSettingsAsync(tenantId);
                return Ok(ApiResponse<string>.SuccessResult("OK", "Khởi tạo cài đặt mặc định thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing settings for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi khi khởi tạo cài đặt"));
            }
        }
    }
}
