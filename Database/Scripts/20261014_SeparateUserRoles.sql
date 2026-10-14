-- Run once against the existing EduSync SQL Server database before deploying the updated API.
-- Existing Users.Role values are copied into Roles before the old column is removed.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    THROW 50001, 'dbo.Users does not exist.', 1;

IF COL_LENGTH(N'dbo.Users', N'Role') IS NULL
    THROW 50002, 'dbo.Users.Role does not exist; check whether this script was already applied.', 1;

IF COL_LENGTH(N'dbo.Users', N'RoleId') IS NOT NULL
    THROW 50003, 'dbo.Users.RoleId already exists; review the database before migration.', 1;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_Roles_Id DEFAULT newsequentialid(),
        Name varchar(20) NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX UQ_Roles_Name ON dbo.Roles (Name);
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE NULLIF(LTRIM(RTRIM(Role)), '') IS NULL)
    THROW 50004, 'Users contains an empty role; correct it before migration.', 1;

INSERT INTO dbo.Roles (Name)
SELECT DISTINCT u.Role
FROM dbo.Users AS u
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles AS r WHERE r.Name = u.Role);

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Student')
    INSERT INTO dbo.Roles (Name) VALUES ('Student');

ALTER TABLE dbo.Users ADD RoleId uniqueidentifier NULL;

EXEC sys.sp_executesql N'
    UPDATE u
    SET RoleId = r.Id
    FROM dbo.Users AS u
    INNER JOIN dbo.Roles AS r ON r.Name = u.Role;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE RoleId IS NULL)
        THROW 50005, ''One or more user roles could not be mapped.'', 1;

    ALTER TABLE dbo.Users ALTER COLUMN RoleId uniqueidentifier NOT NULL;
    CREATE INDEX IX_Users_RoleId ON dbo.Users (RoleId);
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id);';

DECLARE @defaultConstraint sysname;
SELECT @defaultConstraint = dc.name
FROM sys.default_constraints AS dc
INNER JOIN sys.columns AS c
    ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Users') AND c.name = N'Role';

IF @defaultConstraint IS NOT NULL
BEGIN
    DECLARE @dropDefaultSql nvarchar(4000) =
        N'ALTER TABLE dbo.Users DROP CONSTRAINT ' + QUOTENAME(@defaultConstraint);
    EXEC sys.sp_executesql @dropDefaultSql;
END;

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.Users') AND name = N'CK_Users_Role')
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;

ALTER TABLE dbo.Users DROP COLUMN Role;

COMMIT TRANSACTION;
