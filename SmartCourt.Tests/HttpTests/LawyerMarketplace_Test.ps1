# LawyerMarketplace_Test.ps1
Add-Type -AssemblyName System.Net.Http
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\LawyerMarketplace_Report.md"

"# Lawyer Marketplace Search Endpoint Test Report`n`n" | Out-File $reportFile -Encoding utf8
"**Generated At:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" | Out-File $reportFile -Append -Encoding utf8
"**Target Endpoint:** `GET /api/lawyers/search``n`n---`n`n" | Out-File $reportFile -Append -Encoding utf8

# --- LOGIN AS SEEDED TEST CLIENT ---
Write-Host "Logging in as seeded Active Client user..." -ForegroundColor Cyan
$loginBodyClient = @{
    Email = "mkt_search_client@test.com"
    Password = "Password123!"
} | ConvertTo-Json

$loginResClient = Invoke-Api -title "0. Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile
if (-not $loginResClient -or -not $loginResClient.Data.AccessToken) {
    Write-Host "Failed to login as Client. Running SeedMarketplaceData.ps1 first..." -ForegroundColor Yellow
    powershell -ExecutionPolicy Bypass -File "$PSScriptRoot\SeedMarketplaceData.ps1"
    $loginResClient = Invoke-Api -title "0. Setup - Login Client (Retry)" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile
}

$clientToken = $loginResClient.Data.AccessToken
Write-Host "Client authenticated successfully. Token acquired." -ForegroundColor Green

# ==============================================================================
# HTTP TEST SCENARIOS FOR GET /api/lawyers/search
# ==============================================================================

# --- 1. DEFAULT SEARCH (ALL ACTIVE LAWYERS, PAGINATED) ---
Invoke-Api -title "1. Default Search - All Active Lawyers (No Query Params)" -method "GET" -endpoint "/api/lawyers/search" -token $clientToken -reportFile $reportFile | Out-Null

# --- 2. PAGINATION TESTS ---
Invoke-Api -title "2a. Pagination - PageNumber=1, PageSize=2" -method "GET" -endpoint "/api/lawyers/search?PageNumber=1&PageSize=2" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "2b. Pagination - PageNumber=2, PageSize=2" -method "GET" -endpoint "/api/lawyers/search?PageNumber=2&PageSize=2" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "2c. Pagination - PageSize Capping (PageSize=100 -> Capped to 50)" -method "GET" -endpoint "/api/lawyers/search?PageNumber=1&PageSize=100" -token $clientToken -reportFile $reportFile | Out-Null

# --- 3. SEARCH TERM FILTERING ---
Invoke-Api -title "3a. SearchTerm - Match by Name ('Ahmed')" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=Ahmed" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "3b. SearchTerm - Match by Bio ('commercial')" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=commercial" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "3c. SearchTerm - Non-matching ('NonExistentLawyer999')" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=NonExistentLawyer999" -token $clientToken -reportFile $reportFile | Out-Null

# --- 4. LEVEL FILTERING ---
Invoke-Api -title "4a. Level Filter - Level=4 (CassationCourt)" -method "GET" -endpoint "/api/lawyers/search?Level=4" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "4b. Level Filter - Level=3 (AppealCourt)" -method "GET" -endpoint "/api/lawyers/search?Level=3" -token $clientToken -reportFile $reportFile | Out-Null

# --- 5. SPECIALIZATION & GOVERNORATE FILTERING ---
Invoke-Api -title "5a. Specialization Filter - Specialization=2 (CommercialLaw)" -method "GET" -endpoint "/api/lawyers/search?Specialization=2" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "5b. Specialization Filter - Specialization=0 (FamilyLaw)" -method "GET" -endpoint "/api/lawyers/search?Specialization=0" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "5c. Governorate Filter - Governorate=Cairo" -method "GET" -endpoint "/api/lawyers/search?Governorate=Cairo" -token $clientToken -reportFile $reportFile | Out-Null

