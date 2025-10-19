-- ============================================
-- Enhancement Script for SavePlus Database
-- Adds Doctor Working Hours and Service Pricing
-- Date: 2025-10-16
-- ============================================

USE [SavePlusDB]
GO

-- ============================================
-- 1. DoctorWorkingHours Table
-- Lưu lịch làm việc của bác sĩ theo ngày trong tuần
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorWorkingHours]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoctorWorkingHours](
        [WorkingHourId] [int] IDENTITY(1,1) NOT NULL,
        [DoctorId] [int] NOT NULL,
        [DayOfWeek] [tinyint] NOT NULL,  -- 1=Monday, 7=Sunday
        [StartTime] [time](7) NOT NULL,  -- e.g., 08:00:00
        [EndTime] [time](7) NOT NULL,    -- e.g., 17:00:00
        [SlotDurationMinutes] [int] NOT NULL DEFAULT 30,  -- Thời gian mỗi slot (mặc định 30 phút)
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_DoctorWorkingHours] PRIMARY KEY CLUSTERED ([WorkingHourId] ASC),
        CONSTRAINT [FK_DoctorWorkingHours_Doctor] FOREIGN KEY([DoctorId]) REFERENCES [dbo].[Doctors] ([DoctorId]),
        CONSTRAINT [CK_DoctorWorkingHours_DayOfWeek] CHECK ([DayOfWeek] >= 1 AND [DayOfWeek] <= 7),
        CONSTRAINT [CK_DoctorWorkingHours_TimeRange] CHECK ([StartTime] < [EndTime])
    )

    CREATE NONCLUSTERED INDEX [IX_DoctorWorkingHours_Doctor_Day] ON [dbo].[DoctorWorkingHours]
    (
        [DoctorId] ASC,
        [DayOfWeek] ASC,
        [IsActive] ASC
    )
END
GO

-- ============================================
-- 2. Services Table (Optional - Danh mục dịch vụ khám)
-- Lưu các loại dịch vụ khám bệnh
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Services](
        [ServiceId] [int] IDENTITY(1,1) NOT NULL,
        [TenantId] [int] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [BasePrice] [decimal](12, 2) NOT NULL DEFAULT 200000,  -- Giá cơ bản
        [DurationMinutes] [int] NOT NULL DEFAULT 30,  -- Thời gian khám dự kiến
        [ServiceType] [nvarchar](50) NOT NULL DEFAULT 'General',  -- General, Specialist, Emergency
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_Services] PRIMARY KEY CLUSTERED ([ServiceId] ASC),
        CONSTRAINT [FK_Services_Tenant] FOREIGN KEY([TenantId]) REFERENCES [dbo].[Tenants] ([TenantId])
    )

    CREATE NONCLUSTERED INDEX [IX_Services_Tenant_Active] ON [dbo].[Services]
    (
        [TenantId] ASC,
        [IsActive] ASC
    )
END
GO

-- ============================================
-- 3. Add ServiceId to Appointments (Optional)
-- Liên kết appointment với service
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'ServiceId')
BEGIN
    ALTER TABLE [dbo].[Appointments]
    ADD [ServiceId] [int] NULL

    ALTER TABLE [dbo].[Appointments]
    ADD CONSTRAINT [FK_Appointments_Service] 
    FOREIGN KEY([ServiceId]) REFERENCES [dbo].[Services] ([ServiceId])
END
GO

-- ============================================
-- 4. Sample Data for DoctorWorkingHours
-- Ví dụ: Bác sĩ ID 1 làm việc Thứ 2-6: 8:00-17:00, Thứ 7: 8:00-12:00
-- ============================================
IF NOT EXISTS (SELECT * FROM [dbo].[DoctorWorkingHours] WHERE DoctorId = 1)
BEGIN
    -- Thứ 2 (Monday)
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 1, '08:00:00', '17:00:00', 30)

    -- Thứ 3 (Tuesday)
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 2, '08:00:00', '17:00:00', 30)

    -- Thứ 4 (Wednesday)
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 3, '08:00:00', '17:00:00', 30)

    -- Thứ 5 (Thursday)
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 4, '08:00:00', '17:00:00', 30)

    -- Thứ 6 (Friday)
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 5, '08:00:00', '17:00:00', 30)

    -- Thứ 7 (Saturday) - Nửa ngày
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES (1, 6, '08:00:00', '12:00:00', 30)
END
GO

-- ============================================
-- 5. Sample Data for Services
-- ============================================
IF NOT EXISTS (SELECT * FROM [dbo].[Services] WHERE TenantId = 1)
BEGIN
    INSERT INTO [dbo].[Services] ([TenantId], [Name], [Description], [BasePrice], [DurationMinutes], [ServiceType])
    VALUES 
    (1, N'Khám dịch vụ', N'Khám bệnh dịch vụ chuyên sâu', 200000, 30, 'Specialist'),
    (1, N'Khám thường', N'Khám bệnh thông thường', 50600, 20, 'General'),
    (1, N'Khám Ngoài giờ (không BHYT)', N'Khám bệnh ngoài giờ hành chính', 120000, 30, 'Emergency')
END
GO

-- ============================================
-- 6. Update existing doctor with working hours (Doctor ID 5)
-- ============================================
IF NOT EXISTS (SELECT * FROM [dbo].[DoctorWorkingHours] WHERE DoctorId = 5)
BEGIN
    INSERT INTO [dbo].[DoctorWorkingHours] ([DoctorId], [DayOfWeek], [StartTime], [EndTime], [SlotDurationMinutes])
    VALUES 
    (5, 1, '08:00:00', '17:00:00', 30),
    (5, 2, '08:00:00', '17:00:00', 30),
    (5, 3, '08:00:00', '17:00:00', 30),
    (5, 4, '08:00:00', '17:00:00', 30),
    (5, 5, '08:00:00', '17:00:00', 30),
    (5, 6, '08:00:00', '12:00:00', 30)
END
GO

PRINT 'Database enhancement completed successfully!'
PRINT 'Added tables: DoctorWorkingHours, Services'
PRINT 'Added sample working hours for Doctor ID 1 and 5'
PRINT 'Added sample services for Tenant ID 1'
GO
