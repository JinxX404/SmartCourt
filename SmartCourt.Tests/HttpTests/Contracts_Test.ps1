$ErrorActionPreference = "Stop"

# Paths and Constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\Contracts_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl = "http://localhost:5049"

# Initialize Report
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Contracts Slice HTTP Tests End-to-End Workflow Report`n" | Out-File $reportFile -Encoding utf8
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

Write-Host "1. Zero-Assumption Setup: Users, Cases, Proposals"

# Register Client
$clientEmail = "client_contract_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerClientBody = @{ FullName = "Test Client"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $registerClientBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginClientBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$clientLoginResp = Invoke-Api -title "Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# Complete Client Profile
$clientProfileBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = "29001011111111" } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Client Profile" -method "POST" -endpoint "/api/clients/profile/complete" -body $clientProfileBody -token $clientToken -reportFile $reportFile | Out-Null
$clientLoginResp = Invoke-Api -title "Setup - Re-Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody -reportFile $reportFile
$clientToken = $clientLoginResp.data.accessToken

# Register Lawyer
$lawyerEmail = "lawyer_contract_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerLawyerBody = @{ FullName = "Test Lawyer"; Email = $lawyerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $registerLawyerBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginLawyerBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$lawyerLoginResp = Invoke-Api -title "Setup - Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $lawyerLoginResp.data.accessToken
$lawyerId = $lawyerLoginResp.data.user.id

# Complete Lawyer Profile
$lawyerProfileBody = @{ PhoneNumber = "+201022222222"; DateOfBirth = "1985-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = "28501012222222"; Bio = "Expert Lawyer"; Level = 1; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "Setup - Complete Lawyer Profile" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $lawyerProfileBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Admin Verifies Lawyer Account
$loginBodyAdmin = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$loginResAdmin = Invoke-Api -title "Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
$adminToken = $loginResAdmin.Data.AccessToken
Invoke-Api -title "Setup - Admin Approve Lawyer" -method "PATCH" -endpoint "/api/admin/verifications/$lawyerId/approve-account" -body "{}" -token $adminToken -reportFile $reportFile | Out-Null

$lawyerLoginResp = Invoke-Api -title "Setup - Re-Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody -reportFile $reportFile
$lawyerToken = $lawyerLoginResp.data.accessToken

# Register Attacker
$attackerEmail = "attacker_contract_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerAttackerBody = @{ FullName = "Test Attacker"; Email = $attackerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Attacker" -method "POST" -endpoint "/api/auth/register/client" -body $registerAttackerBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $attackerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginAttackerBody = @{ Email = $attackerEmail; Password = "Password123!" } | ConvertTo-Json
$attackerLoginResp = Invoke-Api -title "Setup - Login Attacker" -method "POST" -endpoint "/api/auth/login" -body $loginAttackerBody -reportFile $reportFile
$attackerToken = $attackerLoginResp.data.accessToken

# Client creates a Case
$dummyDocPath = "$scriptDir\dummy_contract_case.pdf"
"Dummy PDF Content" | Out-File $dummyDocPath
$createForm = @{ Title = "Case for Contract"; Description = "Detailed description of the case for testing contract creation."; Governorate = "Cairo"; City = "Maadi"; Documents = Get-Item $dummyDocPath }
$createResp = Invoke-ApiForm -title "Setup - Create Case" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$caseId = $createResp.Data.CaseId

# Client Reviews Case (Required before Finalize)
Invoke-Api -title "Setup - Review Case (AI Request)" -method "POST" -endpoint "/api/cases/$caseId/review" -body "{}" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "Setup - Get Latest Review" -method "GET" -endpoint "/api/cases/$caseId/reviews/latest" -token $clientToken -reportFile $reportFile | Out-Null

# Client Finalizes Case
Invoke-Api -title "Setup - Finalize Case" -method "POST" -endpoint "/api/Case/$caseId/finalize" -body "{}" -token $clientToken -reportFile $reportFile | Out-Null

# Client Creates Proposal
$proposalBody = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Let's make a contract." } | ConvertTo-Json
$proposalResp = Invoke-Api -title "Setup - Client Creates Proposal" -method "POST" -endpoint "/api/proposals" -body $proposalBody -token $clientToken -reportFile $reportFile
$proposalId = $proposalResp.Data.Id

# Lawyer Accepts Proposal
Invoke-Api -title "Setup - Lawyer Accepts Proposal" -method "POST" -endpoint "/api/proposals/$proposalId/accept" -body "{}" -token $lawyerToken -reportFile $reportFile | Out-Null

Write-Host "2. Exhaustive Test Scenarios for Contracts"

# POST /api/contracts (Create Contract)
$randomGuid = [Guid]::NewGuid().ToString()
# Missing ProposalId
$body = @{ Title = "Valid Title"; TermsAndConditions = "These are the valid terms and conditions that exceed 20 characters." } | ConvertTo-Json
Invoke-Api -title "POST Create - Missing ProposalId (400)" -method "POST" -endpoint "/api/contracts" -body $body -token $lawyerToken -reportFile $reportFile | Out-Null

# Short Title / Short Terms
$body = @{ ProposalId = $proposalId; Title = "A"; TermsAndConditions = "Short" } | ConvertTo-Json
Invoke-Api -title "POST Create - Validation Error Short strings (400)" -method "POST" -endpoint "/api/contracts" -body $body -token $lawyerToken -reportFile $reportFile | Out-Null

# Malicious Payload
$body = @{ ProposalId = $proposalId; Title = "' OR 1=1; DROP TABLE Contracts;--"; TermsAndConditions = "<script>alert('XSS')</script> " * 10 } | ConvertTo-Json
Invoke-Api -title "POST Create - Malicious Payload" -method "POST" -endpoint "/api/contracts" -body $body -token $lawyerToken -reportFile $reportFile | Out-Null

# Client attempts to create (Forbidden)
$body = @{ ProposalId = $proposalId; Title = "Client Title"; TermsAndConditions = "These are valid terms." * 5 } | ConvertTo-Json
Invoke-Api -title "POST Create - Client attempts (403 Forbidden)" -method "POST" -endpoint "/api/contracts" -body $body -token $clientToken -reportFile $reportFile | Out-Null

# Success Create
$body = @{ ProposalId = $proposalId; Title = "Legal Representation Contract"; TermsAndConditions = "1. First Term.`n2. Second Term.`n3. Third Term.`nThese are the complete terms and conditions that govern the contract and must be adhered to by both parties." } | ConvertTo-Json
$createContractResp = Invoke-Api -title "POST Create - Happy Path Success (201)" -method "POST" -endpoint "/api/contracts" -body $body -token $lawyerToken -reportFile $reportFile
$contractId = $createContractResp.Data.Id

