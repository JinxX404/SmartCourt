# CrossSlice_E2E_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force
Add-Type -AssemblyName System.Net.Http

$reportFile = "$PSScriptRoot\CrossSlice_E2E_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"

# Ensure log directory/file exists (or we'll fail to read it)
if (!(Test-Path $apiLogPath)) {
    New-Item -Path $apiLogPath -ItemType File -Force | Out-Null
}

Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Cross-Slice End-to-End Integration Test Report`n`n" | Out-File $reportFile -Encoding utf8
"This report covers comprehensive cross-slice workflows between Auth, Users, UserVerification, and AdminVerification.`n`n" | Out-File $reportFile -Append -Encoding utf8

$randomNum = Get-Random -Maximum 999999999
$adminEmail = "moatazmohammed2392003@gmail.com"
$adminPassword = "Admin@123"

$lawyerEmail = "lawyer_e2e_${randomNum}@test.com"
$clientEmail = "client_e2e_${randomNum}@test.com"
$password = "Password123!"

# --- Helper for Multipart Form Data ---
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
    "**Form Data:**`n" | Out-File $reportFile -Append -Encoding utf8
    foreach ($key in $form.Keys) {
        "- $key = $($form[$key])`n" | Out-File $reportFile -Append -Encoding utf8
    }
    if ($fileField -and $filePath) {
        "- $fileField = [File: $filePath]`n`n" | Out-File $reportFile -Append -Encoding utf8
    }

    $boundary = [System.Guid]::NewGuid().ToString()
    $multipartContent = [System.Net.Http.MultipartFormDataContent]::new($boundary)

    foreach ($key in $form.Keys) {
        $stringContent = [System.Net.Http.StringContent]::new($form[$key])
        $multipartContent.Add($stringContent, $key)
    }

    if ($fileField -and (Test-Path $filePath)) {
        $fileBytes = [System.IO.File]::ReadAllBytes($filePath)
        $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
        $multipartContent.Add($fileContent, $fileField, [System.IO.Path]::GetFileName($filePath))
    }

    $httpClient = [System.Net.Http.HttpClient]::new()
    if ($token) {
        $httpClient.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $token)
    }

    try {
        $response = $httpClient.PostAsync($url, $multipartContent).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        
        "**Response Status:** $([int]$response.StatusCode)`n" | Out-File $reportFile -Append -Encoding utf8
        if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
            "**Response Body:**`n``````json`n$responseBody`n``````n---`n`n" | Out-File $reportFile -Append -Encoding utf8
        } else {
            "**Response Body:** (Empty)`n---`n`n" | Out-File $reportFile -Append -Encoding utf8
        }
        
        return $responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue
    } catch {
        $ex = $_.Exception
        "**Exception:** $($ex.Message)`n---`n`n" | Out-File $reportFile -Append -Encoding utf8
        return $null
    } finally {
        $httpClient.Dispose()
        $multipartContent.Dispose()
    }
}

# ---------------------------------------------------------
# STAGE 1: AUTHENTICATION & SETUP
# ---------------------------------------------------------

# 1. Admin Login
$loginBodyAdmin = @{ Email = $adminEmail; Password = $adminPassword } | ConvertTo-Json
$loginResAdmin = Invoke-Api -title "1a. Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
$adminToken = $loginResAdmin.Data.AccessToken

# 2. Register & Login Lawyer
$bodyLawyer = @{ Email = $lawyerEmail; FullName = "Lawyer E2E"; Password = $password; ConfirmPassword = $password } | ConvertTo-Json
$regLawyerRes = Invoke-Api -title "1b. Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $bodyLawyer -reportFile $reportFile
$lawyerId = $regLawyerRes.Data.UserId
Start-Sleep -Seconds 2
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginBodyLawyer = @{ Email = $lawyerEmail; Password = $password } | ConvertTo-Json
$loginResLawyer = Invoke-Api -title "1c. Setup - Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginBodyLawyer -reportFile $reportFile
$lawyerToken = $loginResLawyer.Data.AccessToken

