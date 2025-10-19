using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class PaymentTransaction
{
    public long PaymentId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public int? AppointmentId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Method { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? ProviderRef { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
