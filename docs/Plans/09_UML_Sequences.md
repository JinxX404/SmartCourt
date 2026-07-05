# Smart Court — UML Sequence Diagrams

> **Version:** 1.0 | **Date:** 2026-07-03
> **Notation:** Mermaid Sequence Diagrams
> **Coverage:** 10 critical business workflows

---

## 1. Client Registration & Email Verification

```mermaid
sequenceDiagram
    actor Client
    participant API as AuthController
    participant SVC as AuthService
    participant ID as ASP.NET Identity
    participant DB as Database
    participant EMAIL as IEmailProvider

    Client->>API: POST /api/auth/register/client
    API->>SVC: RegisterClientAsync(dto)
    SVC->>ID: CreateAsync(user, password)
    ID->>DB: INSERT AspNetUsers
    ID->>DB: INSERT AspNetUserRoles (Client)
    ID-->>SVC: IdentityResult
    SVC->>DB: INSERT ClientProfile
    SVC->>DB: INSERT NotificationPreference (defaults)
    SVC->>ID: GenerateEmailConfirmationTokenAsync()
    ID-->>SVC: token
    SVC->>EMAIL: SendAsync(verificationEmail)
    EMAIL-->>SVC: sent
    SVC-->>API: RegisterResponse
    API-->>Client: 201 Created

    Note over Client: Client clicks email link

    Client->>API: POST /api/auth/verify-email {userId, token}
    API->>SVC: VerifyEmailAsync(userId, token)
    SVC->>ID: ConfirmEmailAsync(user, token)
    ID->>DB: UPDATE AspNetUsers SET EmailConfirmed = 1
    SVC-->>API: Success
    API-->>Client: 200 OK "تم تأكيد البريد الإلكتروني"

    Note over Client: Client can now log in

    Client->>API: POST /api/auth/login {email, password}
    API->>SVC: LoginAsync(email, password)
    SVC->>ID: CheckPasswordSignInAsync()
    SVC->>SVC: GenerateJwtToken(claims)
    SVC->>DB: INSERT RefreshToken
    SVC-->>API: LoginResponse {accessToken, refreshToken}
    API-->>Client: 200 OK
```

---

## 2. Lawyer Verification Flow

```mermaid
sequenceDiagram
    actor Lawyer
    actor Admin
    participant API as Controller
    participant SVC as Services
    participant DB as Database
    participant NOTIF as NotificationService

    Note over Lawyer: Step 1: Upload verification documents

    Lawyer->>API: POST /api/files/upload (NationalID front)
    API-->>Lawyer: 201 {fileId: "abc"}
    Lawyer->>API: POST /api/files/upload (NationalID back)
    API-->>Lawyer: 201 {fileId: "def"}

    Lawyer->>API: POST /api/lawyer-verification/national-id {frontFileId, backFileId}
    API->>SVC: SubmitNationalIdAsync(frontId, backId)
    SVC->>DB: UPDATE LawyerProfile SET NationalIdStatus = Pending
    SVC->>NOTIF: NotifyAdmins("طلب تحقق جديد")
    NOTIF->>DB: INSERT Notification + UserNotification (all admins)
    SVC-->>API: Success
    API-->>Lawyer: 200 OK

    Lawyer->>API: POST /api/lawyer-verification/bar-card {frontFileId, backFileId}
    API->>SVC: SubmitBarCardAsync(frontId, backId)
    SVC->>DB: UPDATE LawyerProfile SET BarCardStatus = Pending
    SVC-->>API: Success

    Note over Admin: Step 2: Admin reviews documents

    Admin->>API: GET /api/admin/verifications/pending
    API-->>Admin: List of pending verifications

    Admin->>API: PUT /api/admin/verifications/{userId}/national-id {status: Approved}
    API->>SVC: ReviewNationalIdAsync(userId, Approved)
    SVC->>DB: UPDATE LawyerProfile SET NationalIdStatus=Approved, ReviewedBy, VerifiedAt
    SVC->>DB: INSERT StatusChangeLog
    SVC->>NOTIF: NotifyLawyer("تم قبول الهوية الوطنية")
    SVC-->>API: Success

    Admin->>API: PUT /api/admin/verifications/{userId}/bar-card {status: Approved}
    API->>SVC: ReviewBarCardAsync(userId, Approved)
    SVC->>DB: UPDATE LawyerProfile SET BarCardStatus=Approved
    SVC->>DB: INSERT StatusChangeLog

    Note over Lawyer: Both verified → Lawyer is now fully verified
    Note over Lawyer: Can set IsAvailable=true, receive proposals, publish articles
```