# 3. Register & Login Client
$bodyClient = @{ Email = $clientEmail; FullName = "Client E2E"; Password = $password; ConfirmPassword = $password } | ConvertTo-Json
$regClientRes = Invoke-Api -title "1d. Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $bodyClient -reportFile $reportFile
$clientId = $regClientRes.Data.UserId
Start-Sleep -Seconds 2
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginBodyClient = @{ Email = $clientEmail; Password = $password } | ConvertTo-Json
$loginResClient = Invoke-Api -title "1e. Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile
$clientToken = $loginResClient.Data.AccessToken

# ---------------------------------------------------------
# STAGE 2: USER PROFILES
# ---------------------------------------------------------

# Client Profile
$clientProfileCompleteBody = @{ PhoneNumber = "+201012345678"; Gender = 1; DateOfBirth = "1990-01-01"; Address = "Riyadh" } | ConvertTo-Json
Invoke-Api -title "2a. Profile - Complete Client Profile (Valid)" -method "POST" -endpoint "/api/clients/profile/complete" -body $clientProfileCompleteBody -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "2b. Profile - Get Client Profile" -method "GET" -endpoint "/api/clients/profile" -token $clientToken -reportFile $reportFile | Out-Null

$clientProfileUpdateBody = @{ FullName = "Client E2E Updated"; PhoneNumber = "+201012345679"; Gender = 1; DateOfBirth = "1990-01-02"; Address = "Jeddah" } | ConvertTo-Json
Invoke-Api -title "2c. Profile - Update Client Profile (Valid)" -method "PUT" -endpoint "/api/clients/profile" -body $clientProfileUpdateBody -token $clientToken -reportFile $reportFile | Out-Null

$loginResClient2 = Invoke-Api -title "2c2. Setup - Re-Login Client (After Profile Update)" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile
if ($loginResClient2 -and $loginResClient2.Data -and $loginResClient2.Data.AccessToken) { $clientToken = $loginResClient2.Data.AccessToken }
# Lawyer Profile
$randomNat = "2" + (Get-Random -Minimum 1000000000000 -Maximum 9999999999999).ToString()
$lawyerProfileCompleteBody = @{ PhoneNumber = "+201098765432"; NationalNumber = $randomNat; Gender = 1; DateOfBirth = "1985-05-05"; Level = 1; Bio = "Expert"; Address = "Riyadh" } | ConvertTo-Json
Invoke-Api -title "2d. Profile - Complete Lawyer Profile (Valid)" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $lawyerProfileCompleteBody -token $lawyerToken -reportFile $reportFile | Out-Null
Invoke-Api -title "2e. Profile - Get Lawyer Profile" -method "GET" -endpoint "/api/lawyers/profile" -token $lawyerToken -reportFile $reportFile | Out-Null

$lawyerProfileUpdateBody = @{ FullName = "Lawyer E2E Updated"; PhoneNumber = "+201098765433"; NationalNumber = $randomNat; Gender = 1; DateOfBirth = "1985-05-06"; Level = 1; Address = "Dammam" } | ConvertTo-Json
Invoke-Api -title "2f. Profile - Update Lawyer Profile (Valid)" -method "PUT" -endpoint "/api/lawyers/profile" -body $lawyerProfileUpdateBody -token $lawyerToken -reportFile $reportFile | Out-Null

$loginResLawyer2 = Invoke-Api -title "2f2. Setup - Re-Login Lawyer (After Profile Update)" -method "POST" -endpoint "/api/auth/login" -body $loginBodyLawyer -reportFile $reportFile
if ($loginResLawyer2 -and $loginResLawyer2.Data -and $loginResLawyer2.Data.AccessToken) { $lawyerToken = $loginResLawyer2.Data.AccessToken }
# Cross-Authorization Checks (Validation & Security)
Invoke-Api -title "2g. Security - Lawyer Token on Client Endpoint (403)" -method "GET" -endpoint "/api/clients/profile" -token $lawyerToken -reportFile $reportFile | Out-Null
Invoke-Api -title "2h. Security - Client Token on Lawyer Endpoint (403)" -method "GET" -endpoint "/api/lawyers/profile" -token $clientToken -reportFile $reportFile | Out-Null

