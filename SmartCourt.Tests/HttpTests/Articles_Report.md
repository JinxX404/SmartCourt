# Articles Feature Test Report

Run at: 2026-08-14 23:17:51

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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3Yjg4YjFmOC04ZTc1LTRjODEtODE0Zi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdiODhiMWY4LThlNzUtNGM4MS04MTRmLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IjRDUVNJQVJOWU9aN1VVTjVMRVU1TUlONzdOTUxQVDc1IiwianRpIjoiNGU1ZTY3ODktY2FlYS00MTg2LWI4OGItZTQwZDFhNWJiMWZiIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODY3Mzg2NzIsImV4cCI6MTc4Njc0MjI3MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.AmuXXiu2mwa7TR7bs63jw2BZ9CFbD2YUxsL-oamvLNY",
                 "expiresIn":  3600,
                 "refreshToken":  "d/QgHKroy/yaCOMCTliaE9Wg8+jVlp4EfWKfRwHjK/v2sfRGhZpFji2+db4LuVol+jAau5CP3wvAL0sDYqDsZQ==",
                 "refreshTokenExpiration":  "2026-08-21T20:17:52.0055619Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjZGVhNDk5MS05NzMzLTQ0MDAtODE1MS0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImNkZWE0OTkxLTk3MzMtNDQwMC04MTUxLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoibGF3eWVyQHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiIyV0kzRk83TlNOUlNOSktXVkpRQ001RzQ1Q1JWV0tISyIsImp0aSI6IjRjNTU4YjA0LWVmZjAtNGY3ZC1hODYyLTRhN2NhZDdkOTI1OCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjczODY3MiwiZXhwIjoxNzg2NzQyMjcyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.IkHxXx7LCtazAzhAPBqwjDvGiH63IL250I5E1HZxJ7o",
                 "expiresIn":  3600,
                 "refreshToken":  "P13tC1Jzm30+ScoSCCul2jCu41J2ugIM1sfG6JMg1ochdD1UM7vlGmm3ZQ+kDthnlaFebZm1ZjwZVqgn3/YMxw==",
                 "refreshTokenExpiration":  "2026-08-21T20:17:52.1898074Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NDEzNjdmOS00OGYzLTRlYWEtODE1Mi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY0MTM2N2Y5LTQ4ZjMtNGVhYS04MTUyLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiY2xpZW50QHNtYXJ0Y291cnQuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJTN0FaNk1BRk1SUEZWU0RZUjNXN0k2TEhESEFLVVBIMyIsImp0aSI6IjAwZmRiYzAwLWYzMWUtNDdlMS05ZTY0LTI2YTA1N2E2Y2ZmZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjczODY3MiwiZXhwIjoxNzg2NzQyMjcyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.DWTtfQ2yX_TZ18fn4nHnLLlnc5RSH0dw2-Fno7CukXk",
                 "expiresIn":  3600,
                 "refreshToken":  "UI+C0pdSGR1w8wgoel9VQsqyGfOCxV4mESk705u3MGP8juBs1sQjj2VcbMdh3GE/Xtdbqv1rQ90JMHY0qjYRAw==",
                 "refreshTokenExpiration":  "2026-08-21T20:17:52.3306476Z"
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
    "Code":  "ARTCAT_648577975",
    "NameAr":  "Test Category 648577975",
    "Description":  "Category Description"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "code":  "ARTCAT_648577975",
                 "nameAr":  "Test Category 648577975",
                 "description":  "Category Description"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 2b. Update Category (Admin)

**Request:** PUT http://localhost:5049/api/ArticleCategories/admin/d7afc08d-63fe-4acd-f1f9-08defa40c741

