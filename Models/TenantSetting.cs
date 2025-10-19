using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SavePlus_API.Models
{
    [Table("TenantSettings")]
    public class TenantSetting
    {
        [Key]
        public int TenantSettingId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string SettingValue { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string SettingType { get; set; } = null!; // Integer, String, Boolean, Decimal

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
