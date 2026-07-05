# Blueprint for an AI-Powered Lawyer Marketplace in Egypt

## Executive Summary

This report outlines a production-ready blueprint for an AI-powered, dual-sided marketplace that connects Egyptian clients with licensed lawyers, anchored in compliance with Egyptian Bar, data protection, and court system requirements.[^1][^2] It focuses on two main pillars: an AI case evaluator grounded in Egyptian law and an intelligent lawyer matchmaking engine that respects rights of audience across court tiers.[^3][^4]

The document covers (1) regulatory guardrails, (2) advanced AI capabilities, (3) business and operational modules, and (4) technical architecture and data models tailored for Egypt, including integration with local payments (Fawry, wallets) and adherence to Personal Data Protection Law No. 151/2020 and Law No. 15/2004 on e-signatures.[^5][^2][^6]

## 1. Egyptian Legal Ecosystem & Regulatory Guardrails

### 1.1 Bar Association Compliance and Unauthorized Practice

The legal profession is regulated by Law No. 17 of 1983 on Advocacy as amended, and the Egyptian Bar Association oversees admission, discipline, and practice standards.[^1][^7] Article 71 of the Advocacy Law prohibits lawyers from using advertising, inducement, intermediaries, or exploiting real or alleged influence when practicing law, restricting overt commercial promotion.[^8]

Commentary from legal scholars notes that the prohibition targets promotional conduct that undermines dignity of the profession, while modern debates focus on whether using social media and legal platforms constitutes prohibited advertising or a permissible form of digital presence.[^9][^10] Legal analysis of electronic legal consultations emphasizes that lawyers in lower registration grades (جدول عام, ابتدائي) have restrictions on providing written legal opinions, especially under Articles 26 and 34 of the Advocacy Law.[^10]

#### Implications for the Marketplace

- The platform should position itself as an infrastructure service and directory (منصة تنظيمية وخدمة تواصل) rather than as an advertising channel promising results or using comparative language (“أفضل محامي في مصر”).[^9][^8]
- Lawyer profiles should be factual and standardized: name, درجة القيد (جدول عام، ابتدائي، استئناف، نقض), تخصصات (جنائي، مدني، مجلس الدولة، أسرة)، سنوات الخبرة، ورسوم استشارية، avoiding promotional slogans or client testimonials that could be viewed as advertising.[^4]
- The platform must not itself provide legal representation or sign pleadings; it only facilitates communication and contract formation between client and independent lawyer to avoid مزاولة مهنة المحاماة بدون ترخيص.[^11][^1]
- Written automated outputs (AI case summaries, document boilerplates) must be explicitly labeled as "مسودة أولية لأغراض المعلومات العامة وليست استشارة قانونية" and prompt users to consult a licensed lawyer.[^10]

### 1.2 Handling of Legal Consultations by Grade of Lawyer

Egyptian law differentiates between lawyers on the general roll (جدول عام), primary courts (ابتدائي), appeal (استئناف عالي ومجلس الدولة), and cassation (نقض) with corresponding rights to give written opinions and appear before courts.[^12][^4]

- Lawyers on the general roll may attend certain investigations and minor cases before الجزئية courts but are restricted from providing written legal opinions and signing many types of contracts.[^10][^4]
- Primary court lawyers (ابتدائي) can open offices, appear before الجزئية and ابتدائية courts, and provide oral legal consultations but still face restrictions on written opinions.[^10][^13]
- Appeal (استئناف) and cassation (نقض) lawyers have broader rights including giving legal opinions, drafting complex contracts, and appearing before higher courts and مجلس الدولة.[^12][^4]

Marketplace policy should therefore:

- Allow all registered lawyers to take part in initial generic Q&A forums as long as responses are kept high-level and not formal written opinions.
- Restrict formal "مذكرة رأي قانوني" and complex contract drafting services (e.g., M&A, high-value commercial) to lawyers with استئناف or نقض registration.
- Clearly display each lawyer’s درجة القيد and automatically constrain which paid services (e.g., كتابة صحيفة دعوى أمام محكمة الاستئناف) they can offer through product configuration.

### 1.3 Data Privacy & Security under Law 151/2020

Personal Data Protection Law No. 151/2020 applies to any natural person’s personal data processed electronically, whether by controllers or processors inside or outside Egypt when data subjects reside in Egypt.[^14][^2] The law defines personal data broadly (including national ID, voice, address, health, economic and social identity), and categorizes health, biometric, financial, religious, political, and children’s data as sensitive.[^15][^16]

Controllers and processors must obtain licenses or permits from the Personal Data Protection Center to collect, store, or process personal data or to conduct electronic marketing, and must implement technical and organizational measures to protect data, ensure confidentiality, and prevent breaches.[^5][^2] The law and draft executive regulations require explicit, written consent for collection and processing (especially for sensitive data), purpose limitation, data minimization, deletion or anonymization after purpose expiry, and breach notification to the authority.[^17][^18]

#### Implications for Litigation and ID Data

Client litigation files, police reports, national IDs, and case documents will contain multiple categories of sensitive personal data and sometimes children’s data (e.g., محاضر جنح أحداث, قضايا أسرة).[^15][^16]

Platform obligations as controller/processer include:

