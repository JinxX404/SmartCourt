# Smart Court — Product Backlog

> **Version:** 1.0 | **Date:** 2026-07-03
> **Timeline:** 30 days (4 sprints × 1 week) | **Team:** 4 BE + 2 FE
> **Story Points:** 1 SP ≈ 0.5 dev-day

---

## Team Roles

| Code | Role | Focus Area |
|------|------|------------|
| **BE-1** | Backend Lead | Infrastructure, Cases, Payments/Escrow |
| **BE-2** | Backend Dev | Auth, Contracts, Admin |
| **BE-3** | Backend Dev (AI) | AI Analysis, Matching, AI Assistant, RAG |
| **BE-4** | Backend Dev | Proposals, Chat/SignalR, Articles, Reviews |
| **FE-1** | Frontend Dev | Auth, Cases, Contracts, Payments, Admin |
| **FE-2** | Frontend Dev | Shared Components, Marketplace, Chat, Articles, AI Assistant |

---

## Epic Summary

| Epic ID | Epic Name | Stories | Total SP | Sprint(s) |
|---------|-----------|---------|----------|-----------|
| E-01 | Project Setup & Infrastructure | 6 | 21 | S1 |
| E-02 | Authentication & Registration | 6 | 18 | S1 |
| E-03 | User Profiles & Verification | 5 | 14 | S1-S2 |
| E-04 | Legal Case Management | 5 | 16 | S2 |
| E-05 | AI Case Analysis | 3 | 11 | S2 |
| E-06 | Lawyer Matching | 2 | 8 | S2 |
| E-07 | Lawyer Marketplace | 3 | 8 | S2 |
| E-08 | Proposals | 3 | 9 | S2-S3 |
| E-09 | Communication (Chat) | 4 | 14 | S3 |
| E-10 | Contract Management | 5 | 16 | S2-S3 |
| E-11 | Payments & Escrow | 4 | 16 | S3 |
| E-12 | Reviews & Ratings | 2 | 5 | S3 |
| E-13 | Disputes | 2 | 6 | S4 |
| E-14 | Notifications | 3 | 9 | S2, S4 |
| E-15 | Articles & Knowledge Base | 3 | 9 | S3 |
| E-16 | AI Assistant | 3 | 12 | S3-S4 |
| E-17 | Admin Dashboard | 4 | 12 | S4 |
| | | **63** | **204** | |

---

## E-01: Project Setup & Infrastructure

### SC-001 · Backend Solution Scaffolding
- **As a** developer, **I want** the .NET 8 solution structure created with 3 projects (Core, Infrastructure, API), **so that** the team can begin feature development on a solid foundation.
- **SP:** 5 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend` `infrastructure`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] Solution with SmartCourt.Core, SmartCourt.Infrastructure, SmartCourt.API projects
  - [ ] NuGet packages installed (EF Core, Identity, FluentValidation, Serilog, Swagger, SignalR, MailKit, AutoMapper)
  - [ ] `Program.cs` configured with middleware pipeline (CORS, JWT auth, rate limiting, exception handling, Swagger)
  - [ ] `ApplicationDbContext` created with ALL entity DbSets (all 7 modules from schema)
  - [ ] ALL EF Core entity configurations written (Fluent API) matching schema.md exactly
  - [ ] Initial migration generated and tested against SQL Server
  - [ ] `ApiResponse<T>` wrapper, `PagedRequest`, `PagedResponse` classes created
  - [ ] `ExceptionHandlingMiddleware` with Arabic error messages
  - [ ] `appsettings.json` with all provider configuration sections
  - [ ] Serilog logging configured

### SC-002 · Seed Data — Legal Categories
- **As a** developer, **I want** pre-loaded legal categories in the database, **so that** lawyers can select specializations and cases can be categorized.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend` `data`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] Egyptian legal categories seeded (قانون مدني، قانون جنائي، قانون تجاري، قانون الأسرة، قانون العمل، قانون إداري، قانون عقاري، قانون ضريبي، قانون بحري، etc.)
  - [ ] Seed runs on migration apply

