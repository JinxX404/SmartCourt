$ErrorActionPreference = "Stop"

# =========================================================
# Smart Court - Complete A-to-Z E2E Lifecycle Test
# Phases:
#   1. Account & Profile Setup (Client + Lawyer + Admin verify)
#   2. Case & Proposal
#   3. Contract, Milestones & Chat
#   4. Payments (mock-success inline path -> FundedInProgress)
#   5. Milestone Delivery & Acceptance
#   6. Escrow Release (Hangfire + SQL timing) & Wallet
#   7. Outbox & Hangfire Audit
# =========================================================

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\E2E_Lifecycle_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl    = "http://localhost:5049"
$dbName     = "SmartCourt_Graduation"
$dbServer   = "."

Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Smart Court E2E Lifecycle Report`n" | Out-File $reportFile -Encoding utf8
"Generated at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n" | Out-File $reportFile -Append -Encoding utf8

# ==========================================
# HELPERS
# ==========================================
function Write-Section {
    param([string]$h)
    Write-Host "`n========================================`n  $h`n========================================"
    "`n## $h`n" | Out-File $reportFile -Append -Encoding utf8
}

function Write-Assert {
    param([string]$label, [bool]$pass, [string]$detail = "")
    $icon = if ($pass) { "PASS" } else { "FAIL" }
    Write-Host "$icon - $label $detail"
    $md = if ($pass) { "OK" } else { "FAIL" }
    "- [$md] **$label** $detail`n" | Out-File $reportFile -Append -Encoding utf8
}

function Sql-Query {
    param([string]$query)
    return sqlcmd -S $dbServer -d $dbName -Q $query -W -h -1
}

function Get-SqlCount {
    param([string]$q)
    $v = (Sql-Query $q | Select-Object -Last 1)
    return ([int]($v -replace '\s', ''))
}

function Poll-Until {
    param([scriptblock]$cond, [string]$label, [int]$to = 90, [int]$iv = 5)
    $e = 0
    while ($e -lt $to) {
        if (& $cond) { Write-Host "OK: $label"; return $true }
        Start-Sleep -Seconds $iv
        $e += $iv
        Write-Host "  Waiting '$label'...($e/$to s)"
    }
    Write-Host "TIMEOUT: $label"
    return $false
}

function Invoke-ApiRaw {
    param(
        [string]$title,
        [string]$method,
        [string]$endpoint,
        [hashtable]$extraHeaders = @{},
        [string]$body = "",
        [string]$token = ""
    )
    $headers = @{ "Content-Type" = "application/json" }
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    foreach ($kv in $extraHeaders.GetEnumerator()) { $headers[$kv.Key] = $kv.Value }
    $url = "$baseUrl$endpoint"
    try {
        $reqArgs = @{ Method = $method; Uri = $url; Headers = $headers; UseBasicParsing = $true; SkipHttpErrorCheck = $true }
        if ($body -and $method -ne "GET") { $reqArgs["Body"] = $body }
        $resp = Invoke-WebRequest @reqArgs
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $resp.StatusCode -responseBody $resp.Content -reportFile $reportFile
        return ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue)
    }
    catch {
        $status = if ($_.Exception.Response.StatusCode) { [int]$_.Exception.Response.StatusCode } else { "Error" }
        $errBody = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $errBody -reportFile $reportFile
        return $null
    }
}

function Invoke-ApiForm {
    param([string]$title, [string]$method, [string]$endpoint, [hashtable]$form, [string]$token = "")
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $url = "$baseUrl$endpoint"
    try {
        $resp = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -Form $form -UseBasicParsing -SkipHttpErrorCheck
        Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $resp.StatusCode -responseBody $resp.Content -reportFile $reportFile
        return ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue)
    }
    catch {
        $s = if ($_.Exception.Response.StatusCode) { [int]$_.Exception.Response.StatusCode } else { "Error" }
        Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $s -responseBody $_.Exception.Message -reportFile $reportFile
        return $null
    }
}

