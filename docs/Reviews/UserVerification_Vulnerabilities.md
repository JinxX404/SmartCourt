# User Verification Slice - Vulnerability & Bug Report

**Date of Review:** August 5, 2026
**Target Slice:** `UserVerification` (`Features/UserVerification`)
**Discovered By:** Automated HTTP Integration Tests

## 🚨 Critical Security Vulnerability: Missing Authorization

### Description
The `UserVerificationController` is completely missing the `[Authorize]` attribute (or it is improperly configured). The automated tests sent requests to the `POST`, `GET`, and `DELETE` endpoints with an empty or missing Bearer token, and the API successfully processed them (`200 OK`) instead of rejecting them with `401 Unauthorized`.

### Impact
Any unauthenticated user (or anonymous actor) can bypass security constraints to view, submit, or delete verification documents for any registered lawyer or client, provided they know or can guess the target's `UserId` (GUID).

### Steps to Reproduce
1. Start the API.
2. Do **not** log in. Send a `GET` request to `/api/UserVerification/{Valid_UserId}` without an `Authorization` header.
3. The API will return a `200 OK` with the user's private documents.

### Recommended Fix
Add the `[Authorize]` attribute to the `UserVerificationController` or directly to the individual endpoints. Ensure that the logic also verifies that the requesting user (`User.Claims`) matches the `UserId` in the query/route, or that the requester is an Admin.

---

## 🐛 Unhandled Exception (500 Error) on Empty Document List

### Description
When submitting a verification document via `POST /api/UserVerification/submit-verification-documents`, if the form-data request omits the `Documents` array entirely (or sends an empty list), the application crashes and returns a `500 Internal Server Error` instead of a standardized `400 Bad Request` validation error.

### Error Details
- **Endpoint:** `POST /api/UserVerification/submit-verification-documents`
- **Payload:** `{ "UserId": "valid-guid", "Documents": [] }`
- **Result:** `500 Internal Server Error` 

### Recommended Fix
Review the `SubmitVerificationDocumentsCommandValidator` to ensure that empty collections fail validation gracefully before they reach the service layer. Ensure `RuleFor(x => x.Documents).NotEmpty()` is working correctly and that the service handles null cases if the model binder fails to initialize the collection.

---

## ⚠️ Unhandled Scenario: Missing File Type/MIME Validation

### Description
When submitting a verification document, the application does not appear to validate the file extension or MIME type before processing. In the stress tests, uploading a `.exe` file (disguised as a verification document) bypassed any file format checks. It was only stopped later in the pipeline by a duplicate document type check (`"You already uploaded this document before"`).

### Impact
Malicious users could upload executable payloads, scripts, or very large unsupported files, potentially leading to remote code execution (if stored improperly) or storage abuse.

### Recommended Fix
Add a validator for `IFormFile` within `SubmitVerificationDocumentsCommandValidator` (or the respective service) to whitelist allowed file extensions (e.g., `.jpg`, `.png`, `.pdf`) and validate the actual MIME signature before allowing the file to reach the storage provider.

---

## ⚠️ Unhandled Scenario: 401 Unauthorized Returns Empty Body

### Description
The project's architectural constraints require that **all responses must be wrapped in `ApiResponse<T>`**. However, when an endpoint properly rejects an unauthorized request (e.g., `GET /api/admin/verifications` without a valid admin token), the API returns a standard `401 Unauthorized` with an entirely empty body instead of a serialized `ApiResponse<T>` object.

### Impact
Client applications parsing the JSON response might throw unhandled deserialization exceptions when attempting to read the `ApiResponse` schema from a 401/403 status code.

### Recommended Fix
Configure a custom handler for unauthorized/forbidden responses in `Program.cs` (e.g. `services.AddAuthentication().AddJwtBearer(options => { options.Events = new JwtBearerEvents { OnChallenge = ... }})`) or add a custom middleware to capture `401/403` status codes and override the response body with a properly formatted `ApiResponse<T>.Unauthorized()` structure before it leaves the pipeline.

---

## ⚠️ Bug: Inconsistent Identifier Mapping (`DocumentId`) Across Slices

