# Chat and Proposals - Exhaustive & Integration Report

Generated at 2026-08-10 12:58:12



## Phase 1: Zero Assumption Setup

### Register Client 1

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "chatprop_client1_20260810125812@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Client One"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "email": "chatprop_client1_20260810125812@example.com",
    "fullName": "Client One",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for chatprop_client1_20260810125812@example.com: http://localhost:5173/verify-email?userId=15f52ab3-7e20-4c77-d64d-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbTdWUzA2RDM4V25MYnhIZGZnY2V3NUhlWlJFK3NicXpPVUxaK3dpRnpKOU04WWllZGxtbGRHTFVSL0k3Z0dUcVZIN2dtMXdaRlJ4NUJpN21VODJjZ041Ny9FbXllMFMwOWJ3Z0ZkeEIxaWlXQml1ampHcGJDYjN6T292Um1qZG10cFJDNFZNNVZKUExKRVVoMXRndWNjQXJUeEYwYjgvOGlBaW1ZNlpWK0gvWVMxRnBKeTdERGFxSXZhUWxmc1c2MU5EVWMyV3pMZ0dFZmttQjhoSDRTdWtPSERYVkpZai94SHM1TTc2YlpLdz09

### Confirm Email for chatprop_client1_20260810125812@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=15f52ab3-7e20-4c77-d64d-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbTdWUzA2RDM4V25MYnhIZGZnY2V3NUhlWlJFK3NicXpPVUxaK3dpRnpKOU04WWllZGxtbGRHTFVSL0k3Z0dUcVZIN2dtMXdaRlJ4NUJpN21VODJjZ041Ny9FbXllMFMwOWJ3Z0ZkeEIxaWlXQml1ampHcGJDYjN6T292Um1qZG10cFJDNFZNNVZKUExKRVVoMXRndWNjQXJUeEYwYjgvOGlBaW1ZNlpWK0gvWVMxRnBKeTdERGFxSXZhUWxmc1c2MU5EVWMyV3pMZ0dFZmttQjhoSDRTdWtPSERYVkpZai94SHM1TTc2YlpLdz09

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


### Login Client 1

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "chatprop_client1_20260810125812@example.com",
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
      "id": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
      "email": "chatprop_client1_20260810125812@example.com",
      "fullName": "Client One",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNWY1MmFiMy03ZTIwLTRjNzctZDY0ZC0wOGRlZjZjNTM4MmQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ZjUyYWIzLTdlMjAtNGM3Ny1kNjRkLTA4ZGVmNmM1MzgyZCIsImVtYWlsIjoiY2hhdHByb3BfY2xpZW50MV8yMDI2MDgxMDEyNTgxMkBleGFtcGxlLmNvbSIsIm5hbWUiOiJDbGllbnQgT25lIiwic2VjdXJpdHlfc3RhbXAiOiJKTFozQ0lRS082RkVOWE1TRTNVT1BLV0NET1pGUUNVWSIsImp0aSI6IjRmMzJkMDZkLTJmNDMtNGEwNy1iMDM2LTBmNGJlODAxY2NmZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjM1NTg5MywiZXhwIjoxNzg2MzU2NzkzLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.15ndf1z_WVoEMSw9uhdZrKHErxspJHKvqGEazF7XO2Y",
    "expiresIn": 900,
    "refreshToken": "UUl/Y78efvspL90HaSpzDUNoCWdoAnfdfIF+uDb4Q5PPE3QbBKuvloxTogOsCpO6QJZuGeAB1/+inaKdyL5GJQ==",
    "refreshTokenExpiration": "2026-08-17T09:58:13.3420289Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Register Client 2

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "chatprop_client2_20260810125812@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Client Two"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "fb3123f9-8ab5-4e48-d64e-08def6c5382d",
    "email": "chatprop_client2_20260810125812@example.com",
    "fullName": "Client Two",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for chatprop_client2_20260810125812@example.com: http://localhost:5173/verify-email?userId=fb3123f9-8ab5-4e48-d64e-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvSnhLd0M2R1RKamVpNkQ1VUdGT3RSSFdVNk9TWklRd0FSSnhobmdodVF0YVErbUdGVm5TbUpZcStKRG9CNVRWZVp3WGhoRVpCd0hpUzNtcjIyZjVkb2dYUEZUV0tPd21rS2NWallnVjEwdTRWamJValMxelVIUFo1TkpJS1h6MDhpTjRXQkNEdmhRWFUrSlFlenczbTlEMXVaOUR3SFZEd2xPb2pkMEg5TGQ4MlQ5c1lEM1RTOEZDM1BJcUpsYmFpczhnY0ZubmFxVUJ3cmxicXd6Zm9YQVNWWmswWHJqSkNzelJWQjlPOWx2Zz09

