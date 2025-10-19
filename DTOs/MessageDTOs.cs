using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTOs cho gửi tin nhắn
    public class SendMessageDTO
    {
        [Required]
        public long ConversationId { get; set; }
        
        public string? Content { get; set; }
        
        public IFormFile? Attachment { get; set; }
    }

    // DTOs cho phản hồi Message
    public class MessageDTO
    {
        public long MessageId { get; set; }
        public long ConversationId { get; set; }
        public int? SenderUserId { get; set; }
        public string? SenderUserName { get; set; }
        public int? SenderPatientId { get; set; }
        public string? SenderPatientName { get; set; }
        public string? Content { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsFromPatient { get; set; }
    }

    // DTOs cho danh sách tin nhắn với phân trang
    public class MessageListResponseDTO
    {
        public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // DTOs cho query parameters
    public class MessageQueryDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // DTOs cho thống kê chat
    public class ChatStatsDTO
    {
        public int TotalConversations { get; set; }
        public int ActiveConversations { get; set; }
        public int ClosedConversations { get; set; }
        public int TotalMessages { get; set; }
        public int MessagesToday { get; set; }
        public double AverageResponseTime { get; set; } // Tính bằng phút
    }
}
