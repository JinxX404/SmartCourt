# Articles Feature Test Report

Run at: 2026-08-15 15:27:17

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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3Yjg4YjFmOC04ZTc1LTRjODEtODE0Zi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdiODhiMWY4LThlNzUtNGM4MS04MTRmLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IjRDUVNJQVJOWU9aN1VVTjVMRVU1TUlONzdOTUxQVDc1IiwianRpIjoiY2EzODQ1ZGUtMGQwYS00N2UwLTk3M2UtOGRhMjQ5OGZiZGU4IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODY3OTY4NDEsImV4cCI6MTc4NjgwMDQ0MSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.vmtN3tAqX-PInpMz1jouXXBPKULcEWpVMFVzm9FPHU4",
                 "expiresIn":  3600,
                 "refreshToken":  "QQhV/Jbxi46cstamiUSu3iDT9UsYoj8NVsvxpPGi30pT6WyxomTeGc8Qc1S2AvtaE/caaNXER0uDH0U5FNPOaw==",
                 "refreshTokenExpiration":  "2026-08-22T12:27:21.3013709Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjZGVhNDk5MS05NzMzLTQ0MDAtODE1MS0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImNkZWE0OTkxLTk3MzMtNDQwMC04MTUxLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoibGF3eWVyQHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiIyV0kzRk83TlNOUlNOSktXVkpRQ001RzQ1Q1JWV0tISyIsImp0aSI6IjcwYTQ4ZjI4LTYwZTQtNDQ3ZC04Njk2LTdmNTMzNGQ2M2EzZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4Njc5Njg0MSwiZXhwIjoxNzg2ODAwNDQxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.mqWZHS47D_uh31sS26kWrdDJlmMHJVLp_zp4Jq95oJ0",
                 "expiresIn":  3600,
                 "refreshToken":  "SuPtgMdaS5NbraiNIk8X//zf192x6kuwA/lvFpDMGAOmmzDAfO3914N+FvYIoQ0p5Y4bUbfycoKREPs70TBKkw==",
                 "refreshTokenExpiration":  "2026-08-22T12:27:21.6203683Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NDEzNjdmOS00OGYzLTRlYWEtODE1Mi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY0MTM2N2Y5LTQ4ZjMtNGVhYS04MTUyLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiY2xpZW50QHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJTN0FaNk1BRk1SUEZWU0RZUjNXN0k2TEhESEFLVVBIMyIsImp0aSI6IjVhYzQyMTkzLWFhMTMtNGFhYi1hYzMwLThmM2Q3OGM4MDY4NSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4Njc5Njg0MSwiZXhwIjoxNzg2ODAwNDQxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.D_9XFdoEnx_gK6H4_a4VtasgE1aDCCS947Y_5XyqxMY",
                 "expiresIn":  3600,
                 "refreshToken":  "LYWCqS4wXjCcEjH85xaKi8shATKNU0IxeLPg1elV5FMizsFYkRrrUKIRMOL6IF6AUMU5gIl/3Tsb9QdDfAq32g==",
                 "refreshTokenExpiration":  "2026-08-22T12:27:21.7629988Z"
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
    "Code":  "ARTCAT_900333675",
    "NameAr":  "Test Category 900333675",
    "Description":  "Category Description"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "67dc4b58-935a-47cb-d669-08defac88e34",
                 "code":  "ARTCAT_900333675",
                 "nameAr":  "Test Category 900333675",
                 "description":  "Category Description"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 2b. Update Category (Admin)

**Request:** PUT http://localhost:5049/api/ArticleCategories/admin/67dc4b58-935a-47cb-d669-08defac88e34

