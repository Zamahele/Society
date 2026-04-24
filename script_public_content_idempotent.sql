IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IDNumber] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [BankAccountName] nvarchar(max) NOT NULL,
        [BankAccountNumber] nvarchar(max) NOT NULL,
        [BankName] nvarchar(max) NOT NULL,
        [DateRegistered] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [Memberships] (
        [Id] int NOT NULL IDENTITY,
        [MembershipNumber] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [JoiningFeeAmount] decimal(18,2) NOT NULL,
        [MonthlyFeeAmount] decimal(18,2) NOT NULL,
        [DateIssued] datetime2 NOT NULL,
        [DateActivated] datetime2 NULL,
        CONSTRAINT [PK_Memberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Memberships_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [JoiningFeePayments] (
        [Id] int NOT NULL IDENTITY,
        [MembershipId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [PaymentReference] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [ConfirmedByClerkId] nvarchar(450) NULL,
        [ConfirmedDate] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_JoiningFeePayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JoiningFeePayments_AspNetUsers_ConfirmedByClerkId] FOREIGN KEY ([ConfirmedByClerkId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_JoiningFeePayments_Memberships_MembershipId] FOREIGN KEY ([MembershipId]) REFERENCES [Memberships] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [MemberDependants] (
        [Id] int NOT NULL IDENTITY,
        [MembershipId] int NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IDNumber] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Relationship] int NOT NULL,
        [DateAdded] datetime2 NOT NULL,
        CONSTRAINT [PK_MemberDependants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MemberDependants_Memberships_MembershipId] FOREIGN KEY ([MembershipId]) REFERENCES [Memberships] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [MonthlyPayments] (
        [Id] int NOT NULL IDENTITY,
        [MembershipId] int NOT NULL,
        [ForMonth] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NULL,
        [PaymentReference] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ConfirmedByClerkId] nvarchar(450) NULL,
        [ConfirmedDate] datetime2 NULL,
        CONSTRAINT [PK_MonthlyPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MonthlyPayments_AspNetUsers_ConfirmedByClerkId] FOREIGN KEY ([ConfirmedByClerkId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_MonthlyPayments_Memberships_MembershipId] FOREIGN KEY ([MembershipId]) REFERENCES [Memberships] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE TABLE [DeathClaims] (
        [Id] int NOT NULL IDENTITY,
        [MembershipId] int NOT NULL,
        [DeceasedType] int NOT NULL,
        [DependantId] int NULL,
        [DeceasedFullName] nvarchar(max) NOT NULL,
        [DateOfDeath] datetime2 NOT NULL,
        [DeathCertificateData] varbinary(max) NULL,
        [DeathCertificateFileName] nvarchar(max) NULL,
        [ClaimDate] datetime2 NOT NULL,
        [ClaimStatus] int NOT NULL,
        [CashAmount] decimal(18,2) NOT NULL,
        [CashPaidDate] datetime2 NULL,
        [VoucherAmount] decimal(18,2) NOT NULL,
        [VoucherReference] nvarchar(max) NULL,
        [VoucherPaidDate] datetime2 NULL,
        [RejectionReason] nvarchar(max) NULL,
        [ProcessedByAdminId] nvarchar(450) NULL,
        CONSTRAINT [PK_DeathClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeathClaims_AspNetUsers_ProcessedByAdminId] FOREIGN KEY ([ProcessedByAdminId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_DeathClaims_MemberDependants_DependantId] FOREIGN KEY ([DependantId]) REFERENCES [MemberDependants] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_DeathClaims_Memberships_MembershipId] FOREIGN KEY ([MembershipId]) REFERENCES [Memberships] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeathClaims_DependantId] ON [DeathClaims] ([DependantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeathClaims_MembershipId] ON [DeathClaims] ([MembershipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeathClaims_ProcessedByAdminId] ON [DeathClaims] ([ProcessedByAdminId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JoiningFeePayments_ConfirmedByClerkId] ON [JoiningFeePayments] ([ConfirmedByClerkId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JoiningFeePayments_MembershipId] ON [JoiningFeePayments] ([MembershipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MemberDependants_MembershipId] ON [MemberDependants] ([MembershipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Memberships_MembershipNumber] ON [Memberships] ([MembershipNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Memberships_UserId] ON [Memberships] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MonthlyPayments_ConfirmedByClerkId] ON [MonthlyPayments] ([ConfirmedByClerkId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MonthlyPayments_MembershipId] ON [MonthlyPayments] ([MembershipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402134900_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402134900_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    IF SCHEMA_ID(N'society') IS NULL EXEC(N'CREATE SCHEMA [society];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [MonthlyPayments];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [Memberships];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [MemberDependants];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [JoiningFeePayments];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [DeathClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetUserTokens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetUserRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetUserLogins];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetUserClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    ALTER SCHEMA [society] TRANSFER [AspNetRoleClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411123947_AddSocietySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411123947_AddSocietySchema', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[MonthlyPayments] ADD [SubmittedByClerkId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[JoiningFeePayments] ADD [SubmittedByClerkId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[DeathClaims] ADD [SubmittedByClerkId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    CREATE INDEX [IX_MonthlyPayments_SubmittedByClerkId] ON [society].[MonthlyPayments] ([SubmittedByClerkId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    CREATE INDEX [IX_JoiningFeePayments_SubmittedByClerkId] ON [society].[JoiningFeePayments] ([SubmittedByClerkId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    CREATE INDEX [IX_DeathClaims_SubmittedByClerkId] ON [society].[DeathClaims] ([SubmittedByClerkId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[DeathClaims] ADD CONSTRAINT [FK_DeathClaims_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[JoiningFeePayments] ADD CONSTRAINT [FK_JoiningFeePayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    ALTER TABLE [society].[MonthlyPayments] ADD CONSTRAINT [FK_MonthlyPayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411141832_AddClerkSubmittedBy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411141832_AddClerkSubmittedBy', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424123941_AddPublicContentManagement'
)
BEGIN
    CREATE TABLE [society].[CommitteeMembers] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(150) NOT NULL,
        [RoleTitle] nvarchar(80) NOT NULL,
        [Phone] nvarchar(40) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CommitteeMembers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424123941_AddPublicContentManagement'
)
BEGIN
    CREATE TABLE [society].[PublicSiteSettings] (
        [Id] int NOT NULL IDENTITY,
        [OrganizationName] nvarchar(200) NOT NULL,
        [RegistrationNumber] nvarchar(100) NOT NULL,
        [EnterpriseType] nvarchar(120) NOT NULL,
        [EnterpriseStatus] nvarchar(120) NOT NULL,
        [RegistrationDate] nvarchar(max) NOT NULL,
        [BusinessStartDate] nvarchar(max) NOT NULL,
        [FinancialYearEnd] nvarchar(max) NOT NULL,
        [MainBusinessObject] nvarchar(max) NOT NULL,
        [PostalAddress] nvarchar(max) NOT NULL,
        [RegisteredOfficeAddress] nvarchar(max) NOT NULL,
        [BankName] nvarchar(max) NOT NULL,
        [BankAccountName] nvarchar(max) NOT NULL,
        [BankAccountNumber] nvarchar(max) NOT NULL,
        [BankBranchCode] nvarchar(max) NOT NULL,
        [BankAccountType] nvarchar(max) NOT NULL,
        [ContactAddress] nvarchar(max) NOT NULL,
        [ContactPhone1] nvarchar(max) NOT NULL,
        [ContactPhone2] nvarchar(max) NOT NULL,
        [ContactPhone3] nvarchar(max) NOT NULL,
        [ContactEmailInfo] nvarchar(max) NOT NULL,
        [ContactEmailClaims] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PublicSiteSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424123941_AddPublicContentManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424123941_AddPublicContentManagement', N'8.0.0');
END;
GO

COMMIT;
GO

