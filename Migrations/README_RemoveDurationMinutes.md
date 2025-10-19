# Migration: Remove DurationMinutes from Services

## Mục đích
Xóa field `DurationMinutes` khỏi bảng `Services` vì:
- Thời gian khám thực tế không cố định theo service
- Hệ thống sử dụng **cố định 30 phút/slot** cho tất cả appointments
- Đơn giản hóa logic booking

## Các thay đổi

### 1. Database
- **Xóa column**: `Services.DurationMinutes`
- **Script**: `RemoveDurationMinutes.sql`

### 2. Backend (C#)
- ✅ `Models/Service.cs` - Không có field DurationMinutes
- ✅ `DTOs/ServiceDTOs.cs` - Xóa khỏi ServiceDto, ServiceCreateDto, ServiceUpdateDto
- ✅ `Services/ServiceService.cs` - Xóa mapping DurationMinutes trong tất cả methods

### 3. Frontend (TypeScript)
- ✅ `types/service.ts` - Xóa `durationMinutes` từ Service, ServiceCreateDto, ServiceUpdateDto

## Cách chạy migration

### Bước 1: Backup database (khuyến nghị)
```sql
BACKUP DATABASE [SavePlusDb] 
TO DISK = 'C:\Backup\SavePlusDb_BeforeRemoveDuration.bak'
```

### Bước 2: Chạy SQL script
```bash
# Mở SQL Server Management Studio hoặc Azure Data Studio
# Chọn database: SavePlusDb
# Mở file: Migrations/RemoveDurationMinutes.sql
# Execute (F5)
```

Hoặc dùng command line:
```bash
sqlcmd -S localhost -d SavePlusDb -i Migrations/RemoveDurationMinutes.sql
```

### Bước 3: Verify
```sql
-- Check column đã bị xóa chưa
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Services';

-- Kết quả KHÔNG nên có 'DurationMinutes'
```

### Bước 4: Restart backend
```bash
cd SavePlus_API
dotnet run
```

## Impact Analysis

### ✅ Không ảnh hưởng
- Existing appointments: EndAt đã được lưu sẵn, không cần tính lại
- Doctor working hours: Vẫn hoạt động bình thường
- Payment transactions: Chỉ dùng BasePrice, không dùng Duration

### ⚠️ Lưu ý
- Sau khi xóa column, **không thể rollback** trừ khi restore từ backup
- Data migration: Không cần migrate data vì không lưu duration ở đâu khác

## Testing Checklist

- [ ] Xóa column thành công (query INFORMATION_SCHEMA)
- [ ] Backend build thành công (0 errors)
- [ ] API GET /services/tenant/{id} trả về services không có durationMinutes
- [ ] API POST /services tạo service thành công (không cần durationMinutes)
- [ ] Frontend hiển thị services list bình thường
- [ ] Booking flow: Chọn service → date → time (30-min slots) → doctor
- [ ] Create appointment thành công với EndAt = StartAt + 30 minutes

## Rollback (nếu cần)

Nếu cần khôi phục column:
```sql
ALTER TABLE Services
ADD DurationMinutes INT NOT NULL DEFAULT 30;

-- Update existing records
UPDATE Services SET DurationMinutes = 30;
```

Sau đó revert code changes trong:
- `DTOs/ServiceDTOs.cs`
- `Services/ServiceService.cs`
- `types/service.ts` (Frontend)
