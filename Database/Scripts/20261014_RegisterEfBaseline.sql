-- Run only for an existing database after 20261014_SeparateUserRoles.sql.
-- The initial EF migration creates a fresh schema; this database already has that schema.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
    OR COL_LENGTH(N'dbo.Users', N'RoleId') IS NULL
    OR COL_LENGTH(N'dbo.Users', N'Role') IS NOT NULL
    THROW 50010, 'Role migration has not been applied to this database.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.Users AS u
    LEFT JOIN dbo.Roles AS r ON r.Id = u.RoleId
    WHERE r.Id IS NULL)
    THROW 50011, 'Users contains an invalid RoleId.', 1;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId nvarchar(150) NOT NULL,
        ProductVersion nvarchar(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END;

IF EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20261014160102_InitialSchema')
BEGIN
    COMMIT TRANSACTION;
    RETURN;
END;

IF EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory)
    THROW 50012, 'Unexpected EF migration history; review it before registering the baseline.', 1;

INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES (N'20261014160102_InitialSchema', N'8.0.0');

COMMIT TRANSACTION;
