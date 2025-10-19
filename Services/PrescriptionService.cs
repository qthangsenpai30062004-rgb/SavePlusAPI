using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using SavePlus_API.Constants;

namespace SavePlus_API.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(SavePlusDbContext context, ILogger<PrescriptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<PrescriptionDto>> CreatePrescriptionAsync(PrescriptionCreateDto dto, int tenantId)
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
                    return ServiceResult<PrescriptionDto>.ErrorResult("Bệnh nhân không tồn tại hoặc không thuộc phòng khám này");
                }

                // Validate doctor exists and belongs to tenant and can prescribe
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId && d.TenantId == tenantId);

                if (doctor == null)
                {
                    return ServiceResult<PrescriptionDto>.ErrorResult("Bác sĩ không tồn tại hoặc không thuộc phòng khám này");
                }

                // Check if doctor has prescription rights
                if (!UserRoles.PrescriptionRoles.Contains(doctor.User.Role))
                {
                    return ServiceResult<PrescriptionDto>.ErrorResult("Người dùng này không có quyền kê đơn thuốc");
                }

                // Validate CarePlan if provided
                if (dto.CarePlanId.HasValue)
                {
                    var carePlan = await _context.CarePlans
                        .FirstOrDefaultAsync(cp => cp.CarePlanId == dto.CarePlanId.Value && 
                            cp.TenantId == tenantId && cp.PatientId == dto.PatientId);

                    if (carePlan == null)
                    {
                        return ServiceResult<PrescriptionDto>.ErrorResult("Kế hoạch chăm sóc không tồn tại hoặc không thuộc về bệnh nhân này");
                    }
                }

                if (!dto.Items.Any())
                {
                    return ServiceResult<PrescriptionDto>.ErrorResult("Đơn thuốc phải có ít nhất một loại thuốc");
                }

                // Create prescription
                var prescription = new Prescription
                {
                    TenantId = tenantId,
                    PatientId = dto.PatientId,
                    CarePlanId = dto.CarePlanId,
                    DoctorId = dto.DoctorId,
                    IssuedAt = dto.IssuedAt ?? DateTime.UtcNow,
                    Status = dto.Status
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                // Create prescription items
                var prescriptionItems = dto.Items.Select(item => new PrescriptionItem
                {
                    PrescriptionId = prescription.PrescriptionId,
                    DrugName = item.DrugName,
                    Form = item.Form,
                    Strength = item.Strength,
                    Dose = item.Dose,
                    Route = item.Route,
                    Frequency = item.Frequency,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Instructions = item.Instructions
                }).ToList();

                _context.PrescriptionItems.AddRange(prescriptionItems);
                await _context.SaveChangesAsync();

                return await GetPrescriptionByIdAsync(prescription.PrescriptionId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription for patient {PatientId}", dto.PatientId);
                return ServiceResult<PrescriptionDto>.ErrorResult("Lỗi khi tạo đơn thuốc");
            }
        }

        public async Task<ServiceResult<PrescriptionDto>> GetPrescriptionByIdAsync(int prescriptionId, int tenantId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(p => p.CarePlan)
                    .Include(p => p.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.TenantId == tenantId);

                if (prescription == null)
                {
                    return ServiceResult<PrescriptionDto>.ErrorResult("Đơn thuốc không tồn tại");
                }

                var prescriptionDto = new PrescriptionDto
                {
                    PrescriptionId = prescription.PrescriptionId,
                    TenantId = prescription.TenantId,
                    PatientId = prescription.PatientId,
                    CarePlanId = prescription.CarePlanId,
                    DoctorId = prescription.DoctorId,
                    IssuedAt = prescription.IssuedAt,
                    Status = prescription.Status,
                    PatientName = prescription.Patient.FullName,
                    DoctorName = prescription.Doctor.User.FullName,
                    CarePlanName = prescription.CarePlan?.Name,
                    Items = prescription.PrescriptionItems.Select(item => new PrescriptionItemDto
                    {
                        ItemId = item.ItemId,
                        PrescriptionId = item.PrescriptionId,
                        DrugName = item.DrugName,
                        Form = item.Form,
                        Strength = item.Strength,
                        Dose = item.Dose,
                        Route = item.Route,
                        Frequency = item.Frequency,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Instructions = item.Instructions,
                        IsActive = CalculateItemIsActive(item),
                        DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                    }).ToList(),
                    TotalItems = prescription.PrescriptionItems.Count,
                    HasActiveItems = prescription.PrescriptionItems.Any(item => CalculateItemIsActive(item)),
                    NextDue = CalculateNextDue(prescription.PrescriptionItems)
                };

                return ServiceResult<PrescriptionDto>.SuccessResult(prescriptionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", prescriptionId);
                return ServiceResult<PrescriptionDto>.ErrorResult("Lỗi khi lấy thông tin đơn thuốc");
            }
        }

        public async Task<ServiceResult<PrescriptionDto>> UpdatePrescriptionAsync(int prescriptionId, PrescriptionUpdateDto dto, int tenantId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.TenantId == tenantId);

                if (prescription == null)
                {
                    return ServiceResult<PrescriptionDto>.ErrorResult("Đơn thuốc không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.Status))
                    prescription.Status = dto.Status;

                if (dto.IssuedAt.HasValue)
                    prescription.IssuedAt = dto.IssuedAt.Value;

                await _context.SaveChangesAsync();

                return await GetPrescriptionByIdAsync(prescriptionId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId}", prescriptionId);
                return ServiceResult<PrescriptionDto>.ErrorResult("Lỗi khi cập nhật đơn thuốc");
            }
        }

        public async Task<ServiceResult<bool>> DeletePrescriptionAsync(int prescriptionId, int tenantId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.TenantId == tenantId);

                if (prescription == null)
                {
                    return ServiceResult<bool>.ErrorResult("Đơn thuốc không tồn tại");
                }

                // Delete prescription items first
                _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);
                _context.Prescriptions.Remove(prescription);

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", prescriptionId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi xóa đơn thuốc");
            }
        }

        public async Task<ServiceResult<PagedResult<PrescriptionDto>>> GetPrescriptionsAsync(int tenantId, PrescriptionQueryDto query)
        {
            try
            {
                var dbQuery = _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(p => p.CarePlan)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.TenantId == tenantId);

                // Apply filters
                if (query.PatientId.HasValue)
                    dbQuery = dbQuery.Where(p => p.PatientId == query.PatientId.Value);

                if (query.DoctorId.HasValue)
                    dbQuery = dbQuery.Where(p => p.DoctorId == query.DoctorId.Value);

                if (query.CarePlanId.HasValue)
                    dbQuery = dbQuery.Where(p => p.CarePlanId == query.CarePlanId.Value);

                if (!string.IsNullOrEmpty(query.Status))
                    dbQuery = dbQuery.Where(p => p.Status == query.Status);

                if (query.FromDate.HasValue)
                    dbQuery = dbQuery.Where(p => p.IssuedAt >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    dbQuery = dbQuery.Where(p => p.IssuedAt <= query.ToDate.Value);

                if (!string.IsNullOrEmpty(query.DrugName))
                    dbQuery = dbQuery.Where(p => p.PrescriptionItems.Any(item => 
                        item.DrugName.Contains(query.DrugName)));

                // Apply sorting
                switch (query.SortBy?.ToLower())
                {
                    case "patientname":
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(p => p.Patient.FullName) 
                            : dbQuery.OrderByDescending(p => p.Patient.FullName);
                        break;
                    case "doctorname":
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(p => p.Doctor.User.FullName) 
                            : dbQuery.OrderByDescending(p => p.Doctor.User.FullName);
                        break;
                    default: // IssuedAt
                        dbQuery = query.SortOrder?.ToLower() == "asc" 
                            ? dbQuery.OrderBy(p => p.IssuedAt) 
                            : dbQuery.OrderByDescending(p => p.IssuedAt);
                        break;
                }

                var totalCount = await dbQuery.CountAsync();
                var prescriptions = await dbQuery
                    .Skip((query.PageNumber!.Value - 1) * query.PageSize!.Value)
                    .Take(query.PageSize.Value)
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        TenantId = p.TenantId,
                        PatientId = p.PatientId,
                        CarePlanId = p.CarePlanId,
                        DoctorId = p.DoctorId,
                        IssuedAt = p.IssuedAt,
                        Status = p.Status,
                        PatientName = p.Patient.FullName,
                        DoctorName = p.Doctor.User.FullName,
                        CarePlanName = p.CarePlan != null ? p.CarePlan.Name : null,
                        Items = p.PrescriptionItems.Select(item => new PrescriptionItemDto
                        {
                            ItemId = item.ItemId,
                            PrescriptionId = item.PrescriptionId,
                            DrugName = item.DrugName,
                            Form = item.Form,
                            Strength = item.Strength,
                            Dose = item.Dose,
                            Route = item.Route,
                            Frequency = item.Frequency,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Instructions = item.Instructions,
                            IsActive = CalculateItemIsActive(item),
                            DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                        }).ToList(),
                        TotalItems = p.PrescriptionItems.Count,
                        HasActiveItems = p.PrescriptionItems.Any(item => CalculateItemIsActive(item))
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<PrescriptionDto>
                {
                    Data = prescriptions,
                    PageNumber = query.PageNumber.Value,
                    PageSize = query.PageSize.Value,
                    TotalCount = totalCount
                };

                return ServiceResult<PagedResult<PrescriptionDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for tenant {TenantId}", tenantId);
                return ServiceResult<PagedResult<PrescriptionDto>>.ErrorResult("Lỗi khi lấy danh sách đơn thuốc");
            }
        }

        public async Task<ServiceResult<List<PrescriptionDto>>> GetPatientPrescriptionsAsync(int patientId, int tenantId, 
            string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(p => p.CarePlan)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.PatientId == patientId && p.TenantId == tenantId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(p => p.Status == status);

                if (fromDate.HasValue)
                    query = query.Where(p => p.IssuedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(p => p.IssuedAt <= toDate.Value);

                var prescriptions = await query
                    .OrderByDescending(p => p.IssuedAt)
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        TenantId = p.TenantId,
                        PatientId = p.PatientId,
                        CarePlanId = p.CarePlanId,
                        DoctorId = p.DoctorId,
                        IssuedAt = p.IssuedAt,
                        Status = p.Status,
                        PatientName = p.Patient.FullName,
                        DoctorName = p.Doctor.User.FullName,
                        CarePlanName = p.CarePlan != null ? p.CarePlan.Name : null,
                        Items = p.PrescriptionItems.Select(item => new PrescriptionItemDto
                        {
                            ItemId = item.ItemId,
                            PrescriptionId = item.PrescriptionId,
                            DrugName = item.DrugName,
                            Form = item.Form,
                            Strength = item.Strength,
                            Dose = item.Dose,
                            Route = item.Route,
                            Frequency = item.Frequency,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Instructions = item.Instructions,
                            IsActive = CalculateItemIsActive(item),
                            DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                        }).ToList(),
                        TotalItems = p.PrescriptionItems.Count,
                        HasActiveItems = p.PrescriptionItems.Any(item => CalculateItemIsActive(item))
                    })
                    .ToListAsync();

                return ServiceResult<List<PrescriptionDto>>.SuccessResult(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for patient {PatientId}", patientId);
                return ServiceResult<List<PrescriptionDto>>.ErrorResult("Lỗi khi lấy đơn thuốc của bệnh nhân");
            }
        }

        public async Task<ServiceResult<List<PrescriptionDto>>> GetDoctorPrescriptionsAsync(int doctorId, int tenantId,
            string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int? limit = null)
        {
            try
            {
                var query = _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(p => p.CarePlan)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.DoctorId == doctorId && p.TenantId == tenantId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(p => p.Status == status);

                if (fromDate.HasValue)
                    query = query.Where(p => p.IssuedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(p => p.IssuedAt <= toDate.Value);

                query = query.OrderByDescending(p => p.IssuedAt);

                if (limit.HasValue)
                    query = query.Take(limit.Value);

                var prescriptions = await query
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        TenantId = p.TenantId,
                        PatientId = p.PatientId,
                        CarePlanId = p.CarePlanId,
                        DoctorId = p.DoctorId,
                        IssuedAt = p.IssuedAt,
                        Status = p.Status,
                        PatientName = p.Patient.FullName,
                        DoctorName = p.Doctor.User.FullName,
                        CarePlanName = p.CarePlan != null ? p.CarePlan.Name : null,
                        Items = p.PrescriptionItems.Select(item => new PrescriptionItemDto
                        {
                            ItemId = item.ItemId,
                            PrescriptionId = item.PrescriptionId,
                            DrugName = item.DrugName,
                            Form = item.Form,
                            Strength = item.Strength,
                            Dose = item.Dose,
                            Route = item.Route,
                            Frequency = item.Frequency,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Instructions = item.Instructions,
                            IsActive = CalculateItemIsActive(item),
                            DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                        }).ToList(),
                        TotalItems = p.PrescriptionItems.Count,
                        HasActiveItems = p.PrescriptionItems.Any(item => CalculateItemIsActive(item))
                    })
                    .ToListAsync();

                return ServiceResult<List<PrescriptionDto>>.SuccessResult(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for doctor {DoctorId}", doctorId);
                return ServiceResult<List<PrescriptionDto>>.ErrorResult("Lỗi khi lấy đơn thuốc của bác sĩ");
            }
        }

        public async Task<ServiceResult<PrescriptionItemDto>> CreatePrescriptionItemAsync(int prescriptionId, PrescriptionItemCreateDto dto, int tenantId)
        {
            try
            {
                // Validate prescription exists and belongs to tenant
                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.TenantId == tenantId);

                if (prescription == null)
                {
                    return ServiceResult<PrescriptionItemDto>.ErrorResult("Đơn thuốc không tồn tại");
                }

                var prescriptionItem = new PrescriptionItem
                {
                    PrescriptionId = prescriptionId,
                    DrugName = dto.DrugName,
                    Form = dto.Form,
                    Strength = dto.Strength,
                    Dose = dto.Dose,
                    Route = dto.Route,
                    Frequency = dto.Frequency,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Instructions = dto.Instructions
                };

                _context.PrescriptionItems.Add(prescriptionItem);
                await _context.SaveChangesAsync();

                var result = new PrescriptionItemDto
                {
                    ItemId = prescriptionItem.ItemId,
                    PrescriptionId = prescriptionItem.PrescriptionId,
                    DrugName = prescriptionItem.DrugName,
                    Form = prescriptionItem.Form,
                    Strength = prescriptionItem.Strength,
                    Dose = prescriptionItem.Dose,
                    Route = prescriptionItem.Route,
                    Frequency = prescriptionItem.Frequency,
                    StartDate = prescriptionItem.StartDate,
                    EndDate = prescriptionItem.EndDate,
                    Instructions = prescriptionItem.Instructions,
                    IsActive = CalculateItemIsActive(prescriptionItem),
                    DaysDuration = CalculateDaysDuration(prescriptionItem.StartDate, prescriptionItem.EndDate)
                };

                return ServiceResult<PrescriptionItemDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription item for prescription {PrescriptionId}", prescriptionId);
                return ServiceResult<PrescriptionItemDto>.ErrorResult("Lỗi khi tạo thuốc trong đơn");
            }
        }

        public async Task<ServiceResult<PrescriptionItemDto>> UpdatePrescriptionItemAsync(int itemId, PrescriptionItemUpdateDto dto, int tenantId)
        {
            try
            {
                var prescriptionItem = await _context.PrescriptionItems
                    .Include(pi => pi.Prescription)
                    .FirstOrDefaultAsync(pi => pi.ItemId == itemId && pi.Prescription.TenantId == tenantId);

                if (prescriptionItem == null)
                {
                    return ServiceResult<PrescriptionItemDto>.ErrorResult("Thuốc trong đơn không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.DrugName))
                    prescriptionItem.DrugName = dto.DrugName;

                if (dto.Form != null)
                    prescriptionItem.Form = dto.Form;

                if (dto.Strength != null)
                    prescriptionItem.Strength = dto.Strength;

                if (!string.IsNullOrEmpty(dto.Dose))
                    prescriptionItem.Dose = dto.Dose;

                if (dto.Route != null)
                    prescriptionItem.Route = dto.Route;

                if (!string.IsNullOrEmpty(dto.Frequency))
                    prescriptionItem.Frequency = dto.Frequency;

                if (dto.StartDate.HasValue)
                    prescriptionItem.StartDate = dto.StartDate.Value;

                if (dto.EndDate.HasValue)
                    prescriptionItem.EndDate = dto.EndDate.Value;

                if (dto.Instructions != null)
                    prescriptionItem.Instructions = dto.Instructions;

                await _context.SaveChangesAsync();

                var result = new PrescriptionItemDto
                {
                    ItemId = prescriptionItem.ItemId,
                    PrescriptionId = prescriptionItem.PrescriptionId,
                    DrugName = prescriptionItem.DrugName,
                    Form = prescriptionItem.Form,
                    Strength = prescriptionItem.Strength,
                    Dose = prescriptionItem.Dose,
                    Route = prescriptionItem.Route,
                    Frequency = prescriptionItem.Frequency,
                    StartDate = prescriptionItem.StartDate,
                    EndDate = prescriptionItem.EndDate,
                    Instructions = prescriptionItem.Instructions,
                    IsActive = CalculateItemIsActive(prescriptionItem),
                    DaysDuration = CalculateDaysDuration(prescriptionItem.StartDate, prescriptionItem.EndDate)
                };

                return ServiceResult<PrescriptionItemDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription item {ItemId}", itemId);
                return ServiceResult<PrescriptionItemDto>.ErrorResult("Lỗi khi cập nhật thuốc trong đơn");
            }
        }

        public async Task<ServiceResult<bool>> DeletePrescriptionItemAsync(int itemId, int tenantId)
        {
            try
            {
                var prescriptionItem = await _context.PrescriptionItems
                    .Include(pi => pi.Prescription)
                    .FirstOrDefaultAsync(pi => pi.ItemId == itemId && pi.Prescription.TenantId == tenantId);

                if (prescriptionItem == null)
                {
                    return ServiceResult<bool>.ErrorResult("Thuốc trong đơn không tồn tại");
                }

                _context.PrescriptionItems.Remove(prescriptionItem);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription item {ItemId}", itemId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi xóa thuốc trong đơn");
            }
        }

        public async Task<ServiceResult<List<PrescriptionItemDto>>> GetPrescriptionItemsAsync(int prescriptionId, int tenantId)
        {
            try
            {
                var prescriptionItems = await _context.PrescriptionItems
                    .Include(pi => pi.Prescription)
                    .Where(pi => pi.PrescriptionId == prescriptionId && pi.Prescription.TenantId == tenantId)
                    .Select(item => new PrescriptionItemDto
                    {
                        ItemId = item.ItemId,
                        PrescriptionId = item.PrescriptionId,
                        DrugName = item.DrugName,
                        Form = item.Form,
                        Strength = item.Strength,
                        Dose = item.Dose,
                        Route = item.Route,
                        Frequency = item.Frequency,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Instructions = item.Instructions,
                        IsActive = CalculateItemIsActive(item),
                        DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                    })
                    .ToListAsync();

                return ServiceResult<List<PrescriptionItemDto>>.SuccessResult(prescriptionItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription items for prescription {PrescriptionId}", prescriptionId);
                return ServiceResult<List<PrescriptionItemDto>>.ErrorResult("Lỗi khi lấy danh sách thuốc trong đơn");
            }
        }

        public async Task<ServiceResult<bool>> ValidatePrescriptionAccessAsync(int prescriptionId, int tenantId, int? patientId = null, int? doctorId = null)
        {
            try
            {
                var query = _context.Prescriptions
                    .Where(p => p.PrescriptionId == prescriptionId && p.TenantId == tenantId);

                if (patientId.HasValue)
                    query = query.Where(p => p.PatientId == patientId.Value);

                if (doctorId.HasValue)
                    query = query.Where(p => p.DoctorId == doctorId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating prescription access for {PrescriptionId}", prescriptionId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi kiểm tra quyền truy cập đơn thuốc");
            }
        }

        public async Task<ServiceResult<List<PrescriptionDto>>> GetActivePrescriptionsForPatientAsync(int patientId, int tenantId)
        {
            try
            {
                var activePrescriptions = await _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(p => p.CarePlan)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.PatientId == patientId && 
                        p.TenantId == tenantId && 
                        p.Status == PrescriptionStatuses.Active)
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        TenantId = p.TenantId,
                        PatientId = p.PatientId,
                        CarePlanId = p.CarePlanId,
                        DoctorId = p.DoctorId,
                        IssuedAt = p.IssuedAt,
                        Status = p.Status,
                        PatientName = p.Patient.FullName,
                        DoctorName = p.Doctor.User.FullName,
                        CarePlanName = p.CarePlan != null ? p.CarePlan.Name : null,
                        Items = p.PrescriptionItems.Where(item => CalculateItemIsActive(item)).Select(item => new PrescriptionItemDto
                        {
                            ItemId = item.ItemId,
                            PrescriptionId = item.PrescriptionId,
                            DrugName = item.DrugName,
                            Form = item.Form,
                            Strength = item.Strength,
                            Dose = item.Dose,
                            Route = item.Route,
                            Frequency = item.Frequency,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            Instructions = item.Instructions,
                            IsActive = CalculateItemIsActive(item),
                            DaysDuration = CalculateDaysDuration(item.StartDate, item.EndDate)
                        }).ToList(),
                        TotalItems = p.PrescriptionItems.Count,
                        HasActiveItems = p.PrescriptionItems.Any(item => CalculateItemIsActive(item))
                    })
                    .ToListAsync();

                return ServiceResult<List<PrescriptionDto>>.SuccessResult(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active prescriptions for patient {PatientId}", patientId);
                return ServiceResult<List<PrescriptionDto>>.ErrorResult("Lỗi khi lấy đơn thuốc đang hoạt động");
            }
        }

        public async Task<ServiceResult<List<string>>> GetMostPrescribedDrugsAsync(int tenantId, int? doctorId = null, int? limit = 20)
        {
            try
            {
                var query = _context.PrescriptionItems
                    .Include(pi => pi.Prescription)
                    .Where(pi => pi.Prescription.TenantId == tenantId);

                if (doctorId.HasValue)
                    query = query.Where(pi => pi.Prescription.DoctorId == doctorId.Value);

                var mostPrescribedDrugs = await query
                    .GroupBy(pi => pi.DrugName)
                    .OrderByDescending(g => g.Count())
                    .Take(limit ?? 20)
                    .Select(g => g.Key)
                    .ToListAsync();

                return ServiceResult<List<string>>.SuccessResult(mostPrescribedDrugs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most prescribed drugs for tenant {TenantId}", tenantId);
                return ServiceResult<List<string>>.ErrorResult("Lỗi khi lấy danh sách thuốc được kê nhiều nhất");
            }
        }

        public async Task<ServiceResult<bool>> CanDoctorPrescribeAsync(int doctorId, int tenantId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId && d.TenantId == tenantId);

                if (doctor == null)
                {
                    return ServiceResult<bool>.SuccessResult(false);
                }

                var canPrescribe = UserRoles.PrescriptionRoles.Contains(doctor.User.Role);
                return ServiceResult<bool>.SuccessResult(canPrescribe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if doctor {DoctorId} can prescribe", doctorId);
                return ServiceResult<bool>.ErrorResult("Lỗi khi kiểm tra quyền kê đơn của bác sĩ");
            }
        }

        // Helper methods
        private bool CalculateItemIsActive(PrescriptionItem item)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // If no start date, assume it starts from prescription date
            var startDate = item.StartDate ?? DateOnly.FromDateTime(DateTime.Today);

            // If no end date, consider it active if it started
            if (!item.EndDate.HasValue)
                return startDate <= today;

            // Check if current date is within the range
            return startDate <= today && item.EndDate.Value >= today;
        }

        private int CalculateDaysDuration(DateOnly? startDate, DateOnly? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return 0;

            return endDate.Value.DayNumber - startDate.Value.DayNumber + 1;
        }

        private DateTime? CalculateNextDue(ICollection<PrescriptionItem> items)
        {
            // Simplified logic - in real implementation, this would consider frequency patterns
            var activeItems = items.Where(item => CalculateItemIsActive(item)).ToList();
            if (!activeItems.Any()) return null;

            // Return next scheduled time (simplified to next hour for demonstration)
            return DateTime.UtcNow.AddHours(1);
        }
    }
}
