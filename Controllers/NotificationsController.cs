using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo thông báo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> CreateNotification([FromBody] NotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _notificationService.CreateNotificationAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetNotification), new { id = result.Data?.NotificationId }, result);
        }

        /// <summary>
        /// Lấy thông tin thông báo theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> GetNotification(long id)
        {
            var result = await _notificationService.GetNotificationByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin thông báo
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> UpdateNotification(long id, [FromBody] NotificationUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _notificationService.UpdateNotificationAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(long id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách thông báo (có lọc và phân trang)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] string? channel = null,
            [FromQuery] bool? isRead = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new NotificationFilterDto
            {
                TenantId = tenantId,
                UserId = userId,
                PatientId = patientId,
                Channel = channel,
                IsRead = isRead,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _notificationService.GetNotificationsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách thông báo của user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetUserNotifications(int userId, [FromQuery] int? tenantId = null, [FromQuery] bool? unreadOnly = null)
        {
            var result = await _notificationService.GetUserNotificationsAsync(userId, tenantId, unreadOnly);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách thông báo của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetPatientNotifications(int patientId, [FromQuery] int? tenantId = null, [FromQuery] bool? unreadOnly = null)
        {
            var result = await _notificationService.GetPatientNotificationsAsync(patientId, tenantId, unreadOnly);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách thông báo của phòng khám
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetTenantNotifications(
            int tenantId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _notificationService.GetTenantNotificationsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        [HttpPost("{id}/mark-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(long id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Đánh dấu nhiều thông báo đã đọc
        /// </summary>
        [HttpPost("mark-multiple-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkMultipleAsRead([FromBody] MarkAsReadDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _notificationService.MarkMultipleAsReadAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead(
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? tenantId = null)
        {
            var result = await _notificationService.MarkAllAsReadAsync(userId, patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Gửi thông báo hàng loạt
        /// </summary>
        [HttpPost("bulk-send")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> SendBulkNotification([FromBody] BulkNotificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<List<NotificationDto>>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _notificationService.SendBulkNotificationAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy báo cáo thông báo
        /// </summary>
        [HttpGet("reports")]
        public async Task<ActionResult<ApiResponse<NotificationReportDto>>> GetNotificationReport(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _notificationService.GetNotificationReportAsync(tenantId, userId, patientId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? tenantId = null)
        {
            var result = await _notificationService.GetUnreadCountAsync(userId, patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm thông báo theo từ khóa
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> SearchNotifications(
            [FromQuery] string keyword,
            [FromQuery] int? tenantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest(ApiResponse<List<NotificationDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
            }

            var result = await _notificationService.SearchNotificationsAsync(keyword, tenantId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê thông báo của user/patient
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<UserNotificationSummaryDto>>> GetUserNotificationSummary(
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? tenantId = null)
        {
            var result = await _notificationService.GetUserNotificationSummaryAsync(userId, patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Gửi thông báo từ template
        /// </summary>
        [HttpPost("send-from-template")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> SendNotificationFromTemplate(
            [FromBody] NotificationTemplateDto template,
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int tenantId = 1)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _notificationService.SendNotificationFromTemplateAsync(template, userId, patientId, tenantId);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách thông báo theo kênh
        /// </summary>
        [HttpGet("channel/{channel}")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotificationsByChannel(
            string channel,
            [FromQuery] int? tenantId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _notificationService.GetNotificationsByChannelAsync(channel, tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Xóa thông báo cũ (cleanup)
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<ActionResult<ApiResponse<int>>> CleanupOldNotifications(
            [FromQuery] int daysOld = 30,
            [FromQuery] int? tenantId = null)
        {
            var result = await _notificationService.CleanupOldNotificationsAsync(daysOld, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách kênh thông báo được sử dụng
        /// </summary>
        [HttpGet("channels")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetUsedChannels([FromQuery] int? tenantId = null)
        {
            var result = await _notificationService.GetUsedChannelsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Gửi nhắc nhở cuộc hẹn
        /// </summary>
        [HttpPost("appointment-reminder/{appointmentId}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> SendAppointmentReminder(
            int appointmentId,
            [FromQuery] int hoursBeforeAppointment = 24)
        {
            var result = await _notificationService.SendAppointmentReminderAsync(appointmentId, hoursBeforeAppointment);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Gửi thông báo kết quả xét nghiệm
        /// </summary>
        [HttpPost("test-result")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> SendTestResultNotification(
            [FromQuery] int patientId,
            [FromQuery] string testName,
            [FromQuery] string result,
            [FromQuery] int tenantId)
        {
            if (string.IsNullOrEmpty(testName) || string.IsNullOrEmpty(result))
            {
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult("Tên xét nghiệm và kết quả không được để trống"));
            }

            var response = await _notificationService.SendTestResultNotificationAsync(patientId, testName, result, tenantId);
            
            if (!response.Success)
                return BadRequest(response);
            
            return Ok(response);
        }

        /// <summary>
        /// Lấy thống kê thông báo theo thời gian
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetNotificationStatistics(
            [FromQuery] int? tenantId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "day") // day, week, month
        {
            try
            {
                // Lấy báo cáo thông báo
                var reportResult = await _notificationService.GetNotificationReportAsync(tenantId, null, null, fromDate, toDate);
                
                if (!reportResult.Success)
                    return BadRequest(reportResult);

                var report = reportResult.Data!;

                // Tạo thống kê theo thời gian
                var statistics = new
                {
                    TotalNotifications = report.TotalNotifications,
                    ReadNotifications = report.ReadNotifications,
                    UnreadNotifications = report.UnreadNotifications,
                    ReadRate = report.ReadRate,
                    NotificationsByChannel = report.NotificationsByChannel,
                    NotificationsByDay = report.NotificationsByDay,
                    Period = new
                    {
                        FromDate = report.FromDate,
                        ToDate = report.ToDate
                    },
                    TopChannels = report.NotificationsByChannel
                        .OrderByDescending(x => x.Value)
                        .Take(5)
                        .ToDictionary(x => x.Key, x => x.Value),
                    AverageNotificationsPerDay = report.NotificationsByDay.Any() ? 
                        Math.Round((double)report.TotalNotifications / report.NotificationsByDay.Count, 2) : 0
                };

                return Ok(ApiResponse<object>.SuccessResult(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê thông báo");
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy thống kê"));
            }
        }

        /// <summary>
        /// Lấy danh sách thông báo chưa đọc gần đây
        /// </summary>
        [HttpGet("recent-unread")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetRecentUnreadNotifications(
            [FromQuery] int? userId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] int limit = 10)
        {
            var filter = new NotificationFilterDto
            {
                TenantId = tenantId,
                UserId = userId,
                PatientId = patientId,
                IsRead = false,
                PageNumber = 1,
                PageSize = limit
            };

            var result = await _notificationService.GetNotificationsAsync(filter);
            
            if (!result.Success)
                return BadRequest(result);

            var unreadNotifications = result.Data!.Data;
            return Ok(ApiResponse<List<NotificationDto>>.SuccessResult(unreadNotifications));
        }

        /// <summary>
        /// Lấy danh sách template thông báo có sẵn
        /// </summary>
        [HttpGet("templates")]
        public ActionResult<ApiResponse<List<NotificationTemplateDto>>> GetNotificationTemplates()
        {
            var templates = new List<NotificationTemplateDto>
            {
                new NotificationTemplateDto
                {
                    Type = "appointment_reminder",
                    Title = "Nhắc nhở cuộc hẹn",
                    Body = "Bạn có cuộc hẹn với {DoctorName} vào lúc {AppointmentTime} tại {TenantName}. Vui lòng đến đúng giờ.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "DoctorName", "Tên bác sĩ" },
                        { "AppointmentTime", "Thời gian hẹn" },
                        { "TenantName", "Tên phòng khám" }
                    }
                },
                new NotificationTemplateDto
                {
                    Type = "test_result",
                    Title = "Kết quả xét nghiệm",
                    Body = "Kết quả {TestName} của bạn đã có: {Result}. Vui lòng liên hệ phòng khám để biết thêm chi tiết.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "TestName", "Tên xét nghiệm" },
                        { "Result", "Kết quả" }
                    }
                },
                new NotificationTemplateDto
                {
                    Type = "payment_reminder",
                    Title = "Nhắc nhở thanh toán",
                    Body = "Bạn có khoản thanh toán {Amount} VND cho cuộc hẹn ngày {AppointmentDate} chưa được hoàn tất. Vui lòng thanh toán sớm nhất.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "Amount", "Số tiền" },
                        { "AppointmentDate", "Ngày hẹn" }
                    }
                },
                new NotificationTemplateDto
                {
                    Type = "welcome",
                    Title = "Chào mừng bạn",
                    Body = "Chào mừng bạn đến với {TenantName}! Chúng tôi rất vui được phục vụ bạn.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "TenantName", "Tên phòng khám" }
                    }
                }
            };

            return Ok(ApiResponse<List<NotificationTemplateDto>>.SuccessResult(templates));
        }
    }
}
