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

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

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
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

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
GO

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
GO

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
GO

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
GO

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
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_DeathClaims_DependantId] ON [DeathClaims] ([DependantId]);
GO

CREATE INDEX [IX_DeathClaims_MembershipId] ON [DeathClaims] ([MembershipId]);
GO

CREATE INDEX [IX_DeathClaims_ProcessedByAdminId] ON [DeathClaims] ([ProcessedByAdminId]);
GO

CREATE INDEX [IX_JoiningFeePayments_ConfirmedByClerkId] ON [JoiningFeePayments] ([ConfirmedByClerkId]);
GO

CREATE INDEX [IX_JoiningFeePayments_MembershipId] ON [JoiningFeePayments] ([MembershipId]);
GO

CREATE INDEX [IX_MemberDependants_MembershipId] ON [MemberDependants] ([MembershipId]);
GO

CREATE UNIQUE INDEX [IX_Memberships_MembershipNumber] ON [Memberships] ([MembershipNumber]);
GO

CREATE INDEX [IX_Memberships_UserId] ON [Memberships] ([UserId]);
GO

CREATE INDEX [IX_MonthlyPayments_ConfirmedByClerkId] ON [MonthlyPayments] ([ConfirmedByClerkId]);
GO

CREATE INDEX [IX_MonthlyPayments_MembershipId] ON [MonthlyPayments] ([MembershipId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260402134900_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'society') IS NULL EXEC(N'CREATE SCHEMA [society];');
GO

ALTER SCHEMA [society] TRANSFER [MonthlyPayments];
GO

ALTER SCHEMA [society] TRANSFER [Memberships];
GO

ALTER SCHEMA [society] TRANSFER [MemberDependants];
GO

ALTER SCHEMA [society] TRANSFER [JoiningFeePayments];
GO

ALTER SCHEMA [society] TRANSFER [DeathClaims];
GO

ALTER SCHEMA [society] TRANSFER [AspNetUserTokens];
GO

ALTER SCHEMA [society] TRANSFER [AspNetUsers];
GO

ALTER SCHEMA [society] TRANSFER [AspNetUserRoles];
GO

ALTER SCHEMA [society] TRANSFER [AspNetUserLogins];
GO

ALTER SCHEMA [society] TRANSFER [AspNetUserClaims];
GO

ALTER SCHEMA [society] TRANSFER [AspNetRoles];
GO

ALTER SCHEMA [society] TRANSFER [AspNetRoleClaims];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260411123947_AddSocietySchema', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [society].[MonthlyPayments] ADD [SubmittedByClerkId] nvarchar(450) NULL;
GO

ALTER TABLE [society].[JoiningFeePayments] ADD [SubmittedByClerkId] nvarchar(450) NULL;
GO

ALTER TABLE [society].[DeathClaims] ADD [SubmittedByClerkId] nvarchar(450) NULL;
GO

CREATE INDEX [IX_MonthlyPayments_SubmittedByClerkId] ON [society].[MonthlyPayments] ([SubmittedByClerkId]);
GO

CREATE INDEX [IX_JoiningFeePayments_SubmittedByClerkId] ON [society].[JoiningFeePayments] ([SubmittedByClerkId]);
GO

CREATE INDEX [IX_DeathClaims_SubmittedByClerkId] ON [society].[DeathClaims] ([SubmittedByClerkId]);
GO

ALTER TABLE [society].[DeathClaims] ADD CONSTRAINT [FK_DeathClaims_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [society].[JoiningFeePayments] ADD CONSTRAINT [FK_JoiningFeePayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [society].[MonthlyPayments] ADD CONSTRAINT [FK_MonthlyPayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260411140504_AddClerkSubmittedBy', N'8.0.0');
GO

COMMIT;
GO

