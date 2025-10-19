using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class ConversationService : IConversationService
    {
        private readonly SavePlusDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(SavePlusDbContext context, IWebHostEnvironment environment, ILogger<ConversationService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponse<ConversationDTO>> CreateConversationAsync(int tenantId, CreateConversationDTO dto, int createdByUserId)
        {
            try
            {
                // Kiểm tra bệnh nhân có tồn tại không
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == dto.PatientId);
                if (patient == null)
                {
                    return ApiResponse<ConversationDTO>.ErrorResult("Bệnh nhân không tồn tại");
                }

                // Kiểm tra có conversation nào đang active cho bệnh nhân này không
                var existingConversation = await _context.Conversations
                    .Include(c => c.Patient)
                    .Include(c => c.CreatedByUser)
                    .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.PatientId == dto.PatientId && !c.IsClosed);

                if (existingConversation != null)
                {
                    return ApiResponse<ConversationDTO>.SuccessResult(MapToConversationDTO(existingConversation), "Sử dụng cuộc trò chuyện hiện có");
                }

                // Tạo conversation mới
                var conversation = new Conversation
                {
                    TenantId = tenantId,
                    PatientId = dto.PatientId,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsClosed = false
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                // Load lại với includes
                conversation = await _context.Conversations
                    .Include(c => c.Patient)
                    .Include(c => c.CreatedByUser)
                    .FirstAsync(c => c.ConversationId == conversation.ConversationId);

                return ApiResponse<ConversationDTO>.SuccessResult(MapToConversationDTO(conversation), "Tạo cuộc trò chuyện thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cuộc trò chuyện");
                return ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra khi tạo cuộc trò chuyện");
            }
        }

        public async Task<ApiResponse<List<ConversationListDTO>>> GetConversationsAsync(int tenantId, int? patientId = null, bool? isClosed = null)
        {
            try
            {
                var query = _context.Conversations
                    .Include(c => c.Patient)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                        .ThenInclude(m => m.SenderUser)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                        .ThenInclude(m => m.SenderPatient)
                    .Where(c => c.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(c => c.PatientId == patientId.Value);

                if (isClosed.HasValue)
                    query = query.Where(c => c.IsClosed == isClosed.Value);

                var conversations = await query
                    .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : c.CreatedAt)
                    .ToListAsync();

                var result = new List<ConversationListDTO>();
                
                foreach (var conversation in conversations)
                {
                    var lastMessage = conversation.Messages.FirstOrDefault();
                    var unreadCount = await _context.Messages
                        .CountAsync(m => m.ConversationId == conversation.ConversationId && m.SenderPatientId != null);

                    result.Add(new ConversationListDTO
                    {
                        ConversationId = conversation.ConversationId,
                        PatientId = conversation.PatientId,
                        PatientName = conversation.Patient?.FullName ?? "",
                        PatientAvatar = null,
                        CreatedAt = conversation.CreatedAt,
                        IsClosed = conversation.IsClosed,
                        LastMessage = lastMessage != null ? MapToMessageDTO(lastMessage) : null,
                        UnreadCount = unreadCount
                    });
                }

                return ApiResponse<List<ConversationListDTO>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện");
                return ApiResponse<List<ConversationListDTO>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách cuộc trò chuyện");
            }
        }

        public async Task<ApiResponse<ConversationDTO>> GetConversationByIdAsync(int tenantId, long conversationId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Patient)
                    .Include(c => c.CreatedByUser)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                        .ThenInclude(m => m.SenderUser)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                        .ThenInclude(m => m.SenderPatient)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.TenantId == tenantId);

                if (conversation == null)
                {
                    return ApiResponse<ConversationDTO>.ErrorResult("Cuộc trò chuyện không tồn tại");
                }

                return ApiResponse<ConversationDTO>.SuccessResult(MapToConversationDTO(conversation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin cuộc trò chuyện");
                return ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra khi lấy thông tin cuộc trò chuyện");
            }
        }

        public async Task<ApiResponse<ConversationDTO>> UpdateConversationStatusAsync(int tenantId, long conversationId, UpdateConversationStatusDTO dto)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Patient)
                    .Include(c => c.CreatedByUser)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.TenantId == tenantId);

                if (conversation == null)
                {
                    return ApiResponse<ConversationDTO>.ErrorResult("Cuộc trò chuyện không tồn tại");
                }

                conversation.IsClosed = dto.IsClosed;
                await _context.SaveChangesAsync();

                return ApiResponse<ConversationDTO>.SuccessResult(MapToConversationDTO(conversation), "Cập nhật trạng thái cuộc trò chuyện thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái cuộc trò chuyện");
                return ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra khi cập nhật trạng thái cuộc trò chuyện");
            }
        }

        public async Task<ApiResponse<bool>> DeleteConversationAsync(int tenantId, long conversationId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.TenantId == tenantId);

                if (conversation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Cuộc trò chuyện không tồn tại");
                }

                // Xóa tất cả tin nhắn trước
                _context.Messages.RemoveRange(conversation.Messages);
                
                // Xóa cuộc trò chuyện
                _context.Conversations.Remove(conversation);
                
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa cuộc trò chuyện thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa cuộc trò chuyện");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa cuộc trò chuyện");
            }
        }

        public async Task<ApiResponse<MessageDTO>> SendMessageAsync(int tenantId, SendMessageDTO dto, int? senderUserId = null, int? senderPatientId = null)
        {
            try
            {
                // Kiểm tra conversation có tồn tại không
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.ConversationId == dto.ConversationId && c.TenantId == tenantId);

                if (conversation == null)
                {
                    return ApiResponse<MessageDTO>.ErrorResult("Cuộc trò chuyện không tồn tại");
                }

                if (conversation.IsClosed)
                {
                    return ApiResponse<MessageDTO>.ErrorResult("Cuộc trò chuyện đã đóng");
                }

                // Kiểm tra có content hoặc attachment
                if (string.IsNullOrWhiteSpace(dto.Content) && dto.Attachment == null)
                {
                    return ApiResponse<MessageDTO>.ErrorResult("Tin nhắn phải có nội dung hoặc tệp đính kèm");
                }

                string? attachmentUrl = null;
                
                // Xử lý upload file nếu có
                if (dto.Attachment != null)
                {
                    var uploadResult = await UploadAttachmentAsync(dto.Attachment);
                    if (!uploadResult.Success)
                    {
                        return ApiResponse<MessageDTO>.ErrorResult(uploadResult.Message ?? "Lỗi upload file");
                    }
                    attachmentUrl = uploadResult.Data;
                }

                // Tạo tin nhắn mới
                var message = new Message
                {
                    ConversationId = dto.ConversationId,
                    SenderUserId = senderUserId,
                    SenderPatientId = senderPatientId,
                    Content = dto.Content,
                    AttachmentUrl = attachmentUrl,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Load lại với includes
                message = await _context.Messages
                    .Include(m => m.SenderUser)
                    .Include(m => m.SenderPatient)
                    .FirstAsync(m => m.MessageId == message.MessageId);

                return ApiResponse<MessageDTO>.SuccessResult(MapToMessageDTO(message), "Gửi tin nhắn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi tin nhắn");
                return ApiResponse<MessageDTO>.ErrorResult("Có lỗi xảy ra khi gửi tin nhắn");
            }
        }

        public async Task<ApiResponse<MessageListResponseDTO>> GetMessagesAsync(int tenantId, long conversationId, MessageQueryDTO query)
        {
            try
            {
                // Kiểm tra conversation có tồn tại không
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.TenantId == tenantId);

                if (conversation == null)
                {
                    return ApiResponse<MessageListResponseDTO>.ErrorResult("Cuộc trò chuyện không tồn tại");
                }

                var messageQuery = _context.Messages
                    .Include(m => m.SenderUser)
                    .Include(m => m.SenderPatient)
                    .Where(m => m.ConversationId == conversationId);

                if (query.FromDate.HasValue)
                    messageQuery = messageQuery.Where(m => m.SentAt >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    messageQuery = messageQuery.Where(m => m.SentAt <= query.ToDate.Value);

                var totalCount = await messageQuery.CountAsync();
                
                var messages = await messageQuery
                    .OrderByDescending(m => m.SentAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var result = new MessageListResponseDTO
                {
                    Messages = messages.Select(MapToMessageDTO).ToList(),
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    HasNextPage = (query.Page * query.PageSize) < totalCount,
                    HasPreviousPage = query.Page > 1
                };

                return ApiResponse<MessageListResponseDTO>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tin nhắn");
                return ApiResponse<MessageListResponseDTO>.ErrorResult("Có lỗi xảy ra khi lấy danh sách tin nhắn");
            }
        }

        public async Task<ApiResponse<bool>> DeleteMessageAsync(int tenantId, long messageId)
        {
            try
            {
                var message = await _context.Messages
                    .Include(m => m.Conversation)
                    .FirstOrDefaultAsync(m => m.MessageId == messageId && m.Conversation.TenantId == tenantId);

                if (message == null)
                {
                    return ApiResponse<bool>.ErrorResult("Tin nhắn không tồn tại");
                }

                // Xóa file đính kèm nếu có
                if (!string.IsNullOrEmpty(message.AttachmentUrl))
                {
                    await DeleteAttachmentAsync(message.AttachmentUrl);
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa tin nhắn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tin nhắn");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa tin nhắn");
            }
        }

        public async Task<ApiResponse<ChatStatsDTO>> GetChatStatsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var conversationQuery = _context.Conversations.Where(c => c.TenantId == tenantId);
                var messageQuery = _context.Messages
                    .Include(m => m.Conversation)
                    .Where(m => m.Conversation.TenantId == tenantId);

                if (fromDate.HasValue)
                {
                    conversationQuery = conversationQuery.Where(c => c.CreatedAt >= fromDate.Value);
                    messageQuery = messageQuery.Where(m => m.SentAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    conversationQuery = conversationQuery.Where(c => c.CreatedAt <= toDate.Value);
                    messageQuery = messageQuery.Where(m => m.SentAt <= toDate.Value);
                }

                var totalConversations = await conversationQuery.CountAsync();
                var activeConversations = await conversationQuery.CountAsync(c => !c.IsClosed);
                var closedConversations = totalConversations - activeConversations;
                var totalMessages = await messageQuery.CountAsync();
                
                var today = DateTime.UtcNow.Date;
                var messagesToday = await messageQuery.CountAsync(m => m.SentAt.Date == today);

                var stats = new ChatStatsDTO
                {
                    TotalConversations = totalConversations,
                    ActiveConversations = activeConversations,
                    ClosedConversations = closedConversations,
                    TotalMessages = totalMessages,
                    MessagesToday = messagesToday,
                    AverageResponseTime = 15.0
                };

                return ApiResponse<ChatStatsDTO>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê chat");
                return ApiResponse<ChatStatsDTO>.ErrorResult("Có lỗi xảy ra khi lấy thống kê chat");
            }
        }

        public async Task<bool> IsUserAuthorizedForConversationAsync(int tenantId, long conversationId, int userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.TenantId == tenantId);
            
            return conversation != null;
        }

        public async Task<bool> IsPatientAuthorizedForConversationAsync(long conversationId, int patientId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.PatientId == patientId);
            
            return conversation != null;
        }

        #region Private Methods

        private ConversationDTO MapToConversationDTO(Conversation conversation)
        {
            var lastMessage = conversation.Messages?.FirstOrDefault();
            return new ConversationDTO
            {
                ConversationId = conversation.ConversationId,
                TenantId = conversation.TenantId,
                PatientId = conversation.PatientId,
                PatientName = conversation.Patient?.FullName ?? "",
                CreatedByUserId = conversation.CreatedByUserId,
                CreatedByUserName = conversation.CreatedByUser?.FullName ?? "",
                CreatedAt = conversation.CreatedAt,
                IsClosed = conversation.IsClosed,
                LastMessage = lastMessage != null ? MapToMessageDTO(lastMessage) : null,
                UnreadCount = 0
            };
        }

        private MessageDTO MapToMessageDTO(Message message)
        {
            return new MessageDTO
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                SenderUserName = message.SenderUser?.FullName,
                SenderPatientId = message.SenderPatientId,
                SenderPatientName = message.SenderPatient?.FullName,
                Content = message.Content,
                AttachmentUrl = message.AttachmentUrl,
                SentAt = message.SentAt,
                IsFromPatient = message.SenderPatientId.HasValue
            };
        }

        private async Task<(bool Success, string? Data, string? Message)> UploadAttachmentAsync(IFormFile file)
        {
            try
            {
                // Kiểm tra loại file
                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    return (false, null, "Loại file không được hỗ trợ");
                }

                // Kiểm tra kích thước (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return (false, null, "File quá lớn (tối đa 10MB)");
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "chat-attachments");
                
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/chat-attachments/{fileName}";
                return (true, fileUrl, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file");
                return (false, null, "Có lỗi xảy ra khi upload file");
            }
        }

        private async Task DeleteAttachmentAsync(string attachmentUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(attachmentUrl)) return;

                var fileName = Path.GetFileName(attachmentUrl);
                var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "chat-attachments", fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa file đính kèm: {AttachmentUrl}", attachmentUrl);
            }
        }

        #endregion
    }
}