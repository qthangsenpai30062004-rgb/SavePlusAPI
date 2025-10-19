using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<PaymentTransactionService> _logger;

        public PaymentTransactionService(SavePlusDbContext context, ILogger<PaymentTransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentTransactionDto>> CreatePaymentTransactionAsync(PaymentTransactionCreateDto dto)
        {
            try
            {
                // Debug logging
                _logger.LogInformation("Creating payment transaction with Amount: {Amount}, Currency: {Currency}, Method: {Method}", 
                    dto.Amount, dto.Currency, dto.Method);

                // Validate tenant exists
                var tenant = await _context.Tenants.FindAsync(dto.TenantId);
                if (tenant == null)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Phòng khám không tồn tại");
                }

                // Validate patient exists
                var patient = await _context.Patients.FindAsync(dto.PatientId);
                if (patient == null)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Bệnh nhân không tồn tại");
                }

                // Validate appointment if provided
                if (dto.AppointmentId.HasValue)
                {
                    var appointment = await _context.Appointments.FindAsync(dto.AppointmentId.Value);
                    if (appointment == null)
                    {
                        return ApiResponse<PaymentTransactionDto>.ErrorResult("Cuộc hẹn không tồn tại");
                    }
                }

                var paymentTransaction = new PaymentTransaction
                {
                    TenantId = dto.TenantId,
                    PatientId = dto.PatientId,
                    AppointmentId = dto.AppointmentId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    Method = dto.Method,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    ProviderRef = dto.ProviderRef
                };

                _logger.LogInformation("Payment transaction object before save - Amount: {Amount}", paymentTransaction.Amount);

                _context.PaymentTransactions.Add(paymentTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment transaction saved with ID: {PaymentId}, Amount: {Amount}", 
                    paymentTransaction.PaymentId, paymentTransaction.Amount);

                var result = await GetPaymentTransactionByIdAsync(paymentTransaction.PaymentId);
                return ApiResponse<PaymentTransactionDto>.SuccessResult(result.Data!, "Tạo giao dịch thanh toán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo giao dịch thanh toán");
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi tạo giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> GetPaymentTransactionByIdAsync(long paymentId)
        {
            try
            {
                var paymentTransaction = await _context.PaymentTransactions
                    .Include(pt => pt.Tenant)
                    .Include(pt => pt.Patient)
                    .Include(pt => pt.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.User)
                    .FirstOrDefaultAsync(pt => pt.PaymentId == paymentId);

                if (paymentTransaction == null)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Giao dịch thanh toán không tồn tại");
                }

                var result = MapToPaymentTransactionDto(paymentTransaction);
                return ApiResponse<PaymentTransactionDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin giao dịch thanh toán {PaymentId}", paymentId);
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> UpdatePaymentTransactionAsync(long paymentId, PaymentTransactionUpdateDto dto)
        {
            try
            {
                var paymentTransaction = await _context.PaymentTransactions.FindAsync(paymentId);
                if (paymentTransaction == null)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Giao dịch thanh toán không tồn tại");
                }

                paymentTransaction.Status = dto.Status;
                if (!string.IsNullOrEmpty(dto.ProviderRef))
                {
                    paymentTransaction.ProviderRef = dto.ProviderRef;
                }

                await _context.SaveChangesAsync();

                var result = await GetPaymentTransactionByIdAsync(paymentId);
                return ApiResponse<PaymentTransactionDto>.SuccessResult(result.Data!, "Cập nhật giao dịch thanh toán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giao dịch thanh toán {PaymentId}", paymentId);
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi cập nhật giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentTransactionAsync(long paymentId)
        {
            try
            {
                var paymentTransaction = await _context.PaymentTransactions.FindAsync(paymentId);
                if (paymentTransaction == null)
                {
                    return ApiResponse<bool>.ErrorResult("Giao dịch thanh toán không tồn tại");
                }

                // Chỉ cho phép xóa giao dịch có trạng thái PENDING
                if (paymentTransaction.Status != "PENDING")
                {
                    return ApiResponse<bool>.ErrorResult("Chỉ có thể xóa giao dịch có trạng thái PENDING");
                }

                _context.PaymentTransactions.Remove(paymentTransaction);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa giao dịch thanh toán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giao dịch thanh toán {PaymentId}", paymentId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<PagedResult<PaymentTransactionDto>>> GetPaymentTransactionsAsync(PaymentTransactionFilterDto filter)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.Tenant)
                    .Include(pt => pt.Patient)
                    .Include(pt => pt.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.User)
                    .AsQueryable();

                // Apply filters
                if (filter.TenantId.HasValue)
                    query = query.Where(pt => pt.TenantId == filter.TenantId.Value);

                if (filter.PatientId.HasValue)
                    query = query.Where(pt => pt.PatientId == filter.PatientId.Value);

                if (filter.AppointmentId.HasValue)
                    query = query.Where(pt => pt.AppointmentId == filter.AppointmentId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(pt => pt.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Method))
                    query = query.Where(pt => pt.Method == filter.Method);

                if (filter.FromDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= filter.ToDate.Value);

                if (filter.MinAmount.HasValue)
                    query = query.Where(pt => pt.Amount >= filter.MinAmount.Value);

                if (filter.MaxAmount.HasValue)
                    query = query.Where(pt => pt.Amount <= filter.MaxAmount.Value);

                var totalCount = await query.CountAsync();

                var paymentTransactions = await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = new PagedResult<PaymentTransactionDto>
                {
                    Data = paymentTransactions.Select(MapToPaymentTransactionDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResult<PaymentTransactionDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch thanh toán");
                return ApiResponse<PagedResult<PaymentTransactionDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<List<PaymentTransactionDto>>> GetPatientPaymentTransactionsAsync(int patientId, int? tenantId = null)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.Tenant)
                    .Include(pt => pt.Patient)
                    .Include(pt => pt.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.User)
                    .Where(pt => pt.PatientId == patientId);

                if (tenantId.HasValue)
                    query = query.Where(pt => pt.TenantId == tenantId.Value);

                var paymentTransactions = await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();

                var result = paymentTransactions.Select(MapToPaymentTransactionDto).ToList();
                return ApiResponse<List<PaymentTransactionDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch thanh toán của bệnh nhân {PatientId}", patientId);
                return ApiResponse<List<PaymentTransactionDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<List<PaymentTransactionDto>>> GetTenantPaymentTransactionsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.Tenant)
                    .Include(pt => pt.Patient)
                    .Include(pt => pt.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.User)
                    .Where(pt => pt.TenantId == tenantId);

                if (fromDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= toDate.Value);

                var paymentTransactions = await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();

                var result = paymentTransactions.Select(MapToPaymentTransactionDto).ToList();
                return ApiResponse<List<PaymentTransactionDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch thanh toán của phòng khám {TenantId}", tenantId);
                return ApiResponse<List<PaymentTransactionDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<List<PaymentTransactionDto>>> GetAppointmentPaymentTransactionsAsync(int appointmentId)
        {
            try
            {
                var paymentTransactions = await _context.PaymentTransactions
                    .Include(pt => pt.Tenant)
                    .Include(pt => pt.Patient)
                    .Include(pt => pt.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.User)
                    .Where(pt => pt.AppointmentId == appointmentId)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();

                var result = paymentTransactions.Select(MapToPaymentTransactionDto).ToList();
                return ApiResponse<List<PaymentTransactionDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch thanh toán của cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<List<PaymentTransactionDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách giao dịch thanh toán");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> ProcessPaymentAsync(PaymentTransactionCreateDto dto)
        {
            try
            {
                // Create the payment transaction
                var createResult = await CreatePaymentTransactionAsync(dto);
                if (!createResult.Success)
                    return createResult;

                // Here you would integrate with actual payment providers
                // For now, we'll simulate payment processing based on method
                var paymentTransaction = createResult.Data!;
                
                // Simulate different payment methods
                switch (dto.Method.ToUpper())
                {
                    case "CASH":
                        // Cash payments are immediately completed
                        return await CompletePaymentAsync(paymentTransaction.PaymentId, "CASH_PAYMENT");
                    
                    case "CARD":
                    case "BANK_TRANSFER":
                    case "MOMO":
                    case "ZALOPAY":
                        // Electronic payments remain pending until confirmed
                        return createResult;
                    
                    default:
                        return await FailPaymentAsync(paymentTransaction.PaymentId, "Unsupported payment method");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán");
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi xử lý thanh toán");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> CompletePaymentAsync(long paymentId, string? providerRef = null)
        {
            try
            {
                var updateDto = new PaymentTransactionUpdateDto
                {
                    Status = "COMPLETED",
                    ProviderRef = providerRef
                };

                return await UpdatePaymentTransactionAsync(paymentId, updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành thanh toán {PaymentId}", paymentId);
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi hoàn thành thanh toán");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> FailPaymentAsync(long paymentId, string? reason = null)
        {
            try
            {
                var updateDto = new PaymentTransactionUpdateDto
                {
                    Status = "FAILED",
                    ProviderRef = reason
                };

                return await UpdatePaymentTransactionAsync(paymentId, updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thanh toán thất bại {PaymentId}", paymentId);
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi đánh dấu thanh toán thất bại");
            }
        }

        public async Task<ApiResponse<PaymentTransactionDto>> RefundPaymentAsync(long paymentId, RefundRequestDto refundRequest)
        {
            try
            {
                var paymentTransaction = await _context.PaymentTransactions.FindAsync(paymentId);
                if (paymentTransaction == null)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Giao dịch thanh toán không tồn tại");
                }

                if (paymentTransaction.Status != "COMPLETED")
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Chỉ có thể hoàn tiền cho giao dịch đã hoàn thành");
                }

                // Create refund transaction
                var refundAmount = refundRequest.RefundAmount ?? paymentTransaction.Amount;
                if (refundAmount > paymentTransaction.Amount)
                {
                    return ApiResponse<PaymentTransactionDto>.ErrorResult("Số tiền hoàn không được vượt quá số tiền gốc");
                }

                var refundTransaction = new PaymentTransaction
                {
                    TenantId = paymentTransaction.TenantId,
                    PatientId = paymentTransaction.PatientId,
                    AppointmentId = paymentTransaction.AppointmentId,
                    Amount = -refundAmount, // Negative amount for refund
                    Currency = paymentTransaction.Currency,
                    Method = paymentTransaction.Method,
                    Status = "REFUNDED",
                    CreatedAt = DateTime.UtcNow,
                    ProviderRef = $"REFUND_{paymentId}_{refundRequest.Reason}"
                };

                _context.PaymentTransactions.Add(refundTransaction);

                // Update original transaction status if fully refunded
                if (refundAmount == paymentTransaction.Amount)
                {
                    paymentTransaction.Status = "REFUNDED";
                }

                await _context.SaveChangesAsync();

                var result = await GetPaymentTransactionByIdAsync(refundTransaction.PaymentId);
                return ApiResponse<PaymentTransactionDto>.SuccessResult(result.Data!, "Hoàn tiền thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn tiền {PaymentId}", paymentId);
                return ApiResponse<PaymentTransactionDto>.ErrorResult("Có lỗi xảy ra khi hoàn tiền");
            }
        }

        public async Task<ApiResponse<PaymentStatisticsDto>> GetPaymentStatisticsAsync(int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PaymentTransactions.AsQueryable();

                if (tenantId.HasValue)
                    query = query.Where(pt => pt.TenantId == tenantId.Value);

                if (fromDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= toDate.Value);

                var transactions = await query.ToListAsync();

                var statistics = new PaymentStatisticsDto
                {
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(pt => pt.Amount),
                    CompletedAmount = transactions.Where(pt => pt.Status == "COMPLETED").Sum(pt => pt.Amount),
                    PendingAmount = transactions.Where(pt => pt.Status == "PENDING").Sum(pt => pt.Amount),
                    RefundedAmount = transactions.Where(pt => pt.Status == "REFUNDED").Sum(pt => Math.Abs(pt.Amount)),
                    TransactionsByMethod = transactions.GroupBy(pt => pt.Method).ToDictionary(g => g.Key, g => g.Count()),
                    TransactionsByStatus = transactions.GroupBy(pt => pt.Status).ToDictionary(g => g.Key, g => g.Count()),
                    DailySummary = transactions
                        .GroupBy(pt => pt.CreatedAt.Date)
                        .Select(g => new DailyPaymentSummaryDto
                        {
                            Date = g.Key,
                            TransactionCount = g.Count(),
                            TotalAmount = g.Sum(pt => pt.Amount),
                            CompletedAmount = g.Where(pt => pt.Status == "COMPLETED").Sum(pt => pt.Amount)
                        })
                        .OrderBy(d => d.Date)
                        .ToList()
                };

                return ApiResponse<PaymentStatisticsDto>.SuccessResult(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê thanh toán");
                return ApiResponse<PaymentStatisticsDto>.ErrorResult("Có lỗi xảy ra khi lấy thống kê thanh toán");
            }
        }

        public async Task<ApiResponse<List<DailyPaymentSummaryDto>>> GetDailyPaymentSummaryAsync(int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PaymentTransactions.AsQueryable();

                if (tenantId.HasValue)
                    query = query.Where(pt => pt.TenantId == tenantId.Value);

                if (fromDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= toDate.Value);

                var dailySummary = await query
                    .GroupBy(pt => pt.CreatedAt.Date)
                    .Select(g => new DailyPaymentSummaryDto
                    {
                        Date = g.Key,
                        TransactionCount = g.Count(),
                        TotalAmount = g.Sum(pt => pt.Amount),
                        CompletedAmount = g.Where(pt => pt.Status == "COMPLETED").Sum(pt => pt.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                return ApiResponse<List<DailyPaymentSummaryDto>>.SuccessResult(dailySummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng kết thanh toán theo ngày");
                return ApiResponse<List<DailyPaymentSummaryDto>>.ErrorResult("Có lỗi xảy ra khi lấy tổng kết thanh toán theo ngày");
            }
        }

        private static PaymentTransactionDto MapToPaymentTransactionDto(PaymentTransaction paymentTransaction)
        {
            return new PaymentTransactionDto
            {
                PaymentId = paymentTransaction.PaymentId,
                TenantId = paymentTransaction.TenantId,
                TenantName = paymentTransaction.Tenant?.Name,
                PatientId = paymentTransaction.PatientId,
                PatientName = paymentTransaction.Patient?.FullName,
                PatientPhone = paymentTransaction.Patient?.PrimaryPhoneE164,
                AppointmentId = paymentTransaction.AppointmentId,
                Amount = paymentTransaction.Amount,
                Currency = paymentTransaction.Currency,
                Method = paymentTransaction.Method,
                Status = paymentTransaction.Status,
                CreatedAt = paymentTransaction.CreatedAt,
                ProviderRef = paymentTransaction.ProviderRef,
                AppointmentType = paymentTransaction.Appointment?.Type,
                AppointmentDate = paymentTransaction.Appointment?.StartAt,
                DoctorName = paymentTransaction.Appointment?.Doctor?.User?.FullName
            };
        }
    }
}