# Validation Checks
$invalidClientProfileBody = @{ PhoneNumber = ""; Gender = 99; DateOfBirth = "3000-01-01" } | ConvertTo-Json
Invoke-Api -title "2i. Validation - Complete Client Profile (Invalid Data 400)" -method "POST" -endpoint "/api/clients/profile/complete" -body $invalidClientProfileBody -token $clientToken -reportFile $reportFile | Out-Null

# ---------------------------------------------------------
# STAGE 3: USER VERIFICATIONS
# ---------------------------------------------------------

$tempFilePath = "$PSScriptRoot\dummy_e2e_id.jpg"
[System.IO.File]::WriteAllBytes($tempFilePath, [byte[]]::new(1024))

$formValidLawyer = @{ UserId = $lawyerId; "Documents[0].ExpirationDate" = "2030-01-01"; "Documents[0].Type" = "1" }
Invoke-MultipartApiLocal -title "3a. Verification - Lawyer Uploads Document (Valid)" -endpoint "/api/UserVerification/submit-verification-documents" -token $lawyerToken -form $formValidLawyer -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

$formValidClient = @{ UserId = $clientId; "Documents[0].ExpirationDate" = "2030-01-01"; "Documents[0].Type" = "1" }
Invoke-MultipartApiLocal -title "3b. Verification - Client Uploads Document (Valid)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formValidClient -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# Unauthorized Attempt (Missing Token)
Invoke-MultipartApiLocal -title "3c. Security - Upload without Token (401)" -endpoint "/api/UserVerification/submit-verification-documents" -token "" -form $formValidClient -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# Cross-User Attempt (Lawyer uploading for Client)
$formLawyerUploadForClient = @{ UserId = $clientId; "Documents[0].ExpirationDate" = "2030-01-01"; "Documents[0].Type" = "2" }
Invoke-MultipartApiLocal -title "3d. Security - Lawyer Uploads for Client (Should Fail)" -endpoint "/api/UserVerification/submit-verification-documents" -token $lawyerToken -form $formLawyerUploadForClient -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# ---------------------------------------------------------
# STAGE 4: ADMIN VERIFICATIONS
# ---------------------------------------------------------

Invoke-Api -title "4a. Admin - Get Pending Verifications List" -method "GET" -endpoint "/api/admin/verifications?PageNumber=1&PageSize=20" -token $adminToken -reportFile $reportFile | Out-Null

# Lawyer Details
$detailsLawyerRes = Invoke-Api -title "4b. Admin - Get Lawyer Verification Details" -method "GET" -endpoint "/api/admin/verifications/$lawyerId" -token $adminToken -reportFile $reportFile
$lawyerDocId = $null
if ($detailsLawyerRes -and $detailsLawyerRes.Data -and $detailsLawyerRes.Data.Documents -and $detailsLawyerRes.Data.Documents.Length -gt 0) {
    $lawyerDocId = $detailsLawyerRes.Data.Documents[0].DocumentId
}

# Client Details
$detailsClientRes = Invoke-Api -title "4c. Admin - Get Client Verification Details" -method "GET" -endpoint "/api/admin/verifications/$clientId" -token $adminToken -reportFile $reportFile
$clientDocId = $null
if ($detailsClientRes -and $detailsClientRes.Data -and $detailsClientRes.Data.Documents -and $detailsClientRes.Data.Documents.Length -gt 0) {
    $clientDocId = $detailsClientRes.Data.Documents[0].DocumentId
}

