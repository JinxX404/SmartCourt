$ErrorActionPreference = "Stop"

# Paths and Constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\Milestones_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl = "http://localhost:5049"

# Initialize Report
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Milestones Slice HTTP Tests End-to-End Workflow Report`n" | Out-File $reportFile -Encoding utf8
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
$clientEmail = "client_ms_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
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
$lawyerEmail = "lawyer_ms_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
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

# Register Attacker
$attackerEmail = "attacker_ms_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$registerAttackerBody = @{ FullName = "Test Attacker"; Email = $attackerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "Setup - Register Attacker" -method "POST" -endpoint "/api/auth/register/client" -body $registerAttackerBody -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $attackerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginAttackerBody = @{ Email = $attackerEmail; Password = "Password123!" } | ConvertTo-Json
$attackerLoginResp = Invoke-Api -title "Setup - Login Attacker" -method "POST" -endpoint "/api/auth/login" -body $loginAttackerBody -reportFile $reportFile
$attackerToken = $attackerLoginResp.data.accessToken

# Client creates a Case
$dummyDocPath = "$scriptDir\dummy_ms_case.pdf"
"Dummy PDF Content" | Out-File $dummyDocPath
$createForm = @{ Title = "Case for Milestones"; Description = "Detailed description of the case for testing milestones."; Governorate = "Cairo"; City = "Maadi"; Documents = Get-Item $dummyDocPath }
$createResp = Invoke-ApiForm -title "Setup - Create Case" -method "POST" -endpoint "/api/Case" -form $createForm -token $clientToken
$caseId = $createResp.Data.CaseId

# Client Reviews Case
$reviewResp = Invoke-Api -title "Setup - Review Case (AI Request)" -method "POST" -endpoint "/api/cases/$caseId/review" -body "{}" -token $clientToken -reportFile $reportFile
$reviewReportId = $reviewResp.data.id
Invoke-Api -title "Setup - Get Review Report" -method "GET" -endpoint "/api/cases/$caseId/reviews/$reviewReportId" -token $clientToken -reportFile $reportFile | Out-Null

# Client Finalizes Case
Invoke-Api -title "Setup - Finalize Case" -method "POST" -endpoint "/api/Case/$caseId/finalize" -body "{}" -token $clientToken -reportFile $reportFile | Out-Null

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

Write-Host "2. Exhaustive Test Scenarios for Milestones"

# POST /api/contracts/{contractId}/milestones (Create Draft Milestone)
# Validation Error: Negative Amount
$badBody = @{ Title = "Draft Milestone 1"; Description = "Details."; OrderNumber = 1; Amount = -100; DurationDays = 10 } | ConvertTo-Json
Invoke-Api -title "POST Milestone - Negative Amount (400)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Validation Error: Missing Title
$badBody = @{ Description = "Details."; OrderNumber = 1; Amount = 1000; DurationDays = 10 } | ConvertTo-Json
Invoke-Api -title "POST Milestone - Missing Title (400)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $badBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Success Path Create
$draftBody = @{ Title = "Phase 1: Research"; Description = "Detailed research for the case."; OrderNumber = 1; Amount = 1000.50; DurationDays = 10 } | ConvertTo-Json
$draftResp = Invoke-Api -title "POST Milestone - Happy Path (201)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $draftBody -token $lawyerToken -reportFile $reportFile
$milestoneId = $draftResp.Data.Id
$msIfMatch = $draftResp.Data.Version

# GET /api/contracts/{contractId}/milestones (List)
$listResp = Invoke-Api -title "GET Milestones - List (200)" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $clientToken -reportFile $reportFile

# Ensure Attacker cannot read
Invoke-Api -title "GET Milestones - Attacker (403)" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $attackerToken -reportFile $reportFile | Out-Null

# PUT Update Draft
$updateBody = @{ Title = "Phase 1: Deep Research"; Description = "More details."; DurationDays = 15 } | ConvertTo-Json

# Missing ETag
Invoke-Api -title "PUT Milestone - Missing If-Match (400)" -method "PUT" -endpoint "/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -token $lawyerToken -reportFile $reportFile | Out-Null

# Outdated ETag
$headersOutdated = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = "`"old-version`"" }
try {
    $res = Invoke-WebRequest -Method "PUT" -Uri "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -Headers $headersOutdated -Body $updateBody -UseBasicParsing
    Log-Test -title "PUT Milestone - Outdated If-Match (412)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $res.StatusCode -responseBody $res.Content -reportFile $reportFile
} catch {
    Log-Test -title "PUT Milestone - Outdated If-Match (412)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $_.Exception.Response.StatusCode -responseBody "" -reportFile $reportFile
}

