using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO for reading tenant settings
    public class TenantSettingDto
    {
        public int TenantSettingId { get; set; }
        public int TenantId { get; set; }
        public string SettingKey { get; set; } = null!;
        public string SettingValue { get; set; } = null!;
        public string SettingType { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO for creating/updating tenant settings
    public class TenantSettingUpdateDto
    {
        [Required(ErrorMessage = "Setting key là bắt buộc")]
        [StringLength(100)]
        public string SettingKey { get; set; } = null!;

        [Required(ErrorMessage = "Setting value là bắt buộc")]
        [StringLength(500)]
        public string SettingValue { get; set; } = null!;
    }

    // DTO for bulk setting update
    public class TenantSettingsBulkUpdateDto
    {
        [Required]
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    // DTO for booking configuration (client-friendly format)
    public class BookingConfigDto
    {
        /// <summary>
        /// Maximum number of days in advance a booking can be made
        /// </summary>
        public int MaxAdvanceBookingDays { get; set; }

        /// <summary>
        /// Default time slot duration in minutes
        /// </summary>
        public int DefaultSlotDurationMinutes { get; set; }

        /// <summary>
        /// Minimum hours in advance a booking can be made
        /// </summary>
        public int MinAdvanceBookingHours { get; set; }

        /// <summary>
        /// Maximum hours before appointment to allow cancellation
        /// </summary>
        public int MaxCancellationHours { get; set; }

        /// <summary>
        /// Allow weekend booking
        /// </summary>
        public bool AllowWeekendBooking { get; set; }
    }

    // DTO for payment configuration
    public class PaymentConfigDto
    {
        public bool BankTransferEnabled { get; set; }
        public bool EWalletEnabled { get; set; }
        public bool CashEnabled { get; set; } = true;
    }
}
