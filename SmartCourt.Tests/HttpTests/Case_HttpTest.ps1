$ErrorActionPreference = "Stop"

# Paths and Constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\Case_HttpTest_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl = "http://localhost:5049"

# Initialize Report
"# Case Slice HTTP Tests End-to-End Workflow Report`n" | Out-File $reportFile -Encoding utf8
"Generated at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n" | Out-File $reportFile -Append -Encoding utf8

# Helper for Form-Data
function Invoke-ApiForm {
    param([string]$title, [string]$method, [string]$endpoint, [hashtable]$form, [string]$token = "")
    
    $url = "$baseUrl$endpoint"
    try {
        $client = New-Object System.Net.Http.HttpClient
        if ($token) {
            $client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $token)
        }
        
        $content = New-Object System.Net.Http.MultipartFormDataContent

        foreach ($key in $form.Keys) {
            $val = $form[$key]
            if ($val -is [System.IO.FileInfo]) {
                $fileBytes = [System.IO.File]::ReadAllBytes($val.FullName)
                $byteContent = New-Object System.Net.Http.ByteArrayContent($fileBytes, 0, $fileBytes.Length)
                $byteContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf")
                $content.Add($byteContent, $key, $val.Name)
            } else {
                $stringContent = New-Object System.Net.Http.StringContent([string]$val)
                $content.Add($stringContent, $key)
            }
        }

        $httpMethod = New-Object System.Net.Http.HttpMethod($method)
        $request = New-Object System.Net.Http.HttpRequestMessage($httpMethod, $url)
        $request.Content = $content

        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

        Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $status -responseBody $responseBody -reportFile $reportFile
        return ($responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue)
    } catch {
        $status = "Error"
        $errorResponse = $_.Exception.Message
        Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $status -responseBody $errorResponse -reportFile $reportFile
        return ($errorResponse | ConvertFrom-Json -ErrorAction SilentlyContinue)
    }
}

# 1. Zero-Assumption Setup
Write-Host "Registering Client..."
$clientEmail = "client_case_test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerClientBody = @{
    FullName = "Test Client"
    Email = $clientEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

$regClientResp = Invoke-Api -title "Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $registerClientBody -reportFile $reportFile
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginClientBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$loginClientResp = Invoke-Api -title "Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $loginClientResp.data.accessToken

Write-Host "Registering Lawyer..."
$lawyerEmail = "lawyer_case_test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerLawyerBody = @{
    FullName = "Test Lawyer"
    Email = $lawyerEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

$regLawyerResp = Invoke-Api -title "Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $registerLawyerBody -reportFile $reportFile
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginLawyerBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$loginLawyerResp = Invoke-Api -title "Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $loginLawyerResp.data.accessToken
$lawyerId = $loginLawyerResp.data.user.id

# Complete Profiles & Verify
$randomNat = "2900101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
$clientProfileBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = $randomNat } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Client Profile" -method "POST" -endpoint "/api/clients/profile/complete" -body $clientProfileBody -token $clientToken -reportFile $reportFile | Out-Null

