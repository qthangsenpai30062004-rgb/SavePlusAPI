using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using SavePlus_API.Constants;
using System.Security.Claims;

namespace SavePlus_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;
        private readonly ILogger<MeasurementsController> _logger;

        public MeasurementsController(IMeasurementService measurementService, ILogger<MeasurementsController> logger)
        {
            _measurementService = measurementService;
            _logger = logger;
        }

        private int GetTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (int.TryParse(tenantIdClaim, out int tenantId))
                return tenantId;
            throw new UnauthorizedAccessException("Không tìm thấy thông tin tenant");
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            throw new UnauthorizedAccessException("Không tìm thấy thông tin user");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /// <summary>
        /// Tạo số liệu đo lường mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MeasurementDto>>> CreateMeasurement([FromBody] MeasurementCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only patients and medical staff can create measurements
                if (userRole != UserRoles.Patient && !UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _measurementService.CreateMeasurementAsync(dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<MeasurementDto>.SuccessResult(result.Data!, "Tạo số liệu đo lường thành công"));
                }

                return BadRequest(ApiResponse<MeasurementDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating measurement");
                return StatusCode(500, ApiResponse<MeasurementDto>.ErrorResult("Lỗi server khi tạo số liệu đo lường"));
            }
        }

        /// <summary>
        /// Lấy thông tin số liệu đo lường theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MeasurementDto>>> GetMeasurement(long id)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _measurementService.GetMeasurementByIdAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<MeasurementDto>.SuccessResult(result.Data!, "Lấy thông tin số liệu đo lường thành công"));
                }

                return NotFound(ApiResponse<MeasurementDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement {MeasurementId}", id);
                return StatusCode(500, ApiResponse<MeasurementDto>.ErrorResult("Lỗi server khi lấy thông tin số liệu đo lường"));
            }
        }

        /// <summary>
        /// Cập nhật số liệu đo lường
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MeasurementDto>>> UpdateMeasurement(long id, [FromBody] MeasurementUpdateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (userRole != UserRoles.Patient && !UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _measurementService.UpdateMeasurementAsync(id, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<MeasurementDto>.SuccessResult(result.Data!, "Cập nhật số liệu đo lường thành công"));
                }

                return BadRequest(ApiResponse<MeasurementDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating measurement {MeasurementId}", id);
                return StatusCode(500, ApiResponse<MeasurementDto>.ErrorResult("Lỗi server khi cập nhật số liệu đo lường"));
            }
        }

        /// <summary>
        /// Xóa số liệu đo lường
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMeasurement(long id)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only high-level staff can delete measurements
                if (!new[] { UserRoles.SystemAdmin, UserRoles.ClinicAdmin, UserRoles.Doctor }.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _measurementService.DeleteMeasurementAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa số liệu đo lường thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting measurement {MeasurementId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi xóa số liệu đo lường"));
            }
        }

        /// <summary>
        /// Lấy danh sách số liệu đo lường (có phân trang và filter)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<MeasurementDto>>>> GetMeasurements(
            [FromQuery] int? patientId = null,
            [FromQuery] int? carePlanId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? source = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "MeasuredAt",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var tenantId = GetTenantId();
                var query = new MeasurementQueryDto
                {
                    PatientId = patientId,
                    CarePlanId = carePlanId,
                    Type = type,
                    Source = source,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var result = await _measurementService.GetMeasurementsAsync(tenantId, query);

                if (result.Success)
                {
                    return Ok(ApiResponse<PagedResult<MeasurementDto>>.SuccessResult(result.Data!, "Lấy danh sách số liệu đo lường thành công"));
                }

                return BadRequest(ApiResponse<PagedResult<MeasurementDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurements");
                return StatusCode(500, ApiResponse<PagedResult<MeasurementDto>>.ErrorResult("Lỗi server khi lấy danh sách số liệu đo lường"));
            }
        }

        /// <summary>
        /// Lấy số liệu đo lường của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<MeasurementDto>>>> GetPatientMeasurements(
            int patientId,
            [FromQuery] string? type = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? limit = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _measurementService.GetPatientMeasurementsAsync(patientId, tenantId, type, fromDate, toDate, limit);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<MeasurementDto>>.SuccessResult(result.Data!, "Lấy số liệu đo lường của bệnh nhân thành công"));
                }

                return BadRequest(ApiResponse<List<MeasurementDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient measurements {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<MeasurementDto>>.ErrorResult("Lỗi server khi lấy số liệu đo lường của bệnh nhân"));
            }
        }

        /// <summary>
        /// Lấy số liệu đo lường gần đây của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/recent")]
        public async Task<ActionResult<ApiResponse<List<MeasurementDto>>>> GetRecentMeasurements(
            int patientId,
            [FromQuery] int days = 7)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _measurementService.GetRecentMeasurementsAsync(patientId, tenantId, days);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<MeasurementDto>>.SuccessResult(result.Data!, "Lấy số liệu đo lường gần đây thành công"));
                }

                return BadRequest(ApiResponse<List<MeasurementDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent measurements for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<MeasurementDto>>.ErrorResult("Lỗi server khi lấy số liệu đo lường gần đây"));
            }
        }

        /// <summary>
        /// Lấy thống kê số liệu đo lường của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/stats")]
        public async Task<ActionResult<ApiResponse<List<MeasurementStatsDto>>>> GetMeasurementStats(
            int patientId,
            [FromQuery] string? type = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _measurementService.GetMeasurementStatsAsync(patientId, tenantId, type, fromDate, toDate);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<MeasurementStatsDto>>.SuccessResult(result.Data!, "Lấy thống kê số liệu đo lường thành công"));
                }

                return BadRequest(ApiResponse<List<MeasurementStatsDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement stats for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<MeasurementStatsDto>>.ErrorResult("Lỗi server khi lấy thống kê số liệu đo lường"));
            }
        }

        /// <summary>
        /// Lấy thống kê số liệu đo lường theo loại
        /// </summary>
        [HttpGet("patient/{patientId}/stats/{type}")]
        public async Task<ActionResult<ApiResponse<MeasurementStatsDto>>> GetMeasurementStatsByType(
            int patientId,
            string type,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _measurementService.GetMeasurementStatsByTypeAsync(patientId, tenantId, type, fromDate, toDate);

                if (result.Success)
                {
                    return Ok(ApiResponse<MeasurementStatsDto>.SuccessResult(result.Data!, "Lấy thống kê số liệu đo lường theo loại thành công"));
                }

                return BadRequest(ApiResponse<MeasurementStatsDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement stats by type for patient {PatientId}, type {Type}", patientId, type);
                return StatusCode(500, ApiResponse<MeasurementStatsDto>.ErrorResult("Lỗi server khi lấy thống kê số liệu đo lường theo loại"));
            }
        }

        /// <summary>
        /// Lấy danh sách loại đo lường có sẵn
        /// </summary>
        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<object>>> GetAvailableMeasurementTypes(
            [FromQuery] int? patientId = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var availableTypesResult = await _measurementService.GetAvailableMeasurementTypesAsync(tenantId, patientId);

                if (!availableTypesResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult(availableTypesResult.ErrorMessage!));
                }

                var response = new
                {
                    predefinedTypes = new
                    {
                        allTypes = MeasurementTypes.AllTypes,
                        typeUnits = MeasurementTypes.TypeUnits,
                        requiresTwoValues = MeasurementTypes.RequiresTwoValues
                    },
                    availableTypes = availableTypesResult.Data,
                    sources = MeasurementSources.AllSources
                };

                return Ok(ApiResponse<object>.SuccessResult(response, "Lấy danh sách loại đo lường thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available measurement types");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi server khi lấy danh sách loại đo lường"));
            }
        }

        /// <summary>
        /// Endpoint cho việc nhập số liệu đo lường nhanh (dành cho app mobile)
        /// </summary>
        [HttpPost("quick")]
        public async Task<ActionResult<ApiResponse<MeasurementDto>>> CreateQuickMeasurement(
            [FromBody] QuickMeasurementDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();
                var userRole = GetUserRole();

                // For patients, set their own ID. For staff, use provided patientId
                var patientId = userRole == UserRoles.Patient ? userId : dto.PatientId;

                var measurementDto = new MeasurementCreateDto
                {
                    PatientId = patientId,
                    Type = dto.Type,
                    Value1 = dto.Value1,
                    Value2 = dto.Value2,
                    Unit = MeasurementTypes.TypeUnits.GetValueOrDefault(dto.Type),
                    Source = userRole == UserRoles.Patient ? MeasurementSources.App : MeasurementSources.Clinic,
                    Notes = dto.Notes
                };

                var result = await _measurementService.CreateMeasurementAsync(measurementDto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<MeasurementDto>.SuccessResult(result.Data!, "Nhập số liệu đo lường nhanh thành công"));
                }

                return BadRequest(ApiResponse<MeasurementDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick measurement");
                return StatusCode(500, ApiResponse<MeasurementDto>.ErrorResult("Lỗi server khi nhập số liệu đo lường nhanh"));
            }
        }
    }

    // DTO for quick measurement input
    public class QuickMeasurementDto
    {
        public int PatientId { get; set; } // Only used by staff
        public string Type { get; set; } = null!;
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public string? Notes { get; set; }
    }
}