### Confirm Email for chatprop_client2_20260810125812@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=fb3123f9-8ab5-4e48-d64e-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvSnhLd0M2R1RKamVpNkQ1VUdGT3RSSFdVNk9TWklRd0FSSnhobmdodVF0YVErbUdGVm5TbUpZcStKRG9CNVRWZVp3WGhoRVpCd0hpUzNtcjIyZjVkb2dYUEZUV0tPd21rS2NWallnVjEwdTRWamJValMxelVIUFo1TkpJS1h6MDhpTjRXQkNEdmhRWFUrSlFlenczbTlEMXVaOUR3SFZEd2xPb2pkMEg5TGQ4MlQ5c1lEM1RTOEZDM1BJcUpsYmFpczhnY0ZubmFxVUJ3cmxicXd6Zm9YQVNWWmswWHJqSkNzelJWQjlPOWx2Zz09

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


### Login Client 2

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "chatprop_client2_20260810125812@example.com",
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
      "id": "fb3123f9-8ab5-4e48-d64e-08def6c5382d",
      "email": "chatprop_client2_20260810125812@example.com",
      "fullName": "Client Two",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmYjMxMjNmOS04YWI1LTRlNDgtZDY0ZS0wOGRlZjZjNTM4MmQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImZiMzEyM2Y5LThhYjUtNGU0OC1kNjRlLTA4ZGVmNmM1MzgyZCIsImVtYWlsIjoiY2hhdHByb3BfY2xpZW50Ml8yMDI2MDgxMDEyNTgxMkBleGFtcGxlLmNvbSIsIm5hbWUiOiJDbGllbnQgVHdvIiwic2VjdXJpdHlfc3RhbXAiOiJZQ1haNEVJSTNDUVpQREYyWEdIWFVSQU40SlFLM0dGVCIsImp0aSI6IjE2NjkyZTk3LTA3NTYtNGVjNS1hOWQxLWVlZTM0OTc0NWQwZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjM1NTg5MywiZXhwIjoxNzg2MzU2NzkzLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.A8v9hrwWDJUGdn257j9M22f6jxrA6FifIYbbbOHhSsU",
    "expiresIn": 900,
    "refreshToken": "nuo7dJaIbA3atPUsMI28fZP9TuX2zKPUtc8pTnDUhmVPN/21ICLDC47Upup6zSJYxJz0vBGk/WUN6CQ1vXDcow==",
    "refreshTokenExpiration": "2026-08-17T09:58:13.8731716Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Email": "chatprop_lawyer1_20260810125812@example.com",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "FullName": "Lawyer One"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "email": "chatprop_lawyer1_20260810125812@example.com",
    "fullName": "Lawyer One",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for chatprop_lawyer1_20260810125812@example.com: http://localhost:5173/verify-email?userId=2fffc720-fc9e-4e73-d64f-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5dXZEcU40R0d6MjV6OWgyNEFCb05XMTFlaVRTeE5oejZHWHRHem9QR3dGOUtQSTVmL2tnSEtCREZZbzB2em5IZ00xTC8yUCt0bTFVeWJZc0tmbFJXQWUrdmxuNXNwNDk0NFNSbHluWDhtazllUmFNcGs0SWRWMkhyaVRCMW90WlU5a09GbFNjZWc4dVdlVVJmYTd2WVRYNEZvWXgzVTIzeVh3UzFaZTE4bUlsRnQ1RFZ6ZVZ0MzZvajlWdDlUZnlzaEh0V3VMYlFiSDRXTk5uQnVTOUFUaGZURnlvSmtISHAyMnRPWmRtbGZjdz09

### Confirm Email for chatprop_lawyer1_20260810125812@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=2fffc720-fc9e-4e73-d64f-08def6c5382d&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5dXZEcU40R0d6MjV6OWgyNEFCb05XMTFlaVRTeE5oejZHWHRHem9QR3dGOUtQSTVmL2tnSEtCREZZbzB2em5IZ00xTC8yUCt0bTFVeWJZc0tmbFJXQWUrdmxuNXNwNDk0NFNSbHluWDhtazllUmFNcGs0SWRWMkhyaVRCMW90WlU5a09GbFNjZWc4dVdlVVJmYTd2WVRYNEZvWXgzVTIzeVh3UzFaZTE4bUlsRnQ1RFZ6ZVZ0MzZvajlWdDlUZnlzaEh0V3VMYlFiSDRXTk5uQnVTOUFUaGZURnlvSmtISHAyMnRPWmRtbGZjdz09

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


### Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "chatprop_lawyer1_20260810125812@example.com",
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
      "id": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
      "email": "chatprop_lawyer1_20260810125812@example.com",
      "fullName": "Lawyer One",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyZmZmYzcyMC1mYzllLTRlNzMtZDY0Zi0wOGRlZjZjNTM4MmQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjJmZmZjNzIwLWZjOWUtNGU3My1kNjRmLTA4ZGVmNmM1MzgyZCIsImVtYWlsIjoiY2hhdHByb3BfbGF3eWVyMV8yMDI2MDgxMDEyNTgxMkBleGFtcGxlLmNvbSIsIm5hbWUiOiJMYXd5ZXIgT25lIiwic2VjdXJpdHlfc3RhbXAiOiJZTkNIVDJRVlNNNlVJNlRZV0NLWUdPVkNVNFEzRUVMUCIsImp0aSI6IjhlNzc1ZTA1LTY2MjktNDQ1MC1hYzUwLWZmNzdjMjhjNzRmOSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjM1NTg5NCwiZXhwIjoxNzg2MzU2Nzk0LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.OvPej3HYNrLladtXEAYiy1WWgPmH8RZg31UgggsD9XA",
    "expiresIn": 900,
    "refreshToken": "zTC5UP7nyRcWkia1vSsXPcvIjd7qQTdXw+U9H5WK51g3aHLmCCgYyq58YFDQRW1av7hLpt/v3S4TxKKwR1teyQ==",
    "refreshTokenExpiration": "2026-08-17T09:58:14.5848351Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Complete Client 1 Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "PhoneNumber": "+201011111111",
  "Gender": 1,
  "Address": "Cairo",
  "NationalNumber": "29001015ba277",
  "DateOfBirth": "1990-01-01"
}
``n
**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 78 97 116 105 111 110 97 108 78 117 109 98 101 114 34 58 91 34 216 167 217 132 216 177 217 130 217 133 32 216 167 217 132 217 130 217 136 217 133 217 138 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 216 170 217 131 217 136 217 134 32 217 133 217 134 32 49 52 32 216 177 217 130 217 133 32 216 168 216 167 217 132 216 182 216 168 216 183 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 98 98 98 99 102 52 48 102 54 98 98 49 48 55 50 52 98 49 97 54 100 57 51 53 48 48 57 98 54 101 56 48 45 102 51 100 52 54 57 50 99 50 50 101 100 102 49 50 98 45 48 48 34 125
---


### Complete Lawyer 1 Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Gender": 1,
  "NationalNumber": "2850101ffbba4",
  "DateOfBirth": "1985-01-01",
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "Specialization": 1,
      "CasesHandled": 10
    }
  ],
  "Bio": "Expert Lawyer",
  "Address": "Cairo",
  "Level": 1,
  "PhoneNumber": "+201022222222"
}
``n
**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 78 97 116 105 111 110 97 108 78 117 109 98 101 114 34 58 91 34 216 167 217 132 216 177 217 130 217 133 32 216 167 217 132 217 130 217 136 217 133 217 138 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 216 170 217 131 217 136 217 134 32 217 133 217 134 32 49 52 32 216 177 217 130 217 133 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 55 56 57 50 52 56 56 99 54 50 52 50 57 55 49 100 50 101 54 48 52 100 55 101 99 52 98 100 52 102 102 50 45 53 102 51 56 49 101 102 49 48 54 50 99 53 102 49 98 45 48 48 34 125
---


### Login Admin

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
      "id": "54af6cd4-a46e-4fc6-34ca-08def604e4b7",
      "email": "admin@smartcourt.com",
      "fullName": "System Administrator",
      "role": "Admin",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NGFmNmNkNC1hNDZlLTRmYzYtMzRjYS0wOGRlZjYwNGU0YjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU0YWY2Y2Q0LWE0NmUtNGZjNi0zNGNhLTA4ZGVmNjA0ZTRiNyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IldVS0tLWlBLVFBHTUJINURCTkdYV0VYQkNGVTVPU1g0IiwianRpIjoiYTVhMTRhOTgtZTdkOC00ZTU3LWEwMGEtMGYzZTUwMjQ3YWMzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYzNTU4OTQsImV4cCI6MTc4NjM1Njc5NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.L4Hqr0MC5kxoHiFLZzdYT6UoTKYa4I6zLVtFUvcoX6Y",
    "expiresIn": 900,
    "refreshToken": "AcJTc+3sFoD8g1At9iSXcK6Llba9arLr4ohRXOKvPCPWpeB7GXDh5orrZEYteehk/eCGaeD4C33RddrXguv4zA==",
    "refreshTokenExpiration": "2026-08-17T09:58:14.8133782Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/2fffc720-fc9e-4e73-d64f-08def6c5382d/approve-account

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


### Admin Approve Client 1

**Request:** PATCH http://localhost:5049/api/admin/verifications/15f52ab3-7e20-4c77-d64d-08def6c5382d/approve-account

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


