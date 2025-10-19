using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using SavePlus_API.Constants;

namespace SavePlus_API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(SavePlusDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(AppointmentCreateDto dto)
        {
            try
            {
                if (dto.StartAt < DateTime.Now)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không thể đặt lịch hẹn vào thời gian trong quá khứ");
                }

                var endAt = dto.EndAt ?? dto.StartAt.AddMinutes(30);
                if (endAt <= dto.StartAt)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Thời gian kết thúc phải sau thời gian bắt đầu");
                }

                var patient = await _context.Patients.FindAsync(dto.PatientId);
                if (patient == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy bệnh nhân");
                }

                var tenant = await _context.Tenants.FindAsync(dto.TenantId);
                if (tenant == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy phòng khám");
                }

                if (dto.DoctorId.HasValue)
                {
                    var doctor = await _context.Doctors.FindAsync(dto.DoctorId.Value);
                    if (doctor == null || doctor.TenantId != dto.TenantId)
                    {
                        return ApiResponse<AppointmentDto>.ErrorResult("Bác sĩ không hợp lệ hoặc không thuộc phòng khám này");
                    }

                    var isAvailable = await CheckDoctorAvailabilityAsync(dto.DoctorId.Value, dto.StartAt, endAt);
                    if (!isAvailable.Data)
                    {
                        return ApiResponse<AppointmentDto>.ErrorResult("Bác sĩ không có lịch trống trong thời gian này");
                    }
                }

                var appointment = new Appointment
                {
                    TenantId = dto.TenantId,
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    StartAt = dto.StartAt,
                    EndAt = endAt,
                    Type = dto.Type,
                    Channel = dto.Channel ?? AppointmentConstants.Channels.App,
                    Address = dto.Address,
                    Status = AppointmentConstants.Statuses.Scheduled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                var paymentTransaction = new PaymentTransaction
                {
                    TenantId = dto.TenantId,
                    PatientId = dto.PatientId,
                    AppointmentId = appointment.AppointmentId,
                    Amount = dto.EstimatedCost ?? 0,
                    Currency = "VND",
                    Method = "CASH",
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    ProviderRef = $"APT_{appointment.AppointmentId}"
                };

                _context.PaymentTransactions.Add(paymentTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created payment transaction {PaymentId} for appointment {AppointmentId}", 
                    paymentTransaction.PaymentId, appointment.AppointmentId);

                var result = await GetAppointmentByIdAsync(appointment.AppointmentId);
                return result.Success ? 
                    ApiResponse<AppointmentDto>.SuccessResult(result.Data!, "Đặt lịch hẹn thành công") :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cuộc hẹn");
                return ApiResponse<AppointmentDto>.ErrorResult("Có lỗi xảy ra khi đặt lịch hẹn");
            }
        }

        public async Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                var result = MapToAppointmentDto(appointment);
                return ApiResponse<AppointmentDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<AppointmentDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin cuộc hẹn");
            }
        }

        public async Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(int appointmentId, AppointmentUpdateDto dto)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                // Validate: Nếu update StartAt, không cho phép chuyển thành quá khứ
                if (dto.StartAt.HasValue && dto.StartAt.Value < DateTime.Now)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không thể đặt lịch hẹn vào thời gian trong quá khứ");
                }

                if (dto.StartAt.HasValue)
                    appointment.StartAt = dto.StartAt.Value;
                if (dto.EndAt.HasValue)
                    appointment.EndAt = dto.EndAt.Value;
                    
                // Validate: EndAt phải sau StartAt
                if (appointment.EndAt <= appointment.StartAt)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Thời gian kết thúc phải sau thời gian bắt đầu");
                }
                
                if (!string.IsNullOrEmpty(dto.Type))
                    appointment.Type = dto.Type;
                if (!string.IsNullOrEmpty(dto.Status))
                    appointment.Status = dto.Status;
                if (!string.IsNullOrEmpty(dto.Address))
                    appointment.Address = dto.Address;

                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = await GetAppointmentByIdAsync(appointmentId);
                return result.Success ? 
                    ApiResponse<AppointmentDto>.SuccessResult(result.Data!, "Cập nhật cuộc hẹn thành công") :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<AppointmentDto>.ErrorResult("Có lỗi xảy ra khi cập nhật cuộc hẹn");
            }
        }

        public async Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, string reason = "")
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                appointment.Status = "Cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Hủy cuộc hẹn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi hủy cuộc hẹn");
            }
        }

        public async Task<ApiResponse<PagedResult<AppointmentDto>>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .AsQueryable();

                if (filter.TenantId.HasValue)
                    query = query.Where(a => a.TenantId == filter.TenantId.Value);

                if (filter.PatientId.HasValue)
                    query = query.Where(a => a.PatientId == filter.PatientId.Value);

                if (filter.DoctorId.HasValue)
                    query = query.Where(a => a.DoctorId == filter.DoctorId.Value);

                if (filter.FromDate.HasValue)
                {
                    var fromDateStart = filter.FromDate.Value.Date;
                    _logger.LogInformation("- FromDate filter: {FromDateStart}", fromDateStart);
                    query = query.Where(a => a.StartAt >= fromDateStart);
                }

                if (filter.ToDate.HasValue)
                {
                    var toDateEnd = filter.ToDate.Value.Date.AddDays(1);
                    _logger.LogInformation("- ToDate filter: {ToDateEnd}", toDateEnd);
                    query = query.Where(a => a.StartAt < toDateEnd);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(a => a.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Type))
                    query = query.Where(a => a.Type == filter.Type);

                var totalCount = await query.CountAsync();
                var appointments = await query
                    .OrderBy(a => a.StartAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = new PagedResult<AppointmentDto>
                {
                    Data = appointments.Select(MapToAppointmentDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResult<AppointmentDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc hẹn");
                return ApiResponse<PagedResult<AppointmentDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách cuộc hẹn");
            }
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetPatientAppointmentsAsync(int patientId, int? tenantId = null)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .Where(a => a.PatientId == patientId);

                if (tenantId.HasValue)
                    query = query.Where(a => a.TenantId == tenantId.Value);

                var appointments = await query
                    .OrderByDescending(a => a.StartAt)
                    .ToListAsync();

                var result = appointments.Select(MapToAppointmentDto).ToList();
                return ApiResponse<List<AppointmentDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch hẹn của bệnh nhân {PatientId}", patientId);
                return ApiResponse<List<AppointmentDto>>.ErrorResult("Có lỗi xảy ra khi lấy lịch hẹn bệnh nhân");
            }
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetDoctorAppointmentsAsync(int doctorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .Where(a => a.DoctorId == doctorId);

                if (fromDate == null) fromDate = DateTime.Today;
                if (toDate == null) toDate = DateTime.Today.AddDays(7);

                query = query.Where(a => a.StartAt >= fromDate && a.StartAt <= toDate);

                var appointments = await query
                    .OrderBy(a => a.StartAt)
                    .ToListAsync();

                var result = appointments.Select(MapToAppointmentDto).ToList();
                return ApiResponse<List<AppointmentDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch hẹn của bác sĩ {DoctorId}", doctorId);
                return ApiResponse<List<AppointmentDto>>.ErrorResult("Có lỗi xảy ra khi lấy lịch hẹn bác sĩ");
            }
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetTenantAppointmentsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .Where(a => a.TenantId == tenantId);

                if (fromDate.HasValue)
                    query = query.Where(a => a.StartAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(a => a.StartAt <= toDate.Value);

                var appointments = await query
                    .OrderBy(a => a.StartAt)
                    .ToListAsync();

                var result = appointments.Select(MapToAppointmentDto).ToList();
                return ApiResponse<List<AppointmentDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch hẹn của tenant {TenantId}", tenantId);
                return ApiResponse<List<AppointmentDto>>.ErrorResult("Có lỗi xảy ra khi lấy lịch hẹn phòng khám");
            }
        }

        public async Task<ApiResponse<bool>> CheckDoctorAvailabilityAsync(int doctorId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // Check if the time slot overlaps with any existing appointment
                // Two time slots overlap if: (endTime > a.StartAt && startTime < a.EndAt)
                var allAppointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                               a.StartAt.Date == startTime.Date &&
                               a.Status != "Cancelled" &&
                               a.Status != "NoShow")
                    .Select(a => new { a.AppointmentId, a.StartAt, a.EndAt, a.Status })
                    .ToListAsync();

                _logger.LogInformation("Doctor {DoctorId} has {Count} appointments on {Date}", 
                    doctorId, allAppointments.Count, startTime.Date);

                foreach (var apt in allAppointments)
                {
                    _logger.LogInformation("  - Appointment #{Id}: {Start} to {End} ({Status})", 
                        apt.AppointmentId, apt.StartAt, apt.EndAt, apt.Status);
                }

                var conflictingAppointments = allAppointments
                    .Where(a => endTime > a.StartAt && startTime < a.EndAt)
                    .ToList();

                if (conflictingAppointments.Any())
                {
                    _logger.LogWarning("CONFLICT found for {Start} to {End}:", startTime, endTime);
                    foreach (var apt in conflictingAppointments)
                    {
                        _logger.LogWarning("  - Conflicts with Appointment #{Id}: {Start} to {End}", 
                            apt.AppointmentId, apt.StartAt, apt.EndAt);
                    }
                }
                else
                {
                    _logger.LogInformation("✓ Time slot {Start} to {End} is AVAILABLE", startTime, endTime);
                }

                return ApiResponse<bool>.SuccessResult(conflictingAppointments.Count == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra lịch trống của bác sĩ {DoctorId}", doctorId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi kiểm tra lịch trống");
            }
        }

        public async Task<ApiResponse<List<DateTime>>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date, int durationMinutes = 30)
        {
            try
            {
                var workingHours = new List<(TimeSpan start, TimeSpan end)>
                {
                    (new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)),
                    (new TimeSpan(13, 30, 0), new TimeSpan(17, 30, 0))
                };

                var bookedAppointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                               a.StartAt.Date == date.Date &&
                               a.Status != "Cancelled" &&
                               a.Status != "NoShow")
                    .Select(a => new { a.AppointmentId, a.StartAt, a.EndAt, a.Status })
                    .ToListAsync();

                _logger.LogInformation("Doctor {DoctorId} has {Count} appointments on {Date}", 
                    doctorId, bookedAppointments.Count, date.Date);
                    
                foreach (var apt in bookedAppointments)
                {
                    _logger.LogInformation("  [GetTimeSlots] Appointment #{Id}: {Start} to {End} ({Status})", 
                        apt.AppointmentId, apt.StartAt, apt.EndAt, apt.Status);
                }

                var availableSlots = new List<DateTime>();

                foreach (var (start, end) in workingHours)
                {
                    var slotStart = DateTime.SpecifyKind(date.Date.Add(start), DateTimeKind.Unspecified);
                    var periodEnd = date.Date.Add(end);

                    while (slotStart.AddMinutes(durationMinutes) <= periodEnd)
                    {
                        var slotEnd = slotStart.AddMinutes(durationMinutes);
                        
                        if (slotStart.Hour == 9 && slotStart.Minute == 0)
                        {
                            _logger.LogWarning("[DEBUG] Checking slot 09:00 - 09:30:");
                            foreach (var b in bookedAppointments)
                            {
                                var overlap = slotStart < b.EndAt && slotEnd > b.StartAt;
                                _logger.LogWarning("  Apt #{Id} ({Start} to {End}): slotStart<EndAt={Check1}, slotEnd>StartAt={Check2}, Overlap={Result}",
                                    b.AppointmentId, b.StartAt, b.EndAt,
                                    slotStart < b.EndAt, slotEnd > b.StartAt, overlap);
                            }
                        }
                        
                        var isConflict = bookedAppointments.Any(b =>
                            (slotStart < b.EndAt && slotEnd > b.StartAt));

                        if (!isConflict)
                        {
                            availableSlots.Add(slotStart);
                            _logger.LogInformation("  ✓ Slot available: {Start} to {End}", slotStart, slotEnd);
                        }
                        else
                        {
                            var conflicts = bookedAppointments.Where(b => slotStart < b.EndAt && slotEnd > b.StartAt).ToList();
                            _logger.LogInformation("  ✗ Slot {Start} BLOCKED (conflicts: {Count})", slotStart, conflicts.Count);
                        }

                        slotStart = DateTime.SpecifyKind(slotStart.AddMinutes(durationMinutes), DateTimeKind.Unspecified);
                    }
                }

                _logger.LogInformation("Found {Count} available slots for doctor {DoctorId} on {Date}", 
                    availableSlots.Count, doctorId, date.Date);

                return ApiResponse<List<DateTime>>.SuccessResult(availableSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy khung giờ trống của bác sĩ {DoctorId}", doctorId);
                return ApiResponse<List<DateTime>>.ErrorResult("Có lỗi xảy ra khi lấy khung giờ trống");
            }
        }

        public async Task<ApiResponse<AppointmentDto>> ConfirmAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
            }

            appointment.Status = "Confirmed";
            appointment.UpdatedAt = DateTime.UtcNow;
                return await UpdateAppointmentStatusAsync(appointmentId, "Confirmed", "Xác nhận cuộc hẹn thành công");
        }

        public async Task<ApiResponse<AppointmentDto>> StartAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
            }

            appointment.Status = "InProgress";
            appointment.UpdatedAt = DateTime.UtcNow;
            return await UpdateAppointmentStatusAsync(appointmentId, "InProgress", "Bắt đầu cuộc hẹn thành công");
        }

        public async Task<ApiResponse<AppointmentDto>> CompleteAppointmentAsync(int appointmentId, string? notes = null)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = await GetAppointmentByIdAsync(appointmentId);
                return result.Success ? 
                    ApiResponse<AppointmentDto>.SuccessResult(result.Data!, "Hoàn thành cuộc hẹn thành công") :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<AppointmentDto>.ErrorResult("Có lỗi xảy ra khi hoàn thành cuộc hẹn");
            }
        }

        private async Task<ApiResponse<AppointmentDto>> UpdateAppointmentStatusAsync(int appointmentId, string status, string successMessage)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    return ApiResponse<AppointmentDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                appointment.Status = status;
                await _context.SaveChangesAsync();

                var result = await GetAppointmentByIdAsync(appointmentId);
                return result.Success ? 
                    ApiResponse<AppointmentDto>.SuccessResult(result.Data!, successMessage) :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái cuộc hẹn {AppointmentId}", appointmentId);
                return ApiResponse<AppointmentDto>.ErrorResult("Có lỗi xảy ra khi cập nhật trạng thái cuộc hẹn");
            }
        }

        private AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                TenantId = appointment.TenantId,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                StartAt = appointment.StartAt,
                EndAt = appointment.EndAt,
                Type = appointment.Type,
                Channel = appointment.Channel,
                Address = appointment.Address,
                Notes = null,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                
                PatientName = appointment.Patient?.FullName,
                PatientPhone = appointment.Patient?.PrimaryPhoneE164,
                PatientGender = appointment.Patient?.Gender,
                PatientDateOfBirth = appointment.Patient?.DateOfBirth,
                PatientAddress = appointment.Patient?.Address,
                
                DoctorName = appointment.Doctor?.User?.FullName,
                DoctorSpecialty = appointment.Doctor?.Specialty,
                DoctorLicenseNumber = appointment.Doctor?.LicenseNumber,
                DoctorPhone = appointment.Doctor?.User?.PhoneE164,
                
                TenantName = appointment.Tenant?.Name
            };
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetTodayAppointmentsAsync(int? tenantId = null)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .Where(a => a.StartAt >= today && a.StartAt < tomorrow);

                if (tenantId.HasValue)
                    query = query.Where(a => a.TenantId == tenantId.Value);

                var appointments = await query
                    .OrderBy(a => a.StartAt)
                    .ToListAsync();

                var result = appointments.Select(MapToAppointmentDto).ToList();
                return ApiResponse<List<AppointmentDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch hẹn hôm nay");
                return ApiResponse<List<AppointmentDto>>.ErrorResult("Có lỗi xảy ra khi lấy lịch hẹn hôm nay");
            }
        }
    }
}
