using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    public class MeasurementCreateDto
    {
        [Required(ErrorMessage = "ID bệnh nhân là bắt buộc")]
        public int PatientId { get; set; }

        public int? CarePlanId { get; set; }

        [Required(ErrorMessage = "Loại đo lường là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại đo lường không được vượt quá 50 ký tự")]
        public string Type { get; set; } = null!; // BloodPressure, HeartRate, Weight, Temperature, BloodSugar, etc.

        public decimal? Value1 { get; set; } // Giá trị chính (VD: systolic cho huyết áp)

        public decimal? Value2 { get; set; } // Giá trị phụ (VD: diastolic cho huyết áp)

        [StringLength(20, ErrorMessage = "Đơn vị không được vượt quá 20 ký tự")]
        public string? Unit { get; set; } // mmHg, kg, °C, mg/dL, etc.

        [Required(ErrorMessage = "Nguồn đo lường là bắt buộc")]
        [StringLength(20, ErrorMessage = "Nguồn đo lường không được vượt quá 20 ký tự")]
        public string Source { get; set; } = "App"; // App, Clinic, Hospital, Device

        public DateTime? MeasuredAt { get; set; } // Nếu null sẽ dùng thời gian hiện tại

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    public class MeasurementUpdateDto
    {
        [StringLength(50, ErrorMessage = "Loại đo lường không được vượt quá 50 ký tự")]
        public string? Type { get; set; }

        public decimal? Value1 { get; set; }

        public decimal? Value2 { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị không được vượt quá 20 ký tự")]
        public string? Unit { get; set; }

        [StringLength(20, ErrorMessage = "Nguồn đo lường không được vượt quá 20 ký tự")]
        public string? Source { get; set; }

        public DateTime? MeasuredAt { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    public class MeasurementDto
    {
        public long MeasurementId { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public int? CarePlanId { get; set; }
        public string Type { get; set; } = null!;
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public string? Unit { get; set; }
        public string Source { get; set; } = null!;
        public DateTime MeasuredAt { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public string PatientName { get; set; } = null!;
        public string? CarePlanName { get; set; }

        // Computed properties
        public string DisplayValue => GetDisplayValue();
        public string? PreviousValue { get; set; } // So sánh với lần đo trước đó
        public string? Trend { get; set; } // "Up", "Down", "Stable"

        private string GetDisplayValue()
        {
            if (Value1.HasValue && Value2.HasValue)
            {
                return $"{Value1}/{Value2} {Unit}"; // VD: "120/80 mmHg"
            }
            else if (Value1.HasValue)
            {
                return $"{Value1} {Unit}"; // VD: "70 kg"
            }
            return "N/A";
        }
    }

    // DTO cho thống kê measurement
    public class MeasurementStatsDto
    {
        public string Type { get; set; } = null!;
        public string? Unit { get; set; }
        public int TotalCount { get; set; }
        public DateTime? FirstMeasurement { get; set; }
        public DateTime? LastMeasurement { get; set; }
        
        // Thống kê cho Value1
        public decimal? LatestValue1 { get; set; }
        public decimal? AverageValue1 { get; set; }
        public decimal? MinValue1 { get; set; }
        public decimal? MaxValue1 { get; set; }
        
        // Thống kê cho Value2 (nếu có)
        public decimal? LatestValue2 { get; set; }
        public decimal? AverageValue2 { get; set; }
        public decimal? MinValue2 { get; set; }
        public decimal? MaxValue2 { get; set; }
        
        public string Trend { get; set; } = "Stable"; // "Improving", "Worsening", "Stable"
        public List<MeasurementTrendDataDto> RecentData { get; set; } = new List<MeasurementTrendDataDto>();
    }

    public class MeasurementTrendDataDto
    {
        public DateTime MeasuredAt { get; set; }
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public string DisplayValue { get; set; } = null!;
    }

    // DTO cho query parameters
    public class MeasurementQueryDto
    {
        public int? PatientId { get; set; }
        public int? CarePlanId { get; set; }
        public string? Type { get; set; }
        public string? Source { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "MeasuredAt"; // MeasuredAt, Type, Value1
        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    // Predefined measurement types
    public static class MeasurementTypes
    {
        public const string BloodPressure = "BloodPressure";
        public const string HeartRate = "HeartRate";
        public const string Weight = "Weight";
        public const string Height = "Height";
        public const string Temperature = "Temperature";
        public const string BloodSugar = "BloodSugar";
        public const string Oxygen = "Oxygen";
        public const string BMI = "BMI";
        public const string Pain = "Pain";
        public const string Sleep = "Sleep";
        public const string Steps = "Steps";
        public const string Medication = "Medication";

        public static readonly List<string> AllTypes = new List<string>
        {
            BloodPressure, HeartRate, Weight, Height, Temperature, 
            BloodSugar, Oxygen, BMI, Pain, Sleep, Steps, Medication
        };

        public static readonly Dictionary<string, string> TypeUnits = new Dictionary<string, string>
        {
            { BloodPressure, "mmHg" },
            { HeartRate, "bpm" },
            { Weight, "kg" },
            { Height, "cm" },
            { Temperature, "°C" },
            { BloodSugar, "mg/dL" },
            { Oxygen, "%" },
            { BMI, "kg/m²" },
            { Pain, "scale 1-10" },
            { Sleep, "hours" },
            { Steps, "steps" },
            { Medication, "dose" }
        };

        public static readonly Dictionary<string, bool> RequiresTwoValues = new Dictionary<string, bool>
        {
            { BloodPressure, true }, // Systolic/Diastolic
            { HeartRate, false },
            { Weight, false },
            { Height, false },
            { Temperature, false },
            { BloodSugar, false },
            { Oxygen, false },
            { BMI, false },
            { Pain, false },
            { Sleep, false },
            { Steps, false },
            { Medication, false }
        };
    }

    // Predefined measurement sources
    public static class MeasurementSources
    {
        public const string App = "App";
        public const string Clinic = "Clinic";
        public const string Hospital = "Hospital";
        public const string Device = "Device";
        public const string Manual = "Manual";

        public static readonly List<string> AllSources = new List<string>
        {
            App, Clinic, Hospital, Device, Manual
        };
    }
}
