using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SavePlus_API.Models;

public partial class SavePlusDbContext : DbContext
{
    public SavePlusDbContext()
    {
    }

    public SavePlusDbContext(DbContextOptions<SavePlusDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<CarePlan> CarePlans { get; set; }

    public virtual DbSet<CarePlanItem> CarePlanItems { get; set; }

    public virtual DbSet<CarePlanItemLog> CarePlanItemLogs { get; set; }

    public virtual DbSet<ClinicPatient> ClinicPatients { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorWorkingHour> DoctorWorkingHours { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientAccount> PatientAccounts { get; set; }

    public virtual DbSet<PatientMeasurement> PatientMeasurements { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<TenantSetting> TenantSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }
    }
    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn;
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC25452D8E5");

            entity.HasIndex(e => new { e.TenantId, e.StartAt }, "IX_Appointments_Tenant_StartAt");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Channel).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Booked");
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_Appointments_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Tenant");
        });

        modelBuilder.Entity<CarePlan>(entity =>
        {
            entity.HasKey(e => e.CarePlanId).HasName("PK__CarePlan__2EB4A27D5A6CCB52");

            entity.HasIndex(e => new { e.TenantId, e.PatientId, e.Name }, "UQ_CarePlans_TenantPatientName").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CarePlans)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlans_User");

            entity.HasOne(d => d.Patient).WithMany(p => p.CarePlans)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlans_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.CarePlans)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlans_Tenant");
        });

        modelBuilder.Entity<CarePlanItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__CarePlan__727E838B0439F66E");

            entity.Property(e => e.DaysOfWeek).HasMaxLength(20);
            entity.Property(e => e.FrequencyCron).HasMaxLength(120);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ItemType).HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CarePlan).WithMany(p => p.CarePlanItems)
                .HasForeignKey(d => d.CarePlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlanItems_CarePlan");
        });

        modelBuilder.Entity<CarePlanItemLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__CarePlan__5E5486487492F4E8");

            entity.HasIndex(e => new { e.PatientId, e.PerformedAt }, "IX_CarePlanItemLogs_Patient_Time").IsDescending(false, true);

            entity.Property(e => e.IsCompleted).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ValueNumeric).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.ValueText).HasMaxLength(400);

            entity.HasOne(d => d.Item).WithMany(p => p.CarePlanItemLogs)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlanItemLogs_Item");

            entity.HasOne(d => d.Patient).WithMany(p => p.CarePlanItemLogs)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarePlanItemLogs_Patient");
        });

        modelBuilder.Entity<ClinicPatient>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.PatientId });

            entity.HasIndex(e => new { e.TenantId, e.PrimaryDoctorId }, "IX_ClinicPatients_PrimaryDoctor");

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Mrn)
                .HasMaxLength(50)
                .HasColumnName("MRN");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Patient).WithMany(p => p.ClinicPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClinicPatients_Patient");

            entity.HasOne(d => d.PrimaryDoctor).WithMany(p => p.ClinicPatients)
                .HasForeignKey(d => d.PrimaryDoctorId)
                .HasConstraintName("FK_ClinicPatients_Doctor");

            entity.HasOne(d => d.Tenant).WithMany(p => p.ClinicPatients)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClinicPatients_Tenant");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.ConsultationId).HasName("PK__Consulta__5D014A98F07DF8C2");

            entity.HasIndex(e => e.AppointmentId, "UQ__Consulta__8ECDFCC3B190BF2F").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiagnosisCode).HasMaxLength(30);

            entity.HasOne(d => d.Appointment).WithOne(p => p.Consultation)
                .HasForeignKey<Consultation>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consultations_Appointment");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D877043D0C77");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_User");

            entity.HasOne(d => d.Patient).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_Tenant");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EBF3EEA54B1");

            entity.HasIndex(e => e.UserId, "UQ__Doctors__1788CC4DBD3C11AB").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseNumber).HasMaxLength(60);
            entity.Property(e => e.Specialty).HasMaxLength(120);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctors_Tenant");

            entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctors_User");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78E98AB13ED2");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.RecordType).HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_MedicalRecords_User");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Tenant");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C8657837C");

            entity.HasIndex(e => new { e.ConversationId, e.SentAt }, "IX_Messages_Conversation_Time").IsDescending(false, true);

            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Conversation");

            entity.HasOne(d => d.SenderPatient).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderPatientId)
                .HasConstraintName("FK_Messages_SenderPatient");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderUserId)
                .HasConstraintName("FK_Messages_SenderUser");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1272ED5C77");

            entity.HasIndex(e => new { e.UserId, e.SentAt }, "IX_Notifications_User_Time").IsDescending(false, true);

            entity.Property(e => e.Body).HasMaxLength(500);
            entity.Property(e => e.Channel).HasMaxLength(20);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Patient).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_Notifications_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Tenant");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC366CD8C6E9A");

            entity.HasIndex(e => e.PrimaryPhoneE164, "UQ__Patients__CF1CB11E73313154").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PrimaryPhoneE164).HasMaxLength(20);
        });

        modelBuilder.Entity<PatientAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__PatientA__349DA5A66B4EE8E5");

            entity.HasIndex(e => e.Email, "IX_PatientAccounts_Email");

            entity.HasIndex(e => e.PhoneE164, "IX_PatientAccounts_Phone");

            entity.HasIndex(e => e.Email, "UQ_PatientAccounts_Email").IsUnique();

            entity.HasIndex(e => e.PatientId, "UQ_PatientAccounts_PatientId").IsUnique();

            entity.HasIndex(e => e.PhoneE164, "UQ_PatientAccounts_Phone").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsPhoneVerified).HasDefaultValue(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(128);
            entity.Property(e => e.PhoneE164).HasMaxLength(20);

            entity.HasOne(d => d.Patient).WithOne(p => p.PatientAccount)
                .HasForeignKey<PatientAccount>(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientAccounts_Patient");
        });

        modelBuilder.Entity<PatientMeasurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__PatientM__85599FB870C650D5");

            entity.HasIndex(e => new { e.PatientId, e.MeasuredAt }, "IX_Measurements_Patient_Time").IsDescending(false, true);

            entity.Property(e => e.Notes).HasMaxLength(300);
            entity.Property(e => e.Source)
                .HasMaxLength(30)
                .HasDefaultValue("Manual");
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Value1).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.Value2).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.CarePlan).WithMany(p => p.PatientMeasurements)
                .HasForeignKey(d => d.CarePlanId)
                .HasConstraintName("FK_Measurements_CarePlan");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientMeasurements)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Measurements_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.PatientMeasurements)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Measurements_Tenant");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PaymentT__9B556A3822EB1F1D");

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("VND")
                .IsFixedLength();
            entity.Property(e => e.Method).HasMaxLength(20);
            entity.Property(e => e.ProviderRef).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Appointment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_Payment_Appointment");

            entity.HasOne(d => d.Patient).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Tenant");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__40130832B84EBB3E");

            entity.Property(e => e.IssuedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.CarePlan).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.CarePlanId)
                .HasConstraintName("FK_Prescriptions_CarePlan");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescriptions_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescriptions_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescriptions_Tenant");
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Prescrip__727E838BB865D29C");

            entity.Property(e => e.Dose).HasMaxLength(50);
            entity.Property(e => e.DrugName).HasMaxLength(200);
            entity.Property(e => e.Form).HasMaxLength(50);
            entity.Property(e => e.Frequency).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(400);
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.Strength).HasMaxLength(50);

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionItems_Prescription");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("PK__Reminder__01A83087753E3026");

            entity.HasIndex(e => new { e.TenantId, e.NextFireAt }, "IX_Reminders_NextFire");

            entity.Property(e => e.Body).HasMaxLength(500);
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasDefaultValue("Push");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TargetType).HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Patient).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reminders_Patient");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reminders_Tenant");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("PK__Tenants__2E9B47E15F589DC3");

            entity.HasIndex(e => e.Code, "UQ__Tenants__A25C5AA703C5F56D").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.OwnerUser)
                .WithMany()
                .HasForeignKey(d => d.OwnerUserId)
                .HasConstraintName("FK_Tenants_OwnerUser")
                .OnDelete(DeleteBehavior.Restrict);

            // Disable OUTPUT clause for tables with triggers
            entity.ToTable(tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1B4688FD");

            entity.HasIndex(e => new { e.TenantId, e.Email }, "UQ_Users_Email").IsUnique();

            entity.HasIndex(e => new { e.TenantId, e.PhoneE164 }, "UQ_Users_Phone").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(128);
            entity.Property(e => e.PhoneE164).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(30);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK_Users_Tenant");
        });

        // Configure TenantSettings to avoid OUTPUT clause conflict with triggers
        modelBuilder.Entity<TenantSetting>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("TR_TenantSettings_UpdatedAt"));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
