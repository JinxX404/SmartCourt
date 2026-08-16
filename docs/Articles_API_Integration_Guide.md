# Articles API Contract and Frontend Integration Guide

**Code snapshot analyzed:** 2026-08-14  
**Primary route prefixes:** `/api/Articles`, `/api/ArticleCategories`  
**Audience:** Web/mobile frontend developers integrating the Articles (Blog/News) feature

> This guide describes the implementation in source code, including its current inconsistencies. It does not substitute intended product behavior for actual wire behavior.

## Wire-level conventions

| Concern | Actual behavior |
|---|---|
| Authentication | Some endpoints are Public. Protected endpoints require `Authorization: Bearer <JWT>` or use the application's `accessToken` HttpOnly cookie. The cookie wins if both are present because the JWT handler explicitly reads it. |
| Content type | Send `Content-Type: application/json` for endpoints with a body (except for `POST /api/Articles/lawyer` and `PUT /api/Articles/lawyer/{id}` which require `multipart/form-data` due to image uploads). Success and middleware-handled errors are JSON. |
| JSON naming | Response and request examples use `camelCase`. ASP.NET Core binding is case-insensitive, but frontend code should use the documented casing. |
| Enum encoding | **Enum-valued JSON fields are numbers**, because MVC has no `JsonStringEnumConverter`. Query-string enum binding may accept a defined name such as `Draft` or its numeric value; numeric values are safest against the current wire contract. |
| Dates | `DateTime` values serialize as ISO-8601 strings; stored timestamps are UTC and normally end in `Z`. Nullable dates are JSON `null` until the event occurs. |
| Nulls | Null response properties are not globally suppressed. Envelopes therefore normally include `message: null`, `errors: null`, and failed envelopes include `data: null`. |
| Error codes | There is **no machine-readable application error-code field**. HTTP status plus localized `message`/`errors` is the only implemented discriminator. Frontend logic must not depend on the Arabic prose when an HTTP status or current resource state can be used. |

### Standard success envelope

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

Creation uses `statusCode: 201`. The `data` shape is endpoint-specific.

### Middleware-handled error envelope

```json
{
  "success": false,
  "data": null,
  "message": "Localized or generic error message",
  "errors": null,
  "statusCode": 400
}
```

