using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<MeasurementService> _logger;

        public MeasurementService(SavePlusDbContext context, ILogger<MeasurementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<MeasurementDto>> CreateMeasurementAsync(MeasurementCreateDto dto, int tenantId)
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
                    return ServiceResult<MeasurementDto>.ErrorResult("Bệnh nhân không tồn tại hoặc không thuộc phòng khám này");
                }

                // Validate CarePlan if provided
                if (dto.CarePlanId.HasValue)
                {
                    var carePlan = await _context.CarePlans
                        .FirstOrDefaultAsync(cp => cp.CarePlanId == dto.CarePlanId.Value && 
                            cp.TenantId == tenantId && cp.PatientId == dto.PatientId);

                    if (carePlan == null)
                    {
                        return ServiceResult<MeasurementDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại hoặc không thuộc về bệnh nhân này");
                    }
                }

                var measurement = new PatientMeasurement
                {
                    TenantId = tenantId,
                    PatientId = dto.PatientId,
                    CarePlanId = dto.CarePlanId,
                    Type = dto.Type,
                    Value1 = dto.Value1,
                    Value2 = dto.Value2,
                    Unit = dto.Unit,
                    Source = dto.Source,
                    MeasuredAt = dto.MeasuredAt ?? DateTime.UtcNow,
                    Notes = dto.Notes
                };

                _context.PatientMeasurements.Add(measurement);
                await _context.SaveChangesAsync();

                return await GetMeasurementByIdAsync(measurement.MeasurementId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating measurement for patient {PatientId}", dto.PatientId);
                return ServiceResult<MeasurementDto>.ErrorResult("Lỗi khi tạo số liệu đo lường");
            }
        }

        public async Task<ServiceResult<MeasurementDto>> GetMeasurementByIdAsync(long measurementId, int tenantId)
        {
            try
            {
                var measurement = await _context.PatientMeasurements
                    .Include(pm => pm.Patient)
                    .Include(pm => pm.CarePlan)
                    .FirstOrDefaultAsync(pm => pm.MeasurementId == measurementId && pm.TenantId == tenantId);

                if (measurement == null)
                {
                    return ServiceResult<MeasurementDto>.ErrorResult("Số liệu đo lường không tồn tại");
                }

                // Get previous measurement for trend analysis
                var previousMeasurement = await _context.PatientMeasurements
                    .Where(pm => pm.PatientId == measurement.PatientId && 
                        pm.Type == measurement.Type && 
                        pm.MeasuredAt < measurement.MeasuredAt &&
                        pm.TenantId == tenantId)
                    .OrderByDescending(pm => pm.MeasuredAt)
                    .FirstOrDefaultAsync();

                var measurementDto = new MeasurementDto
                {
                    MeasurementId = measurement.MeasurementId,
                    TenantId = measurement.TenantId,
                    PatientId = measurement.PatientId,
                    CarePlanId = measurement.CarePlanId,
                    Type = measurement.Type,
                    Value1 = measurement.Value1,
                    Value2 = measurement.Value2,
                    Unit = measurement.Unit,
                    Source = measurement.Source,
                    MeasuredAt = measurement.MeasuredAt,
                    Notes = measurement.Notes,
                    PatientName = measurement.Patient.FullName,
                    CarePlanName = measurement.CarePlan?.Name,
                    PreviousValue = GetDisplayValue(previousMeasurement),
                    Trend = CalculateTrend(measurement, previousMeasurement)
                };

                return ServiceResult<MeasurementDto>.SuccessResult(measurementDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement {MeasurementId}", measurementId);
                return ServiceResult<MeasurementDto>.ErrorResult("Lỗi khi lấy thông tin số liệu đo lường");
            }
        }

        public async Task<ServiceResult<MeasurementDto>> UpdateMeasurementAsync(long measurementId, MeasurementUpdateDto dto, int tenantId)
        {
            try
            {
                var measurement = await _context.PatientMeasurements
                    .FirstOrDefaultAsync(pm => pm.MeasurementId == measurementId && pm.TenantId == tenantId);

                if (measurement == null)
                {
                    return ServiceResult<MeasurementDto>.ErrorResult("Số liệu đo lường không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.Type))
                    measurement.Type = dto.Type;

                if (dto.Value1.HasValue)
                    measurement.Value1 = dto.Value1.Value;

                if (dto.Value2.HasValue)
                    measurement.Value2 = dto.Value2.Value;

                if (dto.Unit != null)
                    measurement.Unit = dto.Unit;

                if (!string.IsNullOrEmpty(dto.Source))
                    measurement.Source = dto.Source;

                if (dto.MeasuredAt.HasValue)
                    measurement.MeasuredAt = dto.MeasuredAt.Value;

                if (dto.Notes != null)
                    measurement.Notes = dto.Notes;

                await _context.SaveChangesAsync();

                return await GetMeasurementByIdAsync(measurementId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating measurement {MeasurementId}", measurementId);
                return ServiceResult<MeasurementDto>.ErrorResult("Lỗi khi cập nhật số liệu đo lường");
            }
        }

        public async Task<ServiceResult<bool>> DeleteMeasurementAsync(long measurementId, int tenantId)
        {
            try
            {
                var measurement = await _context.PatientMeasurements
                    .FirstOrDefaultAsync(pm => pm.MeasurementId == measurementId && pm.TenantId == tenantId);

                if (measurement == null)
                {
                    return ServiceResult<bool>.ErrorResult("Số liệu đo lường không tồn tại");
                }

                _context.PatientMeasurements.Remove(measurement);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting measurement {MeasurementId}", measurementId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi xóa số liệu đo lường");
            }
        }

        public async Task<ServiceResult<PagedResult<MeasurementDto>>> GetMeasurementsAsync(int tenantId, MeasurementQueryDto query)
        {
            try
            {
                var dbQuery = _context.PatientMeasurements
                    .Include(pm => pm.Patient)
                    .Include(pm => pm.CarePlan)
                    .Where(pm => pm.TenantId == tenantId);

                // Apply filters
                if (query.PatientId.HasValue)
                    dbQuery = dbQuery.Where(pm => pm.PatientId == query.PatientId.Value);

                if (query.CarePlanId.HasValue)
                    dbQuery = dbQuery.Where(pm => pm.CarePlanId == query.CarePlanId.Value);

                if (!string.IsNullOrEmpty(query.Type))
                    dbQuery = dbQuery.Where(pm => pm.Type == query.Type);

                if (!string.IsNullOrEmpty(query.Source))
                    dbQuery = dbQuery.Where(pm => pm.Source == query.Source);

                if (query.FromDate.HasValue)
                    dbQuery = dbQuery.Where(pm => pm.MeasuredAt >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    dbQuery = dbQuery.Where(pm => pm.MeasuredAt <= query.ToDate.Value);

                // Apply sorting
                switch (query.SortBy?.ToLower())
                {
                    case "type":
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(pm => pm.Type) 
                            : dbQuery.OrderByDescending(pm => pm.Type);
                        break;
                    case "value1":
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(pm => pm.Value1) 
                            : dbQuery.OrderByDescending(pm => pm.Value1);
                        break;
                    default: // MeasuredAt
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(pm => pm.MeasuredAt) 
                            : dbQuery.OrderByDescending(pm => pm.MeasuredAt);
                        break;
                }

                var totalCount = await dbQuery.CountAsync();
                var measurements = await dbQuery
                    .Skip((query.PageNumber!.Value - 1) * query.PageSize!.Value)
                    .Take(query.PageSize.Value)
                    .Select(pm => new MeasurementDto
                    {
                        MeasurementId = pm.MeasurementId,
                        TenantId = pm.TenantId,
                        PatientId = pm.PatientId,
                        CarePlanId = pm.CarePlanId,
                        Type = pm.Type,
                        Value1 = pm.Value1,
                        Value2 = pm.Value2,
                        Unit = pm.Unit,
                        Source = pm.Source,
                        MeasuredAt = pm.MeasuredAt,
                        Notes = pm.Notes,
                        PatientName = pm.Patient.FullName,
                        CarePlanName = pm.CarePlan != null ? pm.CarePlan.Name : null
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<MeasurementDto>
                {
                    Data = measurements,
                    PageNumber = query.PageNumber.Value,
                    PageSize = query.PageSize.Value,
                    TotalCount = totalCount
                };

                return ServiceResult<PagedResult<MeasurementDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurements for tenant {TenantId}", tenantId);
                return ServiceResult<PagedResult<MeasurementDto>>.ErrorResult("Lỗi khi lấy danh sách số liệu đo lường");
            }
        }

        public async Task<ServiceResult<List<MeasurementDto>>> GetPatientMeasurementsAsync(int patientId, int tenantId, 
            string? type = null, DateTime? fromDate = null, DateTime? toDate = null, int? limit = null)
        {
            try
            {
                var query = _context.PatientMeasurements
                    .Include(pm => pm.Patient)
                    .Include(pm => pm.CarePlan)
                    .Where(pm => pm.PatientId == patientId && pm.TenantId == tenantId);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(pm => pm.Type == type);

                if (fromDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt <= toDate.Value);

                query = query.OrderByDescending(pm => pm.MeasuredAt);

                if (limit.HasValue)
                    query = query.Take(limit.Value);

                var measurements = await query
                    .Select(pm => new MeasurementDto
                    {
                        MeasurementId = pm.MeasurementId,
                        TenantId = pm.TenantId,
                        PatientId = pm.PatientId,
                        CarePlanId = pm.CarePlanId,
                        Type = pm.Type,
                        Value1 = pm.Value1,
                        Value2 = pm.Value2,
                        Unit = pm.Unit,
                        Source = pm.Source,
                        MeasuredAt = pm.MeasuredAt,
                        Notes = pm.Notes,
                        PatientName = pm.Patient.FullName,
                        CarePlanName = pm.CarePlan != null ? pm.CarePlan.Name : null
                    })
                    .ToListAsync();

                return ServiceResult<List<MeasurementDto>>.SuccessResult(measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient measurements for patient {PatientId}", patientId);
                return ServiceResult<List<MeasurementDto>>.ErrorResult("Lỗi khi lấy số liệu đo lường của bệnh nhân");
            }
        }

        public async Task<ServiceResult<List<MeasurementStatsDto>>> GetMeasurementStatsAsync(int patientId, int tenantId, 
            string? type = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PatientMeasurements
                    .Where(pm => pm.PatientId == patientId && pm.TenantId == tenantId);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(pm => pm.Type == type);

                if (fromDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt <= toDate.Value);

                var measurementTypes = await query
                    .Select(pm => pm.Type)
                    .Distinct()
                    .ToListAsync();

                var statsList = new List<MeasurementStatsDto>();

                foreach (var measurementType in measurementTypes)
                {
                    var statsResult = await GetMeasurementStatsByTypeAsync(patientId, tenantId, measurementType, fromDate, toDate);
                    if (statsResult.Success && statsResult.Data != null)
                    {
                        statsList.Add(statsResult.Data);
                    }
                }

                return ServiceResult<List<MeasurementStatsDto>>.SuccessResult(statsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement stats for patient {PatientId}", patientId);
                return ServiceResult<List<MeasurementStatsDto>>.ErrorResult("Lỗi khi lấy thống kê số liệu đo lường");
            }
        }

        public async Task<ServiceResult<MeasurementStatsDto>> GetMeasurementStatsByTypeAsync(int patientId, int tenantId, 
            string type, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PatientMeasurements
                    .Where(pm => pm.PatientId == patientId && pm.TenantId == tenantId && pm.Type == type);

                if (fromDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pm => pm.MeasuredAt <= toDate.Value);

                var measurements = await query
                    .OrderBy(pm => pm.MeasuredAt)
                    .ToListAsync();

                if (!measurements.Any())
                {
                    return ServiceResult<MeasurementStatsDto>.ErrorResult("Không có dữ liệu đo lường cho loại này");
                }

                var stats = new MeasurementStatsDto
                {
                    Type = type,
                    Unit = measurements.First().Unit,
                    TotalCount = measurements.Count,
                    FirstMeasurement = measurements.First().MeasuredAt,
                    LastMeasurement = measurements.Last().MeasuredAt
                };

                // Calculate Value1 statistics
                var value1Data = measurements.Where(m => m.Value1.HasValue).Select(m => m.Value1!.Value).ToList();
                if (value1Data.Any())
                {
                    stats.LatestValue1 = measurements.LastOrDefault(m => m.Value1.HasValue)?.Value1;
                    stats.AverageValue1 = value1Data.Average();
                    stats.MinValue1 = value1Data.Min();
                    stats.MaxValue1 = value1Data.Max();
                }

                // Calculate Value2 statistics (if applicable)
                var value2Data = measurements.Where(m => m.Value2.HasValue).Select(m => m.Value2!.Value).ToList();
                if (value2Data.Any())
                {
                    stats.LatestValue2 = measurements.LastOrDefault(m => m.Value2.HasValue)?.Value2;
                    stats.AverageValue2 = value2Data.Average();
                    stats.MinValue2 = value2Data.Min();
                    stats.MaxValue2 = value2Data.Max();
                }

                // Calculate trend
                stats.Trend = CalculateOverallTrend(measurements);

                // Get recent data for trend visualization (last 10 measurements)
                stats.RecentData = measurements
                    .TakeLast(10)
                    .Select(m => new MeasurementTrendDataDto
                    {
                        MeasuredAt = m.MeasuredAt,
                        Value1 = m.Value1,
                        Value2 = m.Value2,
                        DisplayValue = GetDisplayValue(m)
                    })
                    .ToList();

                return ServiceResult<MeasurementStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting measurement stats by type for patient {PatientId}, type {Type}", patientId, type);
                return ServiceResult<MeasurementStatsDto>.ErrorResult("Lỗi khi lấy thống kê số liệu đo lường theo loại");
            }
        }

        public async Task<ServiceResult<List<string>>> GetAvailableMeasurementTypesAsync(int tenantId, int? patientId = null)
        {
            try
            {
                var query = _context.PatientMeasurements.Where(pm => pm.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(pm => pm.PatientId == patientId.Value);

                var types = await query
                    .Select(pm => pm.Type)
                    .Distinct()
                    .OrderBy(type => type)
                    .ToListAsync();

                return ServiceResult<List<string>>.SuccessResult(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available measurement types for tenant {TenantId}", tenantId);
                return ServiceResult<List<string>>.ErrorResult("Lỗi khi lấy danh sách loại đo lường");
            }
        }

        public async Task<ServiceResult<bool>> ValidateMeasurementAccessAsync(long measurementId, int tenantId, int? patientId = null)
        {
            try
            {
                var query = _context.PatientMeasurements
                    .Where(pm => pm.MeasurementId == measurementId && pm.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(pm => pm.PatientId == patientId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating measurement access for {MeasurementId}", measurementId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi kiểm tra quyền truy cập số liệu đo lường");
            }
        }

        public async Task<ServiceResult<List<MeasurementDto>>> GetRecentMeasurementsAsync(int patientId, int tenantId, int days = 7)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);

                var measurements = await _context.PatientMeasurements
                    .Include(pm => pm.Patient)
                    .Include(pm => pm.CarePlan)
                    .Where(pm => pm.PatientId == patientId && 
                        pm.TenantId == tenantId && 
                        pm.MeasuredAt >= fromDate)
                    .OrderByDescending(pm => pm.MeasuredAt)
                    .Select(pm => new MeasurementDto
                    {
                        MeasurementId = pm.MeasurementId,
                        TenantId = pm.TenantId,
                        PatientId = pm.PatientId,
                        CarePlanId = pm.CarePlanId,
                        Type = pm.Type,
                        Value1 = pm.Value1,
                        Value2 = pm.Value2,
                        Unit = pm.Unit,
                        Source = pm.Source,
                        MeasuredAt = pm.MeasuredAt,
                        Notes = pm.Notes,
                        PatientName = pm.Patient.FullName,
                        CarePlanName = pm.CarePlan != null ? pm.CarePlan.Name : null
                    })
                    .ToListAsync();

                return ServiceResult<List<MeasurementDto>>.SuccessResult(measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent measurements for patient {PatientId}", patientId);
                return ServiceResult<List<MeasurementDto>>.ErrorResult("Lỗi khi lấy số liệu đo lường gần đây");
            }
        }

        // Helper methods
        private string GetDisplayValue(PatientMeasurement? measurement)
        {
            if (measurement == null) return "N/A";

            if (measurement.Value1.HasValue && measurement.Value2.HasValue)
            {
                return $"{measurement.Value1}/{measurement.Value2} {measurement.Unit}";
            }
            else if (measurement.Value1.HasValue)
            {
                return $"{measurement.Value1} {measurement.Unit}";
            }
            return "N/A";
        }

        private string CalculateTrend(PatientMeasurement current, PatientMeasurement? previous)
        {
            if (previous == null || !current.Value1.HasValue || !previous.Value1.HasValue)
                return "Stable";

            var difference = current.Value1.Value - previous.Value1.Value;
            var percentChange = Math.Abs(difference) / previous.Value1.Value * 100;

            if (percentChange < 5) return "Stable";

            return difference > 0 ? "Up" : "Down";
        }

        private string CalculateOverallTrend(List<PatientMeasurement> measurements)
        {
            if (measurements.Count < 3) return "Stable";

            var recentMeasurements = measurements.TakeLast(5).Where(m => m.Value1.HasValue).ToList();
            if (recentMeasurements.Count < 3) return "Stable";

            var firstValue = recentMeasurements.First().Value1!.Value;
            var lastValue = recentMeasurements.Last().Value1!.Value;
            var difference = lastValue - firstValue;
            var percentChange = Math.Abs(difference) / firstValue * 100;

            if (percentChange < 10) return "Stable";

            // Determine if it's improving or worsening based on measurement type
            if (difference > 0) return "Improving"; // Can be customized per measurement type
            return "Worsening";
        }
    }
}
