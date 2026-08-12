# Articles_Tests.ps1
$ErrorActionPreference = 'Continue'
Set-StrictMode -Version Latest

# Load test helpers
$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$HelpersPath = Join-Path $PSScriptRoot "TestHelpers.psm1"
if (Test-Path $HelpersPath) {
    Import-Module $HelpersPath -Force
} else {
    Write-Warning "TestHelpers.psm1 not found. Some functions may be missing."
}

# Configuration
$ReportPath = Join-Path $PSScriptRoot "Articles_Report.md"

# Clear previous report
if (Test-Path $ReportPath) {
    Remove-Item $ReportPath
}

"# Articles Feature Test Report`n" | Out-File $ReportPath -Encoding utf8
"Run at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" | Out-File $ReportPath -Append -Encoding utf8

$randomNum = Get-Random

# --- 1. Login Accounts ---
$adminLoginBody = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$adminLoginRes = Invoke-Api -title "1a. Login Admin" -method "POST" -endpoint "/api/auth/login" -body $adminLoginBody -reportFile $ReportPath
$adminToken = $adminLoginRes.data.accessToken

$lawyerLoginBody = @{ Email = "lawyer@smartcourt.com"; Password = "Lawyer@123" } | ConvertTo-Json
$lawyerLoginRes = Invoke-Api -title "1b. Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $lawyerLoginBody -reportFile $ReportPath
$lawyerToken = $lawyerLoginRes.data.accessToken

$clientLoginBody = @{ Email = "client@smartcourt.com"; Password = "Client@123" } | ConvertTo-Json
$clientLoginRes = Invoke-Api -title "1c. Login Client" -method "POST" -endpoint "/api/auth/login" -body $clientLoginBody -reportFile $ReportPath
$clientToken = $clientLoginRes.data.accessToken

# --- 2. Admin creates Category ---
$categoryBody = @{ Code = "ARTCAT_${randomNum}"; NameAr = "Test Category ${randomNum}"; Description = "Category Description" } | ConvertTo-Json
$catRes = Invoke-Api -title "2a. Create Category (Admin)" -method "POST" -endpoint "/api/ArticleCategories/admin" -body $categoryBody -token $adminToken -reportFile $ReportPath
$categoryId = $catRes.data.id

# Admin updates Category
$catUpdateBody = @{ NameAr = "Updated Category ${randomNum}"; Description = "Updated Description" } | ConvertTo-Json
Invoke-Api -title "2b. Update Category (Admin)" -method "PUT" -endpoint "/api/ArticleCategories/admin/$categoryId" -body $catUpdateBody -token $adminToken -reportFile $ReportPath | Out-Null

# Public view categories
Invoke-Api -title "2c. Public View Categories" -method "GET" -endpoint "/api/ArticleCategories/public" -reportFile $ReportPath | Out-Null

# --- 3. Lawyer creates Article ---
$articleBody = @{
    Title = "Test Article ${randomNum}"
    Content = "Test Article Content long..."
    Tags = "Law,Test"
    CategoryId = $categoryId
    IsDraft = $true
} | ConvertTo-Json
$articleRes = Invoke-Api -title "3a. Create Draft Article (Lawyer)" -method "POST" -endpoint "/api/Articles/lawyer" -body $articleBody -token $lawyerToken -reportFile $ReportPath
$articleId = $articleRes.data.id

# Lawyer views drafts
Invoke-Api -title "3b. View Drafts (Lawyer)" -method "GET" -endpoint "/api/Articles/lawyer/drafts" -token $lawyerToken -reportFile $ReportPath | Out-Null

# Lawyer updates article (remains draft)
$articleUpdateBody = @{
    Title = "Updated Article ${randomNum}"
    Content = "Updated Article Content..."
    Tags = "Law,Test"
    CategoryId = $categoryId
    IsDraft = $true
} | ConvertTo-Json
Invoke-Api -title "3c. Update Article (Lawyer)" -method "PUT" -endpoint "/api/Articles/lawyer/$articleId" -body $articleUpdateBody -token $lawyerToken -reportFile $ReportPath | Out-Null

# Lawyer publishes article via Change Status
Invoke-Api -title "3d. Publish Article via Status Change" -method "PUT" -endpoint "/api/Articles/lawyer/$articleId/status" -token $lawyerToken -reportFile $ReportPath | Out-Null