### Re-Login Client 1

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "chatprop_client1_20260810125812@example.com",
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
      "id": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
      "email": "chatprop_client1_20260810125812@example.com",
      "fullName": "Client One",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNWY1MmFiMy03ZTIwLTRjNzctZDY0ZC0wOGRlZjZjNTM4MmQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ZjUyYWIzLTdlMjAtNGM3Ny1kNjRkLTA4ZGVmNmM1MzgyZCIsImVtYWlsIjoiY2hhdHByb3BfY2xpZW50MV8yMDI2MDgxMDEyNTgxMkBleGFtcGxlLmNvbSIsIm5hbWUiOiJDbGllbnQgT25lIiwic2VjdXJpdHlfc3RhbXAiOiJKTFozQ0lRS082RkVOWE1TRTNVT1BLV0NET1pGUUNVWSIsImp0aSI6IjQ4YmI4ZGNiLTYxYzAtNGQxOS05ZWU3LTJmOGI5ZDk4NWU4ZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjM1NTg5NSwiZXhwIjoxNzg2MzU2Nzk1LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.JT1sBvXbtQ-qWFUxhPr1n2zuRwmD7UnDFKDRU1-ZNr8",
    "expiresIn": 900,
    "refreshToken": "MN55QMEytxEtKl74WYWm6nUkqhGRcoHstNe99dLd2y3XSe0mE0iwABMXeDNcmgl2IyAvrK9pZfFA7c8VVwGIPQ==",
    "refreshTokenExpiration": "2026-08-17T09:58:15.0556079Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "chatprop_lawyer1_20260810125812@example.com",
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
      "id": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
      "email": "chatprop_lawyer1_20260810125812@example.com",
      "fullName": "Lawyer One",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyZmZmYzcyMC1mYzllLTRlNzMtZDY0Zi0wOGRlZjZjNTM4MmQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjJmZmZjNzIwLWZjOWUtNGU3My1kNjRmLTA4ZGVmNmM1MzgyZCIsImVtYWlsIjoiY2hhdHByb3BfbGF3eWVyMV8yMDI2MDgxMDEyNTgxMkBleGFtcGxlLmNvbSIsIm5hbWUiOiJMYXd5ZXIgT25lIiwic2VjdXJpdHlfc3RhbXAiOiJZTkNIVDJRVlNNNlVJNlRZV0NLWUdPVkNVNFEzRUVMUCIsImp0aSI6ImExM2M3NjkyLTE5YzEtNDlmMC05ODhmLTYwNmU5ZDVmOTJmNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjM1NTg5NSwiZXhwIjoxNzg2MzU2Nzk1LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.0xLzz5TPYhbNJXvaKKnXZq6uwJp2ncOxy4DGpa6tlYA",
    "expiresIn": 900,
    "refreshToken": "dISgBdgih+c9kGkGcd7kYVkHRlAjriLyZLQ0Qi+suKJbMbshmhL1AQzSqDczLS6lStqDwVMHTicS7oUgWGiq8w==",
    "refreshTokenExpiration": "2026-08-17T09:58:15.1969656Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Profiles completed and verified** 


## Phase 2: Case Initialization

### Create Case

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "failedDocuments": [
      {
        "fileName": "dummy_chatprop.pdf",
        "error": "Error while uploading document : Invalid Compact JWS"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Finalize Case

**Request:** POST http://localhost:5049/api/Case/aa28d157-c6e1-41f6-9f3b-26ec41e31062/finalize

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
    "caseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Case Created & Finalized** 


## Phase 3: Proposals - Edge Cases & Validations

### GET Availability Valid

**Request:** GET http://localhost:5049/api/proposals/cases/aa28d157-c6e1-41f6-9f3b-26ec41e31062/availability

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "activeProposalCount": 0,
    "proposalLimit": 5,
    "availableProposalSlots": 5,
    "canSendProposal": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Availability (Valid Case)** 

### GET Availability 404

**Request:** GET http://localhost:5049/api/proposals/cases/fa0d2687-ce60-4c4e-8fc8-88a0c79607e1/availability

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


- [OK] **GET Availability (Invalid Case -> 404/400)** 

### GET Availability 401

**Request:** GET http://localhost:5049/api/proposals/cases/aa28d157-c6e1-41f6-9f3b-26ec41e31062/availability

**Response Status:** 401

**Response Body:** (Empty)
---


- [OK] **GET Availability (No Token -> 401)** 

### POST Proposal (Empty Message -> 400)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "'Message' must not be empty."
  ],
  "statusCode": 400
}
``n---


- [OK] **POST Proposal - Empty Message** 

### POST Proposal (XSS Message -> 201 or 400)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "<script>alert('xss')</script>"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b36c599b-1fbb-45c5-9794-733b264f0d1f",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "<script>alert('xss')</script>",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:16.8094457",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:16.8094457",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:16.8094457",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **POST Proposal - XSS Payload Handled** 

### Cancel XSS

**Request:** POST http://localhost:5049/api/proposals/b36c599b-1fbb-45c5-9794-733b264f0d1f/cancel

