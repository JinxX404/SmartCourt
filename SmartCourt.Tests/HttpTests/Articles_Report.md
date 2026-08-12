# Articles Feature Test Report

Run at: 2026-08-12 18:52:43

### 1a. Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Admin@123",
    "Email":  "admin@smartcourt.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "7b88b1f8-8e75-4c81-814f-08dee91d7c8f",
                              "email":  "admin@smartcourt.com",
                              "fullName":  "System Administrator",
                              "role":  "Admin",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3Yjg4YjFmOC04ZTc1LTRjODEtODE0Zi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdiODhiMWY4LThlNzUtNGM4MS04MTRmLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IjRDUVNJQVJOWU9aN1VVTjVMRVU1TUlONzdOTUxQVDc1IiwianRpIjoiYTQwZmI3ODUtMGFjZi00YzliLTkyNjEtNmRlM2UxMTM0NDQxIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODY1NDk5NjQsImV4cCI6MTc4NjU1MzU2NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.29cSxeaty0dMCW7I1qo1MtflHBq7sCByM4JNaliTRPk",
                 "expiresIn":  3600,
                 "refreshToken":  "1ul2P+d5K2KE/K52kgZoooR0qv8dkmPNyWXyfA4uNIshYL32lS3/3oBK6nRCsoG45NgSgob/Ha5kwc0TKXgBgQ==",
                 "refreshTokenExpiration":  "2026-08-19T15:52:44.3483616Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1b. Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Lawyer@123",
    "Email":  "lawyer@smartcourt.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                              "email":  "lawyer@smartcourt.com",
                              "fullName":  "Test Lawyer",
                              "role":  "Lawyer",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjZGVhNDk5MS05NzMzLTQ0MDAtODE1MS0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImNkZWE0OTkxLTk3MzMtNDQwMC04MTUxLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoibGF3eWVyQHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiIyV0kzRk83TlNOUlNOSktXVkpRQ001RzQ1Q1JWV0tISyIsImp0aSI6ImQwMGJiYTFkLWY1NDUtNGI1Ni1iNmI3LTE1MDViNWY1Nzc5MCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjU0OTk2NCwiZXhwIjoxNzg2NTUzNTY0LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.6P2DrszvHI85y3Qt5iVog0Tuaue_DE1XeCoKT1TtVJk",
                 "expiresIn":  3600,
                 "refreshToken":  "S4bbpnZjnoTn8Uj2Gm+eSxokECUBcW2tz3qm0qw935xfVsdg0fbhUfNsuDLCp/IJ5JQmIioGph6gPxyZzvXtIQ==",
                 "refreshTokenExpiration":  "2026-08-19T15:52:44.7481736Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1c. Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Client@123",
    "Email":  "client@smartcourt.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                              "email":  "client@smartcourt.com",
                              "fullName":  "Test Client",
                              "role":  "Client",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NDEzNjdmOS00OGYzLTRlYWEtODE1Mi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY0MTM2N2Y5LTQ4ZjMtNGVhYS04MTUyLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiY2xpZW50QHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJTN0FaNk1BRk1SUEZWU0RZUjNXN0k2TEhESEFLVVBIMyIsImp0aSI6IjUyZjYxNDEyLTA0ZjMtNDMyZC1iZTU2LWQ4MDRkNDg5YmIwZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjU0OTk2NSwiZXhwIjoxNzg2NTUzNTY1LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.34WdZ7KRyGC89e6iDuJpoBDR7LfWVRILRT2RmgS5uD4",
                 "expiresIn":  3600,
                 "refreshToken":  "JgGBE7UjIqzA4ediwCz+4hidD7iamwHNKGlclK2Zvu4gDOZ9aAq/F63ORfv4d1PvlgFzg5optG0BiSyXIvsn/Q==",
                 "refreshTokenExpiration":  "2026-08-19T15:52:45.0168706Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2a. Create Category (Admin)

**Request:** POST http://localhost:5049/api/ArticleCategories/admin

