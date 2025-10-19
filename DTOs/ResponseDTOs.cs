using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO chuẩn cho API response
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Thành công";
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    // DTO cho phân trang
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // Service result pattern for business logic layer
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Errors { get; set; }

        public static ServiceResult<T> SuccessResult(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> ErrorResult(string errorMessage)
        {
            return new ServiceResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static ServiceResult<T> ErrorResult(List<string> errors)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Errors = errors,
                ErrorMessage = string.Join("; ", errors)
            };
        }
    }

    // DTO cho authentication
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneE164 { get; set; }
        public string? Role { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public bool IsPatient { get; set; } = false;
    }

    // DTO cho OTP verification
    public class OtpRequestDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "Mục đích sử dụng là bắt buộc")]
        public string Purpose { get; set; } = "login"; // "login", "register", "reset_password"
    }

    public class OtpVerifyDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = null!;
        
        [Required(ErrorMessage = "Mục đích sử dụng là bắt buộc")]
        public string Purpose { get; set; } = "login"; // "login", "register", "reset_password"
    }
}
