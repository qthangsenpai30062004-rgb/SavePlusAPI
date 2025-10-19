using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho hiển thị thông tin notification
    public class NotificationDto
    {
        public long NotificationId { get; set; }
        public int TenantId { get; set; }
        public int? UserId { get; set; }
        public int? PatientId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead => ReadAt.HasValue;
        
        // Navigation properties
        public string? UserName { get; set; }
        public string? PatientName { get; set; }
        public string? TenantName { get; set; }
    }

    // DTO cho tạo notification mới
    public class NotificationCreateDto
    {
        [Required(ErrorMessage = "ID phòng khám là bắt buộc")]
        public int TenantId { get; set; }

        public int? UserId { get; set; }

        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Tiêu đề thông báo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung thông báo là bắt buộc")]
        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string Body { get; set; } = null!;

        [Required(ErrorMessage = "Kênh gửi là bắt buộc")]
        [StringLength(20, ErrorMessage = "Kênh gửi không được vượt quá 20 ký tự")]
        public string Channel { get; set; } = "Push"; // Push, Email, SMS

        public DateTime? ScheduledAt { get; set; } // Để gửi thông báo theo lịch
    }

    // DTO cho cập nhật notification
    public class NotificationUpdateDto
    {
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string? Body { get; set; }

        [StringLength(20, ErrorMessage = "Kênh gửi không được vượt quá 20 ký tự")]
        public string? Channel { get; set; }
    }

    // DTO cho lọc notification
    public class NotificationFilterDto
    {
        public int? TenantId { get; set; }
        public int? UserId { get; set; }
        public int? PatientId { get; set; }
        public string? Channel { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // DTO cho báo cáo notification
    public class NotificationReportDto
    {
        public int TotalNotifications { get; set; }
        public int ReadNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public double ReadRate => TotalNotifications > 0 ? (double)ReadNotifications / TotalNotifications * 100 : 0;
        public Dictionary<string, int> NotificationsByChannel { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> NotificationsByDay { get; set; } = new Dictionary<string, int>();
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    // DTO cho gửi notification hàng loạt
    public class BulkNotificationDto
    {
        [Required(ErrorMessage = "ID phòng khám là bắt buộc")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Tiêu đề thông báo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung thông báo là bắt buộc")]
        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string Body { get; set; } = null!;

        [Required(ErrorMessage = "Kênh gửi là bắt buộc")]
        [StringLength(20, ErrorMessage = "Kênh gửi không được vượt quá 20 ký tự")]
        public string Channel { get; set; } = "Push";

        // Danh sách người nhận
        public List<int>? UserIds { get; set; }
        public List<int>? PatientIds { get; set; }

        // Hoặc gửi cho tất cả user/patient của tenant
        public bool SendToAllUsers { get; set; } = false;
        public bool SendToAllPatients { get; set; } = false;

        public DateTime? ScheduledAt { get; set; }
    }

    // DTO cho đánh dấu đã đọc
    public class MarkAsReadDto
    {
        [Required(ErrorMessage = "Danh sách ID notification là bắt buộc")]
        public List<long> NotificationIds { get; set; } = new List<long>();
    }

    // DTO cho thống kê notification của user
    public class UserNotificationSummaryDto
    {
        public int? UserId { get; set; }
        public int? PatientId { get; set; }
        public string? RecipientName { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public DateTime? LastNotificationDate { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();
    }

    // DTO cho notification template
    public class NotificationTemplateDto
    {
        public string Type { get; set; } = null!; // "appointment_reminder", "test_result", etc.
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Channel { get; set; } = "Push";
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
