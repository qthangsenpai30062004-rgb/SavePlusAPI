using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using SavePlus_API.Constants;

namespace SavePlus_API.Services
{
    public class CarePlanService : ICarePlanService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<CarePlanService> _logger;

        public CarePlanService(SavePlusDbContext context, ILogger<CarePlanService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<CarePlanDto>> CreateCarePlanAsync(CarePlanCreateDto dto, int tenantId, int createdBy)
        {
            try
            {
                // Validate patient exists and belongs to tenant
                var patient = await _context.Patients
                    .Include(p => p.ClinicPatients)
                    .FirstOrDefaultAsync(p => p.PatientId == dto.PatientId && 
                        p.ClinicPatients.Any(cp => cp.TenantId == tenantId));

                if (patient == null)
                {
                    return ServiceResult<CarePlanDto>.ErrorResult("Bệnh nhân không tồn tại hoặc không thuộc phòng khám này");
                }

                // Check for duplicate CarePlan name for this patient
                var existingCarePlan = await _context.CarePlans
                    .FirstOrDefaultAsync(cp => cp.TenantId == tenantId && 
                        cp.PatientId == dto.PatientId && 
                        cp.Name == dto.Name);

                if (existingCarePlan != null)
                {
                    return ServiceResult<CarePlanDto>.ErrorResult("Kế hoạch chăm sóc với tên này đã tồn tại cho bệnh nhân");
                }

                // Create CarePlan
                var carePlan = new CarePlan
                {
                    TenantId = tenantId,
                    PatientId = dto.PatientId,
                    Name = dto.Name,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = dto.Status,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CarePlans.Add(carePlan);
                await _context.SaveChangesAsync();

                // Create CarePlan Items if provided
                if (dto.Items.Any())
                {
                    var carePlanItems = dto.Items.Select(item => new CarePlanItem
                    {
                        CarePlanId = carePlan.CarePlanId,
                        ItemType = item.ItemType,
                        Title = item.Title,
                        Description = item.Description,
                        FrequencyCron = item.FrequencyCron,
                        TimesPerDay = item.TimesPerDay,
                        DaysOfWeek = item.DaysOfWeek,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        IsActive = item.IsActive
                    }).ToList();

                    _context.CarePlanItems.AddRange(carePlanItems);
                    await _context.SaveChangesAsync();
                }

                // Return created CarePlan
                return await GetCarePlanByIdAsync(carePlan.CarePlanId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CarePlan for patient {PatientId}", dto.PatientId);
                return ServiceResult<CarePlanDto>.ErrorResult("Lỗi khi tạo kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanDto>> GetCarePlanByIdAsync(int carePlanId, int tenantId)
        {
            try
            {
                var carePlan = await _context.CarePlans
                    .Include(cp => cp.Patient)
                    .Include(cp => cp.CreatedByNavigation)
                    .Include(cp => cp.CarePlanItems)
                        .ThenInclude(cpi => cpi.CarePlanItemLogs)
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (carePlan == null)
                {
                    return ServiceResult<CarePlanDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại");
                }

                var carePlanDto = new CarePlanDto
                {
                    CarePlanId = carePlan.CarePlanId,
                    TenantId = carePlan.TenantId,
                    PatientId = carePlan.PatientId,
                    Name = carePlan.Name,
                    StartDate = carePlan.StartDate,
                    EndDate = carePlan.EndDate,
                    Status = carePlan.Status,
                    CreatedBy = carePlan.CreatedBy,
                    CreatedAt = carePlan.CreatedAt,
                    PatientName = carePlan.Patient.FullName,
                    CreatedByName = carePlan.CreatedByNavigation.FullName,
                    Items = carePlan.CarePlanItems.Select(item => new CarePlanItemDto
                    {
                        ItemId = item.ItemId,
                        CarePlanId = item.CarePlanId,
                        ItemType = item.ItemType,
                        Title = item.Title,
                        Description = item.Description,
                        FrequencyCron = item.FrequencyCron,
                        TimesPerDay = item.TimesPerDay,
                        DaysOfWeek = item.DaysOfWeek,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        IsActive = item.IsActive,
                        TotalLogs = item.CarePlanItemLogs.Count,
                        CompletedLogs = item.CarePlanItemLogs.Count(log => log.IsCompleted),
                        LastPerformed = item.CarePlanItemLogs.OrderByDescending(log => log.PerformedAt)
                            .FirstOrDefault()?.PerformedAt
                    }).ToList()
                };

                return ServiceResult<CarePlanDto>.SuccessResult(carePlanDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CarePlan {CarePlanId}", carePlanId);
                return ServiceResult<CarePlanDto>.ErrorResult("Lỗi khi lấy thông tin kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanDto>> UpdateCarePlanAsync(int carePlanId, CarePlanUpdateDto dto, int tenantId)
        {
            try
            {
                var carePlan = await _context.CarePlans
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (carePlan == null)
                {
                    return ServiceResult<CarePlanDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.Name))
                {
                    // Check for duplicate name (excluding current CarePlan)
                    var existingCarePlan = await _context.CarePlans
                        .FirstOrDefaultAsync(cp => cp.TenantId == tenantId && 
                            cp.PatientId == carePlan.PatientId && 
                            cp.Name == dto.Name && 
                            cp.CarePlanId != carePlanId);

                    if (existingCarePlan != null)
                    {
                        return ServiceResult<CarePlanDto>.ErrorResult("Kế hoạch chăm sóc với tên này đã tồn tại cho bệnh nhân");
                    }

                    carePlan.Name = dto.Name;
                }

                if (dto.StartDate.HasValue)
                    carePlan.StartDate = dto.StartDate.Value;

                if (dto.EndDate.HasValue)
                    carePlan.EndDate = dto.EndDate.Value;

                if (!string.IsNullOrEmpty(dto.Status))
                    carePlan.Status = dto.Status;

                await _context.SaveChangesAsync();

                return await GetCarePlanByIdAsync(carePlanId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CarePlan {CarePlanId}", carePlanId);
                return ServiceResult<CarePlanDto>.ErrorResult("Lỗi khi cập nhật kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<bool>> DeleteCarePlanAsync(int carePlanId, int tenantId)
        {
            try
            {
                var carePlan = await _context.CarePlans
                    .Include(cp => cp.CarePlanItems)
                        .ThenInclude(cpi => cpi.CarePlanItemLogs)
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (carePlan == null)
                {
                    return ServiceResult<bool>.ErrorResult("Kế hoạch chăm sóc không tồn tại");
                }

                // Delete related data first
                foreach (var item in carePlan.CarePlanItems)
                {
                    _context.CarePlanItemLogs.RemoveRange(item.CarePlanItemLogs);
                }
                _context.CarePlanItems.RemoveRange(carePlan.CarePlanItems);
                _context.CarePlans.Remove(carePlan);

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CarePlan {CarePlanId}", carePlanId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi xóa kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<PagedResult<CarePlanDto>>> GetCarePlansAsync(int tenantId, int? patientId = null, 
            string? status = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.CarePlans
                    .Include(cp => cp.Patient)
                    .Include(cp => cp.CreatedByNavigation)
                    .Include(cp => cp.CarePlanItems)
                        .ThenInclude(cpi => cpi.CarePlanItemLogs)
                    .Where(cp => cp.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(cp => cp.PatientId == patientId.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(cp => cp.Status == status);

                var totalCount = await query.CountAsync();
                var carePlans = await query
                    .OrderByDescending(cp => cp.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(cp => new CarePlanDto
                    {
                        CarePlanId = cp.CarePlanId,
                        TenantId = cp.TenantId,
                        PatientId = cp.PatientId,
                        Name = cp.Name,
                        StartDate = cp.StartDate,
                        EndDate = cp.EndDate,
                        Status = cp.Status,
                        CreatedBy = cp.CreatedBy,
                        CreatedAt = cp.CreatedAt,
                        PatientName = cp.Patient.FullName,
                        CreatedByName = cp.CreatedByNavigation.FullName,
                        Items = cp.CarePlanItems.Select(item => new CarePlanItemDto
                        {
                            ItemId = item.ItemId,
                            CarePlanId = item.CarePlanId,
                            ItemType = item.ItemType,
                            Title = item.Title,
                            Description = item.Description,
                            FrequencyCron = item.FrequencyCron,
                            TimesPerDay = item.TimesPerDay,
                            DaysOfWeek = item.DaysOfWeek,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            IsActive = item.IsActive,
                            TotalLogs = item.CarePlanItemLogs.Count,
                            CompletedLogs = item.CarePlanItemLogs.Count(log => log.IsCompleted),
                            LastPerformed = item.CarePlanItemLogs.OrderByDescending(log => log.PerformedAt)
                                .FirstOrDefault()!.PerformedAt
                        }).ToList()
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<CarePlanDto>
                {
                    Data = carePlans,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ServiceResult<PagedResult<CarePlanDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CarePlans for tenant {TenantId}", tenantId);
                return ServiceResult<PagedResult<CarePlanDto>>.ErrorResult("Lỗi khi lấy danh sách kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanItemDto>> CreateCarePlanItemAsync(int carePlanId, CarePlanItemCreateDto dto, int tenantId)
        {
            try
            {
                // Validate CarePlan exists and belongs to tenant
                var carePlan = await _context.CarePlans
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (carePlan == null)
                {
                    return ServiceResult<CarePlanItemDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại");
                }

                var carePlanItem = new CarePlanItem
                {
                    CarePlanId = carePlanId,
                    ItemType = dto.ItemType,
                    Title = dto.Title,
                    Description = dto.Description,
                    FrequencyCron = dto.FrequencyCron,
                    TimesPerDay = dto.TimesPerDay,
                    DaysOfWeek = dto.DaysOfWeek,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsActive = dto.IsActive
                };

                _context.CarePlanItems.Add(carePlanItem);
                await _context.SaveChangesAsync();

                var result = new CarePlanItemDto
                {
                    ItemId = carePlanItem.ItemId,
                    CarePlanId = carePlanItem.CarePlanId,
                    ItemType = carePlanItem.ItemType,
                    Title = carePlanItem.Title,
                    Description = carePlanItem.Description,
                    FrequencyCron = carePlanItem.FrequencyCron,
                    TimesPerDay = carePlanItem.TimesPerDay,
                    DaysOfWeek = carePlanItem.DaysOfWeek,
                    StartDate = carePlanItem.StartDate,
                    EndDate = carePlanItem.EndDate,
                    IsActive = carePlanItem.IsActive,
                    TotalLogs = 0,
                    CompletedLogs = 0,
                    LastPerformed = null
                };

                return ServiceResult<CarePlanItemDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CarePlanItem for CarePlan {CarePlanId}", carePlanId);
                return ServiceResult<CarePlanItemDto>.ErrorResult("Lỗi khi tạo item cho kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanItemDto>> UpdateCarePlanItemAsync(int itemId, CarePlanItemUpdateDto dto, int tenantId)
        {
            try
            {
                var carePlanItem = await _context.CarePlanItems
                    .Include(cpi => cpi.CarePlan)
                    .Include(cpi => cpi.CarePlanItemLogs)
                    .FirstOrDefaultAsync(cpi => cpi.ItemId == itemId && cpi.CarePlan.TenantId == tenantId);

                if (carePlanItem == null)
                {
                    return ServiceResult<CarePlanItemDto>.ErrorResult("Item kế hoạch chăm sóc không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.ItemType))
                    carePlanItem.ItemType = dto.ItemType;

                if (!string.IsNullOrEmpty(dto.Title))
                    carePlanItem.Title = dto.Title;

                if (dto.Description != null)
                    carePlanItem.Description = dto.Description;

                if (dto.FrequencyCron != null)
                    carePlanItem.FrequencyCron = dto.FrequencyCron;

                if (dto.TimesPerDay.HasValue)
                    carePlanItem.TimesPerDay = dto.TimesPerDay.Value;

                if (dto.DaysOfWeek != null)
                    carePlanItem.DaysOfWeek = dto.DaysOfWeek;

                if (dto.StartDate.HasValue)
                    carePlanItem.StartDate = dto.StartDate.Value;

                if (dto.EndDate.HasValue)
                    carePlanItem.EndDate = dto.EndDate.Value;

                if (dto.IsActive.HasValue)
                    carePlanItem.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                var result = new CarePlanItemDto
                {
                    ItemId = carePlanItem.ItemId,
                    CarePlanId = carePlanItem.CarePlanId,
                    ItemType = carePlanItem.ItemType,
                    Title = carePlanItem.Title,
                    Description = carePlanItem.Description,
                    FrequencyCron = carePlanItem.FrequencyCron,
                    TimesPerDay = carePlanItem.TimesPerDay,
                    DaysOfWeek = carePlanItem.DaysOfWeek,
                    StartDate = carePlanItem.StartDate,
                    EndDate = carePlanItem.EndDate,
                    IsActive = carePlanItem.IsActive,
                    TotalLogs = carePlanItem.CarePlanItemLogs.Count,
                    CompletedLogs = carePlanItem.CarePlanItemLogs.Count(log => log.IsCompleted),
                    LastPerformed = carePlanItem.CarePlanItemLogs.OrderByDescending(log => log.PerformedAt)
                        .FirstOrDefault()?.PerformedAt
                };

                return ServiceResult<CarePlanItemDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CarePlanItem {ItemId}", itemId);
                return ServiceResult<CarePlanItemDto>.ErrorResult("Lỗi khi cập nhật item kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<bool>> DeleteCarePlanItemAsync(int itemId, int tenantId)
        {
            try
            {
                var carePlanItem = await _context.CarePlanItems
                    .Include(cpi => cpi.CarePlan)
                    .Include(cpi => cpi.CarePlanItemLogs)
                    .FirstOrDefaultAsync(cpi => cpi.ItemId == itemId && cpi.CarePlan.TenantId == tenantId);

                if (carePlanItem == null)
                {
                    return ServiceResult<bool>.ErrorResult("Item kế hoạch chăm sóc không tồn tại");
                }

                // Delete related logs first
                _context.CarePlanItemLogs.RemoveRange(carePlanItem.CarePlanItemLogs);
                _context.CarePlanItems.Remove(carePlanItem);

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CarePlanItem {ItemId}", itemId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi xóa item kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<List<CarePlanItemDto>>> GetCarePlanItemsAsync(int carePlanId, int tenantId)
        {
            try
            {
                var carePlanItems = await _context.CarePlanItems
                    .Include(cpi => cpi.CarePlan)
                    .Include(cpi => cpi.CarePlanItemLogs)
                    .Where(cpi => cpi.CarePlanId == carePlanId && cpi.CarePlan.TenantId == tenantId)
                    .Select(item => new CarePlanItemDto
                    {
                        ItemId = item.ItemId,
                        CarePlanId = item.CarePlanId,
                        ItemType = item.ItemType,
                        Title = item.Title,
                        Description = item.Description,
                        FrequencyCron = item.FrequencyCron,
                        TimesPerDay = item.TimesPerDay,
                        DaysOfWeek = item.DaysOfWeek,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        IsActive = item.IsActive,
                        TotalLogs = item.CarePlanItemLogs.Count,
                        CompletedLogs = item.CarePlanItemLogs.Count(log => log.IsCompleted),
                        LastPerformed = item.CarePlanItemLogs.OrderByDescending(log => log.PerformedAt)
                            .FirstOrDefault()!.PerformedAt
                    })
                    .ToListAsync();

                return ServiceResult<List<CarePlanItemDto>>.SuccessResult(carePlanItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CarePlanItems for CarePlan {CarePlanId}", carePlanId);
                return ServiceResult<List<CarePlanItemDto>>.ErrorResult("Lỗi khi lấy danh sách items của kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanItemLogDto>> LogCarePlanItemAsync(CarePlanItemLogCreateDto dto, int tenantId, int patientId)
        {
            try
            {
                // Validate CarePlanItem exists and belongs to tenant and patient
                var carePlanItem = await _context.CarePlanItems
                    .Include(cpi => cpi.CarePlan)
                    .FirstOrDefaultAsync(cpi => cpi.ItemId == dto.ItemId && 
                        cpi.CarePlan.TenantId == tenantId && 
                        cpi.CarePlan.PatientId == patientId);

                if (carePlanItem == null)
                {
                    return ServiceResult<CarePlanItemLogDto>.ErrorResult("Item kế hoạch chăm sóc không tồn tại hoặc không thuộc về bệnh nhân này");
                }

                var log = new CarePlanItemLog
                {
                    ItemId = dto.ItemId,
                    PatientId = patientId,
                    IsCompleted = dto.IsCompleted,
                    Notes = dto.Notes,
                    ValueNumeric = dto.ValueNumeric,
                    ValueText = dto.ValueText,
                    PerformedAt = dto.PerformedAt ?? DateTime.UtcNow
                };

                _context.CarePlanItemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(log)
                    .Reference(l => l.Item)
                    .LoadAsync();
                await _context.Entry(log)
                    .Reference(l => l.Patient)
                    .LoadAsync();

                var result = new CarePlanItemLogDto
                {
                    LogId = log.LogId,
                    ItemId = log.ItemId,
                    PatientId = log.PatientId,
                    IsCompleted = log.IsCompleted,
                    Notes = log.Notes,
                    ValueNumeric = log.ValueNumeric,
                    ValueText = log.ValueText,
                    PerformedAt = log.PerformedAt,
                    ItemTitle = log.Item.Title,
                    ItemType = log.Item.ItemType,
                    PatientName = log.Patient.FullName
                };

                return ServiceResult<CarePlanItemLogDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging CarePlanItem {ItemId}", dto.ItemId);
                return ServiceResult<CarePlanItemLogDto>.ErrorResult("Lỗi khi ghi log cho item kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<PagedResult<CarePlanItemLogDto>>> GetCarePlanItemLogsAsync(int tenantId, 
            int? patientId = null, int? carePlanId = null, int? itemId = null, 
            DateTime? fromDate = null, DateTime? toDate = null, 
            int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.CarePlanItemLogs
                    .Include(log => log.Item)
                        .ThenInclude(item => item.CarePlan)
                    .Include(log => log.Patient)
                    .Where(log => log.Item.CarePlan.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(log => log.PatientId == patientId.Value);

                if (carePlanId.HasValue)
                    query = query.Where(log => log.Item.CarePlanId == carePlanId.Value);

                if (itemId.HasValue)
                    query = query.Where(log => log.ItemId == itemId.Value);

                if (fromDate.HasValue)
                    query = query.Where(log => log.PerformedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(log => log.PerformedAt <= toDate.Value);

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(log => log.PerformedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(log => new CarePlanItemLogDto
                    {
                        LogId = log.LogId,
                        ItemId = log.ItemId,
                        PatientId = log.PatientId,
                        IsCompleted = log.IsCompleted,
                        Notes = log.Notes,
                        ValueNumeric = log.ValueNumeric,
                        ValueText = log.ValueText,
                        PerformedAt = log.PerformedAt,
                        ItemTitle = log.Item.Title,
                        ItemType = log.Item.ItemType,
                        PatientName = log.Patient.FullName
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<CarePlanItemLogDto>
                {
                    Data = logs,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ServiceResult<PagedResult<CarePlanItemLogDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CarePlanItemLogs for tenant {TenantId}", tenantId);
                return ServiceResult<PagedResult<CarePlanItemLogDto>>.ErrorResult("Lỗi khi lấy danh sách logs của kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<CarePlanProgressDto>> GetCarePlanProgressAsync(int carePlanId, int tenantId)
        {
            try
            {
                var carePlan = await _context.CarePlans
                    .Include(cp => cp.CarePlanItems)
                        .ThenInclude(cpi => cpi.CarePlanItemLogs)
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (carePlan == null)
                {
                    return ServiceResult<CarePlanProgressDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại");
                }

                var totalItems = carePlan.CarePlanItems.Count;
                var activeItems = carePlan.CarePlanItems.Count(item => item.IsActive);
                var completedItems = carePlan.CarePlanItems.Count(item => 
                    item.EndDate.HasValue && item.EndDate.Value <= DateOnly.FromDateTime(DateTime.Today));

                var totalLogs = carePlan.CarePlanItems.Sum(item => item.CarePlanItemLogs.Count);
                var completedLogs = carePlan.CarePlanItems.Sum(item => item.CarePlanItemLogs.Count(log => log.IsCompleted));

                var completionRate = totalLogs > 0 ? (decimal)completedLogs / totalLogs * 100 : 0;

                var lastActivity = carePlan.CarePlanItems
                    .SelectMany(item => item.CarePlanItemLogs)
                    .OrderByDescending(log => log.PerformedAt)
                    .FirstOrDefault()?.PerformedAt;

                var itemProgress = carePlan.CarePlanItems.Select(item =>
                {
                    var itemTotalLogs = item.CarePlanItemLogs.Count;
                    var itemCompletedLogs = item.CarePlanItemLogs.Count(log => log.IsCompleted);
                    var itemCompletionRate = itemTotalLogs > 0 ? (decimal)itemCompletedLogs / itemTotalLogs * 100 : 0;

                    var lastPerformed = item.CarePlanItemLogs
                        .OrderByDescending(log => log.PerformedAt)
                        .FirstOrDefault()?.PerformedAt;

                    // Calculate consecutive days (simplified logic)
                    var consecutiveDays = CalculateConsecutiveDays(item.CarePlanItemLogs);

                    return new CarePlanItemProgressDto
                    {
                        ItemId = item.ItemId,
                        Title = item.Title,
                        ItemType = item.ItemType,
                        IsActive = item.IsActive,
                        TotalLogs = itemTotalLogs,
                        CompletedLogs = itemCompletedLogs,
                        CompletionRate = itemCompletionRate,
                        LastPerformed = lastPerformed,
                        ConsecutiveDays = consecutiveDays
                    };
                }).ToList();

                var progress = new CarePlanProgressDto
                {
                    CarePlanId = carePlan.CarePlanId,
                    Name = carePlan.Name,
                    Status = carePlan.Status,
                    StartDate = carePlan.StartDate,
                    EndDate = carePlan.EndDate,
                    TotalItems = totalItems,
                    ActiveItems = activeItems,
                    CompletedItems = completedItems,
                    TotalLogs = totalLogs,
                    CompletedLogs = completedLogs,
                    CompletionRate = completionRate,
                    LastActivity = lastActivity,
                    ItemProgress = itemProgress
                };

                return ServiceResult<CarePlanProgressDto>.SuccessResult(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CarePlan progress for {CarePlanId}", carePlanId);
                return ServiceResult<CarePlanProgressDto>.ErrorResult("Lỗi khi lấy tiến độ kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<List<CarePlanProgressDto>>> GetPatientCarePlanProgressAsync(int patientId, int? tenantId)
        {
            try
            {
                var query = _context.CarePlans.Where(cp => cp.PatientId == patientId);
                
                // Nếu có tenantId thì filter theo tenant, nếu không thì lấy tất cả
                if (tenantId.HasValue)
                {
                    query = query.Where(cp => cp.TenantId == tenantId.Value);
                }
                
                var carePlans = await query
                    .Select(cp => new { cp.CarePlanId, cp.TenantId })
                    .ToListAsync();

                var progressList = new List<CarePlanProgressDto>();

                foreach (var carePlan in carePlans)
                {
                    var progressResult = await GetCarePlanProgressAsync(carePlan.CarePlanId, carePlan.TenantId);
                    if (progressResult.Success && progressResult.Data != null)
                    {
                        progressList.Add(progressResult.Data);
                    }
                }

                return ServiceResult<List<CarePlanProgressDto>>.SuccessResult(progressList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient CarePlan progress for patient {PatientId}", patientId);
                return ServiceResult<List<CarePlanProgressDto>>.ErrorResult("Lỗi khi lấy tiến độ kế hoạch chăm sóc của bệnh nhân");
            }
        }

        public async Task<ServiceResult<bool>> ValidateCarePlanAccessAsync(int carePlanId, int tenantId, int? patientId = null)
        {
            try
            {
                var query = _context.CarePlans.Where(cp => cp.CarePlanId == carePlanId && cp.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(cp => cp.PatientId == patientId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CarePlan access for {CarePlanId}", carePlanId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi kiểm tra quyền truy cập kế hoạch chăm sóc");
            }
        }

        public async Task<ServiceResult<List<CarePlanDto>>> GetActiveCarePlansForPatientAsync(int patientId, int? tenantId)
        {
            try
            {
                var query = _context.CarePlans
                    .Include(cp => cp.Patient)
                    .Include(cp => cp.CreatedByNavigation)
                    .Include(cp => cp.CarePlanItems)
                        .ThenInclude(cpi => cpi.CarePlanItemLogs)
                    .Where(cp => cp.PatientId == patientId && 
                        cp.Status == "Active" &&
                        (!cp.EndDate.HasValue || cp.EndDate.Value >= DateOnly.FromDateTime(DateTime.Today)));
                
                // Nếu có tenantId thì filter theo tenant, nếu không thì lấy tất cả
                if (tenantId.HasValue)
                {
                    query = query.Where(cp => cp.TenantId == tenantId.Value);
                }
                
                var activeCarePlans = await query
                    .Select(cp => new CarePlanDto
                    {
                        CarePlanId = cp.CarePlanId,
                        TenantId = cp.TenantId,
                        PatientId = cp.PatientId,
                        Name = cp.Name,
                        StartDate = cp.StartDate,
                        EndDate = cp.EndDate,
                        Status = cp.Status,
                        CreatedBy = cp.CreatedBy,
                        CreatedAt = cp.CreatedAt,
                        PatientName = cp.Patient.FullName,
                        CreatedByName = cp.CreatedByNavigation.FullName,
                        Items = cp.CarePlanItems.Where(item => item.IsActive).Select(item => new CarePlanItemDto
                        {
                            ItemId = item.ItemId,
                            CarePlanId = item.CarePlanId,
                            ItemType = item.ItemType,
                            Title = item.Title,
                            Description = item.Description,
                            FrequencyCron = item.FrequencyCron,
                            TimesPerDay = item.TimesPerDay,
                            DaysOfWeek = item.DaysOfWeek,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            IsActive = item.IsActive,
                            TotalLogs = item.CarePlanItemLogs.Count,
                            CompletedLogs = item.CarePlanItemLogs.Count(log => log.IsCompleted),
                            LastPerformed = item.CarePlanItemLogs.OrderByDescending(log => log.PerformedAt)
                                .FirstOrDefault()!.PerformedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return ServiceResult<List<CarePlanDto>>.SuccessResult(activeCarePlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active CarePlans for patient {PatientId}", patientId);
                return ServiceResult<List<CarePlanDto>>.ErrorResult("Lỗi khi lấy danh sách kế hoạch chăm sóc đang hoạt động");
            }
        }

        // Helper method to calculate consecutive days
        private int CalculateConsecutiveDays(ICollection<CarePlanItemLog> logs)
        {
            if (!logs.Any()) return 0;

            var completedLogs = logs
                .Where(log => log.IsCompleted)
                .OrderByDescending(log => log.PerformedAt)
                .ToList();

            if (!completedLogs.Any()) return 0;

            var consecutiveDays = 1;
            var currentDate = completedLogs.First().PerformedAt.Date;

            for (int i = 1; i < completedLogs.Count; i++)
            {
                var previousDate = completedLogs[i].PerformedAt.Date;
                if (currentDate.AddDays(-1) == previousDate)
                {
                    consecutiveDays++;
                    currentDate = previousDate;
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }
    }
}
