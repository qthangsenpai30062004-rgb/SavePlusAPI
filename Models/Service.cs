using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SavePlus_API.Models
{
    [Table("Services")]
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal BasePrice { get; set; } = 200000;

        [Required]
        [MaxLength(50)]
        public string ServiceType { get; set; } = "General"; // General, Specialist, Emergency

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }

        public virtual ICollection<Appointment>? Appointments { get; set; }
    }
}