- Obtain a controller/processor license from the Personal Data Protection Center and appoint a Data Protection Officer (مسئول حماية البيانات) responsible for compliance and liaison with the Center.[^5][^16]
- Implement strong access controls, encryption at rest and in transit, and logging to protect document repositories and databases, given the explicit requirement to take "جميع الإجراءات التقنية والتنظيمية" to prevent breach, hacking, alteration, or manipulation.[^15][^17]
- Collect only data necessary for matchmaking and case handling (purpose limitation) and define retention periods per case type, after which data is anonymized or deleted in line with obligations to erase or deliver data upon expiry of processing.[^2][^18]
- Obtain explicit consent for voice recording, document upload, and the use of data to train or fine-tune AI models, with clear opt-out mechanisms.[^2]
- Ensure cross-border transfers (e.g., cloud hosting outside Egypt) comply with conditions for transferring data to foreign controllers/processors, which require Center approval and alignment of purposes and protection levels.[^16]

### 1.4 Avoiding Unauthorized Practice of Law by the Platform

Unauthorized practice of law in Egypt generally involves giving specific legal opinions, drafting pleadings, or representing parties without being a licensed lawyer registered with the Bar.[^11][^1] The advocacy law also restricts corporate legal departments and non-lawyers from providing advocacy services to third parties.[^11]

To avoid مزاولة مهنة بدون ترخيص:

- The AI case evaluator must clearly state that analyses are informational and that final legal advice must be provided by a licensed lawyer engaged via a توكيل رسمي عام قضايا or specific mandate.[^11]
- The platform should not appear on court documents as representative; pleadings must carry only the lawyer’s details, even if drafted using platform tools.
- Revenue models should focus on SaaS-style fees (subscription, lead fees, or flat platform fees) while avoiding fee sharing or contingency fee participation in a manner that would make the platform appear as a co-practitioner; any success-based pricing must be structured as performance-based SaaS or marketing fees, not as a share of lawyer’s أتعاب قضائية.

### 1.5 Rights of Audience & Court Tier Alignment

Egyptian lawyers progress through registration grades that determine where they may appear and plead: جدول عام, ابتدائي, استئناف عالي ومجلس الدولة, and نقض.[^12][^4] General roll lawyers can attend investigations and represent clients before الجزئية courts and in many family law first-instance cases, but cannot appear before الاستئناف or النقض.[^12][^13]

Primary court (ابتدائي) lawyers may appear before الجزئية and ابتدائية courts, administrative courts of first instance, and handle most civil, commercial, labor, and family cases at first instance, but may not plead before courts of appeal, cassation, or the higher administrative/cassation courts.[^12][^4]

Lawyers on the استئناف roll can appear before appeal courts and مجلس الدولة (مجلس الدولة والمحاكم الإدارية)، with broader capacity to issue legal opinions and draft complex contracts, while نقض lawyers can plead before محكمة النقض, المحكمة الإدارية العليا, and the Supreme Constitutional Court.[^12][^4]

#### Platform Court Tier Logic

The marketplace must encode these rights of audience in rules used by the matchmaking engine and booking workflow:

- Every lawyer account includes verified grade from the Bar (integrated via upload of Bar ID and manual verification, potentially via direct API or offline integration with نقابة المحامين if available).[^7][^1]
- Every case evaluation produced by the AI must include mapped court tier and نوع الدعوى: e.g., جنحة أمام محكمة جنح مصر الجديدة (جزئية), دعوى مدنية أمام محكمة شمال القاهرة الابتدائية, طعن أمام محكمة استئناف القاهرة, طعن بالنقض.[^19][^20]
- The matchmaker filters out ineligible lawyers: for example, for a طعن بالنقض, only محامي نقض is eligible; for an appeal before محكمة استئناف, only استئناف أو نقض; for a دعوى ابتدائية, ابتدائي أو أعلى.[^12][^4]
- When a client tries to book a lawyer below the required grade (e.g., جدول عام for a civil دعوى أمام المحكمة الابتدائية), the UI explains that "هذه الدرجة من القيد لا تسمح بالمرافعة أمام هذه المحكمة" and suggests appropriate alternatives.

## 2. Advanced Native AI Capabilities

### 2.1 AI Case Evaluator Agent

The AI case evaluator ingests unstructured user input—text, scans of official documents (محضر شرطة، تقارير طبية، عقود، صحف دعاوى)، and voice notes—and produces a structured legal case profile.[^21][^22] This profile should include: نوع النزاع (جنائي/مدني/أحوال شخصية/عمل/مجلس دولة)، الوقائع الجوهرية (ملخص زمني للأحداث)، الأساس القانوني المحتمل (مواد القانون المدني، قانون العقوبات، قانون العمل، قانون مجلس الدولة)، تقدير مبدئي لقوة الموقف (ضعيف/متوسط/قوي) with reasoning, قائمة بالأدلة الحالية والمفقودة، والتوصية بنوع الدعوى والإجراء القادم.[^19][^23]

Core steps:

1. **Ingestion & normalization**: OCR Arabic documents (including official forms and handwriting where feasible), normalize dates, names, and amounts, and map them to entities (أطراف، شهود، جهات حكومية).[^23]
2. **Legal issue spotting**: Use domain-specific LLMs fine-tuned on Egyptian cases and statutes to tag applicable legal domains (e.g., سرقة، نصب، تعويض، فسخ عقد إيجار، دعوى إلغاء قرار إداري).[^23]
3. **Risk & strength assessment**: Based on precedent snippets and statutory elements, the agent checks completeness of أركان الجريمة or أركان المسؤولية المدنية, highlighting missing elements (e.g., عنصر الخطأ، الضرر، علاقة السببية).[^19]
4. **Evidence and paperwork checklist**: Generate curated lists of required documents (e.g., توكيل رسمي عام قضايا، بطاقة الرقم القومي، عقد مسجل، إيصالات سداد، شهادة ميلاد، صحيفة الحالة الجنائية) and procedural steps tied to the court system.[^19][^20]

All outputs must be explainable, with references to legal provisions and anonymized precedent summaries to help lawyers quickly review the AI’s analysis.

