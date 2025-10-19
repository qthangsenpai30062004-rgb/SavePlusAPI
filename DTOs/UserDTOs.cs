using System.ComponentModel.DataAnnotations;
using SavePlus_API.Constants;
using SavePlus_API.Attributes;

namespace SavePlus_API.DTOs
{
    // DTO cho tạo user mới (staff registration)
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? PhoneE164 { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [StringLength(30)]
        [ValidRole(allowAdvancedRoles: true)] // Cho phép tất cả roles khi tạo user
        public string Role { get; set; } = null!; // Sử dụng UserRoles constants

        [Required(ErrorMessage = "TenantId là bắt buộc")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        public string Password { get; set; } = null!;
    }

    // DTO cho cập nhật user
    public class UserUpdateDto
    {
        [StringLength(200)]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? PhoneE164 { get; set; }

        [StringLength(30)]
        [ValidRole(allowAdvancedRoles: true)]
        public string? Role { get; set; }

        public bool? IsActive { get; set; }
    }

    // DTO cho đăng nhập staff bằng email/password
    public class StaffLoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = null!;

        public int? TenantId { get; set; } // Optional: để specify tenant nếu user thuộc nhiều tenant
    }

    // DTO cho đổi mật khẩu
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6-100 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = null!;
    }

    // DTO cho User response (không có thông tin nhạy cảm)
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneE164 { get; set; }
        public string Role { get; set; } = null!;
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho User với thông tin Doctor (nếu là bác sĩ)
    public class UserWithDoctorDto : UserDto
    {
        public int? DoctorId { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Specialty { get; set; }
    }

    // DTO cho search doctor (dùng cho autocomplete)
    public class DoctorSearchDto
    {
        public int UserId { get; set; }
        public int? DoctorId { get; set; }
        public string FullName { get; set; } = null!;
        public string? LicenseNumber { get; set; }
        public string? Specialty { get; set; }
        public string Email { get; set; } = null!;
    }
}
