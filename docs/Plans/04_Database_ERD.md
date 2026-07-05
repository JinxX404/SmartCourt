# Smart Court — Database ERD

> **Version:** 2.0 (Enhanced) | **Date:** 2026-07-03
> **Source:** `schema.md` + `schema_architectural_review.md` recommendations
> **Database:** SQL Server | **ORM:** EF Core 8

---

## Master Overview

```mermaid
erDiagram
    %% Module 1 - Identity
    AspNetUsers ||--o| ClientProfile : "has"
    AspNetUsers ||--o| LawyerProfile : "has"
    AspNetUsers ||--o{ StoredFile : "uploads"
    LawyerProfile ||--o{ LawyerSpecialization : "has"
    LawyerSpecialization }o--|| LegalCategory : "references"
    
    %% Module 2 - Cases & AI
    ClientProfile ||--o{ LegalCase : "creates"
    LegalCase ||--o{ AIAnalysis : "analyzed by"
    LegalCase ||--o{ LawyerMatch : "matched"
    LegalCase ||--o{ CaseAttachment : "has"
    AIAnalysis }o--o| LegalCategory : "classified as"
    
    %% Module 3 - Proposals & Communication
    LegalCase ||--o{ Proposal : "receives"
    Proposal ||--|| Conversation : "creates"
    Conversation ||--o{ ConversationParticipant : "has"
    Conversation ||--o{ Message : "contains"
    Message ||--o{ MessageAttachment : "has"
    
    %% Module 4 - Contracts & Payments
    Proposal ||--o| Contract : "becomes"
    Contract ||--o{ Milestone : "has"
    Contract ||--o{ ScheduledPayment : "has"
    Contract ||--|| EscrowAccount : "has"
    EscrowAccount ||--o{ EscrowTransaction : "ledger"
    Contract ||--o{ PaymentRelease : "releases"
    PaymentRelease ||--o{ PaymentTransaction : "processes"
    
    %% Module 5 - Reviews & Disputes
    Contract ||--o{ Review : "reviewed"
    Contract ||--o| Dispute : "disputed"
    Dispute ||--o{ DisputeAttachment : "evidence"
    
    %% Module 6 - AI Assistant
    AspNetUsers ||--o{ AIConversation : "chats"
    AIConversation ||--o{ AIMessage : "messages"
    
    %% Module 7 - Knowledge Base
    LawyerProfile ||--o{ LegalArticle : "authors"
    LegalArticle ||--o{ LegalArticleCategory : "categorized"
    LegalArticle ||--o{ LegalArticleAttachment : "has"
    
    %% Module 8 - Notifications
    AspNetUsers ||--o{ UserNotification : "receives"
    Notification ||--o{ UserNotification : "delivered to"
    AspNetUsers ||--o| NotificationPreference : "configures"
    
    %% Cross-cutting
    AspNetUsers ||--o{ StatusChangeLog : "triggers"
```

---

## Module 1 — Identity & User Management

```mermaid
erDiagram
    AspNetUsers {
        uuid Id PK
        varchar UserName
        varchar Email UK
        varchar PhoneNumber
        varchar FirstName
        varchar LastName
        uuid ProfilePictureFileId FK "nullable"
        bool IsActive
        bool IsDeleted "NEW - soft delete"
        datetime DeletedAt "NEW - nullable"
        datetime LastLoginAt "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ClientProfile {
        uuid UserId PK, FK
        datetime DateOfBirth "nullable"
        uuid NationalIdFrontFileId FK "nullable"
        uuid NationalIdBackFileId FK "nullable"
        int NationalIdVerificationStatus "CHECK 0-3"
        uuid NationalIdReviewedByUserId FK "nullable"
        datetime NationalIdVerifiedAt "nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    LawyerProfile {
        uuid UserId PK, FK
        text OfficeAddress "nullable"
        text Bio "nullable"
        int YearsOfExperience
        bool IsAvailable
        uuid NationalIdFrontFileId FK "nullable"
        uuid NationalIdBackFileId FK "nullable"
        int NationalIdVerificationStatus "CHECK 0-3"
        uuid NationalIdReviewedByUserId FK "nullable"
        datetime NationalIdVerifiedAt "nullable"
        uuid BarCardFrontFileId FK "nullable"
        uuid BarCardBackFileId FK "nullable"
        int BarCardVerificationStatus "CHECK 0-3"
        uuid BarCardReviewedByUserId FK "nullable"
        datetime BarCardVerifiedAt "nullable"
        decimal AverageRating "NEW - denormalized"
        int TotalReviews "NEW - denormalized"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    StoredFile {
        uuid Id PK
        varchar OriginalFileName
        varchar StoredFileName
        varchar ContentType
        varchar Extension
        bigint FileSize
        text StoragePath
        varchar StorageProvider "NEW - Local/S3/Azure"
        varchar Checksum "NEW - SHA256 nullable"
        int SensitivityLevel "NEW - 0-3"
        uuid UploadedByUserId FK
        bool IsDeleted "NEW - soft delete"
        datetime DeletedAt "NEW - nullable"
        datetime CreatedAt
    }
    
    LegalCategory {
        uuid Id PK
        varchar Name
        text Description
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    LawyerSpecialization {
        uuid LawyerUserId PK, FK
        uuid LegalCategoryId PK, FK
    }
    
    AspNetUsers ||--o| ClientProfile : "1:0..1"
    AspNetUsers ||--o| LawyerProfile : "1:0..1"
    AspNetUsers ||--o{ StoredFile : "uploads"
    LawyerProfile ||--o{ LawyerSpecialization : "has"
    LawyerSpecialization }o--|| LegalCategory : "references"
```