### 2.2 AI Document Boilerplate Generator

The platform can include an AI-powered generator that drafts localized boilerplate documents using standard Egyptian legal phrasing, with configuration options and clear lawyer approval steps.[^21][^22]

Target document families:

- **إنذار على يد محضر**: Templates for rent arrears, breach of contract, termination of agency, and putting debtor in default, including proper structure (مقدم الإنذار، المنذر إليه، الموضوع، الأسباب، الطلبات).[^19]
- **صحيفة دعوى**: First-instance claim briefs for civil, commercial, labor, and family disputes, including standard headings (محكمة …، الدائرة …، المدعي، المدعى عليه، الوقائع، الطلبات، الأساس القانوني).[^19][^20]
- **عقود الإيجار والبيع**: Standard residential and commercial lease contracts and simple sale agreements, pre-populated with typical Egyptian clauses (المقدم، المؤخر، الشرط الفاسخ الصريح، بنود التسليم، الاختصاص القضائي).[^24]
- **وكالة عامة قضايا**: Assist in filling data fields used in the standardized توكيل رسمي عام قضايا formats (client and lawyer names, addresses, permitted actions), although actual issuance occurs at الشهر العقاري.

Workflow:

- Client or lawyer selects document type and answers a structured questionnaire (أطراف العقد، الثمن، المدة، البيانات الأساسية).
- AI generates a draft in Arabic using standard legal language, tagging variables and optional clauses.
- A licensed lawyer reviews, edits, and approves before download or filing, satisfying requirements that legal opinions and pleadings be made by qualified lawyers.

### 2.3 Retrieval-Augmented Generation on Egyptian Legal Sources

A core differentiator is a robust RAG system grounded in Egyptian legal sources to minimize hallucinations and keep AI outputs aligned with up-to-date law.[^23]

Key corpora:

- **أحكام محكمة النقض**: Decisions published by the Court of Cassation, accessible via the official website and collections, covering both civil and criminal matters.[^23][^4]
- **أحكام المحكمة الإدارية العليا ومحاكم مجلس الدولة** for administrative and public law disputes.
- **الوقائع المصرية** (Official Gazette) for statutes, regulations, decrees, and executive decisions.[^14]
- **Consolidated statutes**: Civil Code, Commercial Code, Criminal Code, Evidence Law, Civil and Criminal Procedure Codes, Family Law, Labor Law, Data Protection, E-signature, etc.

RAG architecture:

- **Document ingestion & preprocessing**: Download, OCR (where necessary), and parse judgments and gazette issues into structured JSON with fields such as court, chamber, date, case number, keywords, legal principles, and holdings.[^23]
- **Chunking & embeddings**: Split texts into semantically coherent chunks (e.g., 200–500 words) and encode them using Arabic-capable embedding models; index in a vector database such as Qdrant or Pinecone.[^23]
- **Query understanding**: Map user queries and the internal prompts from the case evaluator to retrieval queries (e.g., "فسخ عقد إيجار للتكرار في التأخير عن سداد الأجرة"), retrieving top-k relevant cases and statutory articles.
- **Context-constrained generation**: LLM generation is restricted to retrieved context, and prompts require citing specific articles and case holdings, with refusal behavior when retrieved context is insufficient.
- **Versioning and time-awareness**: For statutes with amendments (e.g., قانون المحاماة، قانون حماية البيانات), metadata includes effective date ranges so that AI can reason about the law as of the facts’ date.[^1][^2]

Governance:

- Periodic updates of corpus from Official Gazette and court websites.
- Manual curation of "leading cases" and common legal principles to enrich retrieval quality.
- Logging of queries and human review of contested outputs to refine retrieval and prompting.

### 2.4 Voice-to-Text and Colloquial Arabic Normalization

Many Egyptian users prefer voice notes in العامية المصرية and may describe events informally and emotionally rather than in legal structure.[^22] The platform can differentiate via robust voice ingestion:

- **Speech recognition**: Integrate ASR models tuned for Egyptian Arabic (either custom models or fine-tuned open models) to transcribe WhatsApp-like voice notes into text.[^22]
- **Dialect normalization**: Apply NLP pipelines to map colloquial expressions into Modern Standard Arabic legal phrasing; for instance, converting "هو ضحك عليا وخد منى الفلوس" into a structured fraud narrative referencing نصب per قانون العقوبات.[^19]
- **Entity and timeline extraction**: Extract key entities (أسماء، عناوين، مبالغ، تواريخ، جهات حكومية) and events and align them in a timeline to support the evaluator.[^19]
- **Interactive clarification**: The agent can ask clarifying questions in colloquial Arabic ("هو كتبلك إيصال بالأموال دي؟") and convert answers into structured fields.

The result is a clean, lawyer-friendly case summary in Arabic, with attached anonymized transcripts and the original audio, ready for lawyer review.

### 2.5 Risk Controls and Human-in-the-Loop Design

Given regulatory sensitivity and potential for harm, AI capabilities must be designed with human oversight:

- All AI-generated legal content is tagged as draft and must be explicitly "approved" by a lawyer before being shared with external parties or submitted to courts.[^10]
- Safety filters should detect and block hostile or unethical requests (e.g., instructions to forge documents or bribe officials), responding with legal and ethical guidance instead.
- For high-risk case types (جنايات، قضايا أمن دولة، قضايا أحوال شخصية حساسة، قضايا أطفال), the AI should provide minimal guidance and strongly encourage direct consultation with a specialized lawyer.

## 3. Production-Ready Marketplace Features

### 3.1 Core User Roles and Onboarding

