using System.ComponentModel.DataAnnotations;
using SavePlus_API.Attributes;

namespace SavePlus_API.DTOs
{
    // DTO cho lấy thông tin bác sĩ để edit
    public class DoctorEditDto
    {
        public int DoctorId { get; set; }
        public int TenantId { get; set; }
        
        // From Users table
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneE164 { get; set; }
        
        // From Doctors table
        public string? AvatarUrl { get; set; }
        public string? Title { get; set; }
        public string? PositionTitle { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public short? YearStarted { get; set; }
        public string? About { get; set; }
        public bool IsVerified { get; set; }
    }

    // DTO cho update thông tin bác sĩ (bác sĩ tự update)
    public class DoctorSelfUpdateDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-120 ký tự")]
        public string FullName { get; set; } = null!;

        [ValidPhoneE164(ErrorMessage = "Số điện thoại phải có định dạng +84xxxxxxxxx")]
        [StringLength(20)]
        public string? PhoneE164 { get; set; }

        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(60)]
        public string? Title { get; set; }

        [StringLength(120)]
        public string? PositionTitle { get; set; }

        [StringLength(120)]
        public string? Specialty { get; set; }

        [StringLength(60)]
        public string? LicenseNumber { get; set; }

        [Range(1950, 2100, ErrorMessage = "Năm bắt đầu hành nghề phải từ 1950 đến hiện tại")]
        public short? YearStarted { get; set; }

        [StringLength(5000, ErrorMessage = "Giới thiệu không được vượt quá 5,000 ký tự")]
        public string? About { get; set; }
    }

    // DTO cho admin update (có thể update isVerified)
    public class DoctorAdminUpdateDto : DoctorSelfUpdateDto
    {
        public bool? IsVerified { get; set; }
    }
}