# --- 6. RATING & AVAILABILITY FILTERING ---
Invoke-Api -title "6a. MinRating Filter - MinRating=4.5" -method "GET" -endpoint "/api/lawyers/search?MinRating=4.5" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "6b. IsAvailable Filter - IsAvailable=true" -method "GET" -endpoint "/api/lawyers/search?IsAvailable=true" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "6c. IsAvailable Filter - IsAvailable=false" -method "GET" -endpoint "/api/lawyers/search?IsAvailable=false" -token $clientToken -reportFile $reportFile | Out-Null

# --- 7. SORTING TESTS ---
Invoke-Api -title "7a. Sorting - SortBy Rating Descending (SortBy=0, SortDirection=1)" -method "GET" -endpoint "/api/lawyers/search?SortBy=0&SortDirection=1" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "7b. Sorting - SortBy Rating Ascending (SortBy=0, SortDirection=0)" -method "GET" -endpoint "/api/lawyers/search?SortBy=0&SortDirection=0" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "7c. Sorting - SortBy ResponseTime Ascending (SortBy=1, SortDirection=0)" -method "GET" -endpoint "/api/lawyers/search?SortBy=1&SortDirection=0" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "7d. Sorting - SortBy ExperienceLevel Descending (SortBy=2, SortDirection=1)" -method "GET" -endpoint "/api/lawyers/search?SortBy=2&SortDirection=1" -token $clientToken -reportFile $reportFile | Out-Null

# --- 8. COMBINED MULTI-FIELD FILTERS ---
Invoke-Api -title "8. Multi-field Filter - Governorate=Cairo, Level=4, IsAvailable=true, MinRating=4.0" -method "GET" -endpoint "/api/lawyers/search?Governorate=Cairo&Level=4&IsAvailable=true&MinRating=4.0&SortBy=0&SortDirection=1" -token $clientToken -reportFile $reportFile | Out-Null

# --- 9. VALIDATION ERRORS (400 BAD REQUEST) ---
Invoke-Api -title "9a. Validation Error - Invalid MinRating (> 5.0)" -method "GET" -endpoint "/api/lawyers/search?MinRating=6.5" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9b. Validation Error - Invalid MinRating (< 0.0)" -method "GET" -endpoint "/api/lawyers/search?MinRating=-1.0" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9c. Validation Error - Invalid Level Enum" -method "GET" -endpoint "/api/lawyers/search?Level=99" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9d. Validation Error - Invalid Specialization Enum" -method "GET" -endpoint "/api/lawyers/search?Specialization=99" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9e. Validation Error - Invalid SortBy Enum" -method "GET" -endpoint "/api/lawyers/search?SortBy=99" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9f. Validation Error - Invalid SortDirection Enum" -method "GET" -endpoint "/api/lawyers/search?SortDirection=99" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9g. Validation Error - Invalid PageNumber (0)" -method "GET" -endpoint "/api/lawyers/search?PageNumber=0" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "9h. Validation Error - Invalid PageSize (0)" -method "GET" -endpoint "/api/lawyers/search?PageSize=0" -token $clientToken -reportFile $reportFile | Out-Null

# --- 10. SECURITY / AUTHORIZATION TESTS (401 UNAUTHORIZED) ---
Invoke-Api -title "10a. Unauthorized - Missing Authorization Header" -method "GET" -endpoint "/api/lawyers/search" -token "" -reportFile $reportFile | Out-Null
Invoke-Api -title "10b. Unauthorized - Invalid Token" -method "GET" -endpoint "/api/lawyers/search" -token "invalid_token_xyz" -reportFile $reportFile | Out-Null

# --- 11. STRESS & EDGE CASES ---
Invoke-Api -title "11a. Stress Test - SQL Injection Attempt in SearchTerm" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=' OR '1'='1" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "11b. Stress Test - XSS Payload in SearchTerm" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=<script>alert('xss')</script>" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "11c. Stress Test - Arabic & Emoji Unicode in SearchTerm" -method "GET" -endpoint "/api/lawyers/search?SearchTerm=مستشار ⚖️" -token $clientToken -reportFile $reportFile | Out-Null

Write-Host "Test execution complete! Detailed report written to $reportFile" -ForegroundColor Green
