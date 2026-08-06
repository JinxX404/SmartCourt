# AdminVerifications_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force
Add-Type -AssemblyName System.Net.Http

$reportFile = "$PSScriptRoot\AdminVerifications_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"

# Ensure log directory/file exists (or we'll fail to read it)
if (!(Test-Path $apiLogPath)) {
    New-Item -Path $apiLogPath -ItemType File -Force | Out-Null
}

"# Admin Verifications Slice Test Report`n`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random -Maximum 999999999
$adminEmail = "moatazmohammed2392003@gmail.com"
$adminPassword = "Admin@123"

$lawyerEmail = "lawyer_verification_${randomNum}@test.com"
$lawyerPassword = "Password123!"

# --- SETUP: ADMIN LOGIN ---
$loginBodyAdmin = @{
    Email = $adminEmail
    Password = $adminPassword
} | ConvertTo-Json

$loginResAdmin = Invoke-Api -title "0a. Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
if (-not $loginResAdmin -or -not $loginResAdmin.Data.AccessToken) {
    Write-Host "Failed to login as Admin. Ensure the database is seeded with admin@smartcourt.com."
    exit
}
$adminToken = $loginResAdmin.Data.AccessToken

# --- SETUP: REGISTER & LOGIN LAWYER ---
$bodyLawyer = @{
    Email = $lawyerEmail
    FullName = "Lawyer Verification"
    Password = $lawyerPassword
    ConfirmPassword = $lawyerPassword
} | ConvertTo-Json

$regLawyerRes = Invoke-Api -title "0b. Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $bodyLawyer -reportFile $reportFile
if (-not $regLawyerRes -or -not $regLawyerRes.Data.UserId) {
    Write-Host "Failed to register Lawyer."
    exit
}
$lawyerId = $regLawyerRes.Data.UserId

# Confirm Email
Start-Sleep -Seconds 2
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginBodyLawyer = @{
    Email = $lawyerEmail
    Password = $lawyerPassword
} | ConvertTo-Json

$loginResLawyer = Invoke-Api -title "0c. Setup - Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginBodyLawyer -reportFile $reportFile
if (-not $loginResLawyer -or -not $loginResLawyer.Data.AccessToken) {
    Write-Host "Failed to login as Lawyer."
    exit
}
$lawyerToken = $loginResLawyer.Data.AccessToken

# --- SETUP: LAWYER UPLOADS DOCUMENT ---
$tempFilePath = "$PSScriptRoot\dummy_admin_id.jpg"
[System.IO.File]::WriteAllBytes($tempFilePath, [byte[]]::new(1024))

$formValid = @{
    UserId = $lawyerId
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
# Using Invoke-MultipartApi requires setting it up inline if it's not exported. Let's just define it here since it was in UserVerification_Test.ps1.
function Invoke-MultipartApiLocal {
    param (
        [string]$title,
        [string]$endpoint,
        [string]$token,
        [hashtable]$form,
        [string]$fileField,
        [string]$filePath
    )
    $url = "http://localhost:5049" + $endpoint
    
    "### $title`n" | Out-File $reportFile -Append -Encoding utf8
    "**Request:** POST $url (Multipart Form Data)`n" | Out-File $reportFile -Append -Encoding utf8
    "**Form Data:**" | Out-File $reportFile -Append -Encoding utf8
    foreach ($key in $form.Keys) {
        "- $key = $($form[$key])" | Out-File $reportFile -Append -Encoding utf8
    }
    "- $fileField = [File: $filePath]`n`n" | Out-File $reportFile -Append -Encoding utf8

    $boundary = [System.Guid]::NewGuid().ToString()
    $multipartContent = [System.Net.Http.MultipartFormDataContent]::new($boundary)

    foreach ($key in $form.Keys) {
        $stringContent = [System.Net.Http.StringContent]::new($form[$key])
        $multipartContent.Add($stringContent, $key)
    }

    $fileBytes = [System.IO.File]::ReadAllBytes($filePath)
    $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
    $multipartContent.Add($fileContent, $fileField, [System.IO.Path]::GetFileName($filePath))

    $httpClient = [System.Net.Http.HttpClient]::new()
    if ($token) {
        $httpClient.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $token)
    }

    try {
        $response = $httpClient.PostAsync($url, $multipartContent).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        
        "**Response Status:** $([int]$response.StatusCode)`n" | Out-File $reportFile -Append -Encoding utf8
        "**Response Body:**`n``````json`n$responseBody`n``````n---`n`n" | Out-File $reportFile -Append -Encoding utf8
        
        return $responseBody | ConvertFrom-Json
    } catch {
        $ex = $_.Exception
        "**Exception:** $($ex.Message)`n---`n`n" | Out-File $reportFile -Append -Encoding utf8
        return $null
    } finally {
        $httpClient.Dispose()
        $multipartContent.Dispose()
    }
}