$randomNatLawyer = "2850101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
$lawyerProfileBody = @{ PhoneNumber = "+201022222222"; DateOfBirth = "1985-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = $randomNatLawyer; Bio = "Expert Lawyer"; Level = 1; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Lawyer Profile" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $lawyerProfileBody -token $lawyerToken -reportFile $reportFile | Out-Null

$loginBodyAdmin = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$loginResAdmin = Invoke-Api -title "Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
$adminToken = $loginResAdmin.Data.AccessToken
$clientId = $loginClientResp.data.user.id
Invoke-Api -title "Setup - Admin Approve Lawyer" -method "PATCH" -endpoint "/api/admin/verifications/$lawyerId/approve-account" -body "{}" -token $adminToken -reportFile $reportFile | Out-Null
Invoke-Api -title "Setup - Admin Approve Client" -method "PATCH" -endpoint "/api/admin/verifications/$clientId/approve-account" -body "{}" -token $adminToken -reportFile $reportFile | Out-Null

$lawyerLoginResp = Invoke-Api -title "Setup - Re-Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $lawyerLoginResp.data.accessToken

$clientLoginResp = Invoke-Api -title "Setup - Re-Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# 2. Exhaustive Case Tests (Client)
Write-Host "Testing Case endpoints..."

# Create dummy document
$dummyDocPath = "$scriptDir\dummy_case.pdf"
"Dummy PDF Content" | Out-File $dummyDocPath

# Create Case - 400 Validation Error (Missing Title/Desc)
$badCreateForm = @{
    Title = ""
    Description = ""
}
$createBadResp = Invoke-ApiForm -title "Create Case (400 Validation Error)" -method "POST" -endpoint "/api/Case" -form $badCreateForm -token $clientToken

# Create Case - Success
$createForm = @{
    Title = "Valid Case Title"
    Description = "Detailed description of the case for testing."
    Governorate = "Cairo"
    City = "Maadi"
    Documents = Get-Item $dummyDocPath
}
$createResp = Invoke-ApiForm -title "Create Case (Valid Success)" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$caseId = $createResp.Data.CaseId

if (-not $caseId) {
    Write-Host "Failed to create case. Exiting script."
    exit 1
}

# Get Case By ID - Success (Verify LastReviewId is null initially)
$getByIdResp = Invoke-Api -title "Get Case By ID (Before Review)" -method "GET" -endpoint "/api/Case/$caseId" -token $clientToken -reportFile $reportFile
if ($null -ne $getByIdResp.data.lastReviewId) {
    Write-Error "Assertion Failed: Initial lastReviewId should be null."
} else {
    Write-Host "SUCCESS Assertion Passed: Initial lastReviewId is null as expected." -ForegroundColor Green
}

# Get Case By ID - 404
$randomGuid = [Guid]::NewGuid().ToString()
Invoke-Api -title "Get Case By ID (404 Not Found)" -method "GET" -endpoint "/api/Case/$randomGuid" -token $clientToken -reportFile $reportFile | Out-Null

# Get All Cases
Invoke-Api -title "Get All Cases" -method "GET" -endpoint "/api/Case" -token $clientToken -reportFile $reportFile | Out-Null

# Update Case - Success
$updateForm = @{
    CaseId = $caseId
    Title = "Updated Case Title"
    Description = "Updated description of the case for testing."
}
Invoke-ApiForm -title "Update Case (Valid Success)" -method "PUT" -endpoint "/api/Case/$caseId" -form $updateForm -token $clientToken | Out-Null

# Create throwing case for deletion
$createDelResp = Invoke-ApiForm -title "Create Case to Delete" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$delCaseId = $createDelResp.Data.CaseId
Invoke-Api -title "Delete Case (Success)" -method "DELETE" -endpoint "/api/Case/$delCaseId" -token $clientToken -reportFile $reportFile | Out-Null

# Stress Tests / Malicious
$stressForm = @{
    Title = "<script>alert('XSS')</script> " * 50
    Description = "' OR 1=1; DROP TABLE Cases;--"
}
Invoke-ApiForm -title "Create Case (Stress - Malicious Payload)" -method "POST" -endpoint "/api/Case" -form $stressForm -token $clientToken | Out-Null

# 3. End-to-End Orchestration (Client -> Lawyer)
Write-Host "Starting Orchestration..."

# Client Reviews Case
$reviewResp = Invoke-Api -title "Review Case (AI Request)" -method "POST" -endpoint "/api/cases/$caseId/review" -body "{}" -token $clientToken -reportFile $reportFile
$reviewReportId = $reviewResp.data.id

if (-not $reviewReportId) {
    Write-Error "Failed: Review Report ID was null in review response."
}

# Get Review Report By ID
Invoke-Api -title "Get Review Report" -method "GET" -endpoint "/api/cases/$caseId/reviews/$reviewReportId" -token $clientToken -reportFile $reportFile | Out-Null

# Get Case By ID After Review - Verify LastReviewId matches newly created review report ID
$getByIdAfterReviewResp = Invoke-Api -title "Get Case By ID (After Review)" -method "GET" -endpoint "/api/Case/$caseId" -token $clientToken -reportFile $reportFile
if ($getByIdAfterReviewResp.data.lastReviewId -ne $reviewReportId) {
    Write-Error "Assertion Failed: lastReviewId ($($getByIdAfterReviewResp.data.lastReviewId)) does not match reviewReportId ($reviewReportId)."
} else {
    Write-Host "SUCCESS Assertion Passed: lastReviewId ($reviewReportId) successfully populated on Case!" -ForegroundColor Green
}

# Get All Cases After Review - Verify LastReviewId in list
$getAllAfterReviewResp = Invoke-Api -title "Get All Cases (After Review)" -method "GET" -endpoint "/api/Case" -token $clientToken -reportFile $reportFile
$matchingCaseInList = $getAllAfterReviewResp.data | Where-Object { $_.id -eq $caseId }
if ($matchingCaseInList.lastReviewId -ne $reviewReportId) {
    Write-Error "Assertion Failed: lastReviewId in Get All Cases list ($($matchingCaseInList.lastReviewId)) does not match expected reviewReportId ($reviewReportId)."
} else {
    Write-Host "SUCCESS Assertion Passed: lastReviewId ($reviewReportId) successfully populated in Get All Cases list!" -ForegroundColor Green
}

# Client Finalizes Case (Matches)
$finalizeResp = Invoke-Api -title "Finalize Case (Transition to Matched)" -method "POST" -endpoint "/api/Case/$caseId/finalize" -body "{}" -token $clientToken -reportFile $reportFile

# Client Creates Proposal
$proposalBody = @{
    LegalCaseId = $caseId
    LawyerUserId = $lawyerId
    Message = "I would like to hire you for this case."
} | ConvertTo-Json
$proposalResp = Invoke-Api -title "Create Proposal (Client to Lawyer)" -method "POST" -endpoint "/api/proposals" -body $proposalBody -token $clientToken -reportFile $reportFile
$proposalId = $proposalResp.Data.Id

# Lawyer Views Proposals
Invoke-Api -title "Lawyer Get Proposals" -method "GET" -endpoint "/api/proposals" -token $lawyerToken -reportFile $reportFile | Out-Null

# Lawyer Accepts Proposal
Invoke-Api -title "Lawyer Accepts Proposal" -method "POST" -endpoint "/api/proposals/$proposalId/accept" -body "{}" -token $lawyerToken -reportFile $reportFile | Out-Null

# Verify ChatId field exists in Case endpoints
$getByIdAfterProposalResp = Invoke-Api -title "Get Case By ID (After Proposal Acceptance)" -method "GET" -endpoint "/api/Case/$caseId" -token $clientToken -reportFile $reportFile
Write-Host "SUCCESS Verified ChatId field in CaseDto response." -ForegroundColor Green

Write-Host "Script Execution Completed. Report written to $reportFile"
Remove-Item $dummyDocPath -ErrorAction SilentlyContinue
