using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SavePlus_API.Services
{
    /// <summary>
    /// Service xử lý tìm kiếm bác sĩ trong hệ thống
    /// </summary>
    public class DoctorSearchService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<DoctorSearchService> _logger;

        // Giới hạn số lượng kết quả tối đa
        private const int MAX_SEARCH_LIMIT = 50;
        private const int DEFAULT_SEARCH_LIMIT = 10;
        private const int MIN_SEARCH_TERM_LENGTH = 2;

        public DoctorSearchService(
            SavePlusDbContext context,
            ILogger<DoctorSearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tìm kiếm bác sĩ trong tenant theo từ khóa
        /// </summary>
        /// <param name="tenantId">ID của tenant cần tìm</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên, email, chứng chỉ, chuyên khoa)</param>
        /// <param name="limit">Số lượng kết quả tối đa (mặc định 10, max 50)</param>
        /// <returns>Danh sách bác sĩ tìm được</returns>
        public async Task<ApiResponse<List<DoctorSearchDto>>> SearchDoctorsInTenantAsync(
            int tenantId,
            string searchTerm,
            int limit = DEFAULT_SEARCH_LIMIT)
        {
            try
            {
                // Validate và chuẩn hóa input
                searchTerm = searchTerm?.Trim() ?? string.Empty;

                // Kiểm tra độ dài tối thiểu của search term
                if (searchTerm.Length < MIN_SEARCH_TERM_LENGTH)
                {
                    return ApiResponse<List<DoctorSearchDto>>.SuccessResult(new List<DoctorSearchDto>());
                }

                // Normalize limit về khoảng hợp lệ
                limit = Math.Max(1, Math.Min(limit, MAX_SEARCH_LIMIT));

                // Escape ký tự đặc biệt trong LIKE pattern
                var likePattern = $"%{EscapeLikePattern(searchTerm)}%";

                // Query bác sĩ trong tenant
                var doctors = await _context.Doctors
                    .AsNoTracking()
                    .Where(d =>
                        // Thuộc đúng tenant
                        d.TenantId == tenantId &&
                        // Bác sĩ đang active
                        d.IsActive &&
                        // User tương ứng cũng active
                        d.User.IsActive &&
                        // Khớp một trong các tiêu chí tìm kiếm
                        (
                            EF.Functions.Like(d.User.FullName, likePattern) ||
                            (d.User.Email != null && EF.Functions.Like(d.User.Email, likePattern)) ||
                            (d.LicenseNumber != null && EF.Functions.Like(d.LicenseNumber, likePattern)) ||
                            (d.Specialty != null && EF.Functions.Like(d.Specialty, likePattern))
                        )
                    )
                    .OrderBy(d => d.User.FullName)
                    .Take(limit)
                    .Select(d => new DoctorSearchDto
                    {
                        UserId = d.UserId,
                        DoctorId = (int?)d.DoctorId, // Nullable to match DTO
                        FullName = d.User.FullName,
                        Email = d.User.Email ?? string.Empty,
                        Specialty = d.Specialty ?? string.Empty,
                        LicenseNumber = d.LicenseNumber ?? string.Empty
                    })
                    .ToListAsync();

                return ApiResponse<List<DoctorSearchDto>>.SuccessResult(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bác sĩ trong tenant {TenantId} với từ khóa '{SearchTerm}'",
                    tenantId, searchTerm);
                return ApiResponse<List<DoctorSearchDto>>.ErrorResult(
                    "Đã xảy ra lỗi khi tìm kiếm bác sĩ. Vui lòng thử lại sau.");
            }
        }

        /// <summary>
        /// Escape các ký tự đặc biệt trong LIKE pattern để tránh SQL injection và lỗi pattern
        /// </summary>
        /// <param name="input">Chuỗi cần escape</param>
        /// <returns>Chuỗi đã được escape</returns>
        private static string EscapeLikePattern(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Escape các ký tự đặc biệt trong LIKE: %, _, [
            return input
                .Replace("[", "[[]")  // Escape [ trước
                .Replace("%", "[%]")  // Escape %
                .Replace("_", "[_]"); // Escape _
        }
    }
}
