using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class ClinicPatient
{
    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public string? Mrn { get; set; }

    public int? PrimaryDoctorId { get; set; }

    public byte Status { get; set; }

    public DateTime EnrolledAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Doctor? PrimaryDoctor { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