- **Clients (أصحاب القضايا)**: Individuals and SMEs registering with mobile number and national ID verification, able to submit cases via text, document upload, or voice, browse and book lawyers, manage payments, and track case status.[^21][^25]
- **Lawyers (المحامون)**: Egyptian Bar members registering with Bar ID, degree of registration, and verification of identity, choosing practice areas, courts, and fee structures, and integrating calendars and payment preferences.[^21][^26]
- **Admins (إدارة المنصة)**: Compliance, support, and data teams managing verification queues, dispute resolution, KYC audits, and configuration of AI models and marketplace rules.

Onboarding flows must integrate KYC for clients and KYL (Know Your Lawyer) verification for lawyers, including document validation and possibly selfie/ID verification to mitigate identity fraud.[^5]

### 3.2 Intelligent Matchmaker System

The intelligent matchmaker uses the structured case profile produced by the AI case evaluator to propose ranked lists of lawyers.[^21][^25]

Matching dimensions:

- **Legal domain and sub-domain**: جنائي (جنح، جنايات)، مدني، تجاري، تعويضات، أسرة، عمل، مجلس دولة، ضرائب، استثمار.
- **Court tier & geography**: Match jurisdiction (e.g., محكمة شمال القاهرة الابتدائية, محكمة استئناف القاهرة, محكمة النقض) and ensure lawyer’s درجة القيد is sufficient.[^3][^4]
- **Experience & track record**: Years in practice, self-declared specialization, and anonymized win/loss or settlement statistics when available.
- **Availability and response time**: Calendar openings for استشارة sessions and reaction times.
- **Language & communication preferences**: Arabic only, Arabic/English, or Arabic/English/French, and preferred communication channels.

Output:

- A ranked shortlist with 3–7 lawyers, each profile showing درجة القيد, specialization, city, estimated consultation fee, average rating, and a "fit score" explaining why they match the case.
- Filter and sort controls enabling clients to prioritize budget, seniority, location, or ratings.

### 3.3 Consultation Engine

The consultation engine operationalizes lawyer-client interactions in a compliant and user-friendly way similar to platforms like Mohamy and ElMetr but with deeper workflow integration.[^21][^25]

Key features:

- **Multi-channel scheduling**: Real-time calendar viewing and booking for 30/60/90-minute sessions, with automatic timezone handling and buffer times between sessions.[^25]
- **Integrated video/audio**: WebRTC-based in-browser video and voice with fallback to audio-only, optimized for lower bandwidth conditions common in Egypt.[^25]
- **Automated time tracking**: Automatic start/stop tracking when session begins/ends; overrun handling with lawyer-configured grace periods and overtime billing rules.
- **Session artifacts**: AI-generated consultation summary in Arabic that captures key issues, recommendations, and next steps, which the lawyer edits and approves before the client sees it.
- **Follow-up tickets**: Convert recommendations into tasks (e.g., تجهيز إنذار، رفع دعوى، التقدم بتظلم أمام جهة إدارية) with due dates and responsible party (client or lawyer).

### 3.4 Lawyer Thought Leadership & Knowledge Hub

A knowledge hub can incentivize lawyers to contribute content and build reputation while enabling SEO and user education.[^22]

Components:

- **Articles (مقالات قانونية)**: Lawyers publish posts tagged by domain (قانون جنائي، مدني، عمل، أحوال شخصية، ضرائب، استثمار) with moderation by platform legal editors.
- **Q&A threads**: Public anonymized questions answered by lawyers in short form; answers are clearly labeled as general information, not individual advice.
- **Gamification**: Reputation points, badges (e.g., خبير جنائي، خبير أحوال شخصية), and rankings based on engagement, quality ratings, and peer reviews.
- **Searchable library**: Full-text search over content with filters by domain, court level, and author degree.

These modules help lawyers market themselves in a manner more aligned with professional dignity—through expertise and contribution rather than advertising slogans.

### 3.5 Escrow & Localized Payments

Egypt’s payments landscape includes Fawry, mobile wallets (Vodafone Cash, Orange Money, Etisalat Cash), InstaPay, and local card networks; Fawry provides an API-based payment gateway for cards, e-wallets, and Pay-at-Fawry reference numbers.[^27][^28]

Proposed payment architecture:

- **Escrow accounts**: Client payments for retainers (مقدم أتعاب) and milestones are held in a segregated escrow account under a licensed payment service provider or bank partner, disbursed to lawyers upon milestone completion or by schedule.
- **Milestone structure**: For litigation, milestones can be: قبول التكليف والاطلاع، قيد الدعوى، أول جلسة، مرحلة الخبراء، حجز الدعوى للحكم، استلام الصورة التنفيذية؛ for transactional work, signing term sheet, draft contract, negotiation round, closing.
- **Payment methods**: Integration with FawryPay REST APIs to support card payments, e-wallets, and Pay-at-Fawry codes; support for wallet payouts to lawyers and bank transfers for settlements.[^27][^28]
- **Retainer and success fees**: Product configuration supports splitting fees into مقدم (retainer) and مؤخر (success or completion fee) consistent with local practice while avoiding the platform sharing in the lawyer’s contingency fee in a way that would be considered fee-splitting.[^4]

Additional considerations:

- Clearly documented terms and dispute resolution rules on when funds are released or refunded.
- Invoices and receipts with tax-compliant fields for lawyers subject to VAT and income tax reporting.

### 3.6 Case Milestone Tracker

A case CRM-like dashboard provides transparency to clients and structure to lawyers.

Features:

