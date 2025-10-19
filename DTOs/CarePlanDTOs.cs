using System.ComponentModel.DataAnnotations;
using SavePlus_API.Attributes;
using SavePlus_API.Constants;

namespace SavePlus_API.DTOs
{
    public class CarePlanCreateDto
    {
        [Required(ErrorMessage = "Tên kế hoạch chăm sóc là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên kế hoạch chăm sóc không được vượt quá 200 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "ID bệnh nhân là bắt buộc")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [ValidCarePlanStatus]
        public string Status { get; set; } = CarePlanConstants.Statuses.Active; // Draft, Active, Paused, Completed, Cancelled, Expired

        // Danh sách các CarePlanItems để tạo cùng lúc
        public List<CarePlanItemCreateDto> Items { get; set; } = new List<CarePlanItemCreateDto>();
    }

    public class CarePlanUpdateDto
    {
        [StringLength(200, ErrorMessage = "Tên kế hoạch chăm sóc không được vượt quá 200 ký tự")]
        public string? Name { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [ValidCarePlanStatus]
        public string? Status { get; set; }
    }

    public class CarePlanDto
    {
        public int CarePlanId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = null!;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string PatientName { get; set; } = null!;
        public string CreatedByName { get; set; } = null!;
        public List<CarePlanItemDto> Items { get; set; } = new List<CarePlanItemDto>();
    }

    public class CarePlanItemCreateDto
    {
        [Required(ErrorMessage = "Loại item là bắt buộc")]
        [ValidCarePlanItemType]
        public string ItemType { get; set; } = null!; // Medication, Measurement, Exercise, Diet, Symptom, Note

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [StringLength(120, ErrorMessage = "Frequency Cron không được vượt quá 120 ký tự")]
        public string? FrequencyCron { get; set; }

        [Range(1, 10, ErrorMessage = "Số lần trong ngày phải từ 1 đến 10")]
        public byte? TimesPerDay { get; set; }

        [StringLength(20, ErrorMessage = "Ngày trong tuần không được vượt quá 20 ký tự")]
        public string? DaysOfWeek { get; set; } // Format: "1,2,3,4,5" (Monday to Friday)

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CarePlanItemUpdateDto
    {
        [ValidCarePlanItemType]
        public string? ItemType { get; set; }

        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [StringLength(120, ErrorMessage = "Frequency Cron không được vượt quá 120 ký tự")]
        public string? FrequencyCron { get; set; }

        [Range(1, 10, ErrorMessage = "Số lần trong ngày phải từ 1 đến 10")]
        public byte? TimesPerDay { get; set; }

        [StringLength(20, ErrorMessage = "Ngày trong tuần không được vượt quá 20 ký tự")]
        public string? DaysOfWeek { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CarePlanItemDto
    {
        public int ItemId { get; set; }
        public int CarePlanId { get; set; }
        public string ItemType { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? FrequencyCron { get; set; }
        public byte? TimesPerDay { get; set; }
        public string? DaysOfWeek { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Thông tin về completion logs
        public int TotalLogs { get; set; }
        public int CompletedLogs { get; set; }
        public DateTime? LastPerformed { get; set; }
    }

    public class CarePlanItemLogCreateDto
    {
        [Required(ErrorMessage = "ID CarePlan Item là bắt buộc")]
        public int ItemId { get; set; }

        public bool IsCompleted { get; set; } = true;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        public decimal? ValueNumeric { get; set; }

        [StringLength(400, ErrorMessage = "Giá trị text không được vượt quá 400 ký tự")]
        public string? ValueText { get; set; }

        public DateTime? PerformedAt { get; set; } // Nếu null sẽ dùng thời gian hiện tại
    }

    public class CarePlanItemLogDto
    {
        public long LogId { get; set; }
        public int ItemId { get; set; }
        public int PatientId { get; set; }
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
        public decimal? ValueNumeric { get; set; }
        public string? ValueText { get; set; }
        public DateTime PerformedAt { get; set; }

        // Navigation info
        public string ItemTitle { get; set; } = null!;
        public string ItemType { get; set; } = null!;
        public string PatientName { get; set; } = null!;
    }

    // DTO cho báo cáo tiến độ CarePlan
    public class CarePlanProgressDto
    {
        public int CarePlanId { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedItems { get; set; }
        
        public int TotalLogs { get; set; }
        public int CompletedLogs { get; set; }
        public decimal CompletionRate { get; set; } // Tỷ lệ hoàn thành (%)
        
        public DateTime? LastActivity { get; set; }
        public List<CarePlanItemProgressDto> ItemProgress { get; set; } = new List<CarePlanItemProgressDto>();
    }

    public class CarePlanItemProgressDto
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = null!;
        public string ItemType { get; set; } = null!;
        public bool IsActive { get; set; }
        
        public int TotalLogs { get; set; }
        public int CompletedLogs { get; set; }
        public decimal CompletionRate { get; set; }
        
        public DateTime? LastPerformed { get; set; }
        public int ConsecutiveDays { get; set; } // Số ngày liên tiếp thực hiện
    }
}