# Attempt to create second contract for same proposal (Conflict)
Invoke-Api -title "POST Create - Duplicate ProposalId (409 Conflict)" -method "POST" -endpoint "/api/contracts" -body $body -token $lawyerToken -reportFile $reportFile | Out-Null


# GET /api/contracts (List Contracts)
Invoke-Api -title "GET List - Negative Page (400)" -method "GET" -endpoint "/api/contracts?page=-1&pageSize=10" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "GET List - Invalid Status String (400)" -method "GET" -endpoint "/api/contracts?status=InvalidStatus" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "GET List - Happy Path Client (200)" -method "GET" -endpoint "/api/contracts?page=1&pageSize=10" -token $clientToken -reportFile $reportFile | Out-Null


# GET /api/contracts/{contractId} (Get Contract)
Invoke-Api -title "GET Detail - Unrelated User (403 Forbidden)" -method "GET" -endpoint "/api/contracts/$contractId" -token $attackerToken -reportFile $reportFile | Out-Null
Invoke-Api -title "GET Detail - Valid non-existent GUID (404 Not Found)" -method "GET" -endpoint "/api/contracts/$randomGuid" -token $lawyerToken -reportFile $reportFile | Out-Null
$contractDetailResp = Invoke-Api -title "GET Detail - Happy Path Lawyer (200)" -method "GET" -endpoint "/api/contracts/$contractId" -token $lawyerToken -reportFile $reportFile
$ifMatch = $contractDetailResp.Data.Version


# PUT /api/contracts/{contractId} (Update Contract)
$updateBody = @{ Title = "Updated Representation Contract"; TermsAndConditions = "These are the heavily updated and revised terms and conditions for the contract." } | ConvertTo-Json

# Client attempts (Forbidden)
Invoke-Api -title "PUT Update - Client attempts (403 Forbidden)" -method "PUT" -endpoint "/api/contracts/$contractId" -body $updateBody -token $clientToken -reportFile $reportFile | Out-Null

