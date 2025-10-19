using System.ComponentModel.DataAnnotations;
using SavePlus_API.Constants;
using SavePlus_API.Attributes;

namespace SavePlus_API.DTOs
{
    public class PrescriptionCreateDto
    {
        [Required(ErrorMessage = "ID bệnh nhân là bắt buộc")]
        public int PatientId { get; set; }

        public int? CarePlanId { get; set; } // Có thể link với CarePlan

        [Required(ErrorMessage = "ID bác sĩ là bắt buộc")]
        public int DoctorId { get; set; }

        public DateTime? IssuedAt { get; set; } // Nếu null sẽ dùng thời gian hiện tại

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        [Required(ErrorMessage = "Danh sách thuốc là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 loại thuốc")]
        public List<PrescriptionItemCreateDto> Items { get; set; } = new List<PrescriptionItemCreateDto>();
    }

    public class PrescriptionUpdateDto
    {
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string? Status { get; set; }

        public DateTime? IssuedAt { get; set; }
    }

    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public int? CarePlanId { get; set; }
        public int DoctorId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Status { get; set; } = null!;

        // Navigation properties
        public string PatientName { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public string? CarePlanName { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();

        // Computed properties
        public int TotalItems { get; set; }
        public bool HasActiveItems { get; set; }
        public DateTime? NextDue { get; set; } // Lần uống thuốc tiếp theo
    }

    public class PrescriptionItemCreateDto
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên thuốc không được vượt quá 200 ký tự")]
        public string DrugName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Dạng thuốc không được vượt quá 50 ký tự")]
        public string? Form { get; set; } // Tablet, Capsule, Syrup, Injection, etc.

        [StringLength(50, ErrorMessage = "Nồng độ không được vượt quá 50 ký tự")]
        public string? Strength { get; set; } // 500mg, 10ml, etc.

        [Required(ErrorMessage = "Liều dùng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Liều dùng không được vượt quá 100 ký tự")]
        public string Dose { get; set; } = null!; // 1 tablet, 2 capsules, 5ml, etc.

        [StringLength(50, ErrorMessage = "Đường dùng không được vượt quá 50 ký tự")]
        public string? Route { get; set; } // Oral, IV, IM, Topical, etc.

        [Required(ErrorMessage = "Tần suất dùng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tần suất dùng không được vượt quá 100 ký tự")]
        public string Frequency { get; set; } = null!; // TID (3 times daily), BID (2 times daily), etc.

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Hướng dẫn sử dụng không được vượt quá 500 ký tự")]
        public string? Instructions { get; set; } // Before meals, with food, etc.
    }

    public class PrescriptionItemUpdateDto
    {
        [StringLength(200, ErrorMessage = "Tên thuốc không được vượt quá 200 ký tự")]
        public string? DrugName { get; set; }

        [StringLength(50, ErrorMessage = "Dạng thuốc không được vượt quá 50 ký tự")]
        public string? Form { get; set; }

        [StringLength(50, ErrorMessage = "Nồng độ không được vượt quá 50 ký tự")]
        public string? Strength { get; set; }

        [StringLength(100, ErrorMessage = "Liều dùng không được vượt quá 100 ký tự")]
        public string? Dose { get; set; }

        [StringLength(50, ErrorMessage = "Đường dùng không được vượt quá 50 ký tự")]
        public string? Route { get; set; }

        [StringLength(100, ErrorMessage = "Tần suất dùng không được vượt quá 100 ký tự")]
        public string? Frequency { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Hướng dẫn sử dụng không được vượt quá 500 ký tự")]
        public string? Instructions { get; set; }
    }

    public class PrescriptionItemDto
    {
        public int ItemId { get; set; }
        public int PrescriptionId { get; set; }
        public string DrugName { get; set; } = null!;
        public string? Form { get; set; }
        public string? Strength { get; set; }
        public string? Dose { get; set; }
        public string? Route { get; set; }
        public string? Frequency { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Instructions { get; set; }

        // Computed properties
        public bool IsActive { get; set; }
        public int DaysDuration { get; set; }
        public string DisplayText => GetDisplayText();

        private string GetDisplayText()
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(DrugName))
                parts.Add(DrugName);
                
            if (!string.IsNullOrEmpty(Strength))
                parts.Add(Strength);
                
            if (!string.IsNullOrEmpty(Form))
                parts.Add($"({Form})");
                
            var drugInfo = string.Join(" ", parts);
            
            if (!string.IsNullOrEmpty(Dose) && !string.IsNullOrEmpty(Frequency))
                return $"{drugInfo} - {Dose} {Frequency}";
                
            return drugInfo;
        }
    }

    // DTO cho query parameters
    public class PrescriptionQueryDto
    {
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int? CarePlanId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DrugName { get; set; } // Tìm kiếm theo tên thuốc
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "IssuedAt"; // IssuedAt, PatientName, DoctorName
        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    // Predefined prescription statuses
    public static class PrescriptionStatuses
    {
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";

        public static readonly List<string> AllStatuses = new List<string>
        {
            Active, Completed, Cancelled, Expired
        };
    }

    // Predefined drug forms
    public static class DrugForms
    {
        public const string Tablet = "Tablet";
        public const string Capsule = "Capsule";
        public const string Syrup = "Syrup";
        public const string Injection = "Injection";
        public const string Cream = "Cream";
        public const string Ointment = "Ointment";
        public const string Drops = "Drops";
        public const string Spray = "Spray";
        public const string Patch = "Patch";
        public const string Powder = "Powder";

        public static readonly List<string> AllForms = new List<string>
        {
            Tablet, Capsule, Syrup, Injection, Cream, 
            Ointment, Drops, Spray, Patch, Powder
        };
    }

    // Predefined routes
    public static class DrugRoutes
    {
        public const string Oral = "Oral";
        public const string IV = "IV";
        public const string IM = "IM";
        public const string SC = "SC";
        public const string Topical = "Topical";
        public const string Rectal = "Rectal";
        public const string Inhalation = "Inhalation";
        public const string Sublingual = "Sublingual";

        public static readonly List<string> AllRoutes = new List<string>
        {
            Oral, IV, IM, SC, Topical, Rectal, Inhalation, Sublingual
        };
    }

    // Predefined frequencies
    public static class DrugFrequencies
    {
        public const string QD = "QD"; // Once daily
        public const string BID = "BID"; // Twice daily
        public const string TID = "TID"; // Three times daily
        public const string QID = "QID"; // Four times daily
        public const string Q4H = "Q4H"; // Every 4 hours
        public const string Q6H = "Q6H"; // Every 6 hours
        public const string Q8H = "Q8H"; // Every 8 hours
        public const string Q12H = "Q12H"; // Every 12 hours
        public const string PRN = "PRN"; // As needed
        public const string STAT = "STAT"; // Immediately

        public static readonly Dictionary<string, string> FrequencyDescriptions = new Dictionary<string, string>
        {
            { QD, "Một lần mỗi ngày" },
            { BID, "Hai lần mỗi ngày" },
            { TID, "Ba lần mỗi ngày" },
            { QID, "Bốn lần mỗi ngày" },
            { Q4H, "Mỗi 4 giờ" },
            { Q6H, "Mỗi 6 giờ" },
            { Q8H, "Mỗi 8 giờ" },
            { Q12H, "Mỗi 12 giờ" },
            { PRN, "Khi cần thiết" },
            { STAT, "Ngay lập tức" }
        };
    }
}