### Description
There is a critical mismatch in how `DocumentId` is mapped and exposed between the `UserVerification` and `AdminVerifications` slices. 
- In the **Admin** slice (`GetVerificationDetailsHandler.cs`), `DocumentId` maps to `UserVerificationDocument.Id` (the primary key of the verification entity).
- In the **User** slice (`GetUserVerificationDocumentsHandler.cs` and `DeleteVerificationDocumentHandler.cs`), `DocumentId` maps to `StoredFileId` (the foreign key to the storage table).

### Impact
This creates a fragile integration environment. If an Admin or a frontend client attempts to pass a `DocumentId` retrieved from the Admin endpoint into the User's Delete endpoint, it returns a `404 Not Found` because the database queries different columns for the same variable name.

### Recommended Fix
Standardize the identifier. Update the `UserVerification` slice to use `UserVerificationDocument.Id` consistently instead of `StoredFileId` for DTOs and Delete queries. Specifically, change `d.StoredFile.Id == request.DocumentId` to `d.Id == request.DocumentId` in `DeleteVerificationDocumentHandler` and fix the `Select` projection in `GetUserVerificationDocumentsQueryHandler`.

---

## ⚠️ Unhandled Scenario: Framework Validation & Limits Bypass `ApiResponse<T>`

### Description
While application-level errors (caught by MediatR handlers and custom FluentValidation) correctly return `ApiResponse<T>`, framework-level model binding errors entirely bypass the `ApiResponse<T>` wrapper and return the standard RFC 9110 `ValidationProblemDetails` JSON. 
This was triggered during the exhaustive tests when:
1. Submitting a malformed GUID (`not-a-guid`).
2. Submitting an invalid Date format (`13-13-2030`).
3. Exceeding the `30MB` Kestrel multipart body limit (Stress Test).

### Impact
Client applications strictly expecting the `{ success, data, message, errors, statusCode }` schema will crash when parsing the `ValidationProblemDetails` schema.

### Recommended Fix
Suppress the default `[ApiController]` validation response by adding `builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);` in `Program.cs`. This allows the requests to reach your custom FluentValidation pipeline or a custom ActionFilter that wraps model state errors into `ApiResponse<T>.Fail()`.

---

## ⚠️ Unhandled Scenario: 405 Method Not Allowed Returns Empty Body

### Description
Similar to the `401 Unauthorized` behavior, sending a request with an invalid HTTP method (e.g., `POST` to a `GET` endpoint or `GET` to the `DELETE` endpoint) results in a `405 Method Not Allowed` with an empty body, failing to comply with the `ApiResponse<T>` wrapper standard.

### Recommended Fix
Configure a fallback route or middleware to intercept `405` status codes from the routing pipeline and rewrite the response to use the `ApiResponse<T>` schema.

---

## ⚠️ Bug: Broken Re-upload Lifecycle (`IsCurrent` Leak & Accidental Downgrades)

### Description
There is a massive logic flaw in the `SubmitVerificationDocumentsHandler` regarding the document replacement lifecycle. When a lawyer uploads a replacement document (e.g., their previous one was Rejected or Expired), the handler creates a new `UserVerificationDocument` with `IsCurrent = true` but **never updates the previous document to `IsCurrent = false`**. 

Additionally, there is no guard clause preventing a fully verified (`Active`) lawyer from submitting new documents. Because of the `IsCurrent` leak, they will end up with multiple "Current" documents of the same type in the database (one `Verified` and one `Pending`). 

### Impact
1. **Database Corruption**: The `IsCurrent` flag loses its meaning as lawyers accumulate multiple "current" documents of the same type.
2. **Accidental Account Downgrade**: If a fully verified `Active` lawyer accidentally hits the submit endpoint again, the new document is saved as `Pending`. The `VerificationStatusEvaluator` will immediately downgrade their entire account status from `Active` to `PendingReview` because it detects *any* current document in a `Pending` state.

### Recommended Fix
In `SubmitVerificationDocumentsHandler.cs`, before creating the new document, query the database for any existing document of the same `DocumentType` where `UserId == request.UserId` and `IsCurrent == true`. Update that existing document to `IsCurrent = false`. Furthermore, consider adding a guard clause to block document submissions for a specific type if the lawyer already has a non-expired, `Verified` document of that type.