---

## Module 2 — Legal Cases & AI

```mermaid
erDiagram
    LegalCase {
        uuid Id PK
        uuid ClientUserId FK
        varchar Title
        text Description
        text CaseLocation
        int Status "CHECK 0-4"
        uuid AssignedLawyerUserId FK "NEW - nullable, denorm"
        datetime FinalSubmittedAt "nullable"
        bool IsDeleted "NEW - soft delete"
        datetime DeletedAt "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    AIAnalysis {
        uuid Id PK
        uuid LegalCaseId FK
        int AnalysisNumber
        uuid LegalCategoryId FK "nullable"
        text StrengthPoints
        text WeakPoints
        text MissingInformation
        text Recommendations
        text OverallAssessment
        decimal ConfidenceScore "CHECK 0.0-1.0"
        text RawResponse "NEW - full JSON backup"
        varchar ModelName
        int PromptTokens
        int CompletionTokens
        int TotalTokens
        datetime CreatedAt
    }
    
    LawyerMatch {
        uuid Id PK
        uuid LegalCaseId FK
        uuid LawyerUserId FK
        uuid AIAnalysisId FK "NEW - tracks which analysis"
        decimal MatchScore
        text MatchReason
        int Rank
        datetime CreatedAt
    }
    
    CaseAttachment {
        uuid Id PK
        uuid LegalCaseId FK
        uuid StoredFileId FK
        uuid UploadedByUserId FK
        datetime CreatedAt
    }
    
    LegalCase ||--o{ AIAnalysis : "1:N"
    LegalCase ||--o{ LawyerMatch : "1:N"
    LegalCase ||--o{ CaseAttachment : "1:N"
    AIAnalysis }o--o| LegalCategory : "classified"
    LawyerMatch }o--o| AIAnalysis : "NEW - from analysis"
```

---

## Module 3 — Proposals & Communication

```mermaid
erDiagram
    Proposal {
        uuid Id PK
        uuid LegalCaseId FK
        uuid ClientUserId FK
        uuid LawyerUserId FK
        int Status "CHECK 0-2"
        text RejectionReason "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Conversation {
        uuid Id PK
        uuid ProposalId FK UK
        bool IsClosed
        datetime ClosedAt "nullable"
        datetime LastMessageAt "NEW - denormalized"
        varchar LastMessagePreview "NEW - first 100 chars"
        uuid LastMessageSenderUserId FK "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ConversationParticipant {
        uuid ConversationId PK, FK
        uuid UserId PK, FK
        datetime JoinedAt
        datetime LeftAt "NEW - nullable"
    }
    
    Message {
        uuid Id PK
        uuid ConversationId FK
        uuid SenderUserId FK
        int MessageType "CHECK 0-4"
        text Content "nullable"
        bool IsEdited
        datetime EditedAt "nullable"
        bool IsDeleted "NEW - soft delete"
        datetime DeletedAt "NEW - nullable"
        datetime CreatedAt
    }
    
    MessageAttachment {
        uuid Id PK
        uuid MessageId FK
        uuid StoredFileId FK
        datetime CreatedAt
    }
    
    Proposal ||--|| Conversation : "1:1"
    Conversation ||--o{ ConversationParticipant : "1:N"
    Conversation ||--o{ Message : "1:N"
    Message ||--o{ MessageAttachment : "1:N"
```