**Body:**
`json
{
  "Reason": "Test cleanup"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b36c599b-1fbb-45c5-9794-733b264f0d1f",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "<script>alert('xss')</script>",
    "status": "Cancelled",
    "decisionReason": "Test cleanup",
    "createdAt": "2026-08-10T09:58:16.8094457",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:16.8573866",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:16.8094457",
    "closedAt": "2026-08-10T09:58:16.8573866",
    "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST Proposal (Massive String -> 400)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "The length of 'Message' must be 2000 characters or fewer. You entered 10000 characters."
  ],
  "statusCode": 400
}
``n---


- [OK] **POST Proposal - Massive String** 

### POST Proposal (Invalid Case -> 404/400)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "fa0d2687-ce60-4c4e-8fc8-88a0c79607e1",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Valid"
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


- [OK] **POST Proposal - Invalid Case** 

### POST Proposal (Lawyer Role -> 403)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Valid"
}
``n
**Response Status:** 403

**Response Body:** (Empty)
---


- [OK] **POST Proposal - Role Lawyer** 

### POST Proposal 1 (Valid)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Cancel me"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b31b8e91-3a05-4364-8fa3-5c1133df5f94",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Cancel me",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.0209879",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:17.0209879",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.0209879",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **POST Proposal 1 - Created** 

### POST Cancel Proposal (Client 2 -> 403/404)

**Request:** POST http://localhost:5049/api/proposals/b31b8e91-3a05-4364-8fa3-5c1133df5f94/cancel

**Body:**
`json
{
  "Reason": "Not mine"
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Proposal was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [OK] **Cancel Proposal - Unauthorized User** 

### POST Cancel Proposal (Client 1 -> 200)

**Request:** POST http://localhost:5049/api/proposals/b31b8e91-3a05-4364-8fa3-5c1133df5f94/cancel

**Body:**
`json
{
  "Reason": "Changed mind"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b31b8e91-3a05-4364-8fa3-5c1133df5f94",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Cancel me",
    "status": "Cancelled",
    "decisionReason": "Changed mind",
    "createdAt": "2026-08-10T09:58:17.0209879",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:17.0699538",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.0209879",
    "closedAt": "2026-08-10T09:58:17.0699538",
    "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Cancel Proposal - Success** 

### POST Proposal 2 (Valid)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Reject me"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "09fe7193-35e4-422f-ac0d-dbb26bbb484c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Reject me",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.1698751",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:17.1698751",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.1698751",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### POST Reject Proposal (Client 1 -> 403)

**Request:** POST http://localhost:5049/api/proposals/09fe7193-35e4-422f-ac0d-dbb26bbb484c/reject

**Body:**
`json
{
  "Reason": "Not lawyer"
}
``n
**Response Status:** 403

**Response Body:** (Empty)
---


- [OK] **Reject Proposal - Role Client** 

### POST Reject Proposal (Lawyer -> 200)

**Request:** POST http://localhost:5049/api/proposals/09fe7193-35e4-422f-ac0d-dbb26bbb484c/reject

**Body:**
`json
{
  "Reason": "Too busy"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "09fe7193-35e4-422f-ac0d-dbb26bbb484c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Reject me",
    "status": "Rejected",
    "decisionReason": "Too busy",
    "createdAt": "2026-08-10T09:58:17.1698751",
    "respondedAt": "2026-08-10T09:58:17.2334108",
    "updatedAt": "2026-08-10T09:58:17.2334108",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.1698751",
    "closedAt": "2026-08-10T09:58:17.2334108",
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Reject Proposal - Success** 

### POST Proposal 3 (Valid)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Accept me"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "f44372be-69fc-42cb-9ebd-220526ef5784",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Accept me",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.2890197",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:17.2890197",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.2890197",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### POST Accept Proposal (Lawyer -> 200)

**Request:** POST http://localhost:5049/api/proposals/f44372be-69fc-42cb-9ebd-220526ef5784/accept

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
    "id": "f44372be-69fc-42cb-9ebd-220526ef5784",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Accept me",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.2890197",
    "respondedAt": "2026-08-10T09:58:17.3140089",
    "updatedAt": "2026-08-10T09:58:17.3140089",
    "conversationId": "1ccf2ab6-e483-4ff4-8d6f-b9d458a6c550",
    "expiresAt": "2026-08-13T09:58:17.2890197",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Accept Proposal - Success** 

### POST Terminate Proposal (Client 1 -> 200)

**Request:** POST http://localhost:5049/api/proposals/f44372be-69fc-42cb-9ebd-220526ef5784/terminate

**Body:**
`json
{
  "Reason": "Never mind"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "f44372be-69fc-42cb-9ebd-220526ef5784",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Accept me",
    "status": "Terminated",
    "decisionReason": "Never mind",
    "createdAt": "2026-08-10T09:58:17.2890197",
    "respondedAt": "2026-08-10T09:58:17.3140089",
    "updatedAt": "2026-08-10T09:58:17.3528238",
    "conversationId": "1ccf2ab6-e483-4ff4-8d6f-b9d458a6c550",
    "expiresAt": "2026-08-13T09:58:17.2890197",
    "closedAt": "2026-08-10T09:58:17.3528238",
    "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Terminate Proposal - Success** 

### POST Proposal 4 (Valid - Final)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
  "LawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
  "Message": "Proceed with this one"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Proceed with this one",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.4275364",
    "respondedAt": null,
    "updatedAt": "2026-08-10T09:58:17.4275364",
    "conversationId": null,
    "expiresAt": "2026-08-13T09:58:17.4275364",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### POST Accept Proposal (Lawyer -> 200)

**Request:** POST http://localhost:5049/api/proposals/9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c/accept

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
    "id": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Proceed with this one",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.4275364",
    "respondedAt": "2026-08-10T09:58:17.5068116",
    "updatedAt": "2026-08-10T09:58:17.5068116",
    "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "expiresAt": "2026-08-13T09:58:17.4275364",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Accept Proposal 4 - Success** 

### GET Proposals Listing

**Request:** GET http://localhost:5049/api/proposals?page=1&pageSize=10

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "clientName": "Client One",
        "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "lawyerName": "Lawyer One",
        "status": "Accepted",
        "createdAt": "2026-08-10T09:58:17.4275364",
        "respondedAt": "2026-08-10T09:58:17.5068116",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "expiresAt": "2026-08-13T09:58:17.4275364",
        "closedAt": null,
        "closedByUserId": null
      },
      {
        "id": "f44372be-69fc-42cb-9ebd-220526ef5784",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "clientName": "Client One",
        "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "lawyerName": "Lawyer One",
        "status": "Terminated",
        "createdAt": "2026-08-10T09:58:17.2890197",
        "respondedAt": "2026-08-10T09:58:17.3140089",
        "conversationId": "1ccf2ab6-e483-4ff4-8d6f-b9d458a6c550",
        "expiresAt": "2026-08-13T09:58:17.2890197",
        "closedAt": "2026-08-10T09:58:17.3528238",
        "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
      },
      {
        "id": "09fe7193-35e4-422f-ac0d-dbb26bbb484c",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "clientName": "Client One",
        "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "lawyerName": "Lawyer One",
        "status": "Rejected",
        "createdAt": "2026-08-10T09:58:17.1698751",
        "respondedAt": "2026-08-10T09:58:17.2334108",
        "conversationId": null,
        "expiresAt": "2026-08-13T09:58:17.1698751",
        "closedAt": "2026-08-10T09:58:17.2334108",
        "closedByUserId": null
      },
      {
        "id": "b31b8e91-3a05-4364-8fa3-5c1133df5f94",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "clientName": "Client One",
        "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "lawyerName": "Lawyer One",
        "status": "Cancelled",
        "createdAt": "2026-08-10T09:58:17.0209879",
        "respondedAt": null,
        "conversationId": null,
        "expiresAt": "2026-08-13T09:58:17.0209879",
        "closedAt": "2026-08-10T09:58:17.0699538",
        "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
      },
      {
        "id": "b36c599b-1fbb-45c5-9794-733b264f0d1f",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "clientName": "Client One",
        "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "lawyerName": "Lawyer One",
        "status": "Cancelled",
        "createdAt": "2026-08-10T09:58:16.8094457",
        "respondedAt": null,
        "conversationId": null,
        "expiresAt": "2026-08-13T09:58:16.8094457",
        "closedAt": "2026-08-10T09:58:16.8573866",
        "closedByUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 5,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Proposals Listing** 

### GET Proposal 4 Detail

**Request:** GET http://localhost:5049/api/proposals/9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "clientName": "Client One",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "lawyerName": "Lawyer One",
    "message": "Proceed with this one",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-10T09:58:17.4275364",
    "respondedAt": "2026-08-10T09:58:17.5068116",
    "updatedAt": "2026-08-10T09:58:17.5068116",
    "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "expiresAt": "2026-08-13T09:58:17.4275364",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Proposal Detail** 

### GET Proposal 4 Detail (Client 2 -> 404/403)

**Request:** GET http://localhost:5049/api/proposals/9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Proposal was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [OK] **GET Proposal Detail - Cross tenant** 


## Phase 4: Contract & Milestones

### Lawyer Creates Contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
`json
{
  "TermsAndConditions": "This is a valid Terms And Conditions string that exceeds twenty characters.",
  "Title": "Test Contract long title",
  "ProposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b758d253-5c4b-4bdf-b31b-208407c2401d",
    "proposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "title": "Test Contract long title",
    "termsAndConditions": "This is a valid Terms And Conditions string that exceeds twenty characters.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAFIM=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Contract Created** 

### Get Contract (Client)

**Request:** GET http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b758d253-5c4b-4bdf-b31b-208407c2401d",
    "proposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "title": "Test Contract long title",
    "termsAndConditions": "This is a valid Terms And Conditions string that exceeds twenty characters.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAFIM=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Client Accepts Contract

**Request:** POST http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d/accept

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
    "entityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
    "status": "Draft",
    "occurredAt": "2026-08-10T09:58:17.8266116Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Get Contract (Lawyer)

**Request:** GET http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b758d253-5c4b-4bdf-b31b-208407c2401d",
    "proposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "clientUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "lawyerUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "title": "Test Contract long title",
    "termsAndConditions": "This is a valid Terms And Conditions string that exceeds twenty characters.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-10T09:58:17.8266116",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAFI4=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Accepts Contract

**Request:** POST http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d/accept

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
    "entityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
    "status": "Draft",
    "occurredAt": "2026-08-10T09:58:17.8929798Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Milestone 1

**Request:** POST http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d/milestones

**Body:**
`json
{
  "DurationDays": 14,
  "Amount": 1500.0,
  "Description": "This is a valid Description string that exceeds twenty characters.",
  "Title": "Phase 1 longer title",
  "OrderNumber": 1
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9303d5ad-37eb-4b3d-9da0-69920304aeb7",
    "orderNumber": 1,
    "title": "Phase 1 longer title",
    "description": "This is a valid Description string that exceeds twenty characters.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAFJs=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Milestone Created** 

### List M1

**Request:** GET http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9303d5ad-37eb-4b3d-9da0-69920304aeb7",
      "orderNumber": 1,
      "title": "Phase 1 longer title",
      "description": "This is a valid Description string that exceeds twenty characters.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAFJs=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Client Approves M1

**Request:** POST http://localhost:5049/api/milestones/9303d5ad-37eb-4b3d-9da0-69920304aeb7/approve

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
    "entityId": "9303d5ad-37eb-4b3d-9da0-69920304aeb7",
    "status": "Draft",
    "occurredAt": "2026-08-10T09:58:18.1105409Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### List M1 Lawyer

**Request:** GET http://localhost:5049/api/contracts/b758d253-5c4b-4bdf-b31b-208407c2401d/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9303d5ad-37eb-4b3d-9da0-69920304aeb7",
      "orderNumber": 1,
      "title": "Phase 1 longer title",
      "description": "This is a valid Description string that exceeds twenty characters.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAFKE=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Approves M1

**Request:** POST http://localhost:5049/api/milestones/9303d5ad-37eb-4b3d-9da0-69920304aeb7/approve

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
    "entityId": "9303d5ad-37eb-4b3d-9da0-69920304aeb7",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-10T09:58:18.2174795Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



## Phase 5: Chat - Edge Cases & Validations

### GET Chat Conversations

**Request:** GET http://localhost:5049/api/chat/conversations

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "proposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "client": {
          "userId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
          "name": "Client One",
          "role": "Client"
        },
        "lawyer": {
          "userId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
          "name": "Lawyer One",
          "role": "Lawyer"
        },
        "status": "Open",
        "createdAt": "2026-08-10T09:58:17.5068116",
        "updatedAt": "2026-08-10T09:58:17.8972779",
        "lastMessageAt": "2026-08-10T09:58:17.8972779",
        "lastMessage": {
          "id": "26030928-4c37-48d5-872a-33d5037053d9",
          "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
          "senderUserId": null,
          "senderName": null,
          "type": "System",
          "content": "Contract draft was accepted.",
          "systemCode": "ContractAccepted",
          "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
          "createdAt": "2026-08-10T09:58:17.8972779",
          "isMine": false
        }
      },
      {
        "id": "1ccf2ab6-e483-4ff4-8d6f-b9d458a6c550",
        "proposalId": "f44372be-69fc-42cb-9ebd-220526ef5784",
        "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
        "caseTitle": "Proposal Case",
        "client": {
          "userId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
          "name": "Client One",
          "role": "Client"
        },
        "lawyer": {
          "userId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
          "name": "Lawyer One",
          "role": "Lawyer"
        },
        "status": "Closed",
        "createdAt": "2026-08-10T09:58:17.3140089",
        "updatedAt": "2026-08-10T09:58:17.35288",
        "lastMessageAt": "2026-08-10T09:58:17.35288",
        "lastMessage": {
          "id": "e14c3acd-073c-4b10-ae77-2d12b2fab455",
          "conversationId": "1ccf2ab6-e483-4ff4-8d6f-b9d458a6c550",
          "senderUserId": null,
          "senderName": null,
          "type": "System",
          "content": "This proposal negotiation was ended. Reason: Never mind",
          "systemCode": "ProposalTerminated",
          "relatedEntityId": "f44372be-69fc-42cb-9ebd-220526ef5784",
          "createdAt": "2026-08-10T09:58:17.35288",
          "isMine": false
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 2,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Chat Conversations** 

- [OK] **Conversation exists** (ID: 1fa2e4c2-a745-4779-9b93-f4eca8eeb279)

### GET Conversation Detail (Valid)

**Request:** GET http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "proposalId": "9dd0c88f-11a9-4945-bc9a-0c2f53b95b5c",
    "legalCaseId": "aa28d157-c6e1-41f6-9f3b-26ec41e31062",
    "caseTitle": "Proposal Case",
    "client": {
      "userId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
      "name": "Client One",
      "role": "Client"
    },
    "lawyer": {
      "userId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
      "name": "Lawyer One",
      "role": "Lawyer"
    },
    "status": "Open",
    "createdAt": "2026-08-10T09:58:17.5068116",
    "updatedAt": "2026-08-10T09:58:17.8972779",
    "lastMessageAt": "2026-08-10T09:58:17.8972779"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Conversation Detail** 

### GET Conversation Detail (Client 2 -> 404/403)

**Request:** GET http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Conversation was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [OK] **GET Conversation Detail - Cross tenant** 

### POST Message (Empty -> 400)

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "'Content' must not be empty."
  ],
  "statusCode": 400
}
``n---


- [OK] **POST Message - Empty String** 

### POST Message (Massive -> 400/200)

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "The length of 'Content' must be 2000 characters or fewer. You entered 5000 characters."
  ],
  "statusCode": 400
}
``n---


