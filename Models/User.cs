using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? TenantId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneE164 { get; set; }

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Tenant? Tenant { get; set; }
}