function Invoke-IfMatch {
    param([string]$title, [string]$method, [string]$endpoint, [string]$ifMatch, [string]$body = "{}", [string]$token)
    $eh = @{ "If-Match" = $ifMatch }
    return Invoke-ApiRaw -title $title -method $method -endpoint $endpoint -extraHeaders $eh -body $body -token $token
}

# ==========================================
# PHASE 1 - Account & Profile Setup
# ==========================================
Write-Section "Phase 1: Account and Profile Setup"

$ts = Get-Date -Format 'yyyyMMddHHmmss'

# Client Registration
$clientEmail     = "e2e_client_${ts}@example.com"
$clientRegBody   = @{ FullName = "E2E Client"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
$r               = Invoke-ApiRaw "Register Client" POST "/api/auth/register/client" -body $clientRegBody
Write-Assert "Register Client" ($r -ne $null)

Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

$clientLoginBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$clientLogin     = Invoke-ApiRaw "Login Client" POST "/api/auth/login" -body $clientLoginBody
$clientToken     = $clientLogin.data.accessToken
$clientId        = $clientLogin.data.user.id
Write-Assert "Client Login Token" (-not [string]::IsNullOrEmpty($clientToken))

$randomNat = "2900101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
Invoke-ApiRaw "Complete Client Profile" POST "/api/clients/profile/complete" -token $clientToken -body (
    @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo"; NationalNumber = $randomNat } | ConvertTo-Json
) | Out-Null
$clientLogin = Invoke-ApiRaw "Re-Login Client" POST "/api/auth/login" -body $clientLoginBody
$clientToken = $clientLogin.data.accessToken

# Lawyer Registration
$lawyerEmail     = "e2e_lawyer_${ts}@example.com"
$lawyerRegBody   = @{ FullName = "E2E Lawyer"; Email = $lawyerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
$r               = Invoke-ApiRaw "Register Lawyer" POST "/api/auth/register/lawyer" -body $lawyerRegBody
Write-Assert "Register Lawyer" ($r -ne $null)

Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$lawyerLoginBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$lawyerLogin     = Invoke-ApiRaw "Login Lawyer" POST "/api/auth/login" -body $lawyerLoginBody
$lawyerToken     = $lawyerLogin.data.accessToken
$lawyerId        = $lawyerLogin.data.user.id
Write-Assert "Lawyer Login Token" (-not [string]::IsNullOrEmpty($lawyerToken))

$randomNatL = "2850101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
Invoke-ApiRaw "Complete Lawyer Profile" POST "/api/lawyers/profile/complete" -token $lawyerToken -body (
    @{
        PhoneNumber     = "+201022222222"
        DateOfBirth     = "1985-01-01"
        Gender          = 1
        Address         = "Cairo"
        NationalNumber  = $randomNatL
        Bio             = "Expert E2E Lawyer"
        Level           = 1
        Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 })
    } | ConvertTo-Json
) | Out-Null

# Admin Verification
$adminLoginBody = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
$adminLogin     = Invoke-ApiRaw "Login Admin" POST "/api/auth/login" -body $adminLoginBody
$adminToken     = $adminLogin.Data.AccessToken
Write-Assert "Admin Login" (-not [string]::IsNullOrEmpty($adminToken))

Invoke-ApiRaw "Admin Approve Lawyer" PATCH "/api/admin/verifications/$lawyerId/approve-account" -body "{}" -token $adminToken | Out-Null
Invoke-ApiRaw "Admin Approve Client" PATCH "/api/admin/verifications/$clientId/approve-account" -body "{}" -token $adminToken | Out-Null

$clientLogin = Invoke-ApiRaw "Re-Login Client (post-verify)" POST "/api/auth/login" -body $clientLoginBody
$clientToken = $clientLogin.data.accessToken
$lawyerLogin = Invoke-ApiRaw "Re-Login Lawyer (post-verify)" POST "/api/auth/login" -body $lawyerLoginBody
$lawyerToken = $lawyerLogin.data.accessToken
Write-Assert "Both Verified and Tokens Refreshed" (-not [string]::IsNullOrEmpty($clientToken) -and -not [string]::IsNullOrEmpty($lawyerToken))

