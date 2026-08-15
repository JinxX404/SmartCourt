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
# Admin
$adminLoginBody = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$adminLoginRes = Invoke-Api -title "1a. Login Admin" -method "POST" -endpoint "/api/auth/login" -body $adminLoginBody -reportFile $ReportPath
$adminToken = $adminLoginRes.data.accessToken

# Lawyer (Verified)
$lawyerLoginBody = @{ Email = "lawyer@smartcourt.com"; Password = "Lawyer@123" } | ConvertTo-Json
$lawyerLoginRes = Invoke-Api -title "1b. Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $lawyerLoginBody -reportFile $ReportPath
$lawyerToken = $lawyerLoginRes.data.accessToken

# Client
$clientLoginBody = @{ Email = "client@smartcourt.com"; Password = "Client@123" } | ConvertTo-Json
$clientLoginRes = Invoke-Api -title "1c. Login Client" -method "POST" -endpoint "/api/auth/login" -body $clientLoginBody -reportFile $ReportPath
$clientToken = $clientLoginRes.data.accessToken

# --- 2. Article Categories ---
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
}
$articleRes = Invoke-Api -title "3a. Create Draft Article (Lawyer)" -method "POST" -endpoint "/api/Articles/lawyer" -body $articleBody -token $lawyerToken -reportFile $ReportPath -contentType ""
$articleId = $articleRes.data.id

# Publish article via Status Change
Invoke-Api -title "3b. Publish Article via Status Change" -method "PUT" -endpoint "/api/Articles/lawyer/$articleId/status" -token $lawyerToken -reportFile $ReportPath | Out-Null


# --- 4. Validation & Edge Cases Testing ---

# 4.1 Create Comment (Negative tests)
$emptyCommentBody = @{ Content = "" } | ConvertTo-Json
Invoke-Api -title "4.1a Create Comment - Empty string (Expected 400)" -method "POST" -endpoint "/api/Articles/$articleId/comments" -body $emptyCommentBody -token $clientToken -reportFile $ReportPath | Out-Null

$longString = "a" * 1005
$longCommentBody = @{ Content = $longString } | ConvertTo-Json
Invoke-Api -title "4.1b Create Comment - Over 1000 chars (Expected 400)" -method "POST" -endpoint "/api/Articles/$articleId/comments" -body $longCommentBody -token $clientToken -reportFile $ReportPath | Out-Null

$xssCommentBody = @{ Content = "<script>alert('XSS')</script>" } | ConvertTo-Json
$xssCommentRes = Invoke-Api -title "4.1c Create Comment - XSS attempt" -method "POST" -endpoint "/api/Articles/$articleId/comments" -body $xssCommentBody -token $clientToken -reportFile $ReportPath
$xssCommentId = $xssCommentRes.data.id

# 4.2 Update Comment (Negative tests)
$emptyUpdateCommentBody = @{ Content = "" } | ConvertTo-Json
Invoke-Api -title "4.2a Update Comment - Empty string (Expected 400)" -method "PUT" -endpoint "/api/Articles/$articleId/comments/$xssCommentId" -body $emptyUpdateCommentBody -token $clientToken -reportFile $ReportPath | Out-Null


# 4.3 Report Article (Negative tests)
$emptyReportBody = @{ Reason = "" } | ConvertTo-Json
Invoke-Api -title "4.3a Report Article - Empty Reason (Expected 400)" -method "POST" -endpoint "/api/Articles/$articleId/report" -body $emptyReportBody -token $clientToken -reportFile $ReportPath | Out-Null

$validReportBody = @{ Reason = "Inappropriate content, please review." } | ConvertTo-Json
Invoke-Api -title "4.3b Report Article - Valid (Client)" -method "POST" -endpoint "/api/Articles/$articleId/report" -body $validReportBody -token $clientToken -reportFile $ReportPath | Out-Null

Invoke-Api -title "4.3c Report Article - Duplicate Report (Expected 400)" -method "POST" -endpoint "/api/Articles/$articleId/report" -body $validReportBody -token $clientToken -reportFile $ReportPath | Out-Null

# Self Report Check
Invoke-Api -title "4.3d Report Article - Author Self-Report (Expected 400)" -method "POST" -endpoint "/api/Articles/$articleId/report" -body $validReportBody -token $lawyerToken -reportFile $ReportPath | Out-Null


# 4.4 Likes 
# Client likes article
Invoke-Api -title "4.4a Like Article (Client)" -method "POST" -endpoint "/api/Articles/$articleId/like" -token $clientToken -reportFile $ReportPath | Out-Null

# Client unlikes article (toggles)
Invoke-Api -title "4.4b Unlike Article (Client) - Should not go below 0" -method "POST" -endpoint "/api/Articles/$articleId/like" -token $clientToken -reportFile $ReportPath | Out-Null

# Client views likers
Invoke-Api -title "4.4c View Article Likers (Client)" -method "GET" -endpoint "/api/Articles/$articleId/likers?pageNumber=1&pageSize=10" -token $clientToken -reportFile $ReportPath | Out-Null


# --- 5. Admin Moderation & Admin Delete ---
# Admin deletes article
Invoke-Api -title "5a. Admin Delete Article" -method "DELETE" -endpoint "/api/Articles/admin/$articleId" -token $adminToken -reportFile $ReportPath | Out-Null

# Lawyer tries to update admin deleted article (Expected 404 or 403/Forbidden depending on handling)
$articleUpdateBody = @{ Title = "Hacked Article"; Content = "Attempt to bypass admin deletion"; CategoryId = $categoryId; IsDraft = $false }
Invoke-Api -title "5b. Update Admin-Deleted Article (Lawyer) (Expected 404)" -method "PUT" -endpoint "/api/Articles/lawyer/$articleId" -body $articleUpdateBody -token $lawyerToken -reportFile $ReportPath -contentType "" | Out-Null


Write-Host "Articles tests completed. Report generated at $ReportPath"
