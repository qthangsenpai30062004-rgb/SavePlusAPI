using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.DTOs
{
    /// <summary>
    /// DTO cho dashboard tổng quan
    /// </summary>
    public class DashboardOverviewDto
    {
        // Basic Stats
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        
        // Today's Activity
        public int TodayAppointments { get; set; }
        public int UpcomingReminders { get; set; }
        public int OverdueReminders { get; set; }
        public int UnreadNotifications { get; set; }
        
        // Period Stats
        public int TotalConsultations { get; set; }
        public int TotalNotifications { get; set; }
        public double NotificationReadRate { get; set; }
        
        // Business KPIs (giống dashboard trong hình)
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public int NewPatientsThisMonth { get; set; } // Bệnh nhân mới trong tháng
        public int TotalServiceOrders { get; set; } // Tổng đơn dịch vụ
        public double PatientReturnRate { get; set; } // Tỷ lệ bệnh nhân quay lại
        public decimal OverduePaymentAmount { get; set; } // Tổng tiền đơn quá hạn
        
        // Revenue Trends (cho line chart)
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
        public Dictionary<string, decimal> RevenueByDay { get; set; } = new();
        
        // Service Usage (cho bar chart)
        public Dictionary<string, int> TopServices { get; set; } = new();
        public Dictionary<string, decimal> ServiceRevenue { get; set; } = new();
        
        // Charts Data
        public Dictionary<string, int> ConsultationsByDay { get; set; } = new();
        public Dictionary<string, int> TopDiagnosis { get; set; } = new();
        public Dictionary<string, int> NotificationsByChannel { get; set; } = new();
        
        // Date Range
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho dashboard charts
    /// </summary>
    public class DashboardChartsDto
    {
        public string ChartType { get; set; } = "";
        public string GroupBy { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        // Appointment Charts
        public Dictionary<string, int> AppointmentsByDay { get; set; } = new();
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public Dictionary<string, int> AppointmentsByType { get; set; } = new();
        
        // Consultation Charts
        public Dictionary<string, int> ConsultationsByDay { get; set; } = new();
        public Dictionary<string, int> TopDiagnosis { get; set; } = new();
        
        // Measurement Charts
        public Dictionary<string, int> MeasurementsByType { get; set; } = new();
        public Dictionary<string, double> MeasurementTrends { get; set; } = new();
        
        // Notification Charts
        public Dictionary<string, int> NotificationsByDay { get; set; } = new();
        public Dictionary<string, int> NotificationsByChannel { get; set; } = new();
        
        // Care Plan Charts
        public Dictionary<string, int> CarePlansByStatus { get; set; } = new();
        public Dictionary<string, double> CarePlanProgress { get; set; } = new();
    }

    /// <summary>
    /// DTO cho dashboard bác sĩ
    /// </summary>
    public class DoctorDashboardDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = "";
        
        // Stats
        public int TotalAppointments { get; set; }
        public int TotalConsultations { get; set; }
        public int TodayAppointments { get; set; }
        public int TotalPrescriptions { get; set; }
        public int ActiveCarePlans { get; set; }
        
        // Charts
        public Dictionary<string, int> AppointmentsByDay { get; set; } = new();
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public Dictionary<string, int> ConsultationsByDay { get; set; } = new();
        public Dictionary<string, int> TopDiagnosis { get; set; } = new();
        
        // Date Range
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho dashboard patient
    /// </summary>
    public class PatientDashboardDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = "";
        
        // Stats
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int ActivePrescriptions { get; set; }
        public int ActiveCarePlans { get; set; }
        public int UnreadNotifications { get; set; }
        public int PendingReminders { get; set; }
        
        // Recent Activity
        public DateTime? LastAppointment { get; set; }
        public DateTime? NextAppointment { get; set; }
        public List<RecentMeasurementDto> RecentMeasurements { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();
        
        // Charts
        public Dictionary<string, double> MeasurementTrends { get; set; } = new();
        public Dictionary<string, int> AppointmentsByMonth { get; set; } = new();
        
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho dashboard widget
    /// </summary>
    public class DashboardWidgetDto
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public string Trend { get; set; } = ""; // up, down, stable
        public string? Description { get; set; }
        public string? Link { get; set; }
    }

    /// <summary>
    /// DTO cho recent measurement trong patient dashboard
    /// </summary>
    public class RecentMeasurementDto
    {
        public string Type { get; set; } = "";
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public string Unit { get; set; } = "";
        public DateTime MeasuredAt { get; set; }
        public string Status { get; set; } = ""; // normal, high, low
    }

    /// <summary>
    /// DTO cho system dashboard (SystemAdmin)
    /// </summary>
    public class SystemDashboardDto
    {
        // Global Stats
        public int TotalTenants { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        public int ActiveTenants { get; set; }
        
        // Activity Stats
        public int TodayAppointments { get; set; }
        public int TodayConsultations { get; set; }
        public int TodayRegistrations { get; set; }
        
        // Performance
        public Dictionary<string, int> TenantsByRegion { get; set; } = new();
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public Dictionary<string, int> AppointmentsByTenant { get; set; } = new();
        
        // Growth Charts
        public Dictionary<string, int> TenantGrowth { get; set; } = new();
        public Dictionary<string, int> PatientGrowth { get; set; } = new();
        public Dictionary<string, int> AppointmentGrowth { get; set; } = new();
        
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho analytics dashboard
    /// </summary>
    public class AnalyticsDashboardDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        // KPIs
        public double PatientSatisfactionScore { get; set; }
        public double AppointmentNoShowRate { get; set; }
        public double AverageWaitTime { get; set; }
        public double CarePlanCompletionRate { get; set; }
        
        // Trends
        public Dictionary<string, double> PatientRetentionRate { get; set; } = new();
        public Dictionary<string, double> AppointmentUtilization { get; set; } = new();
        public Dictionary<string, double> MedicationAdherence { get; set; } = new();
        
        // Comparisons
        public Dictionary<string, object> PeriodComparison { get; set; } = new();
        public Dictionary<string, object> BenchmarkComparison { get; set; } = new();
        
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho revenue analytics (giống dashboard trong hình)
    /// </summary>
    public class RevenueAnalyticsDto
    {
        // KPI Cards
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public double RevenueGrowth { get; set; } // % tăng trưởng doanh thu
        
        public int NewPatients { get; set; } // Bệnh nhân mới
        public double PatientGrowth { get; set; } // % tăng trưởng bệnh nhân
        
        public int TotalServiceOrders { get; set; } // Tổng đơn dịch vụ
        public double OrderGrowth { get; set; } // % tăng trưởng đơn hàng
        
        public double PatientReturnRate { get; set; } // Tỷ lệ bệnh nhân quay lại
        public double ReturnRateChange { get; set; } // Thay đổi tỷ lệ quay lại
        
        public decimal OverduePaymentAmount { get; set; } // Tổng tiền đơn quá hạn
        public double OverdueChange { get; set; } // Thay đổi tiền quá hạn
        
        // Charts Data
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new(); // Line chart
        public Dictionary<string, int> TopServices { get; set; } = new(); // Bar chart
        public Dictionary<string, decimal> ServiceRevenueDistribution { get; set; } = new(); // Donut chart
        
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO cho đơn đặt dịch vụ gần đây (bảng trong dashboard)
    /// </summary>
    public class RecentOrderDto
    {
        public string OrderId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string ServiceType { get; set; } = ""; // Loại dịch vụ
        public string Status { get; set; } = ""; // Trạng thái
        public DateTime AppointmentTime { get; set; } // Thời gian
    }
}