---

## 3. Case Lifecycle (Create → AI Analysis → Matching)

```mermaid
sequenceDiagram
    actor Client
    participant API as CasesController
    participant CASE as CaseService
    participant AI as AIAnalysisService
    participant LLM as ILlmProvider
    participant MATCH as LawyerMatchingService
    participant DB as Database

    Client->>API: POST /api/cases {title, description}
    API->>CASE: CreateAsync(dto)
    CASE->>DB: INSERT LegalCase (Status=Draft)
    CASE-->>API: CaseResponse
    API-->>Client: 201 Created

    Client->>API: POST /api/cases/{id}/attachments {fileIds}
    API->>CASE: AddAttachmentsAsync(caseId, fileIds)
    CASE->>DB: INSERT CaseAttachment[]
    API-->>Client: 201 OK

    Note over Client: Client submits for AI analysis

    Client->>API: POST /api/cases/{id}/submit
    API->>CASE: SubmitAsync(caseId)
    CASE->>DB: UPDATE LegalCase SET Status=Submitted
    CASE->>DB: INSERT StatusChangeLog
    CASE->>AI: AnalyzeCaseAsync(caseId)
    AI->>DB: SELECT LegalCase + CaseAttachments
    AI->>AI: Build prompt from template
    AI->>LLM: CompleteAsync(prompt, json_object)
    LLM-->>AI: LlmResponse (JSON)
    AI->>AI: Parse JSON → CaseAnalysisResult
    AI->>DB: INSERT AIAnalysis (AnalysisNumber++)
    AI->>DB: UPDATE LegalCase SET Status=Analyzed
    AI->>DB: INSERT StatusChangeLog
    AI-->>CASE: AnalysisResult
    CASE-->>API: Success
    API-->>Client: 200 OK

    Client->>API: GET /api/cases/{id}/analysis
    API-->>Client: Full AI analysis results

    Note over Client: Client reviews, optionally edits and resubmits

    Client->>API: POST /api/cases/{id}/finalize
    API->>CASE: FinalizeAsync(caseId)
    CASE->>DB: UPDATE LegalCase SET Status=Finalized, FinalSubmittedAt
    CASE->>MATCH: MatchLawyersAsync(caseId)
    MATCH->>DB: SELECT verified, available lawyers
    MATCH->>MATCH: Score each lawyer (weighted algorithm)
    MATCH->>DB: DELETE old LawyerMatch WHERE CaseId
    MATCH->>DB: INSERT LawyerMatch[] (ranked)
    MATCH->>DB: UPDATE LegalCase SET Status=Matched
    MATCH-->>CASE: MatchResults
    CASE-->>API: Success
    API-->>Client: 200 OK

    Client->>API: GET /api/cases/{id}/matches
    API-->>Client: Ranked list of matched lawyers
```

---

## 4. Proposal & Accept Flow

