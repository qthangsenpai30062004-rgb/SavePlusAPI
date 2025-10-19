using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<MedicalRecordService> _logger;
        private readonly IWebHostEnvironment _environment;

        public MedicalRecordService(SavePlusDbContext context, ILogger<MedicalRecordService> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public async Task<ApiResponse<MedicalRecordDto>> CreateMedicalRecordAsync(MedicalRecordCreateDto dto)
        {
            try
            {
                // Kiểm tra patient có tồn tại không
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == dto.PatientId);
                if (patient == null)
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy bệnh nhân");
                }

                // Kiểm tra tenant có tồn tại không
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == dto.TenantId);
                if (tenant == null)
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy phòng khám");
                }

                // Kiểm tra user tạo (nếu có)
                if (dto.CreatedByUserId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.CreatedByUserId.Value);
                    if (user == null)
                    {
                        return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy người dùng tạo hồ sơ");
                    }
                }

                // Validate FileUrl format (cho phép cả absolute URL và relative path)
                if (!IsValidFileUrl(dto.FileUrl))
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("URL file không hợp lệ. Chỉ chấp nhận URL hoặc đường dẫn tương đối hợp lệ.");
                }

                // Tạo medical record mới
                var medicalRecord = new MedicalRecord
                {
                    TenantId = dto.TenantId,
                    PatientId = dto.PatientId,
                    Title = dto.Title,
                    FileUrl = dto.FileUrl,
                    RecordType = dto.RecordType,
                    CreatedByUserId = dto.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalRecords.Add(medicalRecord);
                await _context.SaveChangesAsync();

                // Load lại medical record với thông tin liên quan
                var result = await _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .FirstOrDefaultAsync(mr => mr.RecordId == medicalRecord.RecordId);

                var medicalRecordDto = MapToDto(result!);

                _logger.LogInformation("Đã tạo medical record mới với ID: {RecordId}", medicalRecord.RecordId);

                return ApiResponse<MedicalRecordDto>.SuccessResult(medicalRecordDto, "Tạo hồ sơ y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo medical record");
                return ApiResponse<MedicalRecordDto>.ErrorResult("Có lỗi xảy ra khi tạo hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<MedicalRecordDto>> GetMedicalRecordByIdAsync(long recordId)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .FirstOrDefaultAsync(mr => mr.RecordId == recordId);

                if (medicalRecord == null)
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy hồ sơ y tế");
                }

                var medicalRecordDto = MapToDto(medicalRecord);

                return ApiResponse<MedicalRecordDto>.SuccessResult(medicalRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy medical record với ID: {RecordId}", recordId);
                return ApiResponse<MedicalRecordDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<MedicalRecordDto>> UpdateMedicalRecordAsync(long recordId, MedicalRecordUpdateDto dto)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .FirstOrDefaultAsync(mr => mr.RecordId == recordId);

                if (medicalRecord == null)
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy hồ sơ y tế");
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(dto.Title))
                    medicalRecord.Title = dto.Title;
                
                if (!string.IsNullOrEmpty(dto.FileUrl))
                    medicalRecord.FileUrl = dto.FileUrl;
                
                if (!string.IsNullOrEmpty(dto.RecordType))
                    medicalRecord.RecordType = dto.RecordType;

                await _context.SaveChangesAsync();

                var medicalRecordDto = MapToDto(medicalRecord);

                _logger.LogInformation("Đã cập nhật medical record với ID: {RecordId}", recordId);

                return ApiResponse<MedicalRecordDto>.SuccessResult(medicalRecordDto, "Cập nhật hồ sơ y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật medical record với ID: {RecordId}", recordId);
                return ApiResponse<MedicalRecordDto>.ErrorResult("Có lỗi xảy ra khi cập nhật hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<bool>> DeleteMedicalRecordAsync(long recordId)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .FirstOrDefaultAsync(mr => mr.RecordId == recordId);

                if (medicalRecord == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy hồ sơ y tế");
                }

                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa medical record với ID: {RecordId}", recordId);

                return ApiResponse<bool>.SuccessResult(true, "Xóa hồ sơ y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa medical record với ID: {RecordId}", recordId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<PagedResult<MedicalRecordDto>>> GetMedicalRecordsAsync(MedicalRecordFilterDto filter)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .AsQueryable();

                // Áp dụng filters
                if (filter.TenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == filter.TenantId.Value);
                }

                if (filter.PatientId.HasValue)
                {
                    query = query.Where(mr => mr.PatientId == filter.PatientId.Value);
                }

                if (filter.CreatedByUserId.HasValue)
                {
                    query = query.Where(mr => mr.CreatedByUserId == filter.CreatedByUserId.Value);
                }

                if (!string.IsNullOrEmpty(filter.RecordType))
                {
                    query = query.Where(mr => mr.RecordType == filter.RecordType);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt <= filter.ToDate.Value);
                }

                // Đếm tổng số
                var totalCount = await query.CountAsync();

                // Phân trang
                var medicalRecords = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var medicalRecordDtos = medicalRecords.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<MedicalRecordDto>
                {
                    Data = medicalRecordDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResult<MedicalRecordDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách medical record");
                return ApiResponse<PagedResult<MedicalRecordDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<List<MedicalRecordDto>>> GetPatientMedicalRecordsAsync(int patientId, int? tenantId = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .Where(mr => mr.PatientId == patientId);

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                var medicalRecords = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .ToListAsync();

                var medicalRecordDtos = medicalRecords.Select(MapToDto).ToList();

                return ApiResponse<List<MedicalRecordDto>>.SuccessResult(medicalRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy medical record của bệnh nhân: {PatientId}", patientId);
                return ApiResponse<List<MedicalRecordDto>>.ErrorResult("Có lỗi xảy ra khi lấy hồ sơ y tế của bệnh nhân");
            }
        }

        public async Task<ApiResponse<List<MedicalRecordDto>>> GetTenantMedicalRecordsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .Where(mr => mr.TenantId == tenantId);

                if (fromDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt <= toDate.Value);
                }

                var medicalRecords = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .ToListAsync();

                var medicalRecordDtos = medicalRecords.Select(MapToDto).ToList();

                return ApiResponse<List<MedicalRecordDto>>.SuccessResult(medicalRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy medical record của phòng khám: {TenantId}", tenantId);
                return ApiResponse<List<MedicalRecordDto>>.ErrorResult("Có lỗi xảy ra khi lấy hồ sơ y tế của phòng khám");
            }
        }

        public async Task<ApiResponse<List<MedicalRecordDto>>> GetMedicalRecordsByTypeAsync(string recordType, int? tenantId = null, int? patientId = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .Where(mr => mr.RecordType == recordType);

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(mr => mr.PatientId == patientId.Value);
                }

                var medicalRecords = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .ToListAsync();

                var medicalRecordDtos = medicalRecords.Select(MapToDto).ToList();

                return ApiResponse<List<MedicalRecordDto>>.SuccessResult(medicalRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy medical record theo loại: {RecordType}", recordType);
                return ApiResponse<List<MedicalRecordDto>>.ErrorResult("Có lỗi xảy ra khi lấy hồ sơ y tế theo loại");
            }
        }

        public async Task<ApiResponse<MedicalRecordReportDto>> GetMedicalRecordReportAsync(int? tenantId = null, int? patientId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(mr => mr.PatientId == patientId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(mr => mr.CreatedAt <= toDate.Value);
                }

                var medicalRecords = await query.ToListAsync();

                var totalRecords = medicalRecords.Count;

                // Thống kê theo loại hồ sơ
                var recordTypes = medicalRecords
                    .Where(mr => !string.IsNullOrEmpty(mr.RecordType))
                    .GroupBy(mr => mr.RecordType!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Thống kê theo tháng
                var recordsByMonth = medicalRecords
                    .GroupBy(mr => mr.CreatedAt.ToString("yyyy-MM"))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Hồ sơ gần đây
                var recentRecords = medicalRecords
                    .OrderByDescending(mr => mr.CreatedAt)
                    .Take(10)
                    .Select(MapToDto)
                    .ToList();

                var report = new MedicalRecordReportDto
                {
                    TotalRecords = totalRecords,
                    RecordTypes = recordTypes,
                    RecordsByMonth = recordsByMonth,
                    RecentRecords = recentRecords,
                    FromDate = fromDate ?? DateTime.MinValue,
                    ToDate = toDate ?? DateTime.MaxValue
                };

                return ApiResponse<MedicalRecordReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo medical record");
                return ApiResponse<MedicalRecordReportDto>.ErrorResult("Có lỗi xảy ra khi tạo báo cáo hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<List<MedicalRecordDto>>> SearchMedicalRecordsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                // Tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(mr =>
                        mr.Title.ToLower().Contains(keyword) ||
                        (mr.RecordType != null && mr.RecordType.ToLower().Contains(keyword)) ||
                        mr.Patient.FullName.ToLower().Contains(keyword));
                }

                var medicalRecords = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var medicalRecordDtos = medicalRecords.Select(MapToDto).ToList();

                return ApiResponse<List<MedicalRecordDto>>.SuccessResult(medicalRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm medical record với từ khóa: {Keyword}", keyword);
                return ApiResponse<List<MedicalRecordDto>>.ErrorResult("Có lỗi xảy ra khi tìm kiếm hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<MedicalRecordDto>> UploadMedicalRecordAsync(MedicalRecordUploadDto dto)
        {
            try
            {
                // Tạo thư mục upload nếu chưa tồn tại
                var uploadPath = Path.Combine(_environment.WebRootPath ?? "", "uploads", "medical-records");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                // Tạo URL file
                var fileUrl = $"/uploads/medical-records/{fileName}";

                // Tạo medical record
                var createDto = new MedicalRecordCreateDto
                {
                    TenantId = dto.TenantId,
                    PatientId = dto.PatientId,
                    Title = dto.Title,
                    FileUrl = fileUrl,
                    RecordType = dto.RecordType,
                    CreatedByUserId = dto.CreatedByUserId
                };

                return await CreateMedicalRecordAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload medical record");
                return ApiResponse<MedicalRecordDto>.ErrorResult("Có lỗi xảy ra khi upload hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<byte[]>> DownloadMedicalRecordFileAsync(long recordId)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .FirstOrDefaultAsync(mr => mr.RecordId == recordId);

                if (medicalRecord == null)
                {
                    return ApiResponse<byte[]>.ErrorResult("Không tìm thấy hồ sơ y tế");
                }

                var filePath = Path.Combine(_environment.WebRootPath ?? "", medicalRecord.FileUrl.TrimStart('/'));

                if (!File.Exists(filePath))
                {
                    return ApiResponse<byte[]>.ErrorResult("Không tìm thấy file");
                }

                var fileBytes = await File.ReadAllBytesAsync(filePath);

                return ApiResponse<byte[]>.SuccessResult(fileBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi download medical record file: {RecordId}", recordId);
                return ApiResponse<byte[]>.ErrorResult("Có lỗi xảy ra khi tải file");
            }
        }

        public async Task<ApiResponse<PatientMedicalRecordSummaryDto>> GetPatientMedicalRecordSummaryAsync(int patientId, int? tenantId = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .Where(mr => mr.PatientId == patientId);

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                var medicalRecords = await query.ToListAsync();

                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);

                var summary = new PatientMedicalRecordSummaryDto
                {
                    PatientId = patientId,
                    PatientName = patient?.FullName,
                    TotalRecords = medicalRecords.Count,
                    RecordTypeCount = medicalRecords
                        .Where(mr => !string.IsNullOrEmpty(mr.RecordType))
                        .GroupBy(mr => mr.RecordType!)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    LastRecordDate = medicalRecords.Any() ? medicalRecords.Max(mr => mr.CreatedAt) : null,
                    RecentRecords = medicalRecords
                        .OrderByDescending(mr => mr.CreatedAt)
                        .Take(5)
                        .Select(MapToDto)
                        .ToList()
                };

                return ApiResponse<PatientMedicalRecordSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê medical record của bệnh nhân: {PatientId}", patientId);
                return ApiResponse<PatientMedicalRecordSummaryDto>.ErrorResult("Có lỗi xảy ra khi lấy thống kê hồ sơ y tế");
            }
        }

        public async Task<ApiResponse<List<string>>> GetUsedRecordTypesAsync(int? tenantId = null)
        {
            try
            {
                var query = _context.MedicalRecords.AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                var recordTypes = await query
                    .Where(mr => !string.IsNullOrEmpty(mr.RecordType))
                    .Select(mr => mr.RecordType!)
                    .Distinct()
                    .OrderBy(rt => rt)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResult(recordTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách loại hồ sơ");
                return ApiResponse<List<string>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách loại hồ sơ");
            }
        }

        public async Task<ApiResponse<bool>> CheckMedicalRecordAccessAsync(long recordId, int userId)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.Tenant)
                    .FirstOrDefaultAsync(mr => mr.RecordId == recordId);

                if (medicalRecord == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy hồ sơ y tế");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy người dùng");
                }

                // Kiểm tra quyền truy cập
                var hasAccess = user.TenantId == medicalRecord.TenantId || 
                               medicalRecord.CreatedByUserId == userId ||
                               user.Role == "Admin" || 
                               user.Role == "SuperAdmin";

                return ApiResponse<bool>.SuccessResult(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền truy cập medical record: {RecordId}, User: {UserId}", recordId, userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi kiểm tra quyền truy cập");
            }
        }

        public async Task<ApiResponse<MedicalRecordDto>> GetLatestPatientMedicalRecordAsync(int patientId, int? tenantId = null, string? recordType = null)
        {
            try
            {
                var query = _context.MedicalRecords
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Tenant)
                    .Include(mr => mr.CreatedByUser)
                    .Where(mr => mr.PatientId == patientId);

                if (tenantId.HasValue)
                {
                    query = query.Where(mr => mr.TenantId == tenantId.Value);
                }

                if (!string.IsNullOrEmpty(recordType))
                {
                    query = query.Where(mr => mr.RecordType == recordType);
                }

                var latestRecord = await query
                    .OrderByDescending(mr => mr.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestRecord == null)
                {
                    return ApiResponse<MedicalRecordDto>.ErrorResult("Không tìm thấy hồ sơ y tế nào");
                }

                var medicalRecordDto = MapToDto(latestRecord);

                return ApiResponse<MedicalRecordDto>.SuccessResult(medicalRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ y tế gần đây nhất của bệnh nhân: {PatientId}", patientId);
                return ApiResponse<MedicalRecordDto>.ErrorResult("Có lỗi xảy ra khi lấy hồ sơ y tế gần đây nhất");
            }
        }

        private static MedicalRecordDto MapToDto(MedicalRecord medicalRecord)
        {
            return new MedicalRecordDto
            {
                RecordId = medicalRecord.RecordId,
                TenantId = medicalRecord.TenantId,
                PatientId = medicalRecord.PatientId,
                Title = medicalRecord.Title,
                FileUrl = medicalRecord.FileUrl,
                RecordType = medicalRecord.RecordType,
                CreatedByUserId = medicalRecord.CreatedByUserId,
                CreatedAt = medicalRecord.CreatedAt,
                PatientName = medicalRecord.Patient?.FullName,
                TenantName = medicalRecord.Tenant?.Name,
                CreatedByUserName = medicalRecord.CreatedByUser?.FullName
            };
        }

        private static bool IsValidFileUrl(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            // Cho phép absolute URL (http/https)
            if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri) && 
                (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }

            // Cho phép relative path (bắt đầu với /)
            if (fileUrl.StartsWith('/'))
            {
                return Uri.TryCreate(fileUrl, UriKind.Relative, out _);
            }

            // Cho phép relative path không bắt đầu với /
            return Uri.TryCreate(fileUrl, UriKind.Relative, out _);
        }
    }
}