### SC-003 · File Upload Service & Provider
- **As a** developer, **I want** a file storage abstraction (`IFileStorageProvider` → `LocalFileStorageProvider`), **so that** all features can upload/download files through a swappable interface.
- **SP:** 4 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend` `infrastructure`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] `IFileStorageProvider` interface with Upload, Download, Delete, GetUrl methods
  - [ ] `LocalFileStorageProvider` implementation saving to configurable local path
  - [ ] `FileUploadController` with POST endpoint accepting multipart form data
  - [ ] `StoredFile` entity created on upload with metadata (name, type, size, path)
  - [ ] File type validation (PDF, JPG, PNG, MP3, MP4 allowed)
  - [ ] Max file size configurable (default 50MB)
  - [ ] GET endpoint to download file by StoredFileId

### SC-004 · React Project Setup & Core Infrastructure
- **As a** frontend developer, **I want** the React project bootstrapped with routing, RTL, i18n, and API client, **so that** I can start building feature pages.
- **SP:** 4 | **Sprint:** S1 | **Priority:** P0 | **Label:** `frontend` `infrastructure`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Vite + React + TypeScript project initialized
  - [ ] React Router v6 configured with route definitions for all features
  - [ ] i18next configured for Arabic (RTL direction)
  - [ ] Axios instance configured with base URL, JWT token auto-attach interceptor, and 401 redirect
  - [ ] `ApiResponse<T>` TypeScript type mirroring backend wrapper
  - [ ] `AuthProvider` context with login/logout/token management
  - [ ] `ProtectedRoute` component with role-based guards (Client/Lawyer/Admin)
  - [ ] `.env` file with API base URL

### SC-005 · Shared UI Component Library
- **As a** frontend developer, **I want** reusable shared components, **so that** all feature pages have consistent UX and rapid development.
- **SP:** 4 | **Sprint:** S1 | **Priority:** P0 | **Label:** `frontend` `infrastructure`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Layout components: Sidebar (RTL), Top Navbar, Page Container
  - [ ] Client layout and Lawyer layout (different sidebar menus)
  - [ ] DataTable with pagination, sorting, search (RTL-aware)
  - [ ] FileUpload component (drag & drop + button, preview, progress)
  - [ ] Modal / Dialog component
  - [ ] Form components: Input, TextArea, Select, DatePicker (all RTL)
  - [ ] LoadingSpinner, ErrorBoundary, EmptyState, ConfirmDialog
  - [ ] Toast/notification component
  - [ ] Theme system with CSS variables (dark mode ready)

### SC-006 · ICurrentUserService & JWT Claims Extraction
- **As a** backend developer, **I want** a service that extracts the current user's ID and role from the JWT token, **so that** every feature can identify the logged-in user.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend` `infrastructure`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] `ICurrentUserService` with `UserId`, `Email`, `Role`, `IsAuthenticated` properties
  - [ ] `CurrentUserService` implementation reading from `HttpContext.User` claims
  - [ ] Registered in DI as Scoped

---

## E-02: Authentication & Registration

### SC-007 · Client Registration
- **As a** client, **I want** to register with email and password, **so that** I can create an account and access the platform.
- **SP:** 3 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/auth/register/client` with `{ email, password, firstName, lastName, phoneNumber }`
  - [ ] Creates `AspNetUsers` entry + `ClientProfile` entry
  - [ ] Password hashed via ASP.NET Identity
  - [ ] Sends email verification link (via `IEmailProvider`)
  - [ ] Returns 201 with user info (no JWT until verified)
  - [ ] Validation: email format, password strength (min 8 chars, upper + lower + digit), phone format
  - [ ] Duplicate email returns 409 Conflict

### SC-008 · Lawyer Registration
- **As a** lawyer, **I want** to register for an account, **so that** I can start the verification process and join the marketplace.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/auth/register/lawyer` with `{ email, password, firstName, lastName, phoneNumber }`
  - [ ] Creates `AspNetUsers` entry + `LawyerProfile` entry (with default IsAvailable = false)
  - [ ] Sends email verification link
  - [ ] Returns 201

### SC-009 · Login & JWT Token
- **As a** user, **I want** to log in and receive a JWT token, **so that** I can access protected endpoints.
- **SP:** 3 | **Sprint:** S1 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/auth/login` with `{ email, password }`
  - [ ] Validates credentials via Identity
  - [ ] Returns `{ accessToken, refreshToken, expiresAt, user: { id, email, role, firstName, lastName } }`
  - [ ] JWT contains claims: sub, email, role, jti, exp
  - [ ] Access token expires in 60 min (configurable)
  - [ ] Refresh token stored in DB, expires in 7 days
  - [ ] POST `/api/auth/refresh` with `{ refreshToken }` returns new access + refresh token
  - [ ] Non-verified email returns 403 with message "يرجى تأكيد البريد الإلكتروني"

### SC-010 · Email Verification
- **As a** user, **I want** to verify my email address, **so that** my account becomes active.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/auth/verify-email` with `{ userId, token }`
  - [ ] Token generated via Identity's email confirmation token
  - [ ] On success, sets EmailConfirmed = true
  - [ ] Email template in Arabic
  - [ ] POST `/api/auth/resend-verification` to resend the email

