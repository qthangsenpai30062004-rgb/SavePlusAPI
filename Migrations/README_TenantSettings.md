# Tenant Settings System - Hệ thống cấu hình linh hoạt cho từng phòng khám

## 📋 Tổng quan

Hệ thống TenantSettings cho phép mỗi phòng khám (tenant) tự cấu hình các chính sách riêng về:
- **Đặt lịch** (Booking): Số ngày tối đa đặt trước, thời lượng khung giờ, quy định hủy lịch...
- **Thanh toán** (Payment): Phương thức thanh toán được phép sử dụng
- **Và nhiều hơn nữa** (có thể mở rộng)

## 🗄️ Database Schema

### Bảng TenantSettings

```sql
CREATE TABLE TenantSettings (
    TenantSettingId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(500) NOT NULL,
    SettingType NVARCHAR(50) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_TenantSettings_Tenant FOREIGN KEY (TenantId) 
        REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT UQ_TenantSettings_TenantId_SettingKey 
        UNIQUE (TenantId, SettingKey)
);
```

### Các Settings mặc định

| Setting Key | Giá trị mặc định | Loại | Mô tả |
|------------|------------------|------|-------|
| `Booking.MaxAdvanceBookingDays` | 90 | Integer | Số ngày tối đa có thể đặt lịch trước |
| `Booking.DefaultSlotDurationMinutes` | 30 | Integer | Thời lượng mỗi khung giờ (phút) |
| `Booking.MinAdvanceBookingHours` | 1 | Integer | Số giờ tối thiểu phải đặt trước |
| `Booking.MaxCancellationHours` | 24 | Integer | Hủy lịch trước bao nhiêu giờ |
| `Booking.AllowWeekendBooking` | true | Boolean | Cho phép đặt lịch cuối tuần |
| `Payment.BankTransferEnabled` | true | Boolean | Thanh toán chuyển khoản |
| `Payment.EWalletEnabled` | false | Boolean | Thanh toán ví điện tử |

## 🚀 Cài đặt

### Bước 1: Chạy Migration SQL

```bash
# Mở SQL Server Management Studio
# Chạy file migration
```

```sql
-- File: Migrations/CreateTenantSettings.sql
-- Tự động tạo bảng và insert settings mặc định cho tất cả tenant hiện có
```

### Bước 2: Kiểm tra dữ liệu

```sql
-- Xem settings của tất cả tenant
SELECT 
    t.Name AS TenantName,
    ts.SettingKey,
    ts.SettingValue,
    ts.Category
FROM TenantSettings ts
INNER JOIN Tenants t ON ts.TenantId = t.TenantId
ORDER BY t.Name, ts.Category;
```

### Bước 3: Build & Run Backend

```bash
cd SavePlus_API
dotnet build
dotnet run
```

Backend đã tự động đăng ký `TenantSettingService` trong DI container.

## 📡 API Endpoints

### 1. Lấy Booking Config (Client-friendly)

**GET** `/api/tenants/{tenantId}/booking-config`

Response:
```json
{
  "success": true,
  "data": {
    "maxAdvanceBookingDays": 90,
    "defaultSlotDurationMinutes": 30,
    "minAdvanceBookingHours": 1,
    "maxCancellationHours": 24,
    "allowWeekendBooking": true
  }
}
```

### 2. Lấy Payment Config

**GET** `/api/tenants/{tenantId}/payment-config`

Response:
```json
{
  "success": true,
  "data": {
    "bankTransferEnabled": true,
    "eWalletEnabled": false,
    "cashEnabled": true
  }
}
```

### 3. Lấy tất cả settings

**GET** `/api/tenants/{tenantId}/settings?category=Booking`

### 4. Cập nhật setting đơn lẻ

**PUT** `/api/tenants/{tenantId}/settings/Booking.MaxAdvanceBookingDays`

Request:
```json
{
  "settingKey": "Booking.MaxAdvanceBookingDays",
  "settingValue": "120"
}
```

### 5. Cập nhật nhiều settings cùng lúc

**PUT** `/api/tenants/{tenantId}/settings`

Request:
```json
{
  "settings": {
    "Booking.MaxAdvanceBookingDays": "120",
    "Booking.MinAdvanceBookingHours": "2",
    "Payment.EWalletEnabled": "true"
  }
}
```

### 6. Khởi tạo settings cho tenant mới

**POST** `/api/tenants/{tenantId}/settings/initialize`

## 💻 Frontend Usage

### 1. Import service

```typescript
import { tenantSettingService } from '@/services/tenantSettingService';
```

### 2. Lấy booking config

```typescript
const config = await tenantSettingService.getBookingConfig(tenantId);
const maxDays = config.data.maxAdvanceBookingDays; // 90
```

### 3. Lấy max booking date tự động

```typescript
const maxDate = await tenantSettingService.getMaxBookingDate(tenantId);
// Returns Date object calculated from MaxAdvanceBookingDays
```

### 4. Lấy date range cho calendar

```typescript
const { startDate, endDate } = await tenantSettingService.getBookingDateRange(tenantId);
// startDate: "2025-10-16"
// endDate: "2026-01-14" (90 days from today)
```

