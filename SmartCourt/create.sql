CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [NationalNumber] varchar(14) NULL,
    [Gender] int NULL,
    [DateOfBirth] date NULL,
    [Address] nvarchar(500) NULL,
    [Governorate] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [LastLoginAt] datetimeoffset NULL,
    [Status] int NOT NULL DEFAULT 0,
    [ProfilePictureUrl] nvarchar(max) NULL,
    [ModifiedFieldsJson] nvarchar(max) NULL,
    [RejectionReason] nvarchar(max) NULL,
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
GO


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
    [ProcessingStartedAt] datetimeoffset NULL,
    [CompletedAt] datetimeoffset NULL,
    [Version] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LawDocuments] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LegalArticleCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [NameAr] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LegalArticleCategories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LegalCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_LegalCategories] PRIMARY KEY ([Id])
);
GO


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
    [AvailableAt] datetimeoffset NOT NULL,
    [LeaseId] uniqueidentifier NULL,
    [LeaseExpiresAt] datetimeoffset NULL,
    [ProcessedAt] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OutboxMessages_Attempts_NonNegative] CHECK ([Attempts] >= 0),
    CONSTRAINT [CK_OutboxMessages_EventVersion_Positive] CHECK ([EventVersion] > 0),
    CONSTRAINT [CK_OutboxMessages_Status_Range] CHECK ([Status] BETWEEN 0 AND 3)
);
GO


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
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
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
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ClientPaymentCustomers] (
    [Id] uniqueidentifier NOT NULL,
    [ClientUserId] uniqueidentifier NOT NULL,
    [ProviderCode] varchar(100) NOT NULL,
    [ProviderCustomerId] varchar(200) NOT NULL,
    [IsLive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ClientPaymentCustomers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientPaymentCustomers_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ClientProfile] (
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ClientProfile] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_ClientProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContractFileAccessAudits] (
    [Id] uniqueidentifier NOT NULL,
    [ActorUserId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [Purpose] int NOT NULL,
    [RelatedEntityId] uniqueidentifier NOT NULL,
    [AccessReason] nvarchar(max) NOT NULL,
    [ModeratorAccess] bit NOT NULL,
    [AccessedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ContractFileAccessAudits] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ContractFileAccessAudits_Purpose_Range] CHECK ([Purpose] BETWEEN 1 AND 3),
    CONSTRAINT [FK_ContractFileAccessAudits_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


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
    [ExpiresAt] datetimeoffset NOT NULL,
    [CompletedAt] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_IdempotencyRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_IdempotencyRecords_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
    CONSTRAINT [FK_IdempotencyRecords_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


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
    [LastSynchronizedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_LawyerPayoutAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LawyerPayoutAccounts_ProviderBalance_NonNegative] CHECK ([AvailableProviderAmountMinor] >= 0),
    CONSTRAINT [CK_LawyerPayoutAccounts_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
    CONSTRAINT [FK_LawyerPayoutAccounts_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LawyerProfile] (
    [UserId] uniqueidentifier NOT NULL,
    [Level] int NOT NULL DEFAULT 1,
    [Bio] nvarchar(500) NULL,
    [IsAvailable] bit NOT NULL,
    [AverageRating] decimal(3,2) NOT NULL DEFAULT 0.0,
    [AverageResponseTimeHours] decimal(10,2) NOT NULL DEFAULT 0.0,
    CONSTRAINT [PK_LawyerProfile] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_LawyerProfile_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [LawyerWallets] (
    [Id] uniqueidentifier NOT NULL,
    [LawyerUserId] uniqueidentifier NOT NULL,
    [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
    [PendingBalance] decimal(18,2) NOT NULL,
    [AvailableBalance] decimal(18,2) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_LawyerWallets] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LawyerWallets_Balances_NonNegative] CHECK ([PendingBalance] >= 0 AND [AvailableBalance] >= 0),
    CONSTRAINT [CK_LawyerWallets_Currency_EGP] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [FK_LawyerWallets_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


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
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [ReadAtUtc] datetimeoffset NULL,
    [ExpiresAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Notifications_Severity_Range] CHECK ([Severity] BETWEEN 1 AND 4),
    CONSTRAINT [FK_Notifications_AspNetUsers_RecipientUserId] FOREIGN KEY ([RecipientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [RefreshTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [Id] int NOT NULL IDENTITY,
    [HashedToken] nvarchar(max) NOT NULL,
    [ExpiresOn] datetimeoffset NOT NULL,
    [CreatedOn] datetimeoffset NOT NULL,
    [RevokedOn] datetimeoffset NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([UserId], [Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [LegalArticles] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Tags] nvarchar(500) NULL,
    [FeaturedImageUrl] nvarchar(1000) NULL,
    [ViewCount] int NOT NULL,
    [LikesCount] int NOT NULL,
    [CommentsCount] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    [IsDeletedByAdmin] bit NOT NULL,
    [Status] int NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [AuthorId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LegalArticles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LegalArticles_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LegalArticles_LegalArticleCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [LegalArticleCategories] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LegalSpecializations] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_LegalSpecializations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LegalSpecializations_LegalCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [LegalCategories] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [UserVerificationDocuments] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [DocumentType] tinyint NOT NULL,
    [Status] tinyint NOT NULL,
    [ExpirationDate] date NOT NULL,
    [VerifiedAt] datetimeoffset NULL,
    [VerifiedByAdminId] nvarchar(max) NULL,
    [RejectionReason] nvarchar(max) NULL,
    [IsCurrent] bit NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserVerificationDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserVerificationDocuments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserVerificationDocuments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [WithdrawalRequests] (
    [Id] uniqueidentifier NOT NULL,
    [LawyerUserId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
    [Status] int NOT NULL,
    [ProviderTransactionId] varchar(200) NULL,
    [LawyerPayoutAccountId] uniqueidentifier NULL,
    [ProviderAccountId] varchar(200) NULL,
    [ProviderStatus] varchar(100) NULL,
    [ProviderAmountMinor] bigint NULL,
    [ProviderCurrency] varchar(3) NULL,
    [FailureReason] nvarchar(2000) NULL,
    [RequiresManualAction] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ManualActionRequiredAt] datetimeoffset NULL,
    [RequestedAt] datetimeoffset NOT NULL,
    [ProcessedAt] datetimeoffset NULL,
    [IdempotencyKey] varchar(200) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_WithdrawalRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WithdrawalRequests_Amount_Positive] CHECK ([Amount] > 0),
    CONSTRAINT [CK_WithdrawalRequests_Currency_EGP] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [CK_WithdrawalRequests_ManualActionTimestamp] CHECK ([RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL),
    CONSTRAINT [CK_WithdrawalRequests_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
    CONSTRAINT [FK_WithdrawalRequests_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WithdrawalRequests_LawyerPayoutAccounts_LawyerPayoutAccountId] FOREIGN KEY ([LawyerPayoutAccountId]) REFERENCES [LawyerPayoutAccounts] ([Id]) ON DELETE NO ACTION
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ConsultationOfferings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationOfferings_Currency] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [CK_ConsultationOfferings_Duration] CHECK ([DurationMinutes] BETWEEN 15 AND 240),
    CONSTRAINT [CK_ConsultationOfferings_Price] CHECK ([Price] > 0 AND [Price] <= 100000),
    CONSTRAINT [FK_ConsultationOfferings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LawyerConsultationSettings] (
    [LawyerId] uniqueidentifier NOT NULL,
    [IsEnabled] bit NOT NULL,
    [MinimumBookingNoticeHours] int NOT NULL,
    [MaximumAdvanceBookingDays] int NOT NULL,
    [BufferMinutes] int NOT NULL,
    [TimeZoneId] varchar(100) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_LawyerConsultationSettings] PRIMARY KEY ([LawyerId]),
    CONSTRAINT [CK_LawyerConsultationSettings_Advance] CHECK ([MaximumAdvanceBookingDays] BETWEEN 1 AND 365),
    CONSTRAINT [CK_LawyerConsultationSettings_Buffer] CHECK ([BufferMinutes] BETWEEN 0 AND 120),
    CONSTRAINT [CK_LawyerConsultationSettings_Notice] CHECK ([MinimumBookingNoticeHours] BETWEEN 0 AND 168),
    CONSTRAINT [FK_LawyerConsultationSettings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LawyerSpecializations] (
    [Id] uniqueidentifier NOT NULL,
    [LawyerProfileUserId] uniqueidentifier NOT NULL,
    [Specialization] int NOT NULL,
    [YearsOfExperience] int NOT NULL DEFAULT 0,
    [CasesHandled] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_LawyerSpecializations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LawyerSpecializations_LawyerProfile_LawyerProfileUserId] FOREIGN KEY ([LawyerProfileUserId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ArticleComments] (
    [Id] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ArticleComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ArticleComments_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ArticleLikes] (
    [ArticleId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ArticleLikes] PRIMARY KEY ([ArticleId], [UserId]),
    CONSTRAINT [FK_ArticleLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArticleLikes_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ArticleReports] (
    [Id] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [ReporterId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [IsResolved] bit NOT NULL,
    CONSTRAINT [PK_ArticleReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleReports_AspNetUsers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ArticleReports_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ArticleViews] (
    [Id] uniqueidentifier NOT NULL,
    [ArticleId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ArticleViews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleViews_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ArticleViews_LegalArticles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [LegalArticles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ConsultationAvailabilitySlots] (
    [Id] uniqueidentifier NOT NULL,
    [LawyerId] uniqueidentifier NOT NULL,
    [OfferingId] uniqueidentifier NOT NULL,
    [StartAtUtc] datetimeoffset NOT NULL,
    [EndAtUtc] datetimeoffset NOT NULL,
    [Status] tinyint NOT NULL,
    [ReservedUntilUtc] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ConsultationAvailabilitySlots] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationSlots_TimeRange] CHECK ([EndAtUtc] > [StartAtUtc]),
    CONSTRAINT [FK_ConsultationAvailabilitySlots_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationAvailabilitySlots_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ConsultationOfferingInclusions] (
    [Id] uniqueidentifier NOT NULL,
    [OfferingId] uniqueidentifier NOT NULL,
    [Text] nvarchar(200) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_ConsultationOfferingInclusions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConsultationOfferingInclusions_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE CASCADE
);
GO


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
    [StartAtUtc] datetimeoffset NOT NULL,
    [EndAtUtc] datetimeoffset NOT NULL,
    [Status] tinyint NOT NULL,
    [PaymentExpiresAtUtc] datetimeoffset NOT NULL,
    [PerformedAtUtc] datetimeoffset NULL,
    [CompletedAtUtc] datetimeoffset NULL,
    [CancelledAtUtc] datetimeoffset NULL,
    [CancellationReason] nvarchar(1000) NULL,
    [DisputeReason] nvarchar(2000) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ConsultationBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationBookings_Amounts] CHECK ([GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [LawyerNetAmount]),
    CONSTRAINT [CK_ConsultationBookings_Currency] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [FK_ConsultationBookings_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationBookings_ConsultationAvailabilitySlots_SlotId] FOREIGN KEY ([SlotId]) REFERENCES [ConsultationAvailabilitySlots] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationBookings_ConsultationOfferings_OfferingId] FOREIGN KEY ([OfferingId]) REFERENCES [ConsultationOfferings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationBookings_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


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
    [ProcessedAtUtc] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ConsultationPaymentTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationPaymentTransactions_Amount] CHECK ([Amount] > 0),
    CONSTRAINT [CK_ConsultationPaymentTransactions_Currency] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [FK_ConsultationPaymentTransactions_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ConsultationEscrowHolds] (
    [Id] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NOT NULL,
    [DepositTransactionId] uniqueidentifier NOT NULL,
    [GrossAmount] decimal(18,2) NOT NULL,
    [PlatformFeeAmount] decimal(18,2) NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [Currency] varchar(3) NOT NULL,
    [Status] int NOT NULL,
    [FundedAtUtc] datetimeoffset NOT NULL,
    [HoldStartsAtUtc] datetimeoffset NULL,
    [HoldExpiresAtUtc] datetimeoffset NULL,
    [FrozenAtUtc] datetimeoffset NULL,
    [SettledAtUtc] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ConsultationEscrowHolds] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationEscrowHolds_Amounts] CHECK ([GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [NetAmount]),
    CONSTRAINT [FK_ConsultationEscrowHolds_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationEscrowHolds_ConsultationPaymentTransactions_DepositTransactionId] FOREIGN KEY ([DepositTransactionId]) REFERENCES [ConsultationPaymentTransactions] ([Id]) ON DELETE NO ACTION
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ConsultationLedgerEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConsultationLedgerEntries_Amount] CHECK ([Amount] > 0),
    CONSTRAINT [FK_ConsultationLedgerEntries_ConsultationBookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [ConsultationBookings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConsultationLedgerEntries_ConsultationPaymentTransactions_PaymentTransactionId] FOREIGN KEY ([PaymentTransactionId]) REFERENCES [ConsultationPaymentTransactions] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AgentConversations] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CaseId] uniqueidentifier NULL,
    [Title] nvarchar(200) NULL,
    [CachedCaseContext] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_AgentConversations] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AgentMessages] (
    [Id] uniqueidentifier NOT NULL,
    [ConversationId] uniqueidentifier NOT NULL,
    [Role] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_AgentMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AgentMessages_AgentConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [AgentConversations] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CaseDocuments] (
    [Id] uniqueidentifier NOT NULL,
    [CaseId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CaseDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CaseDocuments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CaseProfiles] (
    [Id] uniqueidentifier NOT NULL,
    [CaseId] uniqueidentifier NOT NULL,
    [Specialization] int NOT NULL,
    [RequiredLawyerLevelId] int NOT NULL,
    [Complexity] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CaseProfiles] PRIMARY KEY ([Id])
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CaseRecommendations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CaseRecommendations_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [CaseReviewReports] (
    [Id] uniqueidentifier NOT NULL,
    [IsLatest] bit NOT NULL,
    [CaseId] uniqueidentifier NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CaseReviewReports] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ReviewPoints] (
    [Id] uniqueidentifier NOT NULL,
    [CaseReviewReportId] uniqueidentifier NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Type] tinyint NOT NULL,
    CONSTRAINT [PK_ReviewPoints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReviewPoints_CaseReviewReports_CaseReviewReportId] FOREIGN KEY ([CaseReviewReportId]) REFERENCES [CaseReviewReports] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Cases] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Governorate] nvarchar(100) NULL,
    [City] nvarchar(100) NULL,
    [ClientId] uniqueidentifier NOT NULL,
    [Status] tinyint NOT NULL,
    [SubmittedAt] datetimeoffset NULL,
    [LawyerId] uniqueidentifier NULL,
    [LastReviewId] uniqueidentifier NULL,
    [ChatId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Cases] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Cases_CaseReviewReports_LastReviewId] FOREIGN KEY ([LastReviewId]) REFERENCES [CaseReviewReports] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Cases_ClientProfile_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [ClientProfile] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Cases_LawyerProfile_LawyerId] FOREIGN KEY ([LawyerId]) REFERENCES [LawyerProfile] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Proposals] (
    [Id] uniqueidentifier NOT NULL,
    [LegalCaseId] uniqueidentifier NOT NULL,
    [ClientUserId] uniqueidentifier NOT NULL,
    [LawyerUserId] uniqueidentifier NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [Status] int NOT NULL,
    [DecisionReason] nvarchar(1000) NULL,
    [RespondedAt] datetimeoffset NULL,
    [ExpiresAt] datetimeoffset NOT NULL,
    [ClosedAt] datetimeoffset NULL,
    [ClosedByUserId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Proposals] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Proposals_Status_Range] CHECK ([Status] BETWEEN 0 AND 6),
    CONSTRAINT [FK_Proposals_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Proposals_AspNetUsers_ClosedByUserId] FOREIGN KEY ([ClosedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Proposals_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Proposals_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ChatConversations] (
    [Id] uniqueidentifier NOT NULL,
    [ProposalId] uniqueidentifier NOT NULL,
    [LegalCaseId] uniqueidentifier NOT NULL,
    [ClientUserId] uniqueidentifier NOT NULL,
    [LawyerUserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [LastMessageAt] datetimeoffset NULL,
    [IsClosed] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ChatConversations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatConversations_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatConversations_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatConversations_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatConversations_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION
);
GO


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
    [AcceptedByClientAt] datetimeoffset NULL,
    [AcceptedByLawyerAt] datetimeoffset NULL,
    [ActivatedAt] datetimeoffset NULL,
    [CompletedAt] datetimeoffset NULL,
    [TerminatedAt] datetimeoffset NULL,
    [TerminationReason] nvarchar(2000) NULL,
    [TerminatedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Contracts_Currency_EGP] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [CK_Contracts_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
    CONSTRAINT [FK_Contracts_AspNetUsers_ClientUserId] FOREIGN KEY ([ClientUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_AspNetUsers_TerminatedByUserId] FOREIGN KEY ([TerminatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_Cases_LegalCaseId] FOREIGN KEY ([LegalCaseId]) REFERENCES [Cases] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Contracts_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ChatMessages] (
    [Id] uniqueidentifier NOT NULL,
    [ConversationId] uniqueidentifier NOT NULL,
    [SenderUserId] uniqueidentifier NULL,
    [Type] int NOT NULL,
    [Content] nvarchar(2000) NOT NULL,
    [SystemCode] nvarchar(100) NULL,
    [RelatedEntityId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ChatMessages_Type_Range] CHECK ([Type] BETWEEN 1 AND 2),
    CONSTRAINT [CK_ChatMessages_UserOrSystem] CHECK (([Type] = 1 AND [SenderUserId] IS NOT NULL AND [SystemCode] IS NULL) OR ([Type] = 2 AND [SenderUserId] IS NULL AND [SystemCode] IS NOT NULL)),
    CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_ChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [ChatConversations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ContractAttachments] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [UploadedByUserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ContractAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractAttachments_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ContractAttachments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ContractAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContractStateHistories] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [PreviousStatus] int NULL,
    [NewStatus] int NOT NULL,
    [Trigger] nvarchar(100) NOT NULL,
    [ActorUserId] uniqueidentifier NULL,
    [Reason] nvarchar(2000) NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ContractStateHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ContractStateHistories_NewStatus_Range] CHECK ([NewStatus] BETWEEN 0 AND 4),
    CONSTRAINT [CK_ContractStateHistories_PreviousStatus_Range] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 4),
    CONSTRAINT [FK_ContractStateHistories_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ContractStateHistories_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_EscrowAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_EscrowAccounts_Currency_EGP] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [CK_EscrowAccounts_NonNegativeTotals] CHECK ([TotalDeposited] >= 0 AND [TotalReleased] >= 0 AND [TotalRefunded] >= 0 AND [TotalFees] >= 0),
    CONSTRAINT [CK_EscrowAccounts_Status_Range] CHECK ([Status] BETWEEN 0 AND 1),
    CONSTRAINT [FK_EscrowAccounts_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Milestones] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Deliverables] nvarchar(max) NULL,
    [Type] int NOT NULL DEFAULT 0,
    [OrderNumber] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [DurationDays] int NULL,
    [DueDate] datetimeoffset NULL,
    [Status] int NOT NULL,
    [AcceptedByClientAt] datetimeoffset NULL,
    [AcceptedByLawyerAt] datetimeoffset NULL,
    [ReadyForFundingAt] datetimeoffset NULL,
    [FundedAt] datetimeoffset NULL,
    [SubmittedAt] datetimeoffset NULL,
    [AutoAcceptEligibleAt] datetimeoffset NULL,
    [AutoAcceptJobId] nvarchar(100) NULL,
    [AcceptedAt] datetimeoffset NULL,
    [AcceptanceSource] int NULL,
    [HoldStartsAt] datetimeoffset NULL,
    [HoldExpiresAt] datetimeoffset NULL,
    [ReleasedAt] datetimeoffset NULL,
    [RefundedAt] datetimeoffset NULL,
    [RejectionReason] nvarchar(2000) NULL,
    [SubmissionVersion] int NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Milestones] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Milestones_Amount_Positive] CHECK ([Amount] > 0),
    CONSTRAINT [CK_Milestones_DurationDays_Range] CHECK ([DurationDays] IS NULL OR [DurationDays] BETWEEN 1 AND 365),
    CONSTRAINT [CK_Milestones_ExpenseFields] CHECK ([Type] <> 1 OR ([Deliverables] IS NULL AND [DurationDays] IS NULL)),
    CONSTRAINT [CK_Milestones_OrderNumber_Positive] CHECK ([OrderNumber] > 0),
    CONSTRAINT [CK_Milestones_Status_Range] CHECK ([Status] BETWEEN 0 AND 10),
    CONSTRAINT [CK_Milestones_SubmissionVersion_Positive] CHECK ([SubmissionVersion] >= 0),
    CONSTRAINT [CK_Milestones_Type_Range] CHECK ([Type] BETWEEN 0 AND 1),
    CONSTRAINT [FK_Milestones_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ChatMessageAttachments] (
    [Id] uniqueidentifier NOT NULL,
    [MessageId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ChatMessageAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessageAttachments_ChatMessages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [ChatMessages] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessageAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE NO ACTION
);
GO


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
    [ResolvedAt] datetimeoffset NULL,
    [ClosedAt] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Disputes] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Disputes_Category_Range] CHECK ([Category] BETWEEN 0 AND 5),
    CONSTRAINT [CK_Disputes_RequestedOutcome_Range] CHECK ([RequestedOutcome] BETWEEN 0 AND 2),
    CONSTRAINT [CK_Disputes_ResolutionType_Range] CHECK ([ResolutionType] IS NULL OR [ResolutionType] BETWEEN 0 AND 2),
    CONSTRAINT [CK_Disputes_Status_Range] CHECK ([Status] BETWEEN 0 AND 4),
    CONSTRAINT [FK_Disputes_AspNetUsers_AssignedModeratorUserId] FOREIGN KEY ([AssignedModeratorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Disputes_AspNetUsers_RaisedByUserId] FOREIGN KEY ([RaisedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Disputes_AspNetUsers_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Disputes_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Disputes_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EscrowHolds] (
    [Id] uniqueidentifier NOT NULL,
    [EscrowAccountId] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [MilestoneId] uniqueidentifier NOT NULL,
    [GrossAmount] decimal(18,2) NOT NULL,
    [PlatformFeeAmount] decimal(18,2) NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [FundedAt] datetimeoffset NOT NULL,
    [HoldStartsAt] datetimeoffset NULL,
    [HoldExpiresAt] datetimeoffset NULL,
    [FrozenAt] datetimeoffset NULL,
    [SettledAt] datetimeoffset NULL,
    [SettlementType] int NULL,
    [ProviderDepositTransactionId] uniqueidentifier NOT NULL,
    [ProviderReleaseTransactionId] uniqueidentifier NULL,
    [ProviderRefundTransactionId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
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
GO


CREATE TABLE [MilestoneChangeRequests] (
    [Id] uniqueidentifier NOT NULL,
    [MilestoneId] uniqueidentifier NOT NULL,
    [RequestedByUserId] uniqueidentifier NOT NULL,
    [ProposedDescription] nvarchar(max) NULL,
    [ProposedDurationDays] int NULL,
    [ProposedDueDate] datetimeoffset NULL,
    [Reason] nvarchar(2000) NOT NULL,
    [Status] int NOT NULL,
    [DecidedByUserId] uniqueidentifier NULL,
    [DecidedAt] datetimeoffset NULL,
    [DecisionReason] nvarchar(2000) NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_MilestoneChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MilestoneChangeRequests_DurationDays_Range] CHECK ([ProposedDurationDays] IS NULL OR [ProposedDurationDays] BETWEEN 1 AND 365),
    CONSTRAINT [CK_MilestoneChangeRequests_Status_Range] CHECK ([Status] BETWEEN 0 AND 3),
    CONSTRAINT [FK_MilestoneChangeRequests_AspNetUsers_DecidedByUserId] FOREIGN KEY ([DecidedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MilestoneChangeRequests_AspNetUsers_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MilestoneChangeRequests_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [MilestoneStateHistories] (
    [Id] uniqueidentifier NOT NULL,
    [MilestoneId] uniqueidentifier NOT NULL,
    [PreviousStatus] int NULL,
    [NewStatus] int NOT NULL,
    [Trigger] nvarchar(100) NOT NULL,
    [ActorUserId] uniqueidentifier NULL,
    [Reason] nvarchar(2000) NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_MilestoneStateHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MilestoneStateHistories_NewStatus_Range] CHECK ([NewStatus] BETWEEN 0 AND 10),
    CONSTRAINT [CK_MilestoneStateHistories_PreviousStatus_Range] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 10),
    CONSTRAINT [FK_MilestoneStateHistories_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MilestoneStateHistories_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [DisputeEvidence] (
    [Id] uniqueidentifier NOT NULL,
    [DisputeId] uniqueidentifier NOT NULL,
    [UploadedByUserId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NULL,
    [Content] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_DisputeEvidence] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DisputeEvidence_FileOrContent] CHECK ([StoredFileId] IS NOT NULL OR [Content] IS NOT NULL),
    CONSTRAINT [FK_DisputeEvidence_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DisputeEvidence_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DisputeEvidence_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE NO ACTION
);
GO


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
    [ResolvedAt] datetimeoffset NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_DisputeResolutions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DisputeResolutions_Amounts_NonNegative] CHECK ([GrossHoldAmount] >= 0 AND [ClientRefundAmount] >= 0 AND [LawyerReleaseAmount] >= 0 AND [PlatformFeeAmount] >= 0),
    CONSTRAINT [CK_DisputeResolutions_Reconciliation] CHECK ([GrossHoldAmount] = [ClientRefundAmount] + [LawyerReleaseAmount] + [PlatformFeeAmount]),
    CONSTRAINT [CK_DisputeResolutions_ResolutionType_Range] CHECK ([ResolutionType] BETWEEN 0 AND 2),
    CONSTRAINT [FK_DisputeResolutions_AspNetUsers_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DisputeResolutions_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LawyerPenalties] (
    [Id] uniqueidentifier NOT NULL,
    [LawyerUserId] uniqueidentifier NOT NULL,
    [DisputeId] uniqueidentifier NOT NULL,
    [PenaltyType] int NOT NULL,
    [Reason] nvarchar(2000) NOT NULL,
    [StartsAt] datetimeoffset NOT NULL,
    [EndsAt] datetimeoffset NULL,
    [CreatedByUserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_LawyerPenalties] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LawyerPenalties_EndAfterStart] CHECK ([EndsAt] IS NULL OR [EndsAt] >= [StartsAt]),
    CONSTRAINT [CK_LawyerPenalties_Type_Range] CHECK ([PenaltyType] BETWEEN 0 AND 3),
    CONSTRAINT [FK_LawyerPenalties_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LawyerPenalties_AspNetUsers_LawyerUserId] FOREIGN KEY ([LawyerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LawyerPenalties_Disputes_DisputeId] FOREIGN KEY ([DisputeId]) REFERENCES [Disputes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [MilestoneSubmissions] (
    [Id] uniqueidentifier NOT NULL,
    [MilestoneId] uniqueidentifier NOT NULL,
    [EscrowHoldId] uniqueidentifier NOT NULL,
    [SubmittedByUserId] uniqueidentifier NOT NULL,
    [Version] int NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [SubmittedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_MilestoneSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MilestoneSubmissions_Version_Positive] CHECK ([Version] > 0),
    CONSTRAINT [FK_MilestoneSubmissions_AspNetUsers_SubmittedByUserId] FOREIGN KEY ([SubmittedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MilestoneSubmissions_EscrowHolds_EscrowHoldId] FOREIGN KEY ([EscrowHoldId]) REFERENCES [EscrowHolds] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MilestoneSubmissions_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PaymentTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [MilestoneId] uniqueidentifier NULL,
    [EscrowHoldId] uniqueidentifier NULL,
    [OperationType] int NOT NULL,
    [ProviderName] varchar(100) NOT NULL,
    [ProviderTransactionId] varchar(200) NULL,
    [ProviderRelatedTransactionId] varchar(200) NULL,
    [ProviderStatus] varchar(100) NULL,
    [ProviderObjectType] varchar(100) NULL,
    [ProviderAmountMinor] bigint NULL,
    [ProviderCurrency] varchar(3) NULL,
    [IdempotencyKey] varchar(200) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] varchar(3) NOT NULL DEFAULT 'EGP',
    [Status] int NOT NULL,
    [FailureReason] nvarchar(2000) NULL,
    [ProviderAttemptCount] int NOT NULL DEFAULT 0,
    [NextRetryAt] datetimeoffset NULL,
    [RequiresManualAction] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ManualActionRequiredAt] datetimeoffset NULL,
    [ProcessedAt] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PaymentTransactions_Amount_Positive] CHECK ([Amount] > 0),
    CONSTRAINT [CK_PaymentTransactions_CompletedDepositRequiresHold] CHECK (NOT ([OperationType] = 0 AND [Status] = 1) OR [EscrowHoldId] IS NOT NULL),
    CONSTRAINT [CK_PaymentTransactions_Currency_EGP] CHECK ([Currency] = 'EGP'),
    CONSTRAINT [CK_PaymentTransactions_ManualActionTimestamp] CHECK ([RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL),
    CONSTRAINT [CK_PaymentTransactions_MilestoneRequiredForMoneyOperations] CHECK ([OperationType] = 3 OR [MilestoneId] IS NOT NULL),
    CONSTRAINT [CK_PaymentTransactions_OperationType_Range] CHECK ([OperationType] BETWEEN 0 AND 3),
    CONSTRAINT [CK_PaymentTransactions_ProviderAttemptCount_NonNegative] CHECK ([ProviderAttemptCount] >= 0),
    CONSTRAINT [CK_PaymentTransactions_Status_Range] CHECK ([Status] BETWEEN 0 AND 2),
    CONSTRAINT [FK_PaymentTransactions_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentTransactions_EscrowHolds_EscrowHoldId] FOREIGN KEY ([EscrowHoldId]) REFERENCES [EscrowHolds] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentTransactions_Milestones_MilestoneId] FOREIGN KEY ([MilestoneId]) REFERENCES [Milestones] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [MilestoneSubmissionAttachments] (
    [Id] uniqueidentifier NOT NULL,
    [MilestoneSubmissionId] uniqueidentifier NOT NULL,
    [StoredFileId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_MilestoneSubmissionAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MilestoneSubmissionAttachments_MilestoneSubmissions_MilestoneSubmissionId] FOREIGN KEY ([MilestoneSubmissionId]) REFERENCES [MilestoneSubmissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MilestoneSubmissionAttachments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE CASCADE
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
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
GO


CREATE TABLE [PaymentWebhookEvents] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] varchar(200) NOT NULL,
    [ProviderCode] varchar(50) NOT NULL,
    [EventType] varchar(100) NOT NULL,
    [ProviderObjectId] varchar(200) NULL,
    [ConnectedAccountId] varchar(200) NULL,
    [PaymentTransactionId] uniqueidentifier NULL,
    [ReceivedAt] datetimeoffset NOT NULL,
    [ProcessedAt] datetimeoffset NULL,
    [ProcessingError] nvarchar(1000) NULL,
    CONSTRAINT [PK_PaymentWebhookEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentWebhookEvents_PaymentTransactions_PaymentTransactionId] FOREIGN KEY ([PaymentTransactionId]) REFERENCES [PaymentTransactions] ([Id]) ON DELETE NO ACTION
);
GO


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
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_WalletAdjustments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WalletAdjustments_Balances_NonNegative] CHECK ([PendingBalanceBefore] >= 0 AND [PendingBalanceAfter] >= 0 AND [AvailableBalanceBefore] >= 0 AND [AvailableBalanceAfter] >= 0),
    CONSTRAINT [CK_WalletAdjustments_Delta_NonZero] CHECK ([PendingBalanceDelta] <> 0 OR [AvailableBalanceDelta] <> 0),
    CONSTRAINT [FK_WalletAdjustments_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WalletAdjustments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WalletAdjustments_EscrowAccounts_EscrowAccountId] FOREIGN KEY ([EscrowAccountId]) REFERENCES [EscrowAccounts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WalletAdjustments_EscrowLedgerEntries_LedgerEntryId] FOREIGN KEY ([LedgerEntryId]) REFERENCES [EscrowLedgerEntries] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WalletAdjustments_LawyerWallets_LawyerWalletId] FOREIGN KEY ([LawyerWalletId]) REFERENCES [LawyerWallets] ([Id]) ON DELETE NO ACTION
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsDeleted', N'LastModifiedBy', N'NameAr', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LegalArticleCategories]'))
    SET IDENTITY_INSERT [LegalArticleCategories] ON;
INSERT INTO [LegalArticleCategories] ([Id], [Code], [CreatedAt], [CreatedBy], [Description], [IsDeleted], [LastModifiedBy], [NameAr], [UpdatedAt])
VALUES ('a0b711e7-f1e1-450a-9d9f-3d12c5b96904', N'criminal', '2026-01-01T00:00:00.0000000+00:00', NULL, NULL, CAST(0 AS bit), NULL, N'القانون الجنائي', '2026-01-01T00:00:00.0000000+00:00'),
('b1b711e7-f1e1-450a-9d9f-3d12c5b96903', N'labor', '2026-01-01T00:00:00.0000000+00:00', NULL, NULL, CAST(0 AS bit), NULL, N'نظام العمل', '2026-01-01T00:00:00.0000000+00:00'),
('c2b711e7-f1e1-450a-9d9f-3d12c5b96902', N'civil', '2026-01-01T00:00:00.0000000+00:00', NULL, NULL, CAST(0 AS bit), NULL, N'القانون المدني', '2026-01-01T00:00:00.0000000+00:00'),
('d3b711e7-f1e1-450a-9d9f-3d12c5b96901', N'commercial', '2026-01-01T00:00:00.0000000+00:00', NULL, NULL, CAST(0 AS bit), NULL, N'القانون التجاري', '2026-01-01T00:00:00.0000000+00:00');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsDeleted', N'LastModifiedBy', N'NameAr', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LegalArticleCategories]'))
    SET IDENTITY_INSERT [LegalArticleCategories] OFF;
GO


CREATE INDEX [IX_AgentConversations_CaseId] ON [AgentConversations] ([CaseId]);
GO


CREATE INDEX [IX_AgentConversations_UserId_IsDeleted_UpdatedAt] ON [AgentConversations] ([UserId], [IsDeleted], [UpdatedAt]);
GO


CREATE INDEX [IX_AgentMessages_ConversationId_CreatedAt] ON [AgentMessages] ([ConversationId], [CreatedAt]);
GO


CREATE INDEX [IX_ArticleComments_ArticleId] ON [ArticleComments] ([ArticleId]);
GO


CREATE INDEX [IX_ArticleComments_UserId] ON [ArticleComments] ([UserId]);
GO


CREATE INDEX [IX_ArticleLikes_UserId] ON [ArticleLikes] ([UserId]);
GO


CREATE INDEX [IX_ArticleReports_ArticleId] ON [ArticleReports] ([ArticleId]);
GO


CREATE INDEX [IX_ArticleReports_ReporterId] ON [ArticleReports] ([ReporterId]);
GO


CREATE INDEX [IX_ArticleViews_ArticleId] ON [ArticleViews] ([ArticleId]);
GO


CREATE INDEX [IX_ArticleViews_UserId] ON [ArticleViews] ([UserId]);
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


CREATE UNIQUE INDEX [IX_ApplicationUser_Email] ON [AspNetUsers] ([Email]);
GO


CREATE UNIQUE INDEX [IX_ApplicationUser_NationalNumber] ON [AspNetUsers] ([NationalNumber]) WHERE [NationalNumber] IS NOT NULL;
GO


CREATE INDEX [IX_ApplicationUser_Status] ON [AspNetUsers] ([Status]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_CaseDocuments_CaseId] ON [CaseDocuments] ([CaseId]);
GO


CREATE UNIQUE INDEX [IX_CaseDocuments_StoredFileId] ON [CaseDocuments] ([StoredFileId]);
GO


CREATE UNIQUE INDEX [IX_CaseProfiles_CaseId] ON [CaseProfiles] ([CaseId]);
GO


CREATE INDEX [IX_CaseRecommendation_CaseId_Rank] ON [CaseRecommendations] ([CaseId], [Rank]);
GO


CREATE INDEX [IX_CaseRecommendations_LawyerId] ON [CaseRecommendations] ([LawyerId]);
GO


CREATE INDEX [IX_CaseReviewReports_CaseId] ON [CaseReviewReports] ([CaseId]);
GO


CREATE INDEX [IX_Cases_ChatId] ON [Cases] ([ChatId]);
GO


CREATE INDEX [IX_Cases_ClientId] ON [Cases] ([ClientId]);
GO


CREATE INDEX [IX_Cases_LastReviewId] ON [Cases] ([LastReviewId]);
GO


CREATE INDEX [IX_Cases_LawyerId] ON [Cases] ([LawyerId]);
GO


CREATE INDEX [IX_ChatConversations_Client_UpdatedAt] ON [ChatConversations] ([ClientUserId], [UpdatedAt]);
GO


CREATE INDEX [IX_ChatConversations_Lawyer_UpdatedAt] ON [ChatConversations] ([LawyerUserId], [UpdatedAt]);
GO


CREATE INDEX [IX_ChatConversations_LegalCaseId] ON [ChatConversations] ([LegalCaseId]);
GO


CREATE UNIQUE INDEX [UX_ChatConversations_ProposalId] ON [ChatConversations] ([ProposalId]);
GO


CREATE INDEX [IX_ChatMessageAttachments_MessageId] ON [ChatMessageAttachments] ([MessageId]);
GO


CREATE UNIQUE INDEX [UX_ChatMessageAttachments_StoredFileId] ON [ChatMessageAttachments] ([StoredFileId]);
GO


CREATE INDEX [IX_ChatMessages_Conversation_CreatedAt] ON [ChatMessages] ([ConversationId], [CreatedAt]);
GO


CREATE INDEX [IX_ChatMessages_SenderUserId] ON [ChatMessages] ([SenderUserId]);
GO


CREATE UNIQUE INDEX [UX_ClientPaymentCustomers_Client_Provider] ON [ClientPaymentCustomers] ([ClientUserId], [ProviderCode]);
GO


CREATE UNIQUE INDEX [UX_ClientPaymentCustomers_ProviderCustomer] ON [ClientPaymentCustomers] ([ProviderCode], [ProviderCustomerId]);
GO


CREATE INDEX [IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc] ON [ConsultationAvailabilitySlots] ([LawyerId], [StartAtUtc], [EndAtUtc]);
GO


CREATE UNIQUE INDEX [IX_ConsultationAvailabilitySlots_OfferingId_StartAtUtc] ON [ConsultationAvailabilitySlots] ([OfferingId], [StartAtUtc]) WHERE [Status] <> 4;
GO


CREATE INDEX [IX_ConsultationBookings_ClientId_Status_StartAtUtc] ON [ConsultationBookings] ([ClientId], [Status], [StartAtUtc]);
GO


CREATE INDEX [IX_ConsultationBookings_LawyerId_Status_StartAtUtc] ON [ConsultationBookings] ([LawyerId], [Status], [StartAtUtc]);
GO


CREATE INDEX [IX_ConsultationBookings_OfferingId] ON [ConsultationBookings] ([OfferingId]);
GO


CREATE UNIQUE INDEX [IX_ConsultationBookings_SlotId] ON [ConsultationBookings] ([SlotId]) WHERE [Status] IN (0,1,2,3,6);
GO


CREATE UNIQUE INDEX [IX_ConsultationEscrowHolds_BookingId] ON [ConsultationEscrowHolds] ([BookingId]);
GO


CREATE INDEX [IX_ConsultationEscrowHolds_DepositTransactionId] ON [ConsultationEscrowHolds] ([DepositTransactionId]);
GO


CREATE INDEX [IX_ConsultationLedgerEntries_BookingId_CreatedAt] ON [ConsultationLedgerEntries] ([BookingId], [CreatedAt]);
GO


CREATE INDEX [IX_ConsultationLedgerEntries_PaymentTransactionId] ON [ConsultationLedgerEntries] ([PaymentTransactionId]);
GO


CREATE UNIQUE INDEX [IX_ConsultationOfferingInclusions_OfferingId_SortOrder] ON [ConsultationOfferingInclusions] ([OfferingId], [SortOrder]);
GO


CREATE INDEX [IX_ConsultationOfferings_LawyerId_IsActive] ON [ConsultationOfferings] ([LawyerId], [IsActive]);
GO


CREATE INDEX [IX_ConsultationOfferings_Mode_Specialization_IsActive] ON [ConsultationOfferings] ([Mode], [Specialization], [IsActive]);
GO


CREATE INDEX [IX_ConsultationPaymentTransactions_BookingId_OperationType_Status] ON [ConsultationPaymentTransactions] ([BookingId], [OperationType], [Status]);
GO


CREATE UNIQUE INDEX [IX_ConsultationPaymentTransactions_ProviderName_IdempotencyKey] ON [ConsultationPaymentTransactions] ([ProviderName], [IdempotencyKey]);
GO


CREATE INDEX [IX_ConsultationPaymentTransactions_ProviderTransactionId] ON [ConsultationPaymentTransactions] ([ProviderTransactionId]);
GO


CREATE INDEX [IX_ContractAttachments_ContractId] ON [ContractAttachments] ([ContractId]);
GO


CREATE INDEX [IX_ContractAttachments_StoredFileId] ON [ContractAttachments] ([StoredFileId]);
GO


CREATE INDEX [IX_ContractAttachments_UploadedByUserId] ON [ContractAttachments] ([UploadedByUserId]);
GO


CREATE INDEX [IX_ContractFileAccessAudits_ActorUserId] ON [ContractFileAccessAudits] ([ActorUserId]);
GO


CREATE INDEX [IX_ContractFileAccessAudits_File_Entity_Time] ON [ContractFileAccessAudits] ([StoredFileId], [RelatedEntityId], [AccessedAt]);
GO


CREATE INDEX [IX_Contracts_ClientUserId] ON [Contracts] ([ClientUserId]);
GO


CREATE INDEX [IX_Contracts_LawyerUserId] ON [Contracts] ([LawyerUserId]);
GO


CREATE INDEX [IX_Contracts_Status] ON [Contracts] ([Status]);
GO


CREATE INDEX [IX_Contracts_TerminatedByUserId] ON [Contracts] ([TerminatedByUserId]);
GO


CREATE UNIQUE INDEX [UX_Contracts_ActiveCase] ON [Contracts] ([LegalCaseId]) WHERE [Status] = 1;
GO


CREATE UNIQUE INDEX [UX_Contracts_ProposalId] ON [Contracts] ([ProposalId]);
GO


CREATE INDEX [IX_ContractStateHistories_ActorUserId] ON [ContractStateHistories] ([ActorUserId]);
GO


CREATE INDEX [IX_ContractStateHistories_ContractId_CreatedAt] ON [ContractStateHistories] ([ContractId], [CreatedAt]);
GO


CREATE INDEX [IX_DisputeEvidence_DisputeId] ON [DisputeEvidence] ([DisputeId]);
GO


CREATE INDEX [IX_DisputeEvidence_StoredFileId] ON [DisputeEvidence] ([StoredFileId]);
GO


CREATE INDEX [IX_DisputeEvidence_UploadedByUserId] ON [DisputeEvidence] ([UploadedByUserId]);
GO


CREATE INDEX [IX_DisputeResolutions_ResolvedByUserId] ON [DisputeResolutions] ([ResolvedByUserId]);
GO


CREATE UNIQUE INDEX [UX_DisputeResolutions_DisputeId] ON [DisputeResolutions] ([DisputeId]);
GO


CREATE INDEX [IX_Disputes_AssignedModeratorUserId] ON [Disputes] ([AssignedModeratorUserId]);
GO


CREATE INDEX [IX_Disputes_ContractId] ON [Disputes] ([ContractId]);
GO


CREATE INDEX [IX_Disputes_RaisedByUserId] ON [Disputes] ([RaisedByUserId]);
GO


CREATE INDEX [IX_Disputes_ResolvedByUserId] ON [Disputes] ([ResolvedByUserId]);
GO


CREATE INDEX [IX_Disputes_Status_CreatedAt] ON [Disputes] ([Status], [CreatedAt]);
GO


CREATE UNIQUE INDEX [UX_Disputes_OpenPerMilestone] ON [Disputes] ([MilestoneId]) WHERE [Status] IN (0, 1, 2);
GO


CREATE UNIQUE INDEX [UX_EscrowAccounts_ContractId] ON [EscrowAccounts] ([ContractId]);
GO


CREATE INDEX [IX_EscrowHolds_ContractId] ON [EscrowHolds] ([ContractId]);
GO


CREATE INDEX [IX_EscrowHolds_EscrowAccountId] ON [EscrowHolds] ([EscrowAccountId]);
GO


CREATE INDEX [IX_EscrowHolds_HoldExpiresAt_Status] ON [EscrowHolds] ([HoldExpiresAt], [Status]);
GO


CREATE UNIQUE INDEX [UX_EscrowHolds_MilestoneId] ON [EscrowHolds] ([MilestoneId]);
GO


CREATE INDEX [IX_EscrowLedgerEntries_AccountId_CreatedAt] ON [EscrowLedgerEntries] ([EscrowAccountId], [CreatedAt]);
GO


CREATE INDEX [IX_EscrowLedgerEntries_CreatedByUserId] ON [EscrowLedgerEntries] ([CreatedByUserId]);
GO


CREATE INDEX [IX_EscrowLedgerEntries_EscrowHoldId] ON [EscrowLedgerEntries] ([EscrowHoldId]);
GO


CREATE INDEX [IX_EscrowLedgerEntries_PaymentTransactionId] ON [EscrowLedgerEntries] ([PaymentTransactionId]);
GO


CREATE INDEX [IX_IdempotencyRecords_Status_ExpiresAt] ON [IdempotencyRecords] ([Status], [ExpiresAt]);
GO


CREATE UNIQUE INDEX [UX_IdempotencyRecords_HoldSettlement] ON [IdempotencyRecords] ([ResourceType], [ResourceId]) WHERE [ResourceType] = 'EscrowHoldSettlement';
GO


CREATE UNIQUE INDEX [UX_IdempotencyRecords_UserId_Key] ON [IdempotencyRecords] ([UserId], [Key]);
GO


CREATE INDEX [IX_LawDocuments_DocumentTitle] ON [LawDocuments] ([DocumentTitle]);
GO


CREATE INDEX [IX_LawDocuments_Language] ON [LawDocuments] ([Language]);
GO


CREATE INDEX [IX_LawDocuments_Status] ON [LawDocuments] ([Status]);
GO


CREATE UNIQUE INDEX [UX_LawyerPayoutAccounts_Lawyer_Provider] ON [LawyerPayoutAccounts] ([LawyerUserId], [ProviderCode]);
GO


CREATE UNIQUE INDEX [UX_LawyerPayoutAccounts_ProviderAccount] ON [LawyerPayoutAccounts] ([ProviderCode], [ProviderAccountId]);
GO


CREATE INDEX [IX_LawyerPenalties_CreatedByUserId] ON [LawyerPenalties] ([CreatedByUserId]);
GO


CREATE INDEX [IX_LawyerPenalties_DisputeId] ON [LawyerPenalties] ([DisputeId]);
GO


CREATE INDEX [IX_LawyerPenalties_LawyerUserId_StartsAt] ON [LawyerPenalties] ([LawyerUserId], [StartsAt]);
GO


CREATE UNIQUE INDEX [IX_LawyerSpecialization_LawyerId_Specialization] ON [LawyerSpecializations] ([LawyerProfileUserId], [Specialization]);
GO


CREATE UNIQUE INDEX [UX_LawyerWallets_LawyerUserId] ON [LawyerWallets] ([LawyerUserId]);
GO


CREATE UNIQUE INDEX [IX_LegalArticleCategories_Code] ON [LegalArticleCategories] ([Code]);
GO


CREATE INDEX [IX_LegalArticles_AuthorId] ON [LegalArticles] ([AuthorId]);
GO


CREATE INDEX [IX_LegalArticles_CategoryId] ON [LegalArticles] ([CategoryId]);
GO


CREATE INDEX [IX_LegalSpecializations_CategoryId] ON [LegalSpecializations] ([CategoryId]);
GO


CREATE INDEX [IX_MilestoneChangeRequests_DecidedByUserId] ON [MilestoneChangeRequests] ([DecidedByUserId]);
GO


CREATE INDEX [IX_MilestoneChangeRequests_RequestedByUserId] ON [MilestoneChangeRequests] ([RequestedByUserId]);
GO


CREATE UNIQUE INDEX [UX_MilestoneChangeRequests_Pending] ON [MilestoneChangeRequests] ([MilestoneId], [Status]) WHERE [Status] = 0;
GO


CREATE INDEX [IX_Milestones_ContractId_Status] ON [Milestones] ([ContractId], [Status]);
GO


CREATE INDEX [IX_Milestones_Status_AutoAcceptEligibleAt] ON [Milestones] ([Status], [AutoAcceptEligibleAt]);
GO


CREATE INDEX [IX_Milestones_Type_Status_FundedAt] ON [Milestones] ([Type], [Status], [FundedAt]);
GO


CREATE UNIQUE INDEX [UX_Milestones_ContractId_OrderNumber] ON [Milestones] ([ContractId], [OrderNumber]);
GO


CREATE INDEX [IX_MilestoneStateHistories_ActorUserId] ON [MilestoneStateHistories] ([ActorUserId]);
GO


CREATE INDEX [IX_MilestoneStateHistories_MilestoneId_CreatedAt] ON [MilestoneStateHistories] ([MilestoneId], [CreatedAt]);
GO


CREATE INDEX [IX_MilestoneSubmissionAttachments_MilestoneSubmissionId] ON [MilestoneSubmissionAttachments] ([MilestoneSubmissionId]);
GO


CREATE INDEX [IX_MilestoneSubmissionAttachments_StoredFileId] ON [MilestoneSubmissionAttachments] ([StoredFileId]);
GO


CREATE INDEX [IX_MilestoneSubmissions_EscrowHoldId] ON [MilestoneSubmissions] ([EscrowHoldId]);
GO


CREATE INDEX [IX_MilestoneSubmissions_SubmittedByUserId] ON [MilestoneSubmissions] ([SubmittedByUserId]);
GO


CREATE UNIQUE INDEX [UX_MilestoneSubmissions_MilestoneId_Version] ON [MilestoneSubmissions] ([MilestoneId], [Version]);
GO


CREATE INDEX [IX_Notifications_Recipient_Sequence] ON [Notifications] ([RecipientUserId], [Sequence] DESC);
GO


CREATE INDEX [IX_Notifications_Recipient_Unread_Sequence] ON [Notifications] ([RecipientUserId], [ReadAtUtc], [Sequence] DESC) WHERE [ReadAtUtc] IS NULL;
GO


CREATE UNIQUE INDEX [UX_Notifications_Sequence] ON [Notifications] ([Sequence]);
GO


CREATE UNIQUE INDEX [UX_Notifications_Source_Recipient_Type] ON [Notifications] ([SourceEventId], [RecipientUserId], [Type]);
GO


CREATE INDEX [IX_OutboxMessages_Aggregate] ON [OutboxMessages] ([AggregateType], [AggregateId]);
GO


CREATE INDEX [IX_OutboxMessages_Status_AvailableAt] ON [OutboxMessages] ([Status], [AvailableAt]);
GO


CREATE INDEX [IX_PaymentTransactions_ContractId_Status] ON [PaymentTransactions] ([ContractId], [Status]);
GO


CREATE INDEX [IX_PaymentTransactions_EscrowHoldId] ON [PaymentTransactions] ([EscrowHoldId]);
GO


CREATE INDEX [IX_PaymentTransactions_MilestoneId_Status] ON [PaymentTransactions] ([MilestoneId], [Status]);
GO


CREATE INDEX [IX_PaymentTransactions_ReconciliationQueue] ON [PaymentTransactions] ([Status], [RequiresManualAction], [CreatedAt], [Id]);
GO


CREATE INDEX [IX_PaymentTransactions_ReleaseRecovery] ON [PaymentTransactions] ([Status], [OperationType], [RequiresManualAction], [NextRetryAt]);
GO


CREATE UNIQUE INDEX [UX_PaymentTransactions_IdempotencyKey] ON [PaymentTransactions] ([IdempotencyKey]);
GO


CREATE UNIQUE INDEX [UX_PaymentTransactions_ProviderTransaction] ON [PaymentTransactions] ([ProviderName], [ProviderTransactionId]) WHERE [ProviderTransactionId] IS NOT NULL;
GO


CREATE INDEX [IX_PaymentWebhookEvents_PaymentTransactionId] ON [PaymentWebhookEvents] ([PaymentTransactionId]);
GO


CREATE UNIQUE INDEX [UX_PaymentWebhookEvents_EventId] ON [PaymentWebhookEvents] ([EventId]);
GO


CREATE INDEX [IX_Proposals_ClientUserId] ON [Proposals] ([ClientUserId]);
GO


CREATE INDEX [IX_Proposals_ClosedByUserId] ON [Proposals] ([ClosedByUserId]);
GO


CREATE INDEX [IX_Proposals_LawyerUserId] ON [Proposals] ([LawyerUserId]);
GO


CREATE UNIQUE INDEX [IX_Proposals_LegalCaseId_LawyerUserId] ON [Proposals] ([LegalCaseId], [LawyerUserId]) WHERE [Status] IN (0, 1);
GO


CREATE INDEX [IX_Proposals_LegalCaseId_Status] ON [Proposals] ([LegalCaseId], [Status]);
GO


CREATE INDEX [IX_Proposals_Status_ExpiresAt] ON [Proposals] ([Status], [ExpiresAt]);
GO


CREATE INDEX [IX_ReviewPoints_CaseReviewReportId] ON [ReviewPoints] ([CaseReviewReportId]);
GO


CREATE UNIQUE INDEX [IX_UserVerificationDocuments_StoredFileId] ON [UserVerificationDocuments] ([StoredFileId]);
GO


CREATE INDEX [IX_UserVerificationDocuments_UserId] ON [UserVerificationDocuments] ([UserId]);
GO


CREATE INDEX [IX_WalletAdjustments_ContractId] ON [WalletAdjustments] ([ContractId]);
GO


CREATE INDEX [IX_WalletAdjustments_CreatedByUserId] ON [WalletAdjustments] ([CreatedByUserId]);
GO


CREATE INDEX [IX_WalletAdjustments_EscrowAccountId] ON [WalletAdjustments] ([EscrowAccountId]);
GO


CREATE UNIQUE INDEX [IX_WalletAdjustments_LedgerEntryId] ON [WalletAdjustments] ([LedgerEntryId]);
GO


CREATE INDEX [IX_WalletAdjustments_WalletId_CreatedAt] ON [WalletAdjustments] ([LawyerWalletId], [CreatedAt]);
GO


CREATE INDEX [IX_WithdrawalRequests_LawyerPayoutAccountId] ON [WithdrawalRequests] ([LawyerPayoutAccountId]);
GO


CREATE INDEX [IX_WithdrawalRequests_LawyerUserId_Status] ON [WithdrawalRequests] ([LawyerUserId], [Status]);
GO


CREATE INDEX [IX_WithdrawalRequests_ReconciliationQueue] ON [WithdrawalRequests] ([Status], [RequiresManualAction], [RequestedAt], [Id]);
GO


CREATE UNIQUE INDEX [UX_WithdrawalRequests_IdempotencyKey] ON [WithdrawalRequests] ([IdempotencyKey]);
GO


ALTER TABLE [AgentConversations] ADD CONSTRAINT [FK_AgentConversations_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE SET NULL;
GO


ALTER TABLE [CaseDocuments] ADD CONSTRAINT [FK_CaseDocuments_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [CaseProfiles] ADD CONSTRAINT [FK_CaseProfiles_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [CaseRecommendations] ADD CONSTRAINT [FK_CaseRecommendations_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [CaseReviewReports] ADD CONSTRAINT [FK_CaseReviewReports_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_ChatConversations_ChatId] FOREIGN KEY ([ChatId]) REFERENCES [ChatConversations] ([Id]) ON DELETE NO ACTION;
GO


