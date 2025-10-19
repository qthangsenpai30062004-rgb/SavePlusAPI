using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IPaymentTransactionService
    {
        // CRUD operations
        Task<ApiResponse<PaymentTransactionDto>> CreatePaymentTransactionAsync(PaymentTransactionCreateDto dto);
        Task<ApiResponse<PaymentTransactionDto>> GetPaymentTransactionByIdAsync(long paymentId);
        Task<ApiResponse<PaymentTransactionDto>> UpdatePaymentTransactionAsync(long paymentId, PaymentTransactionUpdateDto dto);
        Task<ApiResponse<bool>> DeletePaymentTransactionAsync(long paymentId);

        // Query operations
        Task<ApiResponse<PagedResult<PaymentTransactionDto>>> GetPaymentTransactionsAsync(PaymentTransactionFilterDto filter);
        Task<ApiResponse<List<PaymentTransactionDto>>> GetPatientPaymentTransactionsAsync(int patientId, int? tenantId = null);
        Task<ApiResponse<List<PaymentTransactionDto>>> GetTenantPaymentTransactionsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<PaymentTransactionDto>>> GetAppointmentPaymentTransactionsAsync(int appointmentId);

        // Business operations
        Task<ApiResponse<PaymentTransactionDto>> ProcessPaymentAsync(PaymentTransactionCreateDto dto);
        Task<ApiResponse<PaymentTransactionDto>> CompletePaymentAsync(long paymentId, string? providerRef = null);
        Task<ApiResponse<PaymentTransactionDto>> FailPaymentAsync(long paymentId, string? reason = null);
        Task<ApiResponse<PaymentTransactionDto>> RefundPaymentAsync(long paymentId, RefundRequestDto refundRequest);

        // Statistics and reporting
        Task<ApiResponse<PaymentStatisticsDto>> GetPaymentStatisticsAsync(int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<List<DailyPaymentSummaryDto>>> GetDailyPaymentSummaryAsync(int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
