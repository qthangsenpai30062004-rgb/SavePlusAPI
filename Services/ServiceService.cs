using SavePlus_API.DTOs;
using SavePlus_API.Models;
using Microsoft.EntityFrameworkCore;

namespace SavePlus_API.Services
{
    public interface IServiceService
    {
        Task<ApiResponse<List<ServiceDto>>> GetTenantServicesAsync(int tenantId);
        Task<ApiResponse<ServiceDto>> GetServiceByIdAsync(int serviceId);
        Task<ApiResponse<ServiceDto>> CreateServiceAsync(ServiceCreateDto dto);
        Task<ApiResponse<ServiceDto>> UpdateServiceAsync(int serviceId, ServiceUpdateDto dto);
        Task<ApiResponse<bool>> DeleteServiceAsync(int serviceId);
    }

    public class ServiceService : IServiceService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<ServiceService> _logger;

        public ServiceService(SavePlusDbContext context, ILogger<ServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ServiceDto>>> GetTenantServicesAsync(int tenantId)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.TenantId == tenantId && s.IsActive)
                    .Include(s => s.Tenant)
                    .OrderBy(s => s.BasePrice)
                    .Select(s => new ServiceDto
                    {
                        ServiceId = s.ServiceId,
                        TenantId = s.TenantId,
                        TenantName = s.Tenant != null ? s.Tenant.Name : string.Empty,
                        Name = s.Name,
                        Description = s.Description,
                        BasePrice = s.BasePrice,
                        ServiceType = s.ServiceType,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<ServiceDto>>.SuccessResult(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for tenant {TenantId}", tenantId);
                return ApiResponse<List<ServiceDto>>.ErrorResult("Lỗi khi tải danh sách dịch vụ");
            }
        }

        public async Task<ApiResponse<ServiceDto>> GetServiceByIdAsync(int serviceId)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.Tenant)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service == null)
                {
                    return ApiResponse<ServiceDto>.ErrorResult("Không tìm thấy dịch vụ");
                }

                var dto = new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    TenantId = service.TenantId,
                    TenantName = service.Tenant?.Name ?? string.Empty,
                    Name = service.Name,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    ServiceType = service.ServiceType,
                    IsActive = service.IsActive,
                    CreatedAt = service.CreatedAt
                };

                return ApiResponse<ServiceDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service {ServiceId}", serviceId);
                return ApiResponse<ServiceDto>.ErrorResult("Lỗi khi tải thông tin dịch vụ");
            }
        }

        public async Task<ApiResponse<ServiceDto>> CreateServiceAsync(ServiceCreateDto dto)
        {
            try
            {
                // Validate tenant exists
                var tenant = await _context.Tenants.FindAsync(dto.TenantId);
                if (tenant == null)
                {
                    return ApiResponse<ServiceDto>.ErrorResult("Không tìm thấy phòng khám");
                }

                var service = new Service
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Description = dto.Description,
                    BasePrice = dto.BasePrice,
                    ServiceType = dto.ServiceType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                var resultDto = new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    TenantId = service.TenantId,
                    TenantName = tenant.Name,
                    Name = service.Name,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    ServiceType = service.ServiceType,
                    IsActive = service.IsActive,
                    CreatedAt = service.CreatedAt
                };

                return ApiResponse<ServiceDto>.SuccessResult(resultDto, "Tạo dịch vụ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return ApiResponse<ServiceDto>.ErrorResult("Lỗi khi tạo dịch vụ");
            }
        }

        public async Task<ApiResponse<ServiceDto>> UpdateServiceAsync(int serviceId, ServiceUpdateDto dto)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.Tenant)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service == null)
                {
                    return ApiResponse<ServiceDto>.ErrorResult("Không tìm thấy dịch vụ");
                }

                if (dto.Name != null) service.Name = dto.Name;
                if (dto.Description != null) service.Description = dto.Description;
                if (dto.BasePrice.HasValue) service.BasePrice = dto.BasePrice.Value;
                if (dto.ServiceType != null) service.ServiceType = dto.ServiceType;
                if (dto.IsActive.HasValue) service.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                var resultDto = new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    TenantId = service.TenantId,
                    TenantName = service.Tenant?.Name ?? string.Empty,
                    Name = service.Name,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    ServiceType = service.ServiceType,
                    IsActive = service.IsActive,
                    CreatedAt = service.CreatedAt
                };

                return ApiResponse<ServiceDto>.SuccessResult(resultDto, "Cập nhật dịch vụ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", serviceId);
                return ApiResponse<ServiceDto>.ErrorResult("Lỗi khi cập nhật dịch vụ");
            }
        }

        public async Task<ApiResponse<bool>> DeleteServiceAsync(int serviceId)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy dịch vụ");
                }

                // Soft delete
                service.IsActive = false;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa dịch vụ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", serviceId);
                return ApiResponse<bool>.ErrorResult("Lỗi khi xóa dịch vụ");
            }
        }
    }
}
