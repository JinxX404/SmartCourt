# Smart Court API Documentation

## 1. Get Lawyer Profile

### API Description
Retrieves the profile information of a specific lawyer. This endpoint intentionally excludes uploaded sensitive files (e.g., ID Card, Bar Association Certificate) for security and privacy reasons.

### Endpoint
`/api/v1/lawyers/{id}`

### HTTP Method
`GET`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token for authorization. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The unique identifier of the lawyer. |

### Query Parameters
None.

### Request Body
None.

### Field table
*Response Fields*

| Field | Type | Required | Description |
|---|---|---|---|
| Id | Guid | Yes | Unique identifier of the lawyer. |
| Name | String | Yes | Full name of the lawyer. |
| Email | String | Yes | Email address used for login and contact. |
| PhoneNumber | String | Yes | Primary contact phone number. |
| NationalNumber | String | Yes | Unique national identification number. |
| Gender | String | Yes | Gender of the lawyer (e.g., Male, Female). |
| DateOfBirth | Date | Yes | Date of birth. |
| Specialization | String | Yes | Area of legal practice. |
| YearsOfExperience | Int | Yes | Number of years in legal practice. |
| Bio | String | No | A short biography or description. |
| Address | String | No | Primary office or residential address. |
| Status | String | Yes | Account status (e.g., Active, PendingVerification). |

### Validation Rules
- `id` must be a valid Guid.
- The user requesting the profile must have the required permissions (e.g., self-access, or Admin role).

### Business Logic
- Extract the user ID from the JWT token and verify authorization to access the requested profile.
- Query the database for the user with the specified `id` where the role is `Lawyer`.
- Map the database entity to a Data Transfer Object (DTO) explicitly excluding file navigation properties to ensure sensitive files are not exposed.

### Database Operations
- **SELECT**: Query the `Users` table (or dedicated `Lawyers` table, depending on inheritance strategy) joining related profile details.

### Related Tables
- `Users` (Role = Lawyer)

### Uploaded Files
- **Excluded**: File IDs or metadata for ID Card File and Bar Association File are explicitly removed from the response.

### Example Request
```http
GET /api/v1/lawyers/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Example Response
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Ahmed Youssef",
    "email": "ahmed.youssef@example.com",
    "phoneNumber": "+201012345678",
    "nationalNumber": "29001011234567",
    "gender": "Male",
    "dateOfBirth": "1990-01-01",
    "specialization": "Corporate Law",
    "yearsOfExperience": 8,
    "bio": "Experienced corporate lawyer based in Cairo.",
    "address": "123 Legal St, Cairo",
    "status": "Active"
  }
}
```

### Error Responses
- **401 Unauthorized**: Missing or invalid Bearer token.
- **403 Forbidden**: User does not have permission to view this profile.
- **404 Not Found**: Lawyer with the specified ID does not exist.

### Notes
- Uses `ApiResponse<T>.Ok()` wrapper as per project standards.

---

## 2. Get Client Profile

### API Description
Retrieves the profile information of a specific client, excluding uploaded sensitive files.

### Endpoint
`/api/v1/clients/{id}`

### HTTP Method
`GET`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token for authorization. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The unique identifier of the client. |

### Query Parameters
None.

### Request Body
None.

### Field table
*Response Fields*

| Field | Type | Required | Description |
|---|---|---|---|
| Id | Guid | Yes | Unique identifier of the client. |
| Name | String | Yes | Full name of the client. |
| Email | String | Yes | Email address used for login and contact. |
| PhoneNumber | String | Yes | Primary contact phone number. |
| NationalNumber | String | Yes | Unique national identification number. |
| Gender | String | Yes | Gender of the client (e.g., Male, Female). |
| DateOfBirth | Date | Yes | Date of birth. |
| Address | String | No | Primary residential address. |
| Status | String | Yes | Account status. |

### Validation Rules
- `id` must be a valid Guid.
- The user requesting the profile must have the required permissions.

### Business Logic
- Query the database for the user with the specified `id` where the role is `Client`.
- Map entity to a response DTO excluding file references.

### Database Operations
- **SELECT**: Query the `Users` table (Role = Client).

### Related Tables
- `Users` (Role = Client)

### Uploaded Files
- **Excluded**: Any file records (e.g., ID Card) are excluded.

### Example Request
```http
GET /api/v1/clients/1fa85f64-5717-4562-b3fc-2c963f66afb7 HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
```

