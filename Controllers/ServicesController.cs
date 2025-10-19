using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(IServiceService serviceService, ILogger<ServicesController> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all services for a tenant
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> GetTenantServices(int tenantId)
        {
            var result = await _serviceService.GetTenantServicesAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{serviceId}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> GetServiceById(int serviceId)
        {
            var result = await _serviceService.GetServiceByIdAsync(serviceId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Create a new service (Admin only)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> CreateService([FromBody] ServiceCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ServiceDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            var result = await _serviceService.CreateServiceAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetServiceById), new { serviceId = result.Data!.ServiceId }, result);
        }

        /// <summary>
        /// Update a service (Admin only)
        /// </summary>
        [HttpPut("{serviceId}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> UpdateService(int serviceId, [FromBody] ServiceUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ServiceDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            var result = await _serviceService.UpdateServiceAsync(serviceId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a service (Admin only)
        /// </summary>
        [HttpDelete("{serviceId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteService(int serviceId)
        {
            var result = await _serviceService.DeleteServiceAsync(serviceId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