```mermaid
sequenceDiagram
    actor Client
    actor Lawyer
    participant API as ProposalsController
    participant SVC as ProposalService
    participant DB as Database
    participant NOTIF as NotificationService
    participant HUB as ChatHub (SignalR)

    Client->>API: POST /api/proposals {caseId, lawyerUserId, message}
    API->>SVC: CreateProposalAsync(dto)
    SVC->>DB: INSERT Proposal (Status=Pending)
    SVC->>DB: INSERT Conversation (ProposalId)
    SVC->>DB: INSERT ConversationParticipant (Client)
    SVC->>DB: INSERT ConversationParticipant (Lawyer)
    SVC->>DB: INSERT Message (type=ProposalMessage, content=dto.message)
    SVC->>NOTIF: SendAsync(lawyerUserId, "اقتراح جديد")
    NOTIF->>DB: INSERT Notification + UserNotification
    NOTIF->>HUB: ReceiveNotification(lawyer, notification)
    SVC-->>API: ProposalResponse {id, conversationId}
    API-->>Client: 201 Created

    Note over Lawyer: Lawyer receives notification, reviews proposal

    Lawyer->>API: GET /api/proposals (received)
    API-->>Lawyer: List of pending proposals

    Lawyer->>API: GET /api/proposals/{id}
    API-->>Lawyer: Proposal detail with case info

    Lawyer->>API: PUT /api/proposals/{id}/respond {status: Accepted}
    API->>SVC: RespondAsync(proposalId, Accepted)
    SVC->>DB: UPDATE Proposal SET Status=Accepted
    SVC->>DB: INSERT StatusChangeLog
    SVC->>DB: UPDATE LegalCase SET AssignedLawyerUserId
    SVC->>NOTIF: SendAsync(clientUserId, "تم قبول اقتراحك")
    NOTIF->>HUB: ReceiveNotification(client, notification)
    SVC-->>API: Success
    API-->>Lawyer: 200 OK

    Note over Client,Lawyer: Conversation is now open for chat
```

---

## 5. Contract Creation & Dual-Party Signing

```mermaid
sequenceDiagram
    actor Client
    actor Lawyer
    participant API as ContractsController
    participant SVC as ContractService
    participant DB as Database
    participant NOTIF as NotificationService

    Lawyer->>API: POST /api/contracts {proposalId, totalAmount, terms}
    API->>SVC: CreateAsync(dto)
    SVC->>DB: Verify Proposal Status = Accepted
    SVC->>DB: INSERT Contract (Status=Draft)
    SVC->>DB: INSERT EscrowAccount (ContractId, Balance=0)
    SVC->>NOTIF: SendAsync(client, "عقد جديد")
    SVC-->>API: ContractResponse
    API-->>Lawyer: 201 Created

    Lawyer->>API: POST /api/contracts/{id}/milestones {title, amount, order}
    API->>SVC: AddMilestoneAsync(contractId, dto)
    SVC->>DB: INSERT Milestone
    API-->>Lawyer: 201 OK

    Lawyer->>API: POST /api/contracts/{id}/milestones {title, amount, order}
    SVC->>DB: INSERT Milestone (second)
    API-->>Lawyer: 201 OK

    Note over Client: Client reviews contract and milestones

    Client->>API: GET /api/contracts/{id}
    API-->>Client: Contract detail + milestones

    Note over Client: First signature

    Client->>API: POST /api/contracts/{id}/sign
    API->>SVC: SignAsync(contractId)
    SVC->>SVC: Validate sum(milestone.amounts) == totalAmount
    SVC->>DB: UPDATE Contract SET SignedByClientAt = now, Status=PendingSignature
    SVC->>DB: INSERT StatusChangeLog (Draft → PendingSignature)
    SVC->>NOTIF: SendAsync(lawyer, "العميل وقّع العقد")
    SVC-->>API: {status: PendingSignature}
    API-->>Client: 200 OK

    Note over Lawyer: Second signature → Contract becomes Active

    Lawyer->>API: POST /api/contracts/{id}/sign
    API->>SVC: SignAsync(contractId)
    SVC->>DB: UPDATE Contract SET SignedByLawyerAt=now, Status=Active, StartedAt=now
    SVC->>DB: INSERT StatusChangeLog (PendingSignature → Active)
    SVC->>NOTIF: SendAsync(client, "تم تفعيل العقد")
    SVC-->>API: {status: Active}
    API-->>Lawyer: 200 OK "تم تفعيل العقد — وقع الطرفان"
```

---

## 6. Milestone Submit → Approve → Payment Release

