using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public int? CarePlanId { get; set; }

    public int DoctorId { get; set; }

    public DateTime IssuedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual CarePlan? CarePlan { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual Tenant Tenant { get; set; } = null!;
}