- **Matter overview**: For each case, show court, case number, opposing parties, type of claim, and lawyer in charge.
- **Stage tracking**: Predefined stages depending on case type, e.g. for جنائي: محضر الشرطة، تحقيقات النيابة، جلسات محكمة الجنح/الجنايات، الحكم، الطعن؛ for مدني: قيد الدعوى، الإعلان، الجلسات، الخبراء، الحكم، الاستئناف/النقض.[^19][^20]
- **Document repository**: Secure, versioned storage of pleadings, notifications, expert reports, judgments, and powers of attorney.
- **Notifications**: SMS/app notifications for upcoming جلسات, new document uploads, and changes in case status, with caution around not disclosing sensitive data in notification content to comply with data protection.

Lawyers can update case status via web or mobile apps, and optionally the platform can integrate with court e-services if APIs or scraping are legally permissible.

### 3.7 Dispute Resolution and Quality Control

The marketplace must handle disputes between clients and lawyers and maintain quality standards:

- **Internal complaint system**: Clients can file complaints about communication, misrepresentation, or fee disputes which are handled by platform support and, where necessary, referred to the Bar for disciplinary issues.
- **Rating and reviews**: Post-engagement ratings with moderation and defamation safeguards, focusing reviews on professionalism, clarity, and responsiveness rather than case outcomes.
- **KYB and ongoing vetting**: Periodic checks on lawyer registration status, disciplinary actions, and any bar suspension by reviewing Bar publications or integrating with نقابة المحامين systems.[^7][^1]

## 4. Technical Architecture & Data Strategy

### 4.1 High-Level System Architecture

The platform should adopt a modular, service-oriented or microservices architecture with clear separation between marketplace, AI services, and data infrastructure.

Logical layers:

- **Presentation layer**: Web and mobile clients in Arabic-first UX, with English localization for lawyers accustomed to English legal terms.
- **Backend services**:
  - User and identity service (clients, lawyers, admins, roles and permissions)
  - Case management service (case intake, evaluations, milestones, documents)
  - Matching and recommendation service (lawyer ranking and search)
  - Consultation and communication service (scheduling, video, chat)
  - Payment and billing service (escrow, invoicing, payouts)
  - Content service (articles, Q&A, notifications)
- **AI services**:
  - Case evaluator and summarization engine (LLM-based)
  - RAG service for legal knowledge retrieval
  - ASR and dialect normalization service
  - Document boilerplate generator
- **Data infrastructure**:
  - Relational database for transactional data (e.g., PostgreSQL)
  - Object storage for documents and audio (e.g., S3-compatible, on-prem or regional cloud compliant with cross-border rules)
  - Vector database for legal text and embeddings
  - Analytics warehouse for usage metrics and business intelligence.

Security architecture includes API gateways, OAuth2/OpenID Connect for authentication, RBAC/ABAC for authorization, encrypting data in transit (TLS) and at rest, and audit logging for access to sensitive records.

### 4.2 Data Model Overview

A high-level entity relationship design should cover core marketplace objects and their interactions.

Key entities and relationships:

- **User**: Common base entity with subtypes Client and Lawyer; includes authentication credentials, contact info, and preferences.
- **ClientProfile**: Linked 1:1 with User; holds KYC status, verified ID, and demographic info.
- **LawyerProfile**: Linked 1:1 with User; fields include Bar registration number, درجة القيد, النقابة الفرعية, specialization tags, years of experience, hourly rate, and verification status.
- **CaseIntake**: Represents raw submissions from clients (text, audio, documents) with references to uploaded files.
- **CaseEvaluation**: Linked 1:N to CaseIntake (supporting re-evaluations), storing structured fields: legal domain, sub-domain, court tier, jurisdiction, strength assessment, identified legal issues, evidence list, and recommended actions.
- **Matter (Case)**: Created once client engages a lawyer; includes references to CaseEvaluation(s), assigned LawyerProfile, court details, case number, and status.
- **CaseStage**: Child entity of Matter representing milestones (stage type, start date, end date, notes, attached documents).
- **ConsultationBooking**: Session bookings between ClientProfile and LawyerProfile with scheduled time, duration, video link, and post-session summary.
- **PaymentTransaction**: Represents client payments into escrow, milestone releases, refunds, and marketplace fees, tied to Matters or Consultations.
- **Article and QAEntry**: Content entities for the knowledge hub; link to LawyerProfile and tags for legal domain and court tier.

Relationships:

- One Client can have many CaseIntakes and Matters.
- One Lawyer can handle many Matters and Consultations.
- Each Matter can have multiple CaseStages and PaymentTransactions.
- CaseEvaluations feed into Matching recommendations and Matter creation.

### 4.3 Tech Stack Recommendations

For a scalable Egyptian legaltech marketplace with AI, a modern, cloud-native stack is recommended:

- **Frontend**: React or Next.js with TypeScript for web; React Native or Flutter for cross-platform mobile apps to reach a wide user base.[^27]
- **Backend**: Node.js (NestJS) or Python (FastAPI) for API services, with GraphQL or REST; languages integrate well with AI and data tooling ecosystems.
- **Database**: PostgreSQL for relational data; Redis for caching sessions and search results; object storage (e.g., MinIO or AWS S3) for documents and media.
- **Search & Vector DB**: Elasticsearch or OpenSearch for full-text search across lawyers and content; Qdrant, Milvus, or Pinecone for vector embeddings from legal corpora.
- **AI/ML stack**: Python-based microservices using frameworks like PyTorch or TensorFlow; use Arabic-capable LLMs and ASR models, with on-prem or region-hosted LLM gateways for privacy.
- **DevOps & Infrastructure**: Kubernetes or managed container orchestration with CI/CD pipelines; secrets management (e.g., Vault), centralized logging, monitoring, and compliance controls to satisfy PDPL security obligations.[^15][^17]
- **Payments & KYC**: Integration with FawryPay APIs for collections, with connectors for mobile wallets and bank transfers.[^27][^28]

