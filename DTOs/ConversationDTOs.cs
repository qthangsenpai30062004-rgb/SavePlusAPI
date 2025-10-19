using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTOs cho tạo Conversation
    public class CreateConversationDTO
    {
        [Required]
        public int PatientId { get; set; }
    }

    // DTOs cho phản hồi Conversation
    public class ConversationDTO
    {
        public long ConversationId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsClosed { get; set; }
        public MessageDTO? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    // DTOs cho danh sách Conversation
    public class ConversationListDTO
    {
        public long ConversationId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsClosed { get; set; }
        public MessageDTO? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    // DTOs cho cập nhật trạng thái Conversation
    public class UpdateConversationStatusDTO
    {
        [Required]
        public bool IsClosed { get; set; }
    }
}