$uploadRes = Invoke-MultipartApiLocal -title "0d. Setup - Lawyer Uploads Document" -endpoint "/api/UserVerification/submit-verification-documents" -token $lawyerToken -form $formValid -fileField "Documents[0].File" -filePath $tempFilePath

# --- 1. ADMIN GET PENDING VERIFICATIONS ---
Invoke-Api -title "1. Get Pending Verifications (Admin)" -method "GET" -endpoint "/api/admin/verifications?PageNumber=1&PageSize=10" -token $adminToken -reportFile $reportFile | Out-Null

# --- 2. ADMIN GET VERIFICATION DETAILS ---
$detailsRes = Invoke-Api -title "2. Get Verification Details (Admin)" -method "GET" -endpoint "/api/admin/verifications/$lawyerId" -token $adminToken -reportFile $reportFile
$documentId = $null
if ($detailsRes -and $detailsRes.Data -and $detailsRes.Data.Documents -and $detailsRes.Data.Documents.Length -gt 0) {
    $documentId = $detailsRes.Data.Documents[0].DocumentId
}

# --- 3. ADMIN GET DOCUMENT CONTENT ---
if ($documentId) {
    Invoke-Api -title "3. Get Document Content (Admin)" -method "GET" -endpoint "/api/admin/verifications/documents/$documentId/content" -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 4. ADMIN REJECT DOCUMENT ---
if ($documentId) {
    $rejectBody = @{
        Decision = 2
        RejectionReason = "Image is too blurry."
    } | ConvertTo-Json
    Invoke-Api -title "4. Review Verification Document - Reject" -method "PATCH" -endpoint "/api/admin/verifications/documents/$documentId" -body $rejectBody -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 5. LAWYER RE-UPLOADS DOCUMENT ---
$formValid2 = @{
    UserId = $lawyerId
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "2" # Upload a different type to bypass uniqueness check if needed
}
$uploadRes2 = Invoke-MultipartApiLocal -title "5a. Setup - Lawyer Re-uploads Document" -endpoint "/api/UserVerification/submit-verification-documents" -token $lawyerToken -form $formValid2 -fileField "Documents[0].File" -filePath $tempFilePath

$detailsRes2 = Invoke-Api -title "5b. Setup - Get Verification Details Again" -method "GET" -endpoint "/api/admin/verifications/$lawyerId" -token $adminToken -reportFile $reportFile
$documentId2 = $null
if ($detailsRes2 -and $detailsRes2.Data -and $detailsRes2.Data.Documents) {
    $doc = $detailsRes2.Data.Documents | Where-Object { $_.DocumentType -eq 'NationalIdBack' }
    if ($doc) {
        $documentId2 = $doc.DocumentId
    }
}

# --- 6. ADMIN APPROVE DOCUMENT ---
if ($documentId2) {
    $approveBody = @{
        Decision = 1
        RejectionReason = $null
    } | ConvertTo-Json
    Invoke-Api -title "6. Review Verification Document - Approve" -method "PATCH" -endpoint "/api/admin/verifications/documents/$documentId2" -body $approveBody -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 7. UNAUTHORIZED ACCESS ---
# Attempt to access admin endpoint with lawyer token
Invoke-Api -title "7. Unauthorized Access (Lawyer Token)" -method "GET" -endpoint "/api/admin/verifications" -token $lawyerToken -reportFile $reportFile | Out-Null

# --- 8. LAWYER DELETES REJECTED DOCUMENT ---
if ($documentId) {
    Invoke-Api -title "8. Lawyer Deletes Rejected Document" -method "DELETE" -endpoint "/api/UserVerification?UserId=$lawyerId&DocumentId=$documentId" -token $lawyerToken -reportFile $reportFile | Out-Null
}

# --- 9. LAWYER GETS OWN DOCUMENTS (EXPECT FULLY VERIFIED) ---
Invoke-Api -title "9. Get User Documents After Approval" -method "GET" -endpoint "/api/UserVerification/$lawyerId" -token $lawyerToken -reportFile $reportFile | Out-Null

# --- 10. GET LIST INVALID PAGINATION ---
Invoke-Api -title "10. Read - GET list invalid pagination" -method "GET" -endpoint "/api/admin/verifications?PageNumber=0&PageSize=-1" -token $adminToken -reportFile $reportFile | Out-Null

# --- 11. GET LIST MISSING TOKEN ---
Invoke-Api -title "11. Read - GET list missing token" -method "GET" -endpoint "/api/admin/verifications?PageNumber=1&PageSize=10" -token "" -reportFile $reportFile | Out-Null

# --- 12. GET DETAILS MALFORMED GUID ---
Invoke-Api -title "12. Read - GET details malformed Guid" -method "GET" -endpoint "/api/admin/verifications/not-a-guid" -token $adminToken -reportFile $reportFile | Out-Null

# --- 13. GET DETAILS NON-EXISTENT LAWYER ID ---
Invoke-Api -title "13. Read - GET details non-existent LawyerId" -method "GET" -endpoint "/api/admin/verifications/$([guid]::NewGuid())" -token $adminToken -reportFile $reportFile | Out-Null

# --- 14. GET DETAILS FOR CLIENT USER ---
# Register a client quickly
$clientEmail = "client_${randomNum}@test.com"
$bodyClient = @{ Email = $clientEmail; FullName = "Client Verification"; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
$regClientRes = Invoke-Api -title "14a. Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $bodyClient -reportFile $reportFile
$clientId = ""
if ($regClientRes -and $regClientRes.Data.UserId) { $clientId = $regClientRes.Data.UserId }
if ($clientId) {
    Invoke-Api -title "14b. Read - GET details for Client user" -method "GET" -endpoint "/api/admin/verifications/$clientId" -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 15. GET CONTENT NON-EXISTENT DOCUMENT ID ---
Invoke-Api -title "15. Read - GET content non-existent DocumentId" -method "GET" -endpoint "/api/admin/verifications/documents/$([guid]::NewGuid())/content" -token $adminToken -reportFile $reportFile | Out-Null

# --- 16. PATCH REJECT WITHOUT REASON ---
if ($documentId2) {
    $patch16 = @{ Decision = 2; RejectionReason = "" } | ConvertTo-Json
    Invoke-Api -title "16. Update - PATCH Reject without Reason" -method "PATCH" -endpoint "/api/admin/verifications/documents/$documentId2" -body $patch16 -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 17. PATCH APPROVE WITH REASON ---
if ($documentId2) {
    $patch17 = @{ Decision = 1; RejectionReason = "This should fail because you can't have a reason for approve" } | ConvertTo-Json
    Invoke-Api -title "17. Update - PATCH Approve with Reason" -method "PATCH" -endpoint "/api/admin/verifications/documents/$documentId2" -body $patch17 -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 18. PATCH INVALID DECISION ENUM ---
if ($documentId2) {
    $patch18 = @{ Decision = 99; RejectionReason = $null } | ConvertTo-Json
    Invoke-Api -title "18. Update - PATCH invalid Decision enum" -method "PATCH" -endpoint "/api/admin/verifications/documents/$documentId2" -body $patch18 -token $adminToken -reportFile $reportFile | Out-Null
}

# --- 19. PATCH NON-EXISTENT DOCUMENT ID ---
$patch19 = @{ Decision = 1; RejectionReason = $null } | ConvertTo-Json
Invoke-Api -title "19. Update - PATCH non-existent DocumentId" -method "PATCH" -endpoint "/api/admin/verifications/documents/$([guid]::NewGuid())" -body $patch19 -token $adminToken -reportFile $reportFile | Out-Null

if (Test-Path $tempFilePath) { Remove-Item $tempFilePath -Force }

"Tests complete. Results saved to $reportFile`n" | Write-Host