Cloud selection should consider data residency and PDPL cross-border rules; an approach is to deploy in a regionally hosted data center (e.g., in Egypt or nearby) and ensure any transfer outside Egypt has Center authorization.

### 4.4 Data Governance and Lifecycle

Data strategy must align with legal requirements and product needs:

- **Retention policies**: Define and enforce different retention schedules: e.g., longer for active litigations, shorter for unconverted leads; implement automatic archival and anonymization.[^2][^18]
- **Access control**: Use fine-grained ACLs so that only the assigned lawyer and the client (and essential admins) can access each case; implement field-level encryption for especially sensitive data like financials or minors’ data.[^15]
- **Audit trails**: Log who accessed or modified which files and when; this is necessary for incident investigation and demonstrating compliance.[^17]
- **Model training data**: For AI fine-tuning, use anonymized or synthetic datasets derived from real cases with client consent and robust de-identification protocols.

### 4.5 Integration with E-Signature and E-Contracts

Law No. 15 of 2004 grants electronic signatures the same legal validity as handwritten signatures when issued via licensed certification service providers and properly verified.[^29][^6] Under the Civil Code, a contract is formed when offer and acceptance meet regardless of format unless a specific form is mandated (e.g., real estate), and electronic communications like emails and messaging apps can be evidence of contracts when identity and intent are clear.[^24]

For the marketplace:

- Integrate with licensed e-signature providers for signing engagement letters, fee agreements, and some contracts, while respecting that certain contracts (such as many real estate transfers) still require الرسمية and registration.[^24][^30]
- Provide a digital record (PDF + audit trail) of lawyer-client engagement and fee agreements that can be enforced if disputes arise.

## 5. Prioritized Roadmap (High-Level)

A pragmatic phased roadmap helps move from MVP to full-featured platform.

- **Phase 1 – MVP (6–9 months)**:
  - Core marketplace: client and lawyer onboarding, profiles, manual matching, simple consultation bookings with video, and basic payment integration (card + Fawry Pay-at-Fawry).[^21][^25]
  - Basic AI: intake summarization and domain classification, simple evidence checklist, but no automated drafting.
  - Compliance foundations: PDPL controller license application, DPO appointment, basic security controls, and clear disclaimers around non-legal-advice.

- **Phase 2 – AI Expansion (9–18 months)**:
  - Build robust case evaluator, RAG over selected statutes and leading Court of Cassation decisions, and dialect voice intake.
  - Launch boilerplate generator for الإنذارات وصحف الدعاوى البسيطة and simple contracts with lawyer review.
  - Introduce case milestone tracker and initial knowledge hub.

- **Phase 3 – Scale & Deep Legal Integration (18+ months)**:
  - Expand RAG coverage to broader jurisprudence and مجلس الدولة decisions, refine retrieval and explainability.
  - Deepen payments (milestone-based escrow, wallet payouts) and tax-compliant invoicing.
  - Explore integrations with court e-services where legally and technically feasible.
  - Implement advanced analytics and recommendation improvements based on outcomes and satisfaction.

---

## References

