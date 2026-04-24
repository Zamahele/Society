BEGIN TRANSACTION;
GO

CREATE TABLE [society].[CommitteeMembers] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(150) NOT NULL,
    [RoleTitle] nvarchar(80) NOT NULL,
    [Phone] nvarchar(40) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_CommitteeMembers] PRIMARY KEY ([Id])
);
GO

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
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424123941_AddPublicContentManagement', N'8.0.0');
GO

COMMIT;
GO

