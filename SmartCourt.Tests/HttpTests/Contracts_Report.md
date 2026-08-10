# Contracts Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-07 21:00:26


### Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_contract_20260807210026@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Client"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "aded12d0-bafa-40ff-f798-08def48f6968",
    "email": "client_contract_20260807210026@example.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_contract_20260807210026@example.com: http://localhost:5173/verify-email?userId=aded12d0-bafa-40ff-f798-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5TGVYS3E1L3Z2WE9BUjlzZ2tPR1pJUXdOalV5VnpOOCsycjg2NnNUL3ZTNXZzYTJUWE1qQ095cnpLQ05VVk0yQ0NDeHZoVG5uQ2RTcFBTVmNiaXJ1QTZtR1k3dEp5cE5JMGFOSURkR2NCUE5hVm9xWWk1YnM3eUljZm9hRW9WemNQU2hvWXpVTzVoVk1iTXBwZDNSRGdwaXhjbXY0N3RPcXpLVlJlbExzUDcwRGpTRGRtYlgvcVQ4MmhyVzNHNEJLN3dxTDJ0NXk4RG96THB3T2YxNXEwNlhKVjBBL091bnh1S3FOUVMwa0pKdz09

### Confirm Email for client_contract_20260807210026@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=aded12d0-bafa-40ff-f798-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5TGVYS3E1L3Z2WE9BUjlzZ2tPR1pJUXdOalV5VnpOOCsycjg2NnNUL3ZTNXZzYTJUWE1qQ095cnpLQ05VVk0yQ0NDeHZoVG5uQ2RTcFBTVmNiaXJ1QTZtR1k3dEp5cE5JMGFOSURkR2NCUE5hVm9xWWk1YnM3eUljZm9hRW9WemNQU2hvWXpVTzVoVk1iTXBwZDNSRGdwaXhjbXY0N3RPcXpLVlJlbExzUDcwRGpTRGRtYlgvcVQ4MmhyVzNHNEJLN3dxTDJ0NXk4RG96THB3T2YxNXEwNlhKVjBBL091bnh1S3FOUVMwa0pKdz09

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_contract_20260807210026@example.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "aded12d0-bafa-40ff-f798-08def48f6968",
      "email": "client_contract_20260807210026@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZGVkMTJkMC1iYWZhLTQwZmYtZjc5OC0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkZWQxMmQwLWJhZmEtNDBmZi1mNzk4LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2NvbnRyYWN0XzIwMjYwODA3MjEwMDI2QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJIUENJNU0zUkc0RFhBTEZFUVFVRTZLQzRGR1FSNzNCNCIsImp0aSI6ImVhNmU4NWQ3LWE1ZTktNGRkNC1iYjMxLThjYjNhMTRiOGYwNSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjEyNTYyNiwiZXhwIjoxNzg2MTI2NTI2LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.5zQ9CL6JiGVekMxoJ3Z7k1O_5jjBIe3daAl2I2kKVQs",
    "expiresIn": 900,
    "refreshToken": "VwIkA1X8jtpEQ92SrQs8GAaRDYlukTgcpAbIp2HUkRt109pfpY/QaVXCEPfbRoHXFd+RJf+FcM35XFF2crWLAQ==",
    "refreshTokenExpiration": "2026-08-14T18:00:26.8786903Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Complete Client Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "DateOfBirth": "1990-01-01",
  "NationalNumber": "29001011111111",
  "Gender": 1,
  "Address": "Cairo",
  "PhoneNumber": "+201011111111"
}
``n
**Response Status:** 500

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "An internal server error occurred.",
  "errors": null,
  "statusCode": 500
}
``n---


