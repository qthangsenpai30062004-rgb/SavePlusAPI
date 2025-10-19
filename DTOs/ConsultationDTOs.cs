using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    // DTO cho hiển thị thông tin consultation
    public class ConsultationDto
    {
        public int ConsultationId { get; set; }
        public int AppointmentId { get; set; }
        public string? Summary { get; set; }
        public string? DiagnosisCode { get; set; }
        public string? Advice { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Thông tin appointment liên quan
        public AppointmentDto? Appointment { get; set; }
    }

    // DTO cho tạo consultation mới
    public class ConsultationCreateDto
    {
        [Required(ErrorMessage = "ID cuộc hẹn là bắt buộc")]
        public int AppointmentId { get; set; }

        [StringLength(2000, ErrorMessage = "Tóm tắt không được vượt quá 2000 ký tự")]
        public string? Summary { get; set; }

        [StringLength(50, ErrorMessage = "Mã chẩn đoán không được vượt quá 50 ký tự")]
        public string? DiagnosisCode { get; set; }

        [StringLength(2000, ErrorMessage = "Lời khuyên không được vượt quá 2000 ký tự")]
        public string? Advice { get; set; }
    }

    // DTO cho cập nhật consultation
    public class ConsultationUpdateDto
    {
        [StringLength(2000, ErrorMessage = "Tóm tắt không được vượt quá 2000 ký tự")]
        public string? Summary { get; set; }

        [StringLength(50, ErrorMessage = "Mã chẩn đoán không được vượt quá 50 ký tự")]
        public string? DiagnosisCode { get; set; }

        [StringLength(2000, ErrorMessage = "Lời khuyên không được vượt quá 2000 ký tự")]
        public string? Advice { get; set; }
    }

    // DTO cho lọc consultation
    public class ConsultationFilterDto
    {
        public int? TenantId { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DiagnosisCode { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // DTO cho báo cáo consultation
    public class ConsultationReportDto
    {
        public int TotalConsultations { get; set; }
        public Dictionary<string, int> DiagnosisCodes { get; set; } = new Dictionary<string, int>();
        public List<ConsultationDto> RecentConsultations { get; set; } = new List<ConsultationDto>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
