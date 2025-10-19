# Tenant Settings - Hướng dẫn sử dụng

## Tổng quan

Trang **Cài đặt phòng khám** (`/dashboard/settings`) cho phép quản trị viên và chủ sở hữu phòng khám quản lý:

1. **Thông tin cơ bản** - Tên, email, địa chỉ, số điện thoại
2. **Giờ hoạt động** - Giờ mở/đóng cửa cho ngày thường và cuối tuần
3. **Hình ảnh** - Ảnh đại diện và ảnh bìa
4. **Mô tả** - Giới thiệu về phòng khám
5. **⭐ Cài đặt đặt lịch** - Quy tắc và giới hạn cho việc đặt lịch hẹn

## Cài đặt đặt lịch (Booking Settings)

### 1. Số ngày đặt trước tối đa (Max Advance Booking Days)
- **Giá trị mặc định**: 90 ngày
- **Phạm vi**: 1 - 365 ngày
- **Mô tả**: Bệnh nhân có thể đặt lịch trong vòng bao nhiêu ngày tới
- **Ví dụ**: 
  - Nếu đặt `90`, bệnh nhân chỉ có thể đặt lịch từ hôm nay đến 90 ngày sau
  - Nếu đặt `30`, chỉ xem được lịch trong 30 ngày tới

### 2. Thời lượng khung giờ mặc định (Default Slot Duration)
- **Giá trị mặc định**: 30 phút
- **Phạm vi**: 5 - 180 phút
- **Bước tăng**: 5 phút
- **Mô tả**: Mỗi ca khám kéo dài bao lâu
- **Ví dụ**:
  - `15 phút` - Khám nhanh, tư vấn đơn giản
  - `30 phút` - Khám tổng quát
  - `60 phút` - Khám chuyên sâu hoặc tái khám

### 3. Thời gian đặt trước tối thiểu (Min Advance Booking Hours)
- **Giá trị mặc định**: 1 giờ
- **Phạm vi**: 0 - 72 giờ
- **Mô tả**: Phải đặt trước ít nhất bao lâu
- **Ví dụ**:
  - `0 giờ` - Cho phép đặt lịch ngay lập tức
  - `2 giờ` - Phải đặt trước ít nhất 2 giờ
  - `24 giờ` - Phải đặt trước 1 ngày

### 4. Thời hạn hủy lịch (Max Cancellation Hours)
- **Giá trị mặc định**: 24 giờ
- **Phạm vi**: 0 - 168 giờ (7 ngày)
- **Mô tả**: Có thể hủy lịch trước bao lâu
- **Ví dụ**:
  - `24 giờ` - Phải hủy trước 1 ngày
  - `48 giờ` - Phải hủy trước 2 ngày
  - `0 giờ` - Có thể hủy bất cứ lúc nào

### 5. Cho phép đặt lịch cuối tuần (Allow Weekend Booking)
- **Giá trị mặc định**: Bật (true)
- **Mô tả**: Bệnh nhân có thể đặt lịch vào Thứ 7 và Chủ nhật
- **Lưu ý**: Cần đặt giờ mở cửa cuối tuần ở phần "Giờ hoạt động"

## Quy trình lưu thay đổi

Khi bấm nút **"Lưu thay đổi"**, hệ thống sẽ:

1. ✅ Lưu thông tin cơ bản (tên, email, địa chỉ, mô tả...)
2. ✅ Lưu ảnh đại diện và ảnh bìa (nếu có thay đổi)
3. ✅ Lưu cài đặt đặt lịch vào database (bảng `TenantSettings`)
4. ✅ Hiển thị thông báo thành công
5. ✅ Tải lại dữ liệu để hiển thị giá trị mới

## Ảnh hưởng đến các trang khác

### Trang đặt lịch (`/patient/appointments/create`)
- Sử dụng `maxAdvanceBookingDays` để giới hạn ngày có thể chọn
- Hiển thị thông báo: "✓ Có X ngày có lịch trống trong **{maxAdvanceBookingDays}** ngày tới"
- Không cho phép chọn ngày ngoài phạm vi

### API được sử dụng

```typescript
// Lấy cài đặt đặt lịch
GET /api/tenants/{tenantId}/booking-config
Response: {
  maxAdvanceBookingDays: 90,
  defaultSlotDurationMinutes: 30,
  minAdvanceBookingHours: 1,
  maxCancellationHours: 24,
  allowWeekendBooking: true
}

// Cập nhật nhiều cài đặt cùng lúc
PUT /api/tenants/{tenantId}/settings
Body: {
  settings: {
    "Booking.MaxAdvanceBookingDays": "90",
    "Booking.DefaultSlotDurationMinutes": "30",
    "Booking.MinAdvanceBookingHours": "1",
    "Booking.MaxCancellationHours": "24",
    "Booking.AllowWeekendBooking": "true"
  }
}
```

## Quyền truy cập

Chỉ những người dùng sau có quyền chỉnh sửa:
- ✅ **Admin** (role: "Admin")
- ✅ **Chủ sở hữu phòng khám** (ownerUserId khớp với userId hiện tại)

Người dùng khác chỉ có quyền **XEM**, không thể chỉnh sửa.

## Xử lý lỗi

- Nếu không tải được cài đặt từ database → Sử dụng giá trị mặc định
- Nếu lưu thất bại → Hiển thị thông báo lỗi chi tiết
- Validation:
  - Số ngày phải > 0
  - Thời lượng khung giờ phải là bội số của 5
  - Không cho phép giá trị âm

## Kiểm tra sau khi triển khai

1. ✅ Chạy SQL migration `CreateTenantSettings.sql`
2. ✅ Khởi động backend: `dotnet run`
3. ✅ Test API endpoint: `/api/tenants/1/booking-config`
4. ✅ Mở trang `/dashboard/settings`
5. ✅ Thay đổi giá trị và lưu
6. ✅ Mở trang `/patient/appointments/create` kiểm tra số ngày hiển thị

## Files liên quan

### Backend
- `Controllers/TenantSettingsController.cs` - API endpoints
- `Services/TenantSettingService.cs` - Business logic
- `Models/TenantSetting.cs` - Entity model
- `DTOs/TenantSettingDTOs.cs` - Data transfer objects

### Frontend
- `src/pages/dashboard/TenantSettings.tsx` - Trang chính
- `src/components/tenant/BookingSettingsForm.tsx` - Form cài đặt đặt lịch
- `src/services/tenantSettingService.ts` - Service gọi API
- `src/pages/patient/CreateAppointment.tsx` - Sử dụng cài đặt

### Database
- Table: `TenantSettings`
- Trigger: `trg_UpdateTenantSettingTimestamp`
