using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO for Service response
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO for creating a new service
    public class ServiceCreateDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 999999999)]
        public decimal BasePrice { get; set; } = 200000;

        [Required]
        [MaxLength(50)]
        public string ServiceType { get; set; } = "General";
    }

    // DTO for updating a service
    public class ServiceUpdateDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, 999999999)]
        public decimal? BasePrice { get; set; }

        [MaxLength(50)]
        public string? ServiceType { get; set; }

        public bool? IsActive { get; set; }
    }

    // DTO for DoctorWorkingHour response
    public class DoctorWorkingHourDto
    {
        public int WorkingHourId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public byte DayOfWeek { get; set; } // 1=Monday, 7=Sunday
        public string DayOfWeekName { get; set; } = string.Empty; // "Thứ 2", "Thứ 3", etc.
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for creating doctor working hours
    public class DoctorWorkingHourCreateDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        [Range(1, 7)]
        public byte DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(5, 120)]
        public int SlotDurationMinutes { get; set; } = 30;
    }
}