### SC-011 · Password Reset
- **As a** user, **I want** to reset my password, **so that** I can regain access if I forget it.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/auth/forgot-password` with `{ email }` — sends reset link
  - [ ] POST `/api/auth/reset-password` with `{ email, token, newPassword }`
  - [ ] Uses Identity's password reset token
  - [ ] Returns success even for non-existent emails (prevent user enumeration)

### SC-012 · Auth Pages (Frontend)
- **As a** user, **I want** login, registration, and password reset pages, **so that** I can authenticate through the web interface.
- **SP:** 6 | **Sprint:** S1 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Login page with email + password form, validation, error display
  - [ ] Client registration page with all fields + validation
  - [ ] Lawyer registration page with all fields + validation
  - [ ] Forgot password page (enter email)
  - [ ] Reset password page (new password form)
  - [ ] Email verification success/error page
  - [ ] All pages fully RTL Arabic
  - [ ] JWT stored in memory (or httpOnly cookie), auto-redirect on 401
  - [ ] Loading states and error handling on all forms

---

## E-03: User Profiles & Verification

### SC-013 · User Profile Management (Backend)
- **As a** user, **I want** to view and update my profile, **so that** my information is current.
- **SP:** 3 | **Sprint:** S1 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] GET `/api/users/profile` — returns current user's profile (client or lawyer)
  - [ ] PUT `/api/users/profile` — update common fields (firstName, lastName, phone, profilePicture)
  - [ ] PUT `/api/users/profile/client` — update client-specific fields (dateOfBirth)
  - [ ] PUT `/api/users/profile/lawyer` — update lawyer-specific fields (bio, officeAddress, yearsOfExperience, isAvailable, specializations)
  - [ ] Profile picture upload via FileUpload service

### SC-014 · Lawyer Verification Submission (Backend)
- **As a** lawyer, **I want** to submit my National ID and Bar Card for verification, **so that** I can become a verified lawyer on the platform.
- **SP:** 3 | **Sprint:** S1 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] POST `/api/lawyer-verification/national-id` with front + back images (StoredFileId)
  - [ ] POST `/api/lawyer-verification/bar-card` with front + back images (StoredFileId)
  - [ ] Sets NationalIdVerificationStatus / BarCardVerificationStatus to Pending
  - [ ] GET `/api/lawyer-verification/status` — returns verification statuses
  - [ ] Triggers notification to admin

### SC-015 · Lawyer Verification Review (Backend — Admin)
- **As an** admin, **I want** to approve or reject lawyer verification documents, **so that** only real lawyers can operate on the platform.
- **SP:** 2 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] GET `/api/admin/verifications/pending` — lists pending verifications
  - [ ] PUT `/api/admin/verifications/{userId}/national-id` with `{ status: Approved|Rejected }`
  - [ ] PUT `/api/admin/verifications/{userId}/bar-card` with `{ status: Approved|Rejected }`
  - [ ] Sets ReviewedByUserId and VerifiedAt timestamp
  - [ ] When both approved → lawyer can receive proposals and publish articles
  - [ ] Sends notification to lawyer on decision

### SC-016 · Profile Pages (Frontend)
- **As a** user, **I want** to view and edit my profile in the web interface, **so that** I can manage my account.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P1 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Client profile page: view + edit all fields
  - [ ] Lawyer profile page: view + edit all fields + specialization selector
  - [ ] Profile picture upload with preview
  - [ ] Lawyer verification document upload page (front + back for National ID and Bar Card)
  - [ ] Verification status display (Pending / Approved / Rejected badges)

### SC-017 · Lawyer Specialization Management (Backend)
- **As a** lawyer, **I want** to select my legal specializations, **so that** clients can find me by area of expertise.
- **SP:** 2 | **Sprint:** S1 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] GET `/api/legal-categories` — list all categories
  - [ ] PUT `/api/users/profile/lawyer/specializations` with `{ categoryIds: [guid] }`
  - [ ] Replaces existing specializations for the lawyer
  - [ ] Validates category IDs exist

---

## E-04: Legal Case Management

### SC-018 · Case CRUD (Backend)
- **As a** client, **I want** to create and manage my legal cases, **so that** I can describe my legal situation and seek help.
- **SP:** 5 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/cases` — create case (title, description, caseLocation) → status = Draft
  - [ ] GET `/api/cases` — list my cases (paginated, filterable by status)
  - [ ] GET `/api/cases/{id}` — case detail (with attachments, latest analysis)
  - [ ] PUT `/api/cases/{id}` — update case (only if Draft or Analyzed)
  - [ ] DELETE `/api/cases/{id}` — soft delete (only if Draft)
  - [ ] Data isolation: clients see only their own cases

### SC-019 · Case Attachments (Backend)
- **As a** client, **I want** to upload supporting documents to my case, **so that** the AI and lawyers have full context.
- **SP:** 2 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/cases/{id}/attachments` — upload file (creates CaseAttachment + StoredFile)
  - [ ] GET `/api/cases/{id}/attachments` — list attachments
  - [ ] DELETE `/api/cases/{id}/attachments/{attachmentId}` — remove attachment
  - [ ] Allowed types: PDF, JPG, PNG, MP3, MP4

### SC-020 · Case Status Transitions (Backend)
- **As a** client, **I want** to submit my case for AI analysis and later for lawyer matching, **so that** I progress through the case lifecycle.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/cases/{id}/submit` — changes status: Draft → Submitted (triggers AI analysis)
  - [ ] POST `/api/cases/{id}/finalize` — changes status: Analyzed → Finalized (triggers matching)
  - [ ] Status machine enforced: Draft → Submitted → Analyzed → Finalized → Matched
  - [ ] Client can resubmit after analysis: Analyzed → Submitted (re-triggers analysis)
  - [ ] FinalSubmittedAt set on finalize

