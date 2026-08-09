param(
    [string]$BaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [string]$SqlServer = ".",
    [string]$SqlDatabase = "SmartCourt_dev",
    [switch]$FunctionsOnly
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force
if (-not $ApiLogPath) {
    $ApiLogPath = Join-Path $scriptDir "..\..\SmartCourt\api_log.txt"
}
if (-not $ReportFile) {
    $ReportFile = Join-Path $scriptDir "ContractsNotifications_Report.md"
}

$script:passed = 0
$script:failed = 0
$script:failureMessages = [System.Collections.Generic.List[string]]::new()

"# Contracts Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
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
    $suffix = if ([string]::IsNullOrWhiteSpace($Detail)) { "" } else { " $Detail" }
    if ($Condition) {
        $script:passed++
        Write-Host "PASS: $Name$suffix" -ForegroundColor Green
        "- [PASS] **$Name**$suffix" | Out-File $ReportFile -Append -Encoding utf8
    }
    else {
        $script:failed++
        $message = "$Name$suffix"
        $script:failureMessages.Add($message)
        Write-Host "FAIL: $message" -ForegroundColor Red
        "- [FAIL] **$Name**$suffix" | Out-File $ReportFile -Append -Encoding utf8
    }
}

function Protect-ReportText {
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return $Text }

    $safe = $Text -replace '(?i)("(?:password|confirmPassword|accessToken|refreshToken|token|email|phoneNumber|nationalNumber|destinationReference|paymentMethodReference|providerTransactionId|failureReason)"\s*:\s*")[^"]*(")', '$1[REDACTED]$2'
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
                ConvertTo-Json -Depth 20
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
    $bodyForReport = $Body
    if ($Form.Count -gt 0) {
        $arguments.Form = $Form
        $bodyForReport = $Form | ConvertTo-Json -Depth 10
    }
    elseif ($Body) {
        $arguments.Body = $Body
        $arguments.ContentType = "application/json"
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
                $json = $content | ConvertFrom-Json -Depth 40 -ErrorAction Stop
            }
            catch { $json = $null }
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

function Require-Api {
    param(
        [string]$Name,
        $Response,
        [int[]]$ExpectedStatus = @(200)
    )
    $ok = $Response -and $ExpectedStatus -contains $Response.Status -and
        $Response.Json -and $Response.Json.success
    Assert-Test $Name $ok "(status=$($Response.Status))"
    if (-not $ok) { throw "Setup failed: $Name" }
}

function Login-User {
    param([string]$Title, [string]$Email, [string]$Password)
    $response = Invoke-TestRequest $Title POST "/api/auth/login" -Body (
        @{ Email = $Email; Password = $Password } | ConvertTo-Json)
    Require-Api $Title $response
    return $response.Json.data
}