### Example Response
```json
{
  "success": true,
  "data": {
    "id": "1fa85f64-5717-4562-b3fc-2c963f66afb7",
    "name": "Mona Ali",
    "email": "mona.ali@example.com",
    "phoneNumber": "+201122334455",
    "nationalNumber": "29505051234567",
    "gender": "Female",
    "dateOfBirth": "1995-05-05",
    "address": "456 Main St, Alexandria",
    "status": "Active"
  }
}
```

### Error Responses
- **401 Unauthorized**: Missing or invalid token.
- **404 Not Found**: Client does not exist.

### Notes
- Uses `ApiResponse<T>.Ok()` wrapper.

---

## 3. Update Lawyer Profile

### API Description
Updates the editable profile information for a lawyer. Core identity fields and legal documents cannot be modified through this endpoint.

### Endpoint
`/api/v1/lawyers/{id}`

### HTTP Method
`PUT`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |
| Content-Type | application/json | Yes | Must be application/json. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The unique identifier of the lawyer. |

### Query Parameters
None.

### Request Body
| Field | Type | Required | Description |
|---|---|---|---|
| Email | String | Yes | Updated email address. |
| PhoneNumber | String | Yes | Updated phone number. |
| DateOfBirth | Date | Yes | Updated date of birth. |
| Specialization | String | Yes | Updated area of legal practice. |
| YearsOfExperience | Int | Yes | Updated years of experience. |
| Bio | String | No | Updated biography. |
| Address | String | No | Updated address. |

### Field table
*(See Request Body table)*

### Validation Rules
- **Name, National Number, Gender, ID Card File, and Bar Association File** MUST NOT be present in the DTO.
- `Email` must be a valid email address and unique across the system.
- `PhoneNumber` must be a valid phone number format.
- `YearsOfExperience` must be >= 0.

### Business Logic
- Validate the incoming DTO using FluentValidation.
- Verify the `id` in the route matches the authenticated user's ID, or the requester is an Admin.
- Retrieve the existing Lawyer entity.
- Update only the permitted fields on the entity.
- Save changes to the database.

### Database Operations
- **SELECT**: Retrieve the existing Lawyer entity.
- **UPDATE**: Modify the allowed fields and update the `UpdatedAt` timestamp.

### Related Tables
- `Users`

### Uploaded Files
- **N/A**: This endpoint does not handle file uploads.

### Example Request
```http
PUT /api/v1/lawyers/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
Content-Type: application/json

{
  "email": "ahmed.newemail@example.com",
  "phoneNumber": "+201099998888",
  "dateOfBirth": "1990-01-01",
  "specialization": "Corporate Law & Tax",
  "yearsOfExperience": 9,
  "bio": "Updated bio information.",
  "address": "789 New Office St, Cairo"
}
```

### Example Response
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "message": "Profile updated successfully."
  }
}
```

### Error Responses
- **400 Bad Request**: Validation failed (e.g., invalid email format, attempting to update restricted fields if manually injected into payload against DTO definition).
- **401 Unauthorized**: Missing or invalid token.

### Notes
- Uses `ApiResponse<T>.Ok()` on success. Throw `BusinessException` for domain errors.

---

## 4. Update Client Profile

### API Description
Updates the editable profile information for a client. Core identity fields cannot be modified through this endpoint.

### Endpoint
`/api/v1/clients/{id}`

### HTTP Method
`PUT`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |
| Content-Type | application/json | Yes | Must be application/json. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The unique identifier of the client. |

### Query Parameters
None.

### Request Body
| Field | Type | Required | Description |
|---|---|---|---|
| Email | String | Yes | Updated email address. |
| PhoneNumber | String | Yes | Updated phone number. |
| DateOfBirth | Date | Yes | Updated date of birth. |
| Address | String | No | Updated address. |

### Field table
*(See Request Body table)*

### Validation Rules
- **Name, National Number, Gender, ID Card File, and Bar Association File** MUST NOT be present in the DTO.
- `Email` must be a valid email address and unique.

### Business Logic
- Validate the incoming DTO.
- Retrieve the existing Client entity.
- Update permitted fields and save.

### Database Operations
- **SELECT**: Retrieve the existing Client entity.
- **UPDATE**: Modify the allowed fields.

### Related Tables
- `Users`

### Uploaded Files
- **N/A**

### Example Request
```http
PUT /api/v1/clients/1fa85f64-5717-4562-b3fc-2c963f66afb7 HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
Content-Type: application/json