```mermaid
sequenceDiagram
    actor Client
    actor Lawyer
    participant API as Controller
    participant CSVC as ContractService
    participant PSVC as PaymentService
    participant PAY as IPaymentProvider
    participant DB as Database
    participant NOTIF as NotificationService

    Note over Client: Client deposits escrow for milestone

    Client->>API: POST /api/payments/deposit {milestoneId}
    API->>PSVC: DepositAsync(milestoneId)
    PSVC->>DB: SELECT Milestone + Contract
    PSVC->>DB: INSERT PaymentRelease (type=Milestone, status=Pending)
    PSVC->>DB: INSERT PaymentTransaction (status=Pending)
    PSVC->>PAY: CreatePaymentAsync(amount)
    PAY-->>PSVC: {success, gatewayTxnId}
    PSVC->>DB: UPDATE PaymentTransaction SET Status=Completed, GatewayTxnId
    PSVC->>DB: UPDATE EscrowAccount SET TotalDeposited += amount
    PSVC->>DB: INSERT EscrowTransaction (type=Deposit, runningBalance)
    PSVC->>DB: UPDATE Milestone SET Status=InProgress
    PSVC->>NOTIF: SendAsync(lawyer, "تم إيداع ضمان المرحلة")
    PSVC-->>API: PaymentResponse
    API-->>Client: 200 OK

    Note over Lawyer: Lawyer completes work, submits milestone

    Lawyer->>API: POST /api/contracts/{id}/milestones/{mid}/submit {notes}
    API->>CSVC: SubmitMilestoneAsync(milestoneId)
    CSVC->>DB: UPDATE Milestone SET Status=Submitted, SubmittedAt
    CSVC->>NOTIF: SendAsync(client, "تم تقديم المرحلة للمراجعة")
    API-->>Lawyer: 200 OK

    Note over Client: Client reviews and approves

    Client->>API: PUT /api/contracts/{id}/milestones/{mid}/approve
    API->>CSVC: ApproveMilestoneAsync(milestoneId)
    CSVC->>DB: UPDATE Milestone SET Status=Approved, ApprovedAt
    CSVC->>DB: INSERT StatusChangeLog

    Note over Client: Client releases payment

    Client->>API: POST /api/payments/release/{paymentReleaseId}
    API->>PSVC: ReleaseAsync(paymentReleaseId)
    PSVC->>DB: Verify Milestone Status = Approved
    PSVC->>PAY: TransferAsync(lawyerAccount, amount)
    PAY-->>PSVC: {success}
    PSVC->>DB: UPDATE PaymentRelease SET Status=Released, ReleasedAt
    PSVC->>DB: UPDATE EscrowAccount SET TotalReleased += amount
    PSVC->>DB: INSERT EscrowTransaction (type=Release)
    PSVC->>NOTIF: SendAsync(lawyer, "تم تحرير المبلغ")
    PSVC-->>API: Success
    API-->>Client: 200 OK

    Note over Client,Lawyer: If all milestones approved → Contract auto-completes
    CSVC->>DB: Check all milestones Approved
    CSVC->>DB: UPDATE Contract SET Status=Completed, CompletedAt
```

---

## 7. Real-Time Chat (SignalR)

