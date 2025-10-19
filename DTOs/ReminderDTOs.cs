using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTOs cho tạo Reminder
    public class CreateReminderDTO
    {
        [Required(ErrorMessage = "PatientId là bắt buộc")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Nội dung không được quá 500 ký tự")]
        public string? Body { get; set; }

        [Required(ErrorMessage = "Loại đối tượng là bắt buộc")]
        [StringLength(30, ErrorMessage = "Loại đối tượng không được quá 30 ký tự")]
        public string TargetType { get; set; } = string.Empty; // "Prescription", "CarePlan", "Appointment", "Measurement"

        [Required(ErrorMessage = "ID đối tượng là bắt buộc")]
        public int TargetId { get; set; }

        [Required(ErrorMessage = "Thời gian nhắc nhở là bắt buộc")]
        public DateTime NextFireAt { get; set; }

        [StringLength(20, ErrorMessage = "Kênh thông báo không được quá 20 ký tự")]
        public string Channel { get; set; } = "Push"; // "Push", "SMS", "Email"

        public bool IsActive { get; set; } = true;
    }

    // DTOs cho cập nhật Reminder
    public class UpdateReminderDTO
    {
        [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "Nội dung không được quá 500 ký tự")]
        public string? Body { get; set; }

        public DateTime? NextFireAt { get; set; }

        [StringLength(20, ErrorMessage = "Kênh thông báo không được quá 20 ký tự")]
        public string? Channel { get; set; }

        public bool? IsActive { get; set; }
    }

    // DTOs cho phản hồi Reminder
    public class ReminderDTO
    {
        public int ReminderId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string? TargetName { get; set; } // Tên của đối tượng được nhắc
        public DateTime NextFireAt { get; set; }
        public string Channel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOverdue { get; set; } // Đã quá hạn chưa
        public int MinutesUntilFire { get; set; } // Số phút còn lại
    }

    // DTOs cho danh sách Reminder với phân trang
    public class ReminderListResponseDTO
    {
        public List<ReminderDTO> Reminders { get; set; } = new List<ReminderDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // DTOs cho query parameters
    public class ReminderQueryDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? PatientId { get; set; }
        public string? TargetType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsOverdue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Channel { get; set; }
    }

    // DTOs cho thống kê Reminder
    public class ReminderStatsDTO
    {
        public int TotalReminders { get; set; }
        public int ActiveReminders { get; set; }
        public int OverdueReminders { get; set; }
        public int TodayReminders { get; set; }
        public int WeekReminders { get; set; }
        
        // Phân loại theo loại đối tượng
        public Dictionary<string, int> RemindersByType { get; set; } = new Dictionary<string, int>();
        
        // Phân loại theo kênh thông báo
        public Dictionary<string, int> RemindersByChannel { get; set; } = new Dictionary<string, int>();
        
        // Reminders sắp tới (trong 24h)
        public List<ReminderDTO> UpcomingReminders { get; set; } = new List<ReminderDTO>();
    }

    // DTOs cho bulk operations
    public class BulkReminderActionDTO
    {
        [Required(ErrorMessage = "Danh sách ID là bắt buộc")]
        public List<int> ReminderIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Hành động là bắt buộc")]
        public string Action { get; set; } = string.Empty; // "activate", "deactivate", "delete", "snooze"

        // Cho action "snooze"
        public int? SnoozeMinutes { get; set; }
    }

    // DTOs cho snooze reminder
    public class SnoozeReminderDTO
    {
        [Required(ErrorMessage = "Số phút hoãn là bắt buộc")]
        [Range(1, 10080, ErrorMessage = "Số phút hoãn phải từ 1 đến 10080 (1 tuần)")]
        public int Minutes { get; set; } // Số phút muốn hoãn
    }

    // DTOs cho fire reminder (kích hoạt nhắc nhở)
    public class FireReminderDTO
    {
        public int ReminderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string Channel { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public DateTime FiredAt { get; set; }
    }

    // DTOs cho reminder templates (mẫu nhắc nhở)
    public class ReminderTemplateDTO
    {
        public string TargetType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int DefaultMinutesBefore { get; set; } // Mặc định nhắc trước bao nhiêu phút
    }
}
