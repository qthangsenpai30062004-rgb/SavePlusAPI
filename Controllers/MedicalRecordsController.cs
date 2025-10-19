using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService, ILogger<MedicalRecordsController> logger)
        {
            _medicalRecordService = medicalRecordService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo hồ sơ y tế mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> CreateMedicalRecord([FromBody] MedicalRecordCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _medicalRecordService.CreateMedicalRecordAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetMedicalRecord), new { id = result.Data?.RecordId }, result);
        }

        /// <summary>
        /// Lấy thông tin hồ sơ y tế theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> GetMedicalRecord(long id)
        {
            var result = await _medicalRecordService.GetMedicalRecordByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ y tế
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> UpdateMedicalRecord(long id, [FromBody] MedicalRecordUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _medicalRecordService.UpdateMedicalRecordAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Xóa hồ sơ y tế
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMedicalRecord(long id)
        {
            var result = await _medicalRecordService.DeleteMedicalRecordAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ y tế (có lọc và phân trang)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<MedicalRecordDto>>>> GetMedicalRecords(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? createdByUserId = null,
            [FromQuery] string? recordType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new MedicalRecordFilterDto
            {
                TenantId = tenantId,
                PatientId = patientId,
                CreatedByUserId = createdByUserId,
                RecordType = recordType,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _medicalRecordService.GetMedicalRecordsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ y tế của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> GetPatientMedicalRecords(int patientId, [FromQuery] int? tenantId = null)
        {
            var result = await _medicalRecordService.GetPatientMedicalRecordsAsync(patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ y tế của phòng khám
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> GetTenantMedicalRecords(
            int tenantId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _medicalRecordService.GetTenantMedicalRecordsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ y tế theo loại
        /// </summary>
        [HttpGet("type/{recordType}")]
        public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> GetMedicalRecordsByType(
            string recordType,
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null)
        {
            var result = await _medicalRecordService.GetMedicalRecordsByTypeAsync(recordType, tenantId, patientId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy báo cáo hồ sơ y tế
        /// </summary>
        [HttpGet("reports")]
        public async Task<ActionResult<ApiResponse<MedicalRecordReportDto>>> GetMedicalRecordReport(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _medicalRecordService.GetMedicalRecordReportAsync(tenantId, patientId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm hồ sơ y tế theo từ khóa
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> SearchMedicalRecords(
            [FromQuery] string keyword,
            [FromQuery] int? tenantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest(ApiResponse<List<MedicalRecordDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
            }

            var result = await _medicalRecordService.SearchMedicalRecordsAsync(keyword, tenantId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Upload file và tạo hồ sơ y tế
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> UploadMedicalRecord([FromForm] MedicalRecordUploadDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            // Kiểm tra file
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("File không được để trống"));
            }

            // Kiểm tra kích thước file (giới hạn 10MB)
            if (dto.File.Length > 10 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("File không được vượt quá 10MB"));
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" };
            var fileExtension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResult("Định dạng file không được hỗ trợ. Chỉ chấp nhận: PDF, JPG, PNG, DOC, DOCX, TXT"));
            }

            var result = await _medicalRecordService.UploadMedicalRecordAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetMedicalRecord), new { id = result.Data?.RecordId }, result);
        }

        /// <summary>
        /// Tải file hồ sơ y tế
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadMedicalRecordFile(long id)
        {
            var result = await _medicalRecordService.DownloadMedicalRecordFileAsync(id);
            
            if (!result.Success)
                return BadRequest(result);

            var medicalRecord = await _medicalRecordService.GetMedicalRecordByIdAsync(id);
            if (!medicalRecord.Success)
                return BadRequest(medicalRecord);

            var fileName = Path.GetFileName(medicalRecord.Data!.FileUrl);
            var contentType = GetContentType(fileName);

            return File(result.Data!, contentType, fileName);
        }

        /// <summary>
        /// Lấy thống kê hồ sơ y tế của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/summary")]
        public async Task<ActionResult<ApiResponse<PatientMedicalRecordSummaryDto>>> GetPatientMedicalRecordSummary(int patientId, [FromQuery] int? tenantId = null)
        {
            var result = await _medicalRecordService.GetPatientMedicalRecordSummaryAsync(patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách loại hồ sơ y tế được sử dụng
        /// </summary>
        [HttpGet("record-types")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetUsedRecordTypes([FromQuery] int? tenantId = null)
        {
            var result = await _medicalRecordService.GetUsedRecordTypesAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra quyền truy cập hồ sơ y tế
        /// </summary>
        [HttpGet("{id}/access-check")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckMedicalRecordAccess(long id, [FromQuery] int userId)
        {
            var result = await _medicalRecordService.CheckMedicalRecordAccessAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy hồ sơ y tế gần đây nhất của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/latest")]
        public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> GetLatestPatientMedicalRecord(
            int patientId, 
            [FromQuery] int? tenantId = null, 
            [FromQuery] string? recordType = null)
        {
            var result = await _medicalRecordService.GetLatestPatientMedicalRecordAsync(patientId, tenantId, recordType);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê hồ sơ y tế theo thời gian
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetMedicalRecordStatistics(
            [FromQuery] int? tenantId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "month") // day, week, month, year
        {
            try
            {
                // Lấy báo cáo hồ sơ y tế
                var reportResult = await _medicalRecordService.GetMedicalRecordReportAsync(tenantId, null, fromDate, toDate);
                
                if (!reportResult.Success)
                    return BadRequest(reportResult);

                var report = reportResult.Data!;

                // Tạo thống kê theo thời gian
                var statistics = new
                {
                    TotalRecords = report.TotalRecords,
                    RecordTypes = report.RecordTypes,
                    RecordsByMonth = report.RecordsByMonth,
                    Period = new
                    {
                        FromDate = report.FromDate,
                        ToDate = report.ToDate
                    },
                    TopRecordTypes = report.RecordTypes
                        .OrderByDescending(x => x.Value)
                        .Take(5)
                        .ToDictionary(x => x.Key, x => x.Value),
                    AverageRecordsPerMonth = report.RecordsByMonth.Any() ? 
                        Math.Round((double)report.TotalRecords / report.RecordsByMonth.Count, 2) : 0
                };

                return Ok(ApiResponse<object>.SuccessResult(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê hồ sơ y tế");
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy thống kê"));
            }
        }

        /// <summary>
        /// Lấy danh sách hồ sơ y tế cần xem xét (chưa có loại)
        /// </summary>
        [HttpGet("pending-review")]
        public async Task<ActionResult<ApiResponse<List<MedicalRecordDto>>>> GetPendingReviewMedicalRecords([FromQuery] int? tenantId = null)
        {
            var filter = new MedicalRecordFilterDto
            {
                TenantId = tenantId,
                PageNumber = 1,
                PageSize = 100
            };

            var result = await _medicalRecordService.GetMedicalRecordsAsync(filter);
            
            if (!result.Success)
                return BadRequest(result);

            // Lọc những hồ sơ chưa có loại
            var pendingRecords = result.Data!.Data.Where(mr => string.IsNullOrEmpty(mr.RecordType)).ToList();

            return Ok(ApiResponse<List<MedicalRecordDto>>.SuccessResult(pendingRecords));
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}
