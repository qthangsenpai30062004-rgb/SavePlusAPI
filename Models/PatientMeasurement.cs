using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class PatientMeasurement
{
    public long MeasurementId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public int? CarePlanId { get; set; }

    public string Type { get; set; } = null!;

    public decimal? Value1 { get; set; }

    public decimal? Value2 { get; set; }

    public string? Unit { get; set; }

    public string Source { get; set; } = null!;

    public DateTime MeasuredAt { get; set; }

    public string? Notes { get; set; }

    public virtual CarePlan? CarePlan { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
