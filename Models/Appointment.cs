using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public int? DoctorId { get; set; }

    public string Type { get; set; } = null!;

    public string? Channel { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Address { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public int? ServiceId { get; set; }

    public virtual Consultation? Consultation { get; set; }

    public virtual Service? Service { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual Tenant Tenant { get; set; } = null!;
}
