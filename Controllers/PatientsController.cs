using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký bệnh nhân mới
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<PatientDto>>> RegisterPatient([FromBody] PatientRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PatientDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientService.RegisterPatientAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetPatientById), new { id = result.Data?.PatientId }, result);
        }

        /// <summary>
        /// Lấy thông tin bệnh nhân theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientById(int id)
        {
            var result = await _patientService.GetPatientByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Tìm bệnh nhân theo số điện thoại
        /// </summary>
        [HttpGet("phone/{phoneNumber}")]
        public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientByPhone(string phoneNumber)
        {
            var result = await _patientService.GetPatientByPhoneAsync(phoneNumber);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin bệnh nhân
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PatientDto>>> UpdatePatient(int id, [FromBody] PatientUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PatientDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientService.UpdatePatientAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách bệnh nhân (có phân trang và tìm kiếm)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<PatientDto>>>> GetPatients(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            var result = await _patientService.GetPatientsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Đăng ký bệnh nhân vào phòng khám
        /// </summary>
        [HttpPost("{patientId}/enroll/{tenantId}")]
        public async Task<ActionResult<ApiResponse<ClinicPatientDto>>> EnrollPatientToClinic(
            int patientId, 
            int tenantId, 
            [FromBody] EnrollPatientDto? dto = null)
        {
            var result = await _patientService.EnrollPatientToClinicAsync(tenantId, patientId, dto?.Mrn, dto?.PrimaryDoctorId);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Tìm bệnh nhân trong tất cả các phòng khám theo số điện thoại
        /// </summary>
        [HttpGet("search/clinics/{phoneNumber}")]
        public async Task<ActionResult<ApiResponse<List<ClinicPatientDto>>>> FindPatientInClinics(string phoneNumber)
        {
            var result = await _patientService.FindPatientInClinicsAsync(phoneNumber);
            return Ok(result);
        }

        /// <summary>
        /// Đăng nhập bệnh nhân
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] PatientLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientService.AuthenticatePatientAsync(dto.PhoneNumber, dto.VerificationCode ?? "");
            
            if (!result.Success)
                return Unauthorized(result);
            
            return Ok(result);
        }
    }

    public class EnrollPatientDto
    {
        public string? Mrn { get; set; }
        public int? PrimaryDoctorId { get; set; }
    }
}