---

## Module 4 — Contracts & Payments

```mermaid
erDiagram
    Contract {
        uuid Id PK
        uuid ProposalId FK UK
        int Status "CHECK 0-5"
        decimal TotalAmount
        varchar Currency "CHECK EGP"
        text TermsAndConditions
        datetime SignedByClientAt "nullable"
        datetime SignedByLawyerAt "nullable"
        datetime StartedAt "nullable"
        datetime CompletedAt "nullable"
        datetime CancelledAt "nullable"
        uuid CancelledByUserId FK "NEW - nullable"
        text CancellationReason "NEW - nullable"
        bool IsDeleted "NEW"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Milestone {
        uuid Id PK
        uuid ContractId FK
        varchar Title
        text Description
        int OrderNumber
        decimal Amount
        datetime DueDate "nullable"
        int Status "CHECK 0-4"
        datetime SubmittedAt "nullable"
        datetime ApprovedAt "nullable"
        datetime RejectedAt "nullable"
        text RejectionReason "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ScheduledPayment {
        uuid Id PK
        uuid ContractId FK
        varchar Title
        decimal Amount
        datetime StartDate
        datetime EndDate "nullable"
        int IntervalInDays
        datetime NextExecutionDate
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    EscrowAccount {
        uuid Id PK "NEW TABLE"
        uuid ContractId FK UK
        decimal TotalDeposited
        decimal TotalReleased
        decimal TotalRefunded
        decimal PlatformFeeCollected
        decimal CurrentBalance "computed"
        varchar Currency
        int Status "Active/Settled/Frozen"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    EscrowTransaction {
        uuid Id PK "NEW TABLE"
        uuid EscrowAccountId FK
        int TransactionType "Deposit/Release/Refund/Fee"
        decimal Amount
        decimal RunningBalance
        varchar ReferenceEntityType "nullable"
        uuid ReferenceEntityId "nullable"
        uuid PaymentTransactionId FK "nullable"
        text Description "nullable"
        uuid CreatedByUserId FK "nullable"
        datetime CreatedAt
    }
    
    PaymentRelease {
        uuid Id PK
        uuid ContractId FK
        uuid MilestoneId FK "nullable"
        uuid ScheduledPaymentId FK "nullable"
        int ReleaseType "CHECK 0-1"
        decimal Amount
        int Status "CHECK 0-2"
        datetime ReleasedAt "nullable"
        datetime CreatedAt
    }
    
    PaymentTransaction {
        uuid Id PK
        uuid PaymentReleaseId FK
        varchar Gateway
        varchar GatewayTransactionId
        varchar GatewayEventId "NEW - webhook idempotency"
        decimal Amount
        varchar Currency
        int Status "CHECK 0-3"
        text FailureReason "nullable"
        decimal RefundedAmount "NEW - nullable"
        datetime RefundedAt "NEW - nullable"
        datetime ProcessedAt "nullable"
        datetime CreatedAt
    }
    
    ContractAttachment {
        uuid Id PK
        uuid ContractId FK
        uuid StoredFileId FK
        datetime CreatedAt
    }
    
    Contract ||--o{ Milestone : "1:N"
    Contract ||--o{ ScheduledPayment : "1:N"
    Contract ||--|| EscrowAccount : "NEW 1:1"
    EscrowAccount ||--o{ EscrowTransaction : "NEW 1:N ledger"
    Contract ||--o{ PaymentRelease : "1:N"
    PaymentRelease ||--o{ PaymentTransaction : "1:N"
    Contract ||--o{ ContractAttachment : "1:N"
```

---

## Module 5 — Reviews, Disputes & Notifications

