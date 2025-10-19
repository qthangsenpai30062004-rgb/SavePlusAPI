using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo cuộc hẹn mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _appointmentService.CreateAppointmentAsync(dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetAppointment), new { id = result.Data?.AppointmentId }, result);
        }

        /// <summary>
        /// Lấy thông tin cuộc hẹn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointment(int id)
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin cuộc hẹn
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateAppointment(int id, [FromBody] AppointmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _appointmentService.UpdateAppointmentAsync(id, dto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Hủy cuộc hẹn
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelAppointment(int id, [FromQuery] string? reason = null)
        {
            var result = await _appointmentService.CancelAppointmentAsync(id, reason ?? "");
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hẹn (có lọc và phân trang)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AppointmentDto>>>> GetAppointments(
            [FromQuery] int? tenantId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new AppointmentFilterDto
            {
                TenantId = tenantId,
                PatientId = patientId,
                DoctorId = doctorId,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                Type = type,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _appointmentService.GetAppointmentsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hẹn của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetPatientAppointments(int patientId, [FromQuery] int? tenantId = null)
        {
            var result = await _appointmentService.GetPatientAppointmentsAsync(patientId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hẹn của bác sĩ
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetDoctorAppointments(
            int doctorId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _appointmentService.GetDoctorAppointmentsAsync(doctorId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hẹn của phòng khám
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetTenantAppointments(
            int tenantId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _appointmentService.GetTenantAppointmentsAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra lịch trống của bác sĩ
        /// </summary>
        [HttpGet("doctor/{doctorId}/availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckDoctorAvailability(
            int doctorId, 
            [FromQuery] DateTime startTime, 
            [FromQuery] DateTime endTime)
        {
            _logger.LogInformation("==> API received check availability request: Doctor={DoctorId}, Start={Start}, End={End}", 
                doctorId, startTime, endTime);
            var result = await _appointmentService.CheckDoctorAvailabilityAsync(doctorId, startTime, endTime);
            return Ok(result);
        }

        /// <summary>
        /// Lấy khung giờ trống của bác sĩ trong ngày
        /// </summary>
        [HttpGet("doctor/{doctorId}/timeslots")]
        public async Task<ActionResult<ApiResponse<List<DateTime>>>> GetAvailableTimeSlots(
            int doctorId, 
            [FromQuery] DateTime date, 
            [FromQuery] int durationMinutes = 5)
        {
            _logger.LogInformation("==> API received get timeslots request: Doctor={DoctorId}, Date={Date}, Duration={Duration}", 
                doctorId, date, durationMinutes);
            var result = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, date, durationMinutes);
            _logger.LogInformation("<== Returning {Count} available slots", result.Data?.Count ?? 0);
            return Ok(result);
        }

        /// <summary>
        /// Xác nhận cuộc hẹn
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> ConfirmAppointment(int id)
        {
            var result = await _appointmentService.ConfirmAppointmentAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Bắt đầu cuộc hẹn
        /// </summary>
        [HttpPost("{id}/start")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> StartAppointment(int id)
        {
            var result = await _appointmentService.StartAppointmentAsync(id);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Hoàn thành cuộc hẹn
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> CompleteAppointment(int id, [FromBody] CompleteAppointmentDto? dto = null)
        {
            var result = await _appointmentService.CompleteAppointmentAsync(id, dto?.Notes);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hẹn hôm nay
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetTodayAppointments([FromQuery] int? tenantId = null)
        {
            var result = await _appointmentService.GetTodayAppointmentsAsync(tenantId);
            return Ok(result);
        }
    }

    // DTO cho complete appointment
    public class CompleteAppointmentDto
    {
        public string? Notes { get; set; }
    }
}