# Happy Path Update
$headersUpdate = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch }
try {
    $resUpdate = Invoke-WebRequest -Method "PUT" -Uri "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -Headers $headersUpdate -Body $updateBody -UseBasicParsing
    Log-Test -title "PUT Milestone - Happy Path (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $resUpdate.StatusCode -responseBody $resUpdate.Content -reportFile $reportFile
    $msIfMatch = ($resUpdate.Content | ConvertFrom-Json).Data.Version
} catch {
    Log-Test -title "PUT Milestone - Happy Path (200)" -method "PUT" -url "$baseUrl/api/contracts/$contractId/milestones/$milestoneId" -body $updateBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# CREATE MILESTONE 2 (For Change Requests Test later)
$draftBody2 = @{ Title = "Phase 2: Execution"; Description = "Do the work."; OrderNumber = 2; Amount = 2000; DurationDays = 5 } | ConvertTo-Json
$draftResp2 = Invoke-Api -title "POST Milestone 2 (201)" -method "POST" -endpoint "/api/contracts/$contractId/milestones" -body $draftBody2 -token $lawyerToken -reportFile $reportFile
$milestoneId2 = $draftResp2.Data.Id


# Approve Milestone
# Fetch Milestone to get fresh ETag
$listResp = Invoke-Api -title "GET Milestones - Refresh ETag before Approve" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $clientToken -reportFile $reportFile
$msIfMatch = ($listResp.Data | Where-Object { $_.Id -eq $milestoneId }).Version

# Try to approve with outdated
$headersApproveBad = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $clientToken"; "If-Match" = "`"old-ver`"" }
try {
    $res = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId/approve" -Headers $headersApproveBad -Body "{}" -UseBasicParsing
    Log-Test -title "POST Approve - Outdated If-Match (412)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $res.StatusCode -responseBody $res.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Approve - Outdated If-Match (412)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody "" -reportFile $reportFile
}

