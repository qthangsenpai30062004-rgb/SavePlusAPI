using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Reminder
{
    public int ReminderId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public string Title { get; set; } = null!;

    public string? Body { get; set; }

    public string TargetType { get; set; } = null!;

    public int TargetId { get; set; }

    public DateTime NextFireAt { get; set; }

    public string Channel { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
