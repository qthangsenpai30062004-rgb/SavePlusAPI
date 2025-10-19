-- Migration: Remove DurationMinutes from Services table
-- Date: 2024-10-16
-- Reason: Appointment duration should be flexible, not tied to service type

-- Step 1: Drop the DurationMinutes column
ALTER TABLE Services
DROP COLUMN DurationMinutes;

GO

-- Verify the change
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Services';
