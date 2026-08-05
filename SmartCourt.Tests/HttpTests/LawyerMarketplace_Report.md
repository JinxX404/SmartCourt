# Lawyer Marketplace Search Endpoint Test Report


**Generated At:** 2026-08-05 22:18:14

**Target Endpoint:** GET /api/lawyers/search`n
---


### 0. Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "mkt_search_client@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1c5c72c0-fa49-416d-9911-b2bb1993d443",
                              "email":  "mkt_search_client@test.com",
                              "fullName":  "Marketplace Search Client",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxYzVjNzJjMC1mYTQ5LTQxNmQtOTkxMS1iMmJiMTk5M2Q0NDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFjNWM3MmMwLWZhNDktNDE2ZC05OTExLWIyYmIxOTkzZDQ0MyIsImVtYWlsIjoibWt0X3NlYXJjaF9jbGllbnRAdGVzdC5jb20iLCJuYW1lIjoiTWFya2V0cGxhY2UgU2VhcmNoIENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiOWQ4NmU0ZWUtNzAyMC00ZmIxLTk2NDItYWNhNTNlZWQwZTE5IiwianRpIjoiODViMDk0MDctOWM4OC00NmEwLTgxY2YtZjE2NGVhODNlZDA3IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ2xpZW50IiwibmJmIjoxNzg1OTU3NDk0LCJleHAiOjE3ODU5NjEwOTQsImlzcyI6IlNtYXJ0Q291cnRBUEkiLCJhdWQiOiJTbWFydENvdXJ0Q2xpZW50In0.__CmPB5uCJuJoqXbc3fbl264kZBoD2-04utCAuur2mg",
                 "expiresIn":  3600,
                 "refreshToken":  "gBXMrc/JDggkUbCu4Mm1dhfXSaJdVnU8RE1+Ov+JTqL6yDa7jMr6K0zepXQ0LRa8TMP9z+GCemlDfiTo3z+iKA==",
                 "refreshTokenExpiration":  "2026-08-12T19:18:14.8898023Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1. Default Search - All Active Lawyers (No Query Params)

**Request:** GET http://localhost:5049/api/lawyers/search

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2a. Pagination - PageNumber=1, PageSize=2

**Request:** GET http://localhost:5049/api/lawyers/search?PageNumber=1&PageSize=2

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  2,
    "totalPages":  5,
    "totalRecords":  10,
    "hasNextPage":  true,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2b. Pagination - PageNumber=2, PageSize=2

**Request:** GET http://localhost:5049/api/lawyers/search?PageNumber=2&PageSize=2

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  2,
    "pageSize":  2,
    "totalPages":  5,
    "totalRecords":  10,
    "hasNextPage":  true,
    "hasPreviousPage":  true,
    "success":  true,
    "data":  [
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2c. Pagination - PageSize Capping (PageSize=100 -> Capped to 50)

**Request:** GET http://localhost:5049/api/lawyers/search?PageNumber=1&PageSize=100

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  50,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3a. SearchTerm - Match by Name ('Ahmed')

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=Ahmed

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
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3b. SearchTerm - Match by Bio ('commercial')

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=commercial

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
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3c. SearchTerm - Non-matching ('NonExistentLawyer999')

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=NonExistentLawyer999

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


### 4a. Level Filter - Level=4 (CassationCourt)

**Request:** GET http://localhost:5049/api/lawyers/search?Level=4

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
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4b. Level Filter - Level=3 (AppealCourt)

**Request:** GET http://localhost:5049/api/lawyers/search?Level=3

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
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5a. Specialization Filter - Specialization=2 (CommercialLaw)

**Request:** GET http://localhost:5049/api/lawyers/search?Specialization=2

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
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5b. Specialization Filter - Specialization=0 (FamilyLaw)

**Request:** GET http://localhost:5049/api/lawyers/search?Specialization=0

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
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5c. Governorate Filter - Governorate=Cairo

**Request:** GET http://localhost:5049/api/lawyers/search?Governorate=Cairo

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
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6a. MinRating Filter - MinRating=4.5

**Request:** GET http://localhost:5049/api/lawyers/search?MinRating=4.5

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
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6b. IsAvailable Filter - IsAvailable=true

**Request:** GET http://localhost:5049/api/lawyers/search?IsAvailable=true

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  8,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6c. IsAvailable Filter - IsAvailable=false

**Request:** GET http://localhost:5049/api/lawyers/search?IsAvailable=false

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
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7a. Sorting - SortBy Rating Descending (SortBy=0, SortDirection=1)

**Request:** GET http://localhost:5049/api/lawyers/search?SortBy=0&SortDirection=1

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7b. Sorting - SortBy Rating Ascending (SortBy=0, SortDirection=0)

**Request:** GET http://localhost:5049/api/lawyers/search?SortBy=0&SortDirection=0

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7c. Sorting - SortBy ResponseTime Ascending (SortBy=1, SortDirection=0)

**Request:** GET http://localhost:5049/api/lawyers/search?SortBy=1&SortDirection=0

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7d. Sorting - SortBy ExperienceLevel Descending (SortBy=2, SortDirection=1)

**Request:** GET http://localhost:5049/api/lawyers/search?SortBy=2&SortDirection=1

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  10,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "id":  "3d8fa36c-6c2b-4b48-0b16-08def31747a9",
                     "name":  "Ahmed El-Sayed",
                     "gender":  1,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "530735d7-d978-4d2b-9804-79711bd41ce5",
                     "name":  "Nouran Ibrahim",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Labor law consultant handling employment contracts and State Council disputes",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "092b7211-fa0e-4a85-0b17-08def31747a9",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  1,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1e85360e-9bb5-4213-a9ac-005d30fff068",
                     "name":  "Fatima Al-Zahraa",
                     "gender":  null,
                     "level":  3,
                     "bio":  "Appeal Court specialist for family law, alimony, and custody cases",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "da0675ce-ec0e-46f2-0b18-08def31747a9",
                     "name":  "Mahmoud Hassan",
                     "gender":  1,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4f0ffe89-e1a3-44d9-856d-f3b1fb3dcbd8",
                     "name":  "Mahmoud Hassan",
                     "gender":  null,
                     "level":  2,
                     "bio":  "Primary Court litigation expert handling civil law and contract drafting",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "25745b5c-4611-43ab-9b7b-06215a1c5bae",
                     "name":  "Youssef Nabil",
                     "gender":  null,
                     "level":  1,
                     "bio":  "Junior attorney specializing in general civil consultations",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "1c3a1ad9-60f8-4b6e-28cb-08dee32cf02d",
                     "name":  "Test Lawyer",
                     "gender":  0,
                     "level":  1,
                     "bio":  "Experienced corporate lawyer.",
                     "isAvailable":  false,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 8. Multi-field Filter - Governorate=Cairo, Level=4, IsAvailable=true, MinRating=4.0

**Request:** GET http://localhost:5049/api/lawyers/search?Governorate=Cairo&Level=4&IsAvailable=true&MinRating=4.0&SortBy=0&SortDirection=1

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
                     "id":  "c520b9ca-e968-415f-a134-25b9b7ed0421",
                     "name":  "Mostafa Mansour",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Cassation level criminal law defender and constitutional advisor",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 },
                 {
                     "id":  "4ea9266d-e495-4210-a515-a9fa57ae06b0",
                     "name":  "Ahmed El-Sayed",
                     "gender":  null,
                     "level":  4,
                     "bio":  "Senior Cassation attorney specializing in commercial disputes and corporate law",
                     "isAvailable":  true,
                     "profilePictureUrl":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 9a. Validation Error - Invalid MinRating (> 5.0)

**Request:** GET http://localhost:5049/api/lawyers/search?MinRating=6.5

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "MinRating":  [
                                     "يجب أن يكون الحد الأدنى للتقييم بين 0 و 5."
                                 ]
               },
    "traceId":  "00-0b17a8f0f7dac38061dd7b5b13dc99e6-f56698b308a27829-00"
}
``n---


### 9b. Validation Error - Invalid MinRating (< 0.0)

**Request:** GET http://localhost:5049/api/lawyers/search?MinRating=-1.0

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "MinRating":  [
                                     "يجب أن يكون الحد الأدنى للتقييم بين 0 و 5."
                                 ]
               },
    "traceId":  "00-f843ace8f96e9281c298c73b89e1f47c-565d05b0e7ee8839-00"
}
``n---


### 9c. Validation Error - Invalid Level Enum

**Request:** GET http://localhost:5049/api/lawyers/search?Level=99

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Level":  [
                                 "The value \u002799\u0027 is invalid."
                             ]
               },
    "traceId":  "00-fb6246bb348aab9a6a1da783b82c795e-dd8378d27262a40b-00"
}
``n---


### 9d. Validation Error - Invalid Specialization Enum

**Request:** GET http://localhost:5049/api/lawyers/search?Specialization=99

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Specialization":  [
                                          "The value \u002799\u0027 is invalid."
                                      ]
               },
    "traceId":  "00-e7bd2b04a97fcdf9039f123b3b5575aa-4cfc46a3179b18c6-00"
}
``n---


### 9e. Validation Error - Invalid SortBy Enum

**Request:** GET http://localhost:5049/api/lawyers/search?SortBy=99

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "SortBy":  [
                                  "The value \u002799\u0027 is invalid."
                              ]
               },
    "traceId":  "00-f640e9ca387b14faa6d7a8abdbd12891-f07b99c3430eb6cf-00"
}
``n---


### 9f. Validation Error - Invalid SortDirection Enum

**Request:** GET http://localhost:5049/api/lawyers/search?SortDirection=99

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "SortDirection":  [
                                         "The value \u002799\u0027 is invalid."
                                     ]
               },
    "traceId":  "00-48992dbe81fee979fadf52feef7ae9d4-6ea1d242c1205da7-00"
}
``n---


### 9g. Validation Error - Invalid PageNumber (0)

**Request:** GET http://localhost:5049/api/lawyers/search?PageNumber=0

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "PageNumber":  [
                                      "رقم الصفحة يجب أن يكون 1 على الأقل."
                                  ]
               },
    "traceId":  "00-77d9b2e05f66cbbb26922d50b02ea306-eb434bbafa3015ab-00"
}
``n---


### 9h. Validation Error - Invalid PageSize (0)

**Request:** GET http://localhost:5049/api/lawyers/search?PageSize=0

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "PageSize":  [
                                    "حجم الصفحة يجب أن يكون بين 1 و 50."
                                ]
               },
    "traceId":  "00-5497788875763c2c4d67fc603bf2623c-3d9fac9aa2a974f2-00"
}
``n---


### 10a. Unauthorized - Missing Authorization Header

**Request:** GET http://localhost:5049/api/lawyers/search

**Response Status:** 401

**Response Body:** (Empty)
---


### 10b. Unauthorized - Invalid Token

**Request:** GET http://localhost:5049/api/lawyers/search

**Response Status:** 401

**Response Body:** (Empty)
---


### 11a. Stress Test - SQL Injection Attempt in SearchTerm

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=' OR '1'='1

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


### 11b. Stress Test - XSS Payload in SearchTerm

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=<script>alert('xss')</script>

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


### 11c. Stress Test - Arabic & Emoji Unicode in SearchTerm

**Request:** GET http://localhost:5049/api/lawyers/search?SearchTerm=Ù…Ø³ØªØ´Ø§Ø± âš–ï¸

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