### Automatic binding/FluentValidation error shape

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["عنوان المقال مطلوب."]
  },
  "traceId": "00-..."
}
```

### Global HTTP error behavior

| HTTP status | Source | Response behavior / meaning |
|---:|---|---|
| `400 Bad Request` | FluentValidation, binding, or `BusinessException` | Validation problem details for automatic validation; otherwise custom failed envelope. Used for malformed data or failed business preconditions. |
| `401 Unauthorized` | Authorization framework | Missing/invalid/expired token is normally a framework 401 and may have an empty body. |
| `403 Forbidden` | Role policy | Wrong controller role is normally a framework 403 and may have an empty body. |
| `404 Not Found` | GUID route mismatch or absent route | Non-existent article, comment, or category. |
| `500 Internal Server Error` | Unhandled exception | Custom envelope with `message: "An internal server error occurred."` |

---

## 1. Complete Endpoint Catalog

### Endpoint overview

| Method | Exact route | Allowed controller roles | Success | Request body |
|---|---|---|---:|---|
| `GET` | `/api/ArticleCategories/public` | Public | `200` | None |
| `GET` | `/api/Articles/public` | Public | `200` | None |
| `GET` | `/api/Articles/public/{id}` | Public | `200` | None |
| `GET` | `/api/Articles/public/{id}/comments` | Public | `200` | None |
| `POST` | `/api/Articles/{id}/like` | Authenticated (Any) | `200` | None |
| `GET` | `/api/Articles/my-likes` | Authenticated (Any) | `200` | None |
| `GET` | `/api/Articles/{id}/likers` | Authenticated (Any) | `200` | None |
| `POST` | `/api/Articles/{id}/comments` | Authenticated (Any) | `201` | `CreateArticleCommentRequest` |
| `PUT` | `/api/Articles/{articleId}/comments/{commentId}` | Authenticated (Owner) | `200` | `UpdateArticleCommentRequest` |
| `DELETE` | `/api/Articles/{articleId}/comments/{commentId}` | Authenticated (Owner) | `200` | None |
| `POST` | `/api/Articles/{id}/report` | Authenticated (Any) | `200` | `ReportArticleRequest` |
| `POST` | `/api/Articles/lawyer` | Lawyer | `201` | `multipart/form-data` |
| `PUT` | `/api/Articles/lawyer/{id}` | Lawyer (Author) | `200` | `multipart/form-data` |
| `PUT` | `/api/Articles/lawyer/{id}/status` | Lawyer (Author) | `200` | `ChangeArticleStatusRequest` |
| `DELETE` | `/api/Articles/lawyer/{id}` | Lawyer (Author) | `200` | None |
| `GET` | `/api/Articles/lawyer/{id}` | Lawyer (Author) | `200` | None |
| `GET` | `/api/Articles/lawyer/drafts` | Lawyer | `200` | None |
| `GET` | `/api/Articles/lawyer/published` | Lawyer | `200` | None |
| `POST` | `/api/ArticleCategories/admin` | Admin | `201` | `CreateArticleCategoryRequest` |
| `PUT` | `/api/ArticleCategories/admin/{id}` | Admin | `200` | `UpdateArticleCategoryRequest` |
| `DELETE` | `/api/ArticleCategories/admin/{id}` | Admin | `200` | None |
| `GET` | `/api/Articles/admin/reported` | Admin | `200` | None |
| `PUT` | `/api/Articles/admin/reports/{reportId}/resolve` | Admin | `200` | None |
| `DELETE` | `/api/Articles/admin/{id}` | Admin | `200` | None |
| `GET` | `/api/Articles/admin/deleted-by-admin` | Admin | `200` | None |
| `GET` | `/api/Articles/admin/deleted-by-lawyer` | Admin | `200` | None |

---

### 1.1 Public Endpoints

#### View Categories
**HTTP Method & Exact Route:** `GET /api/ArticleCategories/public`  
**Purpose:** To populate category filters/dropdowns when searching for articles. Returns a list of active legal categories.  
**Response (`200 OK`):** `data` is an array of `ArticleCategoryDto`.

#### View Published Articles Feed
**HTTP Method & Exact Route:** `GET /api/Articles/public`  
**Purpose:** To display the main feed of published articles (e.g. the Blog homepage).  
**Query Parameters:** `pageNumber`, `pageSize`, `categoryId`, `authorId`, `searchQuery`.  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.

#### View Article Details
**HTTP Method & Exact Route:** `GET /api/Articles/public/{id}`  
**Purpose:** To view the full contents of an article. If an authenticated user token is passed, it ensures views are only counted once per user and returns an `isLikedByCurrentUser` boolean.  
**Response (`200 OK`):** `data` is `ArticleDto`.

#### View Article Comments
**HTTP Method & Exact Route:** `GET /api/Articles/public/{id}/comments`  
**Purpose:** To read comments on a specific article.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Response (`200 OK`):** `data` is a paginated list of `ArticleCommentDto`.

---

### 1.2 User Engagement Endpoints

#### Like/Unlike Article
**HTTP Method & Exact Route:** `POST /api/Articles/{id}/like`  
**Purpose:** To like or unlike an article (acts as a toggle).  
**Authentication:** Required (Any Role).  
**Response (`200 OK`):** `data` is `true`.

#### View My Liked Articles
**HTTP Method & Exact Route:** `GET /api/Articles/my-likes`  
**Purpose:** To show a user their "Liked Articles" in their personal profile.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Any Role).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.

#### View Article Likers
**HTTP Method & Exact Route:** `GET /api/Articles/{id}/likers`  
**Purpose:** To see a list of users who have liked a specific article.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Any Role).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleLikerDto`.

#### Add Comment
**HTTP Method & Exact Route:** `POST /api/Articles/{id}/comments`  
**Purpose:** To add a new comment to an article.  
**Request Body:** `application/json`
```json
{
  "Content": "Great Article!"
}
```
**Authentication:** Required (Any Role).  
**Response (`201 Created`):** `data` is the created `ArticleCommentDto`.

#### Edit Comment
**HTTP Method & Exact Route:** `PUT /api/Articles/{articleId}/comments/{commentId}`  
**Purpose:** To edit an existing comment (e.g., fixing a typo).  
**Request Body:** `application/json`
```json
{
  "Content": "Great Article! Updated."
}
```
**Authentication:** Required (Comment Owner).  
**Response (`200 OK`):** `data` is the updated `ArticleCommentDto`.

#### Delete Comment
**HTTP Method & Exact Route:** `DELETE /api/Articles/{articleId}/comments/{commentId}`  
**Purpose:** Soft-deletes the comment.  
**Authentication:** Required (Comment Owner).  
**Response (`200 OK`):** `data` is `true`.

#### Report Article
**HTTP Method & Exact Route:** `POST /api/Articles/{id}/report`  
**Purpose:** To flag an article for inappropriate content or legal inaccuracies.  
**Request Body:** `application/json`
```json
{
  "Reason": "Inappropriate content"
}
```
**Authentication:** Required (Any Role).  
**Response (`200 OK`):** `data` is `true`.

