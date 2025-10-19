using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<ConsultationService> _logger;

        public ConsultationService(SavePlusDbContext context, ILogger<ConsultationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ConsultationDto>> CreateConsultationAsync(ConsultationCreateDto dto)
        {
            try
            {
                // Kiểm tra appointment có tồn tại không
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .FirstOrDefaultAsync(a => a.AppointmentId == dto.AppointmentId);

                if (appointment == null)
                {
                    return ApiResponse<ConsultationDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                // Kiểm tra consultation đã tồn tại cho appointment này chưa
                var existingConsultation = await _context.Consultations
                    .FirstOrDefaultAsync(c => c.AppointmentId == dto.AppointmentId);

                if (existingConsultation != null)
                {
                    return ApiResponse<ConsultationDto>.ErrorResult("Cuộc hẹn này đã có consultation");
                }

                // Tạo consultation mới
                var consultation = new Consultation
                {
                    AppointmentId = dto.AppointmentId,
                    Summary = dto.Summary,
                    DiagnosisCode = dto.DiagnosisCode,
                    Advice = dto.Advice,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();

                // Load lại consultation với thông tin appointment
                var result = await _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .FirstOrDefaultAsync(c => c.ConsultationId == consultation.ConsultationId);

                var consultationDto = MapToDto(result!);

                _logger.LogInformation("Đã tạo consultation mới với ID: {ConsultationId}", consultation.ConsultationId);

                return ApiResponse<ConsultationDto>.SuccessResult(consultationDto, "Tạo consultation thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo consultation");
                return ApiResponse<ConsultationDto>.ErrorResult("Có lỗi xảy ra khi tạo consultation");
            }
        }

        public async Task<ApiResponse<ConsultationDto>> GetConsultationByIdAsync(int consultationId)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .FirstOrDefaultAsync(c => c.ConsultationId == consultationId);

                if (consultation == null)
                {
                    return ApiResponse<ConsultationDto>.ErrorResult("Không tìm thấy consultation");
                }

                var consultationDto = MapToDto(consultation);

                return ApiResponse<ConsultationDto>.SuccessResult(consultationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation với ID: {ConsultationId}", consultationId);
                return ApiResponse<ConsultationDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin consultation");
            }
        }

        public async Task<ApiResponse<ConsultationDto>> UpdateConsultationAsync(int consultationId, ConsultationUpdateDto dto)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .FirstOrDefaultAsync(c => c.ConsultationId == consultationId);

                if (consultation == null)
                {
                    return ApiResponse<ConsultationDto>.ErrorResult("Không tìm thấy consultation");
                }

                // Cập nhật thông tin
                consultation.Summary = dto.Summary ?? consultation.Summary;
                consultation.DiagnosisCode = dto.DiagnosisCode ?? consultation.DiagnosisCode;
                consultation.Advice = dto.Advice ?? consultation.Advice;

                await _context.SaveChangesAsync();

                var consultationDto = MapToDto(consultation);

                _logger.LogInformation("Đã cập nhật consultation với ID: {ConsultationId}", consultationId);

                return ApiResponse<ConsultationDto>.SuccessResult(consultationDto, "Cập nhật consultation thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật consultation với ID: {ConsultationId}", consultationId);
                return ApiResponse<ConsultationDto>.ErrorResult("Có lỗi xảy ra khi cập nhật consultation");
            }
        }

        public async Task<ApiResponse<bool>> DeleteConsultationAsync(int consultationId)
        {
            try
            {
                var consultation = await _context.Consultations
                    .FirstOrDefaultAsync(c => c.ConsultationId == consultationId);

                if (consultation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy consultation");
                }

                _context.Consultations.Remove(consultation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa consultation với ID: {ConsultationId}", consultationId);

                return ApiResponse<bool>.SuccessResult(true, "Xóa consultation thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa consultation với ID: {ConsultationId}", consultationId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa consultation");
            }
        }

        public async Task<ApiResponse<PagedResult<ConsultationDto>>> GetConsultationsAsync(ConsultationFilterDto filter)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .AsQueryable();

                // Áp dụng filters
                if (filter.TenantId.HasValue)
                {
                    query = query.Where(c => c.Appointment.TenantId == filter.TenantId.Value);
                }

                if (filter.PatientId.HasValue)
                {
                    query = query.Where(c => c.Appointment.PatientId == filter.PatientId.Value);
                }

                if (filter.DoctorId.HasValue)
                {
                    query = query.Where(c => c.Appointment.DoctorId == filter.DoctorId.Value);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(filter.DiagnosisCode))
                {
                    query = query.Where(c => c.DiagnosisCode == filter.DiagnosisCode);
                }

                // Đếm tổng số
                var totalCount = await query.CountAsync();

                // Phân trang
                var consultations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var consultationDtos = consultations.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<ConsultationDto>
                {
                    Data = consultationDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResult<ConsultationDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách consultation");
                return ApiResponse<PagedResult<ConsultationDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách consultation");
            }
        }

        public async Task<ApiResponse<ConsultationDto>> GetConsultationByAppointmentIdAsync(int appointmentId)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);

                if (consultation == null)
                {
                    return ApiResponse<ConsultationDto>.ErrorResult("Không tìm thấy consultation cho cuộc hẹn này");
                }

                var consultationDto = MapToDto(consultation);

                return ApiResponse<ConsultationDto>.SuccessResult(consultationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation theo appointment ID: {AppointmentId}", appointmentId);
                return ApiResponse<ConsultationDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin consultation");
            }
        }

        public async Task<ApiResponse<List<ConsultationDto>>> GetPatientConsultationsAsync(int patientId, int? tenantId = null)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .Where(c => c.Appointment.PatientId == patientId);

                if (tenantId.HasValue)
                {
                    query = query.Where(c => c.Appointment.TenantId == tenantId.Value);
                }

                var consultations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var consultationDtos = consultations.Select(MapToDto).ToList();

                return ApiResponse<List<ConsultationDto>>.SuccessResult(consultationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation của bệnh nhân: {PatientId}", patientId);
                return ApiResponse<List<ConsultationDto>>.ErrorResult("Có lỗi xảy ra khi lấy consultation của bệnh nhân");
            }
        }

        public async Task<ApiResponse<List<ConsultationDto>>> GetDoctorConsultationsAsync(int doctorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .Where(c => c.Appointment.DoctorId == doctorId);

                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= toDate.Value);
                }

                var consultations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var consultationDtos = consultations.Select(MapToDto).ToList();

                return ApiResponse<List<ConsultationDto>>.SuccessResult(consultationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation của bác sĩ: {DoctorId}", doctorId);
                return ApiResponse<List<ConsultationDto>>.ErrorResult("Có lỗi xảy ra khi lấy consultation của bác sĩ");
            }
        }

        public async Task<ApiResponse<List<ConsultationDto>>> GetTenantConsultationsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .Where(c => c.Appointment.TenantId == tenantId);

                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= toDate.Value);
                }

                var consultations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var consultationDtos = consultations.Select(MapToDto).ToList();

                return ApiResponse<List<ConsultationDto>>.SuccessResult(consultationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy consultation của phòng khám: {TenantId}", tenantId);
                return ApiResponse<List<ConsultationDto>>.ErrorResult("Có lỗi xảy ra khi lấy consultation của phòng khám");
            }
        }

        public async Task<ApiResponse<ConsultationReportDto>> GetConsultationReportAsync(int? tenantId = null, int? doctorId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(c => c.Appointment.TenantId == tenantId.Value);
                }

                if (doctorId.HasValue)
                {
                    query = query.Where(c => c.Appointment.DoctorId == doctorId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= toDate.Value);
                }

                var consultations = await query.ToListAsync();

                var totalConsultations = consultations.Count;

                // Thống kê mã chẩn đoán
                var diagnosisCodes = consultations
                    .Where(c => !string.IsNullOrEmpty(c.DiagnosisCode))
                    .GroupBy(c => c.DiagnosisCode!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Consultation gần đây
                var recentConsultations = consultations
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .Select(MapToDto)
                    .ToList();

                var report = new ConsultationReportDto
                {
                    TotalConsultations = totalConsultations,
                    DiagnosisCodes = diagnosisCodes,
                    RecentConsultations = recentConsultations,
                    FromDate = fromDate ?? DateTime.MinValue,
                    ToDate = toDate ?? DateTime.MaxValue
                };

                return ApiResponse<ConsultationReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo consultation");
                return ApiResponse<ConsultationReportDto>.ErrorResult("Có lỗi xảy ra khi tạo báo cáo consultation");
            }
        }

        public async Task<ApiResponse<List<ConsultationDto>>> SearchConsultationsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(c => c.Appointment)
                    .ThenInclude(a => a.Tenant)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(c => c.Appointment.TenantId == tenantId.Value);
                }

                // Tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(c =>
                        (c.Summary != null && c.Summary.ToLower().Contains(keyword)) ||
                        (c.DiagnosisCode != null && c.DiagnosisCode.ToLower().Contains(keyword)) ||
                        (c.Advice != null && c.Advice.ToLower().Contains(keyword)) ||
                        c.Appointment.Patient.FullName.ToLower().Contains(keyword));
                }

                var consultations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var consultationDtos = consultations.Select(MapToDto).ToList();

                return ApiResponse<List<ConsultationDto>>.SuccessResult(consultationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm consultation với từ khóa: {Keyword}", keyword);
                return ApiResponse<List<ConsultationDto>>.ErrorResult("Có lỗi xảy ra khi tìm kiếm consultation");
            }
        }

        private static ConsultationDto MapToDto(Consultation consultation)
        {
            return new ConsultationDto
            {
                ConsultationId = consultation.ConsultationId,
                AppointmentId = consultation.AppointmentId,
                Summary = consultation.Summary,
                DiagnosisCode = consultation.DiagnosisCode,
                Advice = consultation.Advice,
                CreatedAt = consultation.CreatedAt,
                Appointment = consultation.Appointment != null ? new AppointmentDto
                {
                    AppointmentId = consultation.Appointment.AppointmentId,
                    TenantId = consultation.Appointment.TenantId,
                    PatientId = consultation.Appointment.PatientId,
                    DoctorId = consultation.Appointment.DoctorId,
                    StartAt = consultation.Appointment.StartAt,
                    EndAt = consultation.Appointment.EndAt,
                    Type = consultation.Appointment.Type,
                    Status = consultation.Appointment.Status,
                    Channel = consultation.Appointment.Channel,
                    Address = consultation.Appointment.Address,
                    PatientName = consultation.Appointment.Patient?.FullName,
                    DoctorName = consultation.Appointment.Doctor?.User?.FullName,
                    TenantName = consultation.Appointment.Tenant?.Name
                } : null
            };
        }
    }
}
