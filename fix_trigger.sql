-- Script to fix the trigger to use ClinicAdmin instead of Tenant role
-- Run this in SQL Server Management Studio or Azure Data Studio

-- First, find the trigger name
SELECT 
    name as TriggerName,
    OBJECT_NAME(parent_id) as TableName
FROM sys.triggers
WHERE parent_id = OBJECT_ID('Tenants');

-- Then get the trigger definition to see current code
-- Replace 'YourTriggerName' with the actual trigger name from above query
-- EXEC sp_helptext 'YourTriggerName';

-- Example fix - update the trigger to check for 'ClinicAdmin' instead of 'Tenant'
-- You'll need to replace this with your actual trigger code
/*
ALTER TRIGGER trg_ValidateOwnerUserId ON Tenants
AFTER INSERT, UPDATE
AS BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(OwnerUserId)  -- Only validate when OwnerUserId is actually changed
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM inserted i
            LEFT JOIN Users u ON u.UserId = i.OwnerUserId 
                              AND u.TenantId = i.TenantId 
                              AND u.Role = 'ClinicAdmin'  -- Changed from 'Tenant' to 'ClinicAdmin'
            WHERE i.OwnerUserId IS NOT NULL 
            AND u.UserId IS NULL
        )
        BEGIN
            RAISERROR('OwnerUserId phải là user thuộc tenant với Role = ''ClinicAdmin''.', 16, 1);
            ROLLBACK TRANSACTION;
        END
    END
END;
*/
