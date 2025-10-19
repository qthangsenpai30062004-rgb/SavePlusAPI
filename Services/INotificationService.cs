using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Tạo notification mới
        /// </summary>
        Task<ApiResponse<NotificationDto>> CreateNotificationAsync(NotificationCreateDto dto);

        /// <summary>
        /// Lấy notification theo ID
        /// </summary>
        Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(long notificationId);

        /// <summary>
        /// Cập nhật notification
        /// </summary>
        Task<ApiResponse<NotificationDto>> UpdateNotificationAsync(long notificationId, NotificationUpdateDto dto);

        /// <summary>
        /// Xóa notification
        /// </summary>
        Task<ApiResponse<bool>> DeleteNotificationAsync(long notificationId);

        /// <summary>
        /// Lấy danh sách notification với filter và phân trang
        /// </summary>
        Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(NotificationFilterDto filter);

        /// <summary>
        /// Lấy danh sách notification của user
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, int? tenantId = null, bool? unreadOnly = null);

        /// <summary>
        /// Lấy danh sách notification của patient
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> GetPatientNotificationsAsync(int patientId, int? tenantId = null, bool? unreadOnly = null);

        /// <summary>
        /// Lấy danh sách notification của tenant
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> GetTenantNotificationsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId);

        /// <summary>
        /// Đánh dấu nhiều notification đã đọc
        /// </summary>
        Task<ApiResponse<bool>> MarkMultipleAsReadAsync(MarkAsReadDto dto);

        /// <summary>
        /// Đánh dấu tất cả notification của user đã đọc
        /// </summary>
        Task<ApiResponse<bool>> MarkAllAsReadAsync(int? userId = null, int? patientId = null, int? tenantId = null);

        /// <summary>
        /// Gửi notification hàng loạt
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> SendBulkNotificationAsync(BulkNotificationDto dto);

        /// <summary>
        /// Lấy báo cáo notification
        /// </summary>
        Task<ApiResponse<NotificationReportDto>> GetNotificationReportAsync(int? tenantId = null, int? userId = null, int? patientId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Lấy số lượng notification chưa đọc
        /// </summary>
        Task<ApiResponse<int>> GetUnreadCountAsync(int? userId = null, int? patientId = null, int? tenantId = null);

        /// <summary>
        /// Tìm kiếm notification theo từ khóa
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> SearchNotificationsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Lấy thống kê notification của user
        /// </summary>
        Task<ApiResponse<UserNotificationSummaryDto>> GetUserNotificationSummaryAsync(int? userId = null, int? patientId = null, int? tenantId = null);

        /// <summary>
        /// Gửi notification từ template
        /// </summary>
        Task<ApiResponse<NotificationDto>> SendNotificationFromTemplateAsync(NotificationTemplateDto template, int? userId = null, int? patientId = null, int tenantId = 1);

        /// <summary>
        /// Lấy danh sách notification theo kênh
        /// </summary>
        Task<ApiResponse<List<NotificationDto>>> GetNotificationsByChannelAsync(string channel, int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Xóa notification cũ (cleanup)
        /// </summary>
        Task<ApiResponse<int>> CleanupOldNotificationsAsync(int daysOld = 30, int? tenantId = null);

        /// <summary>
        /// Lấy danh sách kênh notification được sử dụng
        /// </summary>
        Task<ApiResponse<List<string>>> GetUsedChannelsAsync(int? tenantId = null);

        /// <summary>
        /// Gửi notification nhắc nhở cuộc hẹn
        /// </summary>
        Task<ApiResponse<NotificationDto>> SendAppointmentReminderAsync(int appointmentId, int hoursBeforeAppointment = 24);

        /// <summary>
        /// Gửi notification kết quả xét nghiệm
        /// </summary>
        Task<ApiResponse<NotificationDto>> SendTestResultNotificationAsync(int patientId, string testName, string result, int tenantId);
    }
}