```mermaid
sequenceDiagram
    actor Client
    actor Lawyer
    participant HUB as ChatHub (SignalR)
    participant SVC as ChatService
    participant DB as Database
    participant NOTIF as NotificationService

    Note over Client,Lawyer: Both connect to SignalR on app load

    Client->>HUB: Connect(JWT token)
    HUB->>HUB: Validate JWT, extract userId
    HUB->>DB: SELECT conversations WHERE participant = clientId
    HUB->>HUB: Groups.AddToGroupAsync(conversationId) for each

    Lawyer->>HUB: Connect(JWT token)
    HUB->>HUB: Same — join all conversation groups

    Note over Client: Client sends a text message

    Client->>HUB: SendMessage(conversationId, "مرحباً", Text)
    HUB->>SVC: SendMessageAsync(conversationId, userId, content, type)
    SVC->>DB: Verify participant membership
    SVC->>DB: INSERT Message (ConversationId, SenderUserId, Content)
    SVC->>DB: UPDATE Conversation SET LastMessageAt, LastMessagePreview
    SVC-->>HUB: MessageResponse
    HUB->>HUB: Clients.Group(conversationId).ReceiveMessage(msg)
    
    Lawyer->>Lawyer: ReceiveMessage displayed in chat UI

    Note over Client: Client sends a file

    Client->>HUB: REST: POST /api/files/upload (document.pdf)
    HUB-->>Client: {fileId: "xyz"}
    Client->>HUB: SendFileMessage(conversationId, "xyz")
    HUB->>SVC: SendFileMessageAsync(conversationId, userId, fileId)
    SVC->>DB: INSERT Message (type=File)
    SVC->>DB: INSERT MessageAttachment (MessageId, StoredFileId)
    SVC->>DB: UPDATE Conversation LastMessage fields
    SVC-->>HUB: MessageResponse (with attachment info)
    HUB->>HUB: Broadcast to group

    Note over Lawyer: Lawyer sends voice message

    Lawyer->>HUB: REST: POST /api/files/upload (voice.webm)
    HUB-->>Lawyer: {fileId: "abc"}
    Lawyer->>HUB: SendFileMessage(conversationId, "abc")
    HUB->>SVC: SendFileMessageAsync (type=Voice)
    SVC->>DB: INSERT Message + MessageAttachment
    HUB->>HUB: Broadcast to group
    
    Note over Client,Lawyer: Typing indicators (no DB persistence)
    Client->>HUB: StartTyping(conversationId)
    HUB->>HUB: Clients.OthersInGroup().UserTyping(userId, name)
    Client->>HUB: StopTyping(conversationId)
    HUB->>HUB: Clients.OthersInGroup().UserStoppedTyping(userId)
```

---

## 8. AI Assistant — Client (General Legal Q&A)

```mermaid
sequenceDiagram
    actor Client
    participant API as AIAssistantController
    participant SVC as AIAssistantService
    participant LLM as ILlmProvider
    participant DB as Database

    Client->>API: POST /api/ai-assistant/conversations {type: GeneralLegal, initialMessage: "ما هي حقوق المستأجر؟"}
    API->>SVC: CreateConversationAsync(dto)
    SVC->>DB: INSERT AIConversation (type=GeneralLegal, title=first50chars)
    SVC->>DB: INSERT AIMessage (SenderType=User, Content=initialMessage)
    SVC->>SVC: Load system prompt (Arabic legal assistant)
    SVC->>LLM: CompleteAsync(systemPrompt, [userMessage])
    LLM-->>SVC: LlmResponse {content, tokens, responseTime}
    SVC->>DB: INSERT AIMessage (SenderType=AI, Content, ModelName, Tokens)
    SVC-->>API: ConversationResponse with both messages
    API-->>Client: 201 Created

    Note over Client: Client continues conversation

    Client->>API: POST /api/ai-assistant/conversations/{id}/messages {content: "هل يمكنني رفع دعوى؟"}
    API->>SVC: SendMessageAsync(conversationId, content)
    SVC->>DB: INSERT AIMessage (SenderType=User)
    SVC->>DB: SELECT last 10 AIMessages for context
    SVC->>SVC: Build messages array [system, history..., new message]
    SVC->>LLM: CompleteAsync(systemPrompt, messagesArray)
    LLM-->>SVC: LlmResponse
    SVC->>DB: INSERT AIMessage (SenderType=AI, with token tracking)
    SVC->>DB: UPDATE AIConversation SET UpdatedAt
    SVC-->>API: {userMessage, aiMessage}
    API-->>Client: 200 OK
```

---

## 9. Lawyer AI Assistant with RAG Pipeline

