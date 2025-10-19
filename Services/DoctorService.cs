using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(SavePlusDbContext context, ILogger<DoctorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<DoctorEditDto>> GetDoctorByUserIdAsync(int userId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorEditDto>.ErrorResult("Không tìm thấy thông tin bác sĩ");
                }

                var result = MapToDoctorEditDto(doctor);
                return ApiResponse<DoctorEditDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor by userId {UserId}", userId);
                return ApiResponse<DoctorEditDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin bác sĩ");
            }
        }

        public async Task<ApiResponse<DoctorEditDto>> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorEditDto>.ErrorResult("Không tìm thấy thông tin bác sĩ");
                }

                var result = MapToDoctorEditDto(doctor);
                return ApiResponse<DoctorEditDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor by id {DoctorId}", doctorId);
                return ApiResponse<DoctorEditDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin bác sĩ");
            }
        }

        public async Task<ApiResponse<DoctorEditDto>> UpdateDoctorSelfAsync(int userId, DoctorSelfUpdateDto dto)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorEditDto>.ErrorResult("Không tìm thấy thông tin bác sĩ");
                }

                // Update User fields
                doctor.User.FullName = dto.FullName;
                if (dto.PhoneE164 != null)
                    doctor.User.PhoneE164 = dto.PhoneE164;

                // Update Doctor fields
                if (dto.AvatarUrl != null)
                    doctor.AvatarUrl = dto.AvatarUrl;
                if (dto.Title != null)
                    doctor.Title = dto.Title;
                if (dto.PositionTitle != null)
                    doctor.PositionTitle = dto.PositionTitle;
                if (dto.Specialty != null)
                    doctor.Specialty = dto.Specialty;
                if (dto.LicenseNumber != null)
                    doctor.LicenseNumber = dto.LicenseNumber;
                if (dto.YearStarted.HasValue)
                    doctor.YearStarted = dto.YearStarted;
                if (dto.About != null)
                    doctor.About = dto.About;

                await _context.SaveChangesAsync();

                var result = MapToDoctorEditDto(doctor);
                return ApiResponse<DoctorEditDto>.SuccessResult(result, "Cập nhật thông tin thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor self for userId {UserId}", userId);
                return ApiResponse<DoctorEditDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông tin");
            }
        }

        public async Task<ApiResponse<DoctorEditDto>> UpdateDoctorByAdminAsync(int doctorId, DoctorAdminUpdateDto dto)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

                if (doctor == null)
                {
                    return ApiResponse<DoctorEditDto>.ErrorResult("Không tìm thấy thông tin bác sĩ");
                }

                // Update User fields
                doctor.User.FullName = dto.FullName;
                if (dto.PhoneE164 != null)
                    doctor.User.PhoneE164 = dto.PhoneE164;

                // Update Doctor fields
                if (dto.AvatarUrl != null)
                    doctor.AvatarUrl = dto.AvatarUrl;
                if (dto.Title != null)
                    doctor.Title = dto.Title;
                if (dto.PositionTitle != null)
                    doctor.PositionTitle = dto.PositionTitle;
                if (dto.Specialty != null)
                    doctor.Specialty = dto.Specialty;
                if (dto.LicenseNumber != null)
                    doctor.LicenseNumber = dto.LicenseNumber;
                if (dto.YearStarted.HasValue)
                    doctor.YearStarted = dto.YearStarted;
                if (dto.About != null)
                    doctor.About = dto.About;
                
                // Admin can update IsVerified
                if (dto.IsVerified.HasValue)
                    doctor.IsVerified = dto.IsVerified.Value;

                await _context.SaveChangesAsync();

                var result = MapToDoctorEditDto(doctor);
                return ApiResponse<DoctorEditDto>.SuccessResult(result, "Cập nhật thông tin bác sĩ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor by admin for doctorId {DoctorId}", doctorId);
                return ApiResponse<DoctorEditDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông tin bác sĩ");
            }
        }

        private DoctorEditDto MapToDoctorEditDto(Doctor doctor)
        {
            return new DoctorEditDto
            {
                DoctorId = doctor.DoctorId,
                TenantId = doctor.TenantId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? "",
                PhoneE164 = doctor.User.PhoneE164,
                AvatarUrl = doctor.AvatarUrl,
                Title = doctor.Title,
                PositionTitle = doctor.PositionTitle,
                Specialty = doctor.Specialty,
                LicenseNumber = doctor.LicenseNumber,
                YearStarted = doctor.YearStarted,
                About = doctor.About,
                IsVerified = doctor.IsVerified
            };
        }
    }
}
