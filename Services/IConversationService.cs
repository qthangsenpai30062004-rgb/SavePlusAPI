using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public interface IConversationService
    {
        // Conversation management
        Task<ApiResponse<ConversationDTO>> CreateConversationAsync(int tenantId, CreateConversationDTO dto, int createdByUserId);
        Task<ApiResponse<List<ConversationListDTO>>> GetConversationsAsync(int tenantId, int? patientId = null, bool? isClosed = null);
        Task<ApiResponse<ConversationDTO>> GetConversationByIdAsync(int tenantId, long conversationId);
        Task<ApiResponse<ConversationDTO>> UpdateConversationStatusAsync(int tenantId, long conversationId, UpdateConversationStatusDTO dto);
        Task<ApiResponse<bool>> DeleteConversationAsync(int tenantId, long conversationId);

        // Message management
        Task<ApiResponse<MessageDTO>> SendMessageAsync(int tenantId, SendMessageDTO dto, int? senderUserId = null, int? senderPatientId = null);
        Task<ApiResponse<MessageListResponseDTO>> GetMessagesAsync(int tenantId, long conversationId, MessageQueryDTO query);
        Task<ApiResponse<bool>> DeleteMessageAsync(int tenantId, long messageId);

        // Chat statistics
        Task<ApiResponse<ChatStatsDTO>> GetChatStatsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);

        // Utility methods
        Task<bool> IsUserAuthorizedForConversationAsync(int tenantId, long conversationId, int userId);
        Task<bool> IsPatientAuthorizedForConversationAsync(long conversationId, int patientId);
    }
}
