using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public string PrimaryPhoneE164 { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<CarePlanItemLog> CarePlanItemLogs { get; set; } = new List<CarePlanItemLog>();

    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();

    public virtual ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PatientMeasurement> PatientMeasurements { get; set; } = new List<PatientMeasurement>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual PatientAccount? PatientAccount { get; set; }
}
