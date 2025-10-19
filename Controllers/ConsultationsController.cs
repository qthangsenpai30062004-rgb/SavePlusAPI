using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(IConsultationService consultationService, ILogger<ConsultationsController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo consultation mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ConsultationDto>>> CreateConsultation([FromBody] ConsultationCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ConsultationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _consultationService.CreateConsultationAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetConsultation), new { id = result.Data?.ConsultationId }, result);
        }

        /// <summary>
        /// Lấy thông tin consultation theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ConsultationDto>>> GetConsultation(int id)
        {
            var result = await _consultationService.GetConsultationByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin consultation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ConsultationDto>>> UpdateConsultation(int id, [FromBody] ConsultationUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ConsultationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _consultationService.UpdateConsultationAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Xóa consultation
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteConsultation(int id)
        {
            var result = await _consultationService.DeleteConsultationAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách consultation (có lọc và phân trang)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<ConsultationDto>>>> GetConsultations(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? diagnosisCode = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new ConsultationFilterDto
            {
                TenantId = tenantId,
                PatientId = patientId,
                DoctorId = doctorId,
                FromDate = fromDate,
                ToDate = toDate,
                DiagnosisCode = diagnosisCode,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _consultationService.GetConsultationsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy consultation theo appointment ID
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<ApiResponse<ConsultationDto>>> GetConsultationByAppointment(int appointmentId)
        {
            var result = await _consultationService.GetConsultationByAppointmentIdAsync(appointmentId);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách consultation của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<ConsultationDto>>>> GetPatientConsultations(int patientId, [FromQuery] int? tenantId = null)
        {
            var result = await _consultationService.GetPatientConsultationsAsync(patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách consultation của bác sĩ
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<ApiResponse<List<ConsultationDto>>>> GetDoctorConsultations(
            int doctorId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _consultationService.GetDoctorConsultationsAsync(doctorId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách consultation của phòng khám
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<ConsultationDto>>>> GetTenantConsultations(
            int tenantId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _consultationService.GetTenantConsultationsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy báo cáo consultation
        /// </summary>
        [HttpGet("reports")]
        public async Task<ActionResult<ApiResponse<ConsultationReportDto>>> GetConsultationReport(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _consultationService.GetConsultationReportAsync(tenantId, doctorId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm consultation theo từ khóa
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ConsultationDto>>>> SearchConsultations(
            [FromQuery] string keyword,
            [FromQuery] int? tenantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest(ApiResponse<List<ConsultationDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
            }

            var result = await _consultationService.SearchConsultationsAsync(keyword, tenantId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê consultation theo thời gian
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetConsultationStatistics(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "day") // day, week, month, year
        {
            try
            {
                // Lấy báo cáo consultation
                var reportResult = await _consultationService.GetConsultationReportAsync(tenantId, doctorId, fromDate, toDate);
                
                if (!reportResult.Success)
                    return BadRequest(reportResult);

                var report = reportResult.Data!;

                // Tạo thống kê theo thời gian (đơn giản)
                var statistics = new
                {
                    TotalConsultations = report.TotalConsultations,
                    DiagnosisCodes = report.DiagnosisCodes,
                    Period = new
                    {
                        FromDate = report.FromDate,
                        ToDate = report.ToDate
                    },
                    TopDiagnosis = report.DiagnosisCodes
                        .OrderByDescending(x => x.Value)
                        .Take(5)
                        .ToDictionary(x => x.Key, x => x.Value)
                };

                return Ok(ApiResponse<object>.SuccessResult(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê consultation");
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy thống kê"));
            }
        }

        /// <summary>
        /// Lấy consultation gần đây nhất của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/latest")]
        public async Task<ActionResult<ApiResponse<ConsultationDto>>> GetPatientLatestConsultation(int patientId, [FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _consultationService.GetPatientConsultationsAsync(patientId, tenantId);
                
                if (!result.Success)
                    return BadRequest(result);

                var latestConsultation = result.Data?.FirstOrDefault();
                
                if (latestConsultation == null)
                {
                    return NotFound(ApiResponse<ConsultationDto>.ErrorResult("Không tìm thấy consultation nào của bệnh nhân này"));
                }

                return Ok(ApiResponse<ConsultationDto>.SuccessResult(latestConsultation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation gần đây nhất của bệnh nhân: {PatientId}", patientId);
                return BadRequest(ApiResponse<ConsultationDto>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Lấy danh sách mã chẩn đoán được sử dụng
        /// </summary>
        [HttpGet("diagnosis-codes")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetUsedDiagnosisCodes([FromQuery] int? tenantId = null)
        {
            try
            {
                var reportResult = await _consultationService.GetConsultationReportAsync(tenantId);
                
                if (!reportResult.Success)
                    return BadRequest(reportResult);

                var diagnosisCodes = reportResult.Data!.DiagnosisCodes.Keys.ToList();

                return Ok(ApiResponse<List<string>>.SuccessResult(diagnosisCodes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách mã chẩn đoán");
                return BadRequest(ApiResponse<List<string>>.ErrorResult("Có lỗi xảy ra"));
            }
        }
    }
}