1. [قانون المحاماة - نقابة المحامين المصرية](https://egyls.com/%D9%82%D8%A7%D9%86%D9%88%D9%86-%D8%A7%D9%84%D9%85%D8%AD%D8%A7%D9%85%D8%A7%D8%A9/) - قانون رقم ١٧ لسنة ١٩٨٣ بإصدار قانون المحاماة وفقاً لآخر تعديل صادر في ٨ يوليو ٢٠٢٠. يعمل بأحكام القا...

2. [قانون حماية البيانات الشخصية - Masaarmasaar.net › ... › قوانين الاتصالات وتقنية المعلومات في مصر](https://masaar.net/ar/egypt_laws/%D9%82%D8%A7%D9%86%D9%88%D9%86-%D8%AD%D9%85%D8%A7%D9%8A%D8%A9-%D8%A7%D9%84%D8%A8%D9%8A%D8%A7%D9%86%D8%A7%D8%AA-%D8%A7%D9%84%D8%B4%D8%AE%D8%B5%D9%8A%D8%A9/) - قانون رقم ١٥١ لسنة ٢٠٢٠ تاريخ النشر : ١٥ – ٠٧ – ٢٠٢٠ نوع الجريدة : القوانين الرئيسية مضمون التشريع :...

3. [أنواع المحاميين في مصر: الدرجات والاختصاصات | المتر - Elmetr](https://www.elmetr.com/news/75/%D8%A3%D9%86%D9%88%D8%A7%D8%B9-%D8%A7%D9%84%D9%85%D8%AD%D8%A7%D9%85%D9%8A%D9%8A%D9%86-%D9%81%D9%8A-%D9%85%D8%B5%D8%B1:-%D8%A7%D9%84%D8%AF%D8%B1%D8%AC%D8%A7%D8%AA-%D9%88%D8%A7%D9%84%D8%A7%D8%AE%D8%AA%D8%B5%D8%A7%D8%B5%D8%A7%D8%AA-) - يمتلك المحامي هنا صلاحيات اكبر بكثير من المحامي الابتدائي أهمها الحضور والمرافعة أمام محاكم الاستئنا...

4. [محامي بالنقض: دليلك لتوكيل أفضل محامي نقض في مصر والشرق الأوسط](https://www.tcmglaw.com/post/cassation-lawyer) - هل تشعر بالظلم من الحكم القضائي وترغب في الإستعان بمحامي نقض لإعادة فتح قضيتك؟ اترك الأمر لمحامي بال...

5. [Overview on Data Protection Law - ADSERO](https://adsero.me/data-protection-law/)

6. [About Electronic Signature - قطاع النقل البحري واللوجستيات](https://www.mts.gov.eg/en/%D9%86%D8%A8%D8%B0%D9%87-%D8%B9%D9%86-%D8%A7%D9%84%D8%AA%D9%88%D9%82%D9%8A%D8%B9-%D8%A7%D9%84%D8%A7%D9%84%D9%83%D8%AA%D8%B1%D9%88%D9%86%D9%8A/) - About Electronic Signature

7. [النقض: منازعات القيد بنقابة المحامين اختصاص القضاء الإدارى وليس العادى - اليوم السابع](https://www.youm7.com/story/2020/1/14/%D8%A7%D9%84%D9%86%D9%82%D8%B6-%D9%85%D9%86%D8%A7%D8%B2%D8%B9%D8%A7%D8%AA-%D8%A7%D9%84%D9%82%D9%8A%D8%AF-%D8%A8%D9%86%D9%82%D8%A7%D8%A8%D8%A9-%D8%A7%D9%84%D9%85%D8%AD%D8%A7%D9%85%D9%8A%D9%86-%D8%A7%D8%AE%D8%AA%D8%B5%D8%A7%D8%B5-%D8%A7%D9%84%D9%82%D8%B6%D8%A7%D8%A1-%D8%A7%D9%84%D8%A5%D8%AF%D8%A7%D8%B1%D9%89-%D9%88%D9%84%D9%8A%D8%B3-%D8%A7%D9%84%D8%B9%D8%A7%D8%AF%D9%89/4586563) - أكدت محكمة النقض خلال نظرها الطعن رقم 6837 لسنة 88 أن المحاكم الابتدائية ومحاكم الاستئناف غير مختصة ...

8. [قانون المحاماة والإدارات القانونية وفق احدث التعديلات القانون رقم 17 لسنة ...](https://www.facebook.com/100063995204913/posts/%D9%82%D8%A7%D9%86%D9%88%D9%86-%D8%A7%D9%84%D9%85%D8%AD%D8%A7%D9%85%D8%A7%D8%A9%D9%88%D8%A7%D9%84%D8%A5%D8%AF%D8%A7%D8%B1%D8%A7%D8%AA-%D8%A7%D9%84%D9%82%D8%A7%D9%86%D9%88%D9%86%D9%8A%D8%A9%D9%88%D9%81%D9%82-%D8%A7%D8%AD%D8%AF%D8%AB-%D8%A7%D9%84%D8%AA%D8%B9%D8%AF%D9%8A%D9%84%D8%A7%D8%AA%D8%A7%D9%84%D9%82%D8%A7%D9%86%D9%88%D9%86-%D8%B1%D9%82%D9%85-17-%D9%84%D8%B3%D9%86%D8%A9-1983%D9%85%D8%A7%D9%84%D9%82%D8%A7/505591896128769/) - قانون المحاماة والإدارات القانونية وفق احدث التعديلات القانون رقم 17 لسنة 1983م القانون رقم 10 لسنة ...

9. [استخدام المحامي وسائل الدعاية بين الإباحة والتجريم](https://egyls.com/%D8%A7%D8%B3%D8%AA%D8%AE%D8%AF%D8%A7%D9%85-%D8%A7%D9%84%D9%85%D8%AD%D8%A7%D9%85%D9%8A-%D9%88%D8%B3%D8%A7%D8%A6%D9%84-%D8%A7%D9%84%D8%AF%D8%B9%D8%A7%D9%8A%D8%A9-%D8%A8%D9%8A%D9%86-%D8%A7%D9%84%D8%A5/) - د. ريهام فتحي دكتور القانون الجنائي حظرت المادة 71 من قانون المحاماة المصري رقم 17 لسنة 1983 والمعدل...

10. [الاستشارات القانونية الإلكترونية بين الجواز والبطلان](https://egyls.com/%D8%A7%D9%84%D8%A7%D8%B3%D8%AA%D8%B4%D8%A7%D8%B1%D8%A7%D8%AA-%D8%A7%D9%84%D9%82%D8%A7%D9%86%D9%88%D9%86%D9%8A%D8%A9-%D8%A7%D9%84%D8%A5%D9%84%D9%83%D8%AA%D8%B1%D9%88%D9%86%D9%8A%D8%A9-%D8%A8%D9%8A/) - بقلم: يوسف أمين حمدان ثار جدلٌ في الأيام الأخيرة على السوشيال ميديا وبالأخص مواقع التواصل الاجتماعي ...

11. [قضية رقم 15 لسنة 17 قضائية المحكمة الدستورية العليا "دستورية"](https://hrlibrary.umn.edu/arabic/Egypt-SCC-SC/Egypt-SCC-15-Y17.html)

12. [درجات قيد نقابة المحامين ⚖️](https://www.youtube.com/watch?v=mg08bA_XxDQ) - درجات قيد نقابة المحامين ⚖️
الاستاذ محمد القصاص المحامي
#القانون #معلومات #القانون_الجنائي #المحاماة...

13. [انواع المحامين في مصر ودرجاتهم واختصاصتهم](https://www.youtube.com/watch?v=fa1_x0WoOEA) - #أحمد_الخولي_المحامي 
#شغل_اداري
#تعليم_المحاماة_من_الصفر
#المحامي_المبتدأ
#محامين_الجدول_العام
#طلا...

14. [Egypt: Country's First Law on Protection of Personal Data Enters into ...](https://www.loc.gov/item/global-legal-monitor/2020-10-29/egypt-countrys-first-law-on-protection-of-personal-data-enters-into-force/) - (Oct. 29, 2020) On October 17, 2020, Egypt’s first law to protect personal data entered into force. ...

15. [Data Protection Law](https://www.privacylaws.com/media/3263/egypt-data-protection-law-151-of-2020.pdf)

16. [قانون رقم ١٥١ لسنة ٢٠٢٠ بإصدار قانون حماية البيانات الشخصية .](https://elhak.org/2020/07/15/1170/) - ى تطبيق أحكام هذا القانون ، يقصد بالكلمات والعبارات التالية المعنى المبين قرين كل منها :البيانات الش...

17. [[PDF] االلئحة التنفيذية لقانون حماية البيانات الشخصية](https://eipr.org/sites/default/files/reports/pdf/_-_hmy_lbynt-2.pdf)

18. [مادة ( 5 ) : القانون رقم 151 لسنة 2020 بإصدار قانون حماية البيانات الشخصية](https://masaar.net/ar/egypt_laws/%D9%85%D8%A7%D8%AF%D8%A9-5-%D8%A7%D9%84%D9%82%D8%A7%D9%86%D9%88%D9%86-%D8%B1%D9%82%D9%85-151-%D9%84%D8%B3%D9%86%D8%A9-2020-%D8%A8%D8%A5%D8%B5%D8%AF%D8%A7%D8%B1-%D9%82%D8%A7%D9%86%D9%88%D9%86/) - ثانيا : التزامات المعالج مادة ( 5 ) : مع مراعاة أحكام المادة (12) من هذا القانون ، يلتزم معالج البيا...

19. [GUIDE TO DISPUTE RESOLUTION IN AFRICA](http://lawforall.info/uploads/142/89/0177E_Guide_to_dispute_resolution_in_African_nations_EGYPT.PDF)

20. [التسلسل الهرمي للمحاكم المدنية في مصر من إعداد مؤسسة الخليفة للمحاماة ...](https://www.facebook.com/100063764738626/posts/%EF%B8%8F-%D8%A7%D9%84%D8%AA%D8%B3%D9%84%D8%B3%D9%84-%D8%A7%D9%84%D9%87%D8%B1%D9%85%D9%8A-%D9%84%D9%84%D9%85%D8%AD%D8%A7%D9%83%D9%85-%D8%A7%D9%84%D9%85%D8%AF%D9%86%D9%8A%D8%A9-%D9%81%D9%8A-%D9%85%D8%B5%D8%B1-%D9%85%D9%86-%D8%A5%D8%B9%D8%AF%D8%A7%D8%AF%D9%85%D8%A4%D8%B3%D8%B3%D8%A9-%D8%A7%D9%84%D8%AE%D9%84%D9%8A%D9%81%D8%A9-%D9%84%D9%84%D9%85%D8%AD%D8%A7%D9%85%D8%A7%D8%A9-%D9%88%D8%A7%D9%84%D8%A7%D8%B3%D8%AA%D8%B4%D8%A7%D8%B1/1382786090523536/) - أحكامها يجوز استئنافها أمام المحكمة الابتدائية. أمثلة: محكمة بولاق الجزئية – محكمة شبرا الجزئية – مح...

21. [ElMetr - Launch Africa Ventures](https://www.launchafrica.vc/elmetr)

22. [Legal Tech Startup Hekouky is Committed to Making Egyptian Law Accessible | Egyptian Streets](https://egyptianstreets.com/2020/11/19/legal-tech-startup-hekouky-is-committed-to-making-egyptian-law-accessible/) - How much do Egyptians know about their rights and laws? Not much, new legaltech startup Hekouky says...

23. [محكمة النقض المصرية: الرئيسية - محكمة النقض](https://www.cc.gov.eg) - امتداد ميعاد الطعن بالنقض وتقديم الأسباب الى اليوم التالى لنهايته إذا صادف يوم عطلة رسمية . أثر ذلك ...

24. [You emailed the contract. | Ahmed El-Gazzar, LL.M - LinkedIn](https://www.linkedin.com/posts/ahmed-el-gazzar-ll-m-1a7371120_ahmedelgazzar-digitalcontracts-egyptianlaw-activity-7322512023374774272-I-pP) - You emailed the contract. The other party replied “Agreed.” Or maybe the deal was confirmed over Wha...

25. [Egyptian Startup Mohamy Connects User with Lawyers in ...](https://thestartupscene.me/BehindTheStartup/Egyptian-Startup-Mohamy-Connects-User-with-Lawyers-in-Virtual-Offices) - Aiming to democratise the legal industry, Mohamy streamlines the lawyer-client connection and hopes ...

26. [Elmetr . (500 startups portfolio company) - CodeX TechIndex](http://techindex.law.stanford.edu/companies/12352) - Legal Tech Business Database

27. [Fawry | APIs.io Providers](https://providers.apis.io/providers/fawry/) - Fawry publishes 1 API on the APIs.io network: FawryPay Server API. Tagged areas include Payments, E-...

28. [FawryPay Online Payments](https://developer.fawrystaging.com/docs/introduction) - FawryPay is an easy e-commerce solution that connects sellers with buyers offering different payment...

29. [Latest Articles](https://www.easternnco.com/show_article/2/How%20to%20Use%20Electronic%20Signatures%20in%20Egypt%E2%80%94Legally%20and%20Securely)

30. [Law No. 15 of 2004 on E-signature and Establishment of the ... - WIPO](https://www.wipo.int/wipolex/en/legislation/details/13546) - Egypt - Year of Version: 2004 - Adopted: April 21, 2004 - IP-related Laws - Other

