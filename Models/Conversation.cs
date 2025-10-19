using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Conversation
{
    public long ConversationId { get; set; }

    public int TenantId { get; set; }

    public int PatientId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsClosed { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
