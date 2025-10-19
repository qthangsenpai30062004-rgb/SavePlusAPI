using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using SavePlus_API.Constants;
using System.Security.Cryptography;
using System.Text;

namespace SavePlus_API.Services
{
    public class UserService : IUserService
    {
        private readonly SavePlusDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly DoctorSearchService _doctorSearchService;

        public UserService(
            SavePlusDbContext context, 
            IJwtService jwtService, 
            ILogger<UserService> logger,
            DoctorSearchService doctorSearchService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _doctorSearchService = doctorSearchService;
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateDto dto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var emailExists = await EmailExistsAsync(dto.Email);
                if (emailExists.Data)
                {
                    return ApiResponse<UserDto>.ErrorResult("Email đã tồn tại trong hệ thống");
                }

                // Kiểm tra phone đã tồn tại (nếu có)
                if (!string.IsNullOrEmpty(dto.PhoneE164))
                {
                    var phoneExists = await PhoneExistsAsync(dto.PhoneE164);
                    if (phoneExists.Data)
                    {
                        return ApiResponse<UserDto>.ErrorResult("Số điện thoại đã tồn tại trong hệ thống");
                    }
                }

                // Kiểm tra tenant tồn tại (trừ SystemAdmin)
                if (dto.Role != UserRoles.SystemAdmin)
                {
                    var tenantExists = await _context.Tenants.AnyAsync(t => t.TenantId == dto.TenantId);
                    if (!tenantExists)
                    {
                        return ApiResponse<UserDto>.ErrorResult("Tenant không tồn tại");
                    }
                }

                // Hash password
                var (passwordHash, passwordSalt) = HashPassword(dto.Password);

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneE164 = dto.PhoneE164,
                    Role = dto.Role,
                    TenantId = dto.Role == UserRoles.SystemAdmin ? null : dto.TenantId,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var result = await GetUserByIdAsync(user.UserId);
                return result.Success ? 
                    ApiResponse<UserDto>.SuccessResult(result.Data!, "Tạo user thành công") :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo user");
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi tạo user");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Không tìm thấy user");
                }