# Success Approve
$headersApprove = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $clientToken"; "If-Match" = $msIfMatch }
try {
    $resApprove = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId/approve" -Headers $headersApprove -Body "{}" -UseBasicParsing
    Log-Test -title "POST Approve - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $resApprove.StatusCode -responseBody $resApprove.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Approve - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Fetch Milestone again for Lawyer Approve
$listResp = Invoke-Api -title "GET Milestones - Refresh ETag before Lawyer Approve" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $lawyerToken -reportFile $reportFile
$msIfMatch = ($listResp.Data | Where-Object { $_.Id -eq $milestoneId }).Version

$headersLawyerApprove = @{
    "Authorization" = "Bearer $lawyerToken"
    "Content-Type" = "application/json"
    "If-Match" = $msIfMatch
}
try {
    $resLawyerApprove = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId/approve" -Headers $headersLawyerApprove -Body "{}" -UseBasicParsing
    Log-Test -title "POST Approve (Lawyer) - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $resLawyerApprove.StatusCode -responseBody $resLawyerApprove.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Approve (Lawyer) - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/approve" -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Approve M2 (Client & Lawyer)
$listRespM2 = Invoke-Api -title "GET Milestones - Refresh ETag M2" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $clientToken -reportFile $reportFile
$msIfMatch2 = ($listRespM2.Data | Where-Object { $_.Id -eq $milestoneId2 }).Version
$headersApprove2 = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $clientToken"; "If-Match" = $msIfMatch2 }
$resApprove2 = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId2/approve" -Headers $headersApprove2 -Body "{}" -UseBasicParsing

$listRespM2Lawyer = Invoke-Api -title "GET Milestones - Refresh ETag M2 for Lawyer" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $lawyerToken -reportFile $reportFile
$msIfMatch2Lawyer = ($listRespM2Lawyer.Data | Where-Object { $_.Id -eq $milestoneId2 }).Version
$headersLawyerApprove2 = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch2Lawyer }
$resLawyerApprove2 = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId2/approve" -Headers $headersLawyerApprove2 -Body "{}" -UseBasicParsing


# Wait for background contract activation outbox event to process
Write-Host "Waiting for background Outbox processing (Contract Activation)..."
$maxWaitSeconds = 90
$elapsed = 0
$isContractActive = $false
while ($elapsed -lt $maxWaitSeconds) {
    $pollResp = Invoke-WebRequest -Method "GET" -Uri "$baseUrl/api/contracts/$contractId" -Headers $headersClient -UseBasicParsing -ErrorAction SilentlyContinue
    if ($pollResp.StatusCode -eq 200) {
        $body = $pollResp.Content | ConvertFrom-Json
        if ($body.data.status -eq 1) {
            $isContractActive = $true
            Write-Host "Contract is now Active!"
            break
        }
    }
    Start-Sleep -Seconds 5
    $elapsed += 5
    Write-Host "Waited $elapsed seconds..."
}
if (-not $isContractActive) {
    Write-Host "WARNING: Contract did not become active within $maxWaitSeconds seconds. Tests may fail."
}

$listResp = Invoke-Api -title "GET Milestones - Refresh ETag before ReadyForFunding" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $lawyerToken -reportFile $reportFile
$msIfMatch = ($listResp.Data | Where-Object { $_.Id -eq $milestoneId }).Version

# Mark Ready For Funding (Lawyer)
$headersRFF = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch }
try {
    $readyResp = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId/ready-for-funding" -Headers $headersRFF -Body "{}" -UseBasicParsing
    Log-Test -title "POST ReadyForFunding - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/ready-for-funding" -body "{}" -responseStatus $readyResp.StatusCode -responseBody $readyResp.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST ReadyForFunding - Happy Path (200)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId/ready-for-funding" -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

  # --- MOCK FUNDING FOR M1 ---
  # The backend requires a milestone to be FundedInProgress before it can be submitted,
  # and the funding verifier checks for EscrowAccount, EscrowHold, and PaymentTransaction.
  $mockFundingQuery1 = @"
  SET NOCOUNT ON;
  SET QUOTED_IDENTIFIER ON;
  SET ANSI_NULLS ON;
  DECLARE @EscrowAccountId UNIQUEIDENTIFIER = NEWID();
  DECLARE @EscrowHoldId UNIQUEIDENTIFIER = NEWID();
  DECLARE @PaymentTransactionId UNIQUEIDENTIFIER = NEWID();
  DECLARE @Amount DECIMAL(18,2) = (SELECT Amount FROM Milestones WHERE Id = '$milestoneId');
  DECLARE @ContractId UNIQUEIDENTIFIER = (SELECT ContractId FROM Milestones WHERE Id = '$milestoneId');

  IF NOT EXISTS (SELECT 1 FROM EscrowAccounts WHERE ContractId = @ContractId)
  BEGIN
      INSERT INTO EscrowAccounts (Id, ContractId, Currency, TotalDeposited, TotalRefunded, TotalReleased, TotalFees, Status, CreatedAt, UpdatedAt)
      VALUES (@EscrowAccountId, @ContractId, 'EGP', 0, 0, 0, 0, 1, GETUTCDATE(), GETUTCDATE());
  END
  ELSE
  BEGIN
      SET @EscrowAccountId = (SELECT Id FROM EscrowAccounts WHERE ContractId = @ContractId);
  END

  INSERT INTO PaymentTransactions (Id, ContractId, MilestoneId, EscrowHoldId, OperationType, ProviderName, ProviderTransactionId, IdempotencyKey, Amount, Currency, Status, ProviderAttemptCount, RequiresManualAction, CreatedAt, UpdatedAt)
  VALUES (@PaymentTransactionId, @ContractId, '$milestoneId', NULL, 0, 'MockProvider', CONVERT(NVARCHAR(50), NEWID()), NEWID(), @Amount, 'EGP', 0, 1, 0, GETUTCDATE(), GETUTCDATE());

  INSERT INTO EscrowHolds (Id, EscrowAccountId, ContractId, MilestoneId, GrossAmount, PlatformFeeAmount, NetAmount, Status, ProviderDepositTransactionId, HoldExpiresAt, FundedAt, CreatedAt, UpdatedAt)
  VALUES (@EscrowHoldId, @EscrowAccountId, @ContractId, '$milestoneId', @Amount, 0, @Amount, 0, @PaymentTransactionId, DATEADD(day, 30, GETUTCDATE()), GETUTCDATE(), GETUTCDATE(), GETUTCDATE());

  UPDATE PaymentTransactions SET EscrowHoldId = @EscrowHoldId, Status = 1 WHERE Id = @PaymentTransactionId;

  UPDATE Milestones SET Status = 3, FundedAt = GETUTCDATE() WHERE Id = '$milestoneId';
"@
sqlcmd -S "." -d "SmartCourt_Graduation" -Q $mockFundingQuery1

# Insert a valid StoredFile and UserVerificationDocument for the Lawyer so they can attach it
$lawyerFileId = [guid]::NewGuid().ToString()
$insertFileQuery = @"
SET NOCOUNT ON;
INSERT INTO StoredFiles (Id, StoredFileName, OriginalFileName, FileUrl, ContentType, Extension, SizeInBytes, IsDeleted) 
VALUES ('$lawyerFileId', 'fake.pdf', 'fake.pdf', 'http://fake', 'application/pdf', '.pdf', 100, 0);

INSERT INTO UserVerificationDocuments (Id, UserId, StoredFileId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate)
VALUES (NEWID(), '$lawyerId', '$lawyerFileId', 0, 1, 1, 0, DATEADD(year, 1, GETUTCDATE()));
"@
sqlcmd -S "." -d "SmartCourt_Graduation" -Q $insertFileQuery

# Submit Delivery
$submitBody = @{ Notes = "Work completed. Check files."; StoredFileIds = @($lawyerFileId) } | ConvertTo-Json
$submitResp = Invoke-Api -title "POST Submit - Happy Path (200)" -method "POST" -endpoint "/api/milestones/$milestoneId/submit" -body $submitBody -token $lawyerToken -reportFile $reportFile

# Client Requests Changes
$reqChangeBody = @{ Reason = "Need more details in report." } | ConvertTo-Json
$reqChangeResp = Invoke-Api -title "POST RequestChanges - Happy Path (200)" -method "POST" -endpoint "/api/milestones/$milestoneId/request-changes" -body $reqChangeBody -token $clientToken -reportFile $reportFile

# Lawyer Submits Again
$submitResp = Invoke-Api -title "POST Submit Again - Happy Path (200)" -method "POST" -endpoint "/api/milestones/$milestoneId/submit" -body $submitBody -token $lawyerToken -reportFile $reportFile

# Client Accepts M1
$acceptResp = Invoke-Api -title "POST Accept - Happy Path (200)" -method "POST" -endpoint "/api/milestones/$milestoneId/accept" -body "{}" -token $clientToken -reportFile $reportFile

$listResp2 = Invoke-Api -title "GET Milestones - Refresh ETag before ChangeRequest" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $lawyerToken -reportFile $reportFile
$msIfMatch2 = ($listResp2.Data | Where-Object { $_.Id -eq $milestoneId2 }).Version

# --- MOCK FUNDING FOR M2 ---
# M2 cannot have a ChangeRequest unless it's FundedInProgress. Wait, first we must make it AwaitingFunding if it's not already.
# Actually, the Payment service handles the transition. We will just mock it to FundedInProgress.
$mockFundingQuery2 = "SET NOCOUNT ON; UPDATE Milestones SET Status = 3, FundedAt = GETUTCDATE() WHERE Id = '$milestoneId2';"
sqlcmd -S "." -d "SmartCourt_Graduation" -Q $mockFundingQuery2

# Refetch to get the updated ETag after the mock DB update
$listResp2 = Invoke-Api -title "GET Milestones - Refresh ETag after M2 Funding" -method "GET" -endpoint "/api/contracts/$contractId/milestones" -token $lawyerToken -reportFile $reportFile
$msIfMatch2 = ($listResp2.Data | Where-Object { $_.Id -eq $milestoneId2 }).Version

# Lawyer creates a change request for M2
$crBody = @{ ProposedDescription = "Do the hard work."; ProposedDurationDays = 10; Reason = "Need more time." } | ConvertTo-Json
$headersCR = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $lawyerToken"; "If-Match" = $msIfMatch2 }
try {
    $resCR = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/milestones/$milestoneId2/change-requests" -Headers $headersCR -Body $crBody -UseBasicParsing
    Log-Test -title "POST ChangeRequest - Happy Path (201)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId2/change-requests" -body $crBody -responseStatus $resCR.StatusCode -responseBody $resCR.Content -reportFile $reportFile
    $crId = ($resCR.Content | ConvertFrom-Json).Data.EntityId
} catch {
    Log-Test -title "POST ChangeRequest - Happy Path (201)" -method "POST" -url "$baseUrl/api/milestones/$milestoneId2/change-requests" -body $crBody -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

# Fetch again to get ETag for approving CR (ChangeRequests have their own ETag)
$crRowVersionHex = (sqlcmd -S "." -d "SmartCourt_Graduation" -Q "SET NOCOUNT ON; SELECT CONVERT(varchar(50), CAST(RowVersion AS varbinary(8)), 2) FROM MilestoneChangeRequests WHERE Id = '$crId';" -W -h -1).Trim()
$hex = $crRowVersionHex
$bytes = [byte[]]($hex -split '(..)' | Where-Object { $_ } | ForEach-Object { [convert]::ToByte($_, 16) })
$base64 = [Convert]::ToBase64String($bytes)
$crIfMatch = "`"$base64`""

# Client Approves Change Request
$headersCRApprove = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $clientToken"; "If-Match" = $crIfMatch }
try {
    $resCRApp = Invoke-WebRequest -Method "POST" -Uri "$baseUrl/api/change-requests/$crId/approve" -Headers $headersCRApprove -Body "{}" -UseBasicParsing
    Log-Test -title "POST Approve ChangeRequest - Happy Path (200)" -method "POST" -url "$baseUrl/api/change-requests/$crId/approve" -body "{}" -responseStatus $resCRApp.StatusCode -responseBody $resCRApp.Content -reportFile $reportFile
} catch {
    Log-Test -title "POST Approve ChangeRequest - Happy Path (200)" -method "POST" -url "$baseUrl/api/change-requests/$crId/approve" -body "{}" -responseStatus $_.Exception.Response.StatusCode -responseBody $_.Exception.Message -reportFile $reportFile
}

Write-Host "Script Execution Completed. Report written to $reportFile"
Remove-Item $dummyDocPath -ErrorAction SilentlyContinue