### SC-021 · Case Management Pages (Frontend)
- **As a** client, **I want** case management pages, **so that** I can create, edit, and track my legal cases.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Case list page with status filters, search, pagination
  - [ ] Create case form (title, description, location, file uploads)
  - [ ] Case detail page showing status, description, attachments
  - [ ] Edit case form (pre-filled, only when editable)
  - [ ] Submit for analysis button
  - [ ] Finalize for matching button
  - [ ] Status badges and progress indicator

### SC-022 · Case Detail with AI & Matching (Frontend)
- **As a** client, **I want** to see AI analysis results and matched lawyers on the case detail page, **so that** I can make informed decisions.
- **SP:** 2 | **Sprint:** S2 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Depends on:** SC-025, SC-028
- **Acceptance Criteria:**
  - [ ] AI Analysis section: strengths, weaknesses, missing info, recommendations, confidence score
  - [ ] Analysis history (multiple analyses)
  - [ ] Matched lawyers list with score, rank, and "Send Proposal" button
  - [ ] Resubmit for analysis button (when status = Analyzed)

---

## E-05: AI Case Analysis

### SC-023 · ILlmProvider + OpenAI Implementation
- **As a** developer, **I want** a swappable LLM provider, **so that** we can change AI providers without touching business logic.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] `ILlmProvider` interface: AnalyzeCaseAsync, AskAssistantAsync, GetEmbeddingAsync
  - [ ] `OpenAiProvider` implementation using HttpClient (or OpenAI .NET SDK)
  - [ ] Configuration via `appsettings.json` (API key, model name, temperature, max tokens)
  - [ ] Prompt templates stored as external `.txt` files (not hardcoded)
  - [ ] Token usage tracked (PromptTokens, CompletionTokens, TotalTokens)
  - [ ] Error handling: timeout, rate limiting, API errors → graceful failure
  - [ ] Response parsed into structured `CaseAnalysis` model

### SC-024 · Case Analysis Prompt Engineering
- **As a** developer, **I want** a well-crafted Arabic prompt for case analysis, **so that** the AI produces structured, useful legal analysis.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] System prompt establishes AI as Egyptian legal analysis assistant
  - [ ] Prompt includes: case title, description, category, attachments summary
  - [ ] Output structure: StrengthPoints, WeakPoints, MissingInformation, Recommendations, OverallAssessment, ConfidenceScore
  - [ ] AI determines LegalCategoryId from description
  - [ ] Disclaimer text included in response
  - [ ] Response formatted in Arabic
  - [ ] JSON mode enabled for structured output

### SC-025 · AI Analysis Service
- **As a** client, **I want** my case analyzed by AI after I submit it, **so that** I understand the strengths and weaknesses of my legal situation.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] POST `/api/cases/{id}/analyze` — triggers AI analysis (or auto-triggered on submit)
  - [ ] Creates `AIAnalysis` record with all fields (AnalysisNumber auto-incremented)
  - [ ] GET `/api/cases/{id}/analysis` — returns latest analysis
  - [ ] GET `/api/cases/{id}/analysis/history` — returns all analyses for this case
  - [ ] Analysis stores ModelName, token counts
  - [ ] Case status updated to Analyzed after successful analysis
  - [ ] Error handling: if AI fails, case stays in Submitted status + error notification

---

## E-06: Lawyer Matching

### SC-026 · Lawyer Matching Algorithm
- **As a** developer, **I want** a matching algorithm that scores and ranks lawyers for a case, **so that** clients see the most relevant lawyers.
- **SP:** 5 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] Matching considers: specialization match, years of experience, availability, location proximity, rating average
  - [ ] Each factor has configurable weight
  - [ ] Results cached in `LawyerMatch` table (unique per case + lawyer)
  - [ ] MatchScore (0-100), MatchReason (text explaining why), Rank
  - [ ] Only verified lawyers included
  - [ ] Only available lawyers (IsAvailable = true) included

### SC-027 · Lawyer Matching Endpoint
- **As a** client, **I want** to see matched lawyers for my finalized case, **so that** I can choose the best lawyer.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] POST `/api/cases/{id}/match` — triggers matching (or auto-triggered on finalize)
  - [ ] GET `/api/cases/{id}/matches` — returns ranked list of matched lawyers
  - [ ] Response includes: lawyer profile summary, specializations, matchScore, matchReason, rank
  - [ ] Case status updated to Matched after matching completes
  - [ ] Cache invalidation: re-match available if client requests

---

## E-07: Lawyer Marketplace

### SC-028 · Marketplace Browse & Search (Backend)
- **As a** client, **I want** to browse and search for lawyers, **so that** I can find lawyers independently of AI matching.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-1
- **Depends on:** SC-001
- **Acceptance Criteria:**
  - [ ] GET `/api/marketplace/lawyers` — paginated list with filters:
    - Specialization (LegalCategoryId)
    - Years of experience range
    - Location
    - Availability
    - Rating range
    - Free text search (name, bio)
  - [ ] Response: LawyerCard (id, name, photo, specializations, yearsOfExperience, avgRating, reviewCount)

