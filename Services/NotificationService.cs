using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(SavePlusDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(NotificationCreateDto dto)
        {
            try
            {
                // Kiểm tra tenant có tồn tại không
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == dto.TenantId);
                if (tenant == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy phòng khám");
                }

                // Kiểm tra user (nếu có)
                if (dto.UserId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId.Value);
                    if (user == null)
                    {
                        return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy người dùng");
                    }
                }

                // Kiểm tra patient (nếu có)
                if (dto.PatientId.HasValue)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == dto.PatientId.Value);
                    if (patient == null)
                    {
                        return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy bệnh nhân");
                    }
                }

                // Phải có ít nhất user hoặc patient
                if (!dto.UserId.HasValue && !dto.PatientId.HasValue)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Phải chỉ định người nhận (User hoặc Patient)");
                }

                // Tạo notification mới
                var notification = new Notification
                {
                    TenantId = dto.TenantId,
                    UserId = dto.UserId,
                    PatientId = dto.PatientId,
                    Title = dto.Title,
                    Body = dto.Body,
                    Channel = dto.Channel,
                    SentAt = dto.ScheduledAt ?? DateTime.UtcNow,
                    ReadAt = null
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Load lại notification với thông tin liên quan
                var result = await _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .FirstOrDefaultAsync(n => n.NotificationId == notification.NotificationId);

                var notificationDto = MapToDto(result!);

                _logger.LogInformation("Đã tạo notification mới với ID: {NotificationId}", notification.NotificationId);

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Tạo thông báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo notification");
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi tạo thông báo");
            }
        }

        public async Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy thông báo");
                }

                var notificationDto = MapToDto(notification);

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy notification với ID: {NotificationId}", notificationId);
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi lấy thông tin thông báo");
            }
        }

        public async Task<ApiResponse<NotificationDto>> UpdateNotificationAsync(long notificationId, NotificationUpdateDto dto)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy thông báo");
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(dto.Title))
                    notification.Title = dto.Title;
                
                if (!string.IsNullOrEmpty(dto.Body))
                    notification.Body = dto.Body;
                
                if (!string.IsNullOrEmpty(dto.Channel))
                    notification.Channel = dto.Channel;

                await _context.SaveChangesAsync();

                var notificationDto = MapToDto(notification);

                _logger.LogInformation("Đã cập nhật notification với ID: {NotificationId}", notificationId);

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Cập nhật thông báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật notification với ID: {NotificationId}", notificationId);
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi cập nhật thông báo");
            }
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy thông báo");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa notification với ID: {NotificationId}", notificationId);

                return ApiResponse<bool>.SuccessResult(true, "Xóa thông báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa notification với ID: {NotificationId}", notificationId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa thông báo");
            }
        }

        public async Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(NotificationFilterDto filter)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .AsQueryable();

                // Áp dụng filters
                if (filter.TenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == filter.TenantId.Value);
                }

                if (filter.UserId.HasValue)
                {
                    query = query.Where(n => n.UserId == filter.UserId.Value);
                }

                if (filter.PatientId.HasValue)
                {
                    query = query.Where(n => n.PatientId == filter.PatientId.Value);
                }

                if (!string.IsNullOrEmpty(filter.Channel))
                {
                    query = query.Where(n => n.Channel == filter.Channel);
                }

                if (filter.IsRead.HasValue)
                {
                    if (filter.IsRead.Value)
                        query = query.Where(n => n.ReadAt != null);
                    else
                        query = query.Where(n => n.ReadAt == null);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(n => n.SentAt >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(n => n.SentAt <= filter.ToDate.Value);
                }

                // Đếm tổng số
                var totalCount = await query.CountAsync();

                // Phân trang
                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<NotificationDto>
                {
                    Data = notificationDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResult<NotificationDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách notification");
                return ApiResponse<PagedResult<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách thông báo");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, int? tenantId = null, bool? unreadOnly = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .Where(n => n.UserId == userId);

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                if (unreadOnly.HasValue && unreadOnly.Value)
                {
                    query = query.Where(n => n.ReadAt == null);
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy notification của user: {UserId}", userId);
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi lấy thông báo của người dùng");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetPatientNotificationsAsync(int patientId, int? tenantId = null, bool? unreadOnly = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .Where(n => n.PatientId == patientId);

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                if (unreadOnly.HasValue && unreadOnly.Value)
                {
                    query = query.Where(n => n.ReadAt == null);
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy notification của patient: {PatientId}", patientId);
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi lấy thông báo của bệnh nhân");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetTenantNotificationsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .Where(n => n.TenantId == tenantId);

                if (fromDate.HasValue)
                {
                    query = query.Where(n => n.SentAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(n => n.SentAt <= toDate.Value);
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy notification của tenant: {TenantId}", tenantId);
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi lấy thông báo của phòng khám");
            }
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    return ApiResponse<bool>.ErrorResult("Không tìm thấy thông báo");
                }

                if (notification.ReadAt == null)
                {
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return ApiResponse<bool>.SuccessResult(true, "Đã đánh dấu thông báo là đã đọc");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu notification đã đọc: {NotificationId}", notificationId);
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đánh dấu thông báo đã đọc");
            }
        }

        public async Task<ApiResponse<bool>> MarkMultipleAsReadAsync(MarkAsReadDto dto)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => dto.NotificationIds.Contains(n.NotificationId) && n.ReadAt == null)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã đánh dấu {Count} notification đã đọc", notifications.Count);

                return ApiResponse<bool>.SuccessResult(true, $"Đã đánh dấu {notifications.Count} thông báo là đã đọc");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu nhiều notification đã đọc");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đánh dấu thông báo đã đọc");
            }
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(int? userId = null, int? patientId = null, int? tenantId = null)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.ReadAt == null);

                if (userId.HasValue)
                {
                    query = query.Where(n => n.UserId == userId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(n => n.PatientId == patientId.Value);
                }

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                var notifications = await query.ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã đánh dấu tất cả {Count} notification đã đọc", notifications.Count);

                return ApiResponse<bool>.SuccessResult(true, $"Đã đánh dấu tất cả {notifications.Count} thông báo là đã đọc");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả notification đã đọc");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đánh dấu tất cả thông báo đã đọc");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> SendBulkNotificationAsync(BulkNotificationDto dto)
        {
            try
            {
                var notifications = new List<Notification>();

                // Gửi cho danh sách users
                if (dto.UserIds?.Any() == true)
                {
                    foreach (var userId in dto.UserIds)
                    {
                        notifications.Add(new Notification
                        {
                            TenantId = dto.TenantId,
                            UserId = userId,
                            Title = dto.Title,
                            Body = dto.Body,
                            Channel = dto.Channel,
                            SentAt = dto.ScheduledAt ?? DateTime.UtcNow
                        });
                    }
                }

                // Gửi cho danh sách patients
                if (dto.PatientIds?.Any() == true)
                {
                    foreach (var patientId in dto.PatientIds)
                    {
                        notifications.Add(new Notification
                        {
                            TenantId = dto.TenantId,
                            PatientId = patientId,
                            Title = dto.Title,
                            Body = dto.Body,
                            Channel = dto.Channel,
                            SentAt = dto.ScheduledAt ?? DateTime.UtcNow
                        });
                    }
                }

                // Gửi cho tất cả users của tenant
                if (dto.SendToAllUsers)
                {
                    var users = await _context.Users
                        .Where(u => u.TenantId == dto.TenantId && u.IsActive)
                        .Select(u => u.UserId)
                        .ToListAsync();

                    foreach (var userId in users)
                    {
                        notifications.Add(new Notification
                        {
                            TenantId = dto.TenantId,
                            UserId = userId,
                            Title = dto.Title,
                            Body = dto.Body,
                            Channel = dto.Channel,
                            SentAt = dto.ScheduledAt ?? DateTime.UtcNow
                        });
                    }
                }

                // Gửi cho tất cả patients của tenant
                if (dto.SendToAllPatients)
                {
                    var patients = await _context.ClinicPatients
                        .Where(cp => cp.TenantId == dto.TenantId && cp.Status == 1)
                        .Select(cp => cp.PatientId)
                        .ToListAsync();

                    foreach (var patientId in patients)
                    {
                        notifications.Add(new Notification
                        {
                            TenantId = dto.TenantId,
                            PatientId = patientId,
                            Title = dto.Title,
                            Body = dto.Body,
                            Channel = dto.Channel,
                            SentAt = dto.ScheduledAt ?? DateTime.UtcNow
                        });
                    }
                }

                if (!notifications.Any())
                {
                    return ApiResponse<List<NotificationDto>>.ErrorResult("Không có người nhận nào được chỉ định");
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // Load lại với thông tin liên quan
                var notificationIds = notifications.Select(n => n.NotificationId).ToList();
                var result = await _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .Where(n => notificationIds.Contains(n.NotificationId))
                    .ToListAsync();

                var notificationDtos = result.Select(MapToDto).ToList();

                _logger.LogInformation("Đã gửi {Count} notification hàng loạt", notifications.Count);

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos, $"Đã gửi {notifications.Count} thông báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi notification hàng loạt");
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi gửi thông báo hàng loạt");
            }
        }

        public async Task<ApiResponse<NotificationReportDto>> GetNotificationReportAsync(int? tenantId = null, int? userId = null, int? patientId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                if (userId.HasValue)
                {
                    query = query.Where(n => n.UserId == userId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(n => n.PatientId == patientId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(n => n.SentAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(n => n.SentAt <= toDate.Value);
                }

                var notifications = await query.ToListAsync();

                var totalNotifications = notifications.Count;
                var readNotifications = notifications.Count(n => n.ReadAt.HasValue);
                var unreadNotifications = totalNotifications - readNotifications;

                // Thống kê theo kênh
                var notificationsByChannel = notifications
                    .GroupBy(n => n.Channel)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Thống kê theo ngày
                var notificationsByDay = notifications
                    .GroupBy(n => n.SentAt.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Notification gần đây
                var recentNotifications = notifications
                    .OrderByDescending(n => n.SentAt)
                    .Take(10)
                    .Select(MapToDto)
                    .ToList();

                var report = new NotificationReportDto
                {
                    TotalNotifications = totalNotifications,
                    ReadNotifications = readNotifications,
                    UnreadNotifications = unreadNotifications,
                    NotificationsByChannel = notificationsByChannel,
                    NotificationsByDay = notificationsByDay,
                    RecentNotifications = recentNotifications,
                    FromDate = fromDate ?? DateTime.MinValue,
                    ToDate = toDate ?? DateTime.MaxValue
                };

                return ApiResponse<NotificationReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo notification");
                return ApiResponse<NotificationReportDto>.ErrorResult("Có lỗi xảy ra khi tạo báo cáo thông báo");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountAsync(int? userId = null, int? patientId = null, int? tenantId = null)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.ReadAt == null);

                if (userId.HasValue)
                {
                    query = query.Where(n => n.UserId == userId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(n => n.PatientId == patientId.Value);
                }

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                var count = await query.CountAsync();

                return ApiResponse<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng notification chưa đọc");
                return ApiResponse<int>.ErrorResult("Có lỗi xảy ra khi lấy số lượng thông báo chưa đọc");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> SearchNotificationsAsync(string keyword, int? tenantId = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                // Tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(n =>
                        n.Title.ToLower().Contains(keyword) ||
                        n.Body.ToLower().Contains(keyword) ||
                        (n.User != null && n.User.FullName.ToLower().Contains(keyword)) ||
                        (n.Patient != null && n.Patient.FullName.ToLower().Contains(keyword)));
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm notification với từ khóa: {Keyword}", keyword);
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi tìm kiếm thông báo");
            }
        }

        public async Task<ApiResponse<UserNotificationSummaryDto>> GetUserNotificationSummaryAsync(int? userId = null, int? patientId = null, int? tenantId = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(n => n.UserId == userId.Value);
                }

                if (patientId.HasValue)
                {
                    query = query.Where(n => n.PatientId == patientId.Value);
                }

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                var notifications = await query.ToListAsync();

                string? recipientName = null;
                if (userId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
                    recipientName = user?.FullName;
                }
                else if (patientId.HasValue)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId.Value);
                    recipientName = patient?.FullName;
                }

                var summary = new UserNotificationSummaryDto
                {
                    UserId = userId,
                    PatientId = patientId,
                    RecipientName = recipientName,
                    TotalNotifications = notifications.Count,
                    UnreadNotifications = notifications.Count(n => n.ReadAt == null),
                    LastNotificationDate = notifications.Any() ? notifications.Max(n => n.SentAt) : null,
                    RecentNotifications = notifications
                        .OrderByDescending(n => n.SentAt)
                        .Take(5)
                        .Select(MapToDto)
                        .ToList()
                };

                return ApiResponse<UserNotificationSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê notification của user/patient");
                return ApiResponse<UserNotificationSummaryDto>.ErrorResult("Có lỗi xảy ra khi lấy thống kê thông báo");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendNotificationFromTemplateAsync(NotificationTemplateDto template, int? userId = null, int? patientId = null, int tenantId = 1)
        {
            try
            {
                // Thay thế variables trong title và body
                var title = template.Title;
                var body = template.Body;

                foreach (var variable in template.Variables)
                {
                    title = title.Replace($"{{{variable.Key}}}", variable.Value);
                    body = body.Replace($"{{{variable.Key}}}", variable.Value);
                }

                var createDto = new NotificationCreateDto
                {
                    TenantId = tenantId,
                    UserId = userId,
                    PatientId = patientId,
                    Title = title,
                    Body = body,
                    Channel = template.Channel
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi notification từ template: {Type}", template.Type);
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi gửi thông báo từ template");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsByChannelAsync(string channel, int? tenantId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.Patient)
                    .Include(n => n.Tenant)
                    .Where(n => n.Channel == channel);

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(n => n.SentAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(n => n.SentAt <= toDate.Value);
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return ApiResponse<List<NotificationDto>>.SuccessResult(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy notification theo kênh: {Channel}", channel);
                return ApiResponse<List<NotificationDto>>.ErrorResult("Có lỗi xảy ra khi lấy thông báo theo kênh");
            }
        }

        public async Task<ApiResponse<int>> CleanupOldNotificationsAsync(int daysOld = 30, int? tenantId = null)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var query = _context.Notifications.Where(n => n.SentAt < cutoffDate);

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                var oldNotifications = await query.ToListAsync();
                var count = oldNotifications.Count;

                _context.Notifications.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa {Count} notification cũ (hơn {Days} ngày)", count, daysOld);

                return ApiResponse<int>.SuccessResult(count, $"Đã xóa {count} thông báo cũ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa notification cũ");
                return ApiResponse<int>.ErrorResult("Có lỗi xảy ra khi xóa thông báo cũ");
            }
        }

        public async Task<ApiResponse<List<string>>> GetUsedChannelsAsync(int? tenantId = null)
        {
            try
            {
                var query = _context.Notifications.AsQueryable();

                if (tenantId.HasValue)
                {
                    query = query.Where(n => n.TenantId == tenantId.Value);
                }

                var channels = await query
                    .Select(n => n.Channel)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResult(channels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách kênh notification");
                return ApiResponse<List<string>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách kênh");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendAppointmentReminderAsync(int appointmentId, int hoursBeforeAppointment = 24)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Tenant)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Không tìm thấy cuộc hẹn");
                }

                var template = new NotificationTemplateDto
                {
                    Type = "appointment_reminder",
                    Title = "Nhắc nhở cuộc hẹn",
                    Body = "Bạn có cuộc hẹn với {DoctorName} vào lúc {AppointmentTime} tại {TenantName}. Vui lòng đến đúng giờ.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "DoctorName", appointment.Doctor?.User?.FullName ?? "Bác sĩ" },
                        { "AppointmentTime", appointment.StartAt.ToString("dd/MM/yyyy HH:mm") },
                        { "TenantName", appointment.Tenant?.Name ?? "Phòng khám" }
                    }
                };

                return await SendNotificationFromTemplateAsync(template, null, appointment.PatientId, appointment.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi nhắc nhở cuộc hẹn: {AppointmentId}", appointmentId);
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi gửi nhắc nhở cuộc hẹn");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendTestResultNotificationAsync(int patientId, string testName, string result, int tenantId)
        {
            try
            {
                var template = new NotificationTemplateDto
                {
                    Type = "test_result",
                    Title = "Kết quả xét nghiệm",
                    Body = "Kết quả {TestName} của bạn đã có: {Result}. Vui lòng liên hệ phòng khám để biết thêm chi tiết.",
                    Channel = "Push",
                    Variables = new Dictionary<string, string>
                    {
                        { "TestName", testName },
                        { "Result", result }
                    }
                };

                return await SendNotificationFromTemplateAsync(template, null, patientId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo kết quả xét nghiệm cho patient: {PatientId}", patientId);
                return ApiResponse<NotificationDto>.ErrorResult("Có lỗi xảy ra khi gửi thông báo kết quả xét nghiệm");
            }
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                TenantId = notification.TenantId,
                UserId = notification.UserId,
                PatientId = notification.PatientId,
                Title = notification.Title,
                Body = notification.Body,
                Channel = notification.Channel,
                SentAt = notification.SentAt,
                ReadAt = notification.ReadAt,
                UserName = notification.User?.FullName,
                PatientName = notification.Patient?.FullName,
                TenantName = notification.Tenant?.Name
            };
        }
    }
}