```mermaid
erDiagram
    Review {
        uuid Id PK
        uuid ContractId FK
        uuid ReviewerUserId FK
        uuid RevieweeUserId FK
        int Rating "CHECK 1-5"
        text Comment
        bool IsVisible "NEW - moderation"
        text HiddenReason "NEW - nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Dispute {
        uuid Id PK
        uuid ContractId FK
        uuid RaisedByUserId FK
        uuid AssignedModeratorUserId FK "nullable"
        varchar Title
        text Description
        int Status "CHECK 0-3"
        int Priority "NEW - Low/Med/High/Critical"
        text ResolutionSummary "nullable"
        datetime ResolvedAt "nullable"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    DisputeAttachment {
        uuid Id PK
        uuid DisputeId FK
        uuid StoredFileId FK
        datetime CreatedAt
    }
    
    Notification {
        uuid Id PK
        varchar Title
        text Message
        int NotificationType "CHECK 0-18"
        varchar RelatedEntityType "NEW - nullable"
        uuid RelatedEntityId "NEW - nullable"
        datetime CreatedAt
    }
    
    UserNotification {
        uuid Id PK
        uuid NotificationId FK
        uuid UserId FK
        bool IsRead
        datetime ReadAt "nullable"
        datetime CreatedAt
    }
    
    NotificationPreference {
        uuid UserId PK, FK
        bool EnableInApp
        bool EnableEmail
        bool EnableSms
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Contract ||--o{ Review : "1:N (max 2)"
    Contract ||--o| Dispute : "1:0..1"
    Dispute ||--o{ DisputeAttachment : "1:N"
    Notification ||--o{ UserNotification : "1:N"
```

---

## Module 6 — AI Assistant

```mermaid
erDiagram
    AIConversation {
        uuid Id PK
        uuid UserId FK
        uuid RelatedLegalCaseId FK "nullable"
        varchar Title
        int ConversationType "CHECK 0-1"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    AIMessage {
        uuid Id PK
        uuid ConversationId FK
        int SenderType "CHECK 0-1"
        text Content
        varchar ModelName "nullable for user msgs"
        int PromptTokens "nullable for user msgs"
        int CompletionTokens "nullable for user msgs"
        int TotalTokens "nullable for user msgs"
        int ResponseTimeMs "nullable for user msgs"
        datetime CreatedAt
    }
    
    AIConversation ||--o{ AIMessage : "1:N"
```

---

## Module 7 — Knowledge Base

```mermaid
erDiagram
    LegalArticle {
        uuid Id PK
        uuid AuthorLawyerUserId FK
        varchar Title
        text Summary
        text Content
        int Status "CHECK 0-3"
        datetime PublishedAt "nullable"
        int ViewCount
        varchar Slug "NEW - URL-friendly"
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    LegalArticleCategory {
        uuid LegalArticleId PK, FK
        uuid LegalCategoryId PK, FK
    }
    
    LegalArticleAttachment {
        uuid Id PK
        uuid LegalArticleId FK
        uuid StoredFileId FK
        datetime CreatedAt
    }
    
    LegalArticle ||--o{ LegalArticleCategory : "M:N via junction"
    LegalArticle ||--o{ LegalArticleAttachment : "1:N"
```

---

## Cross-Cutting — Audit Trail (NEW)

```mermaid
erDiagram
    StatusChangeLog {
        uuid Id PK "NEW TABLE"
        varchar EntityType "LegalCase/Contract/Dispute/etc"
        uuid EntityId
        int OldStatus
        int NewStatus
        uuid ChangedByUserId FK "nullable - null for system"
        text Reason "nullable"
        datetime CreatedAt
    }
```

---

## Critical Indexes

| Table | Index | Type | Purpose |
|-------|-------|------|---------|
| `Message` | `(ConversationId, CreatedAt DESC)` | Non-clustered | Chat pagination |
| `AIMessage` | `(ConversationId, CreatedAt)` | Non-clustered | AI chat history |
| `LegalCase` | `(ClientUserId, Status)` | Non-clustered | Client dashboard |
| `Proposal` | `(LawyerUserId, Status)` | Non-clustered | Lawyer proposals |
| `Proposal` | `(LegalCaseId, Status)` | Non-clustered | Case proposals |
| `UserNotification` | `(UserId, IsRead, CreatedAt DESC)` | Non-clustered | Notification bell |
| `PaymentTransaction` | `(GatewayTransactionId)` | Unique | Webhook lookup |
| `LawyerMatch` | `(LegalCaseId, Rank)` | Non-clustered | Match display |
| `Review` | `(RevieweeUserId)` | Non-clustered | Profile reviews |
| `LegalArticle` | `(Status, PublishedAt DESC)` | Non-clustered | Article listing |
| `StoredFile` | `(UploadedByUserId)` | Non-clustered | User's files |
| `EscrowTransaction` | `(EscrowAccountId, CreatedAt)` | Non-clustered | Ledger queries |
| `ScheduledPayment` | `(NextExecutionDate)` | Non-clustered | Background job |