if ($lawyerDocId) {
    Invoke-Api -title "4d. Admin - Get Lawyer Document Content" -method "GET" -endpoint "/api/admin/verifications/documents/$lawyerDocId/content" -token $adminToken -reportFile $reportFile | Out-Null
    
    # Reject Lawyer Document
    $rejectBody = @{ Decision = 2; RejectionReason = "Image is too blurry." } | ConvertTo-Json
    Invoke-Api -title "4e. Admin - Reject Lawyer Document" -method "PATCH" -endpoint "/api/admin/verifications/documents/$lawyerDocId" -body $rejectBody -token $adminToken -reportFile $reportFile | Out-Null
    
    # Lawyer Deletes Rejected
    Invoke-Api -title "4f. Lawyer - Delete Rejected Document" -method "DELETE" -endpoint "/api/UserVerification?UserId=$lawyerId&DocumentId=$lawyerDocId" -token $lawyerToken -reportFile $reportFile | Out-Null
    
    # Lawyer Re-uploads
    $formValidLawyerReupload = @{ UserId = $lawyerId; "Documents[0].ExpirationDate" = "2030-01-01"; "Documents[0].Type" = "2" }
    Invoke-MultipartApiLocal -title "4g. Lawyer - Re-uploads Document" -endpoint "/api/UserVerification/submit-verification-documents" -token $lawyerToken -form $formValidLawyerReupload -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null
    
    # Admin gets details again
    $detailsLawyerRes2 = Invoke-Api -title "4h. Admin - Get Lawyer Verification Details (Re-upload)" -method "GET" -endpoint "/api/admin/verifications/$lawyerId" -token $adminToken -reportFile $reportFile
    if ($detailsLawyerRes2 -and $detailsLawyerRes2.Data -and $detailsLawyerRes2.Data.Documents) {
        $lawyerDocId2 = $detailsLawyerRes2.Data.Documents[0].DocumentId
        if ($lawyerDocId2) {
            # Approve Lawyer Document
            $approveBody = @{ Decision = 1; RejectionReason = $null } | ConvertTo-Json
            Invoke-Api -title "4i. Admin - Approve Lawyer Document" -method "PATCH" -endpoint "/api/admin/verifications/documents/$lawyerDocId2" -body $approveBody -token $adminToken -reportFile $reportFile | Out-Null
        }
    }
}

if ($clientDocId) {
    # Approve Client Document immediately
    $approveBody = @{ Decision = 1; RejectionReason = $null } | ConvertTo-Json
    Invoke-Api -title "4j. Admin - Approve Client Document" -method "PATCH" -endpoint "/api/admin/verifications/documents/$clientDocId" -body $approveBody -token $adminToken -reportFile $reportFile | Out-Null
}

# Cross-Authorization for Admin endpoints
Invoke-Api -title "4k. Security - Lawyer on Admin Endpoint (403)" -method "GET" -endpoint "/api/admin/verifications" -token $lawyerToken -reportFile $reportFile | Out-Null

# ---------------------------------------------------------
# STAGE 5: POST-VERIFICATION / TEARDOWN
# ---------------------------------------------------------

Invoke-Api -title "5a. Lawyer - Get Public Profile (Anonymous)" -method "GET" -endpoint "/api/lawyers/public/$lawyerId" -token "" -reportFile $reportFile | Out-Null

$deleteAccountBody = @{ CurrentPassword = $password } | ConvertTo-Json
Invoke-Api -title "5b. Client - Delete Profile" -method "DELETE" -endpoint "/api/clients/profile" -body $deleteAccountBody -token $clientToken -reportFile $reportFile | Out-Null

$loginResLawyer3 = Invoke-Api -title "5b2. Setup - Re-Login Lawyer (After Approval)" -method "POST" -endpoint "/api/auth/login" -body $loginBodyLawyer -reportFile $reportFile
if ($loginResLawyer3 -and $loginResLawyer3.Data -and $loginResLawyer3.Data.AccessToken) { $lawyerToken = $loginResLawyer3.Data.AccessToken }

Invoke-Api -title "5c. Lawyer - Delete Profile" -method "DELETE" -endpoint "/api/lawyers/profile" -body $deleteAccountBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Attempt to Login again
Invoke-Api -title "5d. Setup - Login Client After Delete (Should Fail)" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile | Out-Null

if (Test-Path $tempFilePath) { Remove-Item $tempFilePath -Force }

"Tests complete. Results saved to $reportFile`n" | Write-Host
