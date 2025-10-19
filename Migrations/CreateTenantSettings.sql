-- Create TenantSettings table for flexible per-clinic configuration
-- Each tenant (clinic) can have their own booking policies

CREATE TABLE TenantSettings (
    TenantSettingId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(500) NOT NULL,
    SettingType NVARCHAR(50) NOT NULL, -- 'Integer', 'String', 'Boolean', 'Decimal'
    Category NVARCHAR(50) NOT NULL, -- 'Booking', 'Payment', 'Notification', etc.
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign key to Tenants table
    CONSTRAINT FK_TenantSettings_Tenant FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    
    -- Unique constraint: each tenant can only have one value per setting key
    CONSTRAINT UQ_TenantSettings_TenantId_SettingKey UNIQUE (TenantId, SettingKey)
);

-- Create indexes for faster lookups
CREATE INDEX IX_TenantSettings_TenantId ON TenantSettings(TenantId);
CREATE INDEX IX_TenantSettings_Category ON TenantSettings(Category);

GO

-- Create trigger to update UpdatedAt automatically
CREATE TRIGGER TR_TenantSettings_UpdatedAt
ON TenantSettings
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE TenantSettings
    SET UpdatedAt = GETDATE()
    FROM TenantSettings s
    INNER JOIN inserted i ON s.TenantSettingId = i.TenantSettingId;
END;

GO

-- Insert default settings for existing tenants
-- You can customize these values per tenant as needed
INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.MaxAdvanceBookingDays',
    '90',
    'Integer',
    'Booking',
    'Số ngày tối đa có thể đặt lịch trước (ví dụ: 90 ngày)'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.MaxAdvanceBookingDays'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.DefaultSlotDurationMinutes',
    '30',
    'Integer',
    'Booking',
    'Thời lượng mặc định của mỗi khung giờ khám (phút)'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.DefaultSlotDurationMinutes'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.MinAdvanceBookingHours',
    '1',
    'Integer',
    'Booking',
    'Số giờ tối thiểu phải đặt lịch trước'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.MinAdvanceBookingHours'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.MaxCancellationHours',
    '24',
    'Integer',
    'Booking',
    'Số giờ tối đa có thể hủy lịch trước khi khám'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.MaxCancellationHours'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Booking.AllowWeekendBooking',
    'true',
    'Boolean',
    'Booking',
    'Cho phép đặt lịch vào cuối tuần'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Booking.AllowWeekendBooking'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Payment.BankTransferEnabled',
    'true',
    'Boolean',
    'Payment',
    'Cho phép thanh toán qua chuyển khoản ngân hàng'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Payment.BankTransferEnabled'
);

INSERT INTO TenantSettings (TenantId, SettingKey, SettingValue, SettingType, Category, Description)
SELECT 
    t.TenantId,
    'Payment.EWalletEnabled',
    'false',
    'Boolean',
    'Payment',
    'Cho phép thanh toán qua ví điện tử'
FROM Tenants t
WHERE NOT EXISTS (
    SELECT 1 FROM TenantSettings ts 
    WHERE ts.TenantId = t.TenantId 
    AND ts.SettingKey = 'Payment.EWalletEnabled'
);

GO

-- Verify the data
SELECT 
    t.Name AS TenantName,
    ts.SettingKey,
    ts.SettingValue,
    ts.SettingType,
    ts.Category,
    ts.Description
FROM TenantSettings ts
INNER JOIN Tenants t ON ts.TenantId = t.TenantId
ORDER BY t.Name, ts.Category, ts.SettingKey;
