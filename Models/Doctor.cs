using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int TenantId { get; set; }

    public int UserId { get; set; }

    public string? Specialty { get; set; }

    public string? LicenseNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Title { get; set; }

    public string? PositionTitle { get; set; }

    public short? YearStarted { get; set; }

    public bool IsVerified { get; set; }

    public string? About { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
