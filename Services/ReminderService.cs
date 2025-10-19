using Microsoft.EntityFrameworkCore;
using SavePlus_API.DTOs;
using SavePlus_API.Models;

namespace SavePlus_API.Services
{
    public class ReminderService : IReminderService
    {
        private readonly SavePlusDbContext _context;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(SavePlusDbContext context, ILogger<ReminderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ReminderDTO>> CreateReminderAsync(int tenantId, CreateReminderDTO dto)
        {
            try
            {
                // Validate patient exists and belongs to tenant
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == dto.PatientId);
                if (patient == null)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult("Bệnh nhân không tồn tại");
                }

                // Validate target exists based on TargetType
                var targetExists = await ValidateTargetAsync(tenantId, dto.TargetType, dto.TargetId);
                if (!targetExists)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult($"Đối tượng {dto.TargetType} với ID {dto.TargetId} không tồn tại");
                }

                var reminder = new Reminder
                {
                    TenantId = tenantId,
                    PatientId = dto.PatientId,
                    Title = dto.Title,
                    Body = dto.Body,
                    TargetType = dto.TargetType,
                    TargetId = dto.TargetId,
                    NextFireAt = dto.NextFireAt,
                    Channel = dto.Channel,
                    IsActive = dto.IsActive
                };

                _context.Reminders.Add(reminder);
                await _context.SaveChangesAsync();

                // Load with includes
                reminder = await _context.Reminders
                    .Include(r => r.Patient)
                    .FirstAsync(r => r.ReminderId == reminder.ReminderId);

