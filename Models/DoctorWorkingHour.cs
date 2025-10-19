using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SavePlus_API.Models
{
    [Table("DoctorWorkingHours")]
    public class DoctorWorkingHour
    {
        [Key]
        public int WorkingHourId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [Range(1, 7)] // 1=Monday, 7=Sunday
        public byte DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public int SlotDurationMinutes { get; set; } = 30;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
}