**Body:**
`json
{
    "NameAr":  "Updated Category 648577975",
    "Description":  "Updated Description"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "code":  "ARTCAT_648577975",
                 "nameAr":  "Updated Category 648577975",
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
                     "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                     "code":  "ARTCAT_648577975",
                     "nameAr":  "Updated Category 648577975",
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
                 "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "title":  "Test Article 648577975",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  1,
                 "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "category":  {
                                  "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                                  "code":  "ARTCAT_648577975",
                                  "nameAr":  "Updated Category 648577975",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-14T20:17:52.6487838Z",
                 "updatedAt":  "2026-08-14T20:17:52.5875082Z"
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
                     "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "title":  "Test Article 648577975",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  1,
                     "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                     "categoryNameAr":  "Updated Category 648577975",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-14T20:17:52.6487838"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3c. Update Article (Lawyer)

**Request:** PUT http://localhost:5049/api/Articles/lawyer/79831ff7-0057-40da-71f4-08defa411e92

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
    "traceId":  "00-dd15a76d8dff0db66a4b3f5391614cb7-23f7fec67da0f2a0-00"
}
``n---


### 3d. Publish Article via Status Change

**Request:** PUT http://localhost:5049/api/Articles/lawyer/79831ff7-0057-40da-71f4-08defa411e92/status

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "title":  "Test Article 648577975",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  0,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  2,
                 "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "category":  {
                                  "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                                  "code":  "ARTCAT_648577975",
                                  "nameAr":  "Updated Category 648577975",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-14T20:17:52.6487838",
                 "updatedAt":  "2026-08-14T20:17:52.9332345Z"
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
                     "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "title":  "Test Article 648577975",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                     "categoryNameAr":  "Updated Category 648577975",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-14T20:17:52.6487838"
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
    "totalRecords":  3,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "title":  "Test Article 648577975",
                     "featuredImageUrl":  null,
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                     "categoryNameAr":  "Updated Category 648577975",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-14T20:17:52.6487838"
                 },
                 {
                     "id":  "cea4c6fe-2431-41cf-c6ea-08defa1a3d26",
                     "title":  "قانون المرافعات",
                     "featuredImageUrl":  "https://msahvjipdwvgdartpeqj.supabase.co/storage/v1/object/sign/smart-court-files/articles/e2cfbfd7-b512-46c6-a7e1-1675495d82fd_IMG-20251204-WA0011.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV9hMWU2MTdkZi01NzZkLTQ3ZTItYWJiYy1iYzI1OThmZDRiNWUiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJzbWFydC1jb3VydC1maWxlcy9hcnRpY2xlcy9lMmNmYmZkNy1iNTEyLTQ2YzYtYTdlMS0xNjc1NDk1ZDgyZmRfSU1HLTIwMjUxMjA0LVdBMDAxMS5qcGciLCJzY29wZSI6ImRvd25sb2FkIiwiaWF0IjoxNzg2NzM4NjczLCJleHAiOjE3ODY3NDIyNzN9.qSurkPh0_3-YCz2a-08EU3sYbMNXYBeVNnGEvbV5pPU",
                     "viewCount":  0,
                     "likesCount":  0,
                     "commentsCount":  0,
                     "status":  2,
                     "categoryId":  "a0b711e7-f1e1-450a-9d9f-3d12c5b96904",
                     "categoryNameAr":  "القانون الجنائي",
                     "authorId":  "a8e576cb-9d0f-4f36-5288-08def86410ad",
                     "authorName":  "mahmoud",
                     "createdAt":  "2026-08-14T15:39:33.6321212"
                 },
                 {
                     "id":  "ed4c3439-8b97-44db-a5ab-08def865ac1c",
                     "title":  "لماذا يعد التعدي على شغل الباك مخالف للقانون؟",
                     "featuredImageUrl":  null,
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

**Request:** GET http://localhost:5049/api/Articles/public/79831ff7-0057-40da-71f4-08defa411e92

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "title":  "Test Article 648577975",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  1,
                 "likesCount":  0,
                 "commentsCount":  0,
                 "isLikedByCurrentUser":  false,
                 "status":  2,
                 "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "category":  {
                                  "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                                  "code":  "ARTCAT_648577975",
                                  "nameAr":  "Updated Category 648577975",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-14T20:17:52.6487838",
                 "updatedAt":  "2026-08-14T20:17:53.4309838Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4c. Like Article (Client)

**Request:** POST http://localhost:5049/api/Articles/79831ff7-0057-40da-71f4-08defa411e92/like

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

**Request:** POST http://localhost:5049/api/Articles/79831ff7-0057-40da-71f4-08defa411e92/comments

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
                 "id":  "d695e1c4-01e1-4498-a2fb-f4ee6b9bd893",
                 "articleId":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                 "userName":  "Test Client",
                 "content":  "Great Article!",
                 "createdAt":  "2026-08-14T20:17:53.5803062Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### 4e. Update Comment (Client)

**Request:** PUT http://localhost:5049/api/Articles/79831ff7-0057-40da-71f4-08defa411e92/comments/d695e1c4-01e1-4498-a2fb-f4ee6b9bd893

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
                 "id":  "d695e1c4-01e1-4498-a2fb-f4ee6b9bd893",
                 "articleId":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                 "userName":  "Test Client",
                 "content":  "Great Article! Updated.",
                 "createdAt":  "2026-08-14T20:17:53.5803062"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4f. View Article Comments Paginated (Public)

**Request:** GET http://localhost:5049/api/Articles/public/79831ff7-0057-40da-71f4-08defa411e92/comments?pageNumber=1&pageSize=10

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
                     "id":  "d695e1c4-01e1-4498-a2fb-f4ee6b9bd893",
                     "articleId":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "userId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                     "userName":  "Test Client",
                     "content":  "Great Article! Updated.",
                     "createdAt":  "2026-08-14T20:17:53.5803062"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4g. Report Article (Client)

**Request:** POST http://localhost:5049/api/Articles/79831ff7-0057-40da-71f4-08defa411e92/report

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


### 4h. View My Liked Articles (Client)

**Request:** GET http://localhost:5049/api/Articles/my-likes

**Response Status:** 500

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "An internal server error occurred.",
    "errors":  null,
    "statusCode":  500
}
``n---


### 4i. View Article Check IsLiked (Client Token)

**Request:** GET http://localhost:5049/api/Articles/public/79831ff7-0057-40da-71f4-08defa411e92

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                 "title":  "Test Article 648577975",
                 "content":  "Test Article Content long...",
                 "tags":  "Law,Test",
                 "featuredImageUrl":  null,
                 "viewCount":  1,
                 "likesCount":  1,
                 "commentsCount":  1,
                 "isLikedByCurrentUser":  true,
                 "status":  2,
                 "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                 "category":  {
                                  "id":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                                  "code":  "ARTCAT_648577975",
                                  "nameAr":  "Updated Category 648577975",
                                  "description":  "Updated Description"
                              },
                 "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                 "authorName":  "Test Lawyer",
                 "createdAt":  "2026-08-14T20:17:52.6487838",
                 "updatedAt":  "2026-08-14T20:17:53.6369332"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4j. View Article Likers (Client Token)

**Request:** GET http://localhost:5049/api/Articles/79831ff7-0057-40da-71f4-08defa411e92/likers?pageNumber=1&pageSize=10

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
                     "id":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                     "fullName":  "Test Client",
                     "profilePictureUrl":  null
                 }
             ],
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
                     "id":  "134d01ed-5ab6-4607-b08f-579f18dde3a6",
                     "articleId":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "articleTitle":  "Test Article 648577975",
                     "reporterId":  "641367f9-48f3-4eaa-8152-08dee91d7c8f",
                     "reporterName":  "Test Client",
                     "reason":  "Inappropriate content",
                     "createdAt":  "2026-08-14T20:17:53.8896963",
                     "isResolved":  false
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5b. Resolve Report (Admin)

**Request:** PUT http://localhost:5049/api/Articles/admin/reports/134d01ed-5ab6-4607-b08f-579f18dde3a6/resolve

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

**Request:** DELETE http://localhost:5049/api/Articles/admin/79831ff7-0057-40da-71f4-08defa411e92

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
    "totalRecords":  5,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "79831ff7-0057-40da-71f4-08defa411e92",
                     "title":  "Test Article 648577975",
                     "featuredImageUrl":  null,
                     "viewCount":  1,
                     "likesCount":  1,
                     "commentsCount":  1,
                     "status":  2,
                     "categoryId":  "d7afc08d-63fe-4acd-f1f9-08defa40c741",
                     "categoryNameAr":  "Updated Category 648577975",
                     "authorId":  "cdea4991-9733-4400-8151-08dee91d7c8f",
                     "authorName":  "Test Lawyer",
                     "createdAt":  "2026-08-14T20:17:52.6487838"
                 },
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

**Request:** DELETE http://localhost:5049/api/ArticleCategories/admin/d7afc08d-63fe-4acd-f1f9-08defa40c741

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


