using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Tenant
{
    public int TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? WeekdayOpen { get; set; }

    public string? WeekdayClose { get; set; }

    public string? WeekendOpen { get; set; }

    public string? WeekendClose { get; set; }

    public int? OwnerUserId { get; set; }

    public virtual User? OwnerUser { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();

    public virtual ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PatientMeasurement> PatientMeasurements { get; set; } = new List<PatientMeasurement>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
