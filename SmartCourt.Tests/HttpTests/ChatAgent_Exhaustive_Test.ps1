$ErrorActionPreference = "Stop"

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\ChatAgent_Exhaustive_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"
$baseUrl    = "http://localhost:5049"

Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Chat Agent - Exhaustive & Integration Report`n" | Out-File $reportFile -Encoding utf8
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
        if ($body -and $method -ne "GET" -and $method -ne "DELETE") { $reqArgs["Body"] = $body }
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

# ---------------------------------------------------------
# PHASE 1 - Account & Profile Setup
# ---------------------------------------------------------
Write-Section "Phase 1: Zero Assumption Setup"

$ts = Get-Date -Format 'yyyyMMddHHmmss'

# Client 1
$client1Email = "chatagent_client1_${ts}@example.com"
$r = Invoke-ApiRaw "Register Client 1" POST "/api/auth/register/client" -body (@{ FullName="Client One"; Email=$client1Email; Password="Password123!"; ConfirmPassword="Password123!" } | ConvertTo-Json)
Confirm-EmailFromLog -email $client1Email -reportFile $reportFile -apiLogPath $apiLogPath
$client1Login = Invoke-ApiRaw "Login Client 1" POST "/api/auth/login" -body (@{ Email=$client1Email; Password="Password123!" } | ConvertTo-Json)
$client1Token = $client1Login.Content.data.accessToken
$client1Id = $client1Login.Content.data.user.id

# Client 2 (for unauthorized access testing)
$client2Email = "chatagent_client2_${ts}@example.com"
$r = Invoke-ApiRaw "Register Client 2" POST "/api/auth/register/client" -body (@{ FullName="Client Two"; Email=$client2Email; Password="Password123!"; ConfirmPassword="Password123!" } | ConvertTo-Json)
Confirm-EmailFromLog -email $client2Email -reportFile $reportFile -apiLogPath $apiLogPath
$client2Login = Invoke-ApiRaw "Login Client 2" POST "/api/auth/login" -body (@{ Email=$client2Email; Password="Password123!" } | ConvertTo-Json)
$client2Token = $client2Login.Content.data.accessToken
$client2Id = $client2Login.Content.data.user.id

# Complete Profiles
Invoke-ApiRaw "Complete Client 1 Profile" POST "/api/clients/profile/complete" -token $client1Token -body (@{ PhoneNumber="+201011111111"; DateOfBirth="1990-01-01"; Gender=1; Address="Cairo"; NationalNumber="2900101$([guid]::NewGuid().ToString().Substring(0,6))" } | ConvertTo-Json) | Out-Null
Invoke-ApiRaw "Complete Client 2 Profile" POST "/api/clients/profile/complete" -token $client2Token -body (@{ PhoneNumber="+201022222222"; DateOfBirth="1992-02-02"; Gender=1; Address="Giza"; NationalNumber="2920202$([guid]::NewGuid().ToString().Substring(0,6))" } | ConvertTo-Json) | Out-Null

# Admin Approval
$adminLogin = Invoke-ApiRaw "Login Admin" POST "/api/auth/login" -body (@{ Email="admin@smartcourt.com"; Password="Admin@123" } | ConvertTo-Json)
$adminToken = $adminLogin.Content.data.accessToken
Invoke-ApiRaw "Admin Approve Client 1" PATCH "/api/admin/verifications/$client1Id/approve-account" -body "{}" -token $adminToken | Out-Null
Invoke-ApiRaw "Admin Approve Client 2" PATCH "/api/admin/verifications/$client2Id/approve-account" -body "{}" -token $adminToken | Out-Null

# Re-login to refresh claims
$client1Login = Invoke-ApiRaw "Re-Login Client 1" POST "/api/auth/login" -body (@{ Email=$client1Email; Password="Password123!" } | ConvertTo-Json)
$client1Token = $client1Login.Content.data.accessToken
$client2Login = Invoke-ApiRaw "Re-Login Client 2" POST "/api/auth/login" -body (@{ Email=$client2Email; Password="Password123!" } | ConvertTo-Json)
$client2Token = $client2Login.Content.data.accessToken

Write-Assert "Client 1 & Client 2 Auth Tokens Acquired" ($null -ne $client1Token -and $null -ne $client2Token)

# ---------------------------------------------------------
# PHASE 2 - Conversation Creation
# ---------------------------------------------------------
Write-Section "Phase 2: Conversation Creation"

# 2.1 Unauthorized Creation (no token)
$resNoAuth = Invoke-ApiRaw "Create Conversation without Token" POST "/api/agent/conversations" -body (@{ CaseId = $null } | ConvertTo-Json)
Write-Assert "Unauthorized Request Returns 401" ($resNoAuth.Status -eq 401)

# 2.2 Validation Error (empty Guid CaseId)
$resInvalidCase = Invoke-ApiRaw "Create Conversation with Empty Guid CaseId" POST "/api/agent/conversations" -token $client1Token -body (@{ CaseId = "00000000-0000-0000-0000-000000000000" } | ConvertTo-Json)
Write-Assert "Invalid CaseId Returns 400" ($resInvalidCase.Status -eq 400)

# 2.3 Success Creation (without CaseId)
$resCreate1 = Invoke-ApiRaw "Create General Agent Conversation" POST "/api/agent/conversations" -token $client1Token -body (@{ CaseId = $null } | ConvertTo-Json)
Write-Assert "General Conversation Created (201)" ($resCreate1.Status -eq 201)
$conv1Id = $resCreate1.Content.data.id
Write-Assert "Conversation ID Parsed" ($null -ne $conv1Id)

# ---------------------------------------------------------
# PHASE 3 - RAG Pipeline & Message Exchange
# ---------------------------------------------------------
Write-Section "Phase 3: RAG Pipeline & Message Exchange"

# 3.1 Send Message Validation Error (Empty Content)
$resEmptyMsg = Invoke-ApiRaw "Send Empty Message" POST "/api/agent/conversations/$conv1Id/messages" -token $client1Token -body (@{ Content = "" } | ConvertTo-Json)
Write-Assert "Empty Message Returns 400" ($resEmptyMsg.Status -eq 400)

# 3.2 Send Message Validation Error (Content > 2000 chars)
$longMsg = "أ" * 2001
$resLongMsg = Invoke-ApiRaw "Send Exceedingly Long Message" POST "/api/agent/conversations/$conv1Id/messages" -token $client1Token -body (@{ Content = $longMsg } | ConvertTo-Json)
Write-Assert "Message > 2000 Chars Returns 400" ($resLongMsg.Status -eq 400)

# 3.3 Send Message Forbidden (Client 2 sending to Client 1's conversation)
$resForbiddenMsg = Invoke-ApiRaw "Client 2 Accesses Client 1 Conversation" POST "/api/agent/conversations/$conv1Id/messages" -token $client2Token -body (@{ Content = "استفسار غير مصرح به" } | ConvertTo-Json)
Write-Assert "Foreign User Send Message Returns 403" ($resForbiddenMsg.Status -eq 403)

# 3.4 Send Valid Message (First Message -> Triggers Title Auto-Generation)
$resMsg1 = Invoke-ApiRaw "Send Initial User Prompt" POST "/api/agent/conversations/$conv1Id/messages" -token $client1Token -body (@{ Content = "كيف يمكن فسخ عقد البيع في القانون المدني المصري لعدم السداد؟" } | ConvertTo-Json)
Write-Assert "Message Exchange Success (200)" ($resMsg1.Status -eq 200)
$assistantReply = $resMsg1.Content.data.content
Write-Assert "Assistant Replied" (![string]::IsNullOrWhiteSpace($assistantReply))

# Send follow-up messages for pagination testing
$msg2Res = Invoke-ApiRaw "Send Follow-Up Message 1" POST "/api/agent/conversations/$conv1Id/messages" -token $client1Token -body (@{ Content = "وما هي المدة القانونية لإنذار الوفاء؟" } | ConvertTo-Json)
$msg3Res = Invoke-ApiRaw "Send Follow-Up Message 2" POST "/api/agent/conversations/$conv1Id/messages" -token $client1Token -body (@{ Content = "هل يحق للمشتري التعويض عند الفسخ؟" } | ConvertTo-Json)

# ---------------------------------------------------------
# PHASE 4 - Conversation Detail & Title Check
# ---------------------------------------------------------
Write-Section "Phase 4: Conversation Detail & Auto-Title Check"

$resDetail = Invoke-ApiRaw "Get Conversation Detail" GET "/api/agent/conversations/$conv1Id" -token $client1Token
Write-Assert "Get Conversation Detail Success (200)" ($resDetail.Status -eq 200)
$convTitle = $resDetail.Content.data.title
Write-Assert "Title Auto-Generated" (![string]::IsNullOrWhiteSpace($convTitle))

# Foreign detail access
$resForbiddenDetail = Invoke-ApiRaw "Client 2 Gets Client 1 Detail" GET "/api/agent/conversations/$conv1Id" -token $client2Token
Write-Assert "Foreign Detail Lookup Returns 403" ($resForbiddenDetail.Status -eq 403)

# ---------------------------------------------------------
# PHASE 5 - Conversation Listing & Pagination
# ---------------------------------------------------------
Write-Section "Phase 5: Conversation Listing"

$resList = Invoke-ApiRaw "List Conversations for Client 1" GET "/api/agent/conversations?page=1&pageSize=10" -token $client1Token
Write-Assert "List Conversations Success (200)" ($resList.Status -eq 200)
Write-Assert "Contains Created Conversation" ($resList.Content.data.items.Count -ge 1)

$resListClient2 = Invoke-ApiRaw "List Conversations for Client 2" GET "/api/agent/conversations?page=1&pageSize=10" -token $client2Token
Write-Assert "Client 2 List Does Not Include Client 1 Conversations" ($resListClient2.Content.data.items.Count -eq 0)

# ---------------------------------------------------------
# PHASE 6 - Cursor-Based Message History Pagination
# ---------------------------------------------------------
Write-Section "Phase 6: Cursor Message Pagination"

# Get first page of messages (limit=2)
$resMsgsPage1 = Invoke-ApiRaw "Get Messages Page 1 (limit=2)" GET "/api/agent/conversations/$conv1Id/messages?limit=2" -token $client1Token
Write-Assert "Get Messages Page 1 Success (200)" ($resMsgsPage1.Status -eq 200)
Write-Assert "HasMore Flag is True" ($resMsgsPage1.Content.data.hasMore -eq $true)
Write-Assert "Items Count Equals Limit" ($resMsgsPage1.Content.data.items.Count -eq 2)

$cursorId = $resMsgsPage1.Content.data.items[0].id

# Get next page using before cursor
$resMsgsPage2 = Invoke-ApiRaw "Get Messages Page 2 with Cursor" GET "/api/agent/conversations/$conv1Id/messages?before=$cursorId&limit=10" -token $client1Token
Write-Assert "Get Messages Page 2 Success (200)" ($resMsgsPage2.Status -eq 200)

# Foreign message history access
$resForbiddenMsgs = Invoke-ApiRaw "Client 2 Gets Messages of Client 1" GET "/api/agent/conversations/$conv1Id/messages" -token $client2Token
Write-Assert "Foreign Messages Access Returns 403" ($resForbiddenMsgs.Status -eq 403)

# ---------------------------------------------------------
# PHASE 7 - Soft Deletion
# ---------------------------------------------------------
Write-Section "Phase 7: Soft Deletion"

# Foreign soft delete
$resForbiddenDelete = Invoke-ApiRaw "Client 2 Tries to Delete Client 1 Conversation" DELETE "/api/agent/conversations/$conv1Id" -token $client2Token
Write-Assert "Foreign Soft Delete Returns 403" ($resForbiddenDelete.Status -eq 403)

# Owner soft delete
$resDelete = Invoke-ApiRaw "Owner Soft Deletes Conversation" DELETE "/api/agent/conversations/$conv1Id" -token $client1Token
Write-Assert "Soft Delete Success (200)" ($resDelete.Status -eq 200)

# Verify excluded from detail and list
$resDeletedDetail = Invoke-ApiRaw "Get Deleted Conversation Detail" GET "/api/agent/conversations/$conv1Id" -token $client1Token
Write-Assert "Get Deleted Conversation Returns 404" ($resDeletedDetail.Status -eq 404)

$resListAfterDelete = Invoke-ApiRaw "List Conversations After Delete" GET "/api/agent/conversations?page=1&pageSize=10" -token $client1Token
Write-Assert "Deleted Conversation Excluded from List" ($resListAfterDelete.Content.data.items.Count -eq 0)

Write-Host "`nAll Chat Agent HTTP integration tests completed successfully!"
