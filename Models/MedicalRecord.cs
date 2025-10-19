using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class MedicalRecord
{
    public long RecordId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public string Title { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? RecordType { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