### SC-029 · Lawyer Public Profile (Backend)
- **As a** client, **I want** to view a lawyer's detailed public profile, **so that** I can evaluate them before sending a proposal.
- **SP:** 2 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] GET `/api/marketplace/lawyers/{userId}` — full public profile:
    - Bio, specializations, experience, location, availability
    - Average rating + review count
    - Recent reviews (latest 5)
    - Published articles (latest 5)
  - [ ] Only shows verified lawyers

### SC-030 · Marketplace Pages (Frontend)
- **As a** client, **I want** a marketplace page to browse and search lawyers, **so that** I can find the right legal help.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P1 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Lawyer listing page with card grid/list toggle
  - [ ] Filter sidebar (specialization, experience, rating, location)
  - [ ] Search bar
  - [ ] Lawyer detail profile page (full info, reviews, articles)
  - [ ] "Send Proposal" button on lawyer profile

---

## E-08: Proposals

### SC-031 · Proposal Service (Backend)
- **As a** client, **I want** to send a proposal to a lawyer, **so that** I can initiate collaboration.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] POST `/api/proposals` with `{ legalCaseId, lawyerUserId, message }` → status = Pending
  - [ ] Auto-creates `Conversation` + `ConversationParticipant` entries (client + lawyer)
  - [ ] Inserts initial message into conversation as first Message
  - [ ] Validates: case is Finalized/Matched, lawyer is verified, no existing active proposal for same case+lawyer
  - [ ] GET `/api/proposals` — list my proposals (client: sent, lawyer: received)
  - [ ] GET `/api/proposals/{id}` — proposal detail
  - [ ] Notifies lawyer of new proposal

### SC-032 · Proposal Accept/Reject (Backend)
- **As a** lawyer, **I want** to accept or reject proposals, **so that** I can choose which cases to take.
- **SP:** 2 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] PUT `/api/proposals/{id}/respond` with `{ status: Accepted|Rejected }`
  - [ ] Only the target lawyer can respond
  - [ ] On Accept: conversation stays open, notification to client
  - [ ] On Reject: conversation closed, notification to client

### SC-033 · Proposal Pages (Frontend)
- **As a** user, **I want** proposal management pages, **so that** I can send, view, and respond to proposals.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Send proposal form (from case detail or lawyer profile)
  - [ ] Client: "My Proposals" list (sent proposals with statuses)
  - [ ] Lawyer: "Received Proposals" list with case summary
  - [ ] Accept/Reject buttons for lawyer
  - [ ] Proposal detail page with case info and conversation link

---

## E-09: Communication (Chat)

### SC-034 · Chat Service & SignalR Hub (Backend)
- **As a** developer, **I want** a real-time chat system using SignalR, **so that** clients and lawyers can communicate instantly.
- **SP:** 5 | **Sprint:** S3 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] `ChatHub` with SignalR: `JoinRoom`, `SendMessage`, `LeaveRoom`
  - [ ] On connect: auto-join all user's active conversations
  - [ ] Messages saved to DB (`Message` table) before broadcast
  - [ ] Participant validation: only conversation members can send/receive
  - [ ] Support MessageType: Text (0), File (1), Voice (2), System (3)
  - [ ] GET `/api/chat/conversations` — list user's conversations
  - [ ] GET `/api/chat/conversations/{id}/messages` — paginated message history

### SC-035 · File & Voice Messages in Chat (Backend)
- **As a** user, **I want** to send files and voice messages in chat, **so that** I can share documents and recordings.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] POST `/api/chat/conversations/{id}/files` — upload file → creates Message + MessageAttachment
  - [ ] Voice messages uploaded as audio files (MP3/WebM)
  - [ ] File attachment includes StoredFileId reference
  - [ ] Broadcast file/voice message via SignalR to room

### SC-036 · Chat UI (Frontend)
- **As a** user, **I want** a real-time chat interface, **so that** I can communicate with my lawyer/client.
- **SP:** 4 | **Sprint:** S3 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Conversation list sidebar
  - [ ] Chat window with message history (scrollable, paginated)
  - [ ] Real-time message display via SignalR
  - [ ] Text input with send button
  - [ ] File attachment button + preview
  - [ ] Voice message recorder (MediaRecorder API) + playback
  - [ ] Online/offline status indicator
  - [ ] RTL text rendering

### SC-037 · SignalR React Hook
- **As a** frontend developer, **I want** a reusable SignalR hook, **so that** chat and notifications can use real-time connections.
- **SP:** 2 | **Sprint:** S3 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] `useSignalR` hook managing connection lifecycle (connect, reconnect, disconnect)
  - [ ] Auto-attaches JWT token to SignalR connection
  - [ ] Event subscription/unsubscription
  - [ ] Connection state exposed (connected, reconnecting, disconnected)

---

## E-10: Contract Management

