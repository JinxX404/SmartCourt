////////////////////////////////////////////////////////////
/// MODULE 1 - IDENTITY & USER MANAGEMENT
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// ASP.NET CORE IDENTITY (SIMPLIFIED)
////////////////////////////////////////////////////////////

Table AspNetUsers {

  Id uuid [pk]

  UserName varchar
  Email varchar
  PhoneNumber varchar

  FirstName varchar
  LastName varchar

  ProfilePictureFileId uuid [null]

  IsActive bool

  CreatedAt datetime
  UpdatedAt datetime
}

////////////////////////////////////////////////////////////
/// FILE STORAGE
////////////////////////////////////////////////////////////

Table StoredFile {

  Id uuid [pk]

  OriginalFileName varchar
  StoredFileName varchar

  ContentType varchar
  Extension varchar

  FileSize bigint

  StoragePath text

  UploadedByUserId uuid

  CreatedAt datetime
}

////////////////////////////////////////////////////////////
/// CLIENT PROFILE
////////////////////////////////////////////////////////////

Table ClientProfile {

  UserId uuid [pk]

  DateOfBirth datetime

  NationalIdFrontFileId uuid [null]
  NationalIdBackFileId uuid [null]

  NationalIdVerificationStatus int

  NationalIdReviewedByUserId uuid [null]

  NationalIdVerifiedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime
}

////////////////////////////////////////////////////////////
/// LAWYER PROFILE
////////////////////////////////////////////////////////////

Table LawyerProfile {

  UserId uuid [pk]

  OfficeAddress text

  Bio text

  YearsOfExperience int

  IsAvailable bool

  NationalIdFrontFileId uuid [null]
  NationalIdBackFileId uuid [null]

  NationalIdVerificationStatus int

  NationalIdReviewedByUserId uuid [null]

  NationalIdVerifiedAt datetime [null]

  BarCardFrontFileId uuid [null]
  BarCardBackFileId uuid [null]

  BarCardVerificationStatus int

  BarCardReviewedByUserId uuid [null]

  BarCardVerifiedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime
}

////////////////////////////////////////////////////////////
/// LEGAL CATEGORIES
////////////////////////////////////////////////////////////

Table LegalCategory {

  Id uuid [pk]

  Name varchar

  Description text

  CreatedAt datetime
  UpdatedAt datetime
}

////////////////////////////////////////////////////////////
/// LAWYER SPECIALIZATIONS
////////////////////////////////////////////////////////////

