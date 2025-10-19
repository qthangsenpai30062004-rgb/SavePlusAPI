using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class PatientService : IPatientService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<PatientService> _logger;

        public PatientService(SavePlusDbContext context, ILogger<PatientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PatientDto>> RegisterPatientAsync(PatientRegistrationDto dto)
        {
            try
            {
                // Kiểm tra xem bệnh nhân đã tồn tại chưa
                var existingPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PrimaryPhoneE164 == dto.PrimaryPhoneE164);

                if (existingPatient != null)
                {
                    return ApiResponse<PatientDto>.ErrorResult("Bệnh nhân với số điện thoại này đã tồn tại");
                }

                var patient = new Patient
                {
                    FullName = dto.FullName,
                    PrimaryPhoneE164 = dto.PrimaryPhoneE164,
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var result = MapToPatientDto(patient);
                return ApiResponse<PatientDto>.SuccessResult(result, "Đăng ký bệnh nhân thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký bệnh nhân");
                return ApiResponse<PatientDto>.ErrorResult("Có lỗi xảy ra khi đăng ký bệnh nhân");
            }
        }

        public async Task<ApiResponse<PatientDto>> GetPatientByIdAsync(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    return ApiResponse<PatientDto>.ErrorResult("Không tìm thấy bệnh nhân");
                }

                var result = MapToPatientDto(patient);
                return ApiResponse<PatientDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin bệnh nhân {PatientId}", patientId);
                return ApiResponse<PatientDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin bệnh nhân");
            }
        }

        public async Task<ApiResponse<PatientDto>> GetPatientByPhoneAsync(string phoneNumber)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PrimaryPhoneE164 == phoneNumber);

                if (patient == null)
                {
                    return ApiResponse<PatientDto>.ErrorResult("Không tìm thấy bệnh nhân với số điện thoại này");
                }

                var result = MapToPatientDto(patient);
                return ApiResponse<PatientDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm bệnh nhân theo số điện thoại {PhoneNumber}", phoneNumber);
                return ApiResponse<PatientDto>.ErrorResult("Có lỗi xảy ra khi tìm bệnh nhân");
            }
        }

        public async Task<ApiResponse<PatientDto>> UpdatePatientAsync(int patientId, PatientUpdateDto dto)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    return ApiResponse<PatientDto>.ErrorResult("Không tìm thấy bệnh nhân");
                }

                if (!string.IsNullOrEmpty(dto.FullName))
                    patient.FullName = dto.FullName;
                if (dto.Gender != null)
                    patient.Gender = dto.Gender;
                if (dto.DateOfBirth.HasValue)
                    patient.DateOfBirth = dto.DateOfBirth;
                if (!string.IsNullOrEmpty(dto.Address))
                    patient.Address = dto.Address;

                await _context.SaveChangesAsync();

                var result = MapToPatientDto(patient);
                return ApiResponse<PatientDto>.SuccessResult(result, "Cập nhật thông tin bệnh nhân thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bệnh nhân {PatientId}", patientId);
                return ApiResponse<PatientDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông tin bệnh nhân");
            }
        }

        public async Task<ApiResponse<PagedResult<PatientDto>>> GetPatientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Patients.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.FullName.Contains(searchTerm) || p.PrimaryPhoneE164.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var patients = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<PatientDto>
                {
                    Data = patients.Select(MapToPatientDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<PatientDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bệnh nhân");
                return ApiResponse<PagedResult<PatientDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách bệnh nhân");
            }
        }

        public async Task<ApiResponse<ClinicPatientDto>> EnrollPatientToClinicAsync(int tenantId, int patientId, string? mrn = null, int? primaryDoctorId = null)
        {
            try
            {
                // Kiểm tra xem bệnh nhân đã được đăng ký vào phòng khám chưa
                var existingEnrollment = await _context.ClinicPatients
                    .FirstOrDefaultAsync(cp => cp.TenantId == tenantId && cp.PatientId == patientId);

                if (existingEnrollment != null)
                {
                    return ApiResponse<ClinicPatientDto>.ErrorResult("Bệnh nhân đã được đăng ký vào phòng khám này");
                }

                var clinicPatient = new ClinicPatient
                {
                    TenantId = tenantId,
                    PatientId = patientId,
                    Mrn = mrn,
                    PrimaryDoctorId = primaryDoctorId,
                    Status = 1, // Active
                    EnrolledAt = DateTime.UtcNow
                };

                _context.ClinicPatients.Add(clinicPatient);
                await _context.SaveChangesAsync();

                var result = await GetClinicPatientAsync(tenantId, patientId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký bệnh nhân vào phòng khám");
                return ApiResponse<ClinicPatientDto>.ErrorResult("Có lỗi xảy ra khi đăng ký bệnh nhân vào phòng khám");
            }
        }

        public async Task<ApiResponse<PagedResult<ClinicPatientDto>>> GetClinicPatientsAsync(int tenantId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.ClinicPatients
                    .Include(cp => cp.Patient)
                    .Include(cp => cp.Tenant)
                    .Include(cp => cp.PrimaryDoctor)
                    .ThenInclude(d => d.User)
                    .Where(cp => cp.TenantId == tenantId);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(cp => cp.Patient.FullName.Contains(searchTerm) || 
                                            cp.Patient.PrimaryPhoneE164.Contains(searchTerm) ||
                                            (cp.Mrn != null && cp.Mrn.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();
                var clinicPatients = await query
                    .OrderByDescending(cp => cp.EnrolledAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<ClinicPatientDto>
                {
                    Data = clinicPatients.Select(MapToClinicPatientDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<ClinicPatientDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bệnh nhân phòng khám");
                return ApiResponse<PagedResult<ClinicPatientDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách bệnh nhân");
            }
        }

        public async Task<ApiResponse<ClinicPatientDto>> GetClinicPatientAsync(int tenantId, int patientId)
        {
            try
            {
                var clinicPatient = await _context.ClinicPatients
                    .Include(cp => cp.Patient)
                    .Include(cp => cp.Tenant)
                    .Include(cp => cp.PrimaryDoctor)
                    .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(cp => cp.TenantId == tenantId && cp.PatientId == patientId);

                if (clinicPatient == null)
                {
                    return ApiResponse<ClinicPatientDto>.ErrorResult("Không tìm thấy bệnh nhân trong phòng khám này");
                }

                var result = MapToClinicPatientDto(clinicPatient);
                return ApiResponse<ClinicPatientDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin bệnh nhân phòng khám");
                return ApiResponse<ClinicPatientDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin bệnh nhân");
            }
        }

        public async Task<ApiResponse<ClinicPatientDto>> UpdateClinicPatientAsync(int tenantId, int patientId, string? mrn = null, int? primaryDoctorId = null, byte? status = null)
        {
            try
            {
                var clinicPatient = await _context.ClinicPatients
                    .FirstOrDefaultAsync(cp => cp.TenantId == tenantId && cp.PatientId == patientId);

                if (clinicPatient == null)
                {
                    return ApiResponse<ClinicPatientDto>.ErrorResult("Không tìm thấy bệnh nhân trong phòng khám này");
                }

                if (mrn != null)
                    clinicPatient.Mrn = mrn;
                if (primaryDoctorId.HasValue)
                    clinicPatient.PrimaryDoctorId = primaryDoctorId;
                if (status.HasValue)
                    clinicPatient.Status = status.Value;

                await _context.SaveChangesAsync();

                var result = await GetClinicPatientAsync(tenantId, patientId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bệnh nhân phòng khám");
                return ApiResponse<ClinicPatientDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông tin bệnh nhân");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> AuthenticatePatientAsync(string phoneNumber, string verificationCode)
        {
            try
            {
                // TODO: Implement OTP verification logic
                // For now, just check if patient exists
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PrimaryPhoneE164 == phoneNumber);

                if (patient == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Số điện thoại chưa được đăng ký");
                }

                // TODO: Generate JWT token
                var authResponse = new AuthResponseDto
                {
                    Token = "dummy_token", // Replace with actual JWT token
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    User = new UserInfoDto
                    {
                        UserId = patient.PatientId,
                        FullName = patient.FullName,
                        PhoneE164 = patient.PrimaryPhoneE164
                    }
                };

                return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực bệnh nhân");
                return ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi đăng nhập");
            }
        }

        public async Task<ApiResponse<List<ClinicPatientDto>>> FindPatientInClinicsAsync(string phoneNumber)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PrimaryPhoneE164 == phoneNumber);

                if (patient == null)
                {
                    return ApiResponse<List<ClinicPatientDto>>.SuccessResult(new List<ClinicPatientDto>(), "Không tìm thấy bệnh nhân");
                }

                var clinicPatients = await _context.ClinicPatients
                    .Include(cp => cp.Tenant)
                    .Include(cp => cp.PrimaryDoctor)
                    .ThenInclude(d => d.User)
                    .Where(cp => cp.PatientId == patient.PatientId)
                    .ToListAsync();

                var result = clinicPatients.Select(cp => MapToClinicPatientDto(cp)).ToList();
                return ApiResponse<List<ClinicPatientDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm bệnh nhân trong các phòng khám");
                return ApiResponse<List<ClinicPatientDto>>.ErrorResult("Có lỗi xảy ra khi tìm kiếm bệnh nhân");
            }
        }

        public async Task<ApiResponse<List<PatientSearchDto>>> SearchPatientsInTenantAsync(int tenantId, string searchTerm, int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                {
                    return ApiResponse<List<PatientSearchDto>>.SuccessResult(new List<PatientSearchDto>());
                }

                var searchTermLower = searchTerm.ToLower();

                var patients = await _context.ClinicPatients
                    .Include(cp => cp.Patient)
                    .Where(cp => cp.TenantId == tenantId &&
                                cp.Status == 1 && // Active status
                                (cp.Patient.FullName.ToLower().Contains(searchTermLower) ||
                                 cp.Patient.PrimaryPhoneE164.Contains(searchTerm) ||
                                 (cp.Mrn != null && cp.Mrn.ToLower().Contains(searchTermLower))))
                    .OrderBy(cp => cp.Patient.FullName)
                    .Take(limit)
                    .Select(cp => new PatientSearchDto
                    {
                        PatientId = cp.PatientId,
                        FullName = cp.Patient.FullName,
                        PrimaryPhoneE164 = cp.Patient.PrimaryPhoneE164,
                        Mrn = cp.Mrn,
                        DateOfBirth = cp.Patient.DateOfBirth
                    })
                    .ToListAsync();

                return ApiResponse<List<PatientSearchDto>>.SuccessResult(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bệnh nhân trong tenant {TenantId}", tenantId);
                return ApiResponse<List<PatientSearchDto>>.ErrorResult("Có lỗi xảy ra khi tìm kiếm bệnh nhân");
            }
        }

        private PatientDto MapToPatientDto(Patient patient)
        {
            return new PatientDto
            {
                PatientId = patient.PatientId,
                FullName = patient.FullName,
                PrimaryPhoneE164 = patient.PrimaryPhoneE164,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }

        private ClinicPatientDto MapToClinicPatientDto(ClinicPatient clinicPatient)
        {
            return new ClinicPatientDto
            {
                TenantId = clinicPatient.TenantId,
                PatientId = clinicPatient.PatientId,
                Mrn = clinicPatient.Mrn,
                PrimaryDoctorId = clinicPatient.PrimaryDoctorId,
                Status = clinicPatient.Status,
                EnrolledAt = clinicPatient.EnrolledAt,
                Patient = MapToPatientDto(clinicPatient.Patient),
                TenantName = clinicPatient.Tenant?.Name,
                DoctorName = clinicPatient.PrimaryDoctor?.User?.FullName
            };
        }
    }
}
