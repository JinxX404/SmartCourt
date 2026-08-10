param(
    [string]$BaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force
if (-not $ApiLogPath) {
    $ApiLogPath = Join-Path $scriptDir "..\..\SmartCourt\api_log.txt"
}
if (-not $ReportFile) {
    $ReportFile = Join-Path $scriptDir "Notifications_Report.md"
}

$script:passed = 0
$script:failed = 0
$script:failureMessages = [System.Collections.Generic.List[string]]::new()

"# Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
    Out-File $ReportFile -Encoding utf8

function Write-Section {
    param([string]$Title)
    Write-Host "`n=== $Title ===" -ForegroundColor Cyan
    "`n## $Title`n" | Out-File $ReportFile -Append -Encoding utf8
}

function Assert-Test {
    param(
        [string]$Name,
        [bool]$Condition,
        [string]$Detail = ""
    )
    if ($Condition) {
        $script:passed++
        $detailSuffix = if ([string]::IsNullOrWhiteSpace($Detail)) { "" } else { " $Detail" }
        Write-Host "PASS: $Name$detailSuffix" -ForegroundColor Green
        "- [PASS] **$Name**$detailSuffix" | Out-File $ReportFile -Append -Encoding utf8
    }
    else {
        $script:failed++
        $message = "$Name $Detail".Trim()
        $script:failureMessages.Add($message)
        Write-Host "FAIL: $message" -ForegroundColor Red
        "- [FAIL] **$Name** $Detail" | Out-File $ReportFile -Append -Encoding utf8
    }
}

function Protect-ReportText {
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return $Text }

    $safe = $Text -replace '(?i)("(?:password|confirmPassword|accessToken|refreshToken|token|email|phoneNumber|nationalNumber|destinationReference)"\s*:\s*")[^"]*(")', '$1[REDACTED]$2'
    $safe = $safe -replace '(?i)([?&](?:token|email)=)[^&\s"]+', '$1[REDACTED]'
    return $safe
}

function Write-HttpTestLog {
    param(
        [string]$Title,
        [string]$Method,
        [string]$Url,
        [string]$Body,
        [string]$ResponseStatus,
        [string]$ResponseBody
    )

    $output = "### $Title`n`n"
    $output += "**Request:** $Method $Url`n`n"
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        try {
            $formattedBody = $Body | ConvertFrom-Json -ErrorAction Stop |
                ConvertTo-Json -Depth 10
            $output += "**Body:**`n" + '```json' + "`n$formattedBody`n" + '```' + "`n`n"
        }
        catch {
            $output += "**Body:**`n" + '```text' + "`n$Body`n" + '```' + "`n`n"
        }
    }

    $output += "**Response Status:** $ResponseStatus`n`n"
    $output += "**Response Body:**`n"
    if ([string]::IsNullOrWhiteSpace($ResponseBody)) {
        $output += "(Empty)`n"
    }
    else {
        try {
            $formattedResponse = $ResponseBody |
                ConvertFrom-Json -ErrorAction Stop |
                ConvertTo-Json -Depth 30
            $output += '```json' + "`n$formattedResponse`n" + '```' + "`n"
        }
        catch {
            $output += '```text' + "`n$ResponseBody`n" + '```' + "`n"
        }
    }
    $output += "---`n`n"
    $output | Out-File $ReportFile -Append -Encoding utf8
}

