$ErrorActionPreference = "Stop"

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\ChatAndProposals_Exhaustive_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl    = "http://localhost:5049"

Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Chat and Proposals - Exhaustive & Integration Report`n" | Out-File $reportFile -Encoding utf8
"Generated at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n" | Out-File $reportFile -Append -Encoding utf8

# Helper Functions
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
        return @{ Status = $resp.StatusCode; Content = ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue) }
    }
    catch {
        $status = if ($_.Exception.Response.StatusCode) { [int]$_.Exception.Response.StatusCode } else { 500 }
        $errBody = if ($_.ErrorDetails.Message) { $_.ErrorDetails.Message } else { $_.Exception.Message }
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $errBody -reportFile $reportFile
        return @{ Status = $status; Content = $null }
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
        return @{ Status = $resp.StatusCode; Content = ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue) }
    }
    catch {
        $s = if ($_.Exception.Response.StatusCode) { [int]$_.Exception.Response.StatusCode } else { 500 }
        Log-Test -title $title -method $method -url $url -body "(multipart/form-data)" -responseStatus $s -responseBody $_.Exception.Message -reportFile $reportFile
        return @{ Status = $s; Content = $null }
    }
}

function Invoke-IfMatch {
    param([string]$title, [string]$method, [string]$endpoint, [string]$ifMatch, [string]$body = "{}", [string]$token)
    $eh = @{ "If-Match" = $ifMatch }
    return Invoke-ApiRaw -title $title -method $method -endpoint $endpoint -extraHeaders $eh -body $body -token $token
}

# ---------------------------------------------------------
# PHASE 1 - Account & Profile Setup
# ---------------------------------------------------------
Write-Section "Phase 1: Zero Assumption Setup"

$ts = Get-Date -Format 'yyyyMMddHHmmss'

# Client 1
$clientEmail = "chatprop_client1_${ts}@example.com"
$r = Invoke-ApiRaw "Register Client 1" POST "/api/auth/register/client" -body (@{ FullName="Client One"; Email=$clientEmail; Password="Password123!"; ConfirmPassword="Password123!" } | ConvertTo-Json)
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath
$clientLogin = Invoke-ApiRaw "Login Client 1" POST "/api/auth/login" -body (@{ Email=$clientEmail; Password="Password123!" } | ConvertTo-Json)
$client1Token = $clientLogin.Content.data.accessToken
$client1Id = $clientLogin.Content.data.user.id

# Client 2 (for unauthorized access testing)
$client2Email = "chatprop_client2_${ts}@example.com"
$r = Invoke-ApiRaw "Register Client 2" POST "/api/auth/register/client" -body (@{ FullName="Client Two"; Email=$client2Email; Password="Password123!"; ConfirmPassword="Password123!" } | ConvertTo-Json)
Confirm-EmailFromLog -email $client2Email -reportFile $reportFile -apiLogPath $apiLogPath
$client2Login = Invoke-ApiRaw "Login Client 2" POST "/api/auth/login" -body (@{ Email=$client2Email; Password="Password123!" } | ConvertTo-Json)
$client2Token = $client2Login.Content.data.accessToken

# Lawyer 1
$lawyerEmail = "chatprop_lawyer1_${ts}@example.com"
$r = Invoke-ApiRaw "Register Lawyer" POST "/api/auth/register/lawyer" -body (@{ FullName="Lawyer One"; Email=$lawyerEmail; Password="Password123!"; ConfirmPassword="Password123!" } | ConvertTo-Json)
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath
$lawyerLogin = Invoke-ApiRaw "Login Lawyer" POST "/api/auth/login" -body (@{ Email=$lawyerEmail; Password="Password123!" } | ConvertTo-Json)
$lawyerToken = $lawyerLogin.Content.data.accessToken
$lawyerId = $lawyerLogin.Content.data.user.id

# Complete Profiles
Invoke-ApiRaw "Complete Client 1 Profile" POST "/api/clients/profile/complete" -token $client1Token -body (@{ PhoneNumber="+201011111111"; DateOfBirth="1990-01-01"; Gender=1; Address="Cairo"; NationalNumber="2900101$([guid]::NewGuid().ToString().Substring(0,6))" } | ConvertTo-Json) | Out-Null
Invoke-ApiRaw "Complete Lawyer 1 Profile" POST "/api/lawyers/profile/complete" -token $lawyerToken -body (@{ PhoneNumber="+201022222222"; DateOfBirth="1985-01-01"; Gender=1; Address="Cairo"; NationalNumber="2850101$([guid]::NewGuid().ToString().Substring(0,6))"; Bio="Expert Lawyer"; Level=1; Specializations=@(@{ Specialization=1; YearsOfExperience=5; CasesHandled=10 }) } | ConvertTo-Json) | Out-Null

# Admin Approval
$adminLogin = Invoke-ApiRaw "Login Admin" POST "/api/auth/login" -body (@{ Email="admin@smartcourt.com"; Password="Admin@123" } | ConvertTo-Json)
$adminToken = $adminLogin.Content.data.accessToken
Invoke-ApiRaw "Admin Approve Lawyer" PATCH "/api/admin/verifications/$lawyerId/approve-account" -body "{}" -token $adminToken | Out-Null
Invoke-ApiRaw "Admin Approve Client 1" PATCH "/api/admin/verifications/$client1Id/approve-account" -body "{}" -token $adminToken | Out-Null

$client1Login = Invoke-ApiRaw "Re-Login Client 1" POST "/api/auth/login" -body (@{ Email=$clientEmail; Password="Password123!" } | ConvertTo-Json)
$client1Token = $client1Login.Content.data.accessToken
$lawyerLogin = Invoke-ApiRaw "Re-Login Lawyer" POST "/api/auth/login" -body (@{ Email=$lawyerEmail; Password="Password123!" } | ConvertTo-Json)
$lawyerToken = $lawyerLogin.Content.data.accessToken
Write-Assert "Profiles completed and verified" ($client1Token -ne $null -and $lawyerToken -ne $null)

# ---------------------------------------------------------
# PHASE 2 - Case Setup
# ---------------------------------------------------------
Write-Section "Phase 2: Case Initialization"
$dummyPdf = "$scriptDir\dummy_chatprop.pdf"
"Dummy PDF" | Out-File $dummyPdf
$caseResp = Invoke-ApiForm "Create Case" POST "/api/Case" -form @{ Title="Proposal Case"; Description="Testing proposals"; Governorate="Cairo"; City="Maadi"; Documents=Get-Item $dummyPdf } -token $client1Token
$caseId = $caseResp.Content.Data.CaseId
Invoke-ApiRaw "Finalize Case" POST "/api/Case/$caseId/finalize" -body "{}" -token $client1Token | Out-Null
Write-Assert "Case Created & Finalized" (-not [string]::IsNullOrEmpty($caseId))

$dummyGuid = [guid]::NewGuid().ToString()

# ---------------------------------------------------------
# PHASE 3 - Proposals (Exhaustive & Edge Cases)
# ---------------------------------------------------------
Write-Section "Phase 3: Proposals - Edge Cases & Validations"

# GET Availability
$availValid = Invoke-ApiRaw "GET Availability Valid" GET "/api/proposals/cases/$caseId/availability" -token $client1Token
Write-Assert "GET Availability (Valid Case)" ($availValid.Status -eq 200)

$availInvalid = Invoke-ApiRaw "GET Availability 404" GET "/api/proposals/cases/$dummyGuid/availability" -token $client1Token
Write-Assert "GET Availability (Invalid Case -> 404/400)" ($availInvalid.Status -ge 400)

$availUnauth = Invoke-ApiRaw "GET Availability 401" GET "/api/proposals/cases/$caseId/availability"
Write-Assert "GET Availability (No Token -> 401)" ($availUnauth.Status -eq 401)

# POST Proposals (Validation/Auth)
$propBodyEmpty = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "" } | ConvertTo-Json
$rPropEmpty = Invoke-ApiRaw "POST Proposal (Empty Message -> 400)" POST "/api/proposals" -body $propBodyEmpty -token $client1Token
Write-Assert "POST Proposal - Empty Message" ($rPropEmpty.Status -eq 400)

$propBodyXSS = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "<script>alert('xss')</script>" } | ConvertTo-Json
$rPropXSS = Invoke-ApiRaw "POST Proposal (XSS Message -> 201 or 400)" POST "/api/proposals" -body $propBodyXSS -token $client1Token
Write-Assert "POST Proposal - XSS Payload Handled" ($rPropXSS.Status -in @(201, 400))
$xssPropId = if ($rPropXSS.Status -eq 201) { $rPropXSS.Content.data.id } else { $null }

$propBodyMassive = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = ("A" * 10000) } | ConvertTo-Json
$rPropMassive = Invoke-ApiRaw "POST Proposal (Massive String -> 400)" POST "/api/proposals" -body $propBodyMassive -token $client1Token
Write-Assert "POST Proposal - Massive String" ($rPropMassive.Status -eq 400)

$propBodyNoCase = @{ LegalCaseId = $dummyGuid; LawyerUserId = $lawyerId; Message = "Valid" } | ConvertTo-Json
$rPropNoCase = Invoke-ApiRaw "POST Proposal (Invalid Case -> 404/400)" POST "/api/proposals" -body $propBodyNoCase -token $client1Token
Write-Assert "POST Proposal - Invalid Case" ($rPropNoCase.Status -in @(404, 400))

$rPropLawyerRole = Invoke-ApiRaw "POST Proposal (Lawyer Role -> 403)" POST "/api/proposals" -body (@{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Valid" } | ConvertTo-Json) -token $lawyerToken
Write-Assert "POST Proposal - Role Lawyer" ($rPropLawyerRole.Status -in @(401, 403))

# Valid Proposals Lifecycle
# Proposal 1: Created -> Cancelled (by Client)
$propBodyValid = @{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Cancel me" } | ConvertTo-Json
$prop1Resp = Invoke-ApiRaw "POST Proposal 1 (Valid)" POST "/api/proposals" -body $propBodyValid -token $client1Token
$prop1Id = $prop1Resp.Content.Data.Id
Write-Assert "POST Proposal 1 - Created" ($prop1Resp.Status -eq 201)
$cancelUnauth = Invoke-ApiRaw "POST Cancel Proposal (Client 2 -> 403/404)" POST "/api/proposals/$prop1Id/cancel" -body (@{Reason="Not mine"} | ConvertTo-Json) -token $client2Token
Write-Assert "Cancel Proposal - Unauthorized User" ($cancelUnauth.Status -in @(403, 404))
$cancelResp = Invoke-ApiRaw "POST Cancel Proposal (Client 1 -> 200)" POST "/api/proposals/$prop1Id/cancel" -body (@{Reason="Changed mind"} | ConvertTo-Json) -token $client1Token
Write-Assert "Cancel Proposal - Success" ($cancelResp.Status -eq 200)

# Proposal 2: Created -> Rejected (by Lawyer)
$prop2Resp = Invoke-ApiRaw "POST Proposal 2 (Valid)" POST "/api/proposals" -body (@{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Reject me" } | ConvertTo-Json) -token $client1Token
$prop2Id = $prop2Resp.Content.Data.Id
$rejectUnauth = Invoke-ApiRaw "POST Reject Proposal (Client 1 -> 403)" POST "/api/proposals/$prop2Id/reject" -body (@{Reason="Not lawyer"} | ConvertTo-Json) -token $client1Token
Write-Assert "Reject Proposal - Role Client" ($rejectUnauth.Status -in @(401, 403))
$rejectResp = Invoke-ApiRaw "POST Reject Proposal (Lawyer -> 200)" POST "/api/proposals/$prop2Id/reject" -body (@{Reason="Too busy"} | ConvertTo-Json) -token $lawyerToken
Write-Assert "Reject Proposal - Success" ($rejectResp.Status -eq 200)

# Proposal 3: Created -> Accepted -> Terminated
$prop3Resp = Invoke-ApiRaw "POST Proposal 3 (Valid)" POST "/api/proposals" -body (@{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Accept me" } | ConvertTo-Json) -token $client1Token
$prop3Id = $prop3Resp.Content.Data.Id
$acceptResp = Invoke-ApiRaw "POST Accept Proposal (Lawyer -> 200)" POST "/api/proposals/$prop3Id/accept" -body "{}" -token $lawyerToken
Write-Assert "Accept Proposal - Success" ($acceptResp.Status -eq 200)
$termResp = Invoke-ApiRaw "POST Terminate Proposal (Client 1 -> 200)" POST "/api/proposals/$prop3Id/terminate" -body (@{Reason="Never mind"} | ConvertTo-Json) -token $client1Token
Write-Assert "Terminate Proposal - Success" ($termResp.Status -eq 200)

# Proposal 4: Created -> Accepted -> Will be used for Contracts & Chat
$prop4Resp = Invoke-ApiRaw "POST Proposal 4 (Valid - Final)" POST "/api/proposals" -body (@{ LegalCaseId = $caseId; LawyerUserId = $lawyerId; Message = "Proceed with this one" } | ConvertTo-Json) -token $client1Token
$prop4Id = $prop4Resp.Content.Data.Id
$acceptResp2 = Invoke-ApiRaw "POST Accept Proposal (Lawyer -> 200)" POST "/api/proposals/$prop4Id/accept" -body "{}" -token $lawyerToken
Write-Assert "Accept Proposal 4 - Success" ($acceptResp2.Status -eq 200)

# GET Listing & Details
$listResp = Invoke-ApiRaw "GET Proposals Listing" GET "/api/proposals?page=1&pageSize=10" -token $client1Token
Write-Assert "GET Proposals Listing" ($listResp.Status -eq 200 -and $listResp.Content.data.items.Count -gt 0)

$getProp = Invoke-ApiRaw "GET Proposal 4 Detail" GET "/api/proposals/$prop4Id" -token $lawyerToken
Write-Assert "GET Proposal Detail" ($getProp.Status -eq 200)

$getPropUnauth = Invoke-ApiRaw "GET Proposal 4 Detail (Client 2 -> 404/403)" GET "/api/proposals/$prop4Id" -token $client2Token
Write-Assert "GET Proposal Detail - Cross tenant" ($getPropUnauth.Status -in @(403, 404))

# ---------------------------------------------------------
# PHASE 4 - Contracts & Milestones (To unlock Chat & State)
# ---------------------------------------------------------
Write-Section "Phase 4: Contract & Milestones"
$contractResp = Invoke-ApiRaw "Lawyer Creates Contract" POST "/api/contracts" -token $lawyerToken -body (@{ ProposalId = $prop4Id; Title = "Test Contract"; TermsAndConditions = "Terms" } | ConvertTo-Json)
$contractId = $contractResp.Content.Data.Id
Write-Assert "Contract Created" ($contractResp.Status -eq 201)

$cd = Invoke-ApiRaw "Get Contract (Client)" GET "/api/contracts/$contractId" -token $lawyerToken
$ifMatchC = $cd.Content.Data.Version
Invoke-IfMatch "Client Accepts Contract" POST "/api/contracts/$contractId/accept" -ifMatch $ifMatchC -body "{}" -token $client1Token | Out-Null

$cd2 = Invoke-ApiRaw "Get Contract (Lawyer)" GET "/api/contracts/$contractId" -token $lawyerToken
$ifMatchL = $cd2.Content.Data.Version
Invoke-IfMatch "Lawyer Accepts Contract" POST "/api/contracts/$contractId/accept" -ifMatch $ifMatchL -body "{}" -token $lawyerToken | Out-Null

$ms1Resp = Invoke-ApiRaw "Create Milestone 1" POST "/api/contracts/$contractId/milestones" -token $lawyerToken -body (@{ Title = "Phase 1"; Description = "Desc"; OrderNumber = 1; Amount = 1500.00; DurationDays = 14 } | ConvertTo-Json)
$ms1Id = $ms1Resp.Content.Data.Id
Write-Assert "Milestone Created" ($ms1Resp.Status -eq 201)

$list = Invoke-ApiRaw "List M1" GET "/api/contracts/$contractId/milestones" -token $client1Token
$ms1ETag = ($list.Content.Data | Where-Object { $_.Id -eq $ms1Id }).Version
Invoke-IfMatch "Client Approves M1" POST "/api/milestones/$ms1Id/approve" -ifMatch $ms1ETag -body "{}" -token $client1Token | Out-Null

$list2 = Invoke-ApiRaw "List M1 Lawyer" GET "/api/contracts/$contractId/milestones" -token $lawyerToken
$ms1ETag2 = ($list2.Content.Data | Where-Object { $_.Id -eq $ms1Id }).Version
Invoke-IfMatch "Lawyer Approves M1" POST "/api/milestones/$ms1Id/approve" -ifMatch $ms1ETag2 -body "{}" -token $lawyerToken | Out-Null

# ---------------------------------------------------------
# PHASE 5 - Chat (Exhaustive & Edge Cases)
# ---------------------------------------------------------
Write-Section "Phase 5: Chat - Edge Cases & Validations"

# GET Conversations
$convList = Invoke-ApiRaw "GET Chat Conversations" GET "/api/chat/conversations" -token $client1Token
Write-Assert "GET Chat Conversations" ($convList.Status -eq 200)

$convId = if ($convList.Content.data.items.Count -gt 0) { $convList.Content.data.items[0].id } else { $null }

if (-not $convId) {
    Write-Assert "Conversation exists" $false "(Failed to resolve conversation ID)"
} else {
    Write-Assert "Conversation exists" $true "(ID: $convId)"

    # GET Conversation by ID
    $convDetail = Invoke-ApiRaw "GET Conversation Detail (Valid)" GET "/api/chat/conversations/$convId" -token $client1Token
    Write-Assert "GET Conversation Detail" ($convDetail.Status -eq 200)

    $convDetailUnauth = Invoke-ApiRaw "GET Conversation Detail (Client 2 -> 404/403)" GET "/api/chat/conversations/$convId" -token $client2Token
    Write-Assert "GET Conversation Detail - Cross tenant" ($convDetailUnauth.Status -in @(403, 404))

    # POST Message - Exhaustive Validation
    $emptyMsgBody = @{ Content = "" } | ConvertTo-Json
    $msgEmpty = Invoke-ApiRaw "POST Message (Empty -> 400)" POST "/api/chat/conversations/$convId/messages" -body $emptyMsgBody -token $client1Token
    Write-Assert "POST Message - Empty String" ($msgEmpty.Status -eq 400)
    
    $massiveMsgBody = @{ Content = ("B" * 5000) } | ConvertTo-Json
    $msgMassive = Invoke-ApiRaw "POST Message (Massive -> 400/200)" POST "/api/chat/conversations/$convId/messages" -body $massiveMsgBody -token $client1Token
    Write-Assert "POST Message - Massive String (Validates limits)" ($msgMassive.Status -in @(400, 200))
    
    $xssMsgBody = @{ Content = "<script>alert('xss')</script> 😀😀 Emojis ñ Zalgo ̐̐̐" } | ConvertTo-Json
    $msgXSS = Invoke-ApiRaw "POST Message (XSS/Emojis)" POST "/api/chat/conversations/$convId/messages" -body $xssMsgBody -token $client1Token
    Write-Assert "POST Message - Complex Charsets & XSS" ($msgXSS.Status -eq 200)

    $unauthMsgBody = @{ Content = "Intruder!" } | ConvertTo-Json
    $msgUnauth = Invoke-ApiRaw "POST Message (Client 2 -> 403/404)" POST "/api/chat/conversations/$convId/messages" -body $unauthMsgBody -token $client2Token
    Write-Assert "POST Message - Unauthorized Access" ($msgUnauth.Status -in @(403, 404))

    # Happy Path Flow
    Invoke-ApiRaw "Client Sends Message" POST "/api/chat/conversations/$convId/messages" -body (@{ Content = "Hello Lawyer, I accepted the contract." } | ConvertTo-Json) -token $client1Token | Out-Null
    Invoke-ApiRaw "Lawyer Sends Message" POST "/api/chat/conversations/$convId/messages" -body (@{ Content = "Thank you! I will begin work." } | ConvertTo-Json) -token $lawyerToken | Out-Null

    # GET Messages (Pagination & Constraints)
    $msgListValid = Invoke-ApiRaw "GET Messages (Valid)" GET "/api/chat/conversations/$convId/messages?page=1&pageSize=10" -token $client1Token
    Write-Assert "GET Messages Listing" ($msgListValid.Status -eq 200 -and $msgListValid.Content.data.items.Count -gt 0)

    $msgListNeg = Invoke-ApiRaw "GET Messages (Negative Page -> Should resolve to 1/400)" GET "/api/chat/conversations/$convId/messages?page=-5&pageSize=-10" -token $lawyerToken
    Write-Assert "GET Messages Listing - Negative Constraints" ($msgListNeg.Status -in @(200, 400))
}

# ---------------------------------------------------------
# CLEANUP
# ---------------------------------------------------------
Remove-Item $dummyPdf -ErrorAction SilentlyContinue

Write-Section "Test Execution Summary"
$summary = "---`n`n**Completed at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')**`n`nPlease review the markdown logs above for full JSON requests and responses."
$summary | Out-File $reportFile -Append -Encoding utf8

Write-Host "`nScript complete. Exhaustive Report: $reportFile"
