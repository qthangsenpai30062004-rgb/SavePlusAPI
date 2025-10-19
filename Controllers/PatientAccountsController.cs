using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;

namespace SavePlus_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientAccountsController : ControllerBase
    {
        private readonly IPatientAccountService _patientAccountService;
        private readonly IOtpService _otpService;
        private readonly ILogger<PatientAccountsController> _logger;

        public PatientAccountsController(
            IPatientAccountService patientAccountService,
            IOtpService otpService,
            ILogger<PatientAccountsController> logger)
        {
            _patientAccountService = patientAccountService;
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký tài khoản Patient mới
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<PatientAccountResponseDto>>> Register([FromBody] PatientAccountRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PatientAccountResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Đăng nhập bằng Email và Password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] PatientAccountLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.LoginByEmailAsync(dto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Đăng nhập bằng Phone và Password
        /// </summary>
        [HttpPost("login-by-phone")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginByPhone([FromBody] PatientAccountLoginByPhoneDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.LoginByPhoneAsync(dto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin tài khoản theo AccountId
        /// </summary>
        [HttpGet("{accountId}")]
        public async Task<ActionResult<ApiResponse<PatientAccountResponseDto>>> GetByAccountId(int accountId)
        {
            var result = await _patientAccountService.GetByAccountIdAsync(accountId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin tài khoản theo PatientId
        /// </summary>
        [HttpGet("by-patient/{patientId}")]
        public async Task<ActionResult<ApiResponse<PatientAccountResponseDto>>> GetByPatientId(int patientId)
        {
            var result = await _patientAccountService.GetByPatientIdAsync(patientId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin tài khoản theo Email
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<ApiResponse<PatientAccountResponseDto>>> GetByEmail(string email)
        {
            var result = await _patientAccountService.GetByEmailAsync(email);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản
        /// </summary>
        [HttpPut("{accountId}")]
        public async Task<ActionResult<ApiResponse<PatientAccountResponseDto>>> Update(int accountId, [FromBody] PatientAccountUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PatientAccountResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.UpdateAsync(accountId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("{accountId}/change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(int accountId, [FromBody] PatientAccountChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.ChangePasswordAsync(accountId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Quên mật khẩu - Gửi OTP qua email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] PatientAccountForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Get account by email
                var accountResult = await _patientAccountService.GetByEmailAsync(dto.Email);
                if (!accountResult.Success)
                {
                    // Vẫn trả về success để tránh user enumeration
                    return Ok(ApiResponse<object>.SuccessResult(new
                    {
                        message = "Nếu email tồn tại, mã OTP sẽ được gửi đến email của bạn"
                    }));
                }

                var account = accountResult.Data!;

                // Generate OTP
                var otpCode = await _otpService.GenerateOtpAsync(account.Email, "reset-password");

                // TODO: Send email with OTP
                _logger.LogInformation("Password reset OTP for {Email}: {OTP}", account.Email, otpCode);

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Mã OTP đã được gửi đến email của bạn",
                    otpCode = otpCode // Remove in production
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Reset mật khẩu với OTP
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] PatientAccountResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Validate OTP
                var isValidOtp = await _otpService.ValidateOtpAsync(dto.Email, dto.OtpCode, "reset-password");
                if (!isValidOtp)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Mã OTP không hợp lệ hoặc đã hết hạn"));
                }

                // Get account
                var accountResult = await _patientAccountService.GetByEmailAsync(dto.Email);
                if (!accountResult.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy tài khoản"));
                }

                var account = accountResult.Data!;

                // Reset password
                var resetResult = await _patientAccountService.ResetPasswordAsync(account.AccountId, dto.NewPassword);

                if (!resetResult.Success)
                    return BadRequest(resetResult);

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Reset mật khẩu thành công"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Request OTP để verify email
        /// </summary>
        [HttpPost("{accountId}/verify-email/request")]
        public async Task<ActionResult<ApiResponse<object>>> RequestEmailVerification(int accountId)
        {
            var result = await _patientAccountService.RequestEmailVerificationAsync(accountId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify email với OTP
        /// </summary>
        [HttpPost("{accountId}/verify-email")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(int accountId, [FromBody] PatientAccountVerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.VerifyEmailAsync(accountId, dto.OtpCode);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Request OTP để verify phone
        /// </summary>
        [HttpPost("{accountId}/verify-phone/request")]
        public async Task<ActionResult<ApiResponse<object>>> RequestPhoneVerification(int accountId)
        {
            var result = await _patientAccountService.RequestPhoneVerificationAsync(accountId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify phone với OTP
        /// </summary>
        [HttpPost("{accountId}/verify-phone")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyPhone(int accountId, [FromBody] PatientAccountVerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            var result = await _patientAccountService.VerifyPhoneAsync(accountId, dto.OtpCode);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Vô hiệu hóa tài khoản
        /// </summary>
        [HttpPost("{accountId}/deactivate")]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateAccount(int accountId)
        {
            var result = await _patientAccountService.DeactivateAccountAsync(accountId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Kích hoạt tài khoản
        /// </summary>
        [HttpPost("{accountId}/activate")]
        public async Task<ActionResult<ApiResponse<object>>> ActivateAccount(int accountId)
        {
            var result = await _patientAccountService.ActivateAccountAsync(accountId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(string email)
        {
            var exists = await _patientAccountService.IsEmailExistsAsync(email);
            return Ok(ApiResponse<bool>.SuccessResult(exists));
        }

        /// <summary>
        /// Kiểm tra phone đã tồn tại chưa
        /// </summary>
        [HttpGet("check-phone/{phone}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPhoneExists(string phone)
        {
            var exists = await _patientAccountService.IsPhoneExistsAsync(phone);
            return Ok(ApiResponse<bool>.SuccessResult(exists));
        }
    }
}


