using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin bác sĩ của user hiện tại (GET /api/doctors/me)
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<DoctorEditDto>>> GetMyDoctorProfile([FromQuery] int userId)
        {
            // In production, get userId from JWT token claims
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<DoctorEditDto>.ErrorResult("UserId không hợp lệ"));
            }

            var result = await _doctorService.GetDoctorByUserIdAsync(userId);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin bác sĩ của chính mình (PUT /api/doctors/me)
        /// </summary>
        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<DoctorEditDto>>> UpdateMyDoctorProfile(
            [FromQuery] int userId,
            [FromBody] DoctorSelfUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DoctorEditDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            if (userId <= 0)
            {
                return BadRequest(ApiResponse<DoctorEditDto>.ErrorResult("UserId không hợp lệ"));
            }

            var result = await _doctorService.UpdateDoctorSelfAsync(userId, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Admin: Lấy thông tin bác sĩ theo ID (GET /api/doctors/{doctorId})
        /// </summary>
        [HttpGet("{doctorId}")]
        public async Task<ActionResult<ApiResponse<DoctorEditDto>>> GetDoctorById(int doctorId)
        {
            var result = await _doctorService.GetDoctorByIdAsync(doctorId);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Admin: Cập nhật thông tin bác sĩ (PUT /api/doctors/{doctorId})
        /// </summary>
        [HttpPut("{doctorId}")]
        public async Task<ActionResult<ApiResponse<DoctorEditDto>>> UpdateDoctorByAdmin(
            int doctorId,
            [FromBody] DoctorAdminUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DoctorEditDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _doctorService.UpdateDoctorByAdminAsync(doctorId, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }
    }
}
