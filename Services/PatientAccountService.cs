using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;
using System.Security.Cryptography;
using System.Text;

namespace SavePlus_API.Services
{
    public class PatientAccountService : IPatientAccountService
    {
        private readonly SavePlusDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IOtpService _otpService;
        private readonly ILogger<PatientAccountService> _logger;

        public PatientAccountService(
            SavePlusDbContext context,
            IJwtService jwtService,
            IOtpService otpService,
            ILogger<PatientAccountService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> RegisterAsync(PatientAccountRegisterDto dto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                if (await IsEmailExistsAsync(dto.Email))
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Email đã được đăng ký");
                }

                // Kiểm tra phone đã tồn tại
                if (await IsPhoneExistsAsync(dto.PhoneE164))
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Số điện thoại đã được đăng ký");
                }

                // Kiểm tra phone đã tồn tại trong Patients (PrimaryPhone)
                var phoneInPatients = await _context.Patients.AnyAsync(p => p.PrimaryPhoneE164 == dto.PhoneE164);
                if (phoneInPatients)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Số điện thoại đã được sử dụng");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tạo Patient trước
                    var patient = new Patient
                    {
                        FullName = dto.FullName,
                        PrimaryPhoneE164 = dto.PhoneE164,
                        Gender = dto.Gender,
                        DateOfBirth = dto.DateOfBirth,
                        Address = dto.Address,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    // Hash password
                    var (passwordHash, passwordSalt) = HashPassword(dto.Password);

                    // Tạo PatientAccount
                    var account = new PatientAccount
                    {
                        PatientId = patient.PatientId,
                        Email = dto.Email,
                        PhoneE164 = dto.PhoneE164,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        IsActive = true,
                        IsEmailVerified = false,
                        IsPhoneVerified = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PatientAccounts.Add(account);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var result = MapToResponseDto(account, patient);
                    return ApiResponse<PatientAccountResponseDto>.SuccessResult(result, "Đăng ký tài khoản thành công");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi tạo PatientAccount transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký PatientAccount");
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra khi đăng ký tài khoản");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginByEmailAsync(PatientAccountLoginDto dto)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.Email == dto.Email);

                if (account == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                if (!account.IsActive)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Tài khoản đã bị vô hiệu hóa");
                }

                // Verify password
                if (!VerifyPassword(dto.Password, account.PasswordHash, account.PasswordSalt))
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Email hoặc mật khẩu không đúng");
                }

                // Update last login
                await UpdateLastLoginAsync(account.AccountId);

                // Generate JWT token
                var userInfo = new UserInfoDto
                {
                    UserId = account.PatientId,
                    FullName = account.Patient.FullName,
                    Email = account.Email,
                    PhoneE164 = account.PhoneE164,
                    IsPatient = true
                };

                var token = _jwtService.GeneratePatientToken(userInfo);

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
                _logger.LogError(ex, "Lỗi khi đăng nhập bằng email {Email}", dto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi đăng nhập");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginByPhoneAsync(PatientAccountLoginByPhoneDto dto)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.PhoneE164 == dto.PhoneE164);

                if (account == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Số điện thoại hoặc mật khẩu không đúng");
                }

                if (!account.IsActive)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Tài khoản đã bị vô hiệu hóa");
                }

                // Verify password
                if (!VerifyPassword(dto.Password, account.PasswordHash, account.PasswordSalt))
                {
                    return ApiResponse<AuthResponseDto>.ErrorResult("Số điện thoại hoặc mật khẩu không đúng");
                }

                // Update last login
                await UpdateLastLoginAsync(account.AccountId);

                // Generate JWT token
                var userInfo = new UserInfoDto
                {
                    UserId = account.PatientId,
                    FullName = account.Patient.FullName,
                    Email = account.Email,
                    PhoneE164 = account.PhoneE164,
                    IsPatient = true
                };

                var token = _jwtService.GeneratePatientToken(userInfo);

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
                _logger.LogError(ex, "Lỗi khi đăng nhập bằng phone {Phone}", dto.PhoneE164);
                return ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi đăng nhập");
            }
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> GetByAccountIdAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.AccountId == accountId);