function Confirm-MockEmailFromLog {
    param([string]$Role, [string]$Email)

    $confirmationUrl = $null
    $escapedEmail = [regex]::Escape($Email)
    for ($attempt = 0; $attempt -lt 15; $attempt++) {
        $fullLog = Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue
        if ($fullLog) {
            $emailMatches = [regex]::Matches(
                $fullLog,
                "(?s)To: ${escapedEmail}.*?href='([^']*)'")
        }
        if ($emailMatches -and $emailMatches.Count -gt 0) {
            $confirmationUrl = $emailMatches[$emailMatches.Count - 1].Groups[1].Value `
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
        throw "Mock Email confirmation was not found for $Role."
    }
    $parsedConfirmation = [regex]::Match(
        $confirmationUrl,
        'userId=([^&]+)&token=(.+)$')
    if (-not $parsedConfirmation.Success) {
        throw "Mock Email confirmation link could not be parsed for $Role."
    }

    $response = Invoke-TestRequest "Confirm $Role Email from mock log" GET `
        "/api/auth/confirm-email?userId=$($parsedConfirmation.Groups[1].Value)&token=$($parsedConfirmation.Groups[2].Value)"
    Require-Api "Mock Email $Role confirmation succeeds" $response
}

function Register-And-PrepareUser {
    param(
        [string]$Role,
        [string]$Email,
        [string]$Password,
        [string]$AdminToken,
        [int]$PhonePrefix
    )

    $route = if ($Role -eq "lawyer") { "lawyer" } else { "client" }
    $register = Invoke-TestRequest "Register $Role" POST "/api/auth/register/$route" -Body (
        @{
            FullName = "Contracts Notifications $Role"
            Email = $Email
            Password = $Password
            ConfirmPassword = $Password
        } | ConvertTo-Json)
    Require-Api "Register $Role" $register @(200, 201)
    Confirm-MockEmailFromLog $Role $Email
    $login = Login-User "Login $role" $Email $Password
    $userId = [string]$login.user.id
    $token = [string]$login.accessToken
    $profileBody = @{
        PhoneNumber = "+20$PhonePrefix$((Get-Random -Minimum 10000000 -Maximum 99999999))"
        DateOfBirth = if ($Role -eq "lawyer") { "1985-01-01" } else { "1990-01-01" }
        Gender = 1
        Address = "Cairo"
        NationalNumber = "29$((Get-Date).ToString('MMddyy'))$((Get-Random -Minimum 100000 -Maximum 999999))"
    }
    if ($Role -eq "lawyer") {
        $profileBody.Bio = "Contracts notification lifecycle lawyer"
        $profileBody.Level = 1
        $profileBody.Specializations = @(
            @{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 })
    }
    $profileRoute = if ($Role -eq "lawyer") {
        "/api/lawyers/profile/complete"
    } else {
        "/api/clients/profile/complete"
    }
    $profile = Invoke-TestRequest "Complete $role profile" POST $profileRoute `
        -Token $token -Body ($profileBody | ConvertTo-Json -Depth 5)
    Require-Api "Complete $role profile" $profile
    $approve = Invoke-TestRequest "Approve $role account" PATCH `
        "/api/admin/verifications/$userId/approve-account" `
        -Token $AdminToken -Body "{}"
    Require-Api "Approve $role account" $approve
    $login = Login-User "Re-login approved $role" $Email $Password
    return [pscustomobject]@{
        Id = [string]$login.user.id
        Token = [string]$login.accessToken
    }
}

function Find-ContractNotification {
    param(
        [string]$Title,
        [string]$Token,
        [string]$Type,
        [string]$ContractId,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $response = Invoke-TestRequest $Title GET "/api/notifications?pageSize=50" `
            -Token $Token
        if ($response.Status -eq 200 -and $response.Json.data.items) {
            $match = @($response.Json.data.items) | Where-Object {
                $_.type -eq $Type -and $_.data.contractId -eq $ContractId
            } | Select-Object -First 1
            if ($match) { return $match }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Assert-ContractNotification {
    param(
        [string]$Name,
        $Notification,
        [string]$Type,
        [string]$Severity,
        [string]$Title,
        [string]$Body,
        [string]$ContractId,
        [string]$ProposalId,
        [string]$CaseId
    )
    Assert-Test $Name (
        $null -ne $Notification -and
        $Notification.type -eq $Type -and
        $Notification.severity -eq $Severity -and
        $Notification.title -eq $Title -and
        $Notification.body -eq $Body -and
        $null -eq $Notification.actionUrl -and
        $Notification.data.contractId -eq $ContractId -and
        $Notification.data.proposalId -eq $ProposalId -and
        $Notification.data.legalCaseId -eq $CaseId)
}

function Get-ContractDetail {
    param([string]$Title, [string]$ContractId, [string]$Token)
    $response = Invoke-TestRequest $Title GET "/api/contracts/$ContractId" `
        -Token $Token
    Require-Api $Title $response
    return $response.Json.data
}

function Invoke-ContractIfMatch {
    param(
        [string]$Title,
        [string]$Method,
        [string]$Endpoint,
        [string]$Token,
        [string]$Version,
        [string]$Body = "{}"
    )
    return Invoke-TestRequest $Title $Method $Endpoint -Token $Token `
        -Body $Body -ExtraHeaders @{ "If-Match" = $Version }
}

function New-CaseProposal {
    param(
        [string]$Label,
        [string]$ClientToken,
        [string]$LawyerToken,
        [string]$LawyerId
    )
    $stamp = Get-Date -Format "HHmmssfff"
    $case = Invoke-TestRequest "$Label - create case" POST "/api/Case" `
        -Token $ClientToken -Form @{
            Title = "$Label case $stamp"
            Description = "Complete case foundation for $Label contract notifications."
            Governorate = "Cairo"
            City = "Maadi"
        }
    Require-Api "$Label - create case" $case
    $caseId = [string]$case.Json.data.caseId
    $review = Invoke-TestRequest "$Label - review case" POST `
        "/api/cases/$caseId/review" -Token $ClientToken -Body "{}"
    Require-Api "$Label - review case" $review
    $finalize = Invoke-TestRequest "$Label - finalize case" POST `
        "/api/Case/$caseId/finalize" -Token $ClientToken -Body "{}"
    Require-Api "$Label - finalize case" $finalize
    $proposal = Invoke-TestRequest "$Label - create proposal" POST `
        "/api/proposals" -Token $ClientToken -Body (
        @{
            LegalCaseId = $caseId
            LawyerUserId = $LawyerId
            Message = "$Label proposal for contract notification lifecycle."
        } | ConvertTo-Json)
    Require-Api "$Label - create proposal" $proposal @(200, 201)
    $proposalId = [string]$proposal.Json.data.id
    $accept = Invoke-TestRequest "$Label - accept proposal" POST `
        "/api/proposals/$proposalId/accept" -Token $LawyerToken -Body "{}"
    Require-Api "$Label - accept proposal" $accept
    return [pscustomobject]@{
        CaseId = $caseId
        ProposalId = $proposalId
    }
}

function New-Contract {
    param(
        [string]$Label,
        [string]$ProposalId,
        [string]$LawyerToken
    )
    $response = Invoke-TestRequest "$Label - create contract" POST `
        "/api/contracts" -Token $LawyerToken -Body (
        @{
            ProposalId = $ProposalId
            Title = "$Label legal representation contract"
            TermsAndConditions = "These complete contract terms are used for the $Label notification lifecycle and are accepted by both participants."
        } | ConvertTo-Json)
    Require-Api "$Label - create contract" $response
    Assert-Test "$Label create envelope retains logical 201" (
        [int]$response.Json.statusCode -eq 201)
    return [pscustomobject]@{
        Id = [string]$response.Json.data.id
        Version = [string]$response.Json.data.version
    }
}

function New-ApprovedMilestone {
    param(
        [string]$Label,
        [string]$ContractId,
        [string]$ClientToken,
        [string]$LawyerToken
    )
    $create = Invoke-TestRequest "$Label - add milestone" POST `
        "/api/contracts/$ContractId/milestones" -Token $LawyerToken -Body (
        @{
            Title = "$Label execution milestone"
            Description = "Approved milestone used for the contract lifecycle."
            OrderNumber = 1
            Amount = 1000.00
            DurationDays = 10
        } | ConvertTo-Json)
    Require-Api "$Label - add milestone" $create @(201)
    $milestoneId = [string]$create.Json.data.id
    $list = Invoke-TestRequest "$Label - list milestone for client ETag" GET `
        "/api/contracts/$ContractId/milestones" -Token $ClientToken
    Require-Api "$Label - list milestone for client ETag" $list
    $version = [string](@($list.Json.data) | Where-Object id -eq $milestoneId).version
    $clientApprove = Invoke-TestRequest "$Label - client approves milestone" POST `
        "/api/milestones/$milestoneId/approve" -Token $ClientToken `
        -Body "{}" -ExtraHeaders @{ "If-Match" = $version }
    Require-Api "$Label - client approves milestone" $clientApprove
    $list = Invoke-TestRequest "$Label - list milestone for lawyer ETag" GET `
        "/api/contracts/$ContractId/milestones" -Token $LawyerToken
    Require-Api "$Label - list milestone for lawyer ETag" $list
    $version = [string](@($list.Json.data) | Where-Object id -eq $milestoneId).version
    $lawyerApprove = Invoke-TestRequest "$Label - lawyer approves milestone" POST `
        "/api/milestones/$milestoneId/approve" -Token $LawyerToken `
        -Body "{}" -ExtraHeaders @{ "If-Match" = $version }
    Require-Api "$Label - lawyer approves milestone" $lawyerApprove
    return $milestoneId
}

function Accept-ContractBoth {
    param(
        [string]$Label,
        [string]$ContractId,
        [string]$ClientToken,
        [string]$LawyerToken
    )
    $detail = Get-ContractDetail "$Label - contract ETag for client acceptance" `
        $ContractId $ClientToken
    $clientAccept = Invoke-ContractIfMatch "$Label - client accepts contract" `
        POST "/api/contracts/$ContractId/accept" $ClientToken $detail.version
    Require-Api "$Label - client accepts contract" $clientAccept
    $detail = Get-ContractDetail "$Label - contract ETag for lawyer acceptance" `
        $ContractId $LawyerToken
    $lawyerAccept = Invoke-ContractIfMatch "$Label - lawyer accepts contract" `
        POST "/api/contracts/$ContractId/accept" $LawyerToken $detail.version
    Require-Api "$Label - lawyer accepts contract" $lawyerAccept
}

function Mark-ReadyAndFund {
    param(
        [string]$Label,
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$ClientToken,
        [string]$LawyerToken
    )
    $list = Invoke-TestRequest "$Label - list milestone before funding" GET `
        "/api/contracts/$ContractId/milestones" -Token $LawyerToken
    Require-Api "$Label - list milestone before funding" $list
    $version = [string](@($list.Json.data) | Where-Object id -eq $MilestoneId).version
    $ready = Invoke-TestRequest "$Label - mark milestone ready for funding" POST `
        "/api/milestones/$MilestoneId/ready-for-funding" `
        -Token $LawyerToken -Body "{}" `
        -ExtraHeaders @{ "If-Match" = $version }
    Require-Api "$Label - mark milestone ready for funding" $ready
    $fund = Invoke-TestRequest "$Label - fund milestone through mock provider" POST `
        "/api/milestones/$MilestoneId/fund" -Token $ClientToken -Body (
        @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Require-Api "$Label - fund milestone through mock provider" $fund
}

if ($FunctionsOnly) { return }

try {
    Write-Section "Health and Contracts authorization boundary"
    $health = Invoke-TestRequest "Health check" GET "/health"
    Assert-Test "API is healthy" ($health.Status -eq 200) "(status=$($health.Status))"
    $unknownId = [guid]::NewGuid().ToString()
    foreach ($test in @(
        @{ Name = "Create requires authentication"; Method = "POST"; Path = "/api/contracts"; Body = "{}" },
        @{ Name = "List requires authentication"; Method = "GET"; Path = "/api/contracts"; Body = "" },
        @{ Name = "Detail requires authentication"; Method = "GET"; Path = "/api/contracts/$unknownId"; Body = "" },
        @{ Name = "Update requires authentication"; Method = "PUT"; Path = "/api/contracts/$unknownId"; Body = "{}" },
        @{ Name = "Accept requires authentication"; Method = "POST"; Path = "/api/contracts/$unknownId/accept"; Body = "{}" },
        @{ Name = "Terminate requires authentication"; Method = "POST"; Path = "/api/contracts/$unknownId/terminate"; Body = "{}" },
        @{ Name = "History requires authentication"; Method = "GET"; Path = "/api/contracts/$unknownId/state-history"; Body = "" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path -Body $test.Body
        Assert-Test $test.Name ($response.Status -eq 401) "(status=$($response.Status))"
    }

    Write-Section "Zero-assumption accounts with mock Email confirmation"
    $stamp = Get-Date -Format "yyyyMMddHHmmssfff"
    $password = "Password123!"
    $attackerEmail = "contracts_attacker_$stamp@example.com"
    $adminLogin = Login-User "Login admin" "admin@smartcourt.com" "Admin@123"
    $adminToken = [string]$adminLogin.accessToken
    $client = Register-And-PrepareUser "client" `
        "contracts_client_$stamp@example.com" $password $adminToken 10
    $lawyer = Register-And-PrepareUser "lawyer" `
        "contracts_lawyer_$stamp@example.com" $password $adminToken 11
    $attacker = Register-And-PrepareUser "attacker" `
        $attackerEmail $password $adminToken 12

    Write-Section "Contract create endpoint validation and creation notification"
    $foundation = New-CaseProposal "primary" $client.Token $lawyer.Token $lawyer.Id
    $missingProposal = Invoke-TestRequest "Create missing ProposalId" POST `
        "/api/contracts" -Token $lawyer.Token -Body (
        @{ Title = "Valid contract title"; TermsAndConditions = "Valid contract terms long enough for validation." } | ConvertTo-Json)
    Assert-Test "Create missing ProposalId returns 400" ($missingProposal.Status -eq 400) "(status=$($missingProposal.Status))"
    $clientCreate = Invoke-TestRequest "Client cannot create contract" POST `
        "/api/contracts" -Token $client.Token -Body (
        @{
            ProposalId = $foundation.ProposalId
            Title = "Client attempted contract"
            TermsAndConditions = "This valid body must still be rejected by role authorization."
        } | ConvertTo-Json)
    Assert-Test "Client create is forbidden" ($clientCreate.Status -eq 403) "(status=$($clientCreate.Status))"
    $longTitle = "ع" * 501
    $invalidCreate = Invoke-TestRequest "Create extreme title" POST `
        "/api/contracts" -Token $lawyer.Token -Body (
        @{
            ProposalId = $foundation.ProposalId
            Title = $longTitle
            TermsAndConditions = "Valid terms remain present while title exceeds its allowed length."
        } | ConvertTo-Json)
    Assert-Test "Extreme create title returns 400" ($invalidCreate.Status -eq 400) "(status=$($invalidCreate.Status))"
    $hostileCreate = Invoke-TestRequest "Create hostile body against unknown proposal" POST `
        "/api/contracts" -Token $lawyer.Token -Body (
        @{
            ProposalId = [guid]::NewGuid()
            Title = "<script>alert('xss')</script> عقد ☠"
            TermsAndConditions = "' OR 1=1; DROP TABLE Contracts;-- with enough bounded text."
        } | ConvertTo-Json)
    Assert-Test "Hostile create is rejected without 500" (
        $hostileCreate.Status -in @(400, 404)) "(status=$($hostileCreate.Status))"
    $contract = New-Contract "primary" $foundation.ProposalId $lawyer.Token
    $created = Find-ContractNotification "Poll client for contract.created" `
        $client.Token "contract.created" $contract.Id
    Assert-ContractNotification "Client receives exact contract.created" $created `
        "contract.created" "Information" "مسودة عقد جديدة" `
        "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها." `
        $contract.Id $foundation.ProposalId $foundation.CaseId
    $duplicateCreate = Invoke-TestRequest "Duplicate contract for proposal" POST `
        "/api/contracts" -Token $lawyer.Token -Body (
        @{
            ProposalId = $foundation.ProposalId
            Title = "Duplicate proposal contract"
            TermsAndConditions = "This otherwise valid contract must be rejected as duplicate."
        } | ConvertTo-Json)
    Assert-Test "Duplicate proposal returns 409" ($duplicateCreate.Status -eq 409) "(status=$($duplicateCreate.Status))"

    Write-Section "List, detail, history, filtering, ownership, and headers"
    $listClient = Invoke-TestRequest "List client contracts" GET `
        "/api/contracts?page=1&pageSize=10" -Token $client.Token
    Assert-Test "Client list contains primary contract" (
        $listClient.Status -eq 200 -and
        @($listClient.Json.data.items | Where-Object id -eq $contract.Id).Count -eq 1)
    $listLawyer = Invoke-TestRequest "List lawyer contracts" GET `
        "/api/contracts?page=1&pageSize=10" -Token $lawyer.Token
    Assert-Test "Lawyer list contains primary contract" (
        $listLawyer.Status -eq 200 -and
        @($listLawyer.Json.data.items | Where-Object id -eq $contract.Id).Count -eq 1)
    $listAttacker = Invoke-TestRequest "Unrelated client list is isolated" GET `
        "/api/contracts?page=1&pageSize=10" -Token $attacker.Token
    Assert-Test "Unrelated list does not leak contract" (
        $listAttacker.Status -eq 200 -and
        @($listAttacker.Json.data.items | Where-Object id -eq $contract.Id).Count -eq 0)
    foreach ($query in @(
        @{ Name = "Negative page"; Value = "page=-1&pageSize=10" },
        @{ Name = "Oversized page"; Value = "page=1&pageSize=101" },
        @{ Name = "Wrong page type"; Value = "page=abc&pageSize=10" },
        @{ Name = "Invalid status"; Value = "status=not-a-status" },
        @{ Name = "Unicode status"; Value = "status=%D8%B9%D9%82%D8%AF%E2%98%A0" }
    )) {
        $response = Invoke-TestRequest "List validation - $($query.Name)" GET `
            "/api/contracts?$($query.Value)" -Token $client.Token
        Assert-Test "List $($query.Name) returns 400" ($response.Status -eq 400) "(status=$($response.Status))"
    }
    $detail = Get-ContractDetail "Get primary contract as client" $contract.Id $client.Token
    $attackerDetail = Invoke-TestRequest "Unrelated user cannot read detail" GET `
        "/api/contracts/$($contract.Id)" -Token $attacker.Token
    Assert-Test "Unrelated detail is forbidden" ($attackerDetail.Status -eq 403) "(status=$($attackerDetail.Status))"
    $missingDetail = Invoke-TestRequest "Unknown contract detail" GET `
        "/api/contracts/$unknownId" -Token $lawyer.Token
    Assert-Test "Unknown detail returns 404" ($missingDetail.Status -eq 404) "(status=$($missingDetail.Status))"
    $badRoute = Invoke-TestRequest "Non-Guid contract route" GET `
        "/api/contracts/not-a-guid" -Token $lawyer.Token
    Assert-Test "Non-Guid detail route returns 404" ($badRoute.Status -eq 404) "(status=$($badRoute.Status))"
    $xmlAccept = Invoke-TestRequest "Detail with unusual Accept header" GET `
        "/api/contracts/$($contract.Id)" -Token $client.Token `
        -ExtraHeaders @{ Accept = "application/xml" }
    Assert-Test "Unusual Accept header never causes 500" ($xmlAccept.Status -in @(200, 406)) "(status=$($xmlAccept.Status))"
    $history = Invoke-TestRequest "Get primary state history" GET `
        "/api/contracts/$($contract.Id)/state-history?page=1&pageSize=20" `
        -Token $client.Token
    Assert-Test "History returns creation audit" (
        $history.Status -eq 200 -and @($history.Json.data.items).Count -ge 1)
    $historyAttacker = Invoke-TestRequest "Unrelated user cannot read history" GET `
        "/api/contracts/$($contract.Id)/state-history" -Token $attacker.Token
    Assert-Test "Unrelated history is forbidden" ($historyAttacker.Status -eq 403) "(status=$($historyAttacker.Status))"
    $historyInvalid = Invoke-TestRequest "History invalid page size" GET `
        "/api/contracts/$($contract.Id)/state-history?page=0&pageSize=999" `
        -Token $client.Token
    Assert-Test "History invalid paging returns 400" ($historyInvalid.Status -eq 400) "(status=$($historyInvalid.Status))"

    Write-Section "Acceptance, update reset, and draft-updated notification"
    $clientAcceptance = Invoke-ContractIfMatch "Client first acceptance" POST `
        "/api/contracts/$($contract.Id)/accept" $client.Token $detail.version
    Require-Api "Client first acceptance" $clientAcceptance
    $acceptanceNotice = Find-ContractNotification `
        "Poll lawyer for contract.acceptance-recorded" $lawyer.Token `
        "contract.acceptance-recorded" $contract.Id
    Assert-ContractNotification "Lawyer receives exact first acceptance" `
        $acceptanceNotice "contract.acceptance-recorded" "Information" `
        "موافقة جديدة على العقد" `
        "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك." `
        $contract.Id $foundation.ProposalId $foundation.CaseId
    $detail = Get-ContractDetail "Refresh contract before update" $contract.Id $lawyer.Token
    $updateBody = @{
        Title = "عقد التمثيل القانوني المعدل ⚖"
        TermsAndConditions = "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد."
    } | ConvertTo-Json
    $clientUpdate = Invoke-TestRequest "Client cannot update draft" PUT `
        "/api/contracts/$($contract.Id)" -Token $client.Token -Body $updateBody `
        -ExtraHeaders @{ "If-Match" = $detail.version }
    Assert-Test "Client update is forbidden" ($clientUpdate.Status -eq 403) "(status=$($clientUpdate.Status))"
    $missingIfMatch = Invoke-TestRequest "Update missing If-Match" PUT `
        "/api/contracts/$($contract.Id)" -Token $lawyer.Token -Body $updateBody
    Assert-Test "Update missing If-Match returns 400" ($missingIfMatch.Status -eq 400) "(status=$($missingIfMatch.Status))"
    $invalidUpdate = Invoke-TestRequest "Update extreme terms" PUT `
        "/api/contracts/$($contract.Id)" -Token $lawyer.Token -Body (
        @{ Title = "Valid updated title"; TermsAndConditions = ("ش" * 20001) } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = $detail.version }
    Assert-Test "Extreme update terms returns 400" ($invalidUpdate.Status -eq 400) "(status=$($invalidUpdate.Status))"
    $update = Invoke-ContractIfMatch "Lawyer updates draft" PUT `
        "/api/contracts/$($contract.Id)" $lawyer.Token $detail.version $updateBody
    Require-Api "Lawyer updates draft" $update
    Assert-Test "Update clears both acceptances" (
        $null -eq $update.Json.data.acceptedByClientAt -and
        $null -eq $update.Json.data.acceptedByLawyerAt)
    $draftNotice = Find-ContractNotification "Poll client for draft update" `
        $client.Token "contract.draft-updated" $contract.Id
    Assert-ContractNotification "Client receives exact draft update" $draftNotice `
        "contract.draft-updated" "Warning" "تم تحديث مسودة العقد" `
        "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك." `
        $contract.Id $foundation.ProposalId $foundation.CaseId
    $staleUpdate = Invoke-ContractIfMatch "Update with stale version" PUT `
        "/api/contracts/$($contract.Id)" $lawyer.Token $detail.version $updateBody
    Assert-Test "Stale update is rejected" ($staleUpdate.Status -in @(409, 412)) "(status=$($staleUpdate.Status))"

    Write-Section "Milestone prerequisite and contract activation notifications"
    $milestoneId = New-ApprovedMilestone "primary" $contract.Id `
        $client.Token $lawyer.Token
    $detail = Get-ContractDetail "Refresh for repeated client acceptance" `
        $contract.Id $client.Token
    $clientAcceptAgain = Invoke-ContractIfMatch "Client accepts revised draft" POST `
        "/api/contracts/$($contract.Id)/accept" $client.Token $detail.version
    Require-Api "Client accepts revised draft" $clientAcceptAgain
    $detailAfterClientAccept = Get-ContractDetail `
        "Refresh after client acceptance" $contract.Id $client.Token
    $duplicateAccept = Invoke-TestRequest "Client repeats acceptance" POST `
        "/api/contracts/$($contract.Id)/accept" -Token $client.Token -Body "{}" `
        -ExtraHeaders @{ "If-Match" = $detailAfterClientAccept.version }
    Assert-Test "Repeated acceptance returns 409" ($duplicateAccept.Status -eq 409) "(status=$($duplicateAccept.Status))"
    $detail = Get-ContractDetail "Refresh for final lawyer acceptance" `
        $contract.Id $lawyer.Token
    $attackerAccept = Invoke-ContractIfMatch "Attacker cannot accept" POST `
        "/api/contracts/$($contract.Id)/accept" $attacker.Token $detail.version
    Assert-Test "Attacker acceptance is forbidden" ($attackerAccept.Status -eq 403) "(status=$($attackerAccept.Status))"
    $lawyerAccept = Invoke-ContractIfMatch "Lawyer final acceptance" POST `
        "/api/contracts/$($contract.Id)/accept" $lawyer.Token $detail.version
    Require-Api "Lawyer final acceptance" $lawyerAccept
    $activatedClient = Find-ContractNotification "Poll client for activation" `
        $client.Token "contract.activated" $contract.Id
    $activatedLawyer = Find-ContractNotification "Poll lawyer for activation" `
        $lawyer.Token "contract.activated" $contract.Id
    foreach ($entry in @(
        @{ Name = "Client receives exact activation"; Value = $activatedClient },
        @{ Name = "Lawyer receives exact activation"; Value = $activatedLawyer }
    )) {
        Assert-ContractNotification $entry.Name $entry.Value `
            "contract.activated" "Success" "تم تفعيل العقد" `
            "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله." `
            $contract.Id $foundation.ProposalId $foundation.CaseId
    }

    Write-Section "Termination without settlement and notifications"
    $detail = Get-ContractDetail "Refresh active contract before termination" `
        $contract.Id $client.Token
    $missingReason = Invoke-ContractIfMatch "Terminate missing reason" POST `
        "/api/contracts/$($contract.Id)/terminate" $client.Token $detail.version "{}"
    Assert-Test "Terminate missing reason returns 400" ($missingReason.Status -eq 400) "(status=$($missingReason.Status))"
    $attackerTerminate = Invoke-ContractIfMatch "Attacker cannot terminate" POST `
        "/api/contracts/$($contract.Id)/terminate" $attacker.Token $detail.version `
        (@{ Reason = "Unauthorized termination attempt." } | ConvertTo-Json)
    Assert-Test "Attacker termination is forbidden" ($attackerTerminate.Status -eq 403) "(status=$($attackerTerminate.Status))"
    $terminate = Invoke-ContractIfMatch "Client terminates unfunded contract" POST `
        "/api/contracts/$($contract.Id)/terminate" $client.Token $detail.version `
        (@{ Reason = "اتفق الطرفان على إنهاء العقد قبل بدء التمويل." } | ConvertTo-Json)
    Require-Api "Client terminates unfunded contract" $terminate
    $requestedLawyer = Find-ContractNotification `
        "Poll lawyer for termination request" $lawyer.Token `
        "contract.termination-requested" $contract.Id
    Assert-ContractNotification "Counterparty receives termination request" `
        $requestedLawyer "contract.termination-requested" "Warning" `
        "تم طلب إنهاء العقد" `
        "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة." `
        $contract.Id $foundation.ProposalId $foundation.CaseId
    $terminatedClient = Find-ContractNotification "Poll client for termination" `
        $client.Token "contract.terminated" $contract.Id
    $terminatedLawyer = Find-ContractNotification "Poll lawyer for termination" `
        $lawyer.Token "contract.terminated" $contract.Id
    foreach ($entry in @(
        @{ Name = "Client receives exact termination"; Value = $terminatedClient },
        @{ Name = "Lawyer receives exact termination"; Value = $terminatedLawyer }
    )) {
        Assert-ContractNotification $entry.Name $entry.Value `
            "contract.terminated" "Warning" "تم إنهاء العقد" `
            "اكتملت إجراءات إنهاء العقد وتسويته." `
            $contract.Id $foundation.ProposalId $foundation.CaseId
    }
    $terminatedDetail = Get-ContractDetail "Get terminated contract" `
        $contract.Id $client.Token
    $terminateAgain = Invoke-ContractIfMatch "Cannot terminate twice" POST `
        "/api/contracts/$($contract.Id)/terminate" $client.Token `
        $terminatedDetail.version `
        (@{ Reason = "Repeated termination." } | ConvertTo-Json)
    Assert-Test "Repeated final termination returns 400" ($terminateAgain.Status -eq 400) "(status=$($terminateAgain.Status))"

    Write-Section "Funded settlement termination lifecycle"
    $settlementFoundation = New-CaseProposal "settlement" `
        $client.Token $lawyer.Token $lawyer.Id
    $settlementContract = New-Contract "settlement" `
        $settlementFoundation.ProposalId $lawyer.Token
    $settlementMilestone = New-ApprovedMilestone "settlement" `
        $settlementContract.Id $client.Token $lawyer.Token
    Accept-ContractBoth "settlement" $settlementContract.Id `
        $client.Token $lawyer.Token
    Mark-ReadyAndFund "settlement" $settlementContract.Id `
        $settlementMilestone $client.Token $lawyer.Token
    $settlementDetail = Get-ContractDetail "Refresh funded contract" `
        $settlementContract.Id $client.Token
    $settledTermination = Invoke-ContractIfMatch `
        "Terminate funded contract with refund settlement" POST `
        "/api/contracts/$($settlementContract.Id)/terminate" `
        $client.Token $settlementDetail.version `
        (@{ Reason = "إنهاء العقد مع رد مبلغ المرحلة الممولة." } | ConvertTo-Json)
    Require-Api "Terminate funded contract with refund settlement" $settledTermination
    $settledClient = Find-ContractNotification `
        "Poll client for settled termination" $client.Token `
        "contract.terminated" $settlementContract.Id
    $settledLawyer = Find-ContractNotification `
        "Poll lawyer for settled termination" $lawyer.Token `
        "contract.terminated" $settlementContract.Id
    Assert-Test "Funded termination notifies client" ($null -ne $settledClient)
    Assert-Test "Funded termination notifies lawyer" ($null -ne $settledLawyer)

    Write-Section "Contract completion lifecycle through mock funding"
    $completionFoundation = New-CaseProposal "completion" `
        $client.Token $lawyer.Token $lawyer.Id
    $completionContract = New-Contract "completion" `
        $completionFoundation.ProposalId $lawyer.Token
    $completionMilestone = New-ApprovedMilestone "completion" `
        $completionContract.Id $client.Token $lawyer.Token
    Accept-ContractBoth "completion" $completionContract.Id `
        $client.Token $lawyer.Token
    Mark-ReadyAndFund "completion" $completionContract.Id `
        $completionMilestone $client.Token $lawyer.Token

    $fileId = [guid]::NewGuid()
    $fileName = "contracts-notifications-$stamp.pdf"
    $fixtureSql = @"
SET NOCOUNT ON;
INSERT INTO StoredFiles
    (Id, StoredFileName, OriginalFileName, FileUrl, ContentType, Extension, SizeInBytes, IsDeleted)
VALUES
    ('$fileId', '$fileName', '$fileName', 'https://mock.local/$fileName', 'application/pdf', '.pdf', 1024, 0);
INSERT INTO UserVerificationDocuments
    (Id, UserId, StoredFileId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate)
VALUES
    (NEWID(), '$($lawyer.Id)', '$fileId', 0, 1, 1, 0, DATEADD(year, 1, GETUTCDATE()));
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $fixtureSql | Out-Null
    Assert-Test "Lawyer-owned stored-file fixture created for HTTP submission" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) { throw "Stored-file fixture creation failed." }
    $submit = Invoke-TestRequest "Lawyer submits funded milestone" POST `
        "/api/milestones/$completionMilestone/submit" -Token $lawyer.Token `
        -Body (
        @{
            Notes = "اكتملت أعمال المرحلة وأصبحت جاهزة للمراجعة."
            StoredFileIds = @($fileId)
        } | ConvertTo-Json)
    Require-Api "Lawyer submits funded milestone" $submit
    $acceptMilestone = Invoke-TestRequest "Client accepts delivered milestone" POST `
        "/api/milestones/$completionMilestone/accept" -Token $client.Token `
        -Body "{}"
    Require-Api "Client accepts delivered milestone" $acceptMilestone

    # The seeded administrator has the legacy Admin role, while this endpoint
    # intentionally requires SuperAdministrator. Promote only this run's
    # disposable user after all negative authorization assertions have run.
    $superAdministratorRoleId = [guid]::NewGuid()
    $roleFixtureSql = @"
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @RoleId uniqueidentifier;
SELECT @RoleId = Id
FROM AspNetRoles
WHERE NormalizedName = 'SUPERADMINISTRATOR';
IF @RoleId IS NULL
BEGIN
    SET @RoleId = '$superAdministratorRoleId';
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@RoleId, 'SuperAdministrator', 'SUPERADMINISTRATOR', CONVERT(nvarchar(36), NEWID()));
END;
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles
    WHERE UserId = '$($attacker.Id)' AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES ('$($attacker.Id)', @RoleId);
END;
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $roleFixtureSql | Out-Null
    Assert-Test "Disposable SuperAdministrator fixture created for escrow release" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) {
        throw "SuperAdministrator role fixture creation failed."
    }
    $releaseAdministrator = Login-User `
        "Refresh disposable SuperAdministrator token" $attackerEmail $password
    $release = Invoke-TestRequest "Admin releases accepted escrow hold" POST `
        "/api/admin/milestones/$completionMilestone/release" `
        -Token $releaseAdministrator.accessToken -Body "{}"
    Require-Api "Admin releases accepted escrow hold" $release
    $completedClient = Find-ContractNotification "Poll client for completion" `
        $client.Token "contract.completed" $completionContract.Id 90
    $completedLawyer = Find-ContractNotification "Poll lawyer for completion" `
        $lawyer.Token "contract.completed" $completionContract.Id 90
    foreach ($entry in @(
        @{ Name = "Client receives exact completion"; Value = $completedClient },
        @{ Name = "Lawyer receives exact completion"; Value = $completedLawyer }
    )) {
        Assert-ContractNotification $entry.Name $entry.Value `
            "contract.completed" "Success" "اكتمل العقد" `
            "اكتملت جميع مراحل العقد وتسوياته بنجاح." `
            $completionContract.Id $completionFoundation.ProposalId `
            $completionFoundation.CaseId
    }

    Write-Section "Unsupported methods and final notification isolation"
    foreach ($test in @(
        @{ Name = "DELETE collection is unsupported"; Method = "DELETE"; Path = "/api/contracts" },
        @{ Name = "PATCH detail is unsupported"; Method = "PATCH"; Path = "/api/contracts/$($contract.Id)" },
        @{ Name = "DELETE detail is unsupported"; Method = "DELETE"; Path = "/api/contracts/$($contract.Id)" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path `
            -Token $lawyer.Token
        Assert-Test $test.Name ($response.Status -eq 405) "(status=$($response.Status))"
    }
    $attackerNotifications = Invoke-TestRequest `
        "Unrelated user notification feed remains isolated" GET `
        "/api/notifications?pageSize=50" -Token $attacker.Token
    $leakedContracts = @($attackerNotifications.Json.data.items | Where-Object {
        $_.data.contractId -in @(
            $contract.Id,
            $settlementContract.Id,
            $completionContract.Id)
    })
    Assert-Test "No Contract notification leaks to unrelated user" (
        $attackerNotifications.Status -eq 200 -and $leakedContracts.Count -eq 0)
}
catch {
    $script:failed++
    $script:failureMessages.Add("Fatal test interruption: $($_.Exception.Message)")
    Write-Host "FATAL: $($_.Exception.Message)" -ForegroundColor Red
    "`n- [FAIL] **Fatal test interruption** $(Protect-ReportText $_.Exception.Message)`n" |
        Out-File $ReportFile -Append -Encoding utf8
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
        "`n### Failures`n" | Out-File $ReportFile -Append -Encoding utf8
        foreach ($failure in $script:failureMessages) {
            "- $(Protect-ReportText $failure)" |
                Out-File $ReportFile -Append -Encoding utf8
        }
    }
}

Write-Host "`nContracts notification HTTP tests complete: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) { exit 1 }
