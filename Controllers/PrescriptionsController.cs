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
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(IPrescriptionService prescriptionService, ILogger<PrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
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
        /// Tạo đơn thuốc mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PrescriptionDto>>> CreatePrescription([FromBody] PrescriptionCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can create prescriptions
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền kê đơn thuốc");
                }

                var result = await _prescriptionService.CreatePrescriptionAsync(dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionDto>.SuccessResult(result.Data!, "Tạo đơn thuốc thành công"));
                }

                return BadRequest(ApiResponse<PrescriptionDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                return StatusCode(500, ApiResponse<PrescriptionDto>.ErrorResult("Lỗi server khi tạo đơn thuốc"));
            }
        }

        /// <summary>
        /// Lấy thông tin đơn thuốc theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PrescriptionDto>>> GetPrescription(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetPrescriptionByIdAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionDto>.SuccessResult(result.Data!, "Lấy thông tin đơn thuốc thành công"));
                }

                return NotFound(ApiResponse<PrescriptionDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<PrescriptionDto>.ErrorResult("Lỗi server khi lấy thông tin đơn thuốc"));
            }
        }

        /// <summary>
        /// Cập nhật đơn thuốc
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PrescriptionDto>>> UpdatePrescription(int id, [FromBody] PrescriptionUpdateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can update prescriptions
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền cập nhật đơn thuốc");
                }

                var result = await _prescriptionService.UpdatePrescriptionAsync(id, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionDto>.SuccessResult(result.Data!, "Cập nhật đơn thuốc thành công"));
                }

                return BadRequest(ApiResponse<PrescriptionDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<PrescriptionDto>.ErrorResult("Lỗi server khi cập nhật đơn thuốc"));
            }
        }

        /// <summary>
        /// Xóa đơn thuốc
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePrescription(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only high-level staff can delete prescriptions
                if (!new[] { UserRoles.SystemAdmin, UserRoles.ClinicAdmin, UserRoles.Doctor }.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền xóa đơn thuốc");
                }

                var result = await _prescriptionService.DeletePrescriptionAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa đơn thuốc thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi xóa đơn thuốc"));
            }
        }

        /// <summary>
        /// Lấy danh sách đơn thuốc (có phân trang và filter)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<PrescriptionDto>>>> GetPrescriptions(
            [FromQuery] int? patientId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] int? carePlanId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? drugName = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "IssuedAt",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var tenantId = GetTenantId();
                var query = new PrescriptionQueryDto
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    CarePlanId = carePlanId,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    DrugName = drugName,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var result = await _prescriptionService.GetPrescriptionsAsync(tenantId, query);

                if (result.Success)
                {
                    return Ok(ApiResponse<PagedResult<PrescriptionDto>>.SuccessResult(result.Data!, "Lấy danh sách đơn thuốc thành công"));
                }

                return BadRequest(ApiResponse<PagedResult<PrescriptionDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions");
                return StatusCode(500, ApiResponse<PagedResult<PrescriptionDto>>.ErrorResult("Lỗi server khi lấy danh sách đơn thuốc"));
            }
        }

        /// <summary>
        /// Lấy đơn thuốc của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<PrescriptionDto>>>> GetPatientPrescriptions(
            int patientId,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetPatientPrescriptionsAsync(patientId, tenantId, status, fromDate, toDate);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<PrescriptionDto>>.SuccessResult(result.Data!, "Lấy đơn thuốc của bệnh nhân thành công"));
                }

                return BadRequest(ApiResponse<List<PrescriptionDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient prescriptions {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<PrescriptionDto>>.ErrorResult("Lỗi server khi lấy đơn thuốc của bệnh nhân"));
            }
        }

        /// <summary>
        /// Lấy đơn thuốc đang hoạt động của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/active")]
        public async Task<ActionResult<ApiResponse<List<PrescriptionDto>>>> GetActivePrescriptionsForPatient(int patientId)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetActivePrescriptionsForPatientAsync(patientId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<PrescriptionDto>>.SuccessResult(result.Data!, "Lấy đơn thuốc đang hoạt động thành công"));
                }

                return BadRequest(ApiResponse<List<PrescriptionDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active prescriptions for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<PrescriptionDto>>.ErrorResult("Lỗi server khi lấy đơn thuốc đang hoạt động"));
            }
        }

        /// <summary>
        /// Lấy đơn thuốc của bác sĩ
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<ApiResponse<List<PrescriptionDto>>>> GetDoctorPrescriptions(
            int doctorId,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? limit = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetDoctorPrescriptionsAsync(doctorId, tenantId, status, fromDate, toDate, limit);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<PrescriptionDto>>.SuccessResult(result.Data!, "Lấy đơn thuốc của bác sĩ thành công"));
                }

                return BadRequest(ApiResponse<List<PrescriptionDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor prescriptions {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<List<PrescriptionDto>>.ErrorResult("Lỗi server khi lấy đơn thuốc của bác sĩ"));
            }
        }

        /// <summary>
        /// Thêm thuốc mới vào đơn thuốc
        /// </summary>
        [HttpPost("{prescriptionId}/items")]
        public async Task<ActionResult<ApiResponse<PrescriptionItemDto>>> CreatePrescriptionItem(
            int prescriptionId, [FromBody] PrescriptionItemCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can add prescription items
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền thêm thuốc vào đơn");
                }

                var result = await _prescriptionService.CreatePrescriptionItemAsync(prescriptionId, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionItemDto>.SuccessResult(result.Data!, "Thêm thuốc vào đơn thành công"));
                }

                return BadRequest(ApiResponse<PrescriptionItemDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription item for prescription {PrescriptionId}", prescriptionId);
                return StatusCode(500, ApiResponse<PrescriptionItemDto>.ErrorResult("Lỗi server khi thêm thuốc vào đơn"));
            }
        }

        /// <summary>
        /// Cập nhật thuốc trong đơn thuốc
        /// </summary>
        [HttpPut("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<PrescriptionItemDto>>> UpdatePrescriptionItem(
            int itemId, [FromBody] PrescriptionItemUpdateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can update prescription items
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền cập nhật thuốc trong đơn");
                }

                var result = await _prescriptionService.UpdatePrescriptionItemAsync(itemId, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionItemDto>.SuccessResult(result.Data!, "Cập nhật thuốc trong đơn thành công"));
                }

                return BadRequest(ApiResponse<PrescriptionItemDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<PrescriptionItemDto>.ErrorResult("Lỗi server khi cập nhật thuốc trong đơn"));
            }
        }

        /// <summary>
        /// Xóa thuốc khỏi đơn thuốc
        /// </summary>
        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePrescriptionItem(int itemId)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can delete prescription items
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền xóa thuốc khỏi đơn");
                }

                var result = await _prescriptionService.DeletePrescriptionItemAsync(itemId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa thuốc khỏi đơn thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi xóa thuốc khỏi đơn"));
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc trong đơn thuốc
        /// </summary>
        [HttpGet("{prescriptionId}/items")]
        public async Task<ActionResult<ApiResponse<List<PrescriptionItemDto>>>> GetPrescriptionItems(int prescriptionId)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetPrescriptionItemsAsync(prescriptionId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<PrescriptionItemDto>>.SuccessResult(result.Data!, "Lấy danh sách thuốc trong đơn thành công"));
                }

                return BadRequest(ApiResponse<List<PrescriptionItemDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription items for prescription {PrescriptionId}", prescriptionId);
                return StatusCode(500, ApiResponse<List<PrescriptionItemDto>>.ErrorResult("Lỗi server khi lấy danh sách thuốc trong đơn"));
            }
        }

        /// <summary>
        /// Lấy thuốc được kê nhiều nhất
        /// </summary>
        [HttpGet("popular-drugs")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetMostPrescribedDrugs(
            [FromQuery] int? doctorId = null,
            [FromQuery] int? limit = 20)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.GetMostPrescribedDrugsAsync(tenantId, doctorId, limit);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<string>>.SuccessResult(result.Data!, "Lấy danh sách thuốc được kê nhiều nhất thành công"));
                }

                return BadRequest(ApiResponse<List<string>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most prescribed drugs");
                return StatusCode(500, ApiResponse<List<string>>.ErrorResult("Lỗi server khi lấy danh sách thuốc được kê nhiều nhất"));
            }
        }

        /// <summary>
        /// Lấy thông tin về các dạng thuốc, đường dùng, và tần suất
        /// </summary>
        [HttpGet("metadata")]
        public ActionResult<ApiResponse<object>> GetPrescriptionMetadata()
        {
            try
            {
                var metadata = new
                {
                    statuses = PrescriptionStatuses.AllStatuses,
                    drugForms = DrugForms.AllForms,
                    routes = DrugRoutes.AllRoutes,
                    frequencies = new
                    {
                        codes = DrugFrequencies.FrequencyDescriptions.Keys,
                        descriptions = DrugFrequencies.FrequencyDescriptions
                    }
                };

                return Ok(ApiResponse<object>.SuccessResult(metadata, "Lấy metadata đơn thuốc thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription metadata");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi server khi lấy metadata đơn thuốc"));
            }
        }

        /// <summary>
        /// Kiểm tra quyền kê đơn của bác sĩ
        /// </summary>
        [HttpGet("doctor/{doctorId}/can-prescribe")]
        public async Task<ActionResult<ApiResponse<bool>>> CanDoctorPrescribe(int doctorId)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _prescriptionService.CanDoctorPrescribeAsync(doctorId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(result.Data, "Kiểm tra quyền kê đơn thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if doctor {DoctorId} can prescribe", doctorId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi kiểm tra quyền kê đơn"));
            }
        }

        /// <summary>
        /// Endpoint nhanh cho việc kê đơn thuốc đơn giản
        /// </summary>
        [HttpPost("quick")]
        public async Task<ActionResult<ApiResponse<PrescriptionDto>>> CreateQuickPrescription([FromBody] QuickPrescriptionDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Only doctors and authorized staff can create prescriptions
                if (!UserRoles.PrescriptionRoles.Contains(userRole))
                {
                    return Forbid("Bạn không có quyền kê đơn thuốc");
                }

                var prescriptionDto = new PrescriptionCreateDto
                {
                    PatientId = dto.PatientId,
                    CarePlanId = dto.CarePlanId,
                    DoctorId = dto.DoctorId,
                    Status = PrescriptionStatuses.Active,
                    Items = dto.Medications.Select(med => new PrescriptionItemCreateDto
                    {
                        DrugName = med.DrugName,
                        Form = med.Form,
                        Strength = med.Strength,
                        Dose = med.Dose,
                        Route = med.Route ?? DrugRoutes.Oral,
                        Frequency = med.Frequency,
                        StartDate = med.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
                        EndDate = med.EndDate,
                        Instructions = med.Instructions
                    }).ToList()
                };

                var result = await _prescriptionService.CreatePrescriptionAsync(prescriptionDto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<PrescriptionDto>.SuccessResult(result.Data!, "Tạo đơn thuốc nhanh thành công"));
                }

                return BadRequest(ApiResponse<PrescriptionDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick prescription");
                return StatusCode(500, ApiResponse<PrescriptionDto>.ErrorResult("Lỗi server khi tạo đơn thuốc nhanh"));
            }
        }
    }

    // DTOs for quick prescription input
    public class QuickPrescriptionDto
    {
        public int PatientId { get; set; }
        public int? CarePlanId { get; set; }
        public int DoctorId { get; set; }
        public List<QuickMedicationDto> Medications { get; set; } = new List<QuickMedicationDto>();
    }

    public class QuickMedicationDto
    {
        public string DrugName { get; set; } = null!;
        public string? Form { get; set; }
        public string? Strength { get; set; }
        public string Dose { get; set; } = null!;
        public string? Route { get; set; }
        public string Frequency { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Instructions { get; set; }
    }
}
