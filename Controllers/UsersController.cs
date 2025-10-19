using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using SavePlus_API.Constants;
using SavePlus_API.Extensions;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo user mới (staff registration)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _userService.CreateUserAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetUser), new { id = result.Data?.UserId }, result);
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy user theo email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email)
        {
            var result = await _userService.GetUserByEmailAsync(email);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy user theo số điện thoại
        /// </summary>
        [HttpGet("phone/{phoneNumber}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByPhone(string phoneNumber)
        {
            var result = await _userService.GetUserByPhoneAsync(phoneNumber);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _userService.UpdateUserAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(int id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách users (có phân trang và tìm kiếm)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers(
            [FromQuery] int? tenantId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _userService.GetUsersAsync(tenantId, pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Lấy user với thông tin doctor (nếu là bác sĩ)
        /// </summary>
        [HttpGet("{id}/doctor-info")]
        public async Task<ActionResult<ApiResponse<UserWithDoctorDto>>> GetUserWithDoctorInfo(int id)
        {
            var result = await _userService.GetUserWithDoctorInfoAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy users theo tenant
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsersByTenant(int tenantId, [FromQuery] string? role = null)
        {
            var result = await _userService.GetUsersByTenantAsync(tenantId, role);
            return Ok(result);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _userService.ChangePasswordAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra email có tồn tại không
        /// </summary>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(string email)
        {
            var result = await _userService.EmailExistsAsync(email);
            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra phone có tồn tại không
        /// </summary>
        [HttpGet("check-phone/{phoneNumber}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPhoneExists(string phoneNumber)
        {
            var result = await _userService.PhoneExistsAsync(phoneNumber);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả roles có thể có trong hệ thống
        /// </summary>
        [HttpGet("roles")]
        public ActionResult<ApiResponse<object>> GetAvailableRoles()
        {
            var roles = new
            {
                coreRoles = UserRoles.CoreRoles.Select(r => new { 
                    role = r, 
                    description = r.GetRoleDescription(),
                    isActive = true
                }),
                advancedRoles = UserRoles.AdvancedRoles.Select(r => new { 
                    role = r, 
                    description = r.GetRoleDescription(),
                    isActive = false // Phase 2 - chưa triển khai
                }),
                permissions = new
                {
                    medicalDataRoles = UserRoles.MedicalDataRoles,
                    prescriptionRoles = UserRoles.PrescriptionRoles,
                    appointmentManagementRoles = UserRoles.AppointmentManagementRoles,
                    staffRoles = UserRoles.StaffRoles
                }
            };

            return Ok(ApiResponse<object>.SuccessResult(roles, "Lấy danh sách roles thành công"));
        }

        /// <summary>
        /// Tạo Doctor record cho User có role Doctor (Quick fix)
        /// </summary>
        [HttpPost("{userId}/create-doctor")]
        public async Task<ActionResult<ApiResponse<object>>> CreateDoctorRecord(int userId, [FromBody] CreateDoctorDto dto)
        {
            try
            {
                var result = await _userService.CreateDoctorRecordAsync(userId, dto.Specialty, dto.LicenseNumber);
                
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi tạo Doctor record"));
            }
        }

        /// <summary>
        /// Search bác sĩ trong tenant cho autocomplete
        /// </summary>
        [HttpGet("tenants/{tenantId}/doctors/search")]
        public async Task<ActionResult<ApiResponse<List<DoctorSearchDto>>>> SearchDoctorsInTenant(
            int tenantId,
            [FromQuery] string searchTerm = "",
            [FromQuery] int limit = 10)
        {
            var result = await _userService.SearchDoctorsInTenantAsync(tenantId, searchTerm, limit);
            return Ok(result);
        }
    }

    public class CreateDoctorDto
    {
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
