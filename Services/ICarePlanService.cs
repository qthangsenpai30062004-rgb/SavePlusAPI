using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface ICarePlanService
    {
        // CarePlan CRUD
        Task<ServiceResult<CarePlanDto>> CreateCarePlanAsync(CarePlanCreateDto dto, int tenantId, int createdBy);
        Task<ServiceResult<CarePlanDto>> GetCarePlanByIdAsync(int carePlanId, int tenantId);
        Task<ServiceResult<CarePlanDto>> UpdateCarePlanAsync(int carePlanId, CarePlanUpdateDto dto, int tenantId);
        Task<ServiceResult<bool>> DeleteCarePlanAsync(int carePlanId, int tenantId);
        Task<ServiceResult<PagedResult<CarePlanDto>>> GetCarePlansAsync(int tenantId, int? patientId = null, 
            string? status = null, int pageNumber = 1, int pageSize = 20);

        // CarePlan Items CRUD
        Task<ServiceResult<CarePlanItemDto>> CreateCarePlanItemAsync(int carePlanId, CarePlanItemCreateDto dto, int tenantId);
        Task<ServiceResult<CarePlanItemDto>> UpdateCarePlanItemAsync(int itemId, CarePlanItemUpdateDto dto, int tenantId);
        Task<ServiceResult<bool>> DeleteCarePlanItemAsync(int itemId, int tenantId);
        Task<ServiceResult<List<CarePlanItemDto>>> GetCarePlanItemsAsync(int carePlanId, int tenantId);

        // CarePlan Item Logs
        Task<ServiceResult<CarePlanItemLogDto>> LogCarePlanItemAsync(CarePlanItemLogCreateDto dto, int tenantId, int patientId);
        Task<ServiceResult<PagedResult<CarePlanItemLogDto>>> GetCarePlanItemLogsAsync(int tenantId, 
            int? patientId = null, int? carePlanId = null, int? itemId = null, 
            DateTime? fromDate = null, DateTime? toDate = null, 
            int pageNumber = 1, int pageSize = 20);

        // Progress & Analytics
        Task<ServiceResult<CarePlanProgressDto>> GetCarePlanProgressAsync(int carePlanId, int tenantId);
        Task<ServiceResult<List<CarePlanProgressDto>>> GetPatientCarePlanProgressAsync(int patientId, int? tenantId);

        // Utility methods
        Task<ServiceResult<bool>> ValidateCarePlanAccessAsync(int carePlanId, int tenantId, int? patientId = null);
        Task<ServiceResult<List<CarePlanDto>>> GetActiveCarePlansForPatientAsync(int patientId, int? tenantId);
    }
}
