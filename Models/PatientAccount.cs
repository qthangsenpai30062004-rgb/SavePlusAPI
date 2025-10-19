using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class PatientAccount
{
    public int AccountId { get; set; }

    public int PatientId { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneE164 { get; set; }

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsPhoneVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}


