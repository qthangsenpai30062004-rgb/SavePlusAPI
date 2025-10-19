using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class TenantService : ITenantService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(SavePlusDbContext context, ILogger<TenantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<TenantDto>> CreateTenantAsync(TenantCreateDto dto)
        {
            try
            {
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Code == dto.Code);

                if (existingTenant != null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Mã tenant đã tồn tại");
                }

                var tenant = new Tenant
                {
                    Code = dto.Code,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    Description = dto.Description,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    CoverImageUrl = dto.CoverImageUrl,
                    WeekdayOpen = dto.WeekdayOpen,
                    WeekdayClose = dto.WeekdayClose,
                    WeekendOpen = dto.WeekendOpen,
                    WeekendClose = dto.WeekendClose,
                    OwnerUserId = dto.OwnerUserId
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                var result = MapToTenantDto(tenant);
                return ApiResponse<TenantDto>.SuccessResult(result, "Tạo tenant thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tenant");
                return ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi tạo tenant");
            }
        }

        public async Task<ApiResponse<TenantDto>> GetTenantByIdAsync(int tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .Include(t => t.OwnerUser)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Không tìm thấy tenant");
                }

                var result = MapToTenantDto(tenant);
                return ApiResponse<TenantDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin tenant {TenantId}", tenantId);
                return ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin tenant");
            }
        }

        public async Task<ApiResponse<TenantDto>> GetTenantByCodeAsync(string code)
        {
            try
            {
                var tenant = await _context.Tenants
                    .Include(t => t.OwnerUser)
                    .FirstOrDefaultAsync(t => t.Code == code);

                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Không tìm thấy tenant với mã này");
                }

                var result = MapToTenantDto(tenant);
                return ApiResponse<TenantDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm tenant theo mã {Code}", code);
                return ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi tìm tenant");
            }
        }

        public async Task<ApiResponse<TenantDto>> UpdateTenantAsync(int tenantId, TenantUpdateDto dto)
        {
            try
            {
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    return ApiResponse<TenantDto>.ErrorResult("Không tìm thấy tenant");
                }

                // Validate OwnerUserId if being updated
                if (dto.OwnerUserId.HasValue)
                {
                    var ownerUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == dto.OwnerUserId.Value
                                                && u.TenantId == tenantId
                                                && u.Role == "ClinicAdmin");
                    
                    if (ownerUser == null)
                    {
                        return ApiResponse<TenantDto>.ErrorResult(
                            "OwnerUserId phải là user thuộc tenant với Role = 'ClinicAdmin'"
                        );
                    }
                    tenant.OwnerUserId = dto.OwnerUserId;
                }
                // IMPORTANT: Database trigger validates OwnerUserId on ANY update
                // Must ensure current OwnerUserId is valid before updating ANY field
                else if (tenant.OwnerUserId == null)
                {
                    // Try to find a valid owner for this tenant
                    var validOwner = await _context.Users
                        .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Role == "ClinicAdmin");
                    
                    if (validOwner != null)
                    {
                        _logger.LogWarning(
                            "Tenant {TenantId} had null OwnerUserId, auto-assigning to User {UserId}",
                            tenantId, validOwner.UserId
                        );
                        tenant.OwnerUserId = validOwner.UserId;
                    }
                    else
                    {
                        return ApiResponse<TenantDto>.ErrorResult(
                            "Không thể cập nhật tenant: Chưa có user với Role='ClinicAdmin' trong tenant này. " +
                            "Vui lòng tạo hoặc chỉ định OwnerUserId hợp lệ."
                        );
                    }
                }
                else
                {
                    // Validate that current OwnerUserId is still valid
                    var currentOwner = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == tenant.OwnerUserId
                                                && u.TenantId == tenantId
                                                && u.Role == "ClinicAdmin");
                    
                    if (currentOwner == null)
                    {
                        _logger.LogWarning(
                            "Tenant {TenantId} has invalid OwnerUserId {OwnerUserId}, attempting to fix",
                            tenantId, tenant.OwnerUserId
                        );
                        
                        // Try to find a valid owner
                        var validOwner = await _context.Users
                            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Role == "ClinicAdmin");
                        
                        if (validOwner != null)
                        {
                            _logger.LogWarning(
                                "Auto-fixing Tenant {TenantId} OwnerUserId from {OldId} to {NewId}",
                                tenantId, tenant.OwnerUserId, validOwner.UserId
                            );
                            tenant.OwnerUserId = validOwner.UserId;
                        }
                        else
                        {
                            return ApiResponse<TenantDto>.ErrorResult(
                                "Không thể cập nhật tenant: OwnerUserId hiện tại không hợp lệ và không tìm thấy user thay thế. " +
                                "Vui lòng tạo user với Role='ClinicAdmin' hoặc chỉ định OwnerUserId hợp lệ."
                            );
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    tenant.Name = dto.Name;
                if (!string.IsNullOrEmpty(dto.Phone))
                    tenant.Phone = dto.Phone;
                if (!string.IsNullOrEmpty(dto.Email))
                    tenant.Email = dto.Email;
                if (!string.IsNullOrEmpty(dto.Address))
                    tenant.Address = dto.Address;
                if (dto.Status.HasValue)
                    tenant.Status = dto.Status.Value;
                if (dto.Description != null)
                    tenant.Description = dto.Description;
                if (dto.ThumbnailUrl != null)
                    tenant.ThumbnailUrl = dto.ThumbnailUrl;
                if (dto.CoverImageUrl != null)
                    tenant.CoverImageUrl = dto.CoverImageUrl;
                if (dto.WeekdayOpen != null)
                    tenant.WeekdayOpen = dto.WeekdayOpen;
                if (dto.WeekdayClose != null)
                    tenant.WeekdayClose = dto.WeekdayClose;
                if (dto.WeekendOpen != null)
                    tenant.WeekendOpen = dto.WeekendOpen;
                if (dto.WeekendClose != null)
                    tenant.WeekendClose = dto.WeekendClose;
                if (dto.OwnerUserId.HasValue)
                    tenant.OwnerUserId = dto.OwnerUserId;

                await _context.SaveChangesAsync();

                var result = MapToTenantDto(tenant);
                return ApiResponse<TenantDto>.SuccessResult(result, "Cập nhật tenant thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tenant {TenantId}", tenantId);
                return ApiResponse<TenantDto>.ErrorResult("Có lỗi xảy ra khi cập nhật tenant");
            }
        }

        public async Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Tenants.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(t => t.Name.Contains(searchTerm) || 
                                           t.Code.Contains(searchTerm) ||
                                           (t.Email != null && t.Email.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();
                var tenants = await query
                    .Include(t => t.OwnerUser)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<TenantDto>
                {
                    Data = tenants.Select(MapToTenantDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<TenantDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tenants");
                return ApiResponse<PagedResult<TenantDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách tenants");
            }
        }

        public async Task<ApiResponse<TenantStatsDto>> GetTenantStatsAsync(int tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    return ApiResponse<TenantStatsDto>.ErrorResult("Không tìm thấy tenant");
                }

                var totalPatients = await _context.ClinicPatients
                    .CountAsync(cp => cp.TenantId == tenantId);

                var totalDoctors = await _context.Doctors
                    .CountAsync(d => d.TenantId == tenantId && d.IsActive);

                var totalAppointments = await _context.Appointments
                    .CountAsync(a => a.TenantId == tenantId);

                var activeCarePlans = await _context.CarePlans
                    .CountAsync(cp => cp.TenantId == tenantId && cp.Status == "Active");

                var stats = new TenantStatsDto
                {
                    TenantId = tenantId,
                    Name = tenant.Name,
                    TotalPatients = totalPatients,
                    TotalDoctors = totalDoctors,
                    TotalAppointments = totalAppointments,
                    ActiveCarePlans = activeCarePlans
                };

                return ApiResponse<TenantStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê tenant {TenantId}", tenantId);
                return ApiResponse<TenantStatsDto>.ErrorResult("Có lỗi xảy ra khi lấy thống kê tenant");
            }
        }

        private TenantDto MapToTenantDto(Tenant tenant)
        {
            return new TenantDto
            {
                TenantId = tenant.TenantId,
                Code = tenant.Code,
                Name = tenant.Name,
                Phone = tenant.Phone,
                Email = tenant.Email,
                Address = tenant.Address,
                Status = tenant.Status,
                CreatedAt = tenant.CreatedAt,
                Description = tenant.Description,
                ThumbnailUrl = tenant.ThumbnailUrl,
                CoverImageUrl = tenant.CoverImageUrl,
                WeekdayOpen = tenant.WeekdayOpen,
                WeekdayClose = tenant.WeekdayClose,
                WeekendOpen = tenant.WeekendOpen,
                WeekendClose = tenant.WeekendClose,
                OwnerUserId = tenant.OwnerUserId,
                OwnerName = tenant.OwnerUser?.FullName
            };
        }

        public async Task<ApiResponse<PagedResult<DoctorDto>>> GetTenantDoctorsAsync(int tenantId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                // Kiểm tra tenant có tồn tại không
                var tenantExists = await _context.Tenants.AnyAsync(t => t.TenantId == tenantId);
                if (!tenantExists)
                {
                    return ApiResponse<PagedResult<DoctorDto>>.ErrorResult("Không tìm thấy tenant");
                }

                var query = _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Tenant)
                    .Where(d => d.TenantId == tenantId && d.User.IsActive);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(d => 
                        d.User.FullName.ToLower().Contains(searchLower) ||
                        (d.Specialty != null && d.Specialty.ToLower().Contains(searchLower)) ||
                        (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(searchLower)));
                }

                var totalCount = await query.CountAsync();
                var doctors = await query
                    .OrderBy(d => d.User.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new DoctorDto
                    {
                        DoctorId = d.DoctorId,
                        UserId = d.UserId,
                        TenantId = d.TenantId,
                        FullName = d.User.FullName,
                        Email = d.User.Email,
                        PhoneE164 = d.User.PhoneE164,
                        Specialty = d.Specialty,
                        LicenseNumber = d.LicenseNumber,
                        AvatarUrl = d.AvatarUrl,
                        Title = d.Title,
                        PositionTitle = d.PositionTitle,
                        YearStarted = d.YearStarted,
                        IsVerified = d.IsVerified,
                        About = d.About,
                        IsActive = d.User.IsActive,
                        CreatedAt = d.CreatedAt,
                        TenantName = d.Tenant != null ? d.Tenant.Name : null
                    })
                    .ToListAsync();

                var result = new PagedResult<DoctorDto>
                {
                    Data = doctors,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<DoctorDto>>.SuccessResult(result, $"Lấy danh sách bác sĩ thành công. Tổng: {totalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors for tenant {TenantId}", tenantId);
                return ApiResponse<PagedResult<DoctorDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách bác sĩ");
            }
        }

        public async Task<ApiResponse<DoctorDto>> GetTenantDoctorAsync(int tenantId, int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Tenant)
                    .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.DoctorId == doctorId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorDto>.ErrorResult("Không tìm thấy bác sĩ trong tenant này");
                }

                var result = new DoctorDto
                {
                    DoctorId = doctor.DoctorId,
                    UserId = doctor.UserId,
                    TenantId = doctor.TenantId,
                    FullName = doctor.User.FullName,
                    Email = doctor.User.Email,
                    PhoneE164 = doctor.User.PhoneE164,
                    Specialty = doctor.Specialty,
                    LicenseNumber = doctor.LicenseNumber,
                    AvatarUrl = doctor.AvatarUrl,
                    Title = doctor.Title,
                    PositionTitle = doctor.PositionTitle,
                    YearStarted = doctor.YearStarted,
                    IsVerified = doctor.IsVerified,
                    About = doctor.About,
                    IsActive = doctor.User.IsActive,
                    CreatedAt = doctor.CreatedAt,
                    TenantName = doctor.Tenant?.Name
                };

                return ApiResponse<DoctorDto>.SuccessResult(result, "Lấy thông tin bác sĩ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor {DoctorId} for tenant {TenantId}", doctorId, tenantId);
                return ApiResponse<DoctorDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin bác sĩ");
            }
        }

        public async Task<ApiResponse<DoctorDto>> UpdateTenantDoctorAsync(int tenantId, int doctorId, UpdateDoctorDto dto)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Tenant)
                    .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.DoctorId == doctorId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorDto>.ErrorResult("Không tìm thấy bác sĩ trong tenant này");
                }

                // Update doctor fields
                if (dto.Specialty != null)
                    doctor.Specialty = dto.Specialty;
                if (dto.LicenseNumber != null)
                    doctor.LicenseNumber = dto.LicenseNumber;
                if (dto.AvatarUrl != null)
                    doctor.AvatarUrl = dto.AvatarUrl;
                if (dto.Title != null)
                    doctor.Title = dto.Title;
                if (dto.PositionTitle != null)
                    doctor.PositionTitle = dto.PositionTitle;
                if (dto.YearStarted.HasValue)
                    doctor.YearStarted = dto.YearStarted;
                if (dto.IsVerified.HasValue)
                    doctor.IsVerified = dto.IsVerified.Value;
                if (dto.About != null)
                    doctor.About = dto.About;
                if (dto.IsActive.HasValue)
                    doctor.User.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                var result = new DoctorDto
                {
                    DoctorId = doctor.DoctorId,
                    UserId = doctor.UserId,
                    TenantId = doctor.TenantId,
                    FullName = doctor.User.FullName,
                    Email = doctor.User.Email,
                    PhoneE164 = doctor.User.PhoneE164,
                    Specialty = doctor.Specialty,
                    LicenseNumber = doctor.LicenseNumber,
                    AvatarUrl = doctor.AvatarUrl,
                    Title = doctor.Title,
                    PositionTitle = doctor.PositionTitle,
                    YearStarted = doctor.YearStarted,
                    IsVerified = doctor.IsVerified,
                    About = doctor.About,
                    IsActive = doctor.User.IsActive,
                    CreatedAt = doctor.CreatedAt,
                    TenantName = doctor.Tenant?.Name
                };

                return ApiResponse<DoctorDto>.SuccessResult(result, "Cập nhật thông tin bác sĩ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor {DoctorId} for tenant {TenantId}", doctorId, tenantId);
                return ApiResponse<DoctorDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông tin bác sĩ");
            }
        }
    }
}