{
  "email": "mona.new@example.com",
  "phoneNumber": "+201155556666",
  "dateOfBirth": "1995-05-05",
  "address": "999 New Home St, Alexandria"
}
```

### Example Response
```json
{
  "success": true,
  "data": {
    "id": "1fa85f64-5717-4562-b3fc-2c963f66afb7",
    "message": "Profile updated successfully."
  }
}
```

### Error Responses
- **400 Bad Request**: Validation failed.
- **401 Unauthorized**: Missing or invalid token.

---

## 5. Verification Module

### Architecture & Data Flow

The Verification Module manages the identity and professional verification of users securely. It strictly follows this architectural hierarchy: **User &rarr; Application &rarr; Assets &rarr; Stored Files**.

**File Upload Flow**:
1. The client uploads physical documents (e.g., images, PDFs) directly to a third-party storage service (e.g., AWS S3, Azure Blob, or Cloudinary) via a generic file storage endpoint.
2. The file storage service securely uploads the file to the third-party provider, creates a record in the `StoredFiles` table containing the external file URL, and returns a `StoredFileId`.
3. The client then submits a `Request Verification` payload. This payload does NOT contain binary file data; instead, it contains the `StoredFileId` for each required `AssetType`.
4. The system validates that the `StoredFileId`s exist, creates a new `VerificationApplications` record linked to the `User`, and generates `VerificationAssets` linking the application to the respective stored files.

### Mermaid Entity-Relationship (ER) Diagram
```mermaid
erDiagram
    USERS ||--o{ VERIFICATION_APPLICATIONS : submits
    VERIFICATION_APPLICATIONS ||--|{ VERIFICATION_ASSETS : contains
    VERIFICATION_ASSETS }|--|| STORED_FILES : references

    USERS {
        Guid Id PK
        string Name
        string NationalNumber
    }

    VERIFICATION_APPLICATIONS {
        Guid Id PK
        Guid UserId FK
        int Status
        string Comments
        DateTime CreatedAt
    }

    VERIFICATION_ASSETS {
        Guid Id PK
        Guid ApplicationId FK
        Guid StoredFileId FK
        int AssetType
    }

    STORED_FILES {
        Guid Id PK
        string FileName
        string ContentType
        string FileUrl
        long FileSizeBytes
    }
```

### Mermaid Sequence Diagram
```mermaid
sequenceDiagram
    actor Client
    participant FileAPI as File Upload API
    participant VerifyAPI as Verification API
    participant DB as Database

    Client->>FileAPI: 1. Upload Asset File (Binary)
    FileAPI->>DB: 2. Insert into StoredFiles
    DB-->>FileAPI: 3. Return StoredFileId
    FileAPI-->>Client: 4. Return StoredFileId
    Client->>VerifyAPI: 5. POST /api/v1/verifications (AssetTypes + StoredFileIds)
    VerifyAPI->>DB: 6. Validate StoredFileIds
    VerifyAPI->>DB: 7. Create VerificationApplication
    VerifyAPI->>DB: 8. Create VerificationAssets
    DB-->>VerifyAPI: 9. Success
    VerifyAPI-->>Client: 10. Application Submitted (Status: Pending)
```

### Related Tables
1. **`VerificationApplications`**: Represents a single verification attempt by a User. Contains `Status` (Pending, Approved, Rejected) and admin `Comments`.
2. **`VerificationAssets`**: Represents a specific document requirement (e.g., "National ID Front") linked to an Application.
3. **`StoredFiles`**: Stores the metadata and the external URL (from the third-party storage service) referencing the physical file.

---

### 5.1 Request Verification

### API Description
Submits a new verification application containing the required assets for a user.

### Endpoint
`/api/v1/verifications`

### HTTP Method
`POST`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |
| Content-Type | application/json | Yes | Must be application/json. |

### Path Parameters
None.

### Query Parameters
None.

### Request Body
| Field | Type | Required | Description |
|---|---|---|---|
| Assets | List&lt;AssetDto&gt; | Yes | List of assets required for verification. |

*AssetDto Details*
| Field | Type | Required | Description |
|---|---|---|---|
| AssetType | Int/Enum | Yes | The type of asset (1=NationalIdFront, 2=NationalIdBack, 3=BarAssociationCertificate). |
| StoredFileId | Guid | Yes | The ID of the previously uploaded file. |

### Field table
*(See Request Body table)*

### Validation Rules
- The user must not already have a `Pending` verification application.
- `Assets` list must not be empty.
- Depending on the user's Role (e.g., Lawyer), specific `AssetType`s are mandatory (e.g., `NationalIdFront`, `NationalIdBack`, `BarAssociationCertificate`).
- `StoredFileId` must be a valid Guid.

### Business Logic
- Extract the authenticated `UserId` from the token.
- Validate that all provided `StoredFileId`s exist in the `StoredFiles` table.
- Create a new `VerificationApplication` for the user with Status = `Pending`.
- Create `VerificationAsset` records mapping the provided `StoredFileId`s and `AssetType`s to the new application.
- Save changes atomically within a database transaction.

### Database Operations
- **SELECT**: Check if the user has an existing `Pending` application. Validate `StoredFiles`.
- **INSERT**: `VerificationApplications`, `VerificationAssets`.

### Uploaded Files
- Requires files to be uploaded **prior** to this API call. Only `StoredFileId`s are transmitted here.

### Example Request
```http
POST /api/v1/verifications HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
Content-Type: application/json

{
  "assets": [
    {
      "assetType": 1, 
      "storedFileId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    },
    {
      "assetType": 2, 
      "storedFileId": "b2c3d4e5-f6a7-8901-bcde-f01234567891"
    },
    {
      "assetType": 3, 
      "storedFileId": "c3d4e5f6-a7b8-9012-cdef-012345678912"
    }
  ]
}
```

### Example Response
```json
{
  "success": true,
  "data": {
    "applicationId": "9f8e7d6c-5b4a-3210-fedc-ba0987654321",
    "status": "Pending",
    "message": "Verification request submitted successfully."
  }
}
```

### Error Responses
- **400 Bad Request**: Validation failed (e.g., missing required assets, invalid StoredFileId).
- **409 Conflict**: User already has a pending verification application.

---

### 5.2 Get Verification Status

### API Description
Retrieves the current status of the authenticated user's verification application.

### Endpoint
`/api/v1/verifications/status`

### HTTP Method
`GET`

### Authentication
**Bearer Token (JWT)** - Required.

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |

### Path Parameters
None.

### Query Parameters
None.

### Request Body
None.

### Field table
*Response Fields*

| Field | Type | Required | Description |
|---|---|---|---|
| ApplicationId | Guid | Yes | The ID of the application. |
| Status | String | Yes | Current status (Pending, Approved, Rejected). |
| Comments | String | No | Admin comments (useful if rejected). |
| CreatedAt | DateTime | Yes | Submission date. |

### Validation Rules
- The user must be authenticated.

### Business Logic
- Extract the user ID from the token.
- Fetch the latest Verification Application for that user.

### Database Operations
- **SELECT**: Query `VerificationApplications` for the `UserId`.

### Related Tables
- `VerificationApplications`

### Uploaded Files
- **N/A**

### Example Request
```http
GET /api/v1/verifications/status HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
```

### Example Response
```json
{
  "success": true,
  "data": {
    "applicationId": "9f8e7d6c-5b4a-3210-fedc-ba0987654321",
    "status": "Pending",
    "comments": null,
    "createdAt": "2023-10-01T12:00:00Z"
  }
}
```

### Error Responses
- **401 Unauthorized**: Missing or invalid token.
- **404 Not Found**: No verification application found for the user.

### Notes
- Uses `ApiResponse<T>.Ok()`.

---

### 5.3 Get Verification Details

### API Description
Retrieves the full details of a specific verification application, including asset links. Designed primarily for Admin usage.

### Endpoint
`/api/v1/verifications/{id}`

### HTTP Method
`GET`

### Authentication
**Bearer Token (JWT)** - Required (Admin Role).

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The ID of the verification application. |

### Query Parameters
None.

### Request Body
None.

### Field table
*Response Fields*

| Field | Type | Required | Description |
|---|---|---|---|
| ApplicationId | Guid | Yes | ID of the application. |
| UserId | Guid | Yes | ID of the applicant. |
| Status | String | Yes | Current status. |
| Assets | List&lt;AssetResponse&gt; | Yes | List of submitted assets. |

*AssetResponse Fields*
| Field | Type | Required | Description |
|---|---|---|---|
| AssetType | String | Yes | Type of the asset. |
| FileUrl | String | Yes | Secure URL to download/view the stored file. |

### Validation Rules
- `id` must be a valid Guid.

### Business Logic
- Retrieve the Verification Application by ID.
- Retrieve the related `VerificationAssets` and join with `StoredFiles` to resolve the `FileUrl`.
- Return the full application payload to the admin.

### Database Operations
- **SELECT**: Query `VerificationApplications` joined with `VerificationAssets` and `StoredFiles`.

### Related Tables
- `VerificationApplications`, `VerificationAssets`, `StoredFiles`

### Uploaded Files
- Includes the `FileUrl` resolved from the associated `StoredFiles` record so admins can view the document.

### Example Request
```http
GET /api/v1/verifications/9f8e7d6c-5b4a-3210-fedc-ba0987654321 HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
```

### Example Response
```json
{
  "success": true,
  "data": {
    "applicationId": "9f8e7d6c-5b4a-3210-fedc-ba0987654321",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Pending",
    "assets": [
      {
        "assetType": "NationalIdFront",
        "fileUrl": "https://storage.smartcourt.com/files/a1b2c3d4-e5f6.jpg"
      }
    ]
  }
}
```

### Error Responses
- **401 Unauthorized**: Missing or invalid token.
- **403 Forbidden**: Not an admin.
- **404 Not Found**: Application not found.

### Notes
- The FileUrl must be generated securely (e.g., Pre-Signed URL) if using cloud storage.

---

### 5.4 Approve Verification

### API Description
Approves a pending verification application. Updates the Application status and the underlying User profile status.

### Endpoint
`/api/v1/verifications/{id}/approve`

### HTTP Method
`POST`

### Authentication
**Bearer Token (JWT)** - Required (Admin Role).

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The ID of the verification application. |

### Query Parameters
None.

### Request Body
None.

### Field table
*Response Fields*

| Field | Type | Required | Description |
|---|---|---|---|
| ApplicationId | Guid | Yes | ID of the application. |
| Status | String | Yes | Updated status. |

### Validation Rules
- `id` must be a valid Guid.
- Application must be in `Pending` state.

### Business Logic
- Verify application is in `Pending` state.
- Update `VerificationApplications` Status to `Approved`.
- Update the related `User` profile status to `Verified`.
- Save changes atomically.

### Database Operations
- **SELECT**: Verify Application status.
- **UPDATE**: Modify `VerificationApplications` and `Users` status.

### Related Tables
- `VerificationApplications`, `Users`

### Uploaded Files
- **N/A**

### Example Request
```http
POST /api/v1/verifications/9f8e7d6c-5b4a-3210-fedc-ba0987654321/approve HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
```

### Example Response
```json
{
  "success": true,
  "data": {
    "applicationId": "9f8e7d6c-5b4a-3210-fedc-ba0987654321",
    "status": "Approved"
  }
}
```

### Error Responses
- **400 Bad Request**: Application is already approved/rejected.
- **404 Not Found**: Application not found.

### Notes
- Database operations must be wrapped in a transaction to ensure both user and application statuses are updated together.

---

### 5.5 Reject Verification

### API Description
Rejects a pending verification application and optionally provides a comment explaining why.

### Endpoint
`/api/v1/verifications/{id}/reject`

### HTTP Method
`POST`

### Authentication
**Bearer Token (JWT)** - Required (Admin Role).

### Request Headers
| Header | Value | Required | Description |
|---|---|---|---|
| Authorization | Bearer `<token>` | Yes | JWT Access token. |
| Content-Type | application/json | Yes | Must be application/json. |

### Path Parameters
| Parameter | Type | Required | Description |
|---|---|---|---|
| id | Guid | Yes | The ID of the verification application. |

### Query Parameters
None.

### Request Body
| Field | Type | Required | Description |
|---|---|---|---|
| Comments | String | Yes | Reason for rejection. |

### Field table
*(See Request Body table)*

### Validation Rules
- `id` must be a valid Guid.
- Application must be in `Pending` state.
- `Comments` must not be empty.

### Business Logic
- Verify application is in `Pending` state.
- Update `VerificationApplications` Status to `Rejected`.
- Append `Comments` explaining the rejection.
- Save changes.

### Database Operations
- **SELECT**: Verify Application status.
- **UPDATE**: Modify `VerificationApplications` status and comments.

### Related Tables
- `VerificationApplications`

### Uploaded Files
- **N/A**

### Example Request
```http
POST /api/v1/verifications/9f8e7d6c-5b4a-3210-fedc-ba0987654321/reject HTTP/1.1
Host: api.smartcourt.com
Authorization: Bearer eyJhbGci...
Content-Type: application/json

{
  "comments": "The uploaded National ID is illegible. Please re-upload a clear copy."
}
```

### Example Response
```json
{
  "success": true,
  "data": {
    "applicationId": "9f8e7d6c-5b4a-3210-fedc-ba0987654321",
    "status": "Rejected"
  }
}
```

### Error Responses
- **400 Bad Request**: Application is already approved/rejected, or missing comments.
- **404 Not Found**: Application not found.

### Notes
- Uses `ApiResponse<T>.Ok()`.
