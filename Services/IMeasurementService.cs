using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IMeasurementService
    {
        // Measurement CRUD
        Task<ServiceResult<MeasurementDto>> CreateMeasurementAsync(MeasurementCreateDto dto, int tenantId);
        Task<ServiceResult<MeasurementDto>> GetMeasurementByIdAsync(long measurementId, int tenantId);
        Task<ServiceResult<MeasurementDto>> UpdateMeasurementAsync(long measurementId, MeasurementUpdateDto dto, int tenantId);
        Task<ServiceResult<bool>> DeleteMeasurementAsync(long measurementId, int tenantId);

        // Query measurements
        Task<ServiceResult<PagedResult<MeasurementDto>>> GetMeasurementsAsync(int tenantId, MeasurementQueryDto query);
        Task<ServiceResult<List<MeasurementDto>>> GetPatientMeasurementsAsync(int patientId, int tenantId, 
            string? type = null, DateTime? fromDate = null, DateTime? toDate = null, int? limit = null);

        // Statistics & Analytics
        Task<ServiceResult<List<MeasurementStatsDto>>> GetMeasurementStatsAsync(int patientId, int tenantId, 
            string? type = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ServiceResult<MeasurementStatsDto>> GetMeasurementStatsByTypeAsync(int patientId, int tenantId, 
            string type, DateTime? fromDate = null, DateTime? toDate = null);

        // Utility methods
        Task<ServiceResult<List<string>>> GetAvailableMeasurementTypesAsync(int tenantId, int? patientId = null);
        Task<ServiceResult<bool>> ValidateMeasurementAccessAsync(long measurementId, int tenantId, int? patientId = null);
        Task<ServiceResult<List<MeasurementDto>>> GetRecentMeasurementsAsync(int patientId, int tenantId, int days = 7);
    }
}
