using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho đăng ký bệnh nhân mới
    public class PatientRegistrationDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PrimaryPhoneE164 { get; set; } = null!;

        [StringLength(1)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }
    }

    // DTO cho đăng nhập bệnh nhân
    public class PatientLoginDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;

        // Có thể sử dụng OTP thay vì password
        public string? VerificationCode { get; set; }
    }

    // DTO cho thông tin bệnh nhân trả về
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public string PrimaryPhoneE164 { get; set; } = null!;
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho cập nhật profile bệnh nhân
    public class PatientUpdateDto
    {
        [StringLength(200)]
        public string? FullName { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }
    }

    // DTO cho bệnh nhân trong phòng khám (ClinicPatient)
    public class ClinicPatientDto
    {
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public string? Mrn { get; set; }
        public int? PrimaryDoctorId { get; set; }
        public byte Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public PatientDto Patient { get; set; } = null!;
        public string? TenantName { get; set; }
        public string? DoctorName { get; set; }
    }

    // DTO cho search bệnh nhân (dùng cho autocomplete)
    public class PatientSearchDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public string PrimaryPhoneE164 { get; set; } = null!;
        public string? Mrn { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