- [OK] **POST Message - Massive String (Validates limits)** 

### POST Message (XSS/Emojis)

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": "<script>alert('xss')</script> 😀😀 Emojis ñ Zalgo ̐̐̐"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "a847871a-8f3f-4da9-8998-503fa2b27f90",
    "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "senderName": "Client One",
    "type": "User",
    "content": "<script>alert('xss')</script> 😀😀 Emojis ñ Zalgo ̐̐̐",
    "systemCode": null,
    "relatedEntityId": null,
    "createdAt": "2026-08-10T09:58:18.5484762",
    "isMine": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **POST Message - Complex Charsets & XSS** 

### POST Message (Client 2 -> 403/404)

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": "Intruder!"
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Conversation was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [OK] **POST Message - Unauthorized Access** 

### Client Sends Message

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": "Hello Lawyer, I accepted the contract."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "8cbacf68-e2a5-43bf-b243-6c365787d345",
    "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
    "senderName": "Client One",
    "type": "User",
    "content": "Hello Lawyer, I accepted the contract.",
    "systemCode": null,
    "relatedEntityId": null,
    "createdAt": "2026-08-10T09:58:18.6050083",
    "isMine": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Sends Message

**Request:** POST http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages

**Body:**
`json
{
  "Content": "Thank you! I will begin work."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "8b3da592-1010-418c-8437-b7459ea19bf4",
    "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
    "senderUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
    "senderName": "Lawyer One",
    "type": "User",
    "content": "Thank you! I will begin work.",
    "systemCode": null,
    "relatedEntityId": null,
    "createdAt": "2026-08-10T09:58:18.6435805",
    "isMine": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### GET Messages (Valid)

**Request:** GET http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages?page=1&pageSize=10

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "bcdfb122-4df4-425a-a3a3-b70660aaaba9",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was created.",
        "systemCode": "ContractCreated",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.7318965",
        "isMine": false
      },
      {
        "id": "edb562ee-6ff4-4b54-bfcb-a68915fe487a",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was accepted.",
        "systemCode": "ContractAccepted",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.8279594",
        "isMine": false
      },
      {
        "id": "26030928-4c37-48d5-872a-33d5037053d9",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was accepted.",
        "systemCode": "ContractAccepted",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.8972779",
        "isMine": false
      },
      {
        "id": "a847871a-8f3f-4da9-8998-503fa2b27f90",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "senderName": "Client One",
        "type": "User",
        "content": "<script>alert('xss')</script> 😀😀 Emojis ñ Zalgo ̐̐̐",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.5484762",
        "isMine": true
      },
      {
        "id": "8cbacf68-e2a5-43bf-b243-6c365787d345",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "senderName": "Client One",
        "type": "User",
        "content": "Hello Lawyer, I accepted the contract.",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.6050083",
        "isMine": true
      },
      {
        "id": "8b3da592-1010-418c-8437-b7459ea19bf4",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "senderName": "Lawyer One",
        "type": "User",
        "content": "Thank you! I will begin work.",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.6435805",
        "isMine": false
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 6,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Messages Listing** 

### GET Messages (Negative Page -> Should resolve to 1/400)

**Request:** GET http://localhost:5049/api/chat/conversations/1fa2e4c2-a745-4779-9b93-f4eca8eeb279/messages?page=-5&pageSize=-10

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "bcdfb122-4df4-425a-a3a3-b70660aaaba9",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was created.",
        "systemCode": "ContractCreated",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.7318965",
        "isMine": false
      },
      {
        "id": "edb562ee-6ff4-4b54-bfcb-a68915fe487a",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was accepted.",
        "systemCode": "ContractAccepted",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.8279594",
        "isMine": false
      },
      {
        "id": "26030928-4c37-48d5-872a-33d5037053d9",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": null,
        "senderName": null,
        "type": "System",
        "content": "Contract draft was accepted.",
        "systemCode": "ContractAccepted",
        "relatedEntityId": "b758d253-5c4b-4bdf-b31b-208407c2401d",
        "createdAt": "2026-08-10T09:58:17.8972779",
        "isMine": false
      },
      {
        "id": "a847871a-8f3f-4da9-8998-503fa2b27f90",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "senderName": "Client One",
        "type": "User",
        "content": "<script>alert('xss')</script> 😀😀 Emojis ñ Zalgo ̐̐̐",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.5484762",
        "isMine": false
      },
      {
        "id": "8cbacf68-e2a5-43bf-b243-6c365787d345",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "15f52ab3-7e20-4c77-d64d-08def6c5382d",
        "senderName": "Client One",
        "type": "User",
        "content": "Hello Lawyer, I accepted the contract.",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.6050083",
        "isMine": false
      },
      {
        "id": "8b3da592-1010-418c-8437-b7459ea19bf4",
        "conversationId": "1fa2e4c2-a745-4779-9b93-f4eca8eeb279",
        "senderUserId": "2fffc720-fc9e-4e73-d64f-08def6c5382d",
        "senderName": "Lawyer One",
        "type": "User",
        "content": "Thank you! I will begin work.",
        "systemCode": null,
        "relatedEntityId": null,
        "createdAt": "2026-08-10T09:58:18.6435805",
        "isMine": true
      }
    ],
    "page": 1,
    "pageSize": 50,
    "totalCount": 6,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **GET Messages Listing - Negative Constraints** 


## Test Execution Summary

---

**Completed at: 2026-08-10 12:58:18**

Please review the markdown logs above for full JSON requests and responses.
