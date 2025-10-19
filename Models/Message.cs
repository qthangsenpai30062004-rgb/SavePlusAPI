using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class Message
{
    public long MessageId { get; set; }

    public long ConversationId { get; set; }

    public int? SenderUserId { get; set; }

    public int? SenderPatientId { get; set; }

    public string? Content { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime SentAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual Patient? SenderPatient { get; set; }

    public virtual User? SenderUser { get; set; }
}