                if (account == null)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Không tìm thấy tài khoản");
                }

                var result = MapToResponseDto(account, account.Patient);
                return ApiResponse<PatientAccountResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy PatientAccount {AccountId}", accountId);
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> GetByPatientIdAsync(int patientId)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.PatientId == patientId);

                if (account == null)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Không tìm thấy tài khoản");
                }

                var result = MapToResponseDto(account, account.Patient);
                return ApiResponse<PatientAccountResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy PatientAccount theo PatientId {PatientId}", patientId);
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> GetByEmailAsync(string email)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.Email == email);

                if (account == null)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Không tìm thấy tài khoản với email này");
                }

                var result = MapToResponseDto(account, account.Patient);
                return ApiResponse<PatientAccountResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy PatientAccount theo Email {Email}", email);
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> GetByPhoneAsync(string phoneE164)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.PhoneE164 == phoneE164);

                if (account == null)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Không tìm thấy tài khoản với số điện thoại này");
                }

                var result = MapToResponseDto(account, account.Patient);
                return ApiResponse<PatientAccountResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy PatientAccount theo Phone {Phone}", phoneE164);
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<PatientAccountResponseDto>> UpdateAsync(int accountId, PatientAccountUpdateDto dto)
        {
            try
            {
                var account = await _context.PatientAccounts
                    .Include(pa => pa.Patient)
                    .FirstOrDefaultAsync(pa => pa.AccountId == accountId);

                if (account == null)
                {
                    return ApiResponse<PatientAccountResponseDto>.ErrorResult("Không tìm thấy tài khoản");
                }

                // Update email if provided
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != account.Email)
                {
                    if (await IsEmailExistsAsync(dto.Email))
                    {
                        return ApiResponse<PatientAccountResponseDto>.ErrorResult("Email đã được sử dụng");
                    }
                    account.Email = dto.Email;
                    account.IsEmailVerified = false; // Reset verification
                }

                // Update phone if provided
                if (!string.IsNullOrEmpty(dto.PhoneE164) && dto.PhoneE164 != account.PhoneE164)
                {
                    if (await IsPhoneExistsAsync(dto.PhoneE164))
                    {
                        return ApiResponse<PatientAccountResponseDto>.ErrorResult("Số điện thoại đã được sử dụng");
                    }
                    account.PhoneE164 = dto.PhoneE164;
                    account.IsPhoneVerified = false; // Reset verification
                }

                // Update patient info
                if (!string.IsNullOrEmpty(dto.FullName))
                    account.Patient.FullName = dto.FullName;

                if (!string.IsNullOrEmpty(dto.Gender))
                    account.Patient.Gender = dto.Gender;

                if (dto.DateOfBirth.HasValue)
                    account.Patient.DateOfBirth = dto.DateOfBirth;

                if (!string.IsNullOrEmpty(dto.Address))
                    account.Patient.Address = dto.Address;

                account.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = MapToResponseDto(account, account.Patient);
                return ApiResponse<PatientAccountResponseDto>.SuccessResult(result, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật PatientAccount {AccountId}", accountId);
                return ApiResponse<PatientAccountResponseDto>.ErrorResult("Có lỗi xảy ra khi cập nhật");
            }
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(int accountId, PatientAccountChangePasswordDto dto)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                // Verify current password
                if (!VerifyPassword(dto.CurrentPassword, account.PasswordHash, account.PasswordSalt))
                {
                    return ApiResponse<object>.ErrorResult("Mật khẩu hiện tại không đúng");
                }

                // Hash new password
                var (passwordHash, passwordSalt) = HashPassword(dto.NewPassword);
                account.PasswordHash = passwordHash;
                account.PasswordSalt = passwordSalt;
                account.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu PatientAccount {AccountId}", accountId);
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi đổi mật khẩu");
            }
        }

        public async Task<ApiResponse<bool>> VerifyPasswordAsync(int accountId, string password)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy tài khoản");
                }

                var isValid = VerifyPassword(password, account.PasswordHash, account.PasswordSalt);
                return ApiResponse<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify password");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(int accountId, string newPassword)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                var (passwordHash, passwordSalt) = HashPassword(newPassword);
                account.PasswordHash = passwordHash;
                account.PasswordSalt = passwordSalt;
                account.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Reset mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset password PatientAccount {AccountId}", accountId);
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi reset mật khẩu");
            }
        }

        public async Task<ApiResponse<object>> RequestEmailVerificationAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                if (account.IsEmailVerified)
                {
                    return ApiResponse<object>.ErrorResult("Email đã được xác thực");
                }

                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(account.Email, "verify-email");

                // TODO: Send email with OTP
                _logger.LogInformation("OTP for email verification: {OTP} (AccountId: {AccountId})", otpCode, accountId);

                return ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP đã được gửi đến email của bạn",
                    otpCode = otpCode // Remove in production
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi request email verification");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> VerifyEmailAsync(int accountId, string otpCode)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(account.Email, otpCode, "verify-email");
                if (!isValid)
                {
                    return ApiResponse<object>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn");
                }

                account.IsEmailVerified = true;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Xác thực email thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify email");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> RequestPhoneVerificationAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                if (account.IsPhoneVerified)
                {
                    return ApiResponse<object>.ErrorResult("Số điện thoại đã được xác thực");
                }

                if (string.IsNullOrEmpty(account.PhoneE164))
                {
                    return ApiResponse<object>.ErrorResult("Tài khoản chưa có số điện thoại");
                }

                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(account.PhoneE164, "verify-phone");

                // Send SMS
                var smsSent = await _otpService.SendOtpSmsAsync(account.PhoneE164, otpCode);
                if (!smsSent)
                {
                    return ApiResponse<object>.ErrorResult("Không thể gửi tin nhắn OTP");
                }

                return ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP đã được gửi đến số điện thoại của bạn",
                    otpCode = otpCode // Remove in production
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi request phone verification");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> VerifyPhoneAsync(int accountId, string otpCode)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                if (string.IsNullOrEmpty(account.PhoneE164))
                {
                    return ApiResponse<object>.ErrorResult("Tài khoản chưa có số điện thoại");
                }

                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(account.PhoneE164, otpCode, "verify-phone");
                if (!isValid)
                {
                    return ApiResponse<object>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn");
                }

                account.IsPhoneVerified = true;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Xác thực số điện thoại thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify phone");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> UpdateLastLoginAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                account.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi update last login");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> DeactivateAccountAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                account.IsActive = false;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Đã vô hiệu hóa tài khoản" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi deactivate account");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<ApiResponse<object>> ActivateAccountAsync(int accountId)
        {
            try
            {
                var account = await _context.PatientAccounts.FindAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản");
                }

                account.IsActive = true;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { message = "Đã kích hoạt tài khoản" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi activate account");
                return ApiResponse<object>.ErrorResult("Có lỗi xảy ra");
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.PatientAccounts.AnyAsync(pa => pa.Email == email);
        }

        public async Task<bool> IsPhoneExistsAsync(string phoneE164)
        {
            return await _context.PatientAccounts.AnyAsync(pa => pa.PhoneE164 == phoneE164);
        }

        // Helper methods
        private PatientAccountResponseDto MapToResponseDto(PatientAccount account, Patient patient)
        {
            return new PatientAccountResponseDto
            {
                AccountId = account.AccountId,
                PatientId = account.PatientId,
                Email = account.Email,
                PhoneE164 = account.PhoneE164,
                IsActive = account.IsActive,
                IsEmailVerified = account.IsEmailVerified,
                IsPhoneVerified = account.IsPhoneVerified,
                LastLoginAt = account.LastLoginAt,
                CreatedAt = account.CreatedAt,
                FullName = patient.FullName,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address
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
    }
}


