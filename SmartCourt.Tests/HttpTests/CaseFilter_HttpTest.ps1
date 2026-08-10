$ErrorActionPreference = "Stop"

# Paths and Constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\CaseFilter_HttpTest_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl = "http://localhost:5049"

# Initialize Report
"# Case Filter HTTP Tests End-to-End Workflow Report`n" | Out-File $reportFile -Encoding utf8
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
$clientEmail = "client_filter_test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerClientBody = @{
    FullName = "Test Client"
    Email = $clientEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

Invoke-Api -title "Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $registerClientBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginClientBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$loginClientResp = Invoke-Api -title "Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $loginClientResp.data.accessToken

Write-Host "Registering Lawyer..."
$lawyerEmail = "lawyer_filter_test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerLawyerBody = @{
    FullName = "Test Lawyer"
    Email = $lawyerEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

Invoke-Api -title "Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $registerLawyerBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginLawyerBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$loginLawyerResp = Invoke-Api -title "Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $loginLawyerResp.data.accessToken
$lawyerId = $loginLawyerResp.data.user.id

# 2. Testing Authorization and Filtering

Write-Host "Testing Unauthenticated access..."
# Unauthenticated GET
Invoke-Api -title "Get Cases (Unauthenticated -> 401)" -method "GET" -endpoint "/api/Case" -reportFile $reportFile | Out-Null


Write-Host "Creating Case for Client..."
$dummyDocPath = "$scriptDir\dummy_case_filter.pdf"
"Dummy PDF Content" | Out-File $dummyDocPath

$createForm = @{
    Title = "Filter Test Case"
    Description = "Case to test role filtering."
    Governorate = "Cairo"
    City = "Maadi"
    Documents = Get-Item $dummyDocPath
}
$createResp = Invoke-ApiForm -title "Create Case (Client)" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$caseId = $createResp.Data.CaseId

Write-Host "Testing Client Visibility..."
# Client should see their case
Invoke-Api -title "Get Cases (Client -> Should see case)" -method "GET" -endpoint "/api/Case" -token $clientToken -reportFile $reportFile | Out-Null


Write-Host "Testing Lawyer Visibility (Before Proposal)..."
# Lawyer should NOT see the case
Invoke-Api -title "Get Cases (Lawyer Before Proposal -> Empty list)" -method "GET" -endpoint "/api/Case" -token $lawyerToken -reportFile $reportFile | Out-Null

# Client Finalizes Case (Matches) - Required before creating proposal
Write-Host "Finalizing case before proposal..."
Invoke-Api -title "Finalize Case" -method "POST" -endpoint "/api/Case/$caseId/finalize" -body "{}" -token $clientToken -reportFile $reportFile | Out-Null

Write-Host "Creating Proposal to grant Lawyer access..."
# Client creates Proposal to the Lawyer
$proposalBody = @{
    LegalCaseId = $caseId
    LawyerUserId = $lawyerId
    Message = "Here is a proposal."
} | ConvertTo-Json
$proposalResp = Invoke-Api -title "Create Proposal (Client to Lawyer)" -method "POST" -endpoint "/api/proposals" -body $proposalBody -token $clientToken -reportFile $reportFile

Write-Host "Testing Lawyer Visibility (After Proposal)..."
# Lawyer SHOULD now see the case
Invoke-Api -title "Get Cases (Lawyer After Proposal -> Should see case)" -method "GET" -endpoint "/api/Case" -token $lawyerToken -reportFile $reportFile | Out-Null

Write-Host "Script Execution Completed. Report written to $reportFile"
Remove-Item $dummyDocPath -ErrorAction SilentlyContinue