# Lawyer views published
Invoke-Api -title "3e. View Published (Lawyer)" -method "GET" -endpoint "/api/Articles/lawyer/published" -token $lawyerToken -reportFile $ReportPath | Out-Null

# --- 4. Public & Client Engagement ---
# Client views published articles
Invoke-Api -title "4a. View All Published (Public)" -method "GET" -endpoint "/api/Articles/public" -reportFile $ReportPath | Out-Null

# Client views specific article
Invoke-Api -title "4b. View Article (Client Token)" -method "GET" -endpoint "/api/Articles/public/$articleId" -token $clientToken -reportFile $ReportPath | Out-Null

# Client likes article
Invoke-Api -title "4c. Like Article (Client)" -method "POST" -endpoint "/api/Articles/$articleId/like" -token $clientToken -reportFile $ReportPath | Out-Null

# Client comments on article
$commentBody = @{ Content = "Great Article!" } | ConvertTo-Json
$commentRes = Invoke-Api -title "4d. Comment on Article (Client)" -method "POST" -endpoint "/api/Articles/$articleId/comments" -body $commentBody -token $clientToken -reportFile $ReportPath
$commentId = $commentRes.data.id

# Client updates comment
$updateCommentBody = @{ Content = "Great Article! Updated." } | ConvertTo-Json
Invoke-Api -title "4e. Update Comment (Client)" -method "PUT" -endpoint "/api/Articles/$articleId/comments/$commentId" -body $updateCommentBody -token $clientToken -reportFile $ReportPath | Out-Null

# Client views comments paginated
Invoke-Api -title "4f. View Article Comments Paginated (Public)" -method "GET" -endpoint "/api/Articles/public/$articleId/comments?pageNumber=1&pageSize=10" -reportFile $ReportPath | Out-Null

# Client reports article
$reportArticleBody = @{ Reason = "Inappropriate content" } | ConvertTo-Json
Invoke-Api -title "4g. Report Article (Client)" -method "POST" -endpoint "/api/Articles/$articleId/report" -body $reportArticleBody -token $clientToken -reportFile $ReportPath | Out-Null

# Client checks My Likes
Invoke-Api -title "4h. View My Liked Articles (Client)" -method "GET" -endpoint "/api/Articles/my-likes" -token $clientToken -reportFile $ReportPath | Out-Null

# Client views specific article again to check IsLikedByCurrentUser
Invoke-Api -title "4i. View Article Check IsLiked (Client Token)" -method "GET" -endpoint "/api/Articles/public/$articleId" -token $clientToken -reportFile $ReportPath | Out-Null

# --- 5. Admin Moderation ---
# View Reports
$reportsRes = Invoke-Api -title "5a. View Reported Articles (Admin)" -method "GET" -endpoint "/api/Articles/admin/reported" -token $adminToken -reportFile $ReportPath
$reportId = $null
if ($reportsRes.data -and $reportsRes.data.Length -gt 0) {
    $reportId = $reportsRes.data[0].id
}

# Resolve Report
if ($reportId) {
    Invoke-Api -title "5b. Resolve Report (Admin)" -method "PUT" -endpoint "/api/Articles/admin/reports/$reportId/resolve" -token $adminToken -reportFile $ReportPath | Out-Null
}

# Admin deletes article
Invoke-Api -title "5c. Admin Delete Article" -method "DELETE" -endpoint "/api/Articles/admin/$articleId" -token $adminToken -reportFile $ReportPath | Out-Null

# Admin views admin deleted articles
Invoke-Api -title "5d. View Admin Deleted Articles" -method "GET" -endpoint "/api/Articles/admin/deleted-by-admin" -token $adminToken -reportFile $ReportPath | Out-Null

# --- 6. Clean up Category (Should succeed because article is deleted/soft-deleted so no constraints, or fail if soft-deleted counts. Let's see.) ---
Invoke-Api -title "6a. Delete Category (Admin)" -method "DELETE" -endpoint "/api/ArticleCategories/admin/$categoryId" -token $adminToken -reportFile $ReportPath | Out-Null

Write-Host "Articles tests completed. Report generated at $ReportPath"
