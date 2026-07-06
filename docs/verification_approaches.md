# Verification Process: Architectural Approaches

When designing a secure verification and document upload system, there are several architectural approaches available. This document outlines the currently proposed approach, explores viable alternatives, and concludes with a recommendation on the best fit for the Smart Court project.

---

## 1. The Existing Approach: Decoupled File Upload

**Flow:**
1. The client uploads physical documents (binary) via a dedicated File Upload API.
2. The server stores the file (or offloads it to a cloud provider) and returns a lightweight `StoredFileId`.
3. The client submits a lightweight JSON `POST /verifications` request containing only the `StoredFileId` references.

**Pros:**
- **Separation of Concerns:** Business logic (Verification API) is strictly separated from binary data handling (File API).
- **Clean JSON Payloads:** The verification payload remains lightweight and strongly typed.
- **Resilience:** If the verification submission fails due to validation errors, the user does not need to re-upload large files over the network.

**Cons:**
- **Dangling Files (Orphans):** If a user uploads files but closes the app before submitting the verification request, the storage system holds unused files. A background cleanup job (Cron) is required to purge unlinked files.
- **Client Complexity:** Requires the frontend to orchestrate two separate API calls and manage intermediate state.

---

## 2. Alternative 1: Monolithic Multipart/Form-Data Upload

**Flow:**
The client sends a single API request containing both the JSON/form fields (Verification data) and the physical file binaries using the `multipart/form-data` content type.

**Pros:**
- **Atomic Operations:** The verification request and the file upload succeed or fail together. No orphaned files.
- **Simpler Client Logic:** Only one API call is needed from the frontend.

**Cons:**
- **Heavy Server Load:** The application server must buffer and parse large binary streams alongside business logic, which can bottleneck performance.
- **Complex Validation:** Validating the business logic (e.g., checking if the user already has a pending application) often happens *after* the server has already started receiving the heavy file stream, wasting bandwidth.

---

## 3. Alternative 2: Direct-to-Cloud via Pre-Signed URLs (Serverless Pattern)

**Flow:**
1. The client requests a temporary "Pre-Signed URL" from the Smart Court API.
2. The API generates a secure, time-limited URL from the cloud provider (e.g., AWS S3, Azure Blob) and returns it.
3. The client uploads the binary file **directly** to the cloud provider using that URL, bypassing the Smart Court backend entirely.
4. The client submits the Verification Application to the backend, referencing the cloud file.

**Pros:**
- **Zero Backend Bottleneck:** The application server never touches the binary file streams, saving massive amounts of CPU, memory, and bandwidth.
- **Infinite Scalability:** Relies entirely on cloud provider infrastructure for heavy lifting.

**Cons:**
- **Complex Setup:** Requires configuring CORS, IAM roles, and strict bucket policies on the cloud provider.
- **Synchronous Security:** Harder to run synchronous virus scans or file type validation before the file lands in your storage bucket.

---

## 4. Alternative 3: Base64 Encoded JSON Payloads

**Flow:**
The client converts the image or PDF into a Base64 string and embeds it directly inside the JSON body of the verification request.

**Pros:**
- **Single Request:** Everything is sent in one standard JSON payload. No multipart parsing needed.

**Cons:**
- **Massive Overhead:** Base64 encoding increases file size by roughly 33%.
- **Memory Spikes:** The server must load the entire enormous JSON string into RAM to deserialize it, leading to potential Out-Of-Memory (OOM) exceptions under load.
- **Anti-Pattern:** Widely considered a bad practice for anything larger than tiny thumbnail images.

---

## 5. Alternative 4: Third-Party KYC Identity Verification (SaaS)

**Flow:**
Instead of building a custom manual review process, the system integrates with a dedicated Identity Verification provider (e.g., Jumio, Onfido, Stripe Identity). The user uploads their ID directly to the provider's SDK, which uses AI/OCR to verify the document's authenticity and user's liveness. The provider then fires a Webhook to Smart Court with a `Pass/Fail` result.

**Pros:**
- **Automated Approvals:** Zero manual admin work required. AI handles fraud detection, OCR data extraction, and liveness checks.
- **High Security & Compliance:** Offloads legal liability for storing sensitive government IDs to specialized security companies.

**Cons:**
- **High Cost:** Providers typically charge $1.00 to $2.50+ per verification attempt.
- **Vendor Lock-in:** Highly dependent on external SDKs and service uptime.

---

## Conclusion & Best Fit Recommendation

For the **Smart Court** project, the best fit depends on the exact constraints of the environment (e.g., Budget, Enterprise scale). 

### 🏆 Recommended Approach: The Existing Decoupled Upload (or Direct-to-Cloud)

1. **Why it wins:** Given that this appears to be an educational or startup-tier project (ITI Graduation Project), the **Existing Decoupled File Upload** is highly professional, demonstrates enterprise-level separation of concerns, and avoids the heavy performance penalties of `multipart/form-data`.
2. **The "Next Level" Upgrade:** If you want to impress the reviewers with state-of-the-art cloud architecture, you should adapt the existing decoupled flow into **Alternative 2: Direct-to-Cloud via Pre-Signed URLs**. This demonstrates a deep understanding of cloud scalability and serverless patterns.
3. **What to Avoid:** Do not use Base64 JSON (Alternative 3). Avoid Monolithic Multipart (Alternative 1) unless the frontend team strongly struggles with orchestrating two API calls. Avoid Third-Party KYC (Alternative 4) as it defeats the purpose of building and showcasing your own verification module logic for a graduation project.

**Actionable Next Step:** 
Stick with the current architecture defined in the API documentation, but consider adding a small background worker (e.g., using Hangfire or a simple .NET Hosted Service) that runs nightly to delete records from the `StoredFiles` table that are older than 24 hours and have no associated `VerificationAssets` (to clean up orphaned files).