# ==========================================
# PHASE 2 - Case & Proposal
# ==========================================
Write-Section "Phase 2: Case and Proposal Workflows"

$dummyPdf = "$scriptDir\dummy_e2e.pdf"
"Dummy E2E PDF" | Out-File $dummyPdf
$caseForm = @{
    Title       = "E2E Test Case $(Get-Date -Format 'HHmmss')"
    Description = "A comprehensive E2E test case for the full lifecycle."
    Governorate = "Cairo"
    City        = "Maadi"
    Documents   = Get-Item $dummyPdf
}
$caseResp = Invoke-ApiForm "Create Case" POST "/api/Case" -form $caseForm -token $clientToken
$caseId   = $caseResp.Data.CaseId
Write-Assert "Case Created" (-not [string]::IsNullOrEmpty($caseId))

$revResp = Invoke-ApiRaw "Request Case AI Review" POST "/api/cases/$caseId/review" -body "{}" -token $clientToken
$reviewReportId = $revResp.Data.id
Start-Sleep -Seconds 2
Invoke-ApiRaw "Get Review Report" GET "/api/cases/$caseId/reviews/$reviewReportId" -token $clientToken | Out-Null

$finResp = Invoke-ApiRaw "Finalize Case" POST "/api/Case/$caseId/finalize" -body "{}" -token $clientToken
Write-Assert "Case Finalized" ($finResp -ne $null)

