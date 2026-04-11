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

ALTER TABLE [society].[DeathClaims] ADD CONSTRAINT [FK_DeathClaims_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]);
GO

ALTER TABLE [society].[JoiningFeePayments] ADD CONSTRAINT [FK_JoiningFeePayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [society].[MonthlyPayments] ADD CONSTRAINT [FK_MonthlyPayments_AspNetUsers_SubmittedByClerkId] FOREIGN KEY ([SubmittedByClerkId]) REFERENCES [society].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260411141310_AddClerkSubmittedBy', N'8.0.0');
GO

COMMIT;
GO