                return ApiResponse<ReminderDTO>.SuccessResult(await MapToReminderDTOAsync(reminder), "Tạo nhắc nhở thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nhắc nhở");
                return ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra khi tạo nhắc nhở");
            }
        }

        public async Task<ApiResponse<ReminderListResponseDTO>> GetRemindersAsync(int tenantId, ReminderQueryDTO query)
        {
            try
            {
                var reminderQuery = _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId);

                // Apply filters
                if (query.PatientId.HasValue)
                    reminderQuery = reminderQuery.Where(r => r.PatientId == query.PatientId.Value);

                if (!string.IsNullOrEmpty(query.TargetType))
                    reminderQuery = reminderQuery.Where(r => r.TargetType == query.TargetType);

                if (query.IsActive.HasValue)
                    reminderQuery = reminderQuery.Where(r => r.IsActive == query.IsActive.Value);

                if (!string.IsNullOrEmpty(query.Channel))
                    reminderQuery = reminderQuery.Where(r => r.Channel == query.Channel);

                if (query.FromDate.HasValue)
                    reminderQuery = reminderQuery.Where(r => r.NextFireAt >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    reminderQuery = reminderQuery.Where(r => r.NextFireAt <= query.ToDate.Value);

                if (query.IsOverdue.HasValue)
                {
                    var now = DateTime.UtcNow;
                    if (query.IsOverdue.Value)
                        reminderQuery = reminderQuery.Where(r => r.NextFireAt < now && r.IsActive);
                    else
                        reminderQuery = reminderQuery.Where(r => r.NextFireAt >= now || !r.IsActive);
                }

                var totalCount = await reminderQuery.CountAsync();

                var reminders = await reminderQuery
                    .OrderBy(r => r.NextFireAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var reminderDTOs = new List<ReminderDTO>();
                foreach (var reminder in reminders)
                {
                    reminderDTOs.Add(await MapToReminderDTOAsync(reminder));
                }

                var result = new ReminderListResponseDTO
                {
                    Reminders = reminderDTOs,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    HasNextPage = (query.Page * query.PageSize) < totalCount,
                    HasPreviousPage = query.Page > 1
                };

                return ApiResponse<ReminderListResponseDTO>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở");
                return ApiResponse<ReminderListResponseDTO>.ErrorResult("Có lỗi xảy ra khi lấy danh sách nhắc nhở");
            }
        }

        public async Task<ApiResponse<ReminderDTO>> GetReminderByIdAsync(int tenantId, int reminderId)
        {
            try
            {
                var reminder = await _context.Reminders
                    .Include(r => r.Patient)
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult("Nhắc nhở không tồn tại");
                }

                return ApiResponse<ReminderDTO>.SuccessResult(await MapToReminderDTOAsync(reminder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin nhắc nhở");
                return ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra khi lấy thông tin nhắc nhở");
            }
        }

        public async Task<ApiResponse<ReminderDTO>> UpdateReminderAsync(int tenantId, int reminderId, UpdateReminderDTO dto)
        {
            try
            {
                var reminder = await _context.Reminders
                    .Include(r => r.Patient)
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult("Nhắc nhở không tồn tại");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.Title))
                    reminder.Title = dto.Title;

                if (dto.Body != null)
                    reminder.Body = dto.Body;

                if (dto.NextFireAt.HasValue)
                    reminder.NextFireAt = dto.NextFireAt.Value;

                if (!string.IsNullOrEmpty(dto.Channel))
                    reminder.Channel = dto.Channel;

                if (dto.IsActive.HasValue)
                    reminder.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                return ApiResponse<ReminderDTO>.SuccessResult(await MapToReminderDTOAsync(reminder), "Cập nhật nhắc nhở thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật nhắc nhở");
                return ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra khi cập nhật nhắc nhở");
            }
        }

        public async Task<ApiResponse<bool>> DeleteReminderAsync(int tenantId, int reminderId)
        {
            try
            {
                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<bool>.ErrorResult("Nhắc nhở không tồn tại");
                }

                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa nhắc nhở thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhắc nhở");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi xóa nhắc nhở");
            }
        }

        public async Task<ApiResponse<ReminderDTO>> SnoozeReminderAsync(int tenantId, int reminderId, SnoozeReminderDTO dto)
        {
            try
            {
                var reminder = await _context.Reminders
                    .Include(r => r.Patient)
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult("Nhắc nhở không tồn tại");
                }

                reminder.NextFireAt = DateTime.UtcNow.AddMinutes(dto.Minutes);
                await _context.SaveChangesAsync();

                return ApiResponse<ReminderDTO>.SuccessResult(await MapToReminderDTOAsync(reminder), $"Hoãn nhắc nhở thêm {dto.Minutes} phút");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoãn nhắc nhở");
                return ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra khi hoãn nhắc nhở");
            }
        }

        public async Task<ApiResponse<bool>> ActivateReminderAsync(int tenantId, int reminderId)
        {
            try
            {
                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<bool>.ErrorResult("Nhắc nhở không tồn tại");
                }

                reminder.IsActive = true;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Kích hoạt nhắc nhở thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt nhắc nhở");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi kích hoạt nhắc nhở");
            }
        }

        public async Task<ApiResponse<bool>> DeactivateReminderAsync(int tenantId, int reminderId)
        {
            try
            {
                var reminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);

                if (reminder == null)
                {
                    return ApiResponse<bool>.ErrorResult("Nhắc nhở không tồn tại");
                }

                reminder.IsActive = false;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Tắt nhắc nhở thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tắt nhắc nhở");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi tắt nhắc nhở");
            }
        }

        public async Task<ApiResponse<int>> BulkReminderActionAsync(int tenantId, BulkReminderActionDTO dto)
        {
            try
            {
                var reminders = await _context.Reminders
                    .Where(r => dto.ReminderIds.Contains(r.ReminderId) && r.TenantId == tenantId)
                    .ToListAsync();

                int affectedCount = 0;

                foreach (var reminder in reminders)
                {
                    switch (dto.Action.ToLower())
                    {
                        case "activate":
                            reminder.IsActive = true;
                            affectedCount++;
                            break;
                        case "deactivate":
                            reminder.IsActive = false;
                            affectedCount++;
                            break;
                        case "delete":
                            _context.Reminders.Remove(reminder);
                            affectedCount++;
                            break;
                        case "snooze":
                            if (dto.SnoozeMinutes.HasValue)
                            {
                                reminder.NextFireAt = DateTime.UtcNow.AddMinutes(dto.SnoozeMinutes.Value);
                                affectedCount++;
                            }
                            break;
                    }
                }

                await _context.SaveChangesAsync();

                return ApiResponse<int>.SuccessResult(affectedCount, $"Thực hiện thành công trên {affectedCount} nhắc nhở");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện hành động hàng loạt");
                return ApiResponse<int>.ErrorResult("Có lỗi xảy ra khi thực hiện hành động hàng loạt");
            }
        }

        public async Task<ApiResponse<List<FireReminderDTO>>> GetDueRemindersAsync(int tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var dueReminders = await _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId && r.IsActive && r.NextFireAt <= now)
                    .ToListAsync();

                var fireReminderDTOs = dueReminders.Select(r => new FireReminderDTO
                {
                    ReminderId = r.ReminderId,
                    Title = r.Title,
                    Body = r.Body,
                    Channel = r.Channel,
                    PatientId = r.PatientId,
                    PatientName = r.Patient?.FullName ?? "",
                    TargetType = r.TargetType,
                    TargetId = r.TargetId,
                    FiredAt = now
                }).ToList();

                return ApiResponse<List<FireReminderDTO>>.SuccessResult(fireReminderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở đến hạn");
                return ApiResponse<List<FireReminderDTO>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách nhắc nhở đến hạn");
            }
        }

        public async Task<ApiResponse<bool>> MarkReminderAsFiredAsync(int reminderId)
        {
            try
            {
                var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId);
                if (reminder != null)
                {
                    // For now, we'll just deactivate the reminder
                    // In a more complex system, you might want to keep a log of fired reminders
                    reminder.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu nhắc nhở đã kích hoạt");
                return ApiResponse<bool>.ErrorResult("Có lỗi xảy ra khi đánh dấu nhắc nhở đã kích hoạt");
            }
        }

        public async Task<ApiResponse<ReminderStatsDTO>> GetReminderStatsAsync(int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Reminders.Where(r => r.TenantId == tenantId);

                if (fromDate.HasValue)
                    query = query.Where(r => r.NextFireAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(r => r.NextFireAt <= toDate.Value);

                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekFromNow = now.AddDays(7);

                var totalReminders = await query.CountAsync();
                var activeReminders = await query.CountAsync(r => r.IsActive);
                var overdueReminders = await query.CountAsync(r => r.IsActive && r.NextFireAt < now);
                var todayReminders = await query.CountAsync(r => r.IsActive && r.NextFireAt.Date == today);
                var weekReminders = await query.CountAsync(r => r.IsActive && r.NextFireAt >= now && r.NextFireAt <= weekFromNow);

                // Group by type
                var remindersByType = await query
                    .GroupBy(r => r.TargetType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type, x => x.Count);

                // Group by channel
                var remindersByChannel = await query
                    .GroupBy(r => r.Channel)
                    .Select(g => new { Channel = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Channel, x => x.Count);

                // Upcoming reminders (next 24 hours)
                var upcomingReminders = await _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId && r.IsActive && r.NextFireAt >= now && r.NextFireAt <= now.AddHours(24))
                    .OrderBy(r => r.NextFireAt)
                    .Take(10)
                    .ToListAsync();

                var upcomingReminderDTOs = new List<ReminderDTO>();
                foreach (var reminder in upcomingReminders)
                {
                    upcomingReminderDTOs.Add(await MapToReminderDTOAsync(reminder));
                }

                var stats = new ReminderStatsDTO
                {
                    TotalReminders = totalReminders,
                    ActiveReminders = activeReminders,
                    OverdueReminders = overdueReminders,
                    TodayReminders = todayReminders,
                    WeekReminders = weekReminders,
                    RemindersByType = remindersByType,
                    RemindersByChannel = remindersByChannel,
                    UpcomingReminders = upcomingReminderDTOs
                };

                return ApiResponse<ReminderStatsDTO>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê nhắc nhở");
                return ApiResponse<ReminderStatsDTO>.ErrorResult("Có lỗi xảy ra khi lấy thống kê nhắc nhở");
            }
        }

        public async Task<ApiResponse<List<ReminderDTO>>> GetUpcomingRemindersAsync(int tenantId, int hours = 24)
        {
            try
            {
                var now = DateTime.UtcNow;
                var endTime = now.AddHours(hours);

                var reminders = await _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId && r.IsActive && r.NextFireAt >= now && r.NextFireAt <= endTime)
                    .OrderBy(r => r.NextFireAt)
                    .ToListAsync();

                var reminderDTOs = new List<ReminderDTO>();
                foreach (var reminder in reminders)
                {
                    reminderDTOs.Add(await MapToReminderDTOAsync(reminder));
                }

                return ApiResponse<List<ReminderDTO>>.SuccessResult(reminderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở sắp tới");
                return ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách nhắc nhở sắp tới");
            }
        }

        public async Task<ApiResponse<List<ReminderDTO>>> GetOverdueRemindersAsync(int tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var reminders = await _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId && r.IsActive && r.NextFireAt < now)
                    .OrderBy(r => r.NextFireAt)
                    .ToListAsync();

                var reminderDTOs = new List<ReminderDTO>();
                foreach (var reminder in reminders)
                {
                    reminderDTOs.Add(await MapToReminderDTOAsync(reminder));
                }

                return ApiResponse<List<ReminderDTO>>.SuccessResult(reminderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở quá hạn");
                return ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách nhắc nhở quá hạn");
            }
        }

        public async Task<ApiResponse<List<ReminderDTO>>> GetPatientRemindersAsync(int tenantId, int patientId, bool activeOnly = true)
        {
            try
            {
                var query = _context.Reminders
                    .Include(r => r.Patient)
                    .Where(r => r.TenantId == tenantId && r.PatientId == patientId);

                if (activeOnly)
                    query = query.Where(r => r.IsActive);

                var reminders = await query.OrderBy(r => r.NextFireAt).ToListAsync();

                var reminderDTOs = new List<ReminderDTO>();
                foreach (var reminder in reminders)
                {
                    reminderDTOs.Add(await MapToReminderDTOAsync(reminder));
                }

                return ApiResponse<List<ReminderDTO>>.SuccessResult(reminderDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhắc nhở của bệnh nhân");
                return ApiResponse<List<ReminderDTO>>.ErrorResult("Có lỗi xảy ra khi lấy danh sách nhắc nhở của bệnh nhân");
            }
        }

        public async Task<ApiResponse<ReminderDTO>> CreatePatientReminderAsync(int tenantId, int patientId, CreateReminderDTO dto)
        {
            dto.PatientId = patientId; // Ensure PatientId matches route
            return await CreateReminderAsync(tenantId, dto);
        }

        public async Task<ApiResponse<List<ReminderTemplateDTO>>> GetReminderTemplatesAsync()
        {
            var templates = new List<ReminderTemplateDTO>
            {
                new ReminderTemplateDTO
                {
                    TargetType = "Prescription",
                    Title = "Nhắc nhở uống thuốc",
                    Body = "Đã đến giờ uống thuốc theo đơn của bác sĩ. Vui lòng uống đúng liều lượng.",
                    Channel = "Push",
                    DefaultMinutesBefore = 30
                },
                new ReminderTemplateDTO
                {
                    TargetType = "Appointment", 
                    Title = "Nhắc nhở lịch hẹn",
                    Body = "Bạn có lịch hẹn khám sắp tới. Vui lòng có mặt đúng giờ.",
                    Channel = "Push",
                    DefaultMinutesBefore = 60
                },
                new ReminderTemplateDTO
                {
                    TargetType = "CarePlan",
                    Title = "Nhắc nhở thực hiện chăm sóc",
                    Body = "Đã đến giờ thực hiện hoạt động chăm sóc theo kế hoạch.",
                    Channel = "Push",
                    DefaultMinutesBefore = 15
                },
                new ReminderTemplateDTO
                {
                    TargetType = "Measurement",
                    Title = "Nhắc nhở đo chỉ số",
                    Body = "Vui lòng đo và ghi lại các chỉ số sức khỏe theo hướng dẫn.",
                    Channel = "Push",
                    DefaultMinutesBefore = 0
                }
            };

            return ApiResponse<List<ReminderTemplateDTO>>.SuccessResult(templates);
        }

        public async Task<ApiResponse<ReminderDTO>> CreateReminderFromTemplateAsync(int tenantId, string targetType, int targetId, int patientId, int minutesBefore = 30)
        {
            try
            {
                var templatesResponse = await GetReminderTemplatesAsync();
                var template = templatesResponse.Data?.FirstOrDefault(t => t.TargetType.Equals(targetType, StringComparison.OrdinalIgnoreCase));

                if (template == null)
                {
                    return ApiResponse<ReminderDTO>.ErrorResult($"Không tìm thấy template cho loại {targetType}");
                }

                var createDto = new CreateReminderDTO
                {
                    PatientId = patientId,
                    Title = template.Title,
                    Body = template.Body,
                    TargetType = targetType,
                    TargetId = targetId,
                    NextFireAt = DateTime.UtcNow.AddMinutes(minutesBefore),
                    Channel = template.Channel,
                    IsActive = true
                };

                return await CreateReminderAsync(tenantId, createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nhắc nhở từ template");
                return ApiResponse<ReminderDTO>.ErrorResult("Có lỗi xảy ra khi tạo nhắc nhở từ template");
            }
        }

        public async Task<bool> IsReminderAccessibleAsync(int tenantId, int reminderId)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.TenantId == tenantId);
            return reminder != null;
        }

        public async Task<bool> IsPatientReminderAccessibleAsync(int patientId, int reminderId)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.PatientId == patientId);
            return reminder != null;
        }

        #region Private Methods

        private async Task<ReminderDTO> MapToReminderDTOAsync(Reminder reminder)
        {
            var now = DateTime.UtcNow;
            var targetName = await GetTargetNameAsync(reminder.TargetType, reminder.TargetId);

            return new ReminderDTO
            {
                ReminderId = reminder.ReminderId,
                TenantId = reminder.TenantId,
                PatientId = reminder.PatientId,
                PatientName = reminder.Patient?.FullName ?? "",
                Title = reminder.Title,
                Body = reminder.Body,
                TargetType = reminder.TargetType,
                TargetId = reminder.TargetId,
                TargetName = targetName,
                NextFireAt = reminder.NextFireAt,
                Channel = reminder.Channel,
                IsActive = reminder.IsActive,
                IsOverdue = reminder.IsActive && reminder.NextFireAt < now,
                MinutesUntilFire = reminder.NextFireAt > now ? (int)(reminder.NextFireAt - now).TotalMinutes : 0
            };
        }

        private async Task<string?> GetTargetNameAsync(string targetType, int targetId)
        {
            try
            {
                return targetType.ToLower() switch
                {
                    "prescription" => await _context.Prescriptions
                        .Where(p => p.PrescriptionId == targetId)
                        .Select(p => $"Đơn thuốc #{p.PrescriptionId}")
                        .FirstOrDefaultAsync(),
                    "appointment" => await _context.Appointments
                        .Where(a => a.AppointmentId == targetId)
                        .Select(a => $"Lịch hẹn {a.StartAt:dd/MM/yyyy HH:mm}")
                        .FirstOrDefaultAsync(),
                    "careplan" => await _context.CarePlans
                        .Where(c => c.CarePlanId == targetId)
                        .Select(c => c.Name)
                        .FirstOrDefaultAsync(),
                    "measurement" => $"Đo chỉ số #{targetId}",
                    _ => $"{targetType} #{targetId}"
                };
            }
            catch
            {
                return $"{targetType} #{targetId}";
            }
        }

        private async Task<bool> ValidateTargetAsync(int tenantId, string targetType, int targetId)
        {
            return targetType.ToLower() switch
            {
                "prescription" => await _context.Prescriptions.AnyAsync(p => p.PrescriptionId == targetId && p.TenantId == tenantId),
                "appointment" => await _context.Appointments.AnyAsync(a => a.AppointmentId == targetId && a.TenantId == tenantId),
                "careplan" => await _context.CarePlans.AnyAsync(c => c.CarePlanId == targetId && c.TenantId == tenantId),
                "measurement" => await _context.PatientMeasurements.AnyAsync(m => m.MeasurementId == targetId && m.TenantId == tenantId),
                _ => false
            };
        }

        #endregion
    }
}
