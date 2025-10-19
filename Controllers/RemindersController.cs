using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using SavePlus_API.Attributes;
using SavePlus_API.Constants;
using System.Security.Claims;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger<RemindersController> _logger;

        public RemindersController(IReminderService reminderService, ILogger<RemindersController> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo nhắc nhở mới
        /// </summary>
        [HttpPost]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> CreateReminder([FromBody] CreateReminderDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.CreateReminderAsync(tenantId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhắc nhở với phân trang và filter
        /// </summary>
        [HttpGet]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ReminderListResponseDTO>>> GetReminders([FromQuery] ReminderQueryDTO query)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.GetRemindersAsync(tenantId, query);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderListResponseDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhắc nhở
        /// </summary>
        [HttpGet("{reminderId}")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> GetReminder(int reminderId)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.GetReminderByIdAsync(tenantId, reminderId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Cập nhật nhắc nhở
        /// </summary>
        [HttpPut("{reminderId}")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> UpdateReminder(int reminderId, [FromBody] UpdateReminderDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.UpdateReminderAsync(tenantId, reminderId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Xóa nhắc nhở
        /// </summary>
        [HttpDelete("{reminderId}")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReminder(int reminderId)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.DeleteReminderAsync(tenantId, reminderId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhắc nhở");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Hoãn nhắc nhở
        /// </summary>
        [HttpPost("{reminderId}/snooze")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> SnoozeReminder(int reminderId, [FromBody] SnoozeReminderDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.SnoozeReminderAsync(tenantId, reminderId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoãn nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Kích hoạt nhắc nhở
        /// </summary>
        [HttpPost("{reminderId}/activate")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateReminder(int reminderId)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.ActivateReminderAsync(tenantId, reminderId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt nhắc nhở");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Tắt nhắc nhở
        /// </summary>
        [HttpPost("{reminderId}/deactivate")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateReminder(int reminderId)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Kiểm tra quyền truy cập
                var isAccessible = await _reminderService.IsReminderAccessibleAsync(tenantId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                var result = await _reminderService.DeactivateReminderAsync(tenantId, reminderId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tắt nhắc nhở");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Thực hiện hành động hàng loạt trên nhiều nhắc nhở
        /// </summary>
        [HttpPost("bulk-action")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<int>>> BulkReminderAction([FromBody] BulkReminderActionDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.BulkReminderActionAsync(tenantId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện hành động hàng loạt");
                return StatusCode(500, ApiResponse<int>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhắc nhở sắp tới
        /// </summary>
        [HttpGet("upcoming")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<List<ReminderDTO>>>> GetUpcomingReminders([FromQuery] int hours = 24)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.GetUpcomingRemindersAsync(tenantId, hours);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở sắp tới");
                return StatusCode(500, ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhắc nhở quá hạn
        /// </summary>
        [HttpGet("overdue")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<List<ReminderDTO>>>> GetOverdueReminders()
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.GetOverdueRemindersAsync(tenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở quá hạn");
                return StatusCode(500, ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy thống kê nhắc nhở
        /// </summary>
        [HttpGet("stats")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ReminderStatsDTO>>> GetReminderStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.GetReminderStatsAsync(tenantId, fromDate, toDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderStatsDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhắc nhở đến hạn (cho background service)
        /// </summary>
        [HttpGet("due")]
        [RequireRole(UserRoles.SystemAdmin, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<List<FireReminderDTO>>>> GetDueReminders()
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.GetDueRemindersAsync(tenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở đến hạn");
                return StatusCode(500, ApiResponse<List<FireReminderDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Đánh dấu nhắc nhở đã được kích hoạt (cho background service)
        /// </summary>
        [HttpPost("{reminderId}/mark-fired")]
        [RequireRole(UserRoles.SystemAdmin, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<bool>>> MarkReminderAsFired(int reminderId)
        {
            try
            {
                var result = await _reminderService.MarkReminderAsFiredAsync(reminderId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu nhắc nhở đã kích hoạt");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy các template nhắc nhở có sẵn
        /// </summary>
        [HttpGet("templates")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<List<ReminderTemplateDTO>>>> GetReminderTemplates()
        {
            try
            {
                var result = await _reminderService.GetReminderTemplatesAsync();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách template nhắc nhở");
                return StatusCode(500, ApiResponse<List<ReminderTemplateDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Tạo nhắc nhở từ template
        /// </summary>
        [HttpPost("from-template")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse)]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> CreateReminderFromTemplate(
            [FromQuery] string targetType,
            [FromQuery] int targetId,
            [FromQuery] int patientId,
            [FromQuery] int minutesBefore = 30)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _reminderService.CreateReminderFromTemplateAsync(tenantId, targetType, targetId, patientId, minutesBefore);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nhắc nhở từ template");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        #region Patient APIs

        /// <summary>
        /// Lấy danh sách nhắc nhở của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<ReminderDTO>>>> GetPatientReminders(
            int patientId,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                // For patient access, we might want to check if the requesting user is the patient
                // For now, we'll use tenantId = 0 to bypass tenant check in service
                var result = await _reminderService.GetPatientRemindersAsync(0, patientId, activeOnly);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở của bệnh nhân");
                return StatusCode(500, ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Bệnh nhân hoãn nhắc nhở của mình
        /// </summary>
        [HttpPost("patient/{patientId}/reminders/{reminderId}/snooze")]
        public async Task<ActionResult<ApiResponse<ReminderDTO>>> PatientSnoozeReminder(
            int patientId,
            int reminderId,
            [FromBody] SnoozeReminderDTO dto)
        {
            try
            {
                // Check if patient has access to this reminder
                var isAccessible = await _reminderService.IsPatientReminderAccessibleAsync(patientId, reminderId);
                if (!isAccessible)
                {
                    return Forbid();
                }

                // Get tenant from reminder
                var reminderResult = await _reminderService.GetReminderByIdAsync(0, reminderId);
                if (!reminderResult.Success)
                {
                    return BadRequest(reminderResult.Message);
                }

                var result = await _reminderService.SnoozeReminderAsync(reminderResult.Data!.TenantId, reminderId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bệnh nhân hoãn nhắc nhở");
                return StatusCode(500, ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        #endregion

        #region Helper Methods

        private int GetTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            return int.Parse(tenantIdClaim ?? "0");
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        #endregion
    }
}
