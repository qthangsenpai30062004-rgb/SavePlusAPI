using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorWorkingHoursController : ControllerBase
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<DoctorWorkingHoursController> _logger;

        public DoctorWorkingHoursController(SavePlusDbContext context, ILogger<DoctorWorkingHoursController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get working hours for a specific doctor
        /// </summary>
        [HttpGet("{doctorId}/working-hours")]
        public async Task<ActionResult<ApiResponse<List<DoctorWorkingHourDto>>>> GetDoctorWorkingHours(int doctorId)
        {
            try
            {
                var workingHours = await _context.DoctorWorkingHours
                    .Where(w => w.DoctorId == doctorId && w.IsActive)
                    .Include(w => w.Doctor)
                        .ThenInclude(d => d.User)
                    .OrderBy(w => w.DayOfWeek)
                    .Select(w => new DoctorWorkingHourDto
                    {
                        WorkingHourId = w.WorkingHourId,
                        DoctorId = w.DoctorId,
                        DoctorName = w.Doctor != null && w.Doctor.User != null ? w.Doctor.User.FullName : string.Empty,
                        DayOfWeek = w.DayOfWeek,
                        DayOfWeekName = GetDayName(w.DayOfWeek),
                        StartTime = w.StartTime,
                        EndTime = w.EndTime,
                        SlotDurationMinutes = w.SlotDurationMinutes,
                        IsActive = w.IsActive
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<DoctorWorkingHourDto>>.SuccessResult(workingHours));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting working hours for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<List<DoctorWorkingHourDto>>.ErrorResult("Lỗi khi tải lịch làm việc"));
            }
        }

        /// <summary>
        /// Get doctors available at specific time slot
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<int>>>> GetAvailableDoctors(
            [FromQuery] int tenantId,
            [FromQuery] int dayOfWeek, // 1=Monday, 7=Sunday
            [FromQuery] string timeSlot, // HH:mm format
            [FromQuery] string? date // yyyy-MM-dd format (optional but recommended)
        )
        {
            try
            {
                // Parse time slot
                if (!TimeSpan.TryParse(timeSlot, out TimeSpan requestedTime))
                {
                    return BadRequest(ApiResponse<List<int>>.ErrorResult("Định dạng giờ không hợp lệ"));
                }

                // Parse date if provided
                DateTime? requestedDate = null;
                if (!string.IsNullOrEmpty(date))
                {
                    if (DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        requestedDate = parsedDate;
                    }
                }

                // Step 1: Find doctors with working hours matching the requested time
                var doctorsWithWorkingHours = await _context.DoctorWorkingHours
                    .Where(w => 
                        w.Doctor!.TenantId == tenantId &&
                        w.DayOfWeek == dayOfWeek &&
                        w.IsActive &&
                        w.StartTime <= requestedTime &&
                        w.EndTime > requestedTime
                    )
                    .Select(w => w.DoctorId)
                    .Distinct()
                    .ToListAsync();

                if (!doctorsWithWorkingHours.Any())
                {
                    return Ok(ApiResponse<List<int>>.SuccessResult(new List<int>()));
                }

                // Step 2: If date is provided, filter out doctors who have appointments at that time
                if (requestedDate.HasValue)
                {
                    var requestedDateTime = requestedDate.Value.Date.Add(requestedTime);
                    var endDateTime = requestedDateTime.AddMinutes(30); // Assuming 30-minute slots

                    // Get doctors who have appointments overlapping with the requested time
                    var busyDoctorIds = await _context.Appointments
                        .Where(a => 
                            doctorsWithWorkingHours.Contains(a.DoctorId!.Value) &&
                            a.StartAt < endDateTime &&
                            a.EndAt > requestedDateTime &&
                            (a.Status == "Pending" || a.Status == "Confirmed" || a.Status == "InProgress")
                        )
                        .Select(a => a.DoctorId!.Value)
                        .Distinct()
                        .ToListAsync();

                    // Remove busy doctors from the list
                    var availableDoctorIds = doctorsWithWorkingHours
                        .Where(id => !busyDoctorIds.Contains(id))
                        .ToList();

                    return Ok(ApiResponse<List<int>>.SuccessResult(availableDoctorIds));
                }

                // If no date provided, just return doctors with working hours
                return Ok(ApiResponse<List<int>>.SuccessResult(doctorsWithWorkingHours));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available doctors");
                return StatusCode(500, ApiResponse<List<int>>.ErrorResult("Lỗi khi tìm bác sĩ"));
            }
        }

        /// <summary>
        /// Get available dates for a tenant within a date range
        /// Returns list of dates that have at least one available doctor
        /// </summary>
        [HttpGet("available-dates")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableDates(
            [FromQuery] int tenantId,
            [FromQuery] string startDate, // yyyy-MM-dd
            [FromQuery] string endDate // yyyy-MM-dd
        )
        {
            try
            {
                if (!DateTime.TryParse(startDate, out DateTime start) || 
                    !DateTime.TryParse(endDate, out DateTime end))
                {
                    return BadRequest(ApiResponse<List<string>>.ErrorResult("Định dạng ngày không hợp lệ"));
                }

                var availableDates = new List<string>();
                var currentDate = start.Date;

                while (currentDate <= end.Date)
                {
                    // Get day of week (1=Monday, 7=Sunday)
                    int dayOfWeek = (int)currentDate.DayOfWeek;
                    dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;

                    // Check if any doctor has working hours on this day
                    var doctorsWorkingOnDay = await _context.DoctorWorkingHours
                        .Where(w => 
                            w.Doctor!.TenantId == tenantId &&
                            w.DayOfWeek == dayOfWeek &&
                            w.IsActive
                        )
                        .Select(w => new { w.DoctorId, w.StartTime, w.EndTime })
                        .Distinct()
                        .ToListAsync();

                    if (doctorsWorkingOnDay.Any())
                    {
                        // Check if there are any available time slots on this day
                        bool hasAvailableSlot = false;

                        foreach (var workingHour in doctorsWorkingOnDay)
                        {
                            // Generate time slots for this working hour
                            var currentTime = workingHour.StartTime;
                            var endTime = workingHour.EndTime;

                            while (currentTime < endTime)
                            {
                                var slotStart = currentDate.Date.Add(currentTime);
                                var slotEnd = slotStart.AddMinutes(30);

                                // Check if this doctor is busy at this time
                                var isBusy = await _context.Appointments
                                    .AnyAsync(a => 
                                        a.DoctorId == workingHour.DoctorId &&
                                        a.StartAt < slotEnd &&
                                        a.EndAt > slotStart &&
                                        (a.Status == "Pending" || a.Status == "Confirmed" || a.Status == "InProgress")
                                    );

                                if (!isBusy)
                                {
                                    hasAvailableSlot = true;
                                    break;
                                }

                                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
                            }

                            if (hasAvailableSlot) break;
                        }

                        if (hasAvailableSlot)
                        {
                            availableDates.Add(currentDate.ToString("yyyy-MM-dd"));
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                return Ok(ApiResponse<List<string>>.SuccessResult(availableDates));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available dates");
                return StatusCode(500, ApiResponse<List<string>>.ErrorResult("Lỗi khi tải ngày khả dụng"));
            }
        }

        private string GetDayName(byte dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Thứ Hai",
                2 => "Thứ Ba",
                3 => "Thứ Tư",
                4 => "Thứ Năm",
                5 => "Thứ Sáu",
                6 => "Thứ Bảy",
                7 => "Chủ Nhật",
                _ => "Không xác định"
            };
        }
    }
}
