using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using SavePlus_API.Attributes;
using SavePlus_API.Constants;
using System.Security.Claims;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(IConversationService conversationService, ILogger<ConversationsController> logger)
        {
            _conversationService = conversationService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo cuộc trò chuyện mới
        /// </summary>
        [HttpPost]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ConversationDTO>>> CreateConversation([FromBody] CreateConversationDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var result = await _conversationService.CreateConversationAsync(tenantId, dto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cuộc trò chuyện");
                return StatusCode(500, ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách cuộc trò chuyện
        /// </summary>
        [HttpGet]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<List<ConversationListDTO>>>> GetConversations(
            [FromQuery] int? patientId = null,
            [FromQuery] bool? isClosed = null)
        {
            try
            {
                var tenantId = GetTenantId();

                var result = await _conversationService.GetConversationsAsync(tenantId, patientId, isClosed);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện");
                return StatusCode(500, ApiResponse<List<ConversationListDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết cuộc trò chuyện
        /// </summary>
        [HttpGet("{conversationId}")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ConversationDTO>>> GetConversation(long conversationId)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Kiểm tra quyền truy cập
                var isAuthorized = await _conversationService.IsUserAuthorizedForConversationAsync(tenantId, conversationId, userId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                var result = await _conversationService.GetConversationByIdAsync(tenantId, conversationId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin cuộc trò chuyện");
                return StatusCode(500, ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái cuộc trò chuyện (đóng/mở)
        /// </summary>
        [HttpPut("{conversationId}/status")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ConversationDTO>>> UpdateConversationStatus(
            long conversationId, 
            [FromBody] UpdateConversationStatusDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Kiểm tra quyền truy cập
                var isAuthorized = await _conversationService.IsUserAuthorizedForConversationAsync(tenantId, conversationId, userId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                var result = await _conversationService.UpdateConversationStatusAsync(tenantId, conversationId, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái cuộc trò chuyện");
                return StatusCode(500, ApiResponse<ConversationDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Xóa cuộc trò chuyện
        /// </summary>
        [HttpDelete("{conversationId}")]
        [RequireRole(UserRoles.SystemAdmin, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteConversation(long conversationId)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Kiểm tra quyền truy cập
                var isAuthorized = await _conversationService.IsUserAuthorizedForConversationAsync(tenantId, conversationId, userId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                var result = await _conversationService.DeleteConversationAsync(tenantId, conversationId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa cuộc trò chuyện");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Gửi tin nhắn (từ nhân viên y tế)
        /// </summary>
        [HttpPost("{conversationId}/messages")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<MessageDTO>>> SendMessage(
            long conversationId,
            [FromForm] SendMessageDTO dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Kiểm tra quyền truy cập
                var isAuthorized = await _conversationService.IsUserAuthorizedForConversationAsync(tenantId, conversationId, userId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                // Set conversationId từ route
                dto.ConversationId = conversationId;

                var result = await _conversationService.SendMessageAsync(tenantId, dto, senderUserId: userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi tin nhắn");
                return StatusCode(500, ApiResponse<MessageDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách tin nhắn trong cuộc trò chuyện
        /// </summary>
        [HttpGet("{conversationId}/messages")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<MessageListResponseDTO>>> GetMessages(
            long conversationId,
            [FromQuery] MessageQueryDTO query)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Kiểm tra quyền truy cập
                var isAuthorized = await _conversationService.IsUserAuthorizedForConversationAsync(tenantId, conversationId, userId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                var result = await _conversationService.GetMessagesAsync(tenantId, conversationId, query);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tin nhắn");
                return StatusCode(500, ApiResponse<MessageListResponseDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Xóa tin nhắn
        /// </summary>
        [HttpDelete("messages/{messageId}")]
        [RequireRole(UserRoles.SystemAdmin, UserRoles.ClinicAdmin)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMessage(long messageId)
        {
            try
            {
                var tenantId = GetTenantId();

                var result = await _conversationService.DeleteMessageAsync(tenantId, messageId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tin nhắn");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy thống kê chat
        /// </summary>
        [HttpGet("stats")]
        [RequireRole(UserRoles.Doctor, UserRoles.ClinicAdmin, UserRoles.Nurse, UserRoles.Receptionist)]
        public async Task<ActionResult<ApiResponse<ChatStatsDTO>>> GetChatStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();

                var result = await _conversationService.GetChatStatsAsync(tenantId, fromDate, toDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê chat");
                return StatusCode(500, ApiResponse<ChatStatsDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        #region Patient APIs

        /// <summary>
        /// Gửi tin nhắn (từ bệnh nhân)
        /// </summary>
        [HttpPost("patient/{patientId}/messages")]
        public async Task<ActionResult<ApiResponse<MessageDTO>>> SendPatientMessage(
            int patientId,
            [FromForm] SendPatientMessageDTO dto)
        {
            try
            {
                // Kiểm tra quyền truy cập bệnh nhân
                var isAuthorized = await _conversationService.IsPatientAuthorizedForConversationAsync(dto.ConversationId, patientId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                var sendDto = new SendMessageDTO
                {
                    ConversationId = dto.ConversationId,
                    Content = dto.Content,
                    Attachment = dto.Attachment
                };

                // Lấy tenantId từ conversation
                var conversation = await _conversationService.GetConversationByIdAsync(0, dto.ConversationId);
                if (!conversation.Success)
                {
                    return BadRequest(conversation.Message);
                }

                var result = await _conversationService.SendMessageAsync(conversation.Data!.TenantId, sendDto, senderPatientId: patientId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bệnh nhân gửi tin nhắn");
                return StatusCode(500, ApiResponse<MessageDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy danh sách cuộc trò chuyện của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<ConversationListDTO>>>> GetPatientConversations(int patientId)
        {
            try
            {
                // Tạm thời lấy tất cả tenant (có thể cải thiện sau)
                var result = await _conversationService.GetConversationsAsync(0, patientId, false);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện của bệnh nhân");
                return StatusCode(500, ApiResponse<List<ConversationListDTO>>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        /// <summary>
        /// Lấy tin nhắn trong cuộc trò chuyện (cho bệnh nhân)
        /// </summary>
        [HttpGet("patient/{patientId}/conversations/{conversationId}/messages")]
        public async Task<ActionResult<ApiResponse<MessageListResponseDTO>>> GetPatientMessages(
            int patientId,
            long conversationId,
            [FromQuery] MessageQueryDTO query)
        {
            try
            {
                // Kiểm tra quyền truy cập bệnh nhân
                var isAuthorized = await _conversationService.IsPatientAuthorizedForConversationAsync(conversationId, patientId);
                if (!isAuthorized)
                {
                    return Forbid();
                }

                // Lấy tenantId từ conversation
                var conversation = await _conversationService.GetConversationByIdAsync(0, conversationId);
                if (!conversation.Success)
                {
                    return BadRequest(conversation.Message);
                }

                var result = await _conversationService.GetMessagesAsync(conversation.Data!.TenantId, conversationId, query);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bệnh nhân lấy danh sách tin nhắn");
                return StatusCode(500, ApiResponse<MessageListResponseDTO>.ErrorResult("Có lỗi xảy ra trên server"));
            }
        }

        #endregion

        #region Helper Methods

        private int GetTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            return int.Parse(tenantIdClaim ?? "0");
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        #endregion
    }

    // DTO cho bệnh nhân gửi tin nhắn
    public class SendPatientMessageDTO
    {
        public long ConversationId { get; set; }
        public string? Content { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}