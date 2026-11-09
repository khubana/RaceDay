/* READ-ONLY verification. This script does not create, alter, or delete anything. */
USE RaceDay;
SELECT DB_NAME() AS CurrentDatabase;
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Roles','Users','Events','Categories','EventEnrolment','Results')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
SELECT RoleID, RoleName FROM dbo.Roles ORDER BY RoleName;
