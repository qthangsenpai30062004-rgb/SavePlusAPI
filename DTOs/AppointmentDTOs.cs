using System.ComponentModel.DataAnnotations;
using SavePlus_API.Attributes;

namespace SavePlus_API.DTOs
{
    // DTO cho tạo cuộc hẹn mới
    public class AppointmentCreateDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public int? DoctorId { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        [Required(ErrorMessage = "Loại cuộc hẹn là bắt buộc")]
        [ValidAppointmentType]
        public string Type { get; set; } = null!; // "Home", "Clinic", "Online", "Phone"

        [ValidAppointmentChannel]
        public string? Channel { get; set; } // "App", "Web", "Phone", "Counter", "Staff"

        [StringLength(300)]
        public string? Address { get; set; }

        // Chi phí ước tính cho lịch hẹn
        public decimal? EstimatedCost { get; set; }
    }

    // DTO cho thông tin cuộc hẹn
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Type { get; set; } = null!;
        public string? Channel { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Patient Info
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
        public string? PatientGender { get; set; }
        public DateOnly? PatientDateOfBirth { get; set; }
        public string? PatientAddress { get; set; }
        
        // Doctor Info
        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }
        public string? DoctorLicenseNumber { get; set; }
        public string? DoctorPhone { get; set; }
        
        // Tenant Info
        public string? TenantName { get; set; }
    }

    // DTO cho cập nhật cuộc hẹn
    public class AppointmentUpdateDto
    {
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        
        [ValidAppointmentType]
        public string? Type { get; set; }
        
        [ValidAppointmentStatus]
        public string? Status { get; set; } // "Scheduled", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow", "Rescheduled"
        
        [StringLength(300)]
        public string? Address { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // DTO cho lọc cuộc hẹn
    public class AppointmentFilterDto
    {
        public int? TenantId { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [ValidAppointmentStatus]
        public string? Status { get; set; }
        
        [ValidAppointmentType]
        public string? Type { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