# Missing If-Match
Invoke-Api -title "PUT Update - Missing If-Match (400)" -method "PUT" -endpoint "/api/contracts/$contractId" -body $updateBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Lawyer updates Success
$headers = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $ifMatch }
$url = "$baseUrl/api/contracts/$contractId"
try {
    $response = Invoke-WebRequest -Method "PUT" -Uri $url -Headers $headers -Body $updateBody -UseBasicParsing
    Log-Test -title "PUT Update - Happy Path Success (200)" -method "PUT" -url $url -body $updateBody -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
    $ifMatch = ($response.Content | ConvertFrom-Json).Data.Version
} catch {
    Log-Test -title "PUT Update - Happy Path Success (200)" -method "PUT" -url $url -body $updateBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Update with Outdated If-Match
$headers = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = "old-version-123" }
try {
    $response = Invoke-WebRequest -Method "PUT" -Uri $url -Headers $headers -Body $updateBody -UseBasicParsing
    Log-Test -title "PUT Update - Outdated If-Match (412)" -method "PUT" -url $url -body $updateBody -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
} catch {
    Log-Test -title "PUT Update - Outdated If-Match (412)" -method "PUT" -url $url -body $updateBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}


# POST /api/contracts/{contractId}/accept (Accept Contract)
# Unrelated user
$headers = @{ "Authorization" = "Bearer $attackerToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
$url = "$baseUrl/api/contracts/$contractId/accept"
try {
    $response = Invoke-WebRequest -Method "POST" -Uri $url -Headers $headers -Body "{}" -UseBasicParsing
    Log-Test -title "POST Accept - Unrelated User (403)" -method "POST" -url $url -body "{}" -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Accept - Unrelated User (403)" -method "POST" -url $url -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody "" -reportFile $reportFile
}

# Client Accept
$headers = @{ "Authorization" = "Bearer $clientToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
try {
    $response = Invoke-WebRequest -Method "POST" -Uri $url -Headers $headers -Body "{}" -UseBasicParsing
    Log-Test -title "POST Accept - Client Accept Success (200)" -method "POST" -url $url -body "{}" -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
    $ifMatch = ($response.Content | ConvertFrom-Json).Data.Contract.Version
} catch {
    Log-Test -title "POST Accept - Client Accept Success (200)" -method "POST" -url $url -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Client Accept Again (Conflict)
$headers = @{ "Authorization" = "Bearer $clientToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
try {
    $response = Invoke-WebRequest -Method "POST" -Uri $url -Headers $headers -Body "{}" -UseBasicParsing
    Log-Test -title "POST Accept - Client Accept Again (409 Conflict)" -method "POST" -url $url -body "{}" -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Accept - Client Accept Again (409 Conflict)" -method "POST" -url $url -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody "" -reportFile $reportFile
}

# Lawyer Accept
$headers = @{ "Authorization" = "Bearer $lawyerToken"; "If-Match" = $ifMatch; "Content-Type" = "application/json" }
try {
    $response = Invoke-WebRequest -Method "POST" -Uri $url -Headers $headers -Body "{}" -UseBasicParsing
    Log-Test -title "POST Accept - Lawyer Accept Success (200)" -method "POST" -url $url -body "{}" -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
    $ifMatch = ($response.Content | ConvertFrom-Json).Data.Contract.Version
} catch {
    Log-Test -title "POST Accept - Lawyer Accept Success (200)" -method "POST" -url $url -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody "" -reportFile $reportFile
}


# POST /api/contracts/{contractId}/terminate (Terminate Contract)
$terminateBody = @{ Reason = "Parties mutually agreed to terminate this specific contract early." } | ConvertTo-Json
$url = "$baseUrl/api/contracts/$contractId/terminate"

# Client Terminate
$headers = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $clientToken"; "If-Match" = $ifMatch }
try {
    $response = Invoke-WebRequest -Method "POST" -Uri $url -Headers $headers -Body $terminateBody -UseBasicParsing
    Log-Test -title "POST Terminate - Client Terminate Success (200)" -method "POST" -url $url -body $terminateBody -responseStatus $response.StatusCode -responseBody $response.Content -reportFile $reportFile
    $ifMatch = ($response.Content | ConvertFrom-Json).Data.Version
} catch {
    Log-Test -title "POST Terminate - Client Terminate Success (200)" -method "POST" -url $url -body $terminateBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}


# GET /api/contracts/{contractId}/state-history (Get State History)
Invoke-Api -title "GET State History - Client (200)" -method "GET" -endpoint "/api/contracts/$contractId/state-history" -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "GET State History - Attacker (403 Forbidden)" -method "GET" -endpoint "/api/contracts/$contractId/state-history" -token $attackerToken -reportFile $reportFile | Out-Null


Write-Host "Script Execution Completed. Report written to $reportFile"
Remove-Item $dummyDocPath -ErrorAction SilentlyContinue
