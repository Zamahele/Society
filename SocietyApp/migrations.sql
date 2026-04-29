ALTER TABLE [society].[JoiningFeePayments] ADD [ProofData] varbinary(max) NULL;
ALTER TABLE [society].[JoiningFeePayments] ADD [ProofFileName] nvarchar(max) NULL;
ALTER TABLE [society].[MonthlyPayments] ADD [ProofData] varbinary(max) NULL;
ALTER TABLE [society].[MonthlyPayments] ADD [ProofFileName] nvarchar(max) NULL;