Table LawyerSpecialization {

  LawyerUserId uuid

  LegalCategoryId uuid

  indexes {
    (LawyerUserId, LegalCategoryId) [pk]
  }
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: StoredFile.UploadedByUserId > AspNetUsers.Id

Ref: AspNetUsers.ProfilePictureFileId > StoredFile.Id

Ref: ClientProfile.UserId > AspNetUsers.Id

Ref: ClientProfile.NationalIdFrontFileId > StoredFile.Id
Ref: ClientProfile.NationalIdBackFileId > StoredFile.Id

Ref: ClientProfile.NationalIdReviewedByUserId > AspNetUsers.Id

Ref: LawyerProfile.UserId > AspNetUsers.Id

Ref: LawyerProfile.NationalIdFrontFileId > StoredFile.Id
Ref: LawyerProfile.NationalIdBackFileId > StoredFile.Id

Ref: LawyerProfile.BarCardFrontFileId > StoredFile.Id
Ref: LawyerProfile.BarCardBackFileId > StoredFile.Id

Ref: LawyerProfile.NationalIdReviewedByUserId > AspNetUsers.Id
Ref: LawyerProfile.BarCardReviewedByUserId > AspNetUsers.Id

Ref: LawyerSpecialization.LawyerUserId > LawyerProfile.UserId

Ref: LawyerSpecialization.LegalCategoryId > LegalCategory.Id

////////////////////////////////////////////////////////////
/// MODULE 2 - LEGAL CASES & AI
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// LEGAL CASE
////////////////////////////////////////////////////////////

Table LegalCase {

  Id uuid [pk]

  ClientUserId uuid

  Title varchar

  Description text

  CaseLocation text

  Status int

  FinalSubmittedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Represents a legal case owned by a client.

  The client edits this record directly.
  No version history is stored.

  AI determines the legal category after
  the client submits the case.
  '''
}

////////////////////////////////////////////////////////////
/// AI ANALYSIS
////////////////////////////////////////////////////////////

Table AIAnalysis {

  Id uuid [pk]

  LegalCaseId uuid

  AnalysisNumber int

  LegalCategoryId uuid [null]

  StrengthPoints text

  WeakPoints text

  MissingInformation text

  Recommendations text

  OverallAssessment text

  ConfidenceScore decimal

  ModelName varchar

  PromptTokens int

  CompletionTokens int

  TotalTokens int

  CreatedAt datetime

  Note: '''
  Every AI analysis is stored.

  Multiple analyses may exist for the
  same legal case.

  The newest analysis is considered the
  current analysis by the application.
  '''
}

////////////////////////////////////////////////////////////
/// LAWYER MATCH CACHE
////////////////////////////////////////////////////////////

Table LawyerMatch {

  Id uuid [pk]

  LegalCaseId uuid

  LawyerUserId uuid

  MatchScore decimal

  MatchReason text

  Rank int

  CreatedAt datetime

  indexes {
    (LegalCaseId, LawyerUserId) [unique]
  }

  Note: '''
  Cached AI matching results.

  Prevents repeating expensive AI
  matching every time the client
  opens the case.
  '''
}

////////////////////////////////////////////////////////////
/// CASE ATTACHMENTS
////////////////////////////////////////////////////////////

Table CaseAttachment {

  Id uuid [pk]

  LegalCaseId uuid

  StoredFileId uuid

  UploadedByUserId uuid

  CreatedAt datetime
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: LegalCase.ClientUserId > ClientProfile.UserId

Ref: AIAnalysis.LegalCaseId > LegalCase.Id

Ref: AIAnalysis.LegalCategoryId > LegalCategory.Id

Ref: LawyerMatch.LegalCaseId > LegalCase.Id

Ref: LawyerMatch.LawyerUserId > LawyerProfile.UserId

Ref: CaseAttachment.LegalCaseId > LegalCase.Id

Ref: CaseAttachment.StoredFileId > StoredFile.Id

Ref: CaseAttachment.UploadedByUserId > AspNetUsers.Id

////////////////////////////////////////////////////////////
/// MODULE 3 - PROPOSALS & COMMUNICATION
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// PROPOSALS
////////////////////////////////////////////////////////////

Table Proposal {

  Id uuid [pk]

  LegalCaseId uuid

  ClientUserId uuid

  LawyerUserId uuid

  Status int

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Created by a client after selecting
  a lawyer from the AI matching results.

  The proposal itself contains no message.

  The initial proposal message is
  automatically inserted into the
  conversation as the first message.
  '''
}

////////////////////////////////////////////////////////////
/// CONVERSATIONS
////////////////////////////////////////////////////////////

Table Conversation {

  Id uuid [pk]

  ProposalId uuid [unique]

  IsClosed bool

  ClosedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  One conversation per proposal.

  Created automatically after the
  proposal is submitted.

  Future extension:
  DisputeId can be added later to
  reuse the same messaging system
  for dispute resolution.
  '''
}

////////////////////////////////////////////////////////////
/// CONVERSATION PARTICIPANTS
////////////////////////////////////////////////////////////

Table ConversationParticipant {

  ConversationId uuid

  UserId uuid

  JoinedAt datetime

  indexes {
    (ConversationId, UserId) [pk]
  }

  Note: '''
  Defines who participates in
  a conversation.

  Current MVP:
  - Client
  - Lawyer

  Future:
  Moderators can join dispute
  conversations without changing
  the messaging architecture.
  '''
}

////////////////////////////////////////////////////////////
/// MESSAGES
////////////////////////////////////////////////////////////

Table Message {

  Id uuid [pk]

  ConversationId uuid

  SenderUserId uuid

  MessageType int

  Content text

  IsEdited bool

  EditedAt datetime [null]

  CreatedAt datetime

  Note: '''
  Stores all conversation messages.

  Supports:

  - Proposal opening message
  - Normal chat
  - System messages
  '''
}

////////////////////////////////////////////////////////////
/// MESSAGE ATTACHMENTS
////////////////////////////////////////////////////////////

Table MessageAttachment {

  Id uuid [pk]

  MessageId uuid

  StoredFileId uuid

  CreatedAt datetime
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: Proposal.LegalCaseId > LegalCase.Id

Ref: Proposal.ClientUserId > ClientProfile.UserId

Ref: Proposal.LawyerUserId > LawyerProfile.UserId

Ref: Conversation.ProposalId > Proposal.Id

Ref: ConversationParticipant.ConversationId > Conversation.Id

Ref: ConversationParticipant.UserId > AspNetUsers.Id

Ref: Message.ConversationId > Conversation.Id

Ref: Message.SenderUserId > AspNetUsers.Id

Ref: MessageAttachment.MessageId > Message.Id

Ref: MessageAttachment.StoredFileId > StoredFile.Id

////////////////////////////////////////////////////////////
/// MODULE 4 - CONTRACTS & PAYMENTS
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// CONTRACT
////////////////////////////////////////////////////////////

Table Contract {

  Id uuid [pk]

  ProposalId uuid [unique]

  Status int

  TotalAmount decimal

  Currency varchar

  TermsAndConditions text

  SignedByClientAt datetime [null]

  SignedByLawyerAt datetime [null]

  StartedAt datetime [null]

  CompletedAt datetime [null]

  CancelledAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  One contract per accepted proposal.

  The contract becomes active after
  both parties sign it.
  '''
}

////////////////////////////////////////////////////////////
/// MILESTONES
////////////////////////////////////////////////////////////

Table Milestone {

  Id uuid [pk]

  ContractId uuid

  Title varchar

  Description text

  OrderNumber int

  Amount decimal

  DueDate datetime [null]

  Status int

  SubmittedAt datetime [null]

  ApprovedAt datetime [null]

  RejectedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Represents a deliverable.

  Payment can be released after the
  milestone is approved.
  '''
}

////////////////////////////////////////////////////////////
/// RECURRING SCHEDULED PAYMENTS
////////////////////////////////////////////////////////////

Table ScheduledPayment {

  Id uuid [pk]

  ContractId uuid

  Title varchar

  Amount decimal

  StartDate datetime

  EndDate datetime [null]

  IntervalInDays int

  NextExecutionDate datetime

  IsActive bool

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Represents recurring payments.

  Example:

  Every 30 days release
  3,000 EGP for expenses.

  The background job updates
  NextExecutionDate after each release.
  '''

  indexes {
    (NextExecutionDate)
  }
}

////////////////////////////////////////////////////////////
/// PAYMENT RELEASE
////////////////////////////////////////////////////////////

Table PaymentRelease {

  Id uuid [pk]

  ContractId uuid

  MilestoneId uuid [null]

  ScheduledPaymentId uuid [null]

  ReleaseType int

  Amount decimal

  Status int

  ReleasedAt datetime [null]

  CreatedAt datetime

  Note: '''
  Represents ONE business payment.

  Exactly one of:

  MilestoneId

  OR

  ScheduledPaymentId

  must be populated.

  This record is immutable.
  '''
}

////////////////////////////////////////////////////////////
/// PAYMENT TRANSACTION
////////////////////////////////////////////////////////////

Table PaymentTransaction {

  Id uuid [pk]

  PaymentReleaseId uuid

  Gateway varchar

  GatewayTransactionId varchar

  Amount decimal

  Currency varchar

  Status int

  FailureReason text [null]

  ProcessedAt datetime [null]

  CreatedAt datetime

  Note: '''
  Represents an actual payment
  request sent to Stripe (or
  another payment provider).

  Multiple attempts may exist
  for one PaymentRelease.
  '''
}

////////////////////////////////////////////////////////////
/// CONTRACT ATTACHMENTS
////////////////////////////////////////////////////////////

Table ContractAttachment {

  Id uuid [pk]

  ContractId uuid

  StoredFileId uuid

  CreatedAt datetime

  Note: '''
  Optional documents attached
  to the contract.
  '''
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: Contract.ProposalId > Proposal.Id

Ref: Milestone.ContractId > Contract.Id

Ref: ScheduledPayment.ContractId > Contract.Id

Ref: PaymentRelease.ContractId > Contract.Id

Ref: PaymentRelease.MilestoneId > Milestone.Id

Ref: PaymentRelease.ScheduledPaymentId > ScheduledPayment.Id

Ref: PaymentTransaction.PaymentReleaseId > PaymentRelease.Id

Ref: ContractAttachment.ContractId > Contract.Id

Ref: ContractAttachment.StoredFileId > StoredFile.Id

////////////////////////////////////////////////////////////
/// MODULE 5 - REVIEWS, DISPUTES & NOTIFICATIONS
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// REVIEWS
////////////////////////////////////////////////////////////

Table Review {

  Id uuid [pk]

  ContractId uuid

  ReviewerUserId uuid

  RevieweeUserId uuid

  Rating int

  Comment text

  CreatedAt datetime
  UpdatedAt datetime

  indexes {
    (ContractId, ReviewerUserId) [unique]
  }

  Note: '''
  Reviews can only be submitted after
  the contract has been completed.

  Each participant may review the
  other exactly once per contract.
  '''
}

////////////////////////////////////////////////////////////
/// DISPUTES
////////////////////////////////////////////////////////////

Table Dispute {

  Id uuid [pk]

  ContractId uuid

  RaisedByUserId uuid

  AssignedModeratorUserId uuid [null]

  Title varchar

  Description text

  Status int

  ResolutionSummary text [null]

  ResolvedAt datetime [null]

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Represents a dispute raised by either
  the client or the lawyer regarding
  the execution of a contract.

  The moderation team reviews the
  proposal conversation, contract,
  milestones, payments and attachments,
  then contacts both parties outside
  the platform (phone/email) if needed.

  No dedicated dispute chat exists
  inside the platform.
  '''
}

////////////////////////////////////////////////////////////
/// DISPUTE ATTACHMENTS
////////////////////////////////////////////////////////////

Table DisputeAttachment {

  Id uuid [pk]

  DisputeId uuid

  StoredFileId uuid

  CreatedAt datetime

  Note: '''
  Optional supporting documents
  uploaded with the dispute.
  '''
}

////////////////////////////////////////////////////////////
/// NOTIFICATIONS
////////////////////////////////////////////////////////////

Table Notification {

  Id uuid [pk]

  Title varchar

  Message text

  NotificationType int

  CreatedAt datetime

  Note: '''
  Represents the notification content.

  Examples:

  Proposal Received

  Proposal Accepted

  Contract Signed

  Milestone Submitted

  Milestone Approved

  Payment Released

  Dispute Raised
  '''
}

////////////////////////////////////////////////////////////
/// USER NOTIFICATIONS
////////////////////////////////////////////////////////////

Table UserNotification {

  Id uuid [pk]

  NotificationId uuid

  UserId uuid

  IsRead bool

  ReadAt datetime [null]

  CreatedAt datetime

  Note: '''
  Represents delivering one
  notification to one user.
  '''
}

////////////////////////////////////////////////////////////
/// NOTIFICATION PREFERENCES
////////////////////////////////////////////////////////////

Table NotificationPreference {

  UserId uuid [pk]

  EnableInApp bool

  EnableEmail bool

  EnableSms bool

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Determines how each user
  prefers to receive notifications.
  '''
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: Review.ContractId > Contract.Id

Ref: Review.ReviewerUserId > AspNetUsers.Id

Ref: Review.RevieweeUserId > AspNetUsers.Id

Ref: Dispute.ContractId > Contract.Id

Ref: Dispute.RaisedByUserId > AspNetUsers.Id

Ref: Dispute.AssignedModeratorUserId > AspNetUsers.Id

Ref: DisputeAttachment.DisputeId > Dispute.Id

Ref: DisputeAttachment.StoredFileId > StoredFile.Id

Ref: UserNotification.NotificationId > Notification.Id

Ref: UserNotification.UserId > AspNetUsers.Id

Ref: NotificationPreference.UserId > AspNetUsers.Id

////////////////////////////////////////////////////////////
/// MODULE 6 - AI ASSISTANT
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// AI CONVERSATIONS
////////////////////////////////////////////////////////////

Table AIConversation {

  Id uuid [pk]

  UserId uuid

  RelatedLegalCaseId uuid [null]

  Title varchar

  ConversationType int

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Represents one AI conversation.

  Examples:

  - General Legal Assistant
  - Case Analysis Assistant

  RelatedLegalCaseId is null for
  general conversations.
  '''
}

////////////////////////////////////////////////////////////
/// AI MESSAGES
////////////////////////////////////////////////////////////

Table AIMessage {

  Id uuid [pk]

  ConversationId uuid

  SenderType int

  Content text

  ModelName varchar

  PromptTokens int

  CompletionTokens int

  TotalTokens int

  ResponseTimeMs int

  CreatedAt datetime

  Note: '''
  Stores every exchanged message.

  SenderType:

  User

  AI

  Complete conversation history
  is preserved.
  '''
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: AIConversation.UserId > AspNetUsers.Id

Ref: AIConversation.RelatedLegalCaseId > LegalCase.Id

Ref: AIMessage.ConversationId > AIConversation.Id

////////////////////////////////////////////////////////////
/// MODULE 7 - KNOWLEDGE BASE
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
/// LEGAL ARTICLES
////////////////////////////////////////////////////////////

Table LegalArticle {

  Id uuid [pk]

  AuthorLawyerUserId uuid

  Title varchar

  Summary text

  Content text

  Status int

  PublishedAt datetime [null]

  ViewCount int

  CreatedAt datetime
  UpdatedAt datetime

  Note: '''
  Educational legal article written
  by a verified lawyer.

  Articles may be saved as drafts
  before publication.
  '''
}

////////////////////////////////////////////////////////////
/// ARTICLE CATEGORIES
////////////////////////////////////////////////////////////

Table LegalArticleCategory {

  LegalArticleId uuid

  LegalCategoryId uuid

  indexes {
    (LegalArticleId, LegalCategoryId) [pk]
  }

  Note: '''
  One article may belong to
  multiple legal categories.
  '''
}

////////////////////////////////////////////////////////////
/// ARTICLE ATTACHMENTS
////////////////////////////////////////////////////////////

Table LegalArticleAttachment {

  Id uuid [pk]

  LegalArticleId uuid

  StoredFileId uuid

  CreatedAt datetime

  Note: '''
  Optional files attached to
  an article.

  Examples:

  PDF

  Court Document

  Image

  Reference File
  '''
}

////////////////////////////////////////////////////////////
/// RELATIONSHIPS
////////////////////////////////////////////////////////////

Ref: LegalArticle.AuthorLawyerUserId > LawyerProfile.UserId

Ref: LegalArticleCategory.LegalArticleId > LegalArticle.Id

Ref: LegalArticleCategory.LegalCategoryId > LegalCategory.Id

Ref: LegalArticleAttachment.LegalArticleId > LegalArticle.Id

Ref: LegalArticleAttachment.StoredFileId > StoredFile.Id

