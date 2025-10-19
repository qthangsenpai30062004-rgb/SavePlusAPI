using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Consultation
{
    public int ConsultationId { get; set; }

    public int AppointmentId { get; set; }

    public string? Summary { get; set; }

    public string? DiagnosisCode { get; set; }

    public string? Advice { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
