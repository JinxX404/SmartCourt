# Articles API Integration Guide

This guide is designed for Frontend Developers. It lists all endpoints related to the Articles feature, detailing their purpose, usage, and required roles.

---

## 1. Public Endpoints
*Used by any user (authenticated or unauthenticated) visiting the platform.*

### 1.1 Categories
* **`GET /api/ArticleCategories/public`**
  * **Role**: Public
  * **Why**: To populate category filters/dropdowns when searching for articles.
  * **Details**: Returns a list of active legal categories.

### 1.2 View Articles
* **`GET /api/Articles/public`**
  * **Role**: Public
  * **Why**: To display the main feed of published articles (e.g. the Blog homepage).
  * **Details**: Returns a paginated list of published articles. Accepts `pageNumber`, `pageSize`, `categoryId`, `authorId`, and `searchQuery` as query parameters.
* **`GET /api/Articles/public/{id}`**
  * **Role**: Public (Optional Auth: sending bearer token is highly recommended).
  * **Why**: To view the full contents of an article.
  * **Details**: Tracks `ViewCount`. If an authenticated user token is passed, it ensures views are only counted once per user. Returns an `isLikedByCurrentUser` boolean if authenticated.
* **`GET /api/Articles/public/{id}/comments`**
  * **Role**: Public
  * **Why**: To read comments on a specific article.
  * **Details**: Returns a paginated list (`pageNumber`, `pageSize`) of active comments.

---

## 2. User Engagement Endpoints
*Used by any authenticated user (Clients, Lawyers) interacting with published content.*

### 2.1 Liking & Reading
* **`POST /api/Articles/{id}/like`**
  * **Role**: Authenticated (Any)
  * **Why**: To like or unlike an article.
  * **Details**: Acts as a toggle. Increments `LikesCount` if not liked, decrements if already liked.
* **`GET /api/Articles/my-likes`**
  * **Role**: Authenticated (Any)
  * **Why**: To show a user their "Liked Articles" in their personal profile.
  * **Details**: Returns a paginated list of published articles that the current user has liked.

### 2.2 Commenting
* **`POST /api/Articles/{id}/comments`**
  * **Role**: Authenticated (Any)
  * **Why**: To add a new comment to an article.
  * **Details**: Body requires a simple `Content` string.
* **`PUT /api/Articles/{articleId}/comments/{commentId}`**
  * **Role**: Authenticated (Comment Owner)
  * **Why**: To edit an existing comment (e.g., fixing a typo).
  * **Details**: Will fail if the user is not the author of the comment.
* **`DELETE /api/Articles/{articleId}/comments/{commentId}`**
  * **Role**: Authenticated (Comment Owner)
  * **Why**: To remove a comment.
  * **Details**: Soft-deletes the comment. Will fail if the user is not the author.

### 2.3 Reporting
* **`POST /api/Articles/{id}/report`**
  * **Role**: Authenticated (Any)
  * **Why**: To flag an article for inappropriate content or legal inaccuracies.
  * **Details**: Creates a report for admins to review.

---

## 3. Lawyer / Author Endpoints
*Used exclusively by users with the Lawyer role to manage their own content.*

### 3.1 Content Creation & Management
* **`POST /api/Articles/lawyer`**
  * **Role**: Lawyer
  * **Why**: To create a new article.
  * **Details**: The lawyer can set `Status` to `Draft = 1` or `Published = 2` upon creation.
* **`PUT /api/Articles/lawyer/{id}`**
  * **Role**: Lawyer
  * **Why**: To edit the title, content, image, or category of their article.
* **`PUT /api/Articles/lawyer/{id}/status`**
  * **Role**: Lawyer
  * **Why**: To quickly publish a draft or unpublish an article without sending the full update payload.
  * **Details**: Changes the `Status` property directly.
* **`DELETE /api/Articles/lawyer/{id}`**
  * **Role**: Lawyer (Author)
  * **Why**: To remove their article from the platform.
  * **Details**: Soft-deletes the article (`isDeleted = true`).

### 3.2 Dashboard Feeds
* **`GET /api/Articles/lawyer/{id}`**
  * **Role**: Lawyer (Author)
  * **Why**: To preview their own article layout, even if it is still a draft and not publicly visible.
* **`GET /api/Articles/lawyer/drafts`**
  * **Role**: Lawyer
  * **Why**: To populate the "Drafts" tab in the lawyer's dashboard.
  * **Details**: Paginated.
* **`GET /api/Articles/lawyer/published`**
  * **Role**: Lawyer
  * **Why**: To populate the "Published" tab in the lawyer's dashboard.
  * **Details**: Paginated.

---

## 4. Admin Moderation Endpoints
*Used exclusively by users with the Admin role for moderation and taxonomy management.*

### 4.1 Taxonomy Management
* **`POST /api/ArticleCategories/admin`**
* **`PUT /api/ArticleCategories/admin/{id}`**
* **`DELETE /api/ArticleCategories/admin/{id}`**
  * **Role**: Admin
  * **Why**: To add, update, or soft-delete legal article categories.

### 4.2 Moderation Queues
* **`GET /api/Articles/admin/reported`**
  * **Role**: Admin
  * **Why**: To review articles flagged by users.
  * **Details**: Returns a list of pending reports.
* **`PUT /api/Articles/admin/reports/{reportId}/resolve`**
  * **Role**: Admin
  * **Why**: To close a report after investigation.
  * **Details**: Marks the report as `IsResolved = true`.
* **`DELETE /api/Articles/admin/{id}`**
  * **Role**: Admin
  * **Why**: To forcefully remove a violating article.
  * **Details**: Sets `isDeletedByAdmin = true` on the article, overriding the author.

### 4.3 Auditing
* **`GET /api/Articles/admin/deleted-by-admin`**
  * **Role**: Admin
  * **Why**: To audit articles that were forcibly removed by moderators (e.g. recycling bin).
* **`GET /api/Articles/admin/deleted-by-lawyer`**
  * **Role**: Admin
  * **Why**: To audit articles that authors have deleted themselves.
