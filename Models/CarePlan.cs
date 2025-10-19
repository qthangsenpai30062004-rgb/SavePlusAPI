using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class CarePlan
{
    public int CarePlanId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CarePlanItem> CarePlanItems { get; set; } = new List<CarePlanItem>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PatientMeasurement> PatientMeasurements { get; set; } = new List<PatientMeasurement>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual Tenant Tenant { get; set; } = null!;
}