function Invoke-TestRequest {
    param(
        [string]$Title,
        [string]$Method,
        [string]$Endpoint,
        [string]$Token = "",
        [string]$Body = "",
        [hashtable]$ExtraHeaders = @{},
        [hashtable]$Form = @{}
    )

    $headers = @{ Accept = "application/json" }
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    foreach ($entry in $ExtraHeaders.GetEnumerator()) {
        $headers[$entry.Key] = $entry.Value
    }

    $url = "$BaseUrl$Endpoint"
    $arguments = @{
        Method = $Method
        Uri = $url
        Headers = $headers
        UseBasicParsing = $true
        SkipHttpErrorCheck = $true
        TimeoutSec = 180
    }
    if ($Form.Count -gt 0) {
        $arguments.Form = $Form
    }
    elseif ($Body) {
        $arguments.Body = $Body
        $arguments.ContentType = "application/json"
    }

    $bodyForReport = $Body
    if ($Form.Count -gt 0) {
        $bodyForReport = $Form | ConvertTo-Json -Depth 10
    }

    try {
        $response = Invoke-WebRequest @arguments
        $content = [string]$response.Content
        Write-HttpTestLog -Title $Title -Method $Method `
            -Url (Protect-ReportText $url) `
            -Body (Protect-ReportText $bodyForReport) `
            -ResponseStatus $response.StatusCode `
            -ResponseBody (Protect-ReportText $content)
        $json = $null
        if ($content -match '^\s*[\{\[]') {
            try {
                $json = $content | ConvertFrom-Json -Depth 30 -ErrorAction Stop
            }
            catch {
                $json = $null
            }
        }
        return [pscustomobject]@{
            Status = [int]$response.StatusCode
            Content = $content
            Json = $json
        }
    }
    catch {
        $content = if ($_.ErrorDetails.Message) {
            $_.ErrorDetails.Message
        } else {
            $_.Exception.Message
        }
        Write-HttpTestLog -Title $Title -Method $Method `
            -Url (Protect-ReportText $url) `
            -Body (Protect-ReportText $bodyForReport) `
            -ResponseStatus "TransportError" `
            -ResponseBody (Protect-ReportText $content)
        return [pscustomobject]@{
            Status = 0
            Content = $content
            Json = $null
        }
    }
}

function Require-Setup {
    param([string]$Name, $Response, [int[]]$ExpectedStatus = @(200))
    $ok = $Response -and $ExpectedStatus -contains $Response.Status -and
        $Response.Json -and $Response.Json.success
    Assert-Test $Name $ok "(status=$($Response.Status))"
    if (-not $ok) { throw "Setup failed: $Name" }
}

function Login-User {
    param([string]$Title, [string]$Email, [string]$Password)
    $response = Invoke-TestRequest $Title POST "/api/auth/login" -Body (
        @{ Email = $Email; Password = $Password } | ConvertTo-Json)
    Require-Setup $Title $response
    return $response.Json.data
}

