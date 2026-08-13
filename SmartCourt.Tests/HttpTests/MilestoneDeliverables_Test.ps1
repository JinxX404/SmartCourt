$ErrorActionPreference = "Stop"

# Paths and Constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\MilestoneDeliverables_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl = "http://localhost:5049"

# Initialize Report
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Milestone Deliverables HTTP Tests End-to-End Workflow Report`n" | Out-File $reportFile -Encoding utf8
"Generated at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n" | Out-File $reportFile -Append -Encoding utf8

# Helper for Form-Data
function Invoke-ApiForm {
    param([string]$title, [string]$method, [string]$endpoint, [hashtable]$form, [string]$token = "")
    
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    
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

Write-Host "1. Zero-Assumption Setup: Users, Cases, Proposals, Contracts"

# Register Client
$clientEmail = "client_msdeliv_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerClientBody = @{ FullName = "Test Client"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $registerClientBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginClientBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$clientLoginResp = Invoke-Api -title "Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# Complete Client Profile
$randomNat = "2900101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
$clientProfileBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = $randomNat } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Client Profile" -method "POST" -endpoint "/api/clients/profile/complete" -body $clientProfileBody -token $clientToken -reportFile $reportFile | Out-Null
$clientLoginResp = Invoke-Api -title "Setup - Re-Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# Register Lawyer
$lawyerEmail = "lawyer_msdeliv_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerLawyerBody = @{ FullName = "Test Lawyer"; Email = $lawyerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $registerLawyerBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginLawyerBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$lawyerLoginResp = Invoke-Api -title "Setup - Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $lawyerLoginResp.data.accessToken
$lawyerId = $lawyerLoginResp.data.user.id

# Complete Lawyer Profile
$randomNatLawyer = "2850101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
$lawyerProfileBody = @{ PhoneNumber = "+201022222222"; DateOfBirth = "1985-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = $randomNatLawyer; Bio = "Expert Lawyer"; Level = 1; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Lawyer Profile" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $lawyerProfileBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Admin Verifies Client & Lawyer Account
$loginBodyAdmin = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$loginResAdmin = Invoke-Api -title "Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
$adminToken = $loginResAdmin.Data.AccessToken
Invoke-Api -title "Setup - Admin Approve Lawyer" -method "PATCH" -endpoint "/api/admin/verifications/$lawyerId/approve-account" -body "{}" -token $adminToken -reportFile $reportFile | Out-Null
$clientId = $clientLoginResp.data.user.id
Invoke-Api -title "Setup - Admin Approve Client" -method "PATCH" -endpoint "/api/admin/verifications/$clientId/approve-account" -body "{}" -token $adminToken -reportFile $reportFile | Out-Null

$lawyerLoginResp = Invoke-Api -title "Setup - Re-Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $lawyerLoginResp.data.accessToken

$clientLoginResp = Invoke-Api -title "Setup - Re-Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# Client creates a Case
$dummyDocPath = "$scriptDir\dummy_ms_case.pdf"
"Dummy PDF Content" | Out-File $dummyDocPath
$createForm = @{ Title = "Case for Milestones Deliverables"; Description = "Detailed description of the case for testing milestones."; Governorate = "Cairo"; City = "Maadi"; Documents = Get-Item $dummyDocPath }
$createResp = Invoke-ApiForm -title "Setup - Create Case" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$caseId = $createResp.Data.CaseId

# Bypass AI Review and finalize case directly via SQL
sqlcmd -S . -d SmartCourt_dev -E -Q "SET NOCOUNT ON; UPDATE Cases SET Status = 5 WHERE Id = '$caseId'" | Out-Null


# Client Creates Proposal
$proposalBody = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Let's make a contract." } | ConvertTo-Json
$proposalResp = Invoke-Api -title "Setup - Client Creates Proposal" -method "POST" -endpoint "/api/proposals" -body $proposalBody -token $clientToken -reportFile $reportFile
$proposalId = $proposalResp.Data.Id

# Lawyer Accepts Proposal
Invoke-Api -title "Setup - Lawyer Accepts Proposal" -method "POST" -endpoint "/api/proposals/$proposalId/accept" -body "{}" -token $lawyerToken -reportFile $reportFile | Out-Null

# POST /api/contracts (Create Contract)
$contractBody = @{ ProposalId = $proposalId; Title = "Legal Representation Contract"; TermsAndConditions = "These are the complete terms and conditions that govern the contract and must be adhered to by both parties." } | ConvertTo-Json
$createContractResp = Invoke-Api -title "Setup - Lawyer Creates Contract" -method "POST" -endpoint "/api/contracts" -body $contractBody -token $lawyerToken -reportFile $reportFile
$contractId = $createContractResp.Data.Id

# Lawyer gets contract to fetch ETag
$contractDetailResp = Invoke-Api -title "Setup - Lawyer Gets Contract" -method "GET" -endpoint "/api/contracts/$contractId" -token $lawyerToken -reportFile $reportFile
$ifMatch = $contractDetailResp.Data.Version

# Client Accept Contract
$headersClient = @{ "Authorization" = "Bearer $clientToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
$acceptRes1 = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/contracts/$contractId/accept" -Headers $headersClient -Body "{}" -UseBasicParsing

# Lawyer gets contract to fetch NEW ETag
$contractDetailResp = Invoke-Api -title "Setup - Lawyer Gets Contract (Again)" -method "GET" -endpoint "/api/contracts/$contractId" -token $lawyerToken -reportFile $reportFile
$ifMatch = $contractDetailResp.Data.Version

# Lawyer Accept Contract
$headersLawyer = @{ "Authorization" = "Bearer $lawyerToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
$acceptRes2 = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/contracts/$contractId/accept" -Headers $headersLawyer -Body "{}" -UseBasicParsing

Write-Host "2. Exhaustive Test Scenarios for Milestone Deliverables"

# POST /api/contracts/{contractId}/milestones (Create Draft Milestone)
# Empty Deliverables Array is now valid, should succeed (201)
$badBody = @{ Title = "Draft Milestone"; Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10; Deliverables = @() } | ConvertTo-Json -Depth 5
Invoke-Api -title "POST Milestone - Empty Deliverables (201)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Validation Error: Deliverables exceed 100 items
$tooManyDeliverables = @()
for ($i = 1; $i -le 101; $i++) { $tooManyDeliverables += "Deliverable $i" }
$badBody = @{ Title = "Draft Milestone"; Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10; Deliverables = $tooManyDeliverables } | ConvertTo-Json -Depth 5
Invoke-Api -title "POST Milestone - Too Many Deliverables (400)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Validation Error: Deliverable string exceeds 500 chars
$longString = "A" * 501
$badBody = @{ Title = "Draft Milestone"; Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10; Deliverables = @("Normal Deliverable", $longString) } | ConvertTo-Json -Depth 5
Invoke-Api -title "POST Milestone - Long Deliverable String (400)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Validation Error: Empty Deliverable String
$badBody = @{ Title = "Draft Milestone"; Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10; Deliverables = @("Normal Deliverable", "") } | ConvertTo-Json -Depth 5
Invoke-Api -title "POST Milestone - Empty Deliverable String (400)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Validation Error: Malicious Deliverables (Script tags / SQLi)
$maliciousBody = @{ Title = "Draft Milestone"; Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10; Deliverables = @("<script>alert(1)</script>", "'; DROP TABLE Milestones; --") } | ConvertTo-Json -Depth 5
# This should succeed in creation as string is valid length and not empty, but we verify it processes fine
Invoke-Api -title "POST Milestone - Malicious Deliverable String (201)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $maliciousBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Success Path Create with valid Deliverables
$draftBody = @{ Title = "Phase 1: Research"; Description = "Detailed research."; OrderNumber = 2; Amount = 1000.50; DurationDays = 10; Deliverables = @("Design Doc", "API Specs", "UI Wireframes") } | ConvertTo-Json -Depth 5
$draftResp = Invoke-Api -title "POST Milestone - Valid Deliverables (201)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $draftBody -token $lawyerToken -reportFile $reportFile
$milestoneId = $draftResp.Data.Id
$msIfMatch = $draftResp.Data.Version

# GET /api/contracts/{contractId}/milestones (List)
$listResp = Invoke-Api -title "GET Milestones - Verify Deliverables (200)" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $clientToken -reportFile $reportFile

# Update Draft - Modify Deliverables
$updateBody = @{ Title = "Phase 1: Deep Research"; Description = "More details."; DurationDays = 15; Deliverables = @("Design Doc", "API Specs", "UI Wireframes", "Database Schema") } | ConvertTo-Json -Depth 5
$headersUpdate = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch }
try {
    $resUpdate = Invoke-WebRequest -Method "PUT" -Uri "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -Headers $headersUpdate -Body $updateBody -UseBasicParsing
    Log-Test -title "PUT Milestone - Modify Deliverables (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $resUpdate.StatusCode -responseBody $resUpdate.Content -reportFile $reportFile
    $msIfMatch = ($resUpdate.Content | ConvertFrom-Json).Data.Version
} catch {
    Log-Test -title "PUT Milestone - Modify Deliverables (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Update Draft - Clear Deliverables (Null)
$updateBodyClear = @{ Title = "Phase 1: Deep Research"; Description = "More details."; DurationDays = 15; Deliverables = $null } | ConvertTo-Json -Depth 5
$headersUpdateClear = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch }
try {
    $resUpdateClear = Invoke-WebRequest -Method "PUT" -Uri "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -Headers $headersUpdateClear -Body $updateBodyClear -UseBasicParsing
    Log-Test -title "PUT Milestone - Clear Deliverables (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBodyClear -responseStatus $resUpdateClear.StatusCode -responseBody $resUpdateClear.Content -reportFile $reportFile
    $msIfMatch = ($resUpdateClear.Content | ConvertFrom-Json).Data.Version
} catch {
    Log-Test -title "PUT Milestone - Clear Deliverables (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBodyClear -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Verify Deliverables are cleared
$listResp = Invoke-Api -title "GET Milestones - Verify Cleared Deliverables (200)" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $clientToken -reportFile $reportFile

Write-Host "Done! Report generated at $reportFile"