### Setup - Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_contract_20260807210026@example.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "aded12d0-bafa-40ff-f798-08def48f6968",
      "email": "client_contract_20260807210026@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZGVkMTJkMC1iYWZhLTQwZmYtZjc5OC0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkZWQxMmQwLWJhZmEtNDBmZi1mNzk4LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2NvbnRyYWN0XzIwMjYwODA3MjEwMDI2QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJIUENJNU0zUkc0RFhBTEZFUVFVRTZLQzRGR1FSNzNCNCIsImp0aSI6IjU3MGEyZDljLWVkODgtNDBlNS1iNmNkLThkZDQ1YzQ1MWYyNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjEyNTYyNywiZXhwIjoxNzg2MTI2NTI3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.wsGsDM8aln2CuwXFxA220OQ1sP6TneaLd9OM3bmCuj0",
    "expiresIn": 900,
    "refreshToken": "K1V5ElG44jBtombfNXuqE6FZa5KQnVB+WcjWxj/4yeHlTNwt89x8vDAb7YXWhmyAr/UD7KHpkQ9k2CugPpsNlA==",
    "refreshTokenExpiration": "2026-08-14T18:00:27.4421399Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Email": "lawyer_contract_20260807210027@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Lawyer"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "a0b49d8f-19b3-44be-f799-08def48f6968",
    "email": "lawyer_contract_20260807210027@example.com",
    "fullName": "Test Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_contract_20260807210027@example.com: http://localhost:5173/verify-email?userId=a0b49d8f-19b3-44be-f799-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5d21wS0dqUm0reG9BSmVOR1Z3NEQvTXdZeFdmSWdCb0JjVG50OGFPRHE2WE9taHg5Nnd2cWJ3d0V2VnhyTmxCMlRySWFqVW9GdXBIejZ0dHVLcjZGQ1c2eGtiNk92TjlSc2tPdlFtRlhGd0JiYVl6a1JqbG1uQXhxSk5ETysvd0pYT1J4ZVNDb3pKdndMVmZWRTVNSGpQK3d3T3VwcjhNRXcvZTl3dzYyakcrM2JWTmlOVXJVei9tZFB4djBSMGNMb09LQmFwaTQyQThEbzlDblp6RGdqYzcvNCtyTFNGOGs5UDdacEFUWGoyUT09

### Confirm Email for lawyer_contract_20260807210027@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=a0b49d8f-19b3-44be-f799-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5d21wS0dqUm0reG9BSmVOR1Z3NEQvTXdZeFdmSWdCb0JjVG50OGFPRHE2WE9taHg5Nnd2cWJ3d0V2VnhyTmxCMlRySWFqVW9GdXBIejZ0dHVLcjZGQ1c2eGtiNk92TjlSc2tPdlFtRlhGd0JiYVl6a1JqbG1uQXhxSk5ETysvd0pYT1J4ZVNDb3pKdndMVmZWRTVNSGpQK3d3T3VwcjhNRXcvZTl3dzYyakcrM2JWTmlOVXJVei9tZFB4djBSMGNMb09LQmFwaTQyQThEbzlDblp6RGdqYzcvNCtyTFNGOGs5UDdacEFUWGoyUT09

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_contract_20260807210027@example.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "a0b49d8f-19b3-44be-f799-08def48f6968",
      "email": "lawyer_contract_20260807210027@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMGI0OWQ4Zi0xOWIzLTQ0YmUtZjc5OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEwYjQ5ZDhmLTE5YjMtNDRiZS1mNzk5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2NvbnRyYWN0XzIwMjYwODA3MjEwMDI3QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiI3Qks3UzRNWlVFVTJOQUVLSFYyUlBUUUpHWFZaQk9FWCIsImp0aSI6ImM5OGU5ZjdmLWUzMjctNDJkMC05ZTE3LTE1MzM5ZmY0NmUxMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjEyNTYyOCwiZXhwIjoxNzg2MTI2NTI4LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.BzpHcf3zXCrTTEbMbaICd2xx_CrvT_8lNx1N1fWRchM",
    "expiresIn": 900,
    "refreshToken": "NynpnSRTYrYJK+iAg70N0sbYgxBmfug6ccT46cc/Y1gywDBwlDeCuiTXxknCq9Iv35nzETpXtug6DhQ76codFg==",
    "refreshTokenExpiration": "2026-08-14T18:00:28.8381291Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "NationalNumber": "28501012222222",
  "PhoneNumber": "+201022222222",
  "Bio": "Expert Lawyer",
  "Address": "Cairo",
  "DateOfBirth": "1985-01-01",
  "Level": 1,
  "Gender": 1,
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ]
}
``n
**Response Status:** 500

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "An internal server error occurred.",
  "errors": null,
  "statusCode": 500
}
``n---


### Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "admin@smartcourt.com",
  "Password": "Admin@123"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "a39b6312-19c2-49f7-fe42-08def48c9663",
      "email": "admin@smartcourt.com",
      "fullName": "System Administrator",
      "role": "Admin",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMzliNjMxMi0xOWMyLTQ5ZjctZmU0Mi0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEzOWI2MzEyLTE5YzItNDlmNy1mZTQyLTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IkI0N09OTkw1V05BVUoyMzVMUlhIVTZOUVMyUEZPWkNRIiwianRpIjoiY2UxYWEzZGItMjIyYy00NmI1LWFkNmUtYTVkOTcwNTViZDA3IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYxMjU2MjksImV4cCI6MTc4NjEyNjUyOSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.Gi5RB8N3_24lVzHf9omlEPoP4rLDzEBi6652p4vbB6w",
    "expiresIn": 900,
    "refreshToken": "amat2nNIOBsAQko3eVEcFEQNLE4/UmURVL2U/s8R05r65BSmRW/thf0HjiZq1tNxSyRzWJF+iG2eA9skd/CZjA==",
    "refreshTokenExpiration": "2026-08-14T18:00:29.0710981Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/a0b49d8f-19b3-44be-f799-08def48f6968/approve-account

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_contract_20260807210027@example.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "a0b49d8f-19b3-44be-f799-08def48f6968",
      "email": "lawyer_contract_20260807210027@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMGI0OWQ4Zi0xOWIzLTQ0YmUtZjc5OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEwYjQ5ZDhmLTE5YjMtNDRiZS1mNzk5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2NvbnRyYWN0XzIwMjYwODA3MjEwMDI3QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiI3Qks3UzRNWlVFVTJOQUVLSFYyUlBUUUpHWFZaQk9FWCIsImp0aSI6Ijk0MDdjYzc2LWJlMWUtNGM3YS1iNWZkLTQ0NmJjZWNhZGE3MCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjEyNTYyOSwiZXhwIjoxNzg2MTI2NTI5LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.TjW-AFf2SpzTfoA3d1Y0NlIjCvgF0-HBrFc0K_eZnU0",
    "expiresIn": 900,
    "refreshToken": "wXVaUO4+VUaWYsGScxVGukB9zf2DhmqHkjbp2wMbn+qWJ8+fQ1Q69hlVOgl7z6WOjac106M5ihyXdeqB9cDtwA==",
    "refreshTokenExpiration": "2026-08-14T18:00:29.2849688Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Register Attacker

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "attacker_contract_20260807210029@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Attacker"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "ca413849-442e-4a84-f79a-08def48f6968",
    "email": "attacker_contract_20260807210029@example.com",
    "fullName": "Test Attacker",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for attacker_contract_20260807210029@example.com: http://localhost:5173/verify-email?userId=ca413849-442e-4a84-f79a-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Vi9WSzFNY1JZSEpFalJuNlVCYTZCR0M1TkIyL2ZpUzB4eTFTLzg1UzdOQ0JQbjRaYnFhV0xpM0MyNkZHOERVOGVqcU4wcUU5OUZGT083UXV4WGpBVXgxeG5YWm9zbVhzeGN6UEFlKy9XbmNuWHJQNExvbzYreEx3T09oM1RuTU0xeFVmSGE3V3ZrYjFpWlRpSUFoQ2VBa2pCditQQ2MvWmtiME1nNzByL2lMVlgwQnYrNVBIUG9yRlQ3UmF1eG5WbDBuSGZMbmRVYkZLcGV6ckNwbEtod0t3S3l5ek5QdGlFc1AzU1VLWFp4UT09