function Confirm-MockEmailFromLog {
    param(
        [string]$Role,
        [string]$Email
    )

    $confirmationUrl = $null
    $escapedEmail = [regex]::Escape($Email)
    for ($attempt = 0; $attempt -lt 15; $attempt++) {
        $fullLog = Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue
        if ($fullLog -and
            $fullLog -match "(?s)To: ${escapedEmail}.*?href='([^']*)'") {
            $confirmationUrl = $matches[1] `
                -replace "`r`n", "" `
                -replace "`n", "" `
                -replace "&amp;", "&"
            break
        }
        Start-Sleep -Seconds 1
    }

    $found = -not [string]::IsNullOrWhiteSpace($confirmationUrl)
    Assert-Test "Mock Email log contains $Role confirmation" $found
    if (-not $found) {
        throw "Mock Email confirmation was not found in $ApiLogPath for $Role."
    }

    if ($confirmationUrl -notmatch 'userId=([^&]+)&token=(.+)$') {
        Assert-Test "Mock Email $Role confirmation link is parseable" $false
        throw "Mock Email confirmation link could not be parsed for $Role."
    }

    $userId = $matches[1]
    $token = $matches[2]
    $response = Invoke-TestRequest "Confirm $Role Email from mock log" GET `
        "/api/auth/confirm-email?userId=$userId&token=$token"
    $confirmed = $response.Status -eq 200 -and $response.Json.success
    Assert-Test "Mock Email $Role confirmation succeeds" $confirmed `
        "(status=$($response.Status))"
    if (-not $confirmed) {
        throw "Mock Email confirmation failed for $Role."
    }
}

function Find-Notification {
    param(
        [string]$Title,
        [string]$Token,
        [string]$Type,
        [string]$ProposalId,
        [int]$TimeoutSeconds = 45
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $response = Invoke-TestRequest $Title GET "/api/notifications?pageSize=50" -Token $Token
        if ($response.Status -eq 200 -and $response.Json.data.items) {
            $match = @($response.Json.data.items) | Where-Object {
                $_.type -eq $Type -and $_.data.proposalId -eq $ProposalId
            } | Select-Object -First 1
            if ($match) { return $match }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Create-Proposal {
    param([string]$Title, [string]$CaseId, [string]$LawyerId, [string]$Token)
    $response = Invoke-TestRequest $Title POST "/api/proposals" -Token $Token -Body (
        @{
            LegalCaseId = $CaseId
            LawyerUserId = $LawyerId
            Message = "Notification HTTP lifecycle proposal $(Get-Date -Format HHmmssfff)"
        } | ConvertTo-Json)
    Require-Setup $Title $response @(200, 201)
    return [string]$response.Json.data.id
}

try {
    Write-Section "Health and unauthenticated access"
    $health = Invoke-TestRequest "Health check" GET "/health"
    Assert-Test "API is healthy" ($health.Status -eq 200) "(status=$($health.Status))"

    foreach ($test in @(
        @{ Name = "Feed requires authentication"; Method = "GET"; Path = "/api/notifications" },
        @{ Name = "Unread count requires authentication"; Method = "GET"; Path = "/api/notifications/unread-count" },
        @{ Name = "Mark one requires authentication"; Method = "PATCH"; Path = "/api/notifications/$([guid]::NewGuid())/read" },
        @{ Name = "Mark all requires authentication"; Method = "PATCH"; Path = "/api/notifications/read-all" },
        @{ Name = "SignalR negotiate requires authentication"; Method = "POST"; Path = "/hubs/notifications/negotiate?negotiateVersion=1" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path
        Assert-Test $test.Name ($response.Status -eq 401) "(status=$($response.Status))"
    }

    $badToken = Invoke-TestRequest "Malformed bearer token" GET "/api/notifications" -Token "not-a-jwt"
    Assert-Test "Malformed bearer token returns 401" ($badToken.Status -eq 401) "(status=$($badToken.Status))"

    Write-Section "Zero-assumption account and domain setup"
    $stamp = Get-Date -Format "yyyyMMddHHmmssfff"
    $password = "Password123!"
    $clientEmail = "notifications_client_$stamp@example.com"
    $lawyerEmail = "notifications_lawyer_$stamp@example.com"

    $clientRegister = Invoke-TestRequest "Register notification client" POST "/api/auth/register/client" -Body (
        @{ FullName = "Notification Client"; Email = $clientEmail; Password = $password; ConfirmPassword = $password } | ConvertTo-Json)
    Require-Setup "Register notification client" $clientRegister @(200, 201)
    Confirm-MockEmailFromLog "client" $clientEmail
    $clientLogin = Login-User "Login notification client" $clientEmail $password
    $clientId = [string]$clientLogin.user.id
    $clientToken = [string]$clientLogin.accessToken

    $clientProfile = Invoke-TestRequest "Complete notification client profile" POST "/api/clients/profile/complete" -Token $clientToken -Body (
        @{
            PhoneNumber = "+2010$((Get-Random -Minimum 10000000 -Maximum 99999999))"
            DateOfBirth = "1990-01-01"
            Gender = 1
            Address = "Cairo"
            NationalNumber = "29$((Get-Date).ToString('MMddyy'))$((Get-Random -Minimum 100000 -Maximum 999999))"
        } | ConvertTo-Json)
    Require-Setup "Complete notification client profile" $clientProfile

    $lawyerRegister = Invoke-TestRequest "Register notification lawyer" POST "/api/auth/register/lawyer" -Body (
        @{ FullName = "Notification Lawyer"; Email = $lawyerEmail; Password = $password; ConfirmPassword = $password } | ConvertTo-Json)
    Require-Setup "Register notification lawyer" $lawyerRegister @(200, 201)
    Confirm-MockEmailFromLog "lawyer" $lawyerEmail
    $lawyerLogin = Login-User "Login notification lawyer" $lawyerEmail $password
    $lawyerId = [string]$lawyerLogin.user.id
    $lawyerToken = [string]$lawyerLogin.accessToken

    $lawyerProfile = Invoke-TestRequest "Complete notification lawyer profile" POST "/api/lawyers/profile/complete" -Token $lawyerToken -Body (
        @{
            PhoneNumber = "+2011$((Get-Random -Minimum 10000000 -Maximum 99999999))"
            DateOfBirth = "1985-01-01"
            Gender = 1
            Address = "Cairo"
            NationalNumber = "28$((Get-Date).ToString('MMddyy'))$((Get-Random -Minimum 100000 -Maximum 999999))"
            Bio = "Notification lifecycle test lawyer"
            Level = 1
            Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 })
        } | ConvertTo-Json -Depth 5)
    Require-Setup "Complete notification lawyer profile" $lawyerProfile

    $adminLogin = Login-User "Login admin for account approval" "admin@smartcourt.com" "Admin@123"
    $adminToken = [string]$adminLogin.accessToken
    $approveClient = Invoke-TestRequest "Approve notification client" PATCH "/api/admin/verifications/$clientId/approve-account" -Token $adminToken -Body "{}"
    Require-Setup "Approve notification client" $approveClient
    $approveLawyer = Invoke-TestRequest "Approve notification lawyer" PATCH "/api/admin/verifications/$lawyerId/approve-account" -Token $adminToken -Body "{}"
    Require-Setup "Approve notification lawyer" $approveLawyer

    $clientLogin = Login-User "Re-login approved client" $clientEmail $password
    $clientToken = [string]$clientLogin.accessToken
    $lawyerLogin = Login-User "Re-login approved lawyer" $lawyerEmail $password
    $lawyerToken = [string]$lawyerLogin.accessToken

    $adminFeed = Invoke-TestRequest "Authenticated admin may access personal empty inbox" GET "/api/notifications" -Token $adminToken
    Assert-Test "Notification API has no artificial role restriction" ($adminFeed.Status -eq 200) "(status=$($adminFeed.Status))"
    $hubNegotiate = Invoke-TestRequest "Authenticated SignalR negotiate" POST "/hubs/notifications/negotiate?negotiateVersion=1" -Token $lawyerToken
    Assert-Test "Authenticated SignalR hub negotiation succeeds" ($hubNegotiate.Status -eq 200 -and $hubNegotiate.Json.connectionToken) "(status=$($hubNegotiate.Status))"

    $caseResponse = Invoke-TestRequest "Create case for notification lifecycle" POST "/api/Case" -Token $clientToken -Form @{
        Title = "Notification lifecycle case $stamp"
        Description = "A complete case used to verify durable in-app proposal notifications."
        Governorate = "Cairo"
        City = "Maadi"
    }
    Require-Setup "Create case for notification lifecycle" $caseResponse
    $caseId = [string]$caseResponse.Json.data.caseId

    $review = Invoke-TestRequest "Review notification lifecycle case" POST "/api/cases/$caseId/review" -Token $clientToken -Body "{}"
    Require-Setup "Review notification lifecycle case" $review
    $finalize = Invoke-TestRequest "Finalize notification lifecycle case" POST "/api/Case/$caseId/finalize" -Token $clientToken -Body "{}"
    Require-Setup "Finalize notification lifecycle case" $finalize

    Write-Section "Proposal-created and proposal-rejected notification lifecycle"
    $rejectedProposalId = Create-Proposal "Create proposal that will be rejected" $caseId $lawyerId $clientToken
    $createdOne = Find-Notification "Poll lawyer inbox for proposal.created" $lawyerToken "proposal.created" $rejectedProposalId
    Assert-Test "Lawyer receives durable proposal.created" ($null -ne $createdOne)
    Assert-Test "Created payload contract" (
        $createdOne.type -eq "proposal.created" -and
        $createdOne.severity -eq "Information" -and
        $createdOne.title -eq "عرض جديد" -and
        $createdOne.body -eq "أرسل إليك موكل عرضًا جديدًا لمراجعته." -and
        $createdOne.actionUrl -eq "/proposals/$rejectedProposalId" -and
        $createdOne.data.legalCaseId -eq $caseId -and
        $createdOne.data.proposalId -eq $rejectedProposalId)

    $reject = Invoke-TestRequest "Reject first proposal" POST "/api/proposals/$rejectedProposalId/reject" -Token $lawyerToken -Body (
        @{ Reason = "Unable to take this matter during the requested period." } | ConvertTo-Json)
    Require-Setup "Reject first proposal" $reject
    $rejectedNotification = Find-Notification "Poll client inbox for proposal.rejected" $clientToken "proposal.rejected" $rejectedProposalId
    Assert-Test "Client receives durable proposal.rejected" ($null -ne $rejectedNotification)
    Assert-Test "Rejected Arabic payload contract" (
        $rejectedNotification.type -eq "proposal.rejected" -and
        $rejectedNotification.severity -eq "Warning" -and
        $rejectedNotification.title -eq "تم رفض العرض" -and
        $rejectedNotification.body -eq "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر." -and
        $rejectedNotification.actionUrl -eq "/proposals/$rejectedProposalId" -and
        $rejectedNotification.data.legalCaseId -eq $caseId -and
        $rejectedNotification.data.proposalId -eq $rejectedProposalId)

    Write-Section "Proposal-accepted lifecycle and cursor pagination"
    $acceptedProposalId = Create-Proposal "Create proposal that will be accepted" $caseId $lawyerId $clientToken
    $createdTwo = Find-Notification "Poll lawyer inbox for second proposal.created" $lawyerToken "proposal.created" $acceptedProposalId
    Assert-Test "Lawyer receives second proposal.created" ($null -ne $createdTwo)
    $accept = Invoke-TestRequest "Accept second proposal" POST "/api/proposals/$acceptedProposalId/accept" -Token $lawyerToken -Body "{}"
    Require-Setup "Accept second proposal" $accept
    $acceptedNotification = Find-Notification "Poll client inbox for proposal.accepted" $clientToken "proposal.accepted" $acceptedProposalId
    Assert-Test "Client receives durable proposal.accepted" ($null -ne $acceptedNotification)
    Assert-Test "Accepted Arabic payload contract" (
        $acceptedNotification.type -eq "proposal.accepted" -and
        $acceptedNotification.severity -eq "Success" -and
        $acceptedNotification.title -eq "تم قبول العرض" -and
        $acceptedNotification.body -eq "وافق المحامي على عرضك." -and
        $acceptedNotification.actionUrl -eq "/proposals/$acceptedProposalId" -and
        $acceptedNotification.data.legalCaseId -eq $caseId -and
        $acceptedNotification.data.proposalId -eq $acceptedProposalId)

    $firstPage = Invoke-TestRequest "Lawyer feed first cursor page" GET "/api/notifications?pageSize=1&isRead=false" -Token $lawyerToken
    Assert-Test "First cursor page has one item and nextCursor" (
        $firstPage.Status -eq 200 -and @($firstPage.Json.data.items).Count -eq 1 -and
        -not [string]::IsNullOrWhiteSpace($firstPage.Json.data.nextCursor))
    $cursor = [uri]::EscapeDataString([string]$firstPage.Json.data.nextCursor)
    $secondPage = Invoke-TestRequest "Lawyer feed second cursor page" GET "/api/notifications?pageSize=1&isRead=false&cursor=$cursor" -Token $lawyerToken
    Assert-Test "Second cursor page returns a different item" (
        $secondPage.Status -eq 200 -and @($secondPage.Json.data.items).Count -eq 1 -and
        $secondPage.Json.data.items[0].id -ne $firstPage.Json.data.items[0].id)

    Write-Section "Ownership, read state, and idempotency"
    $crossUser = Invoke-TestRequest "Client cannot mutate lawyer notification" PATCH "/api/notifications/$($createdOne.id)/read" -Token $clientToken
    Assert-Test "Cross-user notification is hidden as 404" ($crossUser.Status -eq 404) "(status=$($crossUser.Status))"

    $readOnce = Invoke-TestRequest "Mark accepted notification read" PATCH "/api/notifications/$($acceptedNotification.id)/read" -Token $clientToken
    Assert-Test "Mark one read succeeds" ($readOnce.Status -eq 200 -and $readOnce.Json.data.readAtUtc) "(status=$($readOnce.Status))"
    $firstReadAt = [string]$readOnce.Json.data.readAtUtc
    $readAgain = Invoke-TestRequest "Repeat mark accepted notification read" PATCH "/api/notifications/$($acceptedNotification.id)/read" -Token $clientToken
    Assert-Test "Repeated mark-read preserves timestamp" ($readAgain.Status -eq 200 -and [string]$readAgain.Json.data.readAtUtc -eq $firstReadAt)

    $readFilter = Invoke-TestRequest "Fetch read-only feed" GET "/api/notifications?isRead=true&pageSize=50" -Token $clientToken
    Assert-Test "Read filter contains accepted notification" (
        $readFilter.Status -eq 200 -and @($readFilter.Json.data.items | Where-Object id -eq $acceptedNotification.id).Count -eq 1)
    $unreadFilter = Invoke-TestRequest "Fetch unread-only feed" GET "/api/notifications?isRead=false&pageSize=50" -Token $clientToken
    Assert-Test "Unread filter excludes accepted notification" (
        $unreadFilter.Status -eq 200 -and @($unreadFilter.Json.data.items | Where-Object id -eq $acceptedNotification.id).Count -eq 0)
    $unreadBeforeAll = Invoke-TestRequest "Get unread count before read-all" GET "/api/notifications/unread-count" -Token $clientToken
    Assert-Test "Unread-count endpoint reconciles feed" (
        $unreadBeforeAll.Status -eq 200 -and
        [int]$unreadBeforeAll.Json.data.unreadCount -eq [int]$unreadFilter.Json.data.unreadCount)

    $readAll = Invoke-TestRequest "Mark all client notifications read" PATCH "/api/notifications/read-all" -Token $clientToken
    Assert-Test "Read-all returns zero" ($readAll.Status -eq 200 -and $readAll.Json.data.unreadCount -eq 0)
    $readAllAgain = Invoke-TestRequest "Repeat mark-all read" PATCH "/api/notifications/read-all" -Token $clientToken
    Assert-Test "Repeated read-all is idempotent" ($readAllAgain.Status -eq 200 -and $readAllAgain.Json.data.unreadCount -eq 0)
    $unreadAfterAll = Invoke-TestRequest "Get unread count after read-all" GET "/api/notifications/unread-count" -Token $clientToken
    Assert-Test "Unread count remains zero" ($unreadAfterAll.Status -eq 200 -and $unreadAfterAll.Json.data.unreadCount -eq 0)

    Write-Section "Validation, type coercion, malicious input, and methods"
    foreach ($case in @(
        @{ Name = "Page size below minimum"; Query = "pageSize=0" },
        @{ Name = "Page size above maximum"; Query = "pageSize=51" },
        @{ Name = "Page size wrong type"; Query = "pageSize=abc" },
        @{ Name = "Boolean wrong type"; Query = "isRead=banana" },
        @{ Name = "Malformed cursor"; Query = "cursor=not-base64" },
        @{ Name = "Unicode cursor"; Query = "cursor=$([uri]::EscapeDataString('⚖️ إشعار'))" },
        @{ Name = "Oversized cursor"; Query = "cursor=$('A' * 4096)" },
        @{ Name = "SQL-like cursor"; Query = "cursor=$([uri]::EscapeDataString(`"' OR 1=1 --`"))" }
    )) {
        $response = Invoke-TestRequest $case.Name GET "/api/notifications?$($case.Query)" -Token $clientToken
        Assert-Test "$($case.Name) returns 400" ($response.Status -eq 400) "(status=$($response.Status))"
    }

    $unknown = [guid]::NewGuid()
    $missing = Invoke-TestRequest "Unknown notification id" PATCH "/api/notifications/$unknown/read" -Token $clientToken
    Assert-Test "Unknown notification returns 404" ($missing.Status -eq 404) "(status=$($missing.Status))"
    $badRoute = Invoke-TestRequest "Non-Guid notification route" PATCH "/api/notifications/not-a-guid/read" -Token $clientToken
    Assert-Test "Non-Guid route does not match" ($badRoute.Status -eq 404) "(status=$($badRoute.Status))"
    $postFeed = Invoke-TestRequest "Unsupported POST on feed" POST "/api/notifications" -Token $clientToken -Body "{}"
    Assert-Test "Unsupported POST returns 405" ($postFeed.Status -eq 405) "(status=$($postFeed.Status))"
    $deleteFeed = Invoke-TestRequest "Unsupported DELETE on feed" DELETE "/api/notifications" -Token $clientToken
    Assert-Test "Unsupported DELETE returns 405" ($deleteFeed.Status -eq 405) "(status=$($deleteFeed.Status))"
}
catch {
    Assert-Test "Fatal lifecycle execution" $false $_.Exception.Message
}
finally {
    Write-Section "Execution summary"
    @"

| Metric | Count |
|---|---:|
| Passed assertions | $script:passed |
| Failed assertions | $script:failed |

"@ | Out-File $ReportFile -Append -Encoding utf8

    if ($script:failureMessages.Count -gt 0) {
        "### Failures`n" | Out-File $ReportFile -Append -Encoding utf8
        foreach ($failure in $script:failureMessages) {
            "- $failure" | Out-File $ReportFile -Append -Encoding utf8
        }
    }
    Write-Host "`nNotifications HTTP tests complete: $script:passed passed, $script:failed failed."
}

if ($script:failed -gt 0) { exit 1 }
