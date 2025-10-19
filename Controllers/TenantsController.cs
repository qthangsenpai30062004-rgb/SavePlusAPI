using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IPatientService _patientService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(
            ITenantService tenantService, 
            IPatientService patientService, 
            ICloudinaryService cloudinaryService,
            ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _patientService = patientService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo tenant mới (phòng khám/bệnh viện)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TenantDto>>> CreateTenant([FromBody] TenantCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TenantDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _tenantService.CreateTenantAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetTenant), new { id = result.Data?.TenantId }, result);
        }

        /// <summary>
        /// Lấy thông tin tenant
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenant(int id)
        {
            var result = await _tenantService.GetTenantByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy tenant theo mã code
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenantByCode(string code)
        {
            var result = await _tenantService.GetTenantByCodeAsync(code);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin tenant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> UpdateTenant(int id, [FromBody] TenantUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TenantDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _tenantService.UpdateTenantAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tenants
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<TenantDto>>>> GetTenants(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            var result = await _tenantService.GetTenantsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê tenant
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<TenantStatsDto>>> GetTenantStats(int id)
        {
            var result = await _tenantService.GetTenantStatsAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách bệnh nhân của tenant
        /// </summary>
        [HttpGet("{id}/patients")]
        public async Task<ActionResult<ApiResponse<PagedResult<ClinicPatientDto>>>> GetTenantPatients(
            int id,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            var result = await _patientService.GetClinicPatientsAsync(id, pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm bệnh nhân trong tenant (cho autocomplete)
        /// </summary>
        [HttpGet("{tenantId}/patients/search")]
        public async Task<ActionResult<ApiResponse<List<PatientSearchDto>>>> SearchPatientsInTenant(
            int tenantId,
            [FromQuery] string searchTerm,
            [FromQuery] int limit = 10)
        {
            var result = await _patientService.SearchPatientsInTenantAsync(tenantId, searchTerm, limit);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin bệnh nhân cụ thể trong tenant
        /// </summary>
        [HttpGet("{tenantId}/patients/{patientId}")]
        public async Task<ActionResult<ApiResponse<ClinicPatientDto>>> GetTenantPatient(int tenantId, int patientId)
        {
            var result = await _patientService.GetClinicPatientAsync(tenantId, patientId);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin bệnh nhân trong tenant
        /// </summary>
        [HttpPut("{tenantId}/patients/{patientId}")]
        public async Task<ActionResult<ApiResponse<ClinicPatientDto>>> UpdateTenantPatient(
            int tenantId, 
            int patientId, 
            [FromBody] UpdateClinicPatientDto dto)
        {
            var result = await _patientService.UpdateClinicPatientAsync(tenantId, patientId, dto.Mrn, dto.PrimaryDoctorId, dto.Status);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách bác sĩ của tenant
        /// </summary>
        [HttpGet("{tenantId}/doctors")]
        public async Task<ActionResult<ApiResponse<PagedResult<DoctorDto>>>> GetTenantDoctors(
            int tenantId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _tenantService.GetTenantDoctorsAsync(tenantId, pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin bác sĩ cụ thể trong tenant
        /// </summary>
        [HttpGet("{tenantId}/doctors/{doctorId}")]
        public async Task<ActionResult<ApiResponse<DoctorDto>>> GetTenantDoctor(int tenantId, int doctorId)
        {
            var result = await _tenantService.GetTenantDoctorAsync(tenantId, doctorId);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin bác sĩ trong tenant
        /// </summary>
        [HttpPut("{tenantId}/doctors/{doctorId}")]
        public async Task<ActionResult<ApiResponse<DoctorDto>>> UpdateTenantDoctor(
            int tenantId, 
            int doctorId, 
            [FromBody] UpdateDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DoctorDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _tenantService.UpdateTenantDoctorAsync(tenantId, doctorId, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Test Cloudinary connection
        /// </summary>
        [HttpGet("test-cloudinary")]
        public IActionResult TestCloudinary()
        {
            try
            {
                _logger.LogInformation("Testing Cloudinary configuration...");
                return Ok(ApiResponse<string>.SuccessResult("Cloudinary service is configured", "Cloudinary OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloudinary test failed");
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Cloudinary error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload thumbnail cho tenant (chỉ upload lên Cloudinary, chưa lưu database)
        /// </summary>
        [HttpPost("upload-image")]
        public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file, [FromForm] string folder = "tenants")
        {
            try
            {
                _logger.LogInformation("Upload image request to folder {Folder}", folder);
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File is null or empty");
                    return BadRequest(ApiResponse<string>.ErrorResult("File không hợp lệ"));
                }

                _logger.LogInformation("File received: {FileName}, Size: {Size} bytes", file.FileName, file.Length);

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file extension: {Extension}", extension);
                    return BadRequest(ApiResponse<string>.ErrorResult("Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)"));
                }

                // Kiểm tra kích thước file (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning("File size too large: {Size} bytes", file.Length);
                    return BadRequest(ApiResponse<string>.ErrorResult("File không được vượt quá 10MB"));
                }

                _logger.LogInformation("Uploading to Cloudinary...");
                // Upload lên Cloudinary (chỉ trả về URL, chưa lưu database)
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, folder);
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogError("Cloudinary returned null or empty URL");
                    return BadRequest(ApiResponse<string>.ErrorResult("Upload ảnh thất bại"));
                }

                _logger.LogInformation("Cloudinary upload successful: {ImageUrl}", imageUrl);
                return Ok(ApiResponse<string>.SuccessResult(imageUrl, "Upload ảnh thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, ApiResponse<string>.ErrorResult($"Có lỗi xảy ra khi upload ảnh: {ex.Message}"));
            }
        }

    }

    // DTO cho update clinic patient
    public class UpdateClinicPatientDto
    {
        public string? Mrn { get; set; }
        public int? PrimaryDoctorId { get; set; }
        public byte? Status { get; set; }
    }
}