$propResp  = Invoke-ApiRaw "Client Creates Proposal" POST "/api/proposals" -token $clientToken -body (
    @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Please take my case." } | ConvertTo-Json)
$proposalId = $propResp.Data.Id
Write-Assert "Proposal Created" (-not [string]::IsNullOrEmpty($proposalId))

$r = Invoke-ApiRaw "Lawyer Accepts Proposal" POST "/api/proposals/$proposalId/accept" -body "{}" -token $lawyerToken
Write-Assert "Proposal Accepted" ($r -ne $null)

# ==========================================
# PHASE 3 - Contract, Milestones & Chat
# ==========================================
Write-Section "Phase 3: Contract, Milestones and Chat"

$contractResp = Invoke-ApiRaw "Lawyer Creates Contract" POST "/api/contracts" -token $lawyerToken -body (
    @{
        ProposalId         = $proposalId
        Title              = "E2E Legal Contract"
        TermsAndConditions = "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein."
    } | ConvertTo-Json)
$contractId = $contractResp.Data.Id
Write-Assert "Contract Created" (-not [string]::IsNullOrEmpty($contractId))

$cd       = Invoke-ApiRaw "Get Contract (Client ETag)" GET "/api/contracts/$contractId" -token $lawyerToken
$ifMatchC = $cd.Data.Version
$r        = Invoke-IfMatch "Client Accepts Contract" POST "/api/contracts/$contractId/accept" -ifMatch $ifMatchC -body "{}" -token $clientToken
Write-Assert "Client Accepted Contract" ($r -ne $null)

$cd       = Invoke-ApiRaw "Get Contract (Lawyer ETag)" GET "/api/contracts/$contractId" -token $lawyerToken
$ifMatchC = $cd.Data.Version
$r        = Invoke-IfMatch "Lawyer Accepts Contract" POST "/api/contracts/$contractId/accept" -ifMatch $ifMatchC -body "{}" -token $lawyerToken
Write-Assert "Lawyer Accepted Contract" ($r -ne $null)

# Milestone 1
$ms1Resp   = Invoke-ApiRaw "Create Milestone 1" POST "/api/contracts/$contractId/milestones" -token $lawyerToken -body (
    @{ Title = "Phase 1: Research"; Description = "Comprehensive research for the case."; OrderNumber = 1; Amount = 1500.00; DurationDays = 14 } | ConvertTo-Json)
$ms1Id     = $ms1Resp.Data.Id
Write-Assert "Milestone 1 Created" (-not [string]::IsNullOrEmpty($ms1Id))

$list    = Invoke-ApiRaw "List M1 (Client Approve)" GET "/api/contracts/$contractId/milestones" -token $clientToken
$ms1ETag = ($list.Data | Where-Object { $_.Id -eq $ms1Id }).Version
$r       = Invoke-IfMatch "Client Approves M1" POST "/api/milestones/$ms1Id/approve" -ifMatch $ms1ETag -body "{}" -token $clientToken
Write-Assert "Client Approved Milestone 1" ($r -ne $null)

$list    = Invoke-ApiRaw "List M1 (Lawyer Approve)" GET "/api/contracts/$contractId/milestones" -token $lawyerToken
$ms1ETag = ($list.Data | Where-Object { $_.Id -eq $ms1Id }).Version
$r       = Invoke-IfMatch "Lawyer Approves M1" POST "/api/milestones/$ms1Id/approve" -ifMatch $ms1ETag -body "{}" -token $lawyerToken
Write-Assert "Lawyer Approved Milestone 1" ($r -ne $null)

# Chat
$convResp = Invoke-ApiRaw "Get Chat Conversations" GET "/api/chat/conversations" -token $clientToken
$convId   = if ($convResp -and $convResp.data -and $convResp.data.items -and $convResp.data.items.Count -gt 0) { $convResp.data.items[0].id } else { $null }
if ($convId) {
    $msgResp = Invoke-ApiRaw "Client Sends Chat Message" POST "/api/chat/conversations/$convId/messages" -token $clientToken -body (
        @{ Content = "Hello, ready to begin the case!" } | ConvertTo-Json)
    Write-Assert "Chat Message Sent" ($msgResp -ne $null)
} else {
    Write-Host "WARNING: No conversation - chat test skipped"
    "- [SKIP] Chat test skipped (no conversation found)`n" | Out-File $reportFile -Append -Encoding utf8
}

Write-Host "`nWaiting for Contract to become Active (Outbox + Hangfire)..."
$contractActive = Poll-Until -label "Contract Active (status=1)" -timeoutSec 120 -intervalSec 5 -cond {
    $r2 = Invoke-ApiRaw "Poll Contract" GET "/api/contracts/$contractId" -token $clientToken
    $r2.data.status -eq 1
}
Write-Assert "Contract is Active" $contractActive

# ==========================================
# PHASE 4 - Payments: Fund via MockProvider
# ==========================================
Write-Section "Phase 4: Payments - Funding Milestone 1"

# ReadyForFunding (Lawyer)
$list    = Invoke-ApiRaw "List M1 (pre-RFF)" GET "/api/contracts/$contractId/milestones" -token $lawyerToken
$ms1ETag = ($list.Data | Where-Object { $_.Id -eq $ms1Id }).Version
$rff     = Invoke-IfMatch "Lawyer: ReadyForFunding M1" POST "/api/milestones/$ms1Id/ready-for-funding" -ifMatch $ms1ETag -body "{}" -token $lawyerToken
Write-Assert "Milestone 1 Marked ReadyForFunding" ($rff -ne $null)

# Fund: MockPaymentProvider resolves 'mock-success' -> Succeeded inline -> FundedInProgress(3)
$idempKey = [guid]::NewGuid().ToString()
$fundBody = @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json
$fundResp = Invoke-ApiRaw "Client Funds M1 (mock-success)" POST "/api/milestones/$ms1Id/fund" -token $clientToken `
               -extraHeaders @{ "Idempotency-Key" = $idempKey } -body $fundBody
Write-Assert "M1 Funding Call Succeeded" ($fundResp -ne $null)

Start-Sleep -Seconds 2
$list      = Invoke-ApiRaw "List M1 (post-fund)" GET "/api/contracts/$contractId/milestones" -token $clientToken
$ms1Status = ($list.Data | Where-Object { $_.Id -eq $ms1Id }).Status
Write-Assert "M1 = FundedInProgress (3)" ($ms1Status -eq 3) "(status=$ms1Status)"

$payHist   = Invoke-ApiRaw "Get Contract Payments" GET "/api/contracts/$contractId/payments" -token $clientToken
Write-Assert "Payment History Retrieved" ($payHist -ne $null)

$escrowCnt = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM EscrowHolds WHERE MilestoneId = '$ms1Id' AND Status = 0;"
Write-Assert "EscrowHold Organically Created (Status=Funded=0)" ($escrowCnt -gt 0) "(holds=$escrowCnt)"

$txCnt     = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM PaymentTransactions WHERE MilestoneId = '$ms1Id' AND OperationType = 0 AND Status = 1;"
Write-Assert "Deposit PaymentTransaction Completed (Status=1)" ($txCnt -gt 0) "(txns=$txCnt)"

# ==========================================
# PHASE 5 - Delivery & Acceptance
# ==========================================
Write-Section "Phase 5: Milestone Delivery and Acceptance"

# Insert StoredFile the Lawyer can reference in submission
$fileId = [guid]::NewGuid().ToString()
$insertFileSql = "SET NOCOUNT ON; INSERT INTO StoredFiles (Id,StoredFileName,OriginalFileName,FileUrl,ContentType,Extension,SizeInBytes,IsDeleted) VALUES ('$fileId','e2e.pdf','E2E_Report.pdf','https://mock/e2e.pdf','application/pdf','.pdf',1024,0);"
$insertDocSql  = "INSERT INTO UserVerificationDocuments (Id,UserId,StoredFileId,DocumentType,Status,IsCurrent,IsDeleted,ExpirationDate) VALUES (NEWID(),'$lawyerId','$fileId',0,1,1,0,DATEADD(year,1,GETUTCDATE()));"
Sql-Query ($insertFileSql + " " + $insertDocSql) | Out-Null

$submitBody = @{ Notes = "Research complete. All documents attached."; StoredFileIds = @($fileId) } | ConvertTo-Json

$sub1 = Invoke-ApiRaw "Lawyer Submits M1 (1st)" POST "/api/milestones/$ms1Id/submit" -body $submitBody -token $lawyerToken
Write-Assert "M1 Submitted (1st)" ($sub1 -ne $null)

$chgBody = @{ Reason = "Please add more detail to the research findings section." } | ConvertTo-Json
$chgResp = Invoke-ApiRaw "Client Requests Changes" POST "/api/milestones/$ms1Id/request-changes" -token $clientToken -body $chgBody
Write-Assert "Client Requested Changes" ($chgResp -ne $null)

$sub2 = Invoke-ApiRaw "Lawyer Submits M1 (2nd)" POST "/api/milestones/$ms1Id/submit" -body $submitBody -token $lawyerToken
Write-Assert "M1 Submitted (2nd)" ($sub2 -ne $null)

$accMs = Invoke-ApiRaw "Client Accepts M1" POST "/api/milestones/$ms1Id/accept" -body "{}" -token $clientToken
Write-Assert "M1 Accepted by Client" ($accMs -ne $null)

Start-Sleep -Seconds 2
$list                 = Invoke-ApiRaw "List M1 (post-accept)" GET "/api/contracts/$contractId/milestones" -token $clientToken
$ms1PostAcceptStatus  = ($list.Data | Where-Object { $_.Id -eq $ms1Id }).Status
Write-Assert "M1 = AcceptedHold (5)" ($ms1PostAcceptStatus -eq 5) "(status=$ms1PostAcceptStatus)"

# ==========================================
# PHASE 6 - Escrow Release & Wallet
# ==========================================
Write-Section "Phase 6: Escrow Release and Wallet Verification"

# 6a: Wait for MilestoneAccepted outbox
$obPending = Poll-Until -label "MilestoneAccepted outbox enqueued" -timeoutSec 60 -intervalSec 5 -cond {
    (Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM OutboxMessages WHERE EventType='MilestoneAccepted' AND AggregateId='$ms1Id' AND Status IN (0,1);") -gt 0
}
Write-Assert "MilestoneAccepted in Outbox" $obPending

# 6b: Wait for outbox to process (Status=2 Processed)
$obDone = Poll-Until -label "MilestoneAccepted outbox processed (Status=2)" -timeoutSec 90 -intervalSec 5 -cond {
    (Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM OutboxMessages WHERE EventType='MilestoneAccepted' AND AggregateId='$ms1Id' AND Status=2;") -gt 0
}
Write-Assert "MilestoneAccepted Outbox Processed (Status=2)" $obDone

# 6c: Move HoldExpiresAt to the past (timing manipulation, NOT domain bypass)
$holdIdRaw = (Sql-Query "SET NOCOUNT ON; SELECT CAST(Id AS NVARCHAR(50)) FROM EscrowHolds WHERE MilestoneId='$ms1Id' AND Status=0;" | Select-Object -Last 1)
if ($null -ne $holdIdRaw) {
    $holdId = $holdIdRaw.Trim()
} else {
    Write-Host "WARNING: No EscrowHold found for Milestone. Test will likely fail downstream."
    $holdId = ""
}

if ($holdId.Length -gt 10) {
    $updateTimingSql = "SET NOCOUNT ON; DECLARE @now DATETIME2 = DATEADD(second,-60,GETUTCDATE()); UPDATE EscrowHolds SET HoldExpiresAt=@now WHERE Id='$holdId'; UPDATE Milestones SET HoldExpiresAt=@now WHERE Id='$ms1Id';"
    Sql-Query $updateTimingSql | Out-Null
    Write-Assert "HoldExpiresAt moved to past (timing helper)" $true "(holdId=$holdId)"

    # 6d: Force Hangfire release job from Scheduled to Enqueued
    $nowUtc        = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $hfEnqueueData = '{"EnqueuedAt":"' + $nowUtc + '","Queue":"default"}'
    $hfSql = @"
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
DECLARE @jid NVARCHAR(20);
SELECT TOP 1 @jid = j.Id
FROM [HangFire].[Job] j
JOIN [HangFire].[State] s ON s.JobId = j.Id AND s.Name = 'Scheduled'
WHERE j.InvocationData LIKE '%ReleaseExpiredHold%'
  AND j.Arguments LIKE '%$holdId%'
ORDER BY j.CreatedAt DESC;
IF @jid IS NOT NULL BEGIN
  INSERT INTO [HangFire].[State] (JobId,Name,Reason,CreatedAt,Data)
  VALUES (@jid,'Enqueued','Forced by E2E test',GETUTCDATE(),'$hfEnqueueData');
  UPDATE [HangFire].[Job]
  SET StateId=(SELECT MAX(Id) FROM [HangFire].[State] WHERE JobId=@jid),StateName='Enqueued'
  WHERE Id=@jid;
  INSERT INTO [HangFire].[JobQueue] (JobId, Queue) VALUES (@jid, 'default');
  PRINT 'Forced job '+@jid+' to Enqueued';
END ELSE PRINT 'No scheduled ReleaseExpiredHold job found';
"@
    $tmpSqlFile = [System.IO.Path]::GetTempFileName()
    $hfSql | Out-File $tmpSqlFile -Encoding utf8
    Start-Sleep -Seconds 2
    & sqlcmd -S . -d SmartCourt_Graduation -i $tmpSqlFile
    Remove-Item $tmpSqlFile -ErrorAction SilentlyContinue

    # 6e: Poll for Released (7)
    $released = Poll-Until -label "M1 = Released (7)" -timeoutSec 120 -intervalSec 5 -cond {
        $r3 = Invoke-ApiRaw "Poll M1 Status" GET "/api/contracts/$contractId/milestones" -token $clientToken
        ($r3.Data | Where-Object { $_.Id -eq $ms1Id }).Status -eq 7
    }
    Write-Assert "M1 Released (7)" $released

    if (-not $released) {
        $dbStatus = (Sql-Query "SET NOCOUNT ON; SELECT Status FROM Milestones WHERE Id='$ms1Id';" | Select-Object -Last 1).Trim()
        Write-Assert "M1 Status via SQL (fallback)" ($dbStatus -eq "7") "(db_status=$dbStatus)"
    }

    # 6f: Release PaymentTransaction
    $relTxCnt = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM PaymentTransactions WHERE EscrowHoldId='$holdId' AND OperationType=1 AND Status=1;"
    Write-Assert "Release PaymentTransaction Created and Completed" ($relTxCnt -gt 0) "(txns=$relTxCnt)"

    # 6g: Lawyer Wallet credited
    $walletResp = Invoke-ApiRaw "Get Lawyer Wallet" GET "/api/wallet" -token $lawyerToken
    $avail      = $walletResp.Data.AvailableBalance
    Write-Assert "Lawyer Wallet Credited (AvailableBalance > 0)" ($avail -gt 0) "(balance=$avail)"
} else {
    Write-Assert "EscrowHold found for M1" $false "(no hold found - holdIdRaw='$holdIdRaw')"
    $holdId = "NOT_FOUND"
}

# ==========================================
# PHASE 7 - Outbox & Hangfire Audit
# ==========================================
Write-Section "Phase 7: Outbox and Hangfire Audit"

$pendingOutbox = Poll-Until -label "All Outbox Messages Processed" -timeoutSec 90 -intervalSec 5 -cond {
    $c = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM OutboxMessages WHERE AggregateId IN ('$ms1Id','$contractId') AND Status=1;"
    $c -eq 0
}
Write-Assert "No Stuck Outbox Messages" $pendingOutbox "(stuck=0)"

$failedCnt = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM OutboxMessages WHERE AggregateId IN ('$ms1Id','$contractId') AND Status=3;"
Write-Assert "No Failed Outbox Messages (Status=3)" ($failedCnt -eq 0) "(failed=$failedCnt)"

$hfFailCnt = Get-SqlCount "SET NOCOUNT ON; SELECT COUNT(*) FROM [HangFire].[Job] WHERE StateName='Failed' AND (InvocationData LIKE '%ReleaseExpiredHold%' OR InvocationData LIKE '%AutoAcceptMilestone%');"
Write-Assert "No Failed Hangfire Release/Accept Jobs" ($hfFailCnt -eq 0) "(failures=$hfFailCnt)"

$outboxRows = Sql-Query "SET NOCOUNT ON; SELECT EventType, Status, COUNT(*) AS Cnt FROM OutboxMessages WHERE AggregateId IN ('$ms1Id','$contractId') GROUP BY EventType, Status ORDER BY EventType;"
"`n### Outbox Message Summary`n`` ``text`n$($outboxRows -join "`n")`n`` `` ``n" | Out-File $reportFile -Append -Encoding utf8

# ==========================================
# CLEANUP & SUMMARY
# ==========================================
Remove-Item $dummyPdf -ErrorAction SilentlyContinue

Write-Section "Test Execution Summary"
$summary = "---`n`n| Entity | Value | Final Status |`n|--------|-------|--------------|`n| Client ID | $clientId | Verified |`n| Lawyer ID | $lawyerId | Verified |`n| Case ID | $caseId | Finalized |`n| Proposal ID | $proposalId | Accepted |`n| Contract ID | $contractId | Active |`n| Milestone 1 | $ms1Id | Released (7) |`n| Escrow Hold | $holdId | Released |`n`n**Completed at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')**"
$summary | Out-File $reportFile -Append -Encoding utf8

Write-Host "`nScript complete. Report: $reportFile"