**Body:**
`json
{
    "Code":  "ARTCAT_819149806",
    "NameAr":  "Test Category 819149806",
    "Description":  "Category Description"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "code":  "ARTCAT_819149806",
                 "nameAr":  "Test Category 819149806",
                 "description":  "Category Description"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 2b. Update Category (Admin)

**Request:** PUT http://localhost:5049/api/ArticleCategories/admin/e698bf27-35f4-4f0a-307b-08def889c03b

**Body:**
`json
{
    "NameAr":  "Updated Category 819149806",
    "Description":  "Updated Description"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "code":  "ARTCAT_819149806",
                 "nameAr":  "Updated Category 819149806",
                 "description":  "Updated Description"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2c. Public View Categories

**Request:** GET http://localhost:5049/api/ArticleCategories/public

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  [
                 {
                     "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                     "code":  "ARTCAT_819149806",
                     "nameAr":  "Updated Category 819149806",
                     "description":  "Updated Description"
                 },
                 {
                     "id":  "d3b711e7-f1e1-450a-9d9f-3d12c5b96901",
                     "code":  "commercial",
                     "nameAr":  "القانون التجاري",
                     "description":  null
                 },
                 {
                     "id":  "a0b711e7-f1e1-450a-9d9f-3d12c5b96904",
                     "code":  "criminal",
                     "nameAr":  "القانون الجنائي",
                     "description":  null
                 },
                 {
                     "id":  "c2b711e7-f1e1-450a-9d9f-3d12c5b96902",
                     "code":  "civil",
                     "nameAr":  "القانون المدني",
                     "description":  null
                 },
                 {
                     "id":  "b1b711e7-f1e1-450a-9d9f-3d12c5b96903",
                     "code":  "labor",
                     "nameAr":  "نظام العمل",
                     "description":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3a. Create Draft Article (Lawyer)

**Request:** POST http://localhost:5049/api/Articles/lawyer

**Body:**
`json
{
    "Title":  "Test Article 819149806",
    "Tags":  "Law,Test",
    "CategoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
    "Content":  "Test Article Content long...",
    "IsDraft":  true
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "title":  "Test Article 819149806",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  1,
                 "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "category":  {
                                  "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                                  "code":  "ARTCAT_819149806",
                                  "nameAr":  "Updated Category 819149806",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-12T15:52:45.6540335Z",
                 "updatedAt":  "2026-08-12T15:52:45.5808575Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 3b. View Drafts (Lawyer)

**Request:** GET http://localhost:5049/api/Articles/lawyer/drafts

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  1,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "title":  "Test Article 819149806",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  1,
                     "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                     "categoryNameAr":  "Updated Category 819149806",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:52:45.6540335"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3c. Update Article (Lawyer)

**Request:** PUT http://localhost:5049/api/Articles/lawyer/f57c4b21-791e-4005-b59c-08def889c06e

**Body:**
`json
{
    "Title":  "Updated Article 819149806",
    "Tags":  "Law,Test",
    "CategoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
    "Content":  "Updated Article Content...",
    "IsDraft":  true
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "title":  "Updated Article 819149806",
                 "content":  "Updated Article Content...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  1,
                 "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "category":  {
                                  "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                                  "code":  "ARTCAT_819149806",
                                  "nameAr":  "Updated Category 819149806",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-12T15:52:45.6540335",
                 "updatedAt":  "2026-08-12T15:52:45.920353Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3d. Publish Article via Status Change

**Request:** PUT http://localhost:5049/api/Articles/lawyer/f57c4b21-791e-4005-b59c-08def889c06e/status

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "title":  "Updated Article 819149806",
                 "content":  "Updated Article Content...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  2,
                 "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "category":  {
                                  "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                                  "code":  "ARTCAT_819149806",
                                  "nameAr":  "Updated Category 819149806",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-12T15:52:45.6540335",
                 "updatedAt":  "2026-08-12T15:52:45.9689788Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3e. View Published (Lawyer)

**Request:** GET http://localhost:5049/api/Articles/lawyer/published

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  1,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "title":  "Updated Article 819149806",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                     "categoryNameAr":  "Updated Category 819149806",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:52:45.6540335"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4a. View All Published (Public)

**Request:** GET http://localhost:5049/api/Articles/public

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  2,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "title":  "Updated Article 819149806",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                     "categoryNameAr":  "Updated Category 819149806",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:52:45.6540335"
                 },
                 {
                     "id":  "ed4c3439-8b97-44db-a5ab-08def865ac1c",
                     "title":  "لماذا يعد التعدي على شغل الباك مخالف للقانون؟",
                     "featuredImageUrl":  "",
                     "viewCount":  1,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "d3b711e7-f1e1-450a-9d9f-3d12c5b96901",
                     "categoryNameAr":  "القانون التجاري",
                     "authorId":  "a8e576cb-9d0f-4f36-5288-08def86410ad",
                     "authorName":  "mahmoud",
                     "createdAt":  "2026-08-12T11:34:29.6563252"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4b. View Article (Client Token)

**Request:** GET http://localhost:5049/api/Articles/public/f57c4b21-791e-4005-b59c-08def889c06e

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "title":  "Updated Article 819149806",
                 "content":  "Updated Article Content...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  1,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  2,
                 "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "category":  {
                                  "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                                  "code":  "ARTCAT_819149806",
                                  "nameAr":  "Updated Category 819149806",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-12T15:52:45.6540335",
                 "updatedAt":  "2026-08-12T15:52:46.2508372Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4c. Like Article (Client)

**Request:** POST http://localhost:5049/api/Articles/f57c4b21-791e-4005-b59c-08def889c06e/like

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4d. Comment on Article (Client)

**Request:** POST http://localhost:5049/api/Articles/f57c4b21-791e-4005-b59c-08def889c06e/comments

**Body:**
`json
{
    "Content":  "Great Article!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "1091a300-3781-4417-8a59-7949fc38e3c4",
                 "articleId":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                 "userName":  "Test Client",
                 "content":  "Great Article!",
                 "createdAt":  "2026-08-12T15:52:46.4360898Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 4e. Update Comment (Client)

**Request:** PUT http://localhost:5049/api/Articles/f57c4b21-791e-4005-b59c-08def889c06e/comments/1091a300-3781-4417-8a59-7949fc38e3c4

**Body:**
`json
{
    "Content":  "Great Article! Updated."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "1091a300-3781-4417-8a59-7949fc38e3c4",
                 "articleId":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                 "userName":  "Test Client",
                 "content":  "Great Article! Updated.",
                 "createdAt":  "2026-08-12T15:52:46.4360898"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4f. View Article Comments Paginated (Public)

**Request:** GET http://localhost:5049/api/Articles/public/f57c4b21-791e-4005-b59c-08def889c06e/comments?pageNumber=1&pageSize=10

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  1,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "1091a300-3781-4417-8a59-7949fc38e3c4",
                     "articleId":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                     "userName":  "Test Client",
                     "content":  "Great Article! Updated.",
                     "createdAt":  "2026-08-12T15:52:46.4360898"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4g. Report Article (Client)

**Request:** POST http://localhost:5049/api/Articles/f57c4b21-791e-4005-b59c-08def889c06e/report

**Body:**
`json
{
    "Reason":  "Inappropriate content"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4i. View Article Check IsLiked (Client Token)

**Request:** GET http://localhost:5049/api/Articles/public/f57c4b21-791e-4005-b59c-08def889c06e

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                 "title":  "Updated Article 819149806",
                 "content":  "Updated Article Content...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  1,
                 "likesCount":  1,
                 "commentsCount":  1,
                 "isLikedByCurrentUser":  true,
                 "status":  2,
                 "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                 "category":  {
                                  "id":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                                  "code":  "ARTCAT_819149806",
                                  "nameAr":  "Updated Category 819149806",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-12T15:52:45.6540335",
                 "updatedAt":  "2026-08-12T15:52:46.4567827"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5a. View Reported Articles (Admin)

**Request:** GET http://localhost:5049/api/Articles/admin/reported

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  1,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "19138d2f-fbcf-4fa8-831a-55f173260ba4",
                     "articleId":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "articleTitle":  "Updated Article 819149806",
                     "reporterId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                     "reporterName":  "Test Client",
                     "reason":  "Inappropriate content",
                     "createdAt":  "2026-08-12T15:52:46.7403342",
                     "isResolved":  false
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5b. Resolve Report (Admin)

**Request:** PUT http://localhost:5049/api/Articles/admin/reports/19138d2f-fbcf-4fa8-831a-55f173260ba4/resolve

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5c. Admin Delete Article

**Request:** DELETE http://localhost:5049/api/Articles/admin/f57c4b21-791e-4005-b59c-08def889c06e

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5d. View Admin Deleted Articles

**Request:** GET http://localhost:5049/api/Articles/admin/deleted-by-admin

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  4,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "f57c4b21-791e-4005-b59c-08def889c06e",
                     "title":  "Updated Article 819149806",
                     "featuredImageUrl":  null,
                     "viewCount":  1,
                     "likesCount":  1,
                     "commentsCount":  1,
                     "status":  2,
                     "categoryId":  "e698bf27-35f4-4f0a-307b-08def889c03b",
                     "categoryNameAr":  "Updated Category 819149806",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:52:45.6540335"
                 },
                 {
                     "id":  "05d9fdd8-875e-447f-a402-08def887de9d",
                     "title":  "Updated Article 215426780",
                     "featuredImageUrl":  null,
                     "viewCount":  1,
                     "likesCount":  1,
                     "commentsCount":  1,
                     "status":  2,
                     "categoryId":  "6003ec15-acac-4b94-fd18-08def887de55",
                     "categoryNameAr":  "Updated Category 215426780",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:40:30.7049813"
                 },
                 {
                     "id":  "67cf6095-3fa8-4feb-a401-08def887de9d",
                     "title":  "Updated Article 1208895854",
                     "featuredImageUrl":  null,
                     "viewCount":  1,
                     "likesCount":  1,
                     "commentsCount":  1,
                     "status":  2,
                     "categoryId":  "99488f09-e39a-4071-fd17-08def887de55",
                     "categoryNameAr":  "Updated Category 1208895854",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T15:39:17.2806655"
                 },
                 {
                     "id":  "9bd22c3c-1f17-4aa8-9699-08def87fa75d",
                     "title":  "Updated Article 1663017242",
                     "featuredImageUrl":  null,
                     "viewCount":  1,
                     "likesCount":  1,
                     "commentsCount":  1,
                     "status":  2,
                     "categoryId":  "b0fac78b-f62e-47af-bf41-08def87fa70b",
                     "categoryNameAr":  "Updated Category 1663017242",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-12T14:40:28.6204349"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6a. Delete Category (Admin)

**Request:** DELETE http://localhost:5049/api/ArticleCategories/admin/e698bf27-35f4-4f0a-307b-08def889c03b

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


