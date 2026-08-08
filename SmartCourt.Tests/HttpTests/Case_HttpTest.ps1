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
    
    $headers = @{}
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    $url = "$baseUrl$endpoint"
    try {
        if ($method -eq "POST" -or $method -eq "PUT") {
            $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -Form $form -SkipHttpErrorCheck
            $status = $response.StatusCode
            $responseBody = $response.Content
            Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $status -responseBody $responseBody -reportFile $reportFile
            return ($responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue)
        }
    } catch {
        $status = "Error"
        if ($_.Exception.Response.StatusCode) { $status = [int]$_.Exception.Response.StatusCode }
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

# Get Case By ID - Success
$getByIdResp = Invoke-Api -title "Get Case By ID" -method "GET" -endpoint "/api/Case/$caseId" -token $clientToken -reportFile $reportFile

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

# Get Latest Review
Invoke-Api -title "Get Latest Review" -method "GET" -endpoint "/api/cases/$caseId/reviews/latest" -token $clientToken -reportFile $reportFile | Out-Null

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

Write-Host "Script Execution Completed. Report written to $reportFile"
Remove-Item $dummyDocPath -ErrorAction SilentlyContinue