```mermaid
sequenceDiagram
    actor Lawyer
    participant API as AIAssistantController
    participant SVC as AIAssistantService
    participant EMB as ILlmProvider ["GetEmbedding"]
    participant VEC as IVectorStoreProvider
    participant LLM as ILlmProvider ["Complete"]
    participant DB as Database

    Lawyer->>API: POST /api/ai-assistant/conversations {type: LawyerResearch, relatedCaseId: "xyz", initialMessage: "ما هي المادة المنظمة لعقد الإيجار؟"}
    API->>SVC: CreateConversationAsync(dto)
    SVC->>DB: INSERT AIConversation (type=LawyerResearch, RelatedCaseId)
    SVC->>DB: INSERT AIMessage (SenderType=User)

    Note over SVC: RAG Pipeline begins

    SVC->>DB: SELECT LegalCase details (if relatedCaseId provided)
    SVC->>EMB: GetEmbeddingAsync(userMessage)
    EMB-->>SVC: float[] vector (1536 dimensions)
    SVC->>VEC: SearchAsync("egyptian_law", vector, topK=5, minScore=0.7)
    VEC-->>SVC: List of relevant law chunks with scores

    alt RAG results found
        SVC->>SVC: Inject chunks into prompt as "مصادر ذات صلة"
        SVC->>SVC: Add case context if relatedCaseId exists
    else No RAG results
        SVC->>SVC: Use LLM without RAG context
        SVC->>SVC: Add note: "لم يتم العثور على مصادر مباشرة"
    end

    SVC->>LLM: CompleteAsync(ragPrompt, messagesHistory)
    LLM-->>SVC: LlmResponse {content, tokens}
    SVC->>DB: INSERT AIMessage (SenderType=AI, ModelName, Tokens, ResponseMs)
    SVC-->>API: ConversationResponse
    API-->>Lawyer: 201 Created
```

---

## 10. Dispute Resolution Flow

```mermaid
sequenceDiagram
    actor Client
    actor Admin
    actor Lawyer
    participant API as Controller
    participant SVC as Services
    participant DB as Database
    participant NOTIF as NotificationService
    participant PSVC as PaymentService

    Client->>API: POST /api/disputes {contractId, title, description, attachments}
    API->>SVC: CreateDisputeAsync(dto)
    SVC->>DB: Verify Contract Status = Active
    SVC->>DB: INSERT Dispute (Status=Open)
    SVC->>DB: INSERT DisputeAttachment[]
    SVC->>DB: UPDATE Contract SET Status=Disputed
    SVC->>DB: INSERT StatusChangeLog (Active → Disputed)
    SVC->>NOTIF: SendAsync(lawyer, "تم رفع نزاع")
    SVC->>NOTIF: SendToAdmins("نزاع جديد يحتاج مراجعة")
    SVC-->>API: DisputeResponse
    API-->>Client: 201 Created

    Note over Admin: Admin reviews and assigns moderator

    Admin->>API: GET /api/admin/disputes (Status=Open)
    API-->>Admin: List of open disputes

    Admin->>API: PUT /api/admin/disputes/{id}/assign {moderatorUserId}
    API->>SVC: AssignModeratorAsync(disputeId, moderatorId)
    SVC->>DB: UPDATE Dispute SET AssignedModeratorUserId, Status=UnderReview
    SVC->>DB: INSERT StatusChangeLog
    SVC->>NOTIF: SendAsync(client, "جاري مراجعة النزاع")
    SVC->>NOTIF: SendAsync(lawyer, "جاري مراجعة النزاع")
    API-->>Admin: 200 OK

    Note over Admin: Admin investigates and resolves

    Admin->>API: PUT /api/admin/disputes/{id}/resolve {summary, contractAction: "resume"}
    API->>SVC: ResolveDisputeAsync(disputeId, dto)
    SVC->>DB: UPDATE Dispute SET Status=Resolved, ResolutionSummary, ResolvedAt
    SVC->>DB: INSERT StatusChangeLog

    alt contractAction = "resume"
        SVC->>DB: UPDATE Contract SET Status=Active
        SVC->>DB: INSERT StatusChangeLog (Disputed → Active)
    else contractAction = "cancel"
        SVC->>DB: UPDATE Contract SET Status=Cancelled, CancelledAt
        SVC->>PSVC: RefundRemainingEscrow(contractId)
        PSVC->>DB: UPDATE EscrowAccount (TotalRefunded)
        PSVC->>DB: INSERT EscrowTransaction (type=Refund)
    else contractAction = "refund"
        SVC->>PSVC: FullRefund(contractId)
        PSVC->>DB: Refund all deposited amounts
        SVC->>DB: UPDATE Contract SET Status=Cancelled
    end

    SVC->>NOTIF: SendAsync(client, "تم حل النزاع")
    SVC->>NOTIF: SendAsync(lawyer, "تم حل النزاع")
    SVC-->>API: Success
    API-->>Admin: 200 OK
```
