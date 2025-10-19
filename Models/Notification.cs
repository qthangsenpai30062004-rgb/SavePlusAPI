using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Notification
{
    public long NotificationId { get; set; }

    public int TenantId { get; set; }

    public int? UserId { get; set; }

    public int? PatientId { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual User? User { get; set; }
}
