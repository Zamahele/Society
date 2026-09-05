/* Run this script while connected to the production ikhaya society database. */

IF SCHEMA_ID(N'society') IS NULL
    EXEC(N'CREATE SCHEMA [society]');
GO

IF OBJECT_ID(N'[society].[Organizations]', N'U') IS NULL
BEGIN
    CREATE TABLE [society].[Organizations]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [RegistrationNumber] nvarchar(100) NOT NULL,
        [ContactPerson] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [DateRegistered] datetime2 NOT NULL,
        CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Organizations_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [society].[AspNetUsers] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_Organizations_UserId'
      AND [object_id] = OBJECT_ID(N'[society].[Organizations]')
)
    CREATE UNIQUE INDEX [IX_Organizations_UserId]
        ON [society].[Organizations] ([UserId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_Organizations_RegistrationNumber'
      AND [object_id] = OBJECT_ID(N'[society].[Organizations]')
)
    CREATE UNIQUE INDEX [IX_Organizations_RegistrationNumber]
        ON [society].[Organizations] ([RegistrationNumber]);
GO

IF OBJECT_ID(N'[society].[OrganizationMembers]', N'U') IS NULL
BEGIN
    CREATE TABLE [society].[OrganizationMembers]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrganizationId] int NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [IDNumber] nvarchar(50) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [DateAdded] datetime2 NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_OrganizationMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationMembers_Organizations_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [society].[Organizations] ([Id])
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_OrganizationMembers_OrganizationId_IDNumber'
      AND [object_id] = OBJECT_ID(N'[society].[OrganizationMembers]')
)
    CREATE UNIQUE INDEX [IX_OrganizationMembers_OrganizationId_IDNumber]
        ON [society].[OrganizationMembers] ([OrganizationId], [IDNumber]);
GO