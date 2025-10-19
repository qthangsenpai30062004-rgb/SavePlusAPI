using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavePlus_API.DTOs;
using SavePlus_API.Services;
using SavePlus_API.Constants;
using System.Security.Claims;

namespace SavePlus_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IAppointmentService _appointmentService;
        private readonly IConsultationService _consultationService;
        private readonly IPatientService _patientService;
        private readonly INotificationService _notificationService;
        private readonly IReminderService _reminderService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ITenantService tenantService,
            IAppointmentService appointmentService,
            IConsultationService consultationService,
            IPatientService patientService,
            INotificationService notificationService,
            IReminderService reminderService,
            ILogger<DashboardController> logger)
        {
            _tenantService = tenantService;
            _appointmentService = appointmentService;
            _consultationService = consultationService;
            _patientService = patientService;
            _notificationService = notificationService;
            _reminderService = reminderService;
            _logger = logger;
        }

        private int GetTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            return int.Parse(tenantIdClaim ?? "0");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /// <summary>
        /// Lấy dữ liệu tổng quan dashboard cho phòng khám
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetDashboardOverview(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Set default date range (last 30 days)
                fromDate ??= DateTime.Today.AddDays(-30);
                toDate ??= DateTime.Today;

                // Get basic stats
                var tenantStats = await _tenantService.GetTenantStatsAsync(tenantId);
                var todayAppointments = await _appointmentService.GetTodayAppointmentsAsync(tenantId);
                var upcomingReminders = await _reminderService.GetUpcomingRemindersAsync(tenantId, 24);
                var overdueReminders = await _reminderService.GetOverdueRemindersAsync(tenantId);
                var unreadNotifications = await _notificationService.GetUnreadCountAsync(null, null, tenantId);

                // Get consultation report
                var consultationReport = await _consultationService.GetConsultationReportAsync(tenantId, null, fromDate, toDate);
                
                // Get notification report
                var notificationReport = await _notificationService.GetNotificationReportAsync(tenantId, null, null, fromDate, toDate);

                var overview = new DashboardOverviewDto
                {
                    // Basic Stats
                    TotalPatients = tenantStats.Success ? tenantStats.Data?.TotalPatients ?? 0 : 0,
                    TotalDoctors = tenantStats.Success ? tenantStats.Data?.TotalDoctors ?? 0 : 0,
                    TotalStaff = tenantStats.Success ? (tenantStats.Data?.TotalDoctors ?? 0) : 0, // Tạm thời dùng TotalDoctors
                    
                    // Today's Activity
                    TodayAppointments = todayAppointments.Success ? todayAppointments.Data?.Count ?? 0 : 0,
                    UpcomingReminders = upcomingReminders.Success ? upcomingReminders.Data?.Count ?? 0 : 0,
                    OverdueReminders = overdueReminders.Success ? overdueReminders.Data?.Count ?? 0 : 0,
                    UnreadNotifications = unreadNotifications.Success ? unreadNotifications.Data : 0,
                    
                    // Period Stats
                    TotalConsultations = consultationReport.Success ? consultationReport.Data?.TotalConsultations ?? 0 : 0,
                    TotalNotifications = notificationReport.Success ? notificationReport.Data?.TotalNotifications ?? 0 : 0,
                    NotificationReadRate = notificationReport.Success ? notificationReport.Data?.ReadRate ?? 0 : 0,
                    
                    // Business KPIs (giống như trong hình)
                    TotalRevenue = 100000000, // 100 triệu VNĐ - cần implement payment system
                    NewPatientsThisMonth = 2000, // +2,000 bệnh nhân mới
                    TotalServiceOrders = 500, // +500 đơn dịch vụ
                    PatientReturnRate = 75.0, // 75% tỷ lệ bệnh nhân quay lại
                    OverduePaymentAmount = 25000000, // 25,000,000đ tiền quá hạn
                    
                    // Charts Data
                    ConsultationsByDay = new Dictionary<string, int>(), // Cần implement trong ConsultationReportDto
                    TopDiagnosis = consultationReport.Success ? 
                        consultationReport.Data?.DiagnosisCodes?.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, int>() 
                        : new Dictionary<string, int>(),
                    NotificationsByChannel = notificationReport.Success ? notificationReport.Data?.NotificationsByChannel ?? new Dictionary<string, int>() : new Dictionary<string, int>(),
                    
                    // Date Range
                    FromDate = fromDate.Value,
                    ToDate = toDate.Value,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(ApiResponse<DashboardOverviewDto>.SuccessResult(overview, "Lấy dữ liệu dashboard thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview for tenant {TenantId}", GetTenantId());
                return StatusCode(500, ApiResponse<DashboardOverviewDto>.ErrorResult("Lỗi server khi lấy dữ liệu dashboard"));
            }
        }

        /// <summary>
        /// Lấy dữ liệu chart cho dashboard
        /// </summary>
        [HttpGet("charts")]
        public async Task<ActionResult<ApiResponse<DashboardChartsDto>>> GetDashboardCharts(
            [FromQuery] string chartType = "all", // appointments, consultations, measurements, notifications
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "day") // day, week, month
        {
            try
            {
                var tenantId = GetTenantId();

                // Set default date range
                fromDate ??= DateTime.Today.AddDays(-30);
                toDate ??= DateTime.Today;

                var charts = new DashboardChartsDto
                {
                    ChartType = chartType,
                    GroupBy = groupBy,
                    FromDate = fromDate.Value,
                    ToDate = toDate.Value
                };

                if (chartType == "all" || chartType == "appointments")
                {
                    var appointmentFilter = new AppointmentFilterDto
                    {
                        TenantId = tenantId,
                        FromDate = fromDate,
                        ToDate = toDate,
                        PageNumber = 1,
                        PageSize = 1000
                    };
                    var appointmentResult = await _appointmentService.GetAppointmentsAsync(appointmentFilter);
                    
                    if (appointmentResult.Success)
                    {
                        charts.AppointmentsByDay = appointmentResult.Data?.Data
                            .GroupBy(a => a.StartAt.Date)
                            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count()) ?? new Dictionary<string, int>();
                            
                        charts.AppointmentsByStatus = appointmentResult.Data?.Data
                            .GroupBy(a => a.Status)
                            .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>();
                    }
                }

                if (chartType == "all" || chartType == "consultations")
                {
                    var consultationReport = await _consultationService.GetConsultationReportAsync(tenantId, null, fromDate, toDate);
                    if (consultationReport.Success)
                    {
                        charts.ConsultationsByDay = new Dictionary<string, int>(); // Cần implement
                        charts.TopDiagnosis = consultationReport.Data?.DiagnosisCodes?
                            .OrderByDescending(x => x.Value)
                            .Take(10)
                            .ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, int>();
                    }
                }

                if (chartType == "all" || chartType == "notifications")
                {
                    var notificationReport = await _notificationService.GetNotificationReportAsync(tenantId, null, null, fromDate, toDate);
                    if (notificationReport.Success)
                    {
                        charts.NotificationsByDay = notificationReport.Data?.NotificationsByDay ?? new Dictionary<string, int>();
                        charts.NotificationsByChannel = notificationReport.Data?.NotificationsByChannel ?? new Dictionary<string, int>();
                    }
                }

                return Ok(ApiResponse<DashboardChartsDto>.SuccessResult(charts, "Lấy dữ liệu charts thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard charts for tenant {TenantId}", GetTenantId());
                return StatusCode(500, ApiResponse<DashboardChartsDto>.ErrorResult("Lỗi server khi lấy dữ liệu charts"));
            }
        }

        /// <summary>
        /// Lấy dữ liệu dashboard dành cho bác sĩ
        /// </summary>
        [HttpGet("doctor")]
        public async Task<ActionResult<ApiResponse<DoctorDashboardDto>>> GetDoctorDashboard(
            [FromQuery] int doctorId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var userRole = GetUserRole();

                // Check permissions
                if (userRole != UserRoles.Doctor && userRole != UserRoles.ClinicAdmin && userRole != UserRoles.SystemAdmin)
                {
                    return Forbid();
                }

                fromDate ??= DateTime.Today.AddDays(-7);
                toDate ??= DateTime.Today;

                var doctorAppointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId, fromDate, toDate);
                var doctorConsultations = await _consultationService.GetDoctorConsultationsAsync(doctorId, fromDate, toDate);
                var todayAppointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId, DateTime.Today, DateTime.Today.AddDays(1));

                var dashboard = new DoctorDashboardDto
                {
                    DoctorId = doctorId,
                    TotalAppointments = doctorAppointments.Success ? doctorAppointments.Data?.Count ?? 0 : 0,
                    TotalConsultations = doctorConsultations.Success ? doctorConsultations.Data?.Count ?? 0 : 0,
                    TodayAppointments = todayAppointments.Success ? todayAppointments.Data?.Count ?? 0 : 0,
                    
                    AppointmentsByDay = doctorAppointments.Success ? 
                        doctorAppointments.Data?.GroupBy(a => a.StartAt.Date)
                            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count()) ?? new Dictionary<string, int>()
                        : new Dictionary<string, int>(),
                        
                    AppointmentsByStatus = doctorAppointments.Success ?
                        doctorAppointments.Data?.GroupBy(a => a.Status)
                            .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>()
                        : new Dictionary<string, int>(),
                    
                    FromDate = fromDate.Value,
                    ToDate = toDate.Value,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(ApiResponse<DoctorDashboardDto>.SuccessResult(dashboard, "Lấy dashboard bác sĩ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor dashboard for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<DoctorDashboardDto>.ErrorResult("Lỗi server khi lấy dashboard bác sĩ"));
            }
        }

        /// <summary>
        /// Lấy các widget cho dashboard (quick stats)
        /// </summary>
        [HttpGet("widgets")]
        public async Task<ActionResult<ApiResponse<List<DashboardWidgetDto>>>> GetDashboardWidgets()
        {
            try
            {
                var tenantId = GetTenantId();
                var widgets = new List<DashboardWidgetDto>();

                // Today's appointments widget
                var todayAppointments = await _appointmentService.GetTodayAppointmentsAsync(tenantId);
                widgets.Add(new DashboardWidgetDto
                {
                    Title = "Lịch hẹn hôm nay",
                    Value = todayAppointments.Success ? todayAppointments.Data?.Count.ToString() ?? "0" : "0",
                    Icon = "calendar",
                    Color = "blue",
                    Trend = "stable"
                });

                // Overdue reminders widget
                var overdueReminders = await _reminderService.GetOverdueRemindersAsync(tenantId);
                widgets.Add(new DashboardWidgetDto
                {
                    Title = "Nhắc nhở quá hạn",
                    Value = overdueReminders.Success ? overdueReminders.Data?.Count.ToString() ?? "0" : "0",
                    Icon = "alert",
                    Color = "red",
                    Trend = "down"
                });

                // Unread notifications widget
                var unreadNotifications = await _notificationService.GetUnreadCountAsync(null, null, tenantId);
                widgets.Add(new DashboardWidgetDto
                {
                    Title = "Thông báo chưa đọc",
                    Value = unreadNotifications.Success ? unreadNotifications.Data.ToString() : "0",
                    Icon = "bell",
                    Color = "orange",
                    Trend = "stable"
                });

                // Upcoming reminders widget
                var upcomingReminders = await _reminderService.GetUpcomingRemindersAsync(tenantId, 24);
                widgets.Add(new DashboardWidgetDto
                {
                    Title = "Nhắc nhở sắp tới",
                    Value = upcomingReminders.Success ? upcomingReminders.Data?.Count.ToString() ?? "0" : "0",
                    Icon = "clock",
                    Color = "green",
                    Trend = "up"
                });

                return Ok(ApiResponse<List<DashboardWidgetDto>>.SuccessResult(widgets, "Lấy dashboard widgets thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard widgets for tenant {TenantId}", GetTenantId());
                return StatusCode(500, ApiResponse<List<DashboardWidgetDto>>.ErrorResult("Lỗi server khi lấy dashboard widgets"));
            }
        }

        /// <summary>
        /// Lấy analytics doanh thu (giống như trong hình dashboard)
        /// </summary>
        [HttpGet("revenue-analytics")]
        public ActionResult<ApiResponse<RevenueAnalyticsDto>> GetRevenueAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var tenantId = GetTenantId();
                
                // Set default date range (last 6 months)
                fromDate ??= DateTime.Today.AddMonths(-6);
                toDate ??= DateTime.Today;

                // Mock data - trong thực tế sẽ lấy từ Payment/Transaction tables
                var revenueAnalytics = new RevenueAnalyticsDto
                {
                    // KPI Cards (giống trong hình)
                    TotalRevenue = 100000000m, // 100 triệu VNĐ
                    RevenueGrowth = 20.5, // +20.5% so với tháng trước
                    NewPatients = 2000, // +2,000 bệnh nhân mới
                    PatientGrowth = 15.3, // +15.3% so với tháng trước
                    TotalServiceOrders = 500, // +500 đơn dịch vụ
                    OrderGrowth = 8.7, // +8.7% so với tháng trước
                    PatientReturnRate = 75.0, // 75% tỷ lệ bệnh nhân quay lại
                    ReturnRateChange = 2.1, // +2.1% so với tháng trước
                    OverduePaymentAmount = 25000000m, // 25,000,000đ tiền quá hạn
                    OverdueChange = -5.2, // -5.2% so với tháng trước (giảm là tốt)

                    // Revenue Trend (Line Chart - Xu hướng Doanh thu)
                    RevenueByMonth = new Dictionary<string, decimal>
                    {
                        { "Tháng 1", 15000000m },
                        { "Tháng 2", 18000000m },
                        { "Tháng 3", 16000000m },
                        { "Tháng 4", 22000000m },
                        { "Tháng 5", 19000000m },
                        { "Tháng 6", 20000000m }
                    },

                    // Top Services (Bar Chart - Top 5 dịch vụ được sử dụng nhiều nhất)
                    TopServices = new Dictionary<string, int>
                    {
                        { "Dịch vụ 1", 150 },
                        { "Dịch vụ 2", 120 },
                        { "Dịch vụ 3", 100 },
                        { "Dịch vụ 4", 80 },
                        { "Dịch vụ 5", 60 }
                    },

                    // Service Revenue Distribution (Donut Chart - Tỷ trọng Doanh thu theo Dịch vụ)
                    ServiceRevenueDistribution = new Dictionary<string, decimal>
                    {
                        { "Dịch vụ 1", 30000000m }, // 30%
                        { "Dịch vụ 2", 25000000m }, // 25%
                        { "Dịch vụ 3", 20000000m }, // 20%
                        { "Dịch vụ 4", 15000000m }, // 15%
                        { "Dịch vụ 5", 10000000m }  // 10%
                    },

                    FromDate = fromDate.Value,
                    ToDate = toDate.Value,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(ApiResponse<RevenueAnalyticsDto>.SuccessResult(revenueAnalytics, "Lấy analytics doanh thu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics for tenant {TenantId}", GetTenantId());
                return StatusCode(500, ApiResponse<RevenueAnalyticsDto>.ErrorResult("Lỗi server khi lấy analytics doanh thu"));
            }
        }

        /// <summary>
        /// Lấy danh sách đơn đặt dịch vụ gần đây (bảng trong dashboard)
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<ActionResult<ApiResponse<List<RecentOrderDto>>>> GetRecentOrders(
            [FromQuery] int limit = 10)
        {
            try
            {
                var tenantId = GetTenantId();

                // Lấy appointments gần đây làm "đơn đặt dịch vụ"
                var appointmentFilter = new AppointmentFilterDto
                {
                    TenantId = tenantId,
                    PageNumber = 1,
                    PageSize = limit
                };
                
                var appointmentResult = await _appointmentService.GetAppointmentsAsync(appointmentFilter);
                
                var recentOrders = new List<RecentOrderDto>();
                
                if (appointmentResult.Success && appointmentResult.Data?.Data != null)
                {
                    recentOrders = appointmentResult.Data.Data.Select(a => new RecentOrderDto
                    {
                        CustomerName = a.PatientName ?? "N/A",
                        ServiceType = a.Type == "Clinic" ? "Tư vấn" : 
                                     a.Type == "Home" ? "Xét nghiệm" : "Khám tại nhà",
                        Status = a.Status == "Completed" ? "Sắp diễn ra" : 
                                a.Status == "Confirmed" ? "Sắp diễn ra" : "Đã hoàn thành",
                        AppointmentTime = a.StartAt,
                        OrderId = a.AppointmentId.ToString()
                    }).ToList();
                }

                return Ok(ApiResponse<List<RecentOrderDto>>.SuccessResult(recentOrders, "Lấy danh sách đơn đặt dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders for tenant {TenantId}", GetTenantId());
                return StatusCode(500, ApiResponse<List<RecentOrderDto>>.ErrorResult("Lỗi server khi lấy danh sách đơn đặt dịch vụ"));
            }
        }
    }
}
