import json
import os

openapi_spec = {
    "openapi": "3.1.0",
    "info": {
        "title": "SmartCourt API",
        "version": "1.0.0",
        "description": "SmartCourt Frontend API Reference. This document is traced from the controller, request/response DTO, validator, and service/handler implementations currently under SmartCourt/Features. It describes the 25 routes exposed by those controllers. All validation messages, field constraints, Arabic error descriptions, and rate limit rules match docs/frontend_api.md exactly."
    },
    "servers": [
        {"url": "/", "description": "Current Server"}
    ],
    "components": {
        "securitySchemes": {
            "bearerAuth": {
                "type": "http",
                "scheme": "bearer",
                "bearerFormat": "JWT",
                "description": "Send Authorization: Bearer <access-token> for secured routes."
            }
        },
        "schemas": {
            "ApiResponse": {
                "type": "object",
                "properties": {
                    "success": {"type": "boolean", "description": "Indicates whether the request succeeded."},
                    "message": {"type": "string", "nullable": True, "description": "Human-readable message or error description."},
                    "errors": {
                        "type": "array",
                        "items": {"type": "string"},
                        "nullable": True,
                        "description": "List of specific error messages if any."
                    },
                    "statusCode": {"type": "integer", "description": "HTTP status code."}
                },
                "required": ["success", "statusCode"]
            },
            "UserDto": {
                "type": "object",
                "properties": {
                    "id": {"type": "string", "format": "uuid"},
                    "email": {"type": "string", "format": "email"},
                    "fullName": {"type": "string"},
                    "role": {"type": "string"}
                },
                "required": ["id", "email", "fullName", "role"]
            },
            "LoginResponse": {
                "type": "object",
                "properties": {
                    "user": {"$ref": "#/components/schemas/UserDto"},
                    "accessToken": {"type": "string"},
                    "expiresIn": {"type": "integer", "description": "Token expiration time in seconds."},
                    "refreshToken": {"type": "string"},
                    "refreshTokenExpiration": {"type": "string", "format": "date-time"}
                },
                "required": ["user", "accessToken", "expiresIn", "refreshToken", "refreshTokenExpiration"]
            },
            "RegisterResponse": {
                "type": "object",
                "properties": {
                    "userId": {"type": "string", "format": "uuid"},
                    "email": {"type": "string", "format": "email"},
                    "fullName": {"type": "string"},
                    "role": {"type": "string"}
                },
                "required": ["userId", "email", "fullName", "role"]
            },
            "RefreshTokenResponse": {
                "type": "object",
                "properties": {
                    "accessToken": {"type": "string"},
                    "refreshToken": {"type": "string"},
                    "expiresAt": {"type": "string", "format": "date-time"}
                },
                "required": ["accessToken", "refreshToken", "expiresAt"]
            },
            "ClientProfileResponse": {
                "type": "object",
                "properties": {
                    "id": {"type": "string", "format": "uuid"},
                    "name": {"type": "string"},
                    "email": {"type": "string", "format": "email"},
                    "phoneNumber": {"type": "string", "description": "Egyptian phone number (+201012345678)"},
                    "gender": {"type": "string"},
                    "dateOfBirth": {"type": "string", "format": "date", "nullable": True},
                    "address": {"type": "string", "nullable": True},
                    "status": {"type": "string"}
                },
                "required": ["id", "name", "email", "phoneNumber", "gender", "status"]
            },
            "LawyerProfileResponse": {
                "type": "object",
                "properties": {
                    "id": {"type": "string", "format": "uuid"},
                    "name": {"type": "string"},
                    "email": {"type": "string", "format": "email"},
                    "phoneNumber": {"type": "string"},
                    "nationalNumber": {"type": "string"},
                    "gender": {"type": "string"},
                    "dateOfBirth": {"type": "string", "format": "date", "nullable": True},
                    "specializationId": {"type": "string", "format": "uuid", "nullable": True},
                    "specializationName": {"type": "string"},
                    "categoryName": {"type": "string"},
                    "yearsOfExperience": {"type": "integer"},
                    "level": {"type": "integer", "description": "1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt"},
                    "bio": {"type": "string", "nullable": True},
                    "address": {"type": "string", "nullable": True},
                    "status": {"type": "string"},
                    "isAvailable": {"type": "boolean"},
                    "profilePictureUrl": {"type": "string", "nullable": True}
                },
                "required": ["id", "name", "email", "phoneNumber", "nationalNumber", "gender", "specializationName", "categoryName", "yearsOfExperience", "level", "status", "isAvailable"]
            },
            "PublicLawyerProfileResponse": {
                "type": "object",
                "properties": {
                    "id": {"type": "string", "format": "uuid"},
                    "name": {"type": "string"},
                    "gender": {"type": "string"},
                    "specializationId": {"type": "string", "format": "uuid", "nullable": True},
                    "specializationName": {"type": "string"},
                    "categoryName": {"type": "string"},
                    "yearsOfExperience": {"type": "integer"},
                    "level": {"type": "integer", "description": "1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt"},
                    "bio": {"type": "string", "nullable": True},
                    "isAvailable": {"type": "boolean"},
                    "profilePictureUrl": {"type": "string", "nullable": True}
                },
                "required": ["id", "name", "gender", "specializationName", "categoryName", "yearsOfExperience", "level", "isAvailable"]
            },
            "PendingVerificationListItemDto": {
                "type": "object",
                "properties": {
                    "lawyerId": {"type": "string", "format": "uuid"},
                    "fullName": {"type": "string"},
                    "email": {"type": "string", "format": "email"},
                    "phoneNumber": {"type": "string", "nullable": True},
                    "pendingDocumentCount": {"type": "integer"},
                    "verifiedDocumentCount": {"type": "integer"},
                    "rejectedDocumentCount": {"type": "integer"}
                },
                "required": ["lawyerId", "fullName", "email", "pendingDocumentCount", "verifiedDocumentCount", "rejectedDocumentCount"]
            },
            "VerificationDocumentDetailsDto": {
                "type": "object",
                "properties": {
                    "documentId": {"type": "string", "format": "uuid"},
                    "documentType": {"type": "string"},
                    "status": {"type": "string"},
                    "fileName": {"type": "string"},
                    "contentType": {"type": "string"},
                    "expirationDate": {"type": "string", "format": "date"},
                    "reviewedAt": {"type": "string", "format": "date-time", "nullable": True},
                    "rejectionReason": {"type": "string", "nullable": True},
                    "contentUrl": {"type": "string"}
                },
                "required": ["documentId", "documentType", "status", "fileName", "contentType", "expirationDate", "contentUrl"]
            },
            "VerificationDetailsDto": {
                "type": "object",
                "properties": {
                    "lawyerId": {"type": "string", "format": "uuid"},
                    "fullName": {"type": "string"},
                    "email": {"type": "string", "format": "email"},
                    "phoneNumber": {"type": "string", "nullable": True},
                    "accountStatus": {"type": "string"},
                    "isFullyVerified": {"type": "boolean"},
                    "documents": {
                        "type": "array",
                        "items": {"$ref": "#/components/schemas/VerificationDocumentDetailsDto"}
                    }
                },
                "required": ["lawyerId", "fullName", "email", "accountStatus", "isFullyVerified", "documents"]
            },
            "ReviewVerificationDocumentResponse": {
                "type": "object",
                "properties": {
                    "documentId": {"type": "string", "format": "uuid"},
                    "documentStatus": {"type": "string"},
                    "lawyerAccountStatus": {"type": "string"},
                    "isFullyVerified": {"type": "boolean"}
                },
                "required": ["documentId", "documentStatus", "lawyerAccountStatus", "isFullyVerified"]
            },
            "UploadedDocumentDto": {
                "type": "object",
                "properties": {
                    "fileName": {"type": "string"},
                    "type": {"type": "integer", "description": "1=NationalIdFront, 2=NationalIdBack, 3=BarAssociationCardFront, 4=BarAssociationCardBack"}
                },
                "required": ["fileName", "type"]
            },
            "DocumentUploadErrorDto": {
                "type": "object",
                "properties": {
                    "fileName": {"type": "string"},
                    "type": {"type": "integer"},
                    "error": {"type": "string"}
                },
                "required": ["fileName", "type", "error"]
            },
            "SubmitVerificationDocumentResponseDto": {
                "type": "object",
                "properties": {
                    "uploadedDocuments": {
                        "type": "array",
                        "items": {"$ref": "#/components/schemas/UploadedDocumentDto"}
                    },
                    "failedDocuments": {
                        "type": "array",
                        "items": {"$ref": "#/components/schemas/DocumentUploadErrorDto"}
                    }
                },
                "required": ["uploadedDocuments", "failedDocuments"]
            },
            "UserVerificationDocumentDto": {
                "type": "object",
                "properties": {
                    "documentId": {"type": "string", "format": "uuid"},
                    "documentType": {"type": "integer", "description": "1=NationalIdFront, 2=NationalIdBack, 3=BarAssociationCardFront, 4=BarAssociationCardBack"},
                    "status": {"type": "integer", "description": "1=Pending, 2=Verified, 3=Rejected, 4=Expired"},
                    "expirationDate": {"type": "string", "format": "date"},
                    "isCurrent": {"type": "boolean"},
                    "fileName": {"type": "string"}
                },
                "required": ["documentId", "documentType", "status", "expirationDate", "isCurrent", "fileName"]
            },
            "GetUserVerificationDocumentsResponseDto": {
                "type": "object",
                "properties": {
                    "documents": {
                        "type": "array",
                        "items": {"$ref": "#/components/schemas/UserVerificationDocumentDto"}
                    }
                },
                "required": ["documents"]
            },
            "PingResponse": {
                "type": "object",
                "properties": {
                    "message": {"type": "string", "example": "Pong! Smart Court API is fully operational."},
                    "serverTimeUtc": {"type": "string", "format": "date-time", "example": "2026-07-23T12:00:00Z"},
                    "version": {"type": "string", "example": "1.0.0"}
                },
                "required": ["message", "serverTimeUtc", "version"]
            }
        }
    },
    "tags": [
        {"name": "Auth", "description": "Authentication, registration, token refresh, and password management (10 endpoints)"},
        {"name": "Users", "description": "Client and Lawyer profile management and public profiles (7 endpoints)"},
        {"name": "Admin verifications", "description": "Admin review and verification of lawyer accounts and documents (4 endpoints)"},
        {"name": "User verification documents", "description": "User submission and deletion of verification documents (3 endpoints)"},
        {"name": "Health", "description": "Health check and operational ping (1 endpoint)"}
    ],
    "paths": {
        "/api/auth/login": {
            "post": {
                "tags": ["Auth"],
                "summary": "Login",
                "description": "Authenticates a user and issues an access token plus a seven-day refresh token. Authentication: Anonymous. Validation errors return HTTP 400. Authentication failures return HTTP 401. Forbidden accounts return HTTP 403.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["email", "password"],
                                "properties": {
                                    "email": {
                                        "type": "string",
                                        "format": "email",
                                        "description": "Required; must be a valid email (البريد الإلكتروني مطلوب. / البريد الإلكتروني غير صالح.)."
                                    },
                                    "password": {
                                        "type": "string",
                                        "minLength": 8,
                                        "description": "Required; minimum 8 characters (كلمة المرور مطلوبة. / كلمة المرور يجب أن تكون 8 أحرف على الأقل.)."
                                    }
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "User authenticated successfully.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/LoginResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "user": {
                                            "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                            "email": "client@example.com",
                                            "fullName": "Example Client",
                                            "role": "Client"
                                        },
                                        "accessToken": "eyJhbGciOi...",
                                        "expiresIn": 3600,
                                        "refreshToken": "base64-refresh-token",
                                        "refreshTokenExpiration": "2026-07-30T12:00:00Z"
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation failure."},
                    "401": {"description": "AuthenticationException('البريد الإلكتروني أو كلمة المرور غير صحيحة.')"},
                    "403": {"description": "ForbiddenAccessException ('يرجى تأكيد البريد الإلكتروني أولاً' or 'تم تعليق حسابك. تواصل مع الدعم')"}
                }
            }
        },
        "/api/auth/register/client": {
            "post": {
                "tags": ["Auth"],
                "summary": "Register client",
                "description": "Creates an unverified client account and queues a confirmation email. Authentication: Anonymous.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["fullName", "email", "password", "confirmPassword"],
                                "properties": {
                                    "fullName": {
                                        "type": "string",
                                        "minLength": 5,
                                        "maxLength": 150,
                                        "description": "Required; 5-150 characters (الاسم الكامل مطلوب.)."
                                    },
                                    "email": {
                                        "type": "string",
                                        "format": "email",
                                        "description": "Required; valid email."
                                    },
                                    "password": {
                                        "type": "string",
                                        "minLength": 8,
                                        "pattern": "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
                                        "description": "Required; at least 8 characters and must match lowercase, uppercase, and digit."
                                    },
                                    "confirmPassword": {
                                        "type": "string",
                                        "description": "Must equal password (تأكيد كلمة المرور غير مطابق. / كلمة المرور وتأكيد كلمة المرور غير متطابقتين.)."
                                    }
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "201": {
                        "description": "Account created successfully.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/RegisterResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "userId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "email": "client@example.com",
                                        "fullName": "Example Client",
                                        "role": "Client"
                                    },
                                    "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
                                    "errors": None,
                                    "statusCode": 201
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or identity creation failure."},
                    "500": {"description": "ConflictException('البريد الإلكتروني مسجل بالفعل.') or email queue failure."}
                }
            }
        },
        "/api/auth/register/lawyer": {
            "post": {
                "tags": ["Auth"],
                "summary": "Register lawyer",
                "description": "Creates an unverified lawyer account from a multipart form and queues a confirmation email. Authentication: Anonymous.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "multipart/form-data": {
                            "schema": {
                                "type": "object",
                                "required": [
                                    "fullName", "email", "password", "confirmPassword", "phone",
                                    "address", "government", "city", "gender", "nationalNumber"
                                ],
                                "properties": {
                                    "fullName": {"type": "string", "minLength": 5, "maxLength": 150},
                                    "email": {"type": "string", "format": "email"},
                                    "password": {"type": "string", "minLength": 8, "pattern": "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$"},
                                    "confirmPassword": {"type": "string"},
                                    "phone": {"type": "string", "pattern": "^\\+20\\d{10}$", "description": "Egyptian phone number"},
                                    "address": {"type": "string", "maxLength": 500},
                                    "government": {"type": "string", "maxLength": 100},
                                    "city": {"type": "string", "maxLength": 100},
                                    "gender": {"type": "string", "enum": ["Male", "Female"]},
                                    "nationalNumber": {"type": "string", "pattern": "^[0-9]{14}$", "description": "Exactly 14 numeric digits"},
                                    "nationalIdFront": {"type": "string", "format": "binary"},
                                    "nationalIdBack": {"type": "string", "format": "binary"},
                                    "syndicateCard": {"type": "string", "format": "binary"},
                                    "personalPhoto": {"type": "string", "format": "binary"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "201": {
                        "description": "Lawyer account created successfully.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/RegisterResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "userId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "email": "lawyer@example.com",
                                        "fullName": "Example Lawyer",
                                        "role": "Lawyer"
                                    },
                                    "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
                                    "errors": None,
                                    "statusCode": 201
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or identity creation failure."},
                    "500": {"description": "ConflictException('البريد الإلكتروني مسجل بالفعل.' or 'الرقم القومي مسجل بالفعل.')"}
                }
            }
        },
        "/api/auth/refresh": {
            "post": {
                "tags": ["Auth"],
                "summary": "Refresh access token",
                "description": "Rotates an active refresh token and returns a new access/refresh pair.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["refreshToken"],
                                "properties": {
                                    "refreshToken": {"type": "string", "description": "Required (رمز التحديث مطلوب.)."}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Token refreshed successfully.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/RefreshTokenResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "accessToken": "eyJhbGciOi...",
                                        "refreshToken": "new-base64-refresh-token",
                                        "expiresAt": "2026-07-30T12:00:00Z"
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or identity update failure."},
                    "401": {"description": "AuthenticationException('رمز التحديث غير صالح أو منتهي الصلاحية.')"}
                }
            }
        },
        "/api/auth/revoke": {
            "post": {
                "tags": ["Auth"],
                "summary": "Revoke refresh token",
                "description": "Validates the access token without checking its lifetime and revokes the supplied refresh token.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["token", "refreshToken"],
                                "properties": {
                                    "token": {"type": "string", "description": "Required (رمز الوصول مطلوب.)."},
                                    "refreshToken": {"type": "string", "description": "Required (رمز التحديث مطلوب.)."}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Refresh token revoked.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"type": "boolean"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": True,
                                    "message": "تم إبطال رمز التحديث بنجاح.",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "BusinessException ('رمز الوصول غير صالح.' or 'رمز التحديث غير صالح.')"}
                }
            }
        },
        "/api/auth/change-password": {
            "post": {
                "tags": ["Auth"],
                "summary": "Change password",
                "description": "Changes the authenticated user's password and revokes all active refresh tokens. Rate limit: IP 20/15 minutes and authenticated user 5/15 minutes.",
                "security": [{"bearerAuth": []}],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["currentPassword", "newPassword", "confirmNewPassword"],
                                "properties": {
                                    "currentPassword": {"type": "string", "description": "Required (كلمة المرور الحالية مطلوبة)"},
                                    "newPassword": {"type": "string", "minLength": 8, "description": "Required; at least 8 characters; one lowercase, one uppercase, and one digit."},
                                    "confirmNewPassword": {"type": "string", "description": "Must equal newPassword (كلمة المرور وتأكيد كلمة المرور غير متطابقتين)"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Password changed successfully.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم تغيير كلمة المرور بنجاح",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation failure ('كلمة المرور الحالية غير صحيحة.' or identity errors)."},
                    "401": {"description": "AuthenticationException('المستخدم غير معروف')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/auth/confirm-email": {
            "get": {
                "tags": ["Auth"],
                "summary": "Confirm email",
                "description": "Confirms an email address from the user id and Base64URL-encoded confirmation token. Rate limit: IP 20/15 minutes and account key 5/hour.",
                "security": [],
                "parameters": [
                    {"name": "userId", "in": "query", "required": False, "schema": {"type": "string", "maxLength": 64}, "description": "User ID Guid"},
                    {"name": "token", "in": "query", "required": False, "schema": {"type": "string", "maxLength": 2048}, "description": "Base64URL-encoded token"}
                ],
                "responses": {
                    "200": {
                        "description": "Email confirmed successfully.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم تأكيد البريد الإلكتروني بنجاح.",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "BusinessException('رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/auth/forgot-password": {
            "post": {
                "tags": ["Auth"],
                "summary": "Forgot password",
                "description": "Requests a password-reset email. Rate limit: IP 5/15 minutes and account key 3/hour.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["email"],
                                "properties": {
                                    "email": {"type": "string", "format": "email", "description": "Required; valid email (عنوان البريد الإلكتروني مطلوب / عنوان البريد الإلكتروني غير صالح)."}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Password reset email sent if eligible.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation failure."},
                    "429": {"description": "Rate limit exceeded."},
                    "500": {"description": "Queue email failure."}
                }
            }
        },
        "/api/auth/reset-password": {
            "post": {
                "tags": ["Auth"],
                "summary": "Reset password",
                "description": "Resets an eligible user's password using the Base64URL-encoded token and revokes all active refresh tokens. Rate limit: IP 10/15 minutes, account key 5/hour.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["email", "newPassword", "confirmNewPassword"],
                                "properties": {
                                    "email": {"type": "string", "format": "email"},
                                    "token": {"type": "string", "nullable": True, "maxLength": 2048},
                                    "newPassword": {"type": "string", "minLength": 8, "description": "Required; at least 8 chars; uppercase, lowercase, digit."},
                                    "confirmNewPassword": {"type": "string"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Password reset successfully.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم إعادة تعيين كلمة المرور بنجاح",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "BusinessException('رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/auth/resend-verification": {
            "post": {
                "tags": ["Auth"],
                "summary": "Resend verification email",
                "description": "Resends a confirmation email. Unknown, already-confirmed, or non-Unverified accounts are treated as a successful no-op. Rate limit: IP 5/15 minutes, account key 1/minute and 3/hour.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["email"],
                                "properties": {
                                    "email": {"type": "string", "format": "email"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Verification link sent.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم إرسال رابط التحقق مرة أخرى",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation failure."},
                    "429": {"description": "Rate limit exceeded."},
                    "500": {"description": "Queue email failure."}
                }
            }
        },
        "/api/clients/profile": {
            "get": {
                "tags": ["Users"],
                "summary": "Get client profile",
                "description": "Returns the profile for the authenticated client. Rate limit: IP 300/minute and user 120/minute.",
                "security": [{"bearerAuth": []}],
                "responses": {
                    "200": {
                        "description": "Client profile returned.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/ClientProfileResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "name": "Example Client",
                                        "email": "client@example.com",
                                        "phoneNumber": "+201012345678",
                                        "gender": "Male",
                                        "dateOfBirth": "1990-05-12",
                                        "address": "Cairo",
                                        "status": "Active"
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "401": {"description": "Unauthorized."},
                    "403": {"description": "Forbidden (role mismatch)."},
                    "404": {"description": "NotFoundException('الموكل غير موجود')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            },
            "put": {
                "tags": ["Users"],
                "summary": "Update client profile",
                "description": "Updates the authenticated client's phone number, date of birth, and address. Rate limit: IP 60/15 minutes and user 20/15 minutes.",
                "security": [{"bearerAuth": []}],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["phoneNumber", "dateOfBirth"],
                                "properties": {
                                    "phoneNumber": {"type": "string", "pattern": "^\\+20\\d{10}$", "description": "Egyptian phone number"},
                                    "dateOfBirth": {"type": "string", "format": "date", "description": "Must be earlier than today"},
                                    "address": {"type": "string", "nullable": True, "maxLength": 500}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Client profile updated.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم تحديث الملف الشخصي بنجاح.",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or identity update failure."},
                    "404": {"description": "NotFoundException('الموكل غير موجود')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            },
            "delete": {
                "tags": ["Users"],
                "summary": "Delete client profile",
                "description": "Soft-deletes the authenticated client, revokes active refresh tokens, and updates the security stamp. Rate limit: IP 10/day and user 3/day.",
                "security": [{"bearerAuth": []}],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["currentPassword"],
                                "properties": {
                                    "currentPassword": {"type": "string", "description": "Required (كلمة المرور الحالية مطلوبة.)."}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Account deleted.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم حذف الملف الشخصي بنجاح.",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "BusinessException('كلمة المرور الحالية غير صحيحة.')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/lawyers/profile": {
            "get": {
                "tags": ["Users"],
                "summary": "Get lawyer profile",
                "description": "Returns the authenticated lawyer's private profile. Rate limit: IP 300/minute and user 120/minute.",
                "security": [{"bearerAuth": []}],
                "responses": {
                    "200": {
                        "description": "Lawyer profile returned.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/LawyerProfileResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "name": "Example Lawyer",
                                        "email": "lawyer@example.com",
                                        "phoneNumber": "+201012345678",
                                        "nationalNumber": "29001011234567",
                                        "gender": "Male",
                                        "dateOfBirth": "1985-03-20",
                                        "specializationId": "a6b4f7cb-2f0f-4f32-bf5f-08f2a6b3c701",
                                        "specializationName": "Civil Law",
                                        "categoryName": "Law",
                                        "yearsOfExperience": 12,
                                        "level": 2,
                                        "bio": "Civil litigation lawyer",
                                        "address": "Giza",
                                        "status": "Active",
                                        "isAvailable": True,
                                        "profilePictureUrl": "https://cdn.example/profile.jpg"
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "401": {"description": "Unauthorized."},
                    "403": {"description": "Forbidden."},
                    "404": {"description": "NotFoundException('المحامي غير موجود')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            },
            "put": {
                "tags": ["Users"],
                "summary": "Update lawyer profile",
                "description": "Updates the authenticated lawyer's contact, specialization, experience, level, biography, and address. Rate limit: IP 60/15 minutes and user 20/15 minutes.",
                "security": [{"bearerAuth": []}],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["phoneNumber", "dateOfBirth", "specializationId", "yearsOfExperience", "level"],
                                "properties": {
                                    "phoneNumber": {"type": "string", "pattern": "^\\+20\\d{10}$"},
                                    "dateOfBirth": {"type": "string", "format": "date"},
                                    "specializationId": {"type": "string", "format": "uuid"},
                                    "yearsOfExperience": {"type": "integer", "minimum": 0, "maximum": 50},
                                    "level": {"type": "integer", "enum": [1, 2, 3, 4], "description": "1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt"},
                                    "bio": {"type": "string", "nullable": True, "maxLength": 500},
                                    "address": {"type": "string", "nullable": True, "maxLength": 255}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Lawyer profile updated.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم تحديث البيانات بنجاح",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or identity error ('مستوى المحامي غير صالح.' or 'التخصص غير صالح.')."},
                    "404": {"description": "NotFoundException('المحامي غير موجود')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            },
            "delete": {
                "tags": ["Users"],
                "summary": "Delete lawyer profile",
                "description": "Soft-deletes the authenticated lawyer, marks the lawyer unavailable, revokes active refresh tokens, and updates the security stamp. Rate limit: IP 10/day and user 3/day.",
                "security": [{"bearerAuth": []}],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["currentPassword"],
                                "properties": {
                                    "currentPassword": {"type": "string"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Account deleted.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": "تم حذف الحساب بنجاح",
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "BusinessException('كلمة المرور الحالية غير صحيحة.')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/lawyers/public/{id}": {
            "get": {
                "tags": ["Users"],
                "summary": "Get public lawyer profile",
                "description": "Returns a public profile only when the user is a lawyer with a current lawyer profile, confirmed email, and Active status. Anonymous. Rate limit: IP 120/minute.",
                "security": [],
                "parameters": [
                    {"name": "id", "in": "path", "required": True, "schema": {"type": "string", "format": "uuid"}, "description": "Lawyer GUID"}
                ],
                "responses": {
                    "200": {
                        "description": "Public lawyer profile returned.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/PublicLawyerProfileResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "name": "Example Lawyer",
                                        "gender": "Male",
                                        "specializationId": "a6b4f7cb-2f0f-4f32-bf5f-08f2a6b3c701",
                                        "specializationName": "Civil Law",
                                        "categoryName": "Law",
                                        "yearsOfExperience": 12,
                                        "level": 2,
                                        "bio": "Civil litigation lawyer",
                                        "isAvailable": True,
                                        "profilePictureUrl": "https://cdn.example/profile.jpg"
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Invalid route GUID."},
                    "404": {"description": "NotFoundException('المحامي غير موجود')"},
                    "429": {"description": "Rate limit exceeded."}
                }
            }
        },
        "/api/admin/verifications": {
            "get": {
                "tags": ["Admin verifications"],
                "summary": "List pending verifications",
                "description": "Returns a paginated list of lawyers whose current verification documents match the optional status filter. Requires Admin role.",
                "security": [{"bearerAuth": []}],
                "parameters": [
                    {"name": "pageNumber", "in": "query", "required": False, "schema": {"type": "integer", "default": 1, "minimum": 1}},
                    {"name": "pageSize", "in": "query", "required": False, "schema": {"type": "integer", "default": 10, "minimum": 1, "maximum": 50}},
                    {"name": "search", "in": "query", "required": False, "schema": {"type": "string", "maxLength": 100, "nullable": True}},
                    {"name": "status", "in": "query", "required": False, "schema": {"type": "integer", "enum": [1, 2, 3, 4], "nullable": True}, "description": "1=Pending, 2=Verified, 3=Rejected, 4=Expired"}
                ],
                "responses": {
                    "200": {
                        "description": "Paginated list of pending verifications.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "type": "object",
                                    "properties": {
                                        "success": {"type": "boolean"},
                                        "data": {
                                            "type": "array",
                                            "items": {"$ref": "#/components/schemas/PendingVerificationListItemDto"}
                                        },
                                        "message": {"type": "string", "nullable": True},
                                        "errors": {"type": "array", "items": {"type": "string"}, "nullable": True},
                                        "statusCode": {"type": "integer"},
                                        "pageNumber": {"type": "integer"},
                                        "pageSize": {"type": "integer"},
                                        "totalPages": {"type": "integer"},
                                        "totalRecords": {"type": "integer"},
                                        "hasNextPage": {"type": "boolean"},
                                        "hasPreviousPage": {"type": "boolean"}
                                    }
                                },
                                "example": {
                                    "success": True,
                                    "data": [
                                        {
                                            "lawyerId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                            "fullName": "Example Lawyer",
                                            "email": "lawyer@example.com",
                                            "phoneNumber": "+201012345678",
                                            "pendingDocumentCount": 2,
                                            "verifiedDocumentCount": 1,
                                            "rejectedDocumentCount": 0
                                        }
                                    ],
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200,
                                    "pageNumber": 1,
                                    "pageSize": 10,
                                    "totalPages": 1,
                                    "totalRecords": 1,
                                    "hasNextPage": False,
                                    "hasPreviousPage": False
                                }
                            }
                        }
                    },
                    "400": {"description": "Invalid query values ('Status must be a valid verification document status.')"}
                }
            }
        },
        "/api/admin/verifications/{lawyerId}": {
            "get": {
                "tags": ["Admin verifications"],
                "summary": "Get lawyer verification details",
                "description": "Returns the lawyer's current verification documents and account verification state. Requires Admin role.",
                "security": [{"bearerAuth": []}],
                "parameters": [
                    {"name": "lawyerId", "in": "path", "required": True, "schema": {"type": "string", "format": "uuid"}}
                ],
                "responses": {
                    "200": {
                        "description": "Lawyer verification details.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/VerificationDetailsDto"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "lawyerId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
                                        "fullName": "Example Lawyer",
                                        "email": "lawyer@example.com",
                                        "phoneNumber": "+201012345678",
                                        "accountStatus": "PendingReview",
                                        "isFullyVerified": False,
                                        "documents": [
                                            {
                                                "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
                                                "documentType": "NationalIdFront",
                                                "status": "Pending",
                                                "fileName": "national-id-front.jpg",
                                                "contentType": "image/jpeg",
                                                "expirationDate": "2030-12-31",
                                                "reviewedAt": None,
                                                "rejectionReason": None,
                                                "contentUrl": "/api/admin/verifications/documents/7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a/content"
                                            }
                                        ]
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Empty GUID validation error."},
                    "404": {"description": "NotFoundException('Lawyer was not found.')"}
                }
            }
        },
        "/api/admin/verifications/documents/{documentId}/content": {
            "get": {
                "tags": ["Admin verifications"],
                "summary": "Download verification document",
                "description": "Downloads the current verification document bytes. Requires Admin role.",
                "security": [{"bearerAuth": []}],
                "parameters": [
                    {"name": "documentId", "in": "path", "required": True, "schema": {"type": "string", "format": "uuid"}}
                ],
                "responses": {
                    "200": {
                        "description": "Binary file stream.",
                        "content": {
                            "image/jpeg": {"schema": {"type": "string", "format": "binary"}},
                            "image/png": {"schema": {"type": "string", "format": "binary"}},
                            "application/octet-stream": {"schema": {"type": "string", "format": "binary"}}
                        }
                    },
                    "400": {"description": "Document id is required."},
                    "404": {"description": "NotFoundException('Verification document was not found.')"}
                }
            }
        },
        "/api/admin/verifications/documents/{documentId}": {
            "patch": {
                "tags": ["Admin verifications"],
                "summary": "Review verification document",
                "description": "Approves or rejects the current pending document and recalculates the lawyer account status. Requires Admin role.",
                "security": [{"bearerAuth": []}],
                "parameters": [
                    {"name": "documentId", "in": "path", "required": True, "schema": {"type": "string", "format": "uuid"}}
                ],
                "requestBody": {
                    "required": True,
                    "content": {
                        "application/json": {
                            "schema": {
                                "type": "object",
                                "required": ["decision"],
                                "properties": {
                                    "decision": {"type": "integer", "enum": [1, 2], "description": "1=Approve, 2=Reject"},
                                    "rejectionReason": {"type": "string", "nullable": True, "maxLength": 500, "description": "Required when decision=2; must be empty when decision=1."}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Review completed.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/ReviewVerificationDocumentResponse"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
                                        "documentStatus": "Verified",
                                        "lawyerAccountStatus": "Active",
                                        "isFullyVerified": True
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation error."},
                    "404": {"description": "Verification document was not found."},
                    "409": {"description": "Only pending documents can be reviewed / document has expired."}
                }
            }
        },
        "/api/UserVerification/submit-verification-documents": {
            "post": {
                "tags": ["User verification documents"],
                "summary": "Submit verification documents",
                "description": "Uploads one or more verification images for a user and returns per-file successes and failures. Anonymous controller action.",
                "security": [],
                "requestBody": {
                    "required": True,
                    "content": {
                        "multipart/form-data": {
                            "schema": {
                                "type": "object",
                                "required": ["userId", "documents[0].file", "documents[0].expirationDate", "documents[0].type"],
                                "properties": {
                                    "userId": {"type": "string", "format": "uuid", "description": "Required (UserId is required)"},
                                    "documents[0].file": {"type": "string", "format": "binary", "description": "JPEG, PNG, WEBP, HEIC, or HEIF image"},
                                    "documents[0].expirationDate": {"type": "string", "format": "date", "description": "Must be future date"},
                                    "documents[0].type": {"type": "integer", "enum": [1, 2, 3, 4, 5], "description": "1=NationalIdFront, 2=NationalIdBack, 3=BarAssociationCardFront, 4=BarAssociationCardBack, 5=other"}
                                }
                            }
                        }
                    }
                },
                "responses": {
                    "200": {
                        "description": "Documents uploaded.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/SubmitVerificationDocumentResponseDto"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "uploadedDocuments": [
                                            {
                                                "fileName": "id-front.jpg",
                                                "type": 1
                                            }
                                        ],
                                        "failedDocuments": [
                                            {
                                                "fileName": "old-card.jpg",
                                                "type": 3,
                                                "error": "This document is expired"
                                            }
                                        ]
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Validation or upload error."}
                }
            }
        },
        "/api/UserVerification/{UserId}": {
            "get": {
                "tags": ["User verification documents"],
                "summary": "Get user verification documents",
                "description": "Lists all verification documents belonging to the supplied user id. Anonymous controller action.",
                "security": [],
                "parameters": [
                    {"name": "UserId", "in": "path", "required": True, "schema": {"type": "string", "format": "uuid"}}
                ],
                "responses": {
                    "200": {
                        "description": "User verification documents returned.",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "allOf": [
                                        {"$ref": "#/components/schemas/ApiResponse"},
                                        {
                                            "type": "object",
                                            "properties": {
                                                "data": {"$ref": "#/components/schemas/GetUserVerificationDocumentsResponseDto"}
                                            }
                                        }
                                    ]
                                },
                                "example": {
                                    "success": True,
                                    "data": {
                                        "documents": [
                                            {
                                                "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
                                                "documentType": 1,
                                                "status": 1,
                                                "expirationDate": "2030-12-31",
                                                "isCurrent": True,
                                                "fileName": "id-front.jpg"
                                            }
                                        ]
                                    },
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "User Id is required."},
                    "404": {"description": "The specified user does not exist."}
                }
            }
        },
        "/api/UserVerification": {
            "delete": {
                "tags": ["User verification documents"],
                "summary": "Delete verification document",
                "description": "Deletes a user's verification document from storage and the database. Anonymous controller action.",
                "security": [],
                "parameters": [
                    {"name": "userId", "in": "query", "required": True, "schema": {"type": "string", "format": "uuid"}},
                    {"name": "documentId", "in": "query", "required": True, "schema": {"type": "string", "format": "uuid"}}
                ],
                "responses": {
                    "200": {
                        "description": "Document deleted.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/ApiResponse"},
                                "example": {
                                    "success": True,
                                    "message": None,
                                    "errors": None,
                                    "statusCode": 200
                                }
                            }
                        }
                    },
                    "400": {"description": "Storage deletion failure or validation error."},
                    "404": {"description": "User or verification document not found."}
                }
            }
        },
        "/api/Health/ping": {
            "get": {
                "tags": ["Health"],
                "summary": "Ping",
                "description": "Returns a live operational marker. This is the only controller response that is not wrapped in ApiResponse<T>.",
                "security": [],
                "responses": {
                    "200": {
                        "description": "Ping successful.",
                        "content": {
                            "application/json": {
                                "schema": {"$ref": "#/components/schemas/PingResponse"},
                                "example": {
                                    "message": "Pong! Smart Court API is fully operational.",
                                    "serverTimeUtc": "2026-07-23T12:00:00Z",
                                    "version": "1.0.0"
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

def to_yaml(obj, indent=0):
    spaces = " " * indent
    if obj is None:
        return "null"
    elif isinstance(obj, bool):
        return "true" if obj else "false"
    elif isinstance(obj, (int, float)):
        return str(obj)
    elif isinstance(obj, str):
        if (
            "\n" in obj or ":" in obj or "#" in obj or "[" in obj or "]" in obj
            or "{" in obj or "}" in obj or "!" in obj or "*" in obj or "&" in obj
            or "?" in obj or "-" in obj or obj.strip() != obj or len(obj) == 0
        ):
            return json.dumps(obj, ensure_ascii=False)
        return obj
    elif isinstance(obj, list):
        if not obj:
            return "[]"
        res = []
        for item in obj:
            if isinstance(item, (dict, list)):
                res.append(f"{spaces}- " + to_yaml(item, indent + 2).lstrip())
            else:
                res.append(f"{spaces}- {to_yaml(item, indent + 2)}")
        return "\n".join(res)
    elif isinstance(obj, dict):
        if not obj:
            return "{}"
        res = []
        for k, v in obj.items():
            val_yaml = to_yaml(v, indent + 2)
            if isinstance(v, (dict, list)) and v:
                res.append(f"{spaces}{k}:\n{val_yaml}")
            else:
                res.append(f"{spaces}{k}: {val_yaml}")
        return "\n".join(res)
    return str(obj)

if __name__ == "__main__":
    json_path = r"P:\Projects\Smart Court\docs\smart_court_openapi.json"
    yaml_path = r"P:\Projects\Smart Court\docs\smart_court_openapi.yaml"
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(openapi_spec, f, indent=2, ensure_ascii=False)
    print("Saved JSON to:", json_path)
    with open(yaml_path, "w", encoding="utf-8") as f:
        f.write(to_yaml(openapi_spec))
    print("Saved YAML to:", yaml_path)
