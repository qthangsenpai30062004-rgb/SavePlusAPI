using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentTransactionsController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly ILogger<PaymentTransactionsController> _logger;

        public PaymentTransactionsController(IPaymentTransactionService paymentTransactionService, ILogger<PaymentTransactionsController> logger)
        {
            _paymentTransactionService = paymentTransactionService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo giao dịch thanh toán mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> CreatePaymentTransaction([FromBody] PaymentTransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PaymentTransactionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _paymentTransactionService.CreatePaymentTransactionAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetPaymentTransaction), new { id = result.Data?.PaymentId }, result);
        }

        /// <summary>
        /// Lấy thông tin giao dịch thanh toán theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> GetPaymentTransaction(long id)
        {
            var result = await _paymentTransactionService.GetPaymentTransactionByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin giao dịch thanh toán
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> UpdatePaymentTransaction(long id, [FromBody] PaymentTransactionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PaymentTransactionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _paymentTransactionService.UpdatePaymentTransactionAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Xóa giao dịch thanh toán (chỉ cho phép xóa giao dịch PENDING)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePaymentTransaction(long id)
        {
            var result = await _paymentTransactionService.DeletePaymentTransactionAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch thanh toán (có lọc và phân trang)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<PaymentTransactionDto>>>> GetPaymentTransactions(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? appointmentId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? method = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new PaymentTransactionFilterDto
            {
                TenantId = tenantId,
                PatientId = patientId,
                AppointmentId = appointmentId,
                Status = status,
                Method = method,
                FromDate = fromDate,
                ToDate = toDate,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paymentTransactionService.GetPaymentTransactionsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch thanh toán của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<PaymentTransactionDto>>>> GetPatientPaymentTransactions(int patientId, [FromQuery] int? tenantId = null)
        {
            var result = await _paymentTransactionService.GetPatientPaymentTransactionsAsync(patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch thanh toán của phòng khám
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<PaymentTransactionDto>>>> GetTenantPaymentTransactions(
            int tenantId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _paymentTransactionService.GetTenantPaymentTransactionsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch thanh toán của cuộc hẹn
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<ApiResponse<List<PaymentTransactionDto>>>> GetAppointmentPaymentTransactions(int appointmentId)
        {
            var result = await _paymentTransactionService.GetAppointmentPaymentTransactionsAsync(appointmentId);
            return Ok(result);
        }

        /// <summary>
        /// Xử lý thanh toán (tạo và xử lý giao dịch)
        /// </summary>
        [HttpPost("process")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> ProcessPayment([FromBody] PaymentTransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PaymentTransactionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _paymentTransactionService.ProcessPaymentAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Hoàn thành giao dịch thanh toán
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> CompletePayment(long id, [FromBody] CompletePaymentDto? dto = null)
        {
            var result = await _paymentTransactionService.CompletePaymentAsync(id, dto?.ProviderRef);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Đánh dấu giao dịch thanh toán thất bại
        /// </summary>
        [HttpPost("{id}/fail")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> FailPayment(long id, [FromBody] FailPaymentDto? dto = null)
        {
            var result = await _paymentTransactionService.FailPaymentAsync(id, dto?.Reason);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Hoàn tiền giao dịch thanh toán
        /// </summary>
        [HttpPost("{id}/refund")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> RefundPayment(long id, [FromBody] RefundRequestDto refundRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PaymentTransactionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _paymentTransactionService.RefundPaymentAsync(id, refundRequest);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê thanh toán
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<PaymentStatisticsDto>>> GetPaymentStatistics(
            [FromQuery] int? tenantId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _paymentTransactionService.GetPaymentStatisticsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tổng kết thanh toán theo ngày
        /// </summary>
        [HttpGet("daily-summary")]
        public async Task<ActionResult<ApiResponse<List<DailyPaymentSummaryDto>>>> GetDailyPaymentSummary(
            [FromQuery] int? tenantId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _paymentTransactionService.GetDailyPaymentSummaryAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }
    }

    // DTO cho complete payment
    public class CompletePaymentDto
    {
        public string? ProviderRef { get; set; }
    }

    // DTO cho fail payment
    public class FailPaymentDto
    {
        public string? Reason { get; set; }
    }
}