## 🎯 Use Cases

### Case 1: Thay đổi chính sách đặt lịch

Phòng khám A muốn cho phép đặt lịch trước **60 ngày** thay vì 90 ngày:

```typescript
await tenantSettingService.updateSetting(
  tenantId, 
  'Booking.MaxAdvanceBookingDays', 
  '60'
);
```

Ngay lập tức:
- Frontend sẽ load max date mới (60 ngày)
- Calendar chỉ cho chọn trong khoảng 60 ngày
- Không cần deploy code mới!

### Case 2: Bật thanh toán ví điện tử

```typescript
await tenantSettingService.updateSetting(
  tenantId,
  'Payment.EWalletEnabled',
  'true'
);
```

Payment page sẽ tự động hiển thị option ví điện tử.

### Case 3: Admin panel - Bulk update

```typescript
const newSettings = {
  'Booking.MaxAdvanceBookingDays': '120',
  'Booking.MinAdvanceBookingHours': '2',
  'Booking.AllowWeekendBooking': 'false',
  'Payment.BankTransferEnabled': 'true',
  'Payment.EWalletEnabled': 'true'
};

await tenantSettingService.updateSettings(tenantId, newSettings);
```

## 🔧 Mở rộng Settings mới

### Thêm setting mới vào hệ thống

1. **Cập nhật TenantSettingService.cs**:

```csharp
private readonly Dictionary<string, (string Value, string Type, string Category, string Description)> _defaultSettings = new()
{
    // ... existing settings ...
    
    // New setting
    ["Booking.RequireDeposit"] = ("false", "Boolean", "Booking", "Yêu cầu đặt cọc khi đặt lịch"),
    ["Booking.DepositPercentage"] = ("30", "Integer", "Booking", "Phần trăm tiền cọc (%)"),
};
```

2. **Thêm vào BookingConfigDto** (nếu muốn expose qua API):

```csharp
public class BookingConfigDto
{
    // ... existing fields ...
    
    public bool RequireDeposit { get; set; }
    public int DepositPercentage { get; set; }
}
```

3. **Cập nhật GetBookingConfigAsync()**:

```csharp
public async Task<BookingConfigDto> GetBookingConfigAsync(int tenantId)
{
    // ... existing code ...
    
    var requireDeposit = await GetSettingValueAsync<bool>(tenantId, "Booking.RequireDeposit");
    var depositPct = await GetSettingValueAsync<int>(tenantId, "Booking.DepositPercentage") ?? 30;
    
    return new BookingConfigDto
    {
        // ... existing fields ...
        RequireDeposit = requireDeposit,
        DepositPercentage = depositPct
    };
}
```

4. **Insert vào database cho tenant hiện có**:

```sql
INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.RequireDeposit',
    'false',
    'Boolean',
    'Booking',
    'Yêu cầu đặt cọc khi đặt lịch'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.RequireDeposit'
);
```

## ✅ Lợi ích

1. **Linh hoạt tối đa**: Mỗi phòng khám tự quản lý chính sách riêng
2. **Không cần deploy**: Thay đổi settings không cần build lại code
3. **Dễ mở rộng**: Thêm setting mới rất đơn giản
4. **Type-safe**: Generic method `GetSettingValueAsync<T>()` tự động convert kiểu
5. **Fallback**: Có giá trị mặc định nếu setting chưa được cài
6. **Audit**: Có `CreatedAt`, `UpdatedAt` để tracking thay đổi

## 📝 Best Practices

1. **Đặt tên key chuẩn**: Dùng format `Category.SettingName` (VD: `Booking.MaxDays`)
2. **Validation**: Validate giá trị trước khi update (VD: MaxDays phải > 0)
3. **Cache**: Frontend có thể cache config để giảm API calls
4. **Tenant initialization**: Luôn gọi `InitializeSettings()` khi tạo tenant mới
5. **Documentation**: Update README khi thêm setting mới

## 🐛 Troubleshooting

### Settings không load được

```typescript
// Check if tenant exists and has settings initialized
const settings = await tenantSettingService.getTenantSettings(tenantId);
console.log(settings);

// If empty, initialize
await tenantSettingService.initializeSettings(tenantId);
```

### Giá trị trả về null

Service tự động fallback về default nếu setting không tồn tại. Nếu vẫn null:

```csharp
// Check if default value is defined in _defaultSettings dictionary
// Or explicitly set a fallback value
var maxDays = await GetSettingValueAsync<int>(tenantId, "Booking.MaxAdvanceBookingDays") ?? 90;
```

## 🎉 Kết luận

Hệ thống TenantSettings giúp:
- ✅ Loại bỏ hardcode ở frontend và backend
- ✅ Quản lý config theo từng tenant trong database
- ✅ Dễ dàng thay đổi và mở rộng
- ✅ Type-safe và có fallback
- ✅ RESTful API chuẩn chỉnh

Giờ đây bạn có thể thay đổi số ngày đặt lịch tối đa, bật/tắt phương thức thanh toán... chỉ bằng việc update database, không cần sửa code!
