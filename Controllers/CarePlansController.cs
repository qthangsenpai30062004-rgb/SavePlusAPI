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
    public class CarePlansController : ControllerBase
    {
        private readonly ICarePlanService _carePlanService;
        private readonly ILogger<CarePlansController> _logger;

        public CarePlansController(ICarePlanService carePlanService, ILogger<CarePlansController> logger)
        {
            _carePlanService = carePlanService;
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
        /// Tạo kế hoạch chăm sóc mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CarePlanDto>>> CreateCarePlan([FromBody] CarePlanCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var createdBy = GetUserId();
                var userRole = GetUserRole();

                // Check permissions - only medical staff can create care plans
                if (!UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.CreateCarePlanAsync(dto, tenantId, createdBy);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanDto>.SuccessResult(result.Data!, "Tạo kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating care plan");
                return StatusCode(500, ApiResponse<CarePlanDto>.ErrorResult("Lỗi server khi tạo kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy thông tin kế hoạch chăm sóc theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CarePlanDto>>> GetCarePlan(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _carePlanService.GetCarePlanByIdAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanDto>.SuccessResult(result.Data!, "Lấy thông tin kế hoạch chăm sóc thành công"));
                }

                return NotFound(ApiResponse<CarePlanDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting care plan {CarePlanId}", id);
                return StatusCode(500, ApiResponse<CarePlanDto>.ErrorResult("Lỗi server khi lấy thông tin kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Cập nhật kế hoạch chăm sóc
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CarePlanDto>>> UpdateCarePlan(int id, [FromBody] CarePlanUpdateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (!UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.UpdateCarePlanAsync(id, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanDto>.SuccessResult(result.Data!, "Cập nhật kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating care plan {CarePlanId}", id);
                return StatusCode(500, ApiResponse<CarePlanDto>.ErrorResult("Lỗi server khi cập nhật kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Xóa kế hoạch chăm sóc
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCarePlan(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions - only high-level staff can delete
                if (!new[] { UserRoles.SystemAdmin, UserRoles.ClinicAdmin, UserRoles.Doctor }.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.DeleteCarePlanAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting care plan {CarePlanId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi xóa kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy danh sách kế hoạch chăm sóc (có phân trang và filter)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<CarePlanDto>>>> GetCarePlans(
            [FromQuery] int? patientId = null,
            [FromQuery] string? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _carePlanService.GetCarePlansAsync(tenantId, patientId, status, pageNumber, pageSize);

                if (result.Success)
                {
                    return Ok(ApiResponse<PagedResult<CarePlanDto>>.SuccessResult(result.Data!, "Lấy danh sách kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<PagedResult<CarePlanDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting care plans");
                return StatusCode(500, ApiResponse<PagedResult<CarePlanDto>>.ErrorResult("Lỗi server khi lấy danh sách kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy kế hoạch chăm sóc đang hoạt động của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/active")]
        public async Task<ActionResult<ApiResponse<List<CarePlanDto>>>> GetActiveCarePlansForPatient(int patientId)
        {
            try
            {
                // Kiểm tra loại user
                var userType = User.FindFirst("UserType")?.Value;
                
                if (userType == "Patient")
                {
                    // Patient có thể xem CarePlan từ tất cả tenant
                    var result = await _carePlanService.GetActiveCarePlansForPatientAsync(patientId, null);
                    if (result.Success)
                    {
                        return Ok(ApiResponse<List<CarePlanDto>>.SuccessResult(result.Data!, "Lấy kế hoạch chăm sóc đang hoạt động thành công"));
                    }
                    return BadRequest(ApiResponse<List<CarePlanDto>>.ErrorResult(result.ErrorMessage!));
                }
                else
                {
                    // Staff chỉ xem CarePlan trong tenant của mình
                    var tenantId = GetTenantId();
                    var result = await _carePlanService.GetActiveCarePlansForPatientAsync(patientId, tenantId);
                    if (result.Success)
                    {
                        return Ok(ApiResponse<List<CarePlanDto>>.SuccessResult(result.Data!, "Lấy kế hoạch chăm sóc đang hoạt động thành công"));
                    }
                    return BadRequest(ApiResponse<List<CarePlanDto>>.ErrorResult(result.ErrorMessage!));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active care plans for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<CarePlanDto>>.ErrorResult("Lỗi server khi lấy kế hoạch chăm sóc đang hoạt động"));
            }
        }

        /// <summary>
        /// Lấy tiến độ kế hoạch chăm sóc
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<ActionResult<ApiResponse<CarePlanProgressDto>>> GetCarePlanProgress(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _carePlanService.GetCarePlanProgressAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanProgressDto>.SuccessResult(result.Data!, "Lấy tiến độ kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanProgressDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting care plan progress {CarePlanId}", id);
                return StatusCode(500, ApiResponse<CarePlanProgressDto>.ErrorResult("Lỗi server khi lấy tiến độ kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy tiến độ tất cả kế hoạch chăm sóc của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}/progress")]
        public async Task<ActionResult<ApiResponse<List<CarePlanProgressDto>>>> GetPatientCarePlanProgress(int patientId)
        {
            try
            {
                // Kiểm tra loại user
                var userType = User.FindFirst("UserType")?.Value;
                
                if (userType == "Patient")
                {
                    // Patient có thể xem progress từ tất cả tenant
                    var result = await _carePlanService.GetPatientCarePlanProgressAsync(patientId, null);
                    if (result.Success)
                    {
                        return Ok(ApiResponse<List<CarePlanProgressDto>>.SuccessResult(result.Data!, "Lấy tiến độ kế hoạch chăm sóc của bệnh nhân thành công"));
                    }
                    return BadRequest(ApiResponse<List<CarePlanProgressDto>>.ErrorResult(result.ErrorMessage!));
                }
                else
                {
                    // Staff chỉ xem progress trong tenant của mình
                    var tenantId = GetTenantId();
                    var result = await _carePlanService.GetPatientCarePlanProgressAsync(patientId, tenantId);
                    if (result.Success)
                    {
                        return Ok(ApiResponse<List<CarePlanProgressDto>>.SuccessResult(result.Data!, "Lấy tiến độ kế hoạch chăm sóc của bệnh nhân thành công"));
                    }
                    return BadRequest(ApiResponse<List<CarePlanProgressDto>>.ErrorResult(result.ErrorMessage!));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient care plan progress {PatientId}", patientId);
                return StatusCode(500, ApiResponse<List<CarePlanProgressDto>>.ErrorResult("Lỗi server khi lấy tiến độ kế hoạch chăm sóc của bệnh nhân"));
            }
        }

        /// <summary>
        /// Thêm item mới vào kế hoạch chăm sóc
        /// </summary>
        [HttpPost("{carePlanId}/items")]
        public async Task<ActionResult<ApiResponse<CarePlanItemDto>>> CreateCarePlanItem(int carePlanId, [FromBody] CarePlanItemCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (!UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.CreateCarePlanItemAsync(carePlanId, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanItemDto>.SuccessResult(result.Data!, "Thêm item vào kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanItemDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating care plan item for care plan {CarePlanId}", carePlanId);
                return StatusCode(500, ApiResponse<CarePlanItemDto>.ErrorResult("Lỗi server khi thêm item vào kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Cập nhật item trong kế hoạch chăm sóc
        /// </summary>
        [HttpPut("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<CarePlanItemDto>>> UpdateCarePlanItem(int itemId, [FromBody] CarePlanItemUpdateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (!UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.UpdateCarePlanItemAsync(itemId, dto, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanItemDto>.SuccessResult(result.Data!, "Cập nhật item kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanItemDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating care plan item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<CarePlanItemDto>.ErrorResult("Lỗi server khi cập nhật item kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Xóa item khỏi kế hoạch chăm sóc
        /// </summary>
        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCarePlanItem(int itemId)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (!UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                var result = await _carePlanService.DeleteCarePlanItemAsync(itemId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa item kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<bool>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting care plan item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi server khi xóa item kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy danh sách items của kế hoạch chăm sóc
        /// </summary>
        [HttpGet("{carePlanId}/items")]
        public async Task<ActionResult<ApiResponse<List<CarePlanItemDto>>>> GetCarePlanItems(int carePlanId)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _carePlanService.GetCarePlanItemsAsync(carePlanId, tenantId);

                if (result.Success)
                {
                    return Ok(ApiResponse<List<CarePlanItemDto>>.SuccessResult(result.Data!, "Lấy danh sách items kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<List<CarePlanItemDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting care plan items for care plan {CarePlanId}", carePlanId);
                return StatusCode(500, ApiResponse<List<CarePlanItemDto>>.ErrorResult("Lỗi server khi lấy danh sách items kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Ghi log thực hiện item kế hoạch chăm sóc (cho bệnh nhân sử dụng qua app)
        /// </summary>
        [HttpPost("items/log")]
        public async Task<ActionResult<ApiResponse<CarePlanItemLogDto>>> LogCarePlanItem([FromBody] CarePlanItemLogCreateDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var patientId = GetUserId(); // Assuming patient is logged in
                var userRole = GetUserRole();

                // Only patients and medical staff can log
                if (userRole != UserRoles.Patient && !UserRoles.MedicalDataRoles.Contains(userRole))
                {
                    return Forbid();
                }

                // If it's medical staff, they need to specify which patient (for demo, using the logged user)
                var result = await _carePlanService.LogCarePlanItemAsync(dto, tenantId, patientId);

                if (result.Success)
                {
                    return Ok(ApiResponse<CarePlanItemLogDto>.SuccessResult(result.Data!, "Ghi log item kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<CarePlanItemLogDto>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging care plan item");
                return StatusCode(500, ApiResponse<CarePlanItemLogDto>.ErrorResult("Lỗi server khi ghi log item kế hoạch chăm sóc"));
            }
        }

        /// <summary>
        /// Lấy danh sách logs của kế hoạch chăm sóc (có phân trang và filter)
        /// </summary>
        [HttpGet("logs")]
        public async Task<ActionResult<ApiResponse<PagedResult<CarePlanItemLogDto>>>> GetCarePlanItemLogs(
            [FromQuery] int? patientId = null,
            [FromQuery] int? carePlanId = null,
            [FromQuery] int? itemId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = GetTenantId();
                var result = await _carePlanService.GetCarePlanItemLogsAsync(tenantId, patientId, carePlanId, itemId, fromDate, toDate, pageNumber, pageSize);

                if (result.Success)
                {
                    return Ok(ApiResponse<PagedResult<CarePlanItemLogDto>>.SuccessResult(result.Data!, "Lấy danh sách logs kế hoạch chăm sóc thành công"));
                }

                return BadRequest(ApiResponse<PagedResult<CarePlanItemLogDto>>.ErrorResult(result.ErrorMessage!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting care plan item logs");
                return StatusCode(500, ApiResponse<PagedResult<CarePlanItemLogDto>>.ErrorResult("Lỗi server khi lấy danh sách logs kế hoạch chăm sóc"));
            }
        }
    }
}