### SC-038 · Contract CRUD (Backend)
- **As a** user, **I want** to create and manage contracts, **so that** we can formalize the legal service agreement.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/contracts` with `{ proposalId, totalAmount, currency, termsAndConditions }` → status = Draft
  - [ ] Only creatable for accepted proposals
  - [ ] One contract per proposal (unique constraint)
  - [ ] GET `/api/contracts` — list user's contracts
  - [ ] GET `/api/contracts/{id}` — contract detail with milestones
  - [ ] PUT `/api/contracts/{id}` — update terms (only if Draft)

### SC-039 · Milestones (Backend)
- **As a** user, **I want** to define milestones in a contract, **so that** work and payments can be tracked incrementally.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/contracts/{id}/milestones` — add milestone (title, description, amount, dueDate, orderNumber)
  - [ ] PUT `/api/contracts/{id}/milestones/{milestoneId}` — update milestone (only if contract Draft)
  - [ ] DELETE `/api/contracts/{id}/milestones/{milestoneId}`
  - [ ] Sum of milestone amounts must equal contract totalAmount (validated on sign)
  - [ ] Milestone status: Pending → InProgress → Submitted → Approved / Rejected

### SC-040 · Contract Signing (Backend)
- **As a** user, **I want** both parties to sign the contract, **so that** it becomes legally binding and active.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/contracts/{id}/sign` — current user signs
  - [ ] Sets SignedByClientAt or SignedByLawyerAt based on role
  - [ ] When both signed: status → Active, StartedAt set
  - [ ] Validation: milestone amounts = totalAmount before signing allowed
  - [ ] Notification to other party when one signs
  - [ ] POST `/api/contracts/{id}/complete` — mark as completed (both parties agree or milestone-based)
  - [ ] POST `/api/contracts/{id}/cancel` — cancel (only if Draft or Pending)

### SC-041 · Contract Pages (Frontend)
- **As a** user, **I want** contract management pages, **so that** I can create, negotiate, sign, and track contracts.
- **SP:** 4 | **Sprint:** S3 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Create contract form (from accepted proposal)
  - [ ] Contract detail page: terms, milestones, signatures, status
  - [ ] Milestone add/edit/delete within contract
  - [ ] Sign button with confirmation dialog
  - [ ] Status progression display
  - [ ] Contract list page (active, completed, all)
  - [ ] Contract attachments upload

### SC-042 · Milestone Workflow Pages (Frontend)
- **As a** user, **I want** to manage milestone submissions and approvals, **so that** work progress is tracked.
- **SP:** 2 | **Sprint:** S3 | **Priority:** P1 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Lawyer: "Submit milestone" button with deliverable notes
  - [ ] Client: "Approve" / "Reject" milestone buttons
  - [ ] Milestone progress bar on contract detail
  - [ ] Milestone status badges

---

## E-11: Payments & Escrow

### SC-043 · IPaymentProvider + Stub Implementation
- **As a** developer, **I want** a payment provider abstraction, **so that** we can integrate any payment gateway later.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] `IPaymentProvider` interface: CreatePaymentAsync, GetPaymentStatusAsync, RefundPaymentAsync
  - [ ] `StubPaymentProvider` — always returns success (for development/testing)
  - [ ] Configuration to switch between Stub and real provider
  - [ ] Payment flow: Create PaymentRelease → Create PaymentTransaction → Process via provider

### SC-044 · Escrow Deposit & Release (Backend)
- **As a** client, **I want** to deposit funds into escrow when a milestone starts, **so that** the lawyer is guaranteed payment on completion.
- **SP:** 5 | **Sprint:** S3 | **Priority:** P0 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/payments/deposit` with `{ milestoneId }` — creates PaymentRelease (type = Milestone) + PaymentTransaction
  - [ ] GET `/api/payments/contract/{contractId}` — list all payment releases for contract
  - [ ] On milestone approval: POST `/api/payments/release/{paymentReleaseId}` — releases funds
  - [ ] PaymentTransaction status tracking: Pending → Processing → Completed / Failed
  - [ ] On failure: allows retry (new PaymentTransaction for same PaymentRelease)

### SC-045 · Payment Webhook Handler
- **As a** developer, **I want** a webhook endpoint for payment gateway callbacks, **so that** payment status is updated asynchronously.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/payments/webhook` — handles payment gateway callbacks
  - [ ] Validates webhook signature/HMAC
  - [ ] Updates PaymentTransaction status
  - [ ] Sends notification on payment success/failure

### SC-046 · Payment Pages (Frontend)
- **As a** user, **I want** payment/escrow pages, **so that** I can deposit, track, and see payment releases.
- **SP:** 5 | **Sprint:** S4 | **Priority:** P0 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Deposit escrow button on milestone (opens payment flow)
  - [ ] Payment status tracking on contract detail page
  - [ ] Payment history table (all transactions)
  - [ ] Release funds button (after milestone approval)
  - [ ] Payment receipt/summary view

---

## E-12: Reviews & Ratings

### SC-047 · Review Service (Backend)
- **As a** user, **I want** to review the other party after contract completion, **so that** the community benefits from feedback.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/reviews` with `{ contractId, rating (1-5), comment }`
  - [ ] Only after contract status = Completed
  - [ ] Each party can review the other once per contract (unique constraint)
  - [ ] GET `/api/reviews/user/{userId}` — paginated reviews for a user
  - [ ] GET `/api/reviews/user/{userId}/summary` — average rating + count
  - [ ] Reviews are immutable after creation (can update within 24h)

