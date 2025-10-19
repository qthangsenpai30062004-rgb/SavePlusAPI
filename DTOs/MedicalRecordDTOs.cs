using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho hiển thị thông tin medical record
    public class MedicalRecordDto
    {
        public long RecordId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public string Title { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string? RecordType { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public string? PatientName { get; set; }
        public string? TenantName { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    // DTO cho tạo medical record mới
    public class MedicalRecordCreateDto
    {
        [Required(ErrorMessage = "ID phòng khám là bắt buộc")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "ID bệnh nhân là bắt buộc")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Tiêu đề hồ sơ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "URL file là bắt buộc")]
        [StringLength(500, ErrorMessage = "URL file không được vượt quá 500 ký tự")]
        public string FileUrl { get; set; } = null!;

        [StringLength(30, ErrorMessage = "Loại hồ sơ không được vượt quá 30 ký tự")]
        public string? RecordType { get; set; }

        public int? CreatedByUserId { get; set; }
    }

    // DTO cho cập nhật medical record
    public class MedicalRecordUpdateDto
    {
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "URL file không được vượt quá 500 ký tự")]
        public string? FileUrl { get; set; }

        [StringLength(30, ErrorMessage = "Loại hồ sơ không được vượt quá 30 ký tự")]
        public string? RecordType { get; set; }
    }

    // DTO cho lọc medical record
    public class MedicalRecordFilterDto
    {
        public int? TenantId { get; set; }
        public int? PatientId { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? RecordType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // DTO cho báo cáo medical record
    public class MedicalRecordReportDto
    {
        public int TotalRecords { get; set; }
        public Dictionary<string, int> RecordTypes { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RecordsByMonth { get; set; } = new Dictionary<string, int>();
        public List<MedicalRecordDto> RecentRecords { get; set; } = new List<MedicalRecordDto>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    // DTO cho upload file
    public class MedicalRecordUploadDto
    {
        [Required(ErrorMessage = "ID phòng khám là bắt buộc")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "ID bệnh nhân là bắt buộc")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Tiêu đề hồ sơ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [StringLength(30, ErrorMessage = "Loại hồ sơ không được vượt quá 30 ký tự")]
        public string? RecordType { get; set; }

        public int? CreatedByUserId { get; set; }

        [Required(ErrorMessage = "File là bắt buộc")]
        public IFormFile File { get; set; } = null!;
    }

    // DTO cho thống kê medical record theo patient
    public class PatientMedicalRecordSummaryDto
    {
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, int> RecordTypeCount { get; set; } = new Dictionary<string, int>();
        public DateTime? LastRecordDate { get; set; }
        public List<MedicalRecordDto> RecentRecords { get; set; } = new List<MedicalRecordDto>();
    }
}
