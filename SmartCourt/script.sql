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
    WHERE [MigrationId] = N'20260705150656_InitialTestCreate'
)
BEGIN
    CREATE TABLE [TestEntities] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TestEntities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705150656_InitialTestCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705150656_InitialTestCreate', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165109_AddBaseEntityAuditing'
)
BEGIN
    ALTER TABLE [TestEntities] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165109_AddBaseEntityAuditing'
)
BEGIN
    ALTER TABLE [TestEntities] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165109_AddBaseEntityAuditing'
)
BEGIN
    ALTER TABLE [TestEntities] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165109_AddBaseEntityAuditing'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705165109_AddBaseEntityAuditing', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165455_AddAuditableEntityFields'
)
BEGIN
    ALTER TABLE [TestEntities] ADD [CreatedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165455_AddAuditableEntityFields'
)
BEGIN
    ALTER TABLE [TestEntities] ADD [LastModifiedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165455_AddAuditableEntityFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705165455_AddAuditableEntityFields', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705165837_MoveAuditFieldsToAuditableEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705165837_MoveAuditFieldsToAuditableEntity', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713140017_AddSampleEntity'
)
BEGIN
    CREATE TABLE [SampleEntities] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_SampleEntities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713140017_AddSampleEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713140017_AddSampleEntity', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [NationalNumber] varchar(14) NOT NULL,
        [Gender] varchar(20) NULL,
        [DateOfBirth] date NULL,
        [Address] nvarchar(500) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [UserName] nvarchar(256) NOT NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NOT NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(20) NULL,
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
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
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
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ApplicationUser_Email] ON [AspNetUsers] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers] ([NationalNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    CREATE INDEX [IX_ApplicationUser_Status] ON [AspNetUsers] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715173748_IdentityTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715173748_IdentityTables', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716062636_AddRefreshTokensTable'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(max) NOT NULL,
        [ExpiresOn] datetime2 NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [RevokedOn] datetime2 NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([UserId], [Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716062636_AddRefreshTokensTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716062636_AddRefreshTokensTable', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716101442_AddUserProfiles'
)
BEGIN
    CREATE TABLE [ClientProfile] (
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ClientProfile] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_ClientProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716101442_AddUserProfiles'
)
BEGIN
    CREATE TABLE [LawyerProfile] (
        [UserId] uniqueidentifier NOT NULL,
        [Specialization] nvarchar(150) NOT NULL,
        [YearsOfExperience] int NOT NULL,
        [Bio] nvarchar(500) NULL,
        [Address] nvarchar(255) NULL,
        CONSTRAINT [PK_LawyerProfile] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_LawyerProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716101442_AddUserProfiles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716101442_AddUserProfiles', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145349_adding the user verification related tables'
)
BEGIN
    CREATE TABLE [StoredFiles] (
        [Id] uniqueidentifier NOT NULL,
        [StoredFileName] nvarchar(max) NOT NULL,
        [OriginalFileName] nvarchar(max) NOT NULL,
        [FileUrl] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [Extension] nvarchar(max) NOT NULL,
        [SizeInBytes] bigint NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StoredFiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145349_adding the user verification related tables'
)
BEGIN
    CREATE TABLE [UserVerificationDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [DocumentType] tinyint NOT NULL,
        [Status] tinyint NOT NULL,
        [ExpirationDate] date NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [VerifiedByAdminId] nvarchar(max) NULL,
        [RejectionReason] nvarchar(max) NULL,
        [IsCurrent] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserVerificationDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserVerificationDocuments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserVerificationDocuments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145349_adding the user verification related tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserVerificationDocuments_StoredFileId] ON [UserVerificationDocuments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145349_adding the user verification related tables'
)
BEGIN
    CREATE INDEX [IX_UserVerificationDocuments_UserId] ON [UserVerificationDocuments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145349_adding the user verification related tables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717145349_adding the user verification related tables', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerProfile]') AND [c].[name] = N'Specialization');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [LawyerProfile] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [LawyerProfile] DROP COLUMN [Specialization];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD [IsAvailable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD [SpecializationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ProfilePictureUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    CREATE TABLE [LegalCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LegalCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    CREATE TABLE [LegalSpecializations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LegalSpecializations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LegalSpecializations_LegalCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [LegalCategories] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    CREATE INDEX [IX_LawyerProfile_SpecializationId] ON [LawyerProfile] ([SpecializationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    CREATE INDEX [IX_LegalSpecializations_CategoryId] ON [LegalSpecializations] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD CONSTRAINT [FK_LawyerProfile_LegalSpecializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [LegalSpecializations] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718144707_AddLegalCategoriesAndSpecializations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718144707_AddLegalCategoriesAndSpecializations', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718151254_AddLawyerLevel'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD [Level] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718151254_AddLawyerLevel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718151254_AddLawyerLevel', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718160232_RemoveTestEntities'
)
BEGIN
    DROP TABLE [SampleEntities];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718160232_RemoveTestEntities'
)
BEGIN
    DROP TABLE [TestEntities];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718160232_RemoveTestEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718160232_RemoveTestEntities', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181754_StoreHasedRefreshToken'
)
BEGIN
    EXEC sp_rename N'[RefreshTokens].[Token]', N'HashedToken', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181754_StoreHasedRefreshToken'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181754_StoreHasedRefreshToken'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Government] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181754_StoreHasedRefreshToken'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181754_StoreHasedRefreshToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260719181754_StoreHasedRefreshToken', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723233804_AddLawDocuments'
)
BEGIN
    CREATE TABLE [LawDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [DocumentTitle] nvarchar(255) NOT NULL,
        [Language] nvarchar(10) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [TotalPages] int NOT NULL,
        [ChunkCount] int NOT NULL,
        [FileStoragePath] nvarchar(max) NULL,
        [ProcessingStartedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [Version] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_LawDocuments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723233804_AddLawDocuments'
)
BEGIN
    CREATE INDEX [IX_LawDocuments_DocumentTitle] ON [LawDocuments] ([DocumentTitle]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723233804_AddLawDocuments'
)
BEGIN
    CREATE INDEX [IX_LawDocuments_Language] ON [LawDocuments] ([Language]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723233804_AddLawDocuments'
)
BEGIN
    CREATE INDEX [IX_LawDocuments_Status] ON [LawDocuments] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723233804_AddLawDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723233804_AddLawDocuments', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724174335_AddRowVersionToUserVerificationDocument'
)
BEGIN
    ALTER TABLE [UserVerificationDocuments] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724174335_AddRowVersionToUserVerificationDocument'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724174335_AddRowVersionToUserVerificationDocument', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE TABLE [Cases] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [Status] tinyint NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Cases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cases_ClientProfile_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [ClientProfile] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE TABLE [CaseDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [CaseId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CaseDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CaseDocuments_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CaseDocuments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE TABLE [CaseProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [CaseId] uniqueidentifier NOT NULL,
        [Specialization] tinyint NOT NULL,
        [RequiredLawyerLevelId] tinyint NOT NULL,
        [Complexity] tinyint NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CaseProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CaseProfiles_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE TABLE [CaseReviewReports] (
        [Id] uniqueidentifier NOT NULL,
        [IsLatest] bit NOT NULL,
        [CaseId] uniqueidentifier NOT NULL,
        [CaseComplexity] tinyint NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CaseReviewReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CaseReviewReports_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE TABLE [ReviewPoints] (
        [Id] uniqueidentifier NOT NULL,
        [CaseReviewReportId] uniqueidentifier NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Type] tinyint NOT NULL,
        CONSTRAINT [PK_ReviewPoints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReviewPoints_CaseReviewReports_CaseReviewReportId] FOREIGN KEY ([CaseReviewReportId]) REFERENCES [CaseReviewReports] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE INDEX [IX_CaseDocuments_CaseId] ON [CaseDocuments] ([CaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CaseDocuments_StoredFileId] ON [CaseDocuments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CaseProfiles_CaseId] ON [CaseProfiles] ([CaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE INDEX [IX_CaseReviewReports_CaseId] ON [CaseReviewReports] ([CaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE INDEX [IX_Cases_ClientId] ON [Cases] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    CREATE INDEX [IX_ReviewPoints_CaseReviewReportId] ON [ReviewPoints] ([CaseReviewReportId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726123951_adding the case related entities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726123951_adding the case related entities', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [Contracts] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [LegalCaseId] uniqueidentifier NOT NULL,
        [ClientUserId] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [TermsAndConditions] nvarchar(max) NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [Status] int NOT NULL,
        [AcceptedByClientAt] datetime2 NULL,
        [AcceptedByLawyerAt] datetime2 NULL,
        [ActivatedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [TerminatedAt] datetime2 NULL,
        [TerminationReason] nvarchar(2000) NULL,
        [TerminatedByUserId] uniqueidentifier NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Contracts_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_Contracts_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
        CONSTRAINT [FK_Contracts_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Contracts_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Contracts_AspNetUsers_TerminatedByUserId] FOREIGN KEY ([TerminatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [IdempotencyRecords] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Key] varchar(200) NOT NULL,
        [Operation] varchar(200) NOT NULL,
        [ResourceType] varchar(100) NOT NULL,
        [ResourceId] uniqueidentifier NOT NULL,
        [RequestHash] varchar(128) NOT NULL,
        [Status] int NOT NULL,
        [ResponseStatusCode] int NULL,
        [ResponseBody] nvarchar(max) NULL,
        [ResultReferenceId] uniqueidentifier NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_IdempotencyRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_IdempotencyRecords_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
        CONSTRAINT [FK_IdempotencyRecords_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [LawyerWallets] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [PendingBalance] decimal(18,2) NOT NULL,
        [AvailableBalance] decimal(18,2) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LawyerWallets] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LawyerWallets_Balances_NonNegative] CHECK ([PendingBalance] >= 0 AND [AvailableBalance] >= 0),
        CONSTRAINT [CK_LawyerWallets_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [FK_LawyerWallets_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [OutboxMessages] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] varchar(200) NOT NULL,
        [EventVersion] int NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [AggregateType] varchar(100) NOT NULL,
        [AggregateId] uniqueidentifier NOT NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [Attempts] int NOT NULL,
        [LastError] nvarchar(2000) NULL,
        [AvailableAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OutboxMessages_Attempts_NonNegative] CHECK ([Attempts] >= 0),
        CONSTRAINT [CK_OutboxMessages_EventVersion_Positive] CHECK ([EventVersion] > 0),
        CONSTRAINT [CK_OutboxMessages_Status_Range] CHECK ([Status] BETWEEN 0 AND 3)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [WithdrawalRequests] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [Status] int NOT NULL,
        [ProviderTransactionId] varchar(200) NULL,
        [FailureReason] nvarchar(2000) NULL,
        [RequestedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [IdempotencyKey] varchar(200) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_WithdrawalRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_WithdrawalRequests_Amount_Positive] CHECK ([Amount] > 0),
        CONSTRAINT [CK_WithdrawalRequests_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_WithdrawalRequests_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
        CONSTRAINT [FK_WithdrawalRequests_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [ContractAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [UploadedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ContractAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContractAttachments_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ContractAttachments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ContractAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [ContractStateHistories] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [PreviousStatus] int NULL,
        [NewStatus] int NOT NULL,
        [Trigger] nvarchar(100) NOT NULL,
        [ActorUserId] uniqueidentifier NULL,
        [Reason] nvarchar(2000) NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ContractStateHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ContractStateHistories_NewStatus_Range] CHECK ([NewStatus] BETWEEN 0 AND 4),
        CONSTRAINT [CK_ContractStateHistories_PreviousStatus_Range] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 4),
        CONSTRAINT [FK_ContractStateHistories_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ContractStateHistories_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [EscrowAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [TotalDeposited] decimal(18,2) NOT NULL,
        [TotalReleased] decimal(18,2) NOT NULL,
        [TotalRefunded] decimal(18,2) NOT NULL,
        [TotalFees] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EscrowAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_EscrowAccounts_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_EscrowAccounts_NonNegativeTotals] CHECK ([TotalDeposited] >= 0 AND [TotalReleased] >= 0 AND [TotalRefunded] >= 0 AND [TotalFees] >= 0),
        CONSTRAINT [CK_EscrowAccounts_Status_Range] CHECK ([Status] BETWEEN 0 AND 1),
        CONSTRAINT [FK_EscrowAccounts_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [Milestones] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [OrderNumber] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [DurationDays] int NULL,
        [DueDate] datetime2 NULL,
        [Status] int NOT NULL,
        [AcceptedByClientAt] datetime2 NULL,
        [AcceptedByLawyerAt] datetime2 NULL,
        [ReadyForFundingAt] datetime2 NULL,
        [FundedAt] datetime2 NULL,
        [SubmittedAt] datetime2 NULL,
        [AutoAcceptEligibleAt] datetime2 NULL,
        [AutoAcceptJobId] nvarchar(100) NULL,
        [AcceptedAt] datetime2 NULL,
        [AcceptanceSource] int NULL,
        [HoldStartsAt] datetime2 NULL,
        [HoldExpiresAt] datetime2 NULL,
        [ReleasedAt] datetime2 NULL,
        [RefundedAt] datetime2 NULL,
        [RejectionReason] nvarchar(2000) NULL,
        [SubmissionVersion] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Milestones] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Milestones_Amount_Positive] CHECK ([Amount] > 0),
        CONSTRAINT [CK_Milestones_DurationDays_Range] CHECK ([DurationDays] IS NULL OR [DurationDays] BETWEEN 1 AND 365),
        CONSTRAINT [CK_Milestones_OrderNumber_Positive] CHECK ([OrderNumber] > 0),
        CONSTRAINT [CK_Milestones_Status_Range] CHECK ([Status] BETWEEN 0 AND 9),
        CONSTRAINT [CK_Milestones_SubmissionVersion_Positive] CHECK ([SubmissionVersion] >= 0),
        CONSTRAINT [FK_Milestones_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [Disputes] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NOT NULL,
        [RaisedByUserId] uniqueidentifier NOT NULL,
        [AssignedModeratorUserId] uniqueidentifier NULL,
        [Category] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [RequestedOutcome] int NOT NULL,
        [ResolutionType] int NULL,
        [ResolutionAmount] decimal(18,2) NULL,
        [ResolutionSummary] nvarchar(2000) NULL,
        [ResolvedByUserId] uniqueidentifier NULL,
        [ResolvedAt] datetime2 NULL,
        [ClosedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Disputes] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Disputes_Category_Range] CHECK ([Category] BETWEEN 0 AND 3),
        CONSTRAINT [CK_Disputes_RequestedOutcome_Range] CHECK ([RequestedOutcome] BETWEEN 0 AND 2),
        CONSTRAINT [CK_Disputes_ResolutionType_Range] CHECK ([ResolutionType] IS NULL OR [ResolutionType] BETWEEN 0 AND 2),
        CONSTRAINT [CK_Disputes_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
        CONSTRAINT [FK_Disputes_AspNetUsers_AssignedModeratorUserId] FOREIGN KEY ([AssignedModeratorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Disputes_AspNetUsers_RaisedByUserId] FOREIGN KEY ([RaisedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Disputes_AspNetUsers_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Disputes_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Disputes_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [EscrowHolds] (
        [Id] uniqueidentifier NOT NULL,
        [EscrowAccountId] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NOT NULL,
        [GrossAmount] decimal(18,2) NOT NULL,
        [PlatformFeeAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [FundedAt] datetime2 NOT NULL,
        [HoldStartsAt] datetime2 NULL,
        [HoldExpiresAt] datetime2 NULL,
        [FrozenAt] datetime2 NULL,
        [SettledAt] datetime2 NULL,
        [SettlementType] int NULL,
        [ProviderDepositTransactionId] uniqueidentifier NOT NULL,
        [ProviderReleaseTransactionId] uniqueidentifier NULL,
        [ProviderRefundTransactionId] uniqueidentifier NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EscrowHolds] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_EscrowHolds_FeesAndNet_NonNegative] CHECK ([PlatformFeeAmount] >= 0 AND [NetAmount] >= 0),
        CONSTRAINT [CK_EscrowHolds_FundedStateRequiresTimestamp] CHECK ([Status] <> 0 OR [FundedAt] IS NOT NULL),
        CONSTRAINT [CK_EscrowHolds_GrossAmount_Positive] CHECK ([GrossAmount] > 0),
        CONSTRAINT [CK_EscrowHolds_Reconciliation] CHECK ([GrossAmount] = [PlatformFeeAmount] + [NetAmount]),
        CONSTRAINT [CK_EscrowHolds_Status_Range] CHECK ([Status] BETWEEN 0 AND 3),
        CONSTRAINT [FK_EscrowHolds_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscrowHolds_EscrowAccounts_EscrowAccountId] FOREIGN KEY ([EscrowAccountId]) REFERENCES [EscrowAccounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscrowHolds_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [MilestoneChangeRequests] (
        [Id] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NOT NULL,
        [RequestedByUserId] uniqueidentifier NOT NULL,
        [ProposedDescription] nvarchar(max) NULL,
        [ProposedDurationDays] int NULL,
        [ProposedDueDate] datetime2 NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [Status] int NOT NULL,
        [DecidedByUserId] uniqueidentifier NULL,
        [DecidedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MilestoneChangeRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MilestoneChangeRequests_DurationDays_Range] CHECK ([ProposedDurationDays] IS NULL OR [ProposedDurationDays] BETWEEN 1 AND 365),
        CONSTRAINT [CK_MilestoneChangeRequests_Status_Range] CHECK ([Status] BETWEEN 0 AND 3),
        CONSTRAINT [FK_MilestoneChangeRequests_AspNetUsers_DecidedByUserId] FOREIGN KEY ([DecidedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MilestoneChangeRequests_AspNetUsers_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MilestoneChangeRequests_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [MilestoneStateHistories] (
        [Id] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NOT NULL,
        [PreviousStatus] int NULL,
        [NewStatus] int NOT NULL,
        [Trigger] nvarchar(100) NOT NULL,
        [ActorUserId] uniqueidentifier NULL,
        [Reason] nvarchar(2000) NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MilestoneStateHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MilestoneStateHistories_NewStatus_Range] CHECK ([NewStatus] BETWEEN 0 AND 9),
        CONSTRAINT [CK_MilestoneStateHistories_PreviousStatus_Range] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 9),
        CONSTRAINT [FK_MilestoneStateHistories_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MilestoneStateHistories_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [DisputeEvidence] (
        [Id] uniqueidentifier NOT NULL,
        [DisputeId] uniqueidentifier NOT NULL,
        [UploadedByUserId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NULL,
        [Content] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DisputeEvidence] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DisputeEvidence_FileOrContent] CHECK ([StoredFileId] IS NOT NULL OR [Content] IS NOT NULL),
        CONSTRAINT [FK_DisputeEvidence_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DisputeEvidence_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DisputeEvidence_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [DisputeResolutions] (
        [Id] uniqueidentifier NOT NULL,
        [DisputeId] uniqueidentifier NOT NULL,
        [ResolutionType] int NOT NULL,
        [GrossHoldAmount] decimal(18,2) NOT NULL,
        [ClientRefundAmount] decimal(18,2) NOT NULL,
        [LawyerReleaseAmount] decimal(18,2) NOT NULL,
        [PlatformFeeAmount] decimal(18,2) NOT NULL,
        [Summary] nvarchar(2000) NOT NULL,
        [ResolvedByUserId] uniqueidentifier NOT NULL,
        [ResolvedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DisputeResolutions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DisputeResolutions_Amounts_NonNegative] CHECK ([GrossHoldAmount] >= 0 AND [ClientRefundAmount] >= 0 AND [LawyerReleaseAmount] >= 0 AND [PlatformFeeAmount] >= 0),
        CONSTRAINT [CK_DisputeResolutions_Reconciliation] CHECK ([GrossHoldAmount] = [ClientRefundAmount] + [LawyerReleaseAmount] + [PlatformFeeAmount]),
        CONSTRAINT [CK_DisputeResolutions_ResolutionType_Range] CHECK ([ResolutionType] BETWEEN 0 AND 2),
        CONSTRAINT [FK_DisputeResolutions_AspNetUsers_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DisputeResolutions_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [LawyerPenalties] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [DisputeId] uniqueidentifier NOT NULL,
        [PenaltyType] int NOT NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [StartsAt] datetime2 NOT NULL,
        [EndsAt] datetime2 NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LawyerPenalties] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LawyerPenalties_EndAfterStart] CHECK ([EndsAt] IS NULL OR [EndsAt] >= [StartsAt]),
        CONSTRAINT [CK_LawyerPenalties_Type_Range] CHECK ([PenaltyType] BETWEEN 0 AND 3),
        CONSTRAINT [FK_LawyerPenalties_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LawyerPenalties_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LawyerPenalties_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [MilestoneSubmissions] (
        [Id] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NOT NULL,
        [EscrowHoldId] uniqueidentifier NOT NULL,
        [SubmittedByUserId] uniqueidentifier NOT NULL,
        [Version] int NOT NULL,
        [Notes] nvarchar(max) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MilestoneSubmissions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MilestoneSubmissions_Version_Positive] CHECK ([Version] > 0),
        CONSTRAINT [FK_MilestoneSubmissions_AspNetUsers_SubmittedByUserId] FOREIGN KEY ([SubmittedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MilestoneSubmissions_EscrowHolds_EscrowHoldId] FOREIGN KEY ([EscrowHoldId]) REFERENCES [EscrowHolds] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MilestoneSubmissions_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [PaymentTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [MilestoneId] uniqueidentifier NULL,
        [EscrowHoldId] uniqueidentifier NULL,
        [OperationType] int NOT NULL,
        [ProviderName] varchar(100) NOT NULL,
        [ProviderTransactionId] varchar(200) NULL,
        [IdempotencyKey] varchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [Status] int NOT NULL,
        [FailureReason] nvarchar(2000) NULL,
        [ProcessedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PaymentTransactions_Amount_Positive] CHECK ([Amount] > 0),
        CONSTRAINT [CK_PaymentTransactions_CompletedDepositRequiresHold] CHECK (NOT ([OperationType] = 0 AND [Status] = 1) OR [EscrowHoldId] IS NOT NULL),
        CONSTRAINT [CK_PaymentTransactions_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_PaymentTransactions_MilestoneRequiredForMoneyOperations] CHECK ([OperationType] = 3 OR [MilestoneId] IS NOT NULL),
        CONSTRAINT [CK_PaymentTransactions_OperationType_Range] CHECK ([OperationType] BETWEEN 0 AND 3),
        CONSTRAINT [CK_PaymentTransactions_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
        CONSTRAINT [FK_PaymentTransactions_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentTransactions_EscrowHolds_EscrowHoldId] FOREIGN KEY ([EscrowHoldId]) REFERENCES [EscrowHolds] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentTransactions_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [MilestoneSubmissionAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [MilestoneSubmissionId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MilestoneSubmissionAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MilestoneSubmissionAttachments_MilestoneSubmissions_MilestoneSubmissionId] FOREIGN KEY ([MilestoneSubmissionId]) REFERENCES [MilestoneSubmissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MilestoneSubmissionAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE TABLE [EscrowLedgerEntries] (
        [Id] uniqueidentifier NOT NULL,
        [EscrowAccountId] uniqueidentifier NOT NULL,
        [EscrowHoldId] uniqueidentifier NULL,
        [TransactionType] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [RunningBalance] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
        [ReferenceType] varchar(100) NOT NULL,
        [ReferenceId] uniqueidentifier NOT NULL,
        [PaymentTransactionId] uniqueidentifier NULL,
        [Description] nvarchar(2000) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EscrowLedgerEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_EscrowLedgerEntries_Amount_Positive] CHECK ([Amount] > 0),
        CONSTRAINT [CK_EscrowLedgerEntries_Currency_EGP] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_EscrowLedgerEntries_RunningBalance_NonNegative] CHECK ([RunningBalance] >= 0),
        CONSTRAINT [CK_EscrowLedgerEntries_TransactionType_Range] CHECK ([TransactionType] BETWEEN 0 AND 4),
        CONSTRAINT [FK_EscrowLedgerEntries_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscrowLedgerEntries_EscrowAccounts_EscrowAccountId] FOREIGN KEY ([EscrowAccountId]) REFERENCES [EscrowAccounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscrowLedgerEntries_EscrowHolds_EscrowHoldId] FOREIGN KEY ([EscrowHoldId]) REFERENCES [EscrowHolds] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscrowLedgerEntries_PaymentTransactions_PaymentTransactionId] FOREIGN KEY ([PaymentTransactionId]) REFERENCES [PaymentTransactions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_ContractAttachments_ContractId] ON [ContractAttachments] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_ContractAttachments_StoredFileId] ON [ContractAttachments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_ContractAttachments_UploadedByUserId] ON [ContractAttachments] ([UploadedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Contracts_ClientUserId] ON [Contracts] ([ClientUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Contracts_LawyerUserId] ON [Contracts] ([LawyerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Contracts_Status] ON [Contracts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Contracts_TerminatedByUserId] ON [Contracts] ([TerminatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Contracts_ProposalId] ON [Contracts] ([ProposalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_ContractStateHistories_ActorUserId] ON [ContractStateHistories] ([ActorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_ContractStateHistories_ContractId_CreatedAt] ON [ContractStateHistories] ([ContractId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_DisputeEvidence_DisputeId] ON [DisputeEvidence] ([DisputeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_DisputeEvidence_StoredFileId] ON [DisputeEvidence] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_DisputeEvidence_UploadedByUserId] ON [DisputeEvidence] ([UploadedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_DisputeResolutions_ResolvedByUserId] ON [DisputeResolutions] ([ResolvedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_DisputeResolutions_DisputeId] ON [DisputeResolutions] ([DisputeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Disputes_AssignedModeratorUserId] ON [Disputes] ([AssignedModeratorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Disputes_ContractId] ON [Disputes] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Disputes_RaisedByUserId] ON [Disputes] ([RaisedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Disputes_ResolvedByUserId] ON [Disputes] ([ResolvedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Disputes_Status_CreatedAt] ON [Disputes] ([Status], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Disputes_OpenPerMilestone] ON [Disputes] ([MilestoneId], [Status]) WHERE [Status] IN (0, 1, 2)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_EscrowAccounts_ContractId] ON [EscrowAccounts] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowHolds_ContractId] ON [EscrowHolds] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowHolds_EscrowAccountId] ON [EscrowHolds] ([EscrowAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowHolds_HoldExpiresAt_Status] ON [EscrowHolds] ([HoldExpiresAt], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_EscrowHolds_MilestoneId] ON [EscrowHolds] ([MilestoneId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowLedgerEntries_AccountId_CreatedAt] ON [EscrowLedgerEntries] ([EscrowAccountId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowLedgerEntries_CreatedByUserId] ON [EscrowLedgerEntries] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowLedgerEntries_EscrowHoldId] ON [EscrowLedgerEntries] ([EscrowHoldId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_EscrowLedgerEntries_PaymentTransactionId] ON [EscrowLedgerEntries] ([PaymentTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_IdempotencyRecords_Status_ExpiresAt] ON [IdempotencyRecords] ([Status], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_IdempotencyRecords_UserId_Key] ON [IdempotencyRecords] ([UserId], [Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_LawyerPenalties_CreatedByUserId] ON [LawyerPenalties] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_LawyerPenalties_DisputeId] ON [LawyerPenalties] ([DisputeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_LawyerPenalties_LawyerUserId_StartsAt] ON [LawyerPenalties] ([LawyerUserId], [StartsAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_LawyerWallets_LawyerUserId] ON [LawyerWallets] ([LawyerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneChangeRequests_DecidedByUserId] ON [MilestoneChangeRequests] ([DecidedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneChangeRequests_RequestedByUserId] ON [MilestoneChangeRequests] ([RequestedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_MilestoneChangeRequests_Pending] ON [MilestoneChangeRequests] ([MilestoneId], [Status]) WHERE [Status] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Milestones_ContractId_Status] ON [Milestones] ([ContractId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_Milestones_Status_AutoAcceptEligibleAt] ON [Milestones] ([Status], [AutoAcceptEligibleAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Milestones_ContractId_OrderNumber] ON [Milestones] ([ContractId], [OrderNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneStateHistories_ActorUserId] ON [MilestoneStateHistories] ([ActorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneStateHistories_MilestoneId_CreatedAt] ON [MilestoneStateHistories] ([MilestoneId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneSubmissionAttachments_MilestoneSubmissionId] ON [MilestoneSubmissionAttachments] ([MilestoneSubmissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneSubmissionAttachments_StoredFileId] ON [MilestoneSubmissionAttachments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneSubmissions_EscrowHoldId] ON [MilestoneSubmissions] ([EscrowHoldId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_MilestoneSubmissions_SubmittedByUserId] ON [MilestoneSubmissions] ([SubmittedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_MilestoneSubmissions_MilestoneId_Version] ON [MilestoneSubmissions] ([MilestoneId], [Version]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_Aggregate] ON [OutboxMessages] ([AggregateType], [AggregateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_Status_AvailableAt] ON [OutboxMessages] ([Status], [AvailableAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_ContractId_Status] ON [PaymentTransactions] ([ContractId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_EscrowHoldId] ON [PaymentTransactions] ([EscrowHoldId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_MilestoneId_Status] ON [PaymentTransactions] ([MilestoneId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentTransactions_IdempotencyKey] ON [PaymentTransactions] ([IdempotencyKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PaymentTransactions_ProviderTransaction] ON [PaymentTransactions] ([ProviderName], [ProviderTransactionId]) WHERE [ProviderTransactionId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE INDEX [IX_WithdrawalRequests_LawyerUserId_Status] ON [WithdrawalRequests] ([LawyerUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    CREATE UNIQUE INDEX [UX_WithdrawalRequests_IdempotencyKey] ON [WithdrawalRequests] ([IdempotencyKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727160911_ContractAndPaymentV1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727160911_ContractAndPaymentV1', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727171426_AddIdempotencySettlementKey'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_IdempotencyRecords_HoldSettlement] ON [IdempotencyRecords] ([ResourceType], [ResourceId]) WHERE [ResourceType] = ''EscrowHoldSettlement''');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727171426_AddIdempotencySettlementKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727171426_AddIdempotencySettlementKey', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727174544_AddOutboxLeasing'
)
BEGIN
    ALTER TABLE [OutboxMessages] ADD [LeaseExpiresAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727174544_AddOutboxLeasing'
)
BEGIN
    ALTER TABLE [OutboxMessages] ADD [LeaseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727174544_AddOutboxLeasing'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727174544_AddOutboxLeasing', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728120000_AddMilestoneChangeRequestDecisionReason'
)
BEGIN
    ALTER TABLE [MilestoneChangeRequests] ADD [DecisionReason] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728120000_AddMilestoneChangeRequestDecisionReason'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728120000_AddMilestoneChangeRequestDecisionReason', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE TABLE [LegalCases] (
        [Id] uniqueidentifier NOT NULL,
        [ClientUserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CaseLocation] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [FinalSubmittedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LegalCases] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LegalCases_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
        CONSTRAINT [FK_LegalCases_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE TABLE [Proposals] (
        [Id] uniqueidentifier NOT NULL,
        [LegalCaseId] uniqueidentifier NOT NULL,
        [ClientUserId] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Proposals] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Proposals_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
        CONSTRAINT [FK_Proposals_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_LegalCases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [LegalCases] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE INDEX [IX_Contracts_LegalCaseId] ON [Contracts] ([LegalCaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE INDEX [IX_LegalCases_ClientUserId_Status] ON [LegalCases] ([ClientUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE INDEX [IX_Proposals_ClientUserId_LawyerUserId_Status] ON [Proposals] ([ClientUserId], [LawyerUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE INDEX [IX_Proposals_LawyerUserId] ON [Proposals] ([LawyerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    CREATE INDEX [IX_Proposals_LegalCaseId_Status] ON [Proposals] ([LegalCaseId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    ALTER TABLE [Contracts] ADD CONSTRAINT [FK_Contracts_LegalCases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [LegalCases] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    ALTER TABLE [Contracts] ADD CONSTRAINT [FK_Contracts_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729072405_AddContractCreationPrerequisites'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729072405_AddContractCreationPrerequisites', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729093314_AddPaymentWebhookEvents'
)
BEGIN
    CREATE TABLE [PaymentWebhookEvents] (
        [Id] uniqueidentifier NOT NULL,
        [EventId] varchar(200) NOT NULL,
        [PaymentTransactionId] uniqueidentifier NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentWebhookEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentWebhookEvents_PaymentTransactions_PaymentTransactionId] FOREIGN KEY ([PaymentTransactionId]) REFERENCES [PaymentTransactions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729093314_AddPaymentWebhookEvents'
)
BEGIN
    CREATE INDEX [IX_PaymentWebhookEvents_PaymentTransactionId] ON [PaymentWebhookEvents] ([PaymentTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729093314_AddPaymentWebhookEvents'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentWebhookEvents_EventId] ON [PaymentWebhookEvents] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729093314_AddPaymentWebhookEvents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729093314_AddPaymentWebhookEvents', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    DROP INDEX [IX_Proposals_ClientUserId_LawyerUserId_Status] ON [Proposals];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    ALTER TABLE [Proposals] ADD [DecisionReason] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    ALTER TABLE [Proposals] ADD [Message] nvarchar(2000) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    ALTER TABLE [Proposals] ADD [RespondedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    CREATE INDEX [IX_Proposals_ClientUserId] ON [Proposals] ([ClientUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Proposals_LegalCaseId] ON [Proposals] ([LegalCaseId]) WHERE [Status] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Proposals_LegalCaseId_LawyerUserId] ON [Proposals] ([LegalCaseId], [LawyerUserId]) WHERE [Status] IN (0, 1)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730121329_AddProposalWorkflowFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730121329_AddProposalWorkflowFields', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE TABLE [ChatConversations] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [LegalCaseId] uniqueidentifier NOT NULL,
        [ClientUserId] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastMessageAt] datetime2 NULL,
        [IsClosed] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ChatConversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatConversations_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatConversations_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatConversations_LegalCases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [LegalCases] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatConversations_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [SenderUserId] uniqueidentifier NULL,
        [Type] int NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [SystemCode] nvarchar(100) NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChatMessages_Type_Range] CHECK ([Type] BETWEEN 1 AND 2),
        CONSTRAINT [CK_ChatMessages_UserOrSystem] CHECK (([Type] = 1 AND [SenderUserId] IS NOT NULL AND [SystemCode] IS NULL) OR ([Type] = 2 AND [SenderUserId] IS NULL AND [SystemCode] IS NOT NULL)),
        CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatMessages_ChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [ChatConversations] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_Client_UpdatedAt] ON [ChatConversations] ([ClientUserId], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_Lawyer_UpdatedAt] ON [ChatConversations] ([LawyerUserId], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_LegalCaseId] ON [ChatConversations] ([LegalCaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ChatConversations_ProposalId] ON [ChatConversations] ([ProposalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_Conversation_CreatedAt] ON [ChatMessages] ([ConversationId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_SenderUserId] ON [ChatMessages] ([SenderUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730123843_AddChatConversations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730123843_AddChatConversations', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801084814_ExpandDisputeCategoryRange'
)
BEGIN
    ALTER TABLE [Disputes] DROP CONSTRAINT [CK_Disputes_Category_Range];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801084814_ExpandDisputeCategoryRange'
)
BEGIN
    EXEC(N'ALTER TABLE [Disputes] ADD CONSTRAINT [CK_Disputes_Category_Range] CHECK ([Category] BETWEEN 0 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801084814_ExpandDisputeCategoryRange'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801084814_ExpandDisputeCategoryRange', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801100735_AddContractFileAccessAudits'
)
BEGIN
    CREATE TABLE [ContractFileAccessAudits] (
        [Id] uniqueidentifier NOT NULL,
        [ActorUserId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [Purpose] int NOT NULL,
        [RelatedEntityId] uniqueidentifier NOT NULL,
        [ModeratorAccess] bit NOT NULL,
        [AccessedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ContractFileAccessAudits] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ContractFileAccessAudits_Purpose_Range] CHECK ([Purpose] BETWEEN 1 AND 3),
        CONSTRAINT [FK_ContractFileAccessAudits_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801100735_AddContractFileAccessAudits'
)
BEGIN
    CREATE INDEX [IX_ContractFileAccessAudits_ActorUserId] ON [ContractFileAccessAudits] ([ActorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801100735_AddContractFileAccessAudits'
)
BEGIN
    CREATE INDEX [IX_ContractFileAccessAudits_File_Entity_Time] ON [ContractFileAccessAudits] ([StoredFileId], [RelatedEntityId], [AccessedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801100735_AddContractFileAccessAudits'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801100735_AddContractFileAccessAudits', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE TABLE [WalletAdjustments] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerWalletId] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NOT NULL,
        [EscrowAccountId] uniqueidentifier NOT NULL,
        [LedgerEntryId] uniqueidentifier NOT NULL,
        [PendingBalanceDelta] decimal(18,2) NOT NULL,
        [AvailableBalanceDelta] decimal(18,2) NOT NULL,
        [PendingBalanceBefore] decimal(18,2) NOT NULL,
        [PendingBalanceAfter] decimal(18,2) NOT NULL,
        [AvailableBalanceBefore] decimal(18,2) NOT NULL,
        [AvailableBalanceAfter] decimal(18,2) NOT NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WalletAdjustments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_WalletAdjustments_Balances_NonNegative] CHECK ([PendingBalanceBefore] >= 0 AND [PendingBalanceAfter] >= 0 AND [AvailableBalanceBefore] >= 0 AND [AvailableBalanceAfter] >= 0),
        CONSTRAINT [CK_WalletAdjustments_Delta_NonZero] CHECK ([PendingBalanceDelta] <> 0 OR [AvailableBalanceDelta] <> 0),
        CONSTRAINT [FK_WalletAdjustments_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WalletAdjustments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WalletAdjustments_EscrowAccounts_EscrowAccountId] FOREIGN KEY ([EscrowAccountId]) REFERENCES [EscrowAccounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WalletAdjustments_EscrowLedgerEntries_LedgerEntryId] FOREIGN KEY ([LedgerEntryId]) REFERENCES [EscrowLedgerEntries] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WalletAdjustments_LawyerWallets_LawyerWalletId] FOREIGN KEY ([LawyerWalletId]) REFERENCES [LawyerWallets] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE INDEX [IX_WalletAdjustments_ContractId] ON [WalletAdjustments] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE INDEX [IX_WalletAdjustments_CreatedByUserId] ON [WalletAdjustments] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE INDEX [IX_WalletAdjustments_EscrowAccountId] ON [WalletAdjustments] ([EscrowAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WalletAdjustments_LedgerEntryId] ON [WalletAdjustments] ([LedgerEntryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    CREATE INDEX [IX_WalletAdjustments_WalletId_CreatedAt] ON [WalletAdjustments] ([LawyerWalletId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801102821_AddWalletAdjustments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801102821_AddWalletAdjustments', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [LawyerProfile] DROP CONSTRAINT [FK_LawyerProfile_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [LawyerProfile] DROP CONSTRAINT [FK_LawyerProfile_LegalSpecializations_SpecializationId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [LawyerProfile] DROP CONSTRAINT [PK_LawyerProfile];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DROP INDEX [IX_LawyerProfile_SpecializationId] ON [LawyerProfile];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerProfile]') AND [c].[name] = N'Address');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [LawyerProfile] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [LawyerProfile] DROP COLUMN [Address];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerProfile]') AND [c].[name] = N'SpecializationId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [LawyerProfile] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [LawyerProfile] DROP COLUMN [SpecializationId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerProfile]') AND [c].[name] = N'YearsOfExperience');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [LawyerProfile] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [LawyerProfile] DROP COLUMN [YearsOfExperience];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseReviewReports]') AND [c].[name] = N'CaseComplexity');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [CaseReviewReports] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [CaseReviewReports] DROP COLUMN [CaseComplexity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    EXEC sp_rename N'[LawyerProfile]', N'lawyerProfile';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    EXEC sp_rename N'[AspNetUsers].[Government]', N'Governorate', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [lawyerProfile] ADD [AverageRating] decimal(3,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [lawyerProfile] ADD [AverageResponseTimeHours] decimal(10,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [Cases] ADD [City] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [Cases] ADD [Governorate] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseProfiles]') AND [c].[name] = N'Specialization');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [CaseProfiles] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [CaseProfiles] ALTER COLUMN [Specialization] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseProfiles]') AND [c].[name] = N'RequiredLawyerLevelId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [CaseProfiles] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [CaseProfiles] ALTER COLUMN [RequiredLawyerLevelId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseProfiles]') AND [c].[name] = N'Complexity');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [CaseProfiles] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [CaseProfiles] ALTER COLUMN [Complexity] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [lawyerProfile] ADD CONSTRAINT [PK_lawyerProfile] PRIMARY KEY ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    CREATE TABLE [CaseRecommendations] (
        [Id] uniqueidentifier NOT NULL,
        [CaseId] uniqueidentifier NOT NULL,
        [LawyerId] uniqueidentifier NOT NULL,
        [TotalScore] decimal(5,4) NOT NULL,
        [LocationScore] decimal(5,4) NOT NULL,
        [ExperienceScore] decimal(5,4) NOT NULL,
        [RatingScore] decimal(5,4) NOT NULL,
        [ResponseTimeScore] decimal(5,4) NOT NULL,
        [Explanation] nvarchar(max) NOT NULL,
        [Rank] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CaseRecommendations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CaseRecommendations_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CaseRecommendations_lawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [lawyerProfile] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    CREATE TABLE [LawyerSpecializations] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerProfileUserId] uniqueidentifier NOT NULL,
        [Specialization] int NOT NULL,
        [YearsOfExperience] int NOT NULL DEFAULT 0,
        [CasesHandled] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_LawyerSpecializations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LawyerSpecializations_lawyerProfile_LawyerProfileUserId] FOREIGN KEY ([LawyerProfileUserId]) REFERENCES [lawyerProfile] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    CREATE INDEX [IX_CaseRecommendation_CaseId_Rank] ON [CaseRecommendations] ([CaseId], [Rank]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    CREATE INDEX [IX_CaseRecommendations_LawyerId] ON [CaseRecommendations] ([LawyerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LawyerSpecialization_LawyerId_Specialization] ON [LawyerSpecializations] ([LawyerProfileUserId], [Specialization]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    ALTER TABLE [lawyerProfile] ADD CONSTRAINT [FK_lawyerProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802075632_CaseWorkflowEntityChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802075632_CaseWorkflowEntityChanges', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [NextRetryAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderAttemptCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [RequiresManualAction] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    UPDATE [PaymentTransactions]
    SET [ProviderAttemptCount] = 1,
        [NextRetryAt] = SYSUTCDATETIME()
    WHERE [OperationType] = 1
      AND [Status] = 2;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_ReleaseRecovery] ON [PaymentTransactions] ([Status], [OperationType], [RequiresManualAction], [NextRetryAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    EXEC(N'ALTER TABLE [PaymentTransactions] ADD CONSTRAINT [CK_PaymentTransactions_ProviderAttemptCount_NonNegative] CHECK ([ProviderAttemptCount] >= 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802141816_AddPaymentReleaseRecovery'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802141816_AddPaymentReleaseRecovery', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802151827_EnforceCriticalFinancialUniqueness'
)
BEGIN
    IF EXISTS (
        SELECT [MilestoneId]
        FROM [Disputes]
        WHERE [Status] IN (0, 1, 2)
        GROUP BY [MilestoneId]
        HAVING COUNT(*) > 1
    )
        THROW 51000, N'توجد نزاعات نشطة مكررة على المرحلة نفسها ويجب تسويتها قبل تطبيق قيد التفرد.', 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802151827_EnforceCriticalFinancialUniqueness'
)
BEGIN
    DROP INDEX [UX_Disputes_OpenPerMilestone] ON [Disputes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802151827_EnforceCriticalFinancialUniqueness'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Disputes_OpenPerMilestone] ON [Disputes] ([MilestoneId]) WHERE [Status] IN (0, 1, 2)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802151827_EnforceCriticalFinancialUniqueness'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802151827_EnforceCriticalFinancialUniqueness', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [ManualActionRequiredAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [RequiresManualAction] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ManualActionRequiredAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    UPDATE [PaymentTransactions]
    SET [ManualActionRequiredAt] = [UpdatedAt]
    WHERE [RequiresManualAction] = 1
      AND [ManualActionRequiredAt] IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    CREATE INDEX [IX_WithdrawalRequests_ReconciliationQueue] ON [WithdrawalRequests] ([Status], [RequiresManualAction], [RequestedAt], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    EXEC(N'ALTER TABLE [WithdrawalRequests] ADD CONSTRAINT [CK_WithdrawalRequests_ManualActionTimestamp] CHECK ([RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_ReconciliationQueue] ON [PaymentTransactions] ([Status], [RequiresManualAction], [CreatedAt], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    EXEC(N'ALTER TABLE [PaymentTransactions] ADD CONSTRAINT [CK_PaymentTransactions_ManualActionTimestamp] CHECK ([RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802153701_AddFinancialManualActionEscalation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802153701_AddFinancialManualActionEscalation', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803071643_MakeNationalNumberNullable'
)
BEGIN
    DROP INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803071643_MakeNationalNumberNullable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers] ([NationalNumber]) WHERE [NationalNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803071643_MakeNationalNumberNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803071643_MakeNationalNumberNullable', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [CaseRecommendations] DROP CONSTRAINT [FK_CaseRecommendations_lawyerProfile_LawyerId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [lawyerProfile] DROP CONSTRAINT [FK_lawyerProfile_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [LawyerSpecializations] DROP CONSTRAINT [FK_LawyerSpecializations_lawyerProfile_LawyerProfileUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [LegalCases] DROP CONSTRAINT [CK_LegalCases_Status_Range];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [lawyerProfile] DROP CONSTRAINT [PK_lawyerProfile];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    EXEC sp_rename N'[lawyerProfile]', N'LawyerProfile';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    DROP INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'NationalNumber');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [NationalNumber] varchar(14) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers] ([NationalNumber]) WHERE [NationalNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD CONSTRAINT [PK_LawyerProfile] PRIMARY KEY ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    EXEC(N'ALTER TABLE [LegalCases] ADD CONSTRAINT [CK_LegalCases_Status_Range] CHECK ([Status] BETWEEN 0 AND 6)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [CaseRecommendations] ADD CONSTRAINT [FK_CaseRecommendations_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [LawyerProfile] ADD CONSTRAINT [FK_LawyerProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    ALTER TABLE [LawyerSpecializations] ADD CONSTRAINT [FK_LawyerSpecializations_LawyerProfile_LawyerProfileUserId] FOREIGN KEY ([LawyerProfileUserId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803225539_MakeNationalNumberOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803225539_MakeNationalNumberOptional', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805135603_ChangeGenderToIntEnum'
)
BEGIN
    UPDATE AspNetUsers SET Gender = NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805135603_ChangeGenderToIntEnum'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Gender');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [Gender] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805135603_ChangeGenderToIntEnum'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260805135603_ChangeGenderToIntEnum', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805224211_AddNotificationsTable'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(255) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805224211_AddNotificationsTable'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805224211_AddNotificationsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260805224211_AddNotificationsTable', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806182109_RemoveNotificationsTable'
)
BEGIN
    DROP TABLE [Notifications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806182109_RemoveNotificationsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806182109_RemoveNotificationsTable', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806230514_AddModifiedFieldsJson'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ModifiedFieldsJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806230514_AddModifiedFieldsJson'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806230514_AddModifiedFieldsJson', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807013552_AddRejectionReasonToUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [RejectionReason] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807013552_AddRejectionReasonToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807013552_AddRejectionReasonToUser', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [ChatConversations] DROP CONSTRAINT [FK_ChatConversations_LegalCases_LegalCaseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [Contracts] DROP CONSTRAINT [FK_Contracts_LegalCases_LegalCaseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [Proposals] DROP CONSTRAINT [FK_Proposals_LegalCases_LegalCaseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    DROP TABLE [LegalCases];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [ChatConversations] ADD CONSTRAINT [FK_ChatConversations_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [Contracts] ADD CONSTRAINT [FK_Contracts_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    ALTER TABLE [Proposals] ADD CONSTRAINT [FK_Proposals_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807192102_MergeLegalCaseIntoCase'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807192102_MergeLegalCaseIntoCase', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [Sequence] bigint NOT NULL IDENTITY,
        [RecipientUserId] uniqueidentifier NOT NULL,
        [SourceEventId] uniqueidentifier NOT NULL,
        [Type] varchar(100) NOT NULL,
        [Severity] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(1000) NOT NULL,
        [ActionUrl] nvarchar(500) NULL,
        [DataJson] nvarchar(4000) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [ReadAtUtc] datetime2 NULL,
        [ExpiresAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Notifications_Severity_Range] CHECK ([Severity] BETWEEN 1 AND 4),
        CONSTRAINT [FK_Notifications_AspNetUsers_RecipientUserId] FOREIGN KEY ([RecipientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    CREATE INDEX [IX_Notifications_Recipient_Sequence] ON [Notifications] ([RecipientUserId], [Sequence] DESC);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Notifications_Recipient_Unread_Sequence] ON [Notifications] ([RecipientUserId], [ReadAtUtc], [Sequence] DESC) WHERE [ReadAtUtc] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Notifications_Sequence] ON [Notifications] ([Sequence]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Notifications_Source_Recipient_Type] ON [Notifications] ([SourceEventId], [RecipientUserId], [Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809102450_AddInAppNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809102450_AddInAppNotifications', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810095202_SyncModelChanges'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'IX_Proposals_LegalCaseId'
          AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
        DROP INDEX [IX_Proposals_LegalCaseId] ON [dbo].[Proposals];

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE [name] = N'CK_Proposals_Status_Range'
          AND [parent_object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
        ALTER TABLE [dbo].[Proposals]
            DROP CONSTRAINT [CK_Proposals_Status_Range];

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'IX_Contracts_LegalCaseId'
          AND [object_id] = OBJECT_ID(N'[dbo].[Contracts]'))
        DROP INDEX [IX_Contracts_LegalCaseId] ON [dbo].[Contracts];

    IF COL_LENGTH(N'dbo.Proposals', N'ClosedAt') IS NULL
        ALTER TABLE [dbo].[Proposals] ADD [ClosedAt] datetime2 NULL;

    IF COL_LENGTH(N'dbo.Proposals', N'ClosedByUserId') IS NULL
        ALTER TABLE [dbo].[Proposals]
            ADD [ClosedByUserId] uniqueidentifier NULL;

    IF COL_LENGTH(N'dbo.Proposals', N'ExpiresAt') IS NULL
        ALTER TABLE [dbo].[Proposals]
            ADD [ExpiresAt] datetime2 NOT NULL
            CONSTRAINT [DF_Proposals_ExpiresAt]
            DEFAULT '0001-01-01T00:00:00.0000000';

    IF COL_LENGTH(N'dbo.Cases', N'LawyerId') IS NULL
        ALTER TABLE [dbo].[Cases] ADD [LawyerId] uniqueidentifier NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'IX_Proposals_ClosedByUserId'
          AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
        CREATE INDEX [IX_Proposals_ClosedByUserId]
            ON [dbo].[Proposals] ([ClosedByUserId]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'IX_Proposals_Status_ExpiresAt'
          AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
        CREATE INDEX [IX_Proposals_Status_ExpiresAt]
            ON [dbo].[Proposals] ([Status], [ExpiresAt]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE [name] = N'CK_Proposals_Status_Range'
          AND [parent_object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
        ALTER TABLE [dbo].[Proposals]
            ADD CONSTRAINT [CK_Proposals_Status_Range]
            CHECK ([Status] BETWEEN 0 AND 6);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'UX_Contracts_ActiveCase'
          AND [object_id] = OBJECT_ID(N'[dbo].[Contracts]'))
        CREATE UNIQUE INDEX [UX_Contracts_ActiveCase]
            ON [dbo].[Contracts] ([LegalCaseId])
            WHERE [Status] = 1;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE [name] = N'IX_Cases_LawyerId'
          AND [object_id] = OBJECT_ID(N'[dbo].[Cases]'))
        CREATE INDEX [IX_Cases_LawyerId]
            ON [dbo].[Cases] ([LawyerId]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE [name] = N'FK_Cases_LawyerProfile_LawyerId')
        ALTER TABLE [dbo].[Cases]
            ADD CONSTRAINT [FK_Cases_LawyerProfile_LawyerId]
            FOREIGN KEY ([LawyerId]) REFERENCES [dbo].[LawyerProfile] ([UserId])
            ON DELETE NO ACTION;

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE [name] = N'FK_Proposals_AspNetUsers_ClosedByUserId')
        ALTER TABLE [dbo].[Proposals]
            ADD CONSTRAINT [FK_Proposals_AspNetUsers_ClosedByUserId]
            FOREIGN KEY ([ClosedByUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810095202_SyncModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260810095202_SyncModelChanges', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    CREATE TABLE [AgentConversations] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CaseId] uniqueidentifier NULL,
        [Title] nvarchar(200) NULL,
        [CachedCaseContext] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AgentConversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AgentConversations_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    CREATE TABLE [AgentMessages] (
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [Role] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AgentMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AgentMessages_AgentConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [AgentConversations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    CREATE INDEX [IX_AgentConversations_UserId_IsDeleted_UpdatedAt] ON [AgentConversations] ([UserId], [IsDeleted], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    CREATE INDEX [IX_AgentConversations_CaseId] ON [AgentConversations] ([CaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    CREATE INDEX [IX_AgentMessages_ConversationId] ON [AgentMessages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810153220_AddChatAgentTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260810153220_AddChatAgentTables', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811133554_AddChatMessageAttachments'
)
BEGIN
    CREATE TABLE [ChatMessageAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [MessageId] uniqueidentifier NOT NULL,
        [StoredFileId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessageAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessageAttachments_ChatMessages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [ChatMessages] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatMessageAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811133554_AddChatMessageAttachments'
)
BEGIN
    CREATE INDEX [IX_ChatMessageAttachments_MessageId] ON [ChatMessageAttachments] ([MessageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811133554_AddChatMessageAttachments'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ChatMessageAttachments_StoredFileId] ON [ChatMessageAttachments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811133554_AddChatMessageAttachments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811133554_AddChatMessageAttachments', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [LawyerPayoutAccountId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [ProviderAccountId] varchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [ProviderAmountMinor] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [ProviderCurrency] varchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD [ProviderStatus] varchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentWebhookEvents]') AND [c].[name] = N'PaymentTransactionId');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [PaymentWebhookEvents] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [PaymentWebhookEvents] ALTER COLUMN [PaymentTransactionId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [ConnectedAccountId] varchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [EventType] varchar(100) NOT NULL DEFAULT '';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [ProcessedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [ProcessingError] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [ProviderCode] varchar(50) NOT NULL DEFAULT '';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentWebhookEvents] ADD [ProviderObjectId] varchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderAmountMinor] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderCurrency] varchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderObjectType] varchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderRelatedTransactionId] varchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [ProviderStatus] varchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    CREATE TABLE [LawyerPayoutAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerUserId] uniqueidentifier NOT NULL,
        [ProviderCode] varchar(100) NOT NULL,
        [ProviderAccountId] varchar(200) NOT NULL,
        [Status] int NOT NULL,
        [DetailsSubmitted] bit NOT NULL,
        [TransfersEnabled] bit NOT NULL,
        [PayoutsEnabled] bit NOT NULL,
        [IsLive] bit NOT NULL,
        [Country] varchar(2) NOT NULL,
        [DefaultCurrency] varchar(3) NOT NULL,
        [AvailableProviderAmountMinor] bigint NOT NULL DEFAULT CAST(0 AS bigint),
        [MaskedDestination] nvarchar(200) NULL,
        [LastProviderStatus] varchar(100) NULL,
        [LastProviderErrorCode] varchar(200) NULL,
        [LastSynchronizedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_LawyerPayoutAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LawyerPayoutAccounts_ProviderBalance_NonNegative] CHECK ([AvailableProviderAmountMinor] >= 0),
        CONSTRAINT [CK_LawyerPayoutAccounts_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
        CONSTRAINT [FK_LawyerPayoutAccounts_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    CREATE INDEX [IX_WithdrawalRequests_LawyerPayoutAccountId] ON [WithdrawalRequests] ([LawyerPayoutAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    CREATE UNIQUE INDEX [UX_LawyerPayoutAccounts_Lawyer_Provider] ON [LawyerPayoutAccounts] ([LawyerUserId], [ProviderCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    CREATE UNIQUE INDEX [UX_LawyerPayoutAccounts_ProviderAccount] ON [LawyerPayoutAccounts] ([ProviderCode], [ProviderAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] ADD CONSTRAINT [FK_WithdrawalRequests_LawyerPayoutAccounts_LawyerPayoutAccountId] FOREIGN KEY ([LawyerPayoutAccountId]) REFERENCES [LawyerPayoutAccounts] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811165755_AddStripeConnectPaymentLifecycle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811165755_AddStripeConnectPaymentLifecycle', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    CREATE TABLE [LegalArticleCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [NameAr] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_LegalArticleCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    CREATE TABLE [LegalArticles] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(255) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Tags] nvarchar(500) NULL,
        [FeaturedImageUrl] nvarchar(1000) NULL,
        [ViewCount] int NOT NULL,
        [Status] int NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_LegalArticles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LegalArticles_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LegalArticles_LegalArticleCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [LegalArticleCategories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsDeleted', N'LastModifiedBy', N'NameAr', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LegalArticleCategories]'))
        SET IDENTITY_INSERT [LegalArticleCategories] ON;
    EXEC(N'INSERT INTO [LegalArticleCategories] ([Id], [Code], [CreatedAt], [CreatedBy], [Description], [IsDeleted], [LastModifiedBy], [NameAr], [UpdatedAt])
    VALUES (''a0b711e7-f1e1-450a-9d9f-3d12c5b96904'', N''criminal'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), NULL, N''القانون الجنائي'', ''2026-01-01T00:00:00.0000000Z''),
    (''b1b711e7-f1e1-450a-9d9f-3d12c5b96903'', N''labor'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), NULL, N''نظام العمل'', ''2026-01-01T00:00:00.0000000Z''),
    (''c2b711e7-f1e1-450a-9d9f-3d12c5b96902'', N''civil'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), NULL, N''القانون المدني'', ''2026-01-01T00:00:00.0000000Z''),
    (''d3b711e7-f1e1-450a-9d9f-3d12c5b96901'', N''commercial'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), NULL, N''القانون التجاري'', ''2026-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsDeleted', N'LastModifiedBy', N'NameAr', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LegalArticleCategories]'))
        SET IDENTITY_INSERT [LegalArticleCategories] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LegalArticleCategories_Code] ON [LegalArticleCategories] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    CREATE INDEX [IX_LegalArticles_AuthorId] ON [LegalArticles] ([AuthorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    CREATE INDEX [IX_LegalArticles_CategoryId] ON [LegalArticles] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812100348_M5_AddArticles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812100348_M5_AddArticles', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    ALTER TABLE [LegalArticles] ADD [CommentsCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    ALTER TABLE [LegalArticles] ADD [IsDeletedByAdmin] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    ALTER TABLE [LegalArticles] ADD [LikesCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE TABLE [ArticleComments] (
        [Id] uniqueidentifier NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Content] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ArticleComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ArticleComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ArticleComments_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE TABLE [ArticleLikes] (
        [ArticleId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ArticleLikes] PRIMARY KEY ([ArticleId], [UserId]),
        CONSTRAINT [FK_ArticleLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleLikes_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE TABLE [ArticleReports] (
        [Id] uniqueidentifier NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [ReporterId] uniqueidentifier NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsResolved] bit NOT NULL,
        CONSTRAINT [PK_ArticleReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ArticleReports_AspNetUsers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ArticleReports_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE TABLE [ArticleViews] (
        [Id] uniqueidentifier NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ArticleViews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ArticleViews_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ArticleViews_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleComments_ArticleId] ON [ArticleComments] ([ArticleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleComments_UserId] ON [ArticleComments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleLikes_UserId] ON [ArticleLikes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleReports_ArticleId] ON [ArticleReports] ([ArticleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleReports_ReporterId] ON [ArticleReports] ([ReporterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleViews_ArticleId] ON [ArticleViews] ([ArticleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    CREATE INDEX [IX_ArticleViews_UserId] ON [ArticleViews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812135103_AddArticleEngagementEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812135103_AddArticleEngagementEntities', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143235_AddClientPaymentCustomers'
)
BEGIN
    CREATE TABLE [ClientPaymentCustomers] (
        [Id] uniqueidentifier NOT NULL,
        [ClientUserId] uniqueidentifier NOT NULL,
        [ProviderCode] varchar(100) NOT NULL,
        [ProviderCustomerId] varchar(200) NOT NULL,
        [IsLive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ClientPaymentCustomers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientPaymentCustomers_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143235_AddClientPaymentCustomers'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ClientPaymentCustomers_Client_Provider] ON [ClientPaymentCustomers] ([ClientUserId], [ProviderCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143235_AddClientPaymentCustomers'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ClientPaymentCustomers_ProviderCustomer] ON [ClientPaymentCustomers] ([ProviderCode], [ProviderCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143235_AddClientPaymentCustomers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812143235_AddClientPaymentCustomers', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813104547_AddMilestoneDeliverables'
)
BEGIN
    ALTER TABLE [Milestones] ADD [Deliverables] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813104547_AddMilestoneDeliverables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813104547_AddMilestoneDeliverables', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    ALTER TABLE [Milestones] DROP CONSTRAINT [CK_Milestones_Status_Range];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    ALTER TABLE [MilestoneStateHistories] DROP CONSTRAINT [CK_MilestoneStateHistories_NewStatus_Range];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    ALTER TABLE [MilestoneStateHistories] DROP CONSTRAINT [CK_MilestoneStateHistories_PreviousStatus_Range];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    ALTER TABLE [Milestones] ADD [Type] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    CREATE INDEX [IX_Milestones_Type_Status_FundedAt] ON [Milestones] ([Type], [Status], [FundedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    EXEC(N'ALTER TABLE [Milestones] ADD CONSTRAINT [CK_Milestones_ExpenseFields] CHECK ([Type] <> 1 OR ([Deliverables] IS NULL AND [DurationDays] IS NULL))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    EXEC(N'ALTER TABLE [Milestones] ADD CONSTRAINT [CK_Milestones_Status_Range] CHECK ([Status] BETWEEN 0 AND 10)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    EXEC(N'ALTER TABLE [Milestones] ADD CONSTRAINT [CK_Milestones_Type_Range] CHECK ([Type] BETWEEN 0 AND 1)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    EXEC(N'ALTER TABLE [MilestoneStateHistories] ADD CONSTRAINT [CK_MilestoneStateHistories_NewStatus_Range] CHECK ([NewStatus] BETWEEN 0 AND 10)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    EXEC(N'ALTER TABLE [MilestoneStateHistories] ADD CONSTRAINT [CK_MilestoneStateHistories_PreviousStatus_Range] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 10)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813162345_AddMilestoneTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813162345_AddMilestoneTypes', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814174427_AddLastReviewIdToCase'
)
BEGIN
    ALTER TABLE [Cases] ADD [LastReviewId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814174427_AddLastReviewIdToCase'
)
BEGIN
    CREATE INDEX [IX_Cases_LastReviewId] ON [Cases] ([LastReviewId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814174427_AddLastReviewIdToCase'
)
BEGIN
    ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_CaseReviewReports_LastReviewId] FOREIGN KEY ([LastReviewId]) REFERENCES [CaseReviewReports] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814174427_AddLastReviewIdToCase'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814174427_AddLastReviewIdToCase', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814180609_AddChatIdToCase'
)
BEGIN
    ALTER TABLE [Cases] ADD [ChatId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814180609_AddChatIdToCase'
)
BEGIN
    CREATE INDEX [IX_Cases_ChatId] ON [Cases] ([ChatId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814180609_AddChatIdToCase'
)
BEGIN
    ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_ChatConversations_ChatId] FOREIGN KEY ([ChatId]) REFERENCES [ChatConversations] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814180609_AddChatIdToCase'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814180609_AddChatIdToCase', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationOfferings] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerId] uniqueidentifier NOT NULL,
        [Mode] tinyint NOT NULL,
        [Specialization] tinyint NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [OfficeLocation] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ConsultationOfferings] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationOfferings_Currency] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [CK_ConsultationOfferings_Duration] CHECK ([DurationMinutes] BETWEEN 15 AND 240),
        CONSTRAINT [CK_ConsultationOfferings_Price] CHECK ([Price] > 0 AND [Price] <= 100000),
        CONSTRAINT [FK_ConsultationOfferings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [LawyerConsultationSettings] (
        [LawyerId] uniqueidentifier NOT NULL,
        [IsEnabled] bit NOT NULL,
        [MinimumBookingNoticeHours] int NOT NULL,
        [MaximumAdvanceBookingDays] int NOT NULL,
        [BufferMinutes] int NOT NULL,
        [TimeZoneId] varchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_LawyerConsultationSettings] PRIMARY KEY ([LawyerId]),
        CONSTRAINT [CK_LawyerConsultationSettings_Advance] CHECK ([MaximumAdvanceBookingDays] BETWEEN 1 AND 365),
        CONSTRAINT [CK_LawyerConsultationSettings_Buffer] CHECK ([BufferMinutes] BETWEEN 0 AND 120),
        CONSTRAINT [CK_LawyerConsultationSettings_Notice] CHECK ([MinimumBookingNoticeHours] BETWEEN 0 AND 168),
        CONSTRAINT [FK_LawyerConsultationSettings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationAvailabilitySlots] (
        [Id] uniqueidentifier NOT NULL,
        [LawyerId] uniqueidentifier NOT NULL,
        [OfferingId] uniqueidentifier NOT NULL,
        [StartAtUtc] datetime2 NOT NULL,
        [EndAtUtc] datetime2 NOT NULL,
        [Status] tinyint NOT NULL,
        [ReservedUntilUtc] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ConsultationAvailabilitySlots] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationSlots_TimeRange] CHECK ([EndAtUtc] > [StartAtUtc]),
        CONSTRAINT [FK_ConsultationAvailabilitySlots_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationAvailabilitySlots_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationOfferingInclusions] (
        [Id] uniqueidentifier NOT NULL,
        [OfferingId] uniqueidentifier NOT NULL,
        [Text] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_ConsultationOfferingInclusions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsultationOfferingInclusions_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationBookings] (
        [Id] uniqueidentifier NOT NULL,
        [OfferingId] uniqueidentifier NOT NULL,
        [SlotId] uniqueidentifier NOT NULL,
        [LawyerId] uniqueidentifier NOT NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [Mode] tinyint NOT NULL,
        [Specialization] tinyint NOT NULL,
        [OfferingTitle] nvarchar(120) NOT NULL,
        [OfferingDescription] nvarchar(2000) NOT NULL,
        [InclusionsJson] nvarchar(3000) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [GrossAmount] decimal(18,2) NOT NULL,
        [PlatformFeeAmount] decimal(18,2) NOT NULL,
        [LawyerNetAmount] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [Subject] nvarchar(150) NOT NULL,
        [MatterSummary] nvarchar(3000) NOT NULL,
        [OfficeLocation] nvarchar(500) NULL,
        [MeetingUrl] varchar(1000) NULL,
        [StartAtUtc] datetime2 NOT NULL,
        [EndAtUtc] datetime2 NOT NULL,
        [Status] tinyint NOT NULL,
        [PaymentExpiresAtUtc] datetime2 NOT NULL,
        [PerformedAtUtc] datetime2 NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancellationReason] nvarchar(1000) NULL,
        [DisputeReason] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ConsultationBookings] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationBookings_Amounts] CHECK ([GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [LawyerNetAmount]),
        CONSTRAINT [CK_ConsultationBookings_Currency] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [FK_ConsultationBookings_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationBookings_ConsultationAvailabilitySlots_SlotId] FOREIGN KEY ([SlotId]) REFERENCES [ConsultationAvailabilitySlots] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationBookings_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationBookings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationPaymentTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [BookingId] uniqueidentifier NOT NULL,
        [OperationType] int NOT NULL,
        [Status] int NOT NULL,
        [ProviderName] varchar(100) NOT NULL,
        [IdempotencyKey] varchar(200) NOT NULL,
        [ProviderTransactionId] varchar(200) NULL,
        [RelatedProviderTransactionId] varchar(200) NULL,
        [ProviderStatus] varchar(100) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [FailureReason] nvarchar(1000) NULL,
        [RequiresManualAction] bit NOT NULL,
        [ProcessedAtUtc] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ConsultationPaymentTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationPaymentTransactions_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_ConsultationPaymentTransactions_Currency] CHECK ([Currency] = 'EGP'),
        CONSTRAINT [FK_ConsultationPaymentTransactions_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationEscrowHolds] (
        [Id] uniqueidentifier NOT NULL,
        [BookingId] uniqueidentifier NOT NULL,
        [DepositTransactionId] uniqueidentifier NOT NULL,
        [GrossAmount] decimal(18,2) NOT NULL,
        [PlatformFeeAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [Status] int NOT NULL,
        [FundedAtUtc] datetime2 NOT NULL,
        [HoldStartsAtUtc] datetime2 NULL,
        [HoldExpiresAtUtc] datetime2 NULL,
        [FrozenAtUtc] datetime2 NULL,
        [SettledAtUtc] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ConsultationEscrowHolds] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationEscrowHolds_Amounts] CHECK ([GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [NetAmount]),
        CONSTRAINT [FK_ConsultationEscrowHolds_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationEscrowHolds_ConsultationPaymentTransactions_DepositTransactionId] FOREIGN KEY ([DepositTransactionId]) REFERENCES [ConsultationPaymentTransactions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE TABLE [ConsultationLedgerEntries] (
        [Id] uniqueidentifier NOT NULL,
        [BookingId] uniqueidentifier NOT NULL,
        [PaymentTransactionId] uniqueidentifier NULL,
        [TransactionType] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [RunningBalance] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ConsultationLedgerEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConsultationLedgerEntries_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_ConsultationLedgerEntries_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConsultationLedgerEntries_ConsultationPaymentTransactions_PaymentTransactionId] FOREIGN KEY ([PaymentTransactionId]) REFERENCES [ConsultationPaymentTransactions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots] ([LawyerId], [StartAtUtc], [EndAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ConsultationAvailabilitySlots_OfferingId_StartAtUtc] ON [ConsultationAvailabilitySlots] ([OfferingId], [StartAtUtc]) WHERE [Status] <> 4');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationBookings_ClientId_Status_StartAtUtc] ON [ConsultationBookings] ([ClientId], [Status], [StartAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationBookings_LawyerId_Status_StartAtUtc] ON [ConsultationBookings] ([LawyerId], [Status], [StartAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationBookings_OfferingId] ON [ConsultationBookings] ([OfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ConsultationBookings_SlotId] ON [ConsultationBookings] ([SlotId]) WHERE [Status] IN (0,1,2,3,6)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConsultationEscrowHolds_BookingId] ON [ConsultationEscrowHolds] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationEscrowHolds_DepositTransactionId] ON [ConsultationEscrowHolds] ([DepositTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationLedgerEntries_BookingId_CreatedAt] ON [ConsultationLedgerEntries] ([BookingId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationLedgerEntries_PaymentTransactionId] ON [ConsultationLedgerEntries] ([PaymentTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConsultationOfferingInclusions_OfferingId_SortOrder] ON [ConsultationOfferingInclusions] ([OfferingId], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationOfferings_LawyerId_IsActive] ON [ConsultationOfferings] ([LawyerId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationOfferings_Mode_Specialization_IsActive] ON [ConsultationOfferings] ([Mode], [Specialization], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationPaymentTransactions_BookingId_OperationType_Status] ON [ConsultationPaymentTransactions] ([BookingId], [OperationType], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConsultationPaymentTransactions_ProviderName_IdempotencyKey] ON [ConsultationPaymentTransactions] ([ProviderName], [IdempotencyKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    CREATE INDEX [IX_ConsultationPaymentTransactions_ProviderTransactionId] ON [ConsultationPaymentTransactions] ([ProviderTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815122808_AddLawyerConsultations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815122808_AddLawyerConsultations', N'8.0.30');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_WithdrawalRequests_ReconciliationQueue] ON [WithdrawalRequests];
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WithdrawalRequests]') AND [c].[name] = N'RequestedAt');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [WithdrawalRequests] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [WithdrawalRequests] ALTER COLUMN [RequestedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_WithdrawalRequests_ReconciliationQueue] ON [WithdrawalRequests] ([Status], [RequiresManualAction], [RequestedAt], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WithdrawalRequests]') AND [c].[name] = N'ProcessedAt');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [WithdrawalRequests] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [WithdrawalRequests] ALTER COLUMN [ProcessedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    ALTER TABLE [WithdrawalRequests] DROP CONSTRAINT [CK_WithdrawalRequests_ManualActionTimestamp];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WithdrawalRequests]') AND [c].[name] = N'ManualActionRequiredAt');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [WithdrawalRequests] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [WithdrawalRequests] ALTER COLUMN [ManualActionRequiredAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_WalletAdjustments_WalletId_CreatedAt] ON [WalletAdjustments];
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WalletAdjustments]') AND [c].[name] = N'CreatedAt');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [WalletAdjustments] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [WalletAdjustments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_WalletAdjustments_WalletId_CreatedAt] ON [WalletAdjustments] ([LawyerWalletId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserVerificationDocuments]') AND [c].[name] = N'VerifiedAt');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [UserVerificationDocuments] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [UserVerificationDocuments] ALTER COLUMN [VerifiedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RefreshTokens]') AND [c].[name] = N'RevokedOn');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [RefreshTokens] ALTER COLUMN [RevokedOn] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RefreshTokens]') AND [c].[name] = N'ExpiresOn');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [RefreshTokens] ALTER COLUMN [ExpiresOn] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RefreshTokens]') AND [c].[name] = N'CreatedOn');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [RefreshTokens] ALTER COLUMN [CreatedOn] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'UpdatedAt');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Proposals] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'RespondedAt');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Proposals] ALTER COLUMN [RespondedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_Proposals_Status_ExpiresAt] ON [Proposals];
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'ExpiresAt');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [Proposals] ALTER COLUMN [ExpiresAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_Proposals_Status_ExpiresAt] ON [Proposals] ([Status], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'CreatedAt');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [Proposals] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'ClosedAt');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [Proposals] ALTER COLUMN [ClosedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentWebhookEvents]') AND [c].[name] = N'ReceivedAt');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [PaymentWebhookEvents] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [PaymentWebhookEvents] ALTER COLUMN [ReceivedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentWebhookEvents]') AND [c].[name] = N'ProcessedAt');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [PaymentWebhookEvents] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [PaymentWebhookEvents] ALTER COLUMN [ProcessedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'UpdatedAt');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'ProcessedAt');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [ProcessedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_ReleaseRecovery] ON [PaymentTransactions];
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'NextRetryAt');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [NextRetryAt] datetimeoffset NULL;
    CREATE INDEX [IX_PaymentTransactions_ReleaseRecovery] ON [PaymentTransactions] ([Status], [OperationType], [RequiresManualAction], [NextRetryAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'ManualActionRequiredAt');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [ManualActionRequiredAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_ReconciliationQueue] ON [PaymentTransactions];
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'CreatedAt');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_PaymentTransactions_ReconciliationQueue] ON [PaymentTransactions] ([Status], [RequiresManualAction], [CreatedAt], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxMessages]') AND [c].[name] = N'ProcessedAt');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [OutboxMessages] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [OutboxMessages] ALTER COLUMN [ProcessedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxMessages]') AND [c].[name] = N'LeaseExpiresAt');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [OutboxMessages] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [OutboxMessages] ALTER COLUMN [LeaseExpiresAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxMessages]') AND [c].[name] = N'CreatedAt');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [OutboxMessages] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [OutboxMessages] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_OutboxMessages_Status_AvailableAt] ON [OutboxMessages];
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxMessages]') AND [c].[name] = N'AvailableAt');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [OutboxMessages] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [OutboxMessages] ALTER COLUMN [AvailableAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_OutboxMessages_Status_AvailableAt] ON [OutboxMessages] ([Status], [AvailableAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_Notifications_Recipient_Unread_Sequence] ON [Notifications];
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'ReadAtUtc');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [Notifications] ALTER COLUMN [ReadAtUtc] datetimeoffset NULL;
    EXEC(N'CREATE INDEX [IX_Notifications_Recipient_Unread_Sequence] ON [Notifications] ([RecipientUserId], [ReadAtUtc], [Sequence] DESC) WHERE [ReadAtUtc] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'ExpiresAtUtc');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [Notifications] ALTER COLUMN [ExpiresAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'CreatedAtUtc');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [Notifications] ALTER COLUMN [CreatedAtUtc] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneSubmissions]') AND [c].[name] = N'SubmittedAt');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneSubmissions] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [MilestoneSubmissions] ALTER COLUMN [SubmittedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneSubmissionAttachments]') AND [c].[name] = N'CreatedAt');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneSubmissionAttachments] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [MilestoneSubmissionAttachments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_MilestoneStateHistories_MilestoneId_CreatedAt] ON [MilestoneStateHistories];
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneStateHistories]') AND [c].[name] = N'CreatedAt');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneStateHistories] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [MilestoneStateHistories] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_MilestoneStateHistories_MilestoneId_CreatedAt] ON [MilestoneStateHistories] ([MilestoneId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'UpdatedAt');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'SubmittedAt');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [SubmittedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'ReleasedAt');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [ReleasedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'RefundedAt');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [RefundedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'ReadyForFundingAt');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [ReadyForFundingAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'HoldStartsAt');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [HoldStartsAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'HoldExpiresAt');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [HoldExpiresAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_Milestones_Type_Status_FundedAt] ON [Milestones];
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'FundedAt');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [FundedAt] datetimeoffset NULL;
    CREATE INDEX [IX_Milestones_Type_Status_FundedAt] ON [Milestones] ([Type], [Status], [FundedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'DueDate');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [DueDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'CreatedAt');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_Milestones_Status_AutoAcceptEligibleAt] ON [Milestones];
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'AutoAcceptEligibleAt');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [AutoAcceptEligibleAt] datetimeoffset NULL;
    CREATE INDEX [IX_Milestones_Status_AutoAcceptEligibleAt] ON [Milestones] ([Status], [AutoAcceptEligibleAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'AcceptedByLawyerAt');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [AcceptedByLawyerAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'AcceptedByClientAt');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [AcceptedByClientAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Milestones]') AND [c].[name] = N'AcceptedAt');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [Milestones] DROP CONSTRAINT [' + @var54 + '];');
    ALTER TABLE [Milestones] ALTER COLUMN [AcceptedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneChangeRequests]') AND [c].[name] = N'ProposedDueDate');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneChangeRequests] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [MilestoneChangeRequests] ALTER COLUMN [ProposedDueDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneChangeRequests]') AND [c].[name] = N'DecidedAt');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneChangeRequests] DROP CONSTRAINT [' + @var56 + '];');
    ALTER TABLE [MilestoneChangeRequests] ALTER COLUMN [DecidedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var57 sysname;
    SELECT @var57 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MilestoneChangeRequests]') AND [c].[name] = N'CreatedAt');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [MilestoneChangeRequests] DROP CONSTRAINT [' + @var57 + '];');
    ALTER TABLE [MilestoneChangeRequests] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var58 sysname;
    SELECT @var58 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LegalArticles]') AND [c].[name] = N'UpdatedAt');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [LegalArticles] DROP CONSTRAINT [' + @var58 + '];');
    ALTER TABLE [LegalArticles] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var59 sysname;
    SELECT @var59 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LegalArticles]') AND [c].[name] = N'CreatedAt');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [LegalArticles] DROP CONSTRAINT [' + @var59 + '];');
    ALTER TABLE [LegalArticles] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var60 sysname;
    SELECT @var60 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LegalArticleCategories]') AND [c].[name] = N'UpdatedAt');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [LegalArticleCategories] DROP CONSTRAINT [' + @var60 + '];');
    ALTER TABLE [LegalArticleCategories] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LegalArticleCategories]') AND [c].[name] = N'CreatedAt');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [LegalArticleCategories] DROP CONSTRAINT [' + @var61 + '];');
    ALTER TABLE [LegalArticleCategories] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var62 sysname;
    SELECT @var62 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerWallets]') AND [c].[name] = N'UpdatedAt');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [LawyerWallets] DROP CONSTRAINT [' + @var62 + '];');
    ALTER TABLE [LawyerWallets] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var63 sysname;
    SELECT @var63 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerWallets]') AND [c].[name] = N'CreatedAt');
    IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [LawyerWallets] DROP CONSTRAINT [' + @var63 + '];');
    ALTER TABLE [LawyerWallets] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_LawyerPenalties_LawyerUserId_StartsAt] ON [LawyerPenalties];
    DECLARE @var64 sysname;
    SELECT @var64 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPenalties]') AND [c].[name] = N'StartsAt');
    IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPenalties] DROP CONSTRAINT [' + @var64 + '];');
    ALTER TABLE [LawyerPenalties] ALTER COLUMN [StartsAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_LawyerPenalties_LawyerUserId_StartsAt] ON [LawyerPenalties] ([LawyerUserId], [StartsAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var65 sysname;
    SELECT @var65 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPenalties]') AND [c].[name] = N'EndsAt');
    IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPenalties] DROP CONSTRAINT [' + @var65 + '];');
    ALTER TABLE [LawyerPenalties] ALTER COLUMN [EndsAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var66 sysname;
    SELECT @var66 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPenalties]') AND [c].[name] = N'CreatedAt');
    IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPenalties] DROP CONSTRAINT [' + @var66 + '];');
    ALTER TABLE [LawyerPenalties] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var67 sysname;
    SELECT @var67 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPayoutAccounts]') AND [c].[name] = N'UpdatedAt');
    IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPayoutAccounts] DROP CONSTRAINT [' + @var67 + '];');
    ALTER TABLE [LawyerPayoutAccounts] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var68 sysname;
    SELECT @var68 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPayoutAccounts]') AND [c].[name] = N'LastSynchronizedAt');
    IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPayoutAccounts] DROP CONSTRAINT [' + @var68 + '];');
    ALTER TABLE [LawyerPayoutAccounts] ALTER COLUMN [LastSynchronizedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var69 sysname;
    SELECT @var69 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerPayoutAccounts]') AND [c].[name] = N'CreatedAt');
    IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [LawyerPayoutAccounts] DROP CONSTRAINT [' + @var69 + '];');
    ALTER TABLE [LawyerPayoutAccounts] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var70 sysname;
    SELECT @var70 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerConsultationSettings]') AND [c].[name] = N'UpdatedAt');
    IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [LawyerConsultationSettings] DROP CONSTRAINT [' + @var70 + '];');
    ALTER TABLE [LawyerConsultationSettings] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var71 sysname;
    SELECT @var71 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawyerConsultationSettings]') AND [c].[name] = N'CreatedAt');
    IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [LawyerConsultationSettings] DROP CONSTRAINT [' + @var71 + '];');
    ALTER TABLE [LawyerConsultationSettings] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var72 sysname;
    SELECT @var72 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawDocuments]') AND [c].[name] = N'UpdatedAt');
    IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [LawDocuments] DROP CONSTRAINT [' + @var72 + '];');
    ALTER TABLE [LawDocuments] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var73 sysname;
    SELECT @var73 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawDocuments]') AND [c].[name] = N'ProcessingStartedAt');
    IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [LawDocuments] DROP CONSTRAINT [' + @var73 + '];');
    ALTER TABLE [LawDocuments] ALTER COLUMN [ProcessingStartedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var74 sysname;
    SELECT @var74 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawDocuments]') AND [c].[name] = N'CreatedAt');
    IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [LawDocuments] DROP CONSTRAINT [' + @var74 + '];');
    ALTER TABLE [LawDocuments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var75 sysname;
    SELECT @var75 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LawDocuments]') AND [c].[name] = N'CompletedAt');
    IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [LawDocuments] DROP CONSTRAINT [' + @var75 + '];');
    ALTER TABLE [LawDocuments] ALTER COLUMN [CompletedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_IdempotencyRecords_Status_ExpiresAt] ON [IdempotencyRecords];
    DECLARE @var76 sysname;
    SELECT @var76 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IdempotencyRecords]') AND [c].[name] = N'ExpiresAt');
    IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [IdempotencyRecords] DROP CONSTRAINT [' + @var76 + '];');
    ALTER TABLE [IdempotencyRecords] ALTER COLUMN [ExpiresAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_IdempotencyRecords_Status_ExpiresAt] ON [IdempotencyRecords] ([Status], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var77 sysname;
    SELECT @var77 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IdempotencyRecords]') AND [c].[name] = N'CreatedAt');
    IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [IdempotencyRecords] DROP CONSTRAINT [' + @var77 + '];');
    ALTER TABLE [IdempotencyRecords] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var78 sysname;
    SELECT @var78 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IdempotencyRecords]') AND [c].[name] = N'CompletedAt');
    IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [IdempotencyRecords] DROP CONSTRAINT [' + @var78 + '];');
    ALTER TABLE [IdempotencyRecords] ALTER COLUMN [CompletedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_EscrowLedgerEntries_AccountId_CreatedAt] ON [EscrowLedgerEntries];
    DECLARE @var79 sysname;
    SELECT @var79 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowLedgerEntries]') AND [c].[name] = N'CreatedAt');
    IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [EscrowLedgerEntries] DROP CONSTRAINT [' + @var79 + '];');
    ALTER TABLE [EscrowLedgerEntries] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_EscrowLedgerEntries_AccountId_CreatedAt] ON [EscrowLedgerEntries] ([EscrowAccountId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var80 sysname;
    SELECT @var80 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'UpdatedAt');
    IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var80 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var81 sysname;
    SELECT @var81 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'SettledAt');
    IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var81 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [SettledAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var82 sysname;
    SELECT @var82 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'HoldStartsAt');
    IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var82 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [HoldStartsAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_EscrowHolds_HoldExpiresAt_Status] ON [EscrowHolds];
    DECLARE @var83 sysname;
    SELECT @var83 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'HoldExpiresAt');
    IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var83 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [HoldExpiresAt] datetimeoffset NULL;
    CREATE INDEX [IX_EscrowHolds_HoldExpiresAt_Status] ON [EscrowHolds] ([HoldExpiresAt], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var84 sysname;
    SELECT @var84 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'FundedAt');
    IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var84 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [FundedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var85 sysname;
    SELECT @var85 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'FrozenAt');
    IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var85 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [FrozenAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var86 sysname;
    SELECT @var86 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowHolds]') AND [c].[name] = N'CreatedAt');
    IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [EscrowHolds] DROP CONSTRAINT [' + @var86 + '];');
    ALTER TABLE [EscrowHolds] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var87 sysname;
    SELECT @var87 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowAccounts]') AND [c].[name] = N'UpdatedAt');
    IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [EscrowAccounts] DROP CONSTRAINT [' + @var87 + '];');
    ALTER TABLE [EscrowAccounts] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var88 sysname;
    SELECT @var88 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EscrowAccounts]') AND [c].[name] = N'CreatedAt');
    IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [EscrowAccounts] DROP CONSTRAINT [' + @var88 + '];');
    ALTER TABLE [EscrowAccounts] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var89 sysname;
    SELECT @var89 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'UpdatedAt');
    IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var89 + '];');
    ALTER TABLE [Disputes] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var90 sysname;
    SELECT @var90 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'ResolvedAt');
    IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var90 + '];');
    ALTER TABLE [Disputes] ALTER COLUMN [ResolvedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_Disputes_Status_CreatedAt] ON [Disputes];
    DECLARE @var91 sysname;
    SELECT @var91 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'CreatedAt');
    IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var91 + '];');
    ALTER TABLE [Disputes] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_Disputes_Status_CreatedAt] ON [Disputes] ([Status], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var92 sysname;
    SELECT @var92 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'ClosedAt');
    IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var92 + '];');
    ALTER TABLE [Disputes] ALTER COLUMN [ClosedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var93 sysname;
    SELECT @var93 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DisputeResolutions]') AND [c].[name] = N'ResolvedAt');
    IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [DisputeResolutions] DROP CONSTRAINT [' + @var93 + '];');
    ALTER TABLE [DisputeResolutions] ALTER COLUMN [ResolvedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var94 sysname;
    SELECT @var94 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DisputeResolutions]') AND [c].[name] = N'CreatedAt');
    IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [DisputeResolutions] DROP CONSTRAINT [' + @var94 + '];');
    ALTER TABLE [DisputeResolutions] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var95 sysname;
    SELECT @var95 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DisputeEvidence]') AND [c].[name] = N'CreatedAt');
    IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [DisputeEvidence] DROP CONSTRAINT [' + @var95 + '];');
    ALTER TABLE [DisputeEvidence] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ContractStateHistories_ContractId_CreatedAt] ON [ContractStateHistories];
    DECLARE @var96 sysname;
    SELECT @var96 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContractStateHistories]') AND [c].[name] = N'CreatedAt');
    IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [ContractStateHistories] DROP CONSTRAINT [' + @var96 + '];');
    ALTER TABLE [ContractStateHistories] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ContractStateHistories_ContractId_CreatedAt] ON [ContractStateHistories] ([ContractId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var97 sysname;
    SELECT @var97 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'UpdatedAt');
    IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var97 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var98 sysname;
    SELECT @var98 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'TerminatedAt');
    IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var98 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [TerminatedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var99 sysname;
    SELECT @var99 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'CreatedAt');
    IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var99 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var100 sysname;
    SELECT @var100 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'CompletedAt');
    IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var100 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [CompletedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var101 sysname;
    SELECT @var101 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'ActivatedAt');
    IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var101 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [ActivatedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var102 sysname;
    SELECT @var102 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'AcceptedByLawyerAt');
    IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var102 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [AcceptedByLawyerAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var103 sysname;
    SELECT @var103 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'AcceptedByClientAt');
    IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var103 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [AcceptedByClientAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ContractFileAccessAudits_File_Entity_Time] ON [ContractFileAccessAudits];
    DECLARE @var104 sysname;
    SELECT @var104 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContractFileAccessAudits]') AND [c].[name] = N'AccessedAt');
    IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [ContractFileAccessAudits] DROP CONSTRAINT [' + @var104 + '];');
    ALTER TABLE [ContractFileAccessAudits] ALTER COLUMN [AccessedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ContractFileAccessAudits_File_Entity_Time] ON [ContractFileAccessAudits] ([StoredFileId], [RelatedEntityId], [AccessedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    ALTER TABLE [ContractFileAccessAudits] ADD [AccessReason] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var105 sysname;
    SELECT @var105 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContractAttachments]') AND [c].[name] = N'CreatedAt');
    IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [ContractAttachments] DROP CONSTRAINT [' + @var105 + '];');
    ALTER TABLE [ContractAttachments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var106 sysname;
    SELECT @var106 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationPaymentTransactions]') AND [c].[name] = N'UpdatedAt');
    IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationPaymentTransactions] DROP CONSTRAINT [' + @var106 + '];');
    ALTER TABLE [ConsultationPaymentTransactions] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var107 sysname;
    SELECT @var107 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationPaymentTransactions]') AND [c].[name] = N'ProcessedAtUtc');
    IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationPaymentTransactions] DROP CONSTRAINT [' + @var107 + '];');
    ALTER TABLE [ConsultationPaymentTransactions] ALTER COLUMN [ProcessedAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var108 sysname;
    SELECT @var108 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationPaymentTransactions]') AND [c].[name] = N'CreatedAt');
    IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationPaymentTransactions] DROP CONSTRAINT [' + @var108 + '];');
    ALTER TABLE [ConsultationPaymentTransactions] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var109 sysname;
    SELECT @var109 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationOfferings]') AND [c].[name] = N'UpdatedAt');
    IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationOfferings] DROP CONSTRAINT [' + @var109 + '];');
    ALTER TABLE [ConsultationOfferings] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var110 sysname;
    SELECT @var110 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationOfferings]') AND [c].[name] = N'CreatedAt');
    IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationOfferings] DROP CONSTRAINT [' + @var110 + '];');
    ALTER TABLE [ConsultationOfferings] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ConsultationLedgerEntries_BookingId_CreatedAt] ON [ConsultationLedgerEntries];
    DECLARE @var111 sysname;
    SELECT @var111 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationLedgerEntries]') AND [c].[name] = N'CreatedAt');
    IF @var111 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationLedgerEntries] DROP CONSTRAINT [' + @var111 + '];');
    ALTER TABLE [ConsultationLedgerEntries] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ConsultationLedgerEntries_BookingId_CreatedAt] ON [ConsultationLedgerEntries] ([BookingId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var112 sysname;
    SELECT @var112 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'UpdatedAt');
    IF @var112 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var112 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var113 sysname;
    SELECT @var113 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'SettledAtUtc');
    IF @var113 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var113 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [SettledAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var114 sysname;
    SELECT @var114 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'HoldStartsAtUtc');
    IF @var114 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var114 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [HoldStartsAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var115 sysname;
    SELECT @var115 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'HoldExpiresAtUtc');
    IF @var115 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var115 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [HoldExpiresAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var116 sysname;
    SELECT @var116 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'FundedAtUtc');
    IF @var116 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var116 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [FundedAtUtc] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var117 sysname;
    SELECT @var117 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'FrozenAtUtc');
    IF @var117 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var117 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [FrozenAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var118 sysname;
    SELECT @var118 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationEscrowHolds]') AND [c].[name] = N'CreatedAt');
    IF @var118 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationEscrowHolds] DROP CONSTRAINT [' + @var118 + '];');
    ALTER TABLE [ConsultationEscrowHolds] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var119 sysname;
    SELECT @var119 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'UpdatedAt');
    IF @var119 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var119 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ConsultationBookings_ClientId_Status_StartAtUtc] ON [ConsultationBookings];
    DROP INDEX [IX_ConsultationBookings_LawyerId_Status_StartAtUtc] ON [ConsultationBookings];
    DECLARE @var120 sysname;
    SELECT @var120 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'StartAtUtc');
    IF @var120 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var120 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [StartAtUtc] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ConsultationBookings_ClientId_Status_StartAtUtc] ON [ConsultationBookings] ([ClientId], [Status], [StartAtUtc]);
    CREATE INDEX [IX_ConsultationBookings_LawyerId_Status_StartAtUtc] ON [ConsultationBookings] ([LawyerId], [Status], [StartAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var121 sysname;
    SELECT @var121 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'PerformedAtUtc');
    IF @var121 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var121 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [PerformedAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var122 sysname;
    SELECT @var122 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'PaymentExpiresAtUtc');
    IF @var122 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var122 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [PaymentExpiresAtUtc] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var123 sysname;
    SELECT @var123 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'EndAtUtc');
    IF @var123 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var123 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [EndAtUtc] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var124 sysname;
    SELECT @var124 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'CreatedAt');
    IF @var124 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var124 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var125 sysname;
    SELECT @var125 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'CompletedAtUtc');
    IF @var125 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var125 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [CompletedAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var126 sysname;
    SELECT @var126 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationBookings]') AND [c].[name] = N'CancelledAtUtc');
    IF @var126 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationBookings] DROP CONSTRAINT [' + @var126 + '];');
    ALTER TABLE [ConsultationBookings] ALTER COLUMN [CancelledAtUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var127 sysname;
    SELECT @var127 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilitySlots]') AND [c].[name] = N'UpdatedAt');
    IF @var127 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilitySlots] DROP CONSTRAINT [' + @var127 + '];');
    ALTER TABLE [ConsultationAvailabilitySlots] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots];
    DROP INDEX [IX_ConsultationAvailabilitySlots_OfferingId_StartAtUtc] ON [ConsultationAvailabilitySlots];
    DECLARE @var128 sysname;
    SELECT @var128 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilitySlots]') AND [c].[name] = N'StartAtUtc');
    IF @var128 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilitySlots] DROP CONSTRAINT [' + @var128 + '];');
    ALTER TABLE [ConsultationAvailabilitySlots] ALTER COLUMN [StartAtUtc] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots] ([LawyerId], [StartAtUtc], [EndAtUtc]);
    EXEC(N'CREATE UNIQUE INDEX [IX_ConsultationAvailabilitySlots_OfferingId_StartAtUtc] ON [ConsultationAvailabilitySlots] ([OfferingId], [StartAtUtc]) WHERE [Status] <> 4');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var129 sysname;
    SELECT @var129 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilitySlots]') AND [c].[name] = N'ReservedUntilUtc');
    IF @var129 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilitySlots] DROP CONSTRAINT [' + @var129 + '];');
    ALTER TABLE [ConsultationAvailabilitySlots] ALTER COLUMN [ReservedUntilUtc] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots];
    DECLARE @var130 sysname;
    SELECT @var130 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilitySlots]') AND [c].[name] = N'EndAtUtc');
    IF @var130 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilitySlots] DROP CONSTRAINT [' + @var130 + '];');
    ALTER TABLE [ConsultationAvailabilitySlots] ALTER COLUMN [EndAtUtc] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots] ([LawyerId], [StartAtUtc], [EndAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var131 sysname;
    SELECT @var131 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilitySlots]') AND [c].[name] = N'CreatedAt');
    IF @var131 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilitySlots] DROP CONSTRAINT [' + @var131 + '];');
    ALTER TABLE [ConsultationAvailabilitySlots] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var132 sysname;
    SELECT @var132 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientPaymentCustomers]') AND [c].[name] = N'UpdatedAt');
    IF @var132 IS NOT NULL EXEC(N'ALTER TABLE [ClientPaymentCustomers] DROP CONSTRAINT [' + @var132 + '];');
    ALTER TABLE [ClientPaymentCustomers] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var133 sysname;
    SELECT @var133 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClientPaymentCustomers]') AND [c].[name] = N'CreatedAt');
    IF @var133 IS NOT NULL EXEC(N'ALTER TABLE [ClientPaymentCustomers] DROP CONSTRAINT [' + @var133 + '];');
    ALTER TABLE [ClientPaymentCustomers] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ChatMessages_Conversation_CreatedAt] ON [ChatMessages];
    DECLARE @var134 sysname;
    SELECT @var134 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChatMessages]') AND [c].[name] = N'CreatedAt');
    IF @var134 IS NOT NULL EXEC(N'ALTER TABLE [ChatMessages] DROP CONSTRAINT [' + @var134 + '];');
    ALTER TABLE [ChatMessages] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ChatMessages_Conversation_CreatedAt] ON [ChatMessages] ([ConversationId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var135 sysname;
    SELECT @var135 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChatMessageAttachments]') AND [c].[name] = N'CreatedAt');
    IF @var135 IS NOT NULL EXEC(N'ALTER TABLE [ChatMessageAttachments] DROP CONSTRAINT [' + @var135 + '];');
    ALTER TABLE [ChatMessageAttachments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_ChatConversations_Client_UpdatedAt] ON [ChatConversations];
    DROP INDEX [IX_ChatConversations_Lawyer_UpdatedAt] ON [ChatConversations];
    DECLARE @var136 sysname;
    SELECT @var136 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChatConversations]') AND [c].[name] = N'UpdatedAt');
    IF @var136 IS NOT NULL EXEC(N'ALTER TABLE [ChatConversations] DROP CONSTRAINT [' + @var136 + '];');
    ALTER TABLE [ChatConversations] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_ChatConversations_Client_UpdatedAt] ON [ChatConversations] ([ClientUserId], [UpdatedAt]);
    CREATE INDEX [IX_ChatConversations_Lawyer_UpdatedAt] ON [ChatConversations] ([LawyerUserId], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var137 sysname;
    SELECT @var137 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChatConversations]') AND [c].[name] = N'LastMessageAt');
    IF @var137 IS NOT NULL EXEC(N'ALTER TABLE [ChatConversations] DROP CONSTRAINT [' + @var137 + '];');
    ALTER TABLE [ChatConversations] ALTER COLUMN [LastMessageAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var138 sysname;
    SELECT @var138 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChatConversations]') AND [c].[name] = N'CreatedAt');
    IF @var138 IS NOT NULL EXEC(N'ALTER TABLE [ChatConversations] DROP CONSTRAINT [' + @var138 + '];');
    ALTER TABLE [ChatConversations] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var139 sysname;
    SELECT @var139 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'UpdatedAt');
    IF @var139 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT [' + @var139 + '];');
    ALTER TABLE [Cases] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var140 sysname;
    SELECT @var140 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'SubmittedAt');
    IF @var140 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT [' + @var140 + '];');
    ALTER TABLE [Cases] ALTER COLUMN [SubmittedAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var141 sysname;
    SELECT @var141 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'CreatedAt');
    IF @var141 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT [' + @var141 + '];');
    ALTER TABLE [Cases] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var142 sysname;
    SELECT @var142 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseReviewReports]') AND [c].[name] = N'UpdatedAt');
    IF @var142 IS NOT NULL EXEC(N'ALTER TABLE [CaseReviewReports] DROP CONSTRAINT [' + @var142 + '];');
    ALTER TABLE [CaseReviewReports] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var143 sysname;
    SELECT @var143 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseReviewReports]') AND [c].[name] = N'CreatedAt');
    IF @var143 IS NOT NULL EXEC(N'ALTER TABLE [CaseReviewReports] DROP CONSTRAINT [' + @var143 + '];');
    ALTER TABLE [CaseReviewReports] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var144 sysname;
    SELECT @var144 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseRecommendations]') AND [c].[name] = N'UpdatedAt');
    IF @var144 IS NOT NULL EXEC(N'ALTER TABLE [CaseRecommendations] DROP CONSTRAINT [' + @var144 + '];');
    ALTER TABLE [CaseRecommendations] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var145 sysname;
    SELECT @var145 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseRecommendations]') AND [c].[name] = N'CreatedAt');
    IF @var145 IS NOT NULL EXEC(N'ALTER TABLE [CaseRecommendations] DROP CONSTRAINT [' + @var145 + '];');
    ALTER TABLE [CaseRecommendations] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var146 sysname;
    SELECT @var146 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseProfiles]') AND [c].[name] = N'UpdatedAt');
    IF @var146 IS NOT NULL EXEC(N'ALTER TABLE [CaseProfiles] DROP CONSTRAINT [' + @var146 + '];');
    ALTER TABLE [CaseProfiles] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var147 sysname;
    SELECT @var147 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseProfiles]') AND [c].[name] = N'CreatedAt');
    IF @var147 IS NOT NULL EXEC(N'ALTER TABLE [CaseProfiles] DROP CONSTRAINT [' + @var147 + '];');
    ALTER TABLE [CaseProfiles] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var148 sysname;
    SELECT @var148 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseDocuments]') AND [c].[name] = N'UpdatedAt');
    IF @var148 IS NOT NULL EXEC(N'ALTER TABLE [CaseDocuments] DROP CONSTRAINT [' + @var148 + '];');
    ALTER TABLE [CaseDocuments] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var149 sysname;
    SELECT @var149 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CaseDocuments]') AND [c].[name] = N'CreatedAt');
    IF @var149 IS NOT NULL EXEC(N'ALTER TABLE [CaseDocuments] DROP CONSTRAINT [' + @var149 + '];');
    ALTER TABLE [CaseDocuments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var150 sysname;
    SELECT @var150 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LastLoginAt');
    IF @var150 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var150 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [LastLoginAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var151 sysname;
    SELECT @var151 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ArticleViews]') AND [c].[name] = N'CreatedAt');
    IF @var151 IS NOT NULL EXEC(N'ALTER TABLE [ArticleViews] DROP CONSTRAINT [' + @var151 + '];');
    ALTER TABLE [ArticleViews] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var152 sysname;
    SELECT @var152 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ArticleReports]') AND [c].[name] = N'CreatedAt');
    IF @var152 IS NOT NULL EXEC(N'ALTER TABLE [ArticleReports] DROP CONSTRAINT [' + @var152 + '];');
    ALTER TABLE [ArticleReports] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var153 sysname;
    SELECT @var153 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ArticleLikes]') AND [c].[name] = N'CreatedAt');
    IF @var153 IS NOT NULL EXEC(N'ALTER TABLE [ArticleLikes] DROP CONSTRAINT [' + @var153 + '];');
    ALTER TABLE [ArticleLikes] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var154 sysname;
    SELECT @var154 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ArticleComments]') AND [c].[name] = N'CreatedAt');
    IF @var154 IS NOT NULL EXEC(N'ALTER TABLE [ArticleComments] DROP CONSTRAINT [' + @var154 + '];');
    ALTER TABLE [ArticleComments] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_AgentMessages_ConversationId_CreatedAt] ON [AgentMessages];
    DECLARE @var155 sysname;
    SELECT @var155 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AgentMessages]') AND [c].[name] = N'CreatedAt');
    IF @var155 IS NOT NULL EXEC(N'ALTER TABLE [AgentMessages] DROP CONSTRAINT [' + @var155 + '];');
    ALTER TABLE [AgentMessages] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_AgentMessages_ConversationId_CreatedAt] ON [AgentMessages] ([ConversationId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DROP INDEX [IX_AgentConversations_UserId_IsDeleted_UpdatedAt] ON [AgentConversations];
    DECLARE @var156 sysname;
    SELECT @var156 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AgentConversations]') AND [c].[name] = N'UpdatedAt');
    IF @var156 IS NOT NULL EXEC(N'ALTER TABLE [AgentConversations] DROP CONSTRAINT [' + @var156 + '];');
    ALTER TABLE [AgentConversations] ALTER COLUMN [UpdatedAt] datetimeoffset NOT NULL;
    CREATE INDEX [IX_AgentConversations_UserId_IsDeleted_UpdatedAt] ON [AgentConversations] ([UserId], [IsDeleted], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    DECLARE @var157 sysname;
    SELECT @var157 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AgentConversations]') AND [c].[name] = N'CreatedAt');
    IF @var157 IS NOT NULL EXEC(N'ALTER TABLE [AgentConversations] DROP CONSTRAINT [' + @var157 + '];');
    ALTER TABLE [AgentConversations] ALTER COLUMN [CreatedAt] datetimeoffset NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    EXEC(N'UPDATE [LegalArticleCategories] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000+00:00'', [UpdatedAt] = ''2026-01-01T00:00:00.0000000+00:00''
    WHERE [Id] = ''a0b711e7-f1e1-450a-9d9f-3d12c5b96904'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    EXEC(N'UPDATE [LegalArticleCategories] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000+00:00'', [UpdatedAt] = ''2026-01-01T00:00:00.0000000+00:00''
    WHERE [Id] = ''b1b711e7-f1e1-450a-9d9f-3d12c5b96903'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    EXEC(N'UPDATE [LegalArticleCategories] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000+00:00'', [UpdatedAt] = ''2026-01-01T00:00:00.0000000+00:00''
    WHERE [Id] = ''c2b711e7-f1e1-450a-9d9f-3d12c5b96902'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    EXEC(N'UPDATE [LegalArticleCategories] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000+00:00'', [UpdatedAt] = ''2026-01-01T00:00:00.0000000+00:00''
    WHERE [Id] = ''d3b711e7-f1e1-450a-9d9f-3d12c5b96901'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815134657_MigrateDateTimeToDateTimeOffset'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815134657_MigrateDateTimeToDateTimeOffset', N'8.0.30');
END;
GO

COMMIT;
GO

