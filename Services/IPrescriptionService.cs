using SavePlus_API.DTOs;

namespace SavePlus_API.Services
{
    public interface IPrescriptionService
    {
        // Prescription CRUD
        Task<ServiceResult<PrescriptionDto>> CreatePrescriptionAsync(PrescriptionCreateDto dto, int tenantId);
        Task<ServiceResult<PrescriptionDto>> GetPrescriptionByIdAsync(int prescriptionId, int tenantId);
        Task<ServiceResult<PrescriptionDto>> UpdatePrescriptionAsync(int prescriptionId, PrescriptionUpdateDto dto, int tenantId);
        Task<ServiceResult<bool>> DeletePrescriptionAsync(int prescriptionId, int tenantId);

        // Query prescriptions
        Task<ServiceResult<PagedResult<PrescriptionDto>>> GetPrescriptionsAsync(int tenantId, PrescriptionQueryDto query);
        Task<ServiceResult<List<PrescriptionDto>>> GetPatientPrescriptionsAsync(int patientId, int tenantId, 
            string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ServiceResult<List<PrescriptionDto>>> GetDoctorPrescriptionsAsync(int doctorId, int tenantId,
            string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int? limit = null);

        // Prescription Items CRUD
        Task<ServiceResult<PrescriptionItemDto>> CreatePrescriptionItemAsync(int prescriptionId, PrescriptionItemCreateDto dto, int tenantId);
        Task<ServiceResult<PrescriptionItemDto>> UpdatePrescriptionItemAsync(int itemId, PrescriptionItemUpdateDto dto, int tenantId);
        Task<ServiceResult<bool>> DeletePrescriptionItemAsync(int itemId, int tenantId);
        Task<ServiceResult<List<PrescriptionItemDto>>> GetPrescriptionItemsAsync(int prescriptionId, int tenantId);

        // Utility methods
        Task<ServiceResult<bool>> ValidatePrescriptionAccessAsync(int prescriptionId, int tenantId, int? patientId = null, int? doctorId = null);
        Task<ServiceResult<List<PrescriptionDto>>> GetActivePrescriptionsForPatientAsync(int patientId, int tenantId);
        Task<ServiceResult<List<string>>> GetMostPrescribedDrugsAsync(int tenantId, int? doctorId = null, int? limit = 20);
        Task<ServiceResult<bool>> CanDoctorPrescribeAsync(int doctorId, int tenantId);
    }
}