### SC-048 · Review UI (Frontend)
- **As a** user, **I want** to submit and view reviews, **so that** I can rate my experience.
- **SP:** 2 | **Sprint:** S4 | **Priority:** P2 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Review form on completed contract page (star rating + comment)
  - [ ] Reviews list on lawyer/client profile pages
  - [ ] Average rating display with star icons

---

## E-13: Disputes

### SC-049 · Dispute Service (Backend)
- **As a** user, **I want** to raise a dispute on a contract, **so that** an admin can mediate the issue.
- **SP:** 4 | **Sprint:** S4 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-1
- **Acceptance Criteria:**
  - [ ] POST `/api/disputes` with `{ contractId, title, description, attachmentFileIds[] }`
  - [ ] Creates Dispute + DisputeAttachments
  - [ ] Only on active contracts
  - [ ] GET `/api/disputes` — list user's disputes
  - [ ] GET `/api/disputes/{id}` — dispute detail
  - [ ] Admin: PUT `/api/admin/disputes/{id}/assign` — assign moderator
  - [ ] Admin: PUT `/api/admin/disputes/{id}/resolve` — resolve with summary
  - [ ] Notifications to all parties on status changes

### SC-050 · Dispute Pages (Frontend)
- **As a** user, **I want** dispute management pages, **so that** I can raise and track disputes.
- **SP:** 2 | **Sprint:** S4 | **Priority:** P2 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] "Raise Dispute" button on active contract
  - [ ] Dispute form (title, description, file uploads)
  - [ ] Dispute detail page with status tracking
  - [ ] Dispute list page

---

## E-14: Notifications

### SC-051 · Notification Service — In-App (Backend)
- **As a** developer, **I want** an in-app notification service, **so that** users receive real-time updates about platform events.
- **SP:** 4 | **Sprint:** S2 | **Priority:** P1 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] `INotificationService` with `SendAsync(userId, title, message, type)` and `SendToMultipleAsync`
  - [ ] Creates `Notification` + `UserNotification` records
  - [ ] GET `/api/notifications` — paginated list for current user
  - [ ] GET `/api/notifications/unread-count` — count of unread
  - [ ] PUT `/api/notifications/{id}/read` — mark as read
  - [ ] PUT `/api/notifications/read-all` — mark all as read
  - [ ] Pushes to SignalR for real-time bell updates

### SC-052 · Notification Preferences (Backend)
- **As a** user, **I want** to configure how I receive notifications, **so that** I can control my notification channels.
- **SP:** 2 | **Sprint:** S4 | **Priority:** P3 | **Label:** `backend`
- **Assignee:** BE-4
- **Acceptance Criteria:**
  - [ ] GET `/api/notifications/preferences` — get current preferences
  - [ ] PUT `/api/notifications/preferences` with `{ enableInApp, enableEmail, enableSms }`
  - [ ] Default: InApp = true, Email = true, SMS = false
  - [ ] Notification service checks preferences before sending email/SMS

### SC-053 · Notification UI (Frontend)
- **As a** user, **I want** a notification bell and list, **so that** I stay informed about platform activities.
- **SP:** 3 | **Sprint:** S2 | **Priority:** P1 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Notification bell icon in navbar with unread count badge
  - [ ] Dropdown showing latest notifications
  - [ ] Full notifications page with list and "mark all read"
  - [ ] Real-time update via SignalR (badge increments without refresh)
  - [ ] Notification preferences settings page

---

## E-15: Articles & Knowledge Base

### SC-054 · Article Service (Backend)
- **As a** verified lawyer, **I want** to publish legal articles, **so that** I can share my expertise and attract clients.
- **SP:** 4 | **Sprint:** S3 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] POST `/api/articles` with `{ title, summary, content, categoryIds[], attachmentFileIds[] }` → status = PendingApproval
  - [ ] Only verified lawyers can create articles
  - [ ] GET `/api/articles` — public paginated list (only Published articles)
  - [ ] GET `/api/articles/{id}` — article detail (increments ViewCount)
  - [ ] GET `/api/articles/my` — lawyer's own articles (all statuses)
  - [ ] PUT `/api/articles/{id}` — update (only if Draft or PendingApproval)
  - [ ] DELETE `/api/articles/{id}` — soft delete
  - [ ] Admin: PUT `/api/admin/articles/{id}/approve` and `/reject`
  - [ ] Filter by category, search by title/content

### SC-055 · Article Pages (Frontend)
- **As a** user, **I want** to browse and read legal articles, **so that** I can learn about legal topics.
- **SP:** 3 | **Sprint:** S3 | **Priority:** P2 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] Public article listing page (cards with title, summary, author, date)
  - [ ] Category filter sidebar
  - [ ] Article detail page (full content, author info, attachments)
  - [ ] Lawyer: "My Articles" management page
  - [ ] Lawyer: Create/edit article form with rich text editor
  - [ ] Search functionality

### SC-056 · Article Moderation (Backend + Frontend shared with Admin)
- **As an** admin, **I want** to moderate articles, **so that** only quality legal content is published.
- **SP:** 2 | **Sprint:** S4 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] GET `/api/admin/articles/pending` — list pending articles
  - [ ] Approve/reject with reason
  - [ ] Notification to lawyer on decision

