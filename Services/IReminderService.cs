using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IReminderService
    {
        // Basic CRUD operations
        Task<ApiResponse<ReminderDTO>> CreateReminderAsync(int tenantId, CreateReminderDTO dto);
        Task<ApiResponse<ReminderListResponseDTO>> GetRemindersAsync(int tenantId, ReminderQueryDTO query);
        Task<ApiResponse<ReminderDTO>> GetReminderByIdAsync(int tenantId, int reminderId);
        Task<ApiResponse<ReminderDTO>> UpdateReminderAsync(int tenantId, int reminderId, UpdateReminderDTO dto);
        Task<ApiResponse<bool>> DeleteReminderAsync(int tenantId, int reminderId);

        // Reminder scheduling and management
        Task<ApiResponse<ReminderDTO>> SnoozeReminderAsync(int tenantId, int reminderId, SnoozeReminderDTO dto);
        Task<ApiResponse<bool>> ActivateReminderAsync(int tenantId, int reminderId);
        Task<ApiResponse<bool>> DeactivateReminderAsync(int tenantId, int reminderId);

        // Bulk operations
        Task<ApiResponse<int>> BulkReminderActionAsync(int tenantId, BulkReminderActionDTO dto);

        // Fire reminders (for background service)
        Task<ApiResponse<List<FireReminderDTO>>> GetDueRemindersAsync(int tenantId);
        Task<ApiResponse<bool>> MarkReminderAsFiredAsync(int reminderId);

        // Statistics and reporting
        Task<ApiResponse<ReminderStatsDTO>> GetReminderStatsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<ReminderDTO>>> GetUpcomingRemindersAsync(int tenantId, int hours = 24);
        Task<ApiResponse<List<ReminderDTO>>> GetOverdueRemindersAsync(int tenantId);

        // Patient-specific operations
        Task<ApiResponse<List<ReminderDTO>>> GetPatientRemindersAsync(int tenantId, int patientId, bool activeOnly = true);
        Task<ApiResponse<ReminderDTO>> CreatePatientReminderAsync(int tenantId, int patientId, CreateReminderDTO dto);

        // Templates and helpers
        Task<ApiResponse<List<ReminderTemplateDTO>>> GetReminderTemplatesAsync();
        Task<ApiResponse<ReminderDTO>> CreateReminderFromTemplateAsync(int tenantId, string targetType, int targetId, int patientId, int minutesBefore = 30);

        // Utility methods
        Task<bool> IsReminderAccessibleAsync(int tenantId, int reminderId);
        Task<bool> IsPatientReminderAccessibleAsync(int patientId, int reminderId);
    }
}
