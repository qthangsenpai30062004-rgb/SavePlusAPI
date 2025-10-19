using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho đăng ký Patient Account
    public class PatientAccountRegisterDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string PhoneE164 { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = null!;

        // Thông tin cá nhân
        [StringLength(1, ErrorMessage = "Giới tính chỉ có 1 ký tự (M/F/O)")]
        public string? Gender { get; set; } // M, F, O

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }
    }

    // DTO cho đăng nhập Patient bằng Email/Password
    public class PatientAccountLoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = null!;
    }

    // DTO cho đăng nhập Patient bằng Phone/Password (alternative)
    public class PatientAccountLoginByPhoneDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneE164 { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = null!;
    }

    // DTO cho response sau khi đăng ký/đăng nhập
    public class PatientAccountResponseDto
    {
        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneE164 { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Patient Info
        public string FullName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }

    // DTO cho cập nhật Patient Account
    public class PatientAccountUpdateDto
    {
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? PhoneE164 { get; set; }

        // Patient Info
        [StringLength(200)]
        public string? FullName { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }
    }

    // DTO cho đổi mật khẩu Patient
    public class PatientAccountChangePasswordDto
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

    // DTO cho request OTP verify email/phone
    public class PatientAccountVerifyRequestDto
    {
        [Required(ErrorMessage = "Loại xác thực là bắt buộc")]
        public string VerificationType { get; set; } = null!; // "email" or "phone"
    }

    // DTO cho verify email/phone với OTP
    public class PatientAccountVerifyOtpDto
    {
        [Required(ErrorMessage = "Loại xác thực là bắt buộc")]
        public string VerificationType { get; set; } = null!; // "email" or "phone"

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = null!;
    }

    // DTO cho forgot password Patient
    public class PatientAccountForgotPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;
    }

    // DTO cho reset password Patient
    public class PatientAccountResetPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = null!;
    }

}