---

### 1.3 Lawyer / Author Endpoints

#### Create Draft/Published Article
**HTTP Method & Exact Route:** `POST /api/Articles/lawyer`  
**Purpose:** To create a new article. The lawyer can set `Status` to `Draft = 1` or `Published = 2` upon creation.  
**Request Body:** `multipart/form-data`
* `Title` (string)
* `Content` (string)
* `Tags` (string)
* `CategoryId` (GUID)
* `IsDraft` (boolean)
* `FeaturedImage` (IFormFile, optional)  
**Authentication:** Required (Lawyer).  
**Response (`201 Created`):** `data` is the created `ArticleDto`.

#### Update Article
**HTTP Method & Exact Route:** `PUT /api/Articles/lawyer/{id}`  
**Purpose:** To edit the title, content, image, or category of their article.  
**Request Body:** `multipart/form-data` (similar to Create)  
**Authentication:** Required (Lawyer Author).  
**Response (`200 OK`):** `data` is the updated `ArticleDto`.

#### Change Article Status
**HTTP Method & Exact Route:** `PUT /api/Articles/lawyer/{id}/status`  
**Purpose:** To quickly publish a draft or unpublish an article without sending the full update payload.  
**Request Body:** `application/json`
```json
{
  "Status": 2
}
```
**Authentication:** Required (Lawyer Author).  
**Response (`200 OK`):** `data` is the updated `ArticleDto`.

#### Delete Article
**HTTP Method & Exact Route:** `DELETE /api/Articles/lawyer/{id}`  
**Purpose:** Soft-deletes the article (`isDeleted = true`).  
**Authentication:** Required (Lawyer Author).  
**Response (`200 OK`):** `data` is `true`.

#### View Drafts Feed
**HTTP Method & Exact Route:** `GET /api/Articles/lawyer/drafts`  
**Purpose:** To populate the "Drafts" tab in the lawyer's dashboard.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Lawyer).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.

#### View Published Feed (Lawyer Dashboard)
**HTTP Method & Exact Route:** `GET /api/Articles/lawyer/published`  
**Purpose:** To populate the "Published" tab in the lawyer's dashboard.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Lawyer).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.

#### Preview Own Article
**HTTP Method & Exact Route:** `GET /api/Articles/lawyer/{id}`  
**Purpose:** To preview their own article layout, even if it is still a draft and not publicly visible.  
**Authentication:** Required (Lawyer Author).  
**Response (`200 OK`):** `data` is `ArticleDto`.

---

### 1.4 Admin Moderation Endpoints

#### Create Category
**HTTP Method & Exact Route:** `POST /api/ArticleCategories/admin`  
**Request Body:** `application/json`
```json
{
  "Code": "ARTCAT_123",
  "NameAr": "Test Category",
  "Description": "Category Description"
}
```
**Authentication:** Required (Admin).  
**Response (`201 Created`):** `data` is `ArticleCategoryDto`.

#### Update Category
**HTTP Method & Exact Route:** `PUT /api/ArticleCategories/admin/{id}`  
**Request Body:** `application/json`
```json
{
  "NameAr": "Updated Category",
  "Description": "Updated Description"
}
```
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is `ArticleCategoryDto`.

#### Delete Category
**HTTP Method & Exact Route:** `DELETE /api/ArticleCategories/admin/{id}`  
**Purpose:** Soft-deletes a category.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is `true`.

#### View Reported Articles
**HTTP Method & Exact Route:** `GET /api/Articles/admin/reported`  
**Purpose:** To review articles flagged by users. Returns a list of pending reports.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is a paginated list of `ReportedArticleDto`.

#### Resolve Report
**HTTP Method & Exact Route:** `PUT /api/Articles/admin/reports/{reportId}/resolve`  
**Purpose:** To close a report after investigation (Marks `IsResolved = true`).  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is `true`.

#### Delete Article (Force Remove)
**HTTP Method & Exact Route:** `DELETE /api/Articles/admin/{id}`  
**Purpose:** To forcefully remove a violating article. Sets `isDeletedByAdmin = true` on the article, overriding the author.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is `true`.

#### View Articles Deleted by Admin
**HTTP Method & Exact Route:** `GET /api/Articles/admin/deleted-by-admin`  
**Purpose:** To audit articles that were forcibly removed by moderators (e.g. recycling bin).  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.

#### View Articles Deleted by Lawyer
**HTTP Method & Exact Route:** `GET /api/Articles/admin/deleted-by-lawyer`  
**Purpose:** To audit articles that authors have deleted themselves.  
**Query Parameters:** `pageNumber`, `pageSize`.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is a paginated list of `ArticleDto`.