---

## E-16: AI Assistant

### SC-057 · Client Legal AI Assistant (Backend)
- **As a** client, **I want** to ask legal questions to an AI assistant, **so that** I can understand basic legal concepts before consulting a lawyer.
- **SP:** 4 | **Sprint:** S3 | **Priority:** P1 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] POST `/api/ai-assistant/conversations` — create new conversation (type = GeneralLegal)
  - [ ] POST `/api/ai-assistant/conversations/{id}/messages` with `{ content }`
  - [ ] Sends to ILlmProvider with legal assistant system prompt
  - [ ] Stores AIMessage (User + AI messages) with token tracking
  - [ ] GET `/api/ai-assistant/conversations` — list user's conversations
  - [ ] GET `/api/ai-assistant/conversations/{id}/messages` — message history
  - [ ] AI response includes disclaimer: "هذا ليس مشورة قانونية"
  - [ ] Conversation title auto-generated from first message

### SC-058 · Lawyer AI Assistant with RAG (Backend)
- **As a** lawyer, **I want** an AI assistant that can search Egyptian law, **so that** I can research legal precedents and regulations quickly.
- **SP:** 5 | **Sprint:** S4 | **Priority:** P2 | **Label:** `backend` `ai`
- **Assignee:** BE-3
- **Acceptance Criteria:**
  - [ ] `IVectorStoreProvider` interface: StoreEmbeddingAsync, SearchSimilarAsync
  - [ ] `QdrantProvider` implementation (self-hosted Qdrant)
  - [ ] POST `/api/ai-assistant/conversations` with `{ type: LawyerResearch, relatedLegalCaseId? }`
  - [ ] RAG pipeline: embed query → search Qdrant → inject context into prompt → LLM
  - [ ] Case-aware assistant: if relatedLegalCaseId provided, case details included in context
  - [ ] Supports: legal research, case summarization, contract drafting assistance

### SC-059 · AI Assistant Chat UI (Frontend)
- **As a** user, **I want** a chat-like interface for the AI assistant, **so that** I can have conversational interactions with the AI.
- **SP:** 3 | **Sprint:** S4 | **Priority:** P1 | **Label:** `frontend`
- **Assignee:** FE-2
- **Acceptance Criteria:**
  - [ ] AI conversation list (sidebar or page)
  - [ ] Chat-style interface (user messages right, AI messages left — RTL)
  - [ ] New conversation button
  - [ ] Message input with send button
  - [ ] Loading indicator while AI responds
  - [ ] Disclaimer banner: "الردود لا تمثل مشورة قانونية"
  - [ ] Copy message button
  - [ ] Markdown rendering for AI responses

---

## E-17: Admin Dashboard

### SC-060 · Admin Dashboard Stats (Backend)
- **As an** admin, **I want** a statistics overview, **so that** I can monitor platform health.
- **SP:** 3 | **Sprint:** S4 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] GET `/api/admin/dashboard` returns:
    - Total users (clients, lawyers, admins)
    - Total cases (by status)
    - Total contracts (by status)
    - Total revenue (completed payments)
    - Pending verifications count
    - Pending articles count
    - Open disputes count
  - [ ] Date range filter support

### SC-061 · Admin User Management (Backend)
- **As an** admin, **I want** to manage user accounts, **so that** I can suspend or reactivate users.
- **SP:** 3 | **Sprint:** S4 | **Priority:** P2 | **Label:** `backend`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] GET `/api/admin/users` — paginated user list with filters (role, status, search)
  - [ ] GET `/api/admin/users/{id}` — user detail (profile, cases, contracts, reviews)
  - [ ] PUT `/api/admin/users/{id}/suspend` — set IsActive = false
  - [ ] PUT `/api/admin/users/{id}/activate` — set IsActive = true
  - [ ] Audit log for all admin actions

### SC-062 · Admin Dashboard Pages (Frontend)
- **As an** admin, **I want** a comprehensive dashboard, **so that** I can manage the platform through a web interface.
- **SP:** 5 | **Sprint:** S4 | **Priority:** P2 | **Label:** `frontend`
- **Assignee:** FE-1
- **Acceptance Criteria:**
  - [ ] Dashboard page with stat cards and charts
  - [ ] User management page (list, search, suspend/activate)
  - [ ] Pending verifications queue (approve/reject)
  - [ ] Pending articles queue (approve/reject)
  - [ ] Disputes list with assignment and resolution
  - [ ] Admin-specific sidebar layout

### SC-063 · Admin Role & Route Protection
- **As a** developer, **I want** admin routes and APIs protected, **so that** only admins can access administrative features.
- **SP:** 1 | **Sprint:** S4 | **Priority:** P0 | **Label:** `fullstack`
- **Assignee:** BE-2
- **Acceptance Criteria:**
  - [ ] All `/api/admin/*` endpoints decorated with `[Authorize(Roles = "Admin")]`
  - [ ] Frontend: admin routes guarded by role check
  - [ ] Unauthorized access returns 403