### Confirm Email for attacker_contract_20260807210029@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=ca413849-442e-4a84-f79a-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Vi9WSzFNY1JZSEpFalJuNlVCYTZCR0M1TkIyL2ZpUzB4eTFTLzg1UzdOQ0JQbjRaYnFhV0xpM0MyNkZHOERVOGVqcU4wcUU5OUZGT083UXV4WGpBVXgxeG5YWm9zbVhzeGN6UEFlKy9XbmNuWHJQNExvbzYreEx3T09oM1RuTU0xeFVmSGE3V3ZrYjFpWlRpSUFoQ2VBa2pCditQQ2MvWmtiME1nNzByL2lMVlgwQnYrNVBIUG9yRlQ3UmF1eG5WbDBuSGZMbmRVYkZLcGV6ckNwbEtod0t3S3l5ek5QdGlFc1AzU1VLWFp4UT09

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Login Attacker

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "attacker_contract_20260807210029@example.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "ca413849-442e-4a84-f79a-08def48f6968",
      "email": "attacker_contract_20260807210029@example.com",
      "fullName": "Test Attacker",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjYTQxMzg0OS00NDJlLTRhODQtZjc5YS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImNhNDEzODQ5LTQ0MmUtNGE4NC1mNzlhLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiYXR0YWNrZXJfY29udHJhY3RfMjAyNjA4MDcyMTAwMjlAZXhhbXBsZS5jb20iLCJuYW1lIjoiVGVzdCBBdHRhY2tlciIsInNlY3VyaXR5X3N0YW1wIjoiQjVWSFFNR1ZYSk1LVlpTWllCMk1FRzdLQUtPWEdISE8iLCJqdGkiOiJhMDljZTg2Ny1jNmQwLTRiZTMtYTY0OS0yZmVkODhjY2IxNDgiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMjU2MzAsImV4cCI6MTc4NjEyNjUzMCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.iIN5LWBySpKfvKbvbJ0NUL1rT_MGqn5UdzmJX4UmNcw",
    "expiresIn": 900,
    "refreshToken": "jjKi9nCM7xuc0tiWAKa6DvaYY5ZTicc37+ksL+2DITwPIvC/w/NTUXcuZnQeTQxRLtFB9dT2Q2nY6fm3jEHLyA==",
    "refreshTokenExpiration": "2026-08-14T18:00:30.6934049Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Create Case

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Setup - Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/8ac1f7c1-febe-41f7-bf00-0240a3b35aee/review

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "644db39e-61bb-4bdf-bfe3-c58a9b49a63a",
    "caseId": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee",
    "isLatest": true,
    "createdAt": "2026-08-07T18:00:35.6159026Z",
    "reviewPoints": [
      {
        "id": "e5098422-751a-46d3-8dc0-b68c34c9967c",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Case for Contract'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "6dbb724a-b887-4978-a079-1dd9b02ee5cd",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "189df112-b81d-4dc3-8fea-6e290959d4ab",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "62155312-0412-4011-99f9-b32ee44c18e5",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "d87e373a-dab1-4e38-8d7d-d0370c1948ea",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "c1ce3ecc-2786-4df0-82d3-8365282bdbcd",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "06654d2f-d38d-4d5a-9930-ff5cb9eb1e6d",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Get Latest Review

**Request:** GET http://localhost:5049/api/cases/8ac1f7c1-febe-41f7-bf00-0240a3b35aee/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "644db39e-61bb-4bdf-bfe3-c58a9b49a63a",
    "caseId": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee",
    "isLatest": true,
    "createdAt": "2026-08-07T18:00:35.6159026",
    "reviewPoints": [
      {
        "id": "6dbb724a-b887-4978-a079-1dd9b02ee5cd",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "189df112-b81d-4dc3-8fea-6e290959d4ab",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "c1ce3ecc-2786-4df0-82d3-8365282bdbcd",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "62155312-0412-4011-99f9-b32ee44c18e5",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "e5098422-751a-46d3-8dc0-b68c34c9967c",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Case for Contract'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "d87e373a-dab1-4e38-8d7d-d0370c1948ea",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "06654d2f-d38d-4d5a-9930-ff5cb9eb1e6d",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Finalize Case

**Request:** POST http://localhost:5049/api/Case/8ac1f7c1-febe-41f7-bf00-0240a3b35aee/finalize

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Client Creates Proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "Message": "Let's make a contract.",
  "LawyerUserId": "a0b49d8f-19b3-44be-f799-08def48f6968",
  "LegalCaseId": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee"
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Case was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### Setup - Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals//accept

**Body:**
`json
{}
``n
**Response Status:** 404

**Response Body:**
Response status code does not indicate success: 404 (Not Found).
---