**Body:**
`json
{
    "NameAr":  "Updated Category 900333675",
    "Description":  "Updated Description"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "67dc4b58-935a-47cb-d669-08defac88e34",
                 "code":  "ARTCAT_900333675",
                 "nameAr":  "Updated Category 900333675",
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
                     "id":  "67dc4b58-935a-47cb-d669-08defac88e34",
                     "code":  "ARTCAT_900333675",
                     "nameAr":  "Updated Category 900333675",
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
System.Collections.Hashtable

**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "630fe156-38ae-4e4f-53d2-08defac88e60",
                 "title":  "Test Article 900333675",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  1,
                 "categoryId":  "67dc4b58-935a-47cb-d669-08defac88e34",
                 "category":  {
                                  "id":  "67dc4b58-935a-47cb-d669-08defac88e34",
                                  "code":  "ARTCAT_900333675",
                                  "nameAr":  "Updated Category 900333675",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-15T12:27:22.3120365Z",
                 "updatedAt":  "2026-08-15T12:27:22.2229969Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 3b. Publish Article via Status Change

**Request:** PUT http://localhost:5049/api/Articles/lawyer/630fe156-38ae-4e4f-53d2-08defac88e60/status

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "630fe156-38ae-4e4f-53d2-08defac88e60",
                 "title":  "Test Article 900333675",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  2,
                 "categoryId":  "67dc4b58-935a-47cb-d669-08defac88e34",
                 "category":  {
                                  "id":  "67dc4b58-935a-47cb-d669-08defac88e34",
                                  "code":  "ARTCAT_900333675",
                                  "nameAr":  "Updated Category 900333675",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-15T12:27:22.3120365",
                 "updatedAt":  "2026-08-15T12:27:22.5776892Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4.1a Create Comment - Empty string (Expected 400)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/comments

**Body:**
`json
{
    "Content":  ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Content":  [
                                   "محتوى التعليق مطلوب."
                               ]
               },
    "traceId":  "00-d712050c0a05a1ec4985e36e2286a8f9-9d70b028fe567b23-00"
}
``n---


### 4.1b Create Comment - Over 1000 chars (Expected 400)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/comments

**Body:**
`json
{
    "Content":  "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Content":  [
                                   "التعليق يجب ألا يتجاوز 1000 حرف."
                               ]
               },
    "traceId":  "00-e9c94f137261b4bd2a576d7648b3c741-acef526a4e0326dd-00"
}
``n---


### 4.1c Create Comment - XSS attempt

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/comments

**Body:**
`json
{
    "Content":  "\u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f02c614f-1ffa-40b4-bf79-e3e17ac1eef0",
                 "articleId":  "630fe156-38ae-4e4f-53d2-08defac88e60",
                 "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                 "userName":  "Test Client",
                 "content":  "\u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e",
                 "createdAt":  "2026-08-15T12:27:22.7561477Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 4.2a Update Comment - Empty string (Expected 400)

**Request:** PUT http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/comments/f02c614f-1ffa-40b4-bf79-e3e17ac1eef0

**Body:**
`json
{
    "Content":  ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Content":  [
                                   "محتوى التعليق مطلوب."
                               ]
               },
    "traceId":  "00-6b2cbd41e2466ed8df920cadfeb79fdc-f08d590c155b7b4c-00"
}
``n---


### 4.3a Report Article - Empty Reason (Expected 400)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/report

**Body:**
`json
{
    "Reason":  ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Reason":  [
                                  "سبب البلاغ مطلوب."
                              ]
               },
    "traceId":  "00-de0b13ce28e19f37186d90dc1ebb93ab-82675e5255202f18-00"
}
``n---


### 4.3b Report Article - Valid (Client)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/report

**Body:**
`json
{
    "Reason":  "Inappropriate content, please review."
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


### 4.3c Report Article - Duplicate Report (Expected 400)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/report

**Body:**
`json
{
    "Reason":  "Inappropriate content, please review."
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "لقد قمت بالإبلاغ عن هذا المقال مسبقاً.",
    "errors":  null,
    "statusCode":  400
}
``n---


### 4.3d Report Article - Author Self-Report (Expected 400)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/report

**Body:**
`json
{
    "Reason":  "Inappropriate content, please review."
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "لا يمكنك الإبلاغ عن مقالك الخاص.",
    "errors":  null,
    "statusCode":  400
}
``n---


### 4.4a Like Article (Client)

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/like

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


### 4.4b Unlike Article (Client) - Should not go below 0

**Request:** POST http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/like

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  false,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4.4c View Article Likers (Client)

**Request:** GET http://localhost:5049/api/Articles/630fe156-38ae-4e4f-53d2-08defac88e60/likers?pageNumber=1&pageSize=10

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  0,
    "totalRecords":  0,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [

             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5a. Admin Delete Article

**Request:** DELETE http://localhost:5049/api/Articles/admin/630fe156-38ae-4e4f-53d2-08defac88e60

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


### 5b. Update Admin-Deleted Article (Lawyer) (Expected 404)

**Request:** PUT http://localhost:5049/api/Articles/lawyer/630fe156-38ae-4e4f-53d2-08defac88e60

**Body:**
System.Collections.Hashtable

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Title":  [
                                 "عنوان المقال مطلوب."
                             ],
                   "Content":  [
                                   "محتوى المقال مطلوب عند النشر."
                               ],
                   "CategoryId":  [
                                      "التصنيف مطلوب."
                                  ]
               },
    "traceId":  "00-76f0598cc05904920cfb5372adc7be3e-61e1f18bc747c43e-00"
}
``n---


