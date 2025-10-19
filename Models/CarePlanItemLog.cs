using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class CarePlanItemLog
{
    public long LogId { get; set; }

    public int ItemId { get; set; }

    public int PatientId { get; set; }

    public DateTime PerformedAt { get; set; }

    public decimal? ValueNumeric { get; set; }

    public string? ValueText { get; set; }

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public virtual CarePlanItem Item { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
