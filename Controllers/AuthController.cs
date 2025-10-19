using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IPatientService patientService,
            IUserService userService,
            IOtpService otpService,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _patientService = patientService;
            _userService = userService;
            _otpService = otpService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Yêu cầu mã OTP cho đăng nhập
        /// </summary>
        [HttpPost("request-otp")]
        public async Task<ActionResult<ApiResponse<object>>> RequestOtp([FromBody] OtpRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Kiểm tra xem bệnh nhân có tồn tại không (chỉ cho purpose = login)
                if (dto.Purpose == "login")
                {
                    var patientResult = await _patientService.GetPatientByPhoneAsync(dto.PhoneNumber);
                    if (!patientResult.Success)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Số điện thoại chưa được đăng ký"));
                    }
                }

                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(dto.PhoneNumber, dto.Purpose);
                
                // Send SMS
                var smsSent = await _otpService.SendOtpSmsAsync(dto.PhoneNumber, otpCode);
                
                if (!smsSent)
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResult("Không thể gửi tin nhắn OTP"));
                }

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP đã được gửi đến số điện thoại của bạn",
                    expiresIn = "5 phút",
                    // For development only - remove in production
                    otpCode = otpCode
                }, "Gửi OTP thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting OTP for {PhoneNumber}", dto.PhoneNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi gửi OTP"));
            }
        }

        /// <summary>
        /// Xác thực OTP và đăng nhập bệnh nhân
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyOtp([FromBody] OtpVerifyDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Validate OTP
                var isValidOtp = await _otpService.ValidateOtpAsync(dto.PhoneNumber, dto.OtpCode, dto.Purpose);
                if (!isValidOtp)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn"));
                }

                // Get patient info
                var patientResult = await _patientService.GetPatientByPhoneAsync(dto.PhoneNumber);
                if (!patientResult.Success)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Không tìm thấy bệnh nhân"));
                }

                // Generate JWT token
                var token = _jwtService.GeneratePatientToken(patientResult.Data!);
                
                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    User = new UserInfoDto
                    {
                        UserId = patientResult.Data!.PatientId,
                        FullName = patientResult.Data!.FullName,
                        PhoneE164 = patientResult.Data!.PrimaryPhoneE164
                    }
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Đăng nhập thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {PhoneNumber}", dto.PhoneNumber);
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi xác thực OTP"));
            }
        }

        /// <summary>
        /// Yêu cầu OTP cho nhân viên (staff/doctors)
        /// </summary>
        [HttpPost("staff/request-otp")]
        public async Task<ActionResult<ApiResponse<object>>> RequestStaffOtp([FromBody] OtpRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Kiểm tra staff có tồn tại không
                if (dto.Purpose == "login")
                {
                    var staffResult = await _userService.GetUserByPhoneAsync(dto.PhoneNumber);
                    if (!staffResult.Success)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Số điện thoại nhân viên chưa được đăng ký"));
                    }

                    if (!staffResult.Data!.IsActive)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Tài khoản nhân viên đã bị vô hiệu hóa"));
                    }
                }
                
                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(dto.PhoneNumber, $"staff-{dto.Purpose}");
                
                // Send SMS
                var smsSent = await _otpService.SendOtpSmsAsync(dto.PhoneNumber, otpCode);
                
                if (!smsSent)
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResult("Không thể gửi tin nhắn OTP"));
                }

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP nhân viên đã được gửi đến số điện thoại",
                    expiresIn = "5 phút",
                    // For development only
                    otpCode = otpCode
                }, "Gửi OTP nhân viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting staff OTP for {PhoneNumber}", dto.PhoneNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi gửi OTP nhân viên"));
            }
        }

        /// <summary>
        /// Xác thực OTP và đăng nhập nhân viên
        /// </summary>
        [HttpPost("staff/verify-otp")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyStaffOtp([FromBody] OtpVerifyDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Validate OTP
                var isValidOtp = await _otpService.ValidateOtpAsync(dto.PhoneNumber, dto.OtpCode, $"staff-{dto.Purpose}");
                if (!isValidOtp)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn"));
                }

                // Get staff info from Users table
                var staffResult = await _userService.GetUserByPhoneAsync(dto.PhoneNumber);
                if (!staffResult.Success)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Không tìm thấy nhân viên"));
                }

                if (!staffResult.Data!.IsActive)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Tài khoản nhân viên đã bị vô hiệu hóa"));
                }

                var staffUser = new UserInfoDto
                {
                    UserId = staffResult.Data.UserId,
                    FullName = staffResult.Data.FullName,
                    Email = staffResult.Data.Email,
                    PhoneE164 = staffResult.Data.PhoneE164,
                    Role = staffResult.Data.Role,
                    TenantId = staffResult.Data.TenantId,
                    TenantName = staffResult.Data.TenantName
                };

                // Generate JWT token for staff
                var token = _jwtService.GenerateUserToken(staffUser);
                
                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    User = staffUser
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResult(authResponse, "Đăng nhập nhân viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying staff OTP for {PhoneNumber}", dto.PhoneNumber);
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResult("Có lỗi xảy ra khi xác thực OTP nhân viên"));
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        [HttpPost("validate-token")]
        public async Task<ActionResult<ApiResponse<object>>> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _jwtService.ValidateTokenAsync(token);
                
                if (isValid)
                {
                    var userId = _jwtService.GetUserIdFromToken(token);
                    return Ok(ApiResponse<object>.SuccessResult(new { 
                        valid = true, 
                        userId = userId 
                    }, "Token hợp lệ"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Token không hợp lệ"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi validate token"));
            }
        }

        /// <summary>
        /// Đăng nhập staff bằng email và mật khẩu
        /// </summary>
        [HttpPost("staff/login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> StaffLogin([FromBody] StaffLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _userService.AuthenticateAsync(dto);
            
            if (!result.Success)
                return Unauthorized(result);
            
            return Ok(result);
        }

        /// <summary>
        /// Đăng xuất (invalidate token - client-side)
        /// </summary>
        [HttpPost("logout")]
        public ActionResult<ApiResponse<object>> Logout()
        {
            // JWT tokens are stateless, so logout is typically handled client-side
            // by removing the token from storage
            return Ok(ApiResponse<object>.SuccessResult(new { 
                message = "Đăng xuất thành công. Vui lòng xóa token ở client." 
            }));
        }

        /// <summary>
        /// Yêu cầu reset mật khẩu - Gửi OTP qua email/SMS
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Kiểm tra user có tồn tại không (theo email hoặc phone)
                var userResult = !string.IsNullOrEmpty(dto.Email) 
                    ? await _userService.GetUserByEmailAsync(dto.Email)
                    : await _userService.GetUserByPhoneAsync(dto.PhoneNumber!);

                if (!userResult.Success)
                {
                    // Vẫn trả về success để tránh user enumeration attack
                    return Ok(ApiResponse<object>.SuccessResult(new
                    {
                        message = "Nếu tài khoản tồn tại, mã OTP sẽ được gửi đến email/số điện thoại của bạn",
                        expiresIn = "10 phút"
                    }, "Yêu cầu reset mật khẩu đã được xử lý"));
                }

                var user = userResult.Data!;

                // Kiểm tra tài khoản có bị vô hiệu hóa không
                if (!user.IsActive)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Tài khoản đã bị vô hiệu hóa"));
                }

                // Generate OTP cho reset password
                var identifier = !string.IsNullOrEmpty(dto.Email) ? dto.Email : dto.PhoneNumber!;
                var otpCode = await _otpService.GenerateOtpAsync(identifier, "reset-password");

                // Gửi OTP qua email hoặc SMS
                bool sent = false;
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    // Gửi qua email (cần implement email service)
                    sent = await SendPasswordResetEmail(dto.Email, user.FullName, otpCode);
                }
                else
                {
                    // Gửi qua SMS
                    sent = await _otpService.SendOtpSmsAsync(dto.PhoneNumber!, otpCode);
                }

                if (!sent)
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResult("Không thể gửi mã OTP. Vui lòng thử lại sau"));
                }

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP đã được gửi đến email/số điện thoại của bạn",
                    expiresIn = "10 phút",
                    // For development only - remove in production
                    otpCode = otpCode
                }, "Gửi OTP reset mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for {Identifier}", dto.Email ?? dto.PhoneNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi xử lý yêu cầu"));
            }
        }

        /// <summary>
        /// Xác thực OTP và reset mật khẩu mới
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Validate OTP
                var identifier = !string.IsNullOrEmpty(dto.Email) ? dto.Email : dto.PhoneNumber!;
                var isValidOtp = await _otpService.ValidateOtpAsync(identifier, dto.OtpCode, "reset-password");
                
                if (!isValidOtp)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn"));
                }

                // Tìm user
                var userResult = !string.IsNullOrEmpty(dto.Email)
                    ? await _userService.GetUserByEmailAsync(dto.Email)
                    : await _userService.GetUserByPhoneAsync(dto.PhoneNumber!);

                if (!userResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản"));
                }

                var user = userResult.Data!;

                // Kiểm tra tài khoản có bị vô hiệu hóa không
                if (!user.IsActive)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Tài khoản đã bị vô hiệu hóa"));
                }

                // Reset password
                var resetResult = await _userService.ResetPasswordAsync(user.UserId, dto.NewPassword);
                
                if (!resetResult.Success)
                {
                    return BadRequest(resetResult);
                }

                // Log activity
                _logger.LogInformation("Password reset successful for user {UserId} - {Email}", user.UserId, user.Email);

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mật khẩu đã được thay đổi thành công",
                    userId = user.UserId
                }, "Reset mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Identifier}", dto.Email ?? dto.PhoneNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi reset mật khẩu"));
            }
        }

        /// <summary>
        /// Đổi mật khẩu cho user đã đăng nhập
        /// </summary>
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] AuthChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Lấy user ID từ JWT token (cần implement JWT claims)
                // Tạm thời dùng email để tìm user
                var userResult = await _userService.GetUserByEmailAsync(dto.Email);
                
                if (!userResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản"));
                }

                var user = userResult.Data!;

                // Verify mật khẩu cũ
                var verifyResult = await _userService.VerifyPasswordAsync(user.UserId, dto.CurrentPassword);
                
                if (!verifyResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Mật khẩu hiện tại không đúng"));
                }

                // Đổi mật khẩu mới
                var changeResult = await _userService.ResetPasswordAsync(user.UserId, dto.NewPassword);
                
                if (!changeResult.Success)
                {
                    return BadRequest(changeResult);
                }

                _logger.LogInformation("Password changed successfully for user {UserId} - {Email}", user.UserId, user.Email);

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mật khẩu đã được thay đổi thành công"
                }, "Đổi mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi đổi mật khẩu"));
            }
        }

        /// <summary>
        /// Helper method để gửi email reset password
        /// </summary>
        private async Task<bool> SendPasswordResetEmail(string email, string fullName, string otpCode)
        {
            try
            {
                // TODO: Implement email service
                // Tạm thời return true, trong thực tế cần implement:
                // - SMTP service
                // - Email template
                // - Queue system cho reliability
                
                _logger.LogInformation("Sending password reset email to {Email} with OTP {OTP}", email, otpCode);
                
                // Simulate email sending
                await Task.Delay(100);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }
    }

    // DTOs for forgot password
    public class ForgotPasswordRequestDto
    {
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
    }

    public class ResetPasswordDto
    {
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = "";
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự")]
        public string NewPassword { get; set; } = "";
        
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class AuthChangePasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string CurrentPassword { get; set; } = "";
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự")]
        public string NewPassword { get; set; } = "";
        
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }
}
