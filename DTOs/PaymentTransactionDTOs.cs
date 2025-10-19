using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho hiển thị thông tin giao dịch thanh toán
    public class PaymentTransactionDto
    {
        public long PaymentId { get; set; }
        public int TenantId { get; set; }
        public string? TenantName { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
        public int? AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? ProviderRef { get; set; }
        
        // Thông tin cuộc hẹn (nếu có)
        public string? AppointmentType { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? DoctorName { get; set; }
    }

    // DTO cho tạo giao dịch thanh toán mới
    public class PaymentTransactionCreateDto
    {
        [Required(ErrorMessage = "TenantId là bắt buộc")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "PatientId là bắt buộc")]
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required(ErrorMessage = "Số tiền là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Loại tiền tệ là bắt buộc")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Loại tiền tệ phải có 3 ký tự")]
        public string Currency { get; set; } = "VND";

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        [StringLength(50, ErrorMessage = "Phương thức thanh toán không được vượt quá 50 ký tự")]
        public string Method { get; set; } = null!; // "CASH", "CARD", "BANK_TRANSFER", "MOMO", "ZALOPAY", etc.

        [StringLength(100, ErrorMessage = "Mã tham chiếu không được vượt quá 100 ký tự")]
        public string? ProviderRef { get; set; }
    }

    // DTO cho cập nhật giao dịch thanh toán
    public class PaymentTransactionUpdateDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string Status { get; set; } = null!; // "PENDING", "COMPLETED", "FAILED", "REFUNDED"

        [StringLength(100, ErrorMessage = "Mã tham chiếu không được vượt quá 100 ký tự")]
        public string? ProviderRef { get; set; }
    }

    // DTO cho lọc và tìm kiếm giao dịch thanh toán
    public class PaymentTransactionFilterDto
    {
        public int? TenantId { get; set; }
        public int? PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public string? Status { get; set; }
        public string? Method { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // DTO cho thống kê thanh toán
    public class PaymentStatisticsDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CompletedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public Dictionary<string, int> TransactionsByMethod { get; set; } = new();
        public Dictionary<string, int> TransactionsByStatus { get; set; } = new();
        public List<DailyPaymentSummaryDto> DailySummary { get; set; } = new();
    }

    // DTO cho tổng kết thanh toán theo ngày
    public class DailyPaymentSummaryDto
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CompletedAmount { get; set; }
    }

    // DTO cho hoàn tiền
    public class RefundRequestDto
    {
        [Required(ErrorMessage = "Lý do hoàn tiền là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do hoàn tiền không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền hoàn phải lớn hơn 0")]
        public decimal? RefundAmount { get; set; } // Nếu null thì hoàn toàn bộ
    }
}