                var result = MapToUserDto(user);
                return ApiResponse<UserDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin user");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Không tìm thấy user với email này");
                }

                var result = MapToUserDto(user);
                return ApiResponse<UserDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm user theo email {Email}", email);
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi tìm user");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByPhoneAsync(string phoneNumber)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.PhoneE164 == phoneNumber);

                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Không tìm thấy user với số điện thoại này");
                }

                var result = MapToUserDto(user);
                return ApiResponse<UserDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm user theo số điện thoại {PhoneNumber}", phoneNumber);
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi tìm user");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(int userId, UserUpdateDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResult("Không tìm thấy user");
                }

                // Kiểm tra email mới nếu có thay đổi
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    var emailExists = await EmailExistsAsync(dto.Email, userId);
                    if (emailExists.Data)
                    {
                        return ApiResponse<UserDto>.ErrorResult("Email đã tồn tại trong hệ thống");
                    }
                    user.Email = dto.Email;
                }

                // Kiểm tra phone mới nếu có thay đổi
                if (!string.IsNullOrEmpty(dto.PhoneE164) && dto.PhoneE164 != user.PhoneE164)
                {
                    var phoneExists = await PhoneExistsAsync(dto.PhoneE164, userId);
                    if (phoneExists.Data)
                    {
                        return ApiResponse<UserDto>.ErrorResult("Số điện thoại đã tồn tại trong hệ thống");
                    }
                    user.PhoneE164 = dto.PhoneE164;
                }

                if (!string.IsNullOrEmpty(dto.FullName))
                    user.FullName = dto.FullName;
                if (!string.IsNullOrEmpty(dto.Role))
                    user.Role = dto.Role;
                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                var result = await GetUserByIdAsync(userId);
                return result.Success ? 
                    ApiResponse<UserDto>.SuccessResult(result.Data!, "Cập nhật user thành công") :
                    result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật user {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra khi cập nhật user");
            }
        }

        public async Task<ApiResponse<bool>> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy user");
                }

                user.IsActive = false;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Vô hiệu hóa user thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi vô hiệu hóa user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi vô hiệu hóa user");
            }
        }

        public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(int? tenantId = null, int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Tenant)
                    .AsQueryable();

                if (tenantId.HasValue)
                    query = query.Where(u => u.TenantId == tenantId.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.FullName.Contains(searchTerm) || 
                                           u.Email.Contains(searchTerm) ||
                                           (u.PhoneE164 != null && u.PhoneE164.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<UserDto>
                {
                    Data = users.Select(MapToUserDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<UserDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách users");
                return ApiResponse<PagedResult<UserDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách users");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> AuthenticateAsync(StaffLoginDto dto)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Tenant)
                    .Where(u => u.Email == dto.Email && u.IsActive);

                if (dto.TenantId.HasValue)
                    query = query.Where(u => u.TenantId == dto.TenantId.Value);

                var user = await query.FirstOrDefaultAsync();

                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                if (!VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                var userInfo = new UserInfoDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneE164 = user.PhoneE164,
                    Role = user.Role,
                    TenantId = user.TenantId,
                    TenantName = user.Tenant?.Name
                };

                var token = _jwtService.GenerateUserToken(userInfo);

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    User = userInfo
                };

                return ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực user {Email}", dto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi đăng nhập");
            }
        }

        public async Task<ApiResponse<bool>> ValidatePasswordAsync(int userId, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy user");
                }

                var isValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
                return ApiResponse<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate password cho user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi validate password");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy user");
                }

                if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    return ApiResponse<bool>.ErrorResult("Mật khẩu hiện tại không đúng");
                }

                var (newPasswordHash, newPasswordSalt) = HashPassword(dto.NewPassword);
                user.PasswordHash = newPasswordHash;
                user.PasswordSalt = newPasswordSalt;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu cho user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đổi mật khẩu");
            }
        }

        public async Task<ApiResponse<UserWithDoctorDto>> GetUserWithDoctorInfoAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .Include(u => u.Doctor)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return ApiResponse<UserWithDoctorDto>.ErrorResult("Không tìm thấy user");
                }

                var result = new UserWithDoctorDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneE164 = user.PhoneE164,
                    Role = user.Role,
                    TenantId = user.TenantId,
                    TenantName = user.Tenant?.Name,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    DoctorId = user.Doctor?.DoctorId,
                    LicenseNumber = user.Doctor?.LicenseNumber,
                    Specialty = user.Doctor?.Specialty
                };

                return ApiResponse<UserWithDoctorDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user với doctor info {UserId}", userId);
                return ApiResponse<UserWithDoctorDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin user");
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetUsersByTenantAsync(int tenantId, string? role = null)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Tenant)
                    .Where(u => u.TenantId == tenantId && u.IsActive);

                if (!string.IsNullOrEmpty(role))
                    query = query.Where(u => u.Role == role);

                var users = await query
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                var result = users.Select(MapToUserDto).ToList();
                return ApiResponse<List<UserDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy users theo tenant {TenantId}", tenantId);
                return ApiResponse<List<UserDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách users");
            }
        }

        public async Task<ApiResponse<bool>> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.Email == email);
                if (excludeUserId.HasValue)
                    query = query.Where(u => u.UserId != excludeUserId.Value);

                var exists = await query.AnyAsync();
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra email tồn tại");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi kiểm tra email");
            }
        }

        public async Task<ApiResponse<bool>> PhoneExistsAsync(string phoneNumber, int? excludeUserId = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.PhoneE164 == phoneNumber);
                if (excludeUserId.HasValue)
                    query = query.Where(u => u.UserId != excludeUserId.Value);

                var exists = await query.AnyAsync();
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra phone tồn tại");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi kiểm tra phone");
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneE164 = user.PhoneE164,
                Role = user.Role,
                TenantId = user.TenantId,
                TenantName = user.Tenant?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        private (byte[] hash, byte[] salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return (hash, salt);
        }

        private bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }

        public async Task<ApiResponse<object>> CreateDoctorRecordAsync(int userId, string? specialty, string? licenseNumber)
        {
            try
            {
                // Kiểm tra user có tồn tại và có role Doctor không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy user");
                }

                if (user.Role != UserRoles.Doctor)
                {
                    return ApiResponse<object>.ErrorResult("User không có role Doctor");
                }

                if (user.TenantId == null)
                {
                    return ApiResponse<object>.ErrorResult("User chưa được gán vào tenant");
                }

                // Kiểm tra đã có Doctor record chưa
                var existingDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (existingDoctor != null)
                {
                    return ApiResponse<object>.SuccessResult(new { 
                        DoctorId = existingDoctor.DoctorId,
                        Message = "Doctor record đã tồn tại" 
                    });
                }

                // Tạo Doctor record mới
                var doctor = new Doctor
                {
                    TenantId = user.TenantId.Value,
                    UserId = userId,
                    Specialty = specialty ?? "Đa khoa",
                    LicenseNumber = licenseNumber ?? $"DOC-{user.TenantId:000}-{DateTime.Now:yyyyMMdd}-{userId:000}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { 
                    DoctorId = doctor.DoctorId,
                    UserId = doctor.UserId,
                    TenantId = doctor.TenantId,
                    Specialty = doctor.Specialty,
                    LicenseNumber = doctor.LicenseNumber,
                    Message = "Tạo Doctor record thành công" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Doctor record cho User {UserId}", userId);
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi tạo Doctor record");
            }
        }

        public async Task<ApiResponse<List<DoctorSearchDto>>> SearchDoctorsInTenantAsync(int tenantId, string searchTerm, int limit = 10)
        {
            // Delegate to specialized DoctorSearchService for better performance and maintainability
            return await _doctorSearchService.SearchDoctorsInTenantAsync(tenantId, searchTerm, limit);
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(int userId, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy user");
                }

                // Validate password strength
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    return ApiResponse<bool>.ErrorResult("Mật khẩu phải có ít nhất 6 ký tự");
                }

                CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user {UserId}", userId);
                return ApiResponse<bool>.SuccessResult(true, "Reset mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi reset mật khẩu");
            }
        }

        public async Task<ApiResponse<bool>> VerifyPasswordAsync(int userId, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy user");
                }

                if (user.PasswordHash == null || user.PasswordSalt == null)
                {
                    return ApiResponse<bool>.ErrorResult("User chưa có mật khẩu");
                }

                var isValid = VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
                
                if (isValid)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Mật khẩu đúng");
                }
                else
                {
                    return ApiResponse<bool>.ErrorResult("Mật khẩu không đúng");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xác thực mật khẩu");
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }
    }
}
