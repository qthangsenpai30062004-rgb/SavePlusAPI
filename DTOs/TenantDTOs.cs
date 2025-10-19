using System.ComponentModel.DataAnnotations;
using SavePlus_API.Attributes;

namespace SavePlus_API.DTOs
{
    // DTO cho tạo tenant mới
    public class TenantCreateDto
    {
        [Required(ErrorMessage = "Mã tenant là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã tenant không được vượt quá 50 ký tự")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Tên phòng khám là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên phòng khám phải từ 3-200 ký tự")]
        public string Name { get; set; } = null!;

        [ValidPhoneE164(ErrorMessage = "Số điện thoại phải có định dạng +84xxxxxxxxx")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(5000, ErrorMessage = "Mô tả không được vượt quá 5000 ký tự")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL thumbnail không hợp lệ")]
        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Url(ErrorMessage = "URL cover image không hợp lệ")]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ mở cửa phải có định dạng HH:mm (ví dụ: 07:30)")]
        [StringLength(10)]
        public string? WeekdayOpen { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ đóng cửa phải có định dạng HH:mm (ví dụ: 19:00)")]
        [StringLength(10)]
        public string? WeekdayClose { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ mở cửa cuối tuần phải có định dạng HH:mm (ví dụ: 08:00)")]
        [StringLength(10)]
        public string? WeekendOpen { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ đóng cửa cuối tuần phải có định dạng HH:mm (ví dụ: 18:00)")]
        [StringLength(10)]
        public string? WeekendClose { get; set; }

        public int? OwnerUserId { get; set; }
    }

    // DTO cho thông tin tenant
    public class TenantDto
    {
        public int TenantId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WeekdayOpen { get; set; }
        public string? WeekdayClose { get; set; }
        public string? WeekendOpen { get; set; }
        public string? WeekendClose { get; set; }
        public int? OwnerUserId { get; set; }
        public string? OwnerName { get; set; }
    }

    // DTO cho cập nhật tenant
    public class TenantUpdateDto
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên phòng khám phải từ 3-200 ký tự")]
        public string? Name { get; set; }

        [ValidPhoneE164(ErrorMessage = "Số điện thoại phải có định dạng +84xxxxxxxxx")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        public byte? Status { get; set; }

        [StringLength(5000, ErrorMessage = "Mô tả không được vượt quá 5000 ký tự")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL thumbnail không hợp lệ")]
        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Url(ErrorMessage = "URL cover image không hợp lệ")]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ mở cửa phải có định dạng HH:mm (ví dụ: 07:30)")]
        [StringLength(10)]
        public string? WeekdayOpen { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ đóng cửa phải có định dạng HH:mm (ví dụ: 19:00)")]
        [StringLength(10)]
        public string? WeekdayClose { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ mở cửa cuối tuần phải có định dạng HH:mm (ví dụ: 08:00)")]
        [StringLength(10)]
        public string? WeekendOpen { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ đóng cửa cuối tuần phải có định dạng HH:mm (ví dụ: 18:00)")]
        [StringLength(10)]
        public string? WeekendClose { get; set; }

        public int? OwnerUserId { get; set; }
    }

    // DTO cho thống kê tenant
    public class TenantStatsDto
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = null!;
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int ActiveCarePlans { get; set; }
    }

    // DTO cho thông tin bác sĩ
    public class DoctorDto
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneE164 { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Title { get; set; }
        public string? PositionTitle { get; set; }
        public short? YearStarted { get; set; }
        public bool IsVerified { get; set; }
        public string? About { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TenantName { get; set; }
    }

    // DTO cho cập nhật thông tin bác sĩ
    public class UpdateDoctorDto
    {
        [StringLength(200)]
        public string? Specialty { get; set; }

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(60)]
        public string? Title { get; set; }

        [StringLength(120)]
        public string? PositionTitle { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm bắt đầu không hợp lệ")]
        public short? YearStarted { get; set; }

        public bool? IsVerified { get; set; }

        public string? About { get; set; }

        public bool? IsActive { get; set; }
    }
}
