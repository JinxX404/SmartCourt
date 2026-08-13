param(
    [string]$BaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [string]$SqlServer = ".",
    [string]$SqlDatabase = "SmartCourt_dev",
    [string]$ExistingLawyerEmail = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ApiLogPath) {
    $ApiLogPath = Join-Path $scriptDir "MilestoneTypes_StripeSandbox_Api.log"
}
if (-not $ReportFile) {
    $ReportFile = Join-Path $scriptDir "MilestoneTypes_StripeSandbox_Report.md"
}

$script:passed = 0
$script:failed = 0
$script:failureMessages = [System.Collections.Generic.List[string]]::new()

@"
# Milestone Types - Real Stripe Sandbox HTTP Test Report

Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')

This suite uses the application's real Stripe Connect provider with Stripe test-mode
objects. Authentication tokens, confirmation links, client secrets, provider object
identifiers, and application/Stripe secrets are redacted.
"@ | Out-File $ReportFile -Encoding utf8

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
    $suffix = if ([string]::IsNullOrWhiteSpace($Detail)) {
        ""
    } else {
        " $Detail"
    }
    if ($Condition) {
        $script:passed++
        Write-Host "PASS: $Name$suffix" -ForegroundColor Green
        "- [PASS] **$Name**$suffix" |
            Out-File $ReportFile -Append -Encoding utf8
    }
    else {
        $script:failed++
        $message = "$Name$suffix"
        $script:failureMessages.Add($message)
        Write-Host "FAIL: $message" -ForegroundColor Red
        "- [FAIL] **$Name**$suffix" |
            Out-File $ReportFile -Append -Encoding utf8
    }
}

# Override the shared reporter's redaction for Stripe-shaped snake_case fields and
# one-time hosted links. The actual values remain available to the running test only.
function Protect-ReportText {
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return $Text }
    $safe = $Text -replace '(?i)("(?:password|confirmPassword|accessToken|refreshToken|token|email|phoneNumber|nationalNumber|destinationReference|paymentMethodReference|providerTransactionId|failureReason|clientSecret|client_secret|secret|url)"\s*:\s*")[^"]*(")', '$1[REDACTED]$2'
    $safe = $safe -replace '(?i)([?&](?:token|email|client_secret)=)[^&\s"]+', '$1[REDACTED]'
    $safe = $safe -replace '(?i)\b(?:sk|pk)_test_[A-Za-z0-9_]+', '[REDACTED_TEST_KEY]'
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
    $output += "**Request:** $Method $(Protect-ReportText $Url)`n`n"
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        $safeBody = Protect-ReportText $Body
        try {
            $formattedBody = $safeBody | ConvertFrom-Json -ErrorAction Stop |
                ConvertTo-Json -Depth 20
            $output += "**Body:**`n" + '```json' +
                "`n$formattedBody`n" + '```' + "`n`n"
        }
        catch {
            $output += "**Body:**`n" + '```text' +
                "`n$safeBody`n" + '```' + "`n`n"
        }
    }
    $output += "**Response Status:** $ResponseStatus`n`n"
    $output += "**Response Body:**`n"
    $safeResponse = Protect-ReportText $ResponseBody
    if ([string]::IsNullOrWhiteSpace($safeResponse)) {
        $output += "(Empty)`n"
    }
    else {
        try {
            $formattedResponse = $safeResponse |
                ConvertFrom-Json -ErrorAction Stop |
                ConvertTo-Json -Depth 30
            $output += '```json' + "`n$formattedResponse`n" + '```' + "`n"
        }
        catch {
            $output += '```text' + "`n$safeResponse`n" + '```' + "`n"
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
        [hashtable]$ExtraHeaders = @{}
    )

    $headers = @{ Accept = "application/json" }
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    foreach ($entry in $ExtraHeaders.GetEnumerator()) {
        $headers[$entry.Key] = $entry.Value
    }
    $arguments = @{
        Method = $Method
        Uri = "$BaseUrl$Endpoint"
        Headers = $headers
        UseBasicParsing = $true
        TimeoutSec = 180
    }
    if ($Body) {
        $arguments.Body = $Body
        $arguments.ContentType = "application/json"
    }

    try {
        $response = Invoke-WebRequest @arguments
        $content = [string]$response.Content
        $status = [int]$response.StatusCode
    }
    catch {
        $status = 0
        $content = if ($_.ErrorDetails.Message) {
            [string]$_.ErrorDetails.Message
        } else {
            [string]$_.Exception.Message
        }
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            try {
                $reader = [IO.StreamReader]::new(
                    $_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                $reader.Dispose()
                if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
                    $content = $responseBody
                }
            } catch { }
        }
    }

    Write-HttpTestLog -Title $Title -Method $Method `
        -Url "$BaseUrl$Endpoint" -Body $Body -ResponseStatus $status `
        -ResponseBody $content
    $json = $null
    if ($content -match '^\s*[\{\[]') {
        $json = $content | ConvertFrom-Json -ErrorAction SilentlyContinue
    }
    return [pscustomobject]@{
        Status = $status
        Content = $content
        Json = $json
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
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $fullLog = Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue
        if ($fullLog) {
            $matches = [regex]::Matches(
                $fullLog,
                "(?s)To: ${escapedEmail}.*?href='([^']*)'")
            if ($matches.Count -gt 0) {
                $confirmationUrl =
                    $matches[$matches.Count - 1].Groups[1].Value `
                    -replace "`r`n", "" `
                    -replace "`n", "" `
                    -replace "&amp;", "&"
                break
            }
        }
        Start-Sleep -Seconds 1
    }
    $found = -not [string]::IsNullOrWhiteSpace($confirmationUrl)
    Assert-Test "Mock Email log contains $Role confirmation" $found
    if (-not $found) {
        throw "Mock Email confirmation was not found for $Role."
    }
    $parsed = [regex]::Match(
        $confirmationUrl,
        'userId=([^&]+)&token=(.+)$')
    if (-not $parsed.Success) {
        throw "Mock Email confirmation link could not be parsed for $Role."
    }
    $response = Invoke-TestRequest "Confirm $Role Email from mock log" GET `
        "/api/auth/confirm-email?userId=$($parsed.Groups[1].Value)&token=$($parsed.Groups[2].Value)"
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

    $register = Invoke-TestRequest "Register $Role" POST `
        "/api/auth/register/$Role" -Body (@{
            FullName = "Stripe sandbox milestone $Role"
            Email = $Email
            Password = $Password
            ConfirmPassword = $Password
        } | ConvertTo-Json)
    Require-Api "Register $Role" $register @(200, 201)
    Confirm-MockEmailFromLog $Role $Email
    $login = Login-User "Login $Role" $Email $Password
    $profile = @{
        PhoneNumber = "+20$PhonePrefix$((Get-Random -Minimum 10000000 -Maximum 99999999))"
        DateOfBirth = if ($Role -eq "lawyer") { "1985-01-01" } else { "1990-01-01" }
        Gender = 1
        Address = "Cairo"
        NationalNumber = "29$((Get-Date).ToString('MMddyy'))$((Get-Random -Minimum 100000 -Maximum 999999))"
    }
    if ($Role -eq "lawyer") {
        $profile.Bio = "Stripe sandbox milestone workflow lawyer"
        $profile.Level = 1
        $profile.Specializations = @(
            @{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 })
    }
    $profileRoute = if ($Role -eq "lawyer") {
        "/api/lawyers/profile/complete"
    } else {
        "/api/clients/profile/complete"
    }
    $complete = Invoke-TestRequest "Complete $Role profile" POST `
        $profileRoute -Token $login.accessToken `
        -Body ($profile | ConvertTo-Json -Depth 5)
    Require-Api "Complete $Role profile" $complete
    $approve = Invoke-TestRequest "Approve $Role account" PATCH `
        "/api/admin/verifications/$($login.user.id)/approve-account" `
        -Token $AdminToken -Body "{}"
    Require-Api "Approve $Role account" $approve
    $login = Login-User "Re-login approved $Role" $Email $Password
    return [pscustomobject]@{
        Id = [string]$login.user.id
        Token = [string]$login.accessToken
    }
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

function Assert-Status {
    param([string]$Name, $Response, [int[]]$Expected)
    Assert-Test $Name ($null -ne $Response -and $Expected -contains $Response.Status) `
        "(status=$($Response.Status), expected=$($Expected -join ','))"
}

function Test-EnumValue {
    param($Actual, [int]$Number, [string]$Name)
    return [string]$Actual -eq [string]$Number -or
        [string]$Actual -eq $Name
}

function Invoke-MultipartTestRequest {
    param(
        [string]$Title,
        [string]$Endpoint,
        [string]$Token,
        [hashtable]$Fields
    )

    Add-Type -AssemblyName System.Net.Http
    $client = [System.Net.Http.HttpClient]::new()
    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::Post,
        "$BaseUrl$Endpoint")
    $request.Headers.Authorization =
        [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
    $content = [System.Net.Http.MultipartFormDataContent]::new()
    foreach ($entry in $Fields.GetEnumerator()) {
        $content.Add(
            [System.Net.Http.StringContent]::new([string]$entry.Value),
            [string]$entry.Key)
    }
    $request.Content = $content
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        Write-HttpTestLog -Title $Title -Method "POST" `
            -Url "$BaseUrl$Endpoint" `
            -Body ($Fields | ConvertTo-Json) `
            -ResponseStatus ([int]$response.StatusCode) `
            -ResponseBody (Protect-ReportText $body)
        $json = $null
        if ($body -match '^\s*[\{\[]') {
            $json = $body | ConvertFrom-Json -ErrorAction SilentlyContinue
        }
        return [pscustomobject]@{
            Status = [int]$response.StatusCode
            Content = $body
            Json = $json
        }
    }
    finally {
        $content.Dispose()
        $request.Dispose()
        $client.Dispose()
    }
}

function Invoke-StripeSetupIntentConfirmation {
    param([string]$SetupIntentId, [string]$StripeSecretKey)

    $authorization = [Convert]::ToBase64String(
        [Text.Encoding]::ASCII.GetBytes("${StripeSecretKey}:"))
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Method POST `
            -Uri "https://api.stripe.com/v1/setup_intents/$SetupIntentId/confirm" `
            -Headers @{ Authorization = "Basic $authorization" } `
            -ContentType "application/x-www-form-urlencoded" `
            -Body "payment_method=pm_card_visa"
        $json = $response.Content | ConvertFrom-Json
        Write-HttpTestLog -Title "Confirm SetupIntent with Stripe test PaymentMethod" `
            -Method "POST" -Url "https://api.stripe.com/v1/setup_intents/[REDACTED]/confirm" `
            -Body "payment_method=pm_card_visa" `
            -ResponseStatus ([int]$response.StatusCode) `
            -ResponseBody (@{
                id = "[REDACTED]"
                status = $json.status
                livemode = $json.livemode
                payment_method = "[REDACTED]"
            } | ConvertTo-Json)
        return [pscustomobject]@{
            Status = [int]$response.StatusCode
            Json = $json
        }
    }
    catch {
        $status = 0
        $message = $_.Exception.Message
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            try {
                $reader = [IO.StreamReader]::new(
                    $_.Exception.Response.GetResponseStream())
                $message = $reader.ReadToEnd()
                $reader.Dispose()
            } catch { }
        }
        Write-HttpTestLog -Title "Confirm SetupIntent with Stripe test PaymentMethod" `
            -Method "POST" -Url "https://api.stripe.com/v1/setup_intents/[REDACTED]/confirm" `
            -Body "payment_method=pm_card_visa" `
            -ResponseStatus $status `
            -ResponseBody (Protect-ReportText $message)
        return [pscustomobject]@{ Status = $status; Json = $null }
    }
}

function Get-Milestone {
    param(
        [string]$Title,
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$Token
    )
    $response = Invoke-TestRequest $Title GET `
        "/api/contracts/$ContractId/milestones" -Token $Token
    Require-Api $Title $response
    return @($response.Json.data) |
        Where-Object { [string]$_.id -eq $MilestoneId } |
        Select-Object -First 1
}

function Approve-Milestone {
    param(
        [string]$Label,
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$Token
    )
    $milestone = Get-Milestone "$Label - read current milestone version" `
        $ContractId $MilestoneId $Token
    $response = Invoke-TestRequest "$Label - approve milestone" POST `
        "/api/milestones/$MilestoneId/approve" -Token $Token -Body "{}" `
        -ExtraHeaders @{ "If-Match" = [string]$milestone.version }
    Require-Api "$Label - approve milestone" $response
}

function Add-LawyerFileFixture {
    param([string]$LawyerId)
    $fileId = [guid]::NewGuid()
    $fileName = "milestone-types-stripe-$($fileId.ToString('N')).pdf"
    $sql = @"
SET NOCOUNT ON;
INSERT INTO StoredFiles
    (Id, StoredFileName, OriginalFileName, FileUrl, ContentType, Extension, SizeInBytes, IsDeleted)
VALUES
    ('$fileId', '$fileName', '$fileName', 'https://test.invalid/$fileName', 'application/pdf', '.pdf', 1024, 0);
INSERT INTO UserVerificationDocuments
    (Id, UserId, StoredFileId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate)
VALUES
    (NEWID(), '$LawyerId', '$fileId', 0, 1, 1, 0, DATEADD(year, 1, GETUTCDATE()));
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $sql | Out-Null
    Assert-Test "Create lawyer-owned submission file fixture" ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) { throw "Could not create submission file fixture." }
    return [string]$fileId
}

function Wait-ForExpenseRelease {
    param(
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$Token,
        [int]$TimeoutSeconds = 120
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $milestone = Get-Milestone "Poll expense release status" `
            $ContractId $MilestoneId $Token
        if (Test-EnumValue $milestone.status 7 "Released") {
            return $milestone
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    return $milestone
}

try {
    Write-Section "Provider safety and authorization boundaries"
    $config = Invoke-TestRequest "Read active payment provider configuration" GET `
        "/api/payments/config"
    Require-Api "Payment provider configuration is reachable" $config
    Assert-Test "Real Stripe Connect provider is active" `
        ($config.Json.data.providerCode -eq "StripeConnect")
    Assert-Test "Provider is strictly sandbox-only" `
        ($config.Json.data.sandboxOnly -eq $true)
    Assert-Test "Publishable key is a Stripe test key" `
        ([string]$config.Json.data.publishableKey).StartsWith("pk_test_")

    $unknownId = [guid]::NewGuid().ToString()
    foreach ($boundary in @(
        @{ Name = "Anonymous milestone list is rejected"; Method = "GET"; Path = "/api/contracts/$unknownId/milestones"; Body = "" },
        @{ Name = "Anonymous milestone creation is rejected"; Method = "POST"; Path = "/api/contracts/$unknownId/milestones"; Body = "{}" },
        @{ Name = "Anonymous funding is rejected"; Method = "POST"; Path = "/api/milestones/$unknownId/fund"; Body = "{}" },
        @{ Name = "Anonymous payout account access is rejected"; Method = "GET"; Path = "/api/wallet/payout-account"; Body = "" }
    )) {
        $response = Invoke-TestRequest $boundary.Name $boundary.Method `
            $boundary.Path -Body $boundary.Body
        Assert-Status $boundary.Name $response @(401)
    }

    Write-Section "Zero-assumption users and mock Email confirmation"
    $stamp = Get-Date -Format "yyyyMMddHHmmssfff"
    $password = "Password123!"
    $admin = Login-User "Login test SuperAdministrator" `
        "admin@smartcourt.com" "Admin@123"
    $client = Register-And-PrepareUser "client" `
        "stripe_milestone_client_$stamp@example.com" $password `
        $admin.accessToken 10
    if ([string]::IsNullOrWhiteSpace($ExistingLawyerEmail)) {
        $lawyer = Register-And-PrepareUser "lawyer" `
            "stripe_milestone_lawyer_$stamp@example.com" $password `
            $admin.accessToken 11
    }
    else {
        $existingLawyer = Login-User "Login existing onboarded test lawyer" `
            $ExistingLawyerEmail $password
        $lawyer = [pscustomobject]@{
            Id = [string]$existingLawyer.user.id
            Token = [string]$existingLawyer.accessToken
        }
    }

    Write-Section "Real Stripe saved payment method"
    $developmentSettingsPath = Join-Path $scriptDir `
        "..\..\SmartCourt\appsettings.Development.json"
    $settings = Get-Content $developmentSettingsPath -Raw | ConvertFrom-Json
    $stripeSecretKey = [string]$settings.PaymentProvider.Stripe.SecretKey
    Assert-Test "Stripe secret is test-mode only" `
        ($stripeSecretKey.StartsWith("sk_test_"))
    if (-not $stripeSecretKey.StartsWith("sk_test_")) {
        throw "Refusing to run: a Stripe test secret key is required."
    }

    $setup = Invoke-TestRequest "Create SetupIntent through Smart Court" POST `
        "/api/payment-methods/setup-session" -Token $client.Token -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Require-Api "Create SetupIntent through Smart Court" $setup
    $setupConfirmation = Invoke-StripeSetupIntentConfirmation `
        ([string]$setup.Json.data.setupIntentId) $stripeSecretKey
    Assert-Test "Stripe confirms the test SetupIntent" `
        ($setupConfirmation.Status -eq 200 -and
         $setupConfirmation.Json.status -eq "succeeded" -and
         $setupConfirmation.Json.livemode -eq $false)

    $methods = Invoke-TestRequest "List Stripe-backed saved payment methods" GET `
        "/api/payment-methods" -Token $client.Token
    Require-Api "List Stripe-backed saved payment methods" $methods
    $savedCard = @($methods.Json.data) |
        Where-Object { $_.brand -eq "visa" -and $_.last4 -eq "4242" } |
        Select-Object -First 1
    Assert-Test "Stripe test Visa is visible through Smart Court" ($null -ne $savedCard)
    if ($null -eq $savedCard) { throw "Stripe test card was not saved." }
    $paymentMethodReference = [string]$savedCard.paymentMethodReference
    $setDefault = Invoke-TestRequest "Set Stripe test card as default" PUT `
        "/api/payment-methods/$paymentMethodReference/default" `
        -Token $client.Token -Body "{}"
    Require-Api "Set Stripe test card as default" $setDefault

    Write-Section "Stripe Connect test payout onboarding"
    $payout = Invoke-TestRequest "Read lawyer payout account" GET `
        "/api/wallet/payout-account" -Token $lawyer.Token
    Require-Api "Read lawyer payout account" $payout
    if ($null -eq $payout.Json.data -or
        -not ($payout.Json.data.transfersEnabled -and $payout.Json.data.payoutsEnabled)) {
        $onboarding = Invoke-TestRequest "Create Stripe hosted onboarding link" POST `
            "/api/wallet/payout-account/onboarding-link" `
            -Token $lawyer.Token -Body "{}"
        Require-Api "Create Stripe hosted onboarding link" $onboarding
        $onboardingUrl = [string]$onboarding.Json.data.url
        Write-Host "`nSTRIPE_TEST_ONBOARDING_URL=$onboardingUrl" -ForegroundColor Yellow
        Write-Host "Complete the hosted Stripe TEST onboarding, then return here." `
            -ForegroundColor Yellow
        Read-Host "Press Enter after Stripe reports onboarding complete" | Out-Null

        $deadline = (Get-Date).AddSeconds(150)
        do {
            $payout = Invoke-TestRequest "Synchronize Stripe payout account" GET `
                "/api/wallet/payout-account" -Token $lawyer.Token
            if ($payout.Status -eq 200 -and
                $payout.Json.data.transfersEnabled -and
                $payout.Json.data.payoutsEnabled) { break }
            Start-Sleep -Seconds 3
        } while ((Get-Date) -lt $deadline)
    }
    Assert-Test "Stripe test recipient can receive transfers" `
        ($payout.Json.data.transfersEnabled -eq $true)
    Assert-Test "Stripe test recipient can receive payouts" `
        ($payout.Json.data.payoutsEnabled -eq $true)
    if (-not ($payout.Json.data.transfersEnabled -and $payout.Json.data.payoutsEnabled)) {
        throw "Stripe payout onboarding is incomplete."
    }

    Write-Section "Contract foundation and Draft milestone types"
    $case = Invoke-MultipartTestRequest "Create a fresh client case" `
        "/api/Case" $client.Token @{
            Title = "Stripe milestone types case $stamp"
            Description = "Real Stripe sandbox verification for Standard and Expense milestones."
            Governorate = "Cairo"
            City = "Maadi"
        }
    Require-Api "Create a fresh client case" $case
    $caseId = [string]$case.Json.data.caseId
    $review = Invoke-TestRequest "Run case review" POST `
        "/api/cases/$caseId/review" -Token $client.Token -Body "{}"
    Require-Api "Run case review" $review
    $finalize = Invoke-TestRequest "Finalize case" POST `
        "/api/Case/$caseId/finalize" -Token $client.Token -Body "{}"
    Require-Api "Finalize case" $finalize
    $proposal = Invoke-TestRequest "Client proposes lawyer engagement" POST `
        "/api/proposals" -Token $client.Token -Body (@{
            LegalCaseId = $caseId
            LawyerUserId = $lawyer.Id
            Message = "Real Stripe sandbox milestone type workflow."
        } | ConvertTo-Json)
    Require-Api "Client proposes lawyer engagement" $proposal @(200, 201)
    $proposalId = [string]$proposal.Json.data.id
    $acceptProposal = Invoke-TestRequest "Lawyer accepts proposal" POST `
        "/api/proposals/$proposalId/accept" -Token $lawyer.Token -Body "{}"
    Require-Api "Lawyer accepts proposal" $acceptProposal
    $contract = Invoke-TestRequest "Lawyer creates Draft contract" POST `
        "/api/contracts" -Token $lawyer.Token -Body (@{
            ProposalId = $proposalId
            Title = "Milestone types Stripe sandbox contract"
            TermsAndConditions = "Complete terms for real Stripe sandbox verification of Standard deliverables and Expense reimbursement."
        } | ConvertTo-Json)
    Require-Api "Lawyer creates Draft contract" $contract @(200, 201)
    $contractId = [string]$contract.Json.data.id

    $clientCreate = Invoke-TestRequest "Client cannot create milestones" POST `
        "/api/contracts/$contractId/milestones" -Token $client.Token -Body (@{
            Title = "Unauthorized expense"
            OrderNumber = 90
            Amount = 50
            Type = 1
        } | ConvertTo-Json)
    Assert-Status "Client cannot create milestones" $clientCreate @(403)

    $invalidType = Invoke-TestRequest "Unknown milestone type is rejected" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Unknown type"
            OrderNumber = 91
            Amount = 50
            Type = 99
        } | ConvertTo-Json)
    Assert-Status "Unknown milestone type is rejected" $invalidType @(400)

    $invalidExpense = Invoke-TestRequest `
        "Expense rejects Standard-only duration and deliverables" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Invalid expense"
            Description = "Must not accept Standard fields."
            Deliverables = @("Receipt")
            OrderNumber = 92
            Amount = 200
            DurationDays = 5
            Type = 1
        } | ConvertTo-Json)
    Assert-Status "Expense rejects Standard-only duration and deliverables" `
        $invalidExpense @(400)

    $standard = Invoke-TestRequest "Create Draft Standard milestone" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Standard written deliverable"
            Description = "Prepare and submit the written legal deliverable."
            Deliverables = @("Written legal memorandum")
            OrderNumber = 1
            Amount = 1100
            DurationDays = 10
            Type = 0
        } | ConvertTo-Json)
    Require-Api "Create Draft Standard milestone" $standard @(201)
    $standardId = [string]$standard.Json.data.id

    $draftExpense = Invoke-TestRequest "Create Draft Expense milestone" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Draft filing fee expense"
            Description = "Court filing fee reimbursement."
            OrderNumber = 2
            Amount = 300
            Type = 1
        } | ConvertTo-Json)
    Require-Api "Create Draft Expense milestone" $draftExpense @(201)
    $draftExpenseId = [string]$draftExpense.Json.data.id
    Assert-Test "Draft Expense response exposes Expense type" `
        (Test-EnumValue $draftExpense.Json.data.type 1 "Expense")
    Assert-Test "Draft Expense response omits Deliverables" `
        ($draftExpense.Content -notmatch '"deliverables"')
    Assert-Test "Draft Expense response omits DurationDays" `
        ($draftExpense.Content -notmatch '"durationDays"')

    $missingEtag = Invoke-TestRequest "Expense update requires If-Match" PUT `
        "/api/contracts/$contractId/milestones/$draftExpenseId" `
        -Token $lawyer.Token -Body (@{
            Title = "Updated filing fee"
            Description = "Updated receipt amount."
            Type = 1
        } | ConvertTo-Json)
    Assert-Status "Expense update requires If-Match" $missingEtag @(412)

    $draftExpenseCurrent = Get-Milestone "Read Draft Expense for update" `
        $contractId $draftExpenseId $lawyer.Token
    $updateExpense = Invoke-TestRequest "Update Draft Expense" PUT `
        "/api/contracts/$contractId/milestones/$draftExpenseId" `
        -Token $lawyer.Token -Body (@{
            Title = "Updated filing fee expense"
            Description = "Updated court filing fee receipt."
            Type = 1
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = [string]$draftExpenseCurrent.version }
    Require-Api "Update Draft Expense" $updateExpense
    Assert-Test "Updated Expense still omits Standard-only fields" `
        ($updateExpense.Content -notmatch '"deliverables"' -and
         $updateExpense.Content -notmatch '"durationDays"')

    Approve-Milestone "Client explicitly approves Draft Expense" `
        $contractId $draftExpenseId $client.Token
    Approve-Milestone "Client approves Standard milestone" `
        $contractId $standardId $client.Token
    Approve-Milestone "Lawyer approves Standard milestone" `
        $contractId $standardId $lawyer.Token

    $detail = Get-ContractDetail "Read contract for client acceptance" `
        $contractId $client.Token
    $clientAccept = Invoke-ContractIfMatch "Client accepts contract" POST `
        "/api/contracts/$contractId/accept" $client.Token $detail.version
    Require-Api "Client accepts contract" $clientAccept
    $detail = Get-ContractDetail "Read contract for lawyer acceptance" `
        $contractId $lawyer.Token
    $lawyerAccept = Invoke-ContractIfMatch "Lawyer accepts contract" POST `
        "/api/contracts/$contractId/accept" $lawyer.Token $detail.version
    Require-Api "Lawyer accepts contract" $lawyerAccept

    Write-Section "Mid-contract Expense approval and forbidden work stages"
    $midExpense = Invoke-TestRequest "Lawyer proposes mid-contract Expense" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Mid-contract courier expense"
            Description = "Urgent legal document courier reimbursement."
            OrderNumber = 3
            Amount = 450
            Type = 1
        } | ConvertTo-Json)
    Require-Api "Lawyer proposes mid-contract Expense" $midExpense @(201)
    $expenseId = [string]$midExpense.Json.data.id

    $preapprovalFund = Invoke-TestRequest `
        "Client cannot fund Expense before explicit approval" POST `
        "/api/milestones/$expenseId/fund" -Token $client.Token -Body (@{
            PaymentMethodReference = $paymentMethodReference
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Status "Client cannot fund Expense before explicit approval" `
        $preapprovalFund @(400, 409)

    $lawyerFund = Invoke-TestRequest "Lawyer cannot fund Expense" POST `
        "/api/milestones/$expenseId/fund" -Token $lawyer.Token -Body (@{
            PaymentMethodReference = $paymentMethodReference
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Status "Lawyer cannot fund Expense" $lawyerFund @(403)

    $expenseSubmit = Invoke-TestRequest "Expense cannot enter Submission" POST `
        "/api/milestones/$expenseId/submit" -Token $lawyer.Token -Body (@{
            Notes = "This stage must be unavailable."
            StoredFileIds = @()
        } | ConvertTo-Json)
    Assert-Status "Expense cannot enter Submission" $expenseSubmit @(400, 409)
    $expenseAccept = Invoke-TestRequest "Expense cannot enter Acceptance" POST `
        "/api/milestones/$expenseId/accept" -Token $client.Token -Body "{}"
    Assert-Status "Expense cannot enter Acceptance" $expenseAccept @(400, 409)
    $expenseChanges = Invoke-TestRequest `
        "Expense cannot enter request-changes stage" POST `
        "/api/milestones/$expenseId/request-changes" `
        -Token $client.Token -Body (@{ Reason = "Not applicable" } | ConvertTo-Json)
    Assert-Status "Expense cannot enter request-changes stage" `
        $expenseChanges @(400, 409)
    Approve-Milestone "Client explicitly approves mid-contract Expense" `
        $contractId $expenseId $client.Token
    $approvedExpense = Get-Milestone "Read approved mid-contract Expense" `
        $contractId $expenseId $client.Token
    Assert-Test "Approved Expense is immediately awaiting funding" `
        (Test-EnumValue $approvedExpense.status 1 "AwaitingFunding")

    $rejectCandidate = Invoke-TestRequest "Create Expense rejection candidate" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Expense to reject"
            Description = "Client rejection workflow."
            OrderNumber = 4
            Amount = 80
            Type = 1
        } | ConvertTo-Json)
    Require-Api "Create Expense rejection candidate" $rejectCandidate @(201)
    $reject = Invoke-TestRequest "Client rejects proposed Expense" POST `
        "/api/milestones/$($rejectCandidate.Json.data.id)/reject" `
        -Token $client.Token -Body (@{ Reason = "Receipt is insufficient." } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = [string]$rejectCandidate.Json.data.version }
    Require-Api "Client rejects proposed Expense" $reject

    $cancelCandidate = Invoke-TestRequest "Create Expense cancellation candidate" POST `
        "/api/contracts/$contractId/milestones" -Token $lawyer.Token -Body (@{
            Title = "Expense to cancel"
            Description = "Lawyer cancellation workflow."
            OrderNumber = 5
            Amount = 90
            Type = 1
        } | ConvertTo-Json)
    Require-Api "Create Expense cancellation candidate" $cancelCandidate @(201)
    $cancel = Invoke-TestRequest "Lawyer cancels proposed Expense" POST `
        "/api/milestones/$($cancelCandidate.Json.data.id)/cancel" `
        -Token $lawyer.Token -Body (@{ Reason = "Charge was reversed." } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = [string]$cancelCandidate.Json.data.version }
    Require-Api "Lawyer cancels proposed Expense" $cancel

    Write-Section "Real Stripe Expense funding and instant release"
    $walletBefore = Invoke-TestRequest "Read wallet before Expense funding" GET `
        "/api/wallet" -Token $lawyer.Token
    Require-Api "Read wallet before Expense funding" $walletBefore
    $expenseFundKey = [guid]::NewGuid().ToString()
    $fundExpense = Invoke-TestRequest "Fund Expense with real Stripe test card" POST `
        "/api/milestones/$expenseId/fund" -Token $client.Token -Body (@{
            PaymentMethodReference = $paymentMethodReference
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = $expenseFundKey }
    Require-Api "Fund Expense with real Stripe test card" $fundExpense @(200, 202)
    Assert-Test "Real Expense funding creates a payment transaction" `
        (-not [string]::IsNullOrWhiteSpace(
            [string]$fundExpense.Json.data.paymentTransactionId))

    $fundExpenseRepeat = Invoke-TestRequest `
        "Repeat Expense funding idempotently" POST `
        "/api/milestones/$expenseId/fund" -Token $client.Token -Body (@{
            PaymentMethodReference = $paymentMethodReference
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = $expenseFundKey }
    Require-Api "Repeat Expense funding idempotently" $fundExpenseRepeat @(200, 202)
    Assert-Test "Repeated funding returns the same transaction" `
        ([string]$fundExpenseRepeat.Json.data.paymentTransactionId -eq
         [string]$fundExpense.Json.data.paymentTransactionId)

    $releasedExpense = Wait-ForExpenseRelease $contractId $expenseId `
        $client.Token
    Assert-Test "Expense is released without Submission or Acceptance" `
        (Test-EnumValue $releasedExpense.status 7 "Released")
    Assert-Test "Expense never receives SubmittedAt" `
        ($null -eq $releasedExpense.submittedAt)
    Assert-Test "Expense never receives a 14-day HoldExpiresAt" `
        ($null -eq $releasedExpense.holdExpiresAt)

    $expensePayment = Invoke-TestRequest "Read released Expense payment" GET `
        "/api/milestones/$expenseId/payment" -Token $client.Token
    Require-Api "Read released Expense payment" $expensePayment
    Assert-Test "Expense escrow hold is Released" `
        (Test-EnumValue $expensePayment.Json.data.status 2 "Released")
    $walletAfterExpense = Invoke-TestRequest "Read wallet after Expense release" GET `
        "/api/wallet" -Token $lawyer.Token
    Require-Api "Read wallet after Expense release" $walletAfterExpense
    Assert-Test "Expense release increases lawyer available balance" `
        ([decimal]$walletAfterExpense.Json.data.availableBalance -gt
         [decimal]$walletBefore.Json.data.availableBalance)

    Write-Section "Unchanged Standard funding, Submission, Acceptance, and hold"
    $standardCurrent = Get-Milestone "Read Standard before ready-for-funding" `
        $contractId $standardId $lawyer.Token
    $ready = Invoke-TestRequest "Lawyer marks Standard ready for funding" POST `
        "/api/milestones/$standardId/ready-for-funding" `
        -Token $lawyer.Token -Body "{}" `
        -ExtraHeaders @{ "If-Match" = [string]$standardCurrent.version }
    Require-Api "Lawyer marks Standard ready for funding" $ready

    $fundStandard = Invoke-TestRequest "Fund Standard with real Stripe test card" POST `
        "/api/milestones/$standardId/fund" -Token $client.Token -Body (@{
            PaymentMethodReference = $paymentMethodReference
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Require-Api "Fund Standard with real Stripe test card" $fundStandard @(200, 202)
    $fundedStandard = Get-Milestone "Read funded Standard milestone" `
        $contractId $standardId $client.Token
    Assert-Test "Standard remains FundedInProgress after funding" `
        (Test-EnumValue $fundedStandard.status 3 "FundedInProgress")
    Assert-Test "Standard is not instantly released" `
        (-not (Test-EnumValue $fundedStandard.status 7 "Released"))

    $fileId = Add-LawyerFileFixture $lawyer.Id
    $submit = Invoke-TestRequest "Lawyer submits Standard deliverable" POST `
        "/api/milestones/$standardId/submit" -Token $lawyer.Token -Body (@{
            Notes = "Completed written legal memorandum."
            StoredFileIds = @($fileId)
        } | ConvertTo-Json)
    Require-Api "Lawyer submits Standard deliverable" $submit
    Assert-Test "Standard enters Submitted" `
        (Test-EnumValue $submit.Json.data.status 4 "Submitted")

    $accept = Invoke-TestRequest "Client accepts Standard deliverable" POST `
        "/api/milestones/$standardId/accept" -Token $client.Token -Body "{}"
    Require-Api "Client accepts Standard deliverable" $accept
    Assert-Test "Standard enters AcceptedHold" `
        (Test-EnumValue $accept.Json.data.status 5 "AcceptedHold")
    $holdExpiry = [DateTime]$accept.Json.data.holdExpiresAt
    $daysUntilRelease = ($holdExpiry.ToUniversalTime() -
        [DateTime]::UtcNow).TotalDays
    Assert-Test "Standard retains approximately 14-day hold" `
        ($daysUntilRelease -gt 13.9 -and $daysUntilRelease -le 14.01) `
        "(days=$([math]::Round($daysUntilRelease, 3)))"

    $standardPayment = Invoke-TestRequest "Read Standard escrow hold" GET `
        "/api/milestones/$standardId/payment" -Token $lawyer.Token
    Require-Api "Read Standard escrow hold" $standardPayment
    Assert-Test "Standard payment remains held, not released" `
        (-not (Test-EnumValue $standardPayment.Json.data.status 2 "Released"))
    Assert-Test "Standard payment exposes the same hold expiry" `
        ($null -ne $standardPayment.Json.data.holdExpiresAt)

    $history = Invoke-TestRequest "Read complete contract payment history" GET `
        "/api/contracts/$contractId/payments" -Token $client.Token
    Require-Api "Read complete contract payment history" $history
    $stripeAttempts = @($history.Json.data.attempts) |
        Where-Object { $_.providerName -like "*Stripe*" }
    Assert-Test "Payment history records Stripe provider attempts" `
        ($stripeAttempts.Count -ge 3)
    Assert-Test "Both Standard and Expense escrow holds are recorded" `
        (@($history.Json.data.payments).Count -ge 2)

    $remove = Invoke-TestRequest "Remove Stripe test payment method" DELETE `
        "/api/payment-methods/$paymentMethodReference" -Token $client.Token
    Require-Api "Remove Stripe test payment method" $remove
    $methodsAfter = Invoke-TestRequest "List payment methods after removal" GET `
        "/api/payment-methods" -Token $client.Token
    Require-Api "List payment methods after removal" $methodsAfter
    Assert-Test "Removed Stripe payment method is no longer listed" `
        ($null -eq (@($methodsAfter.Json.data) |
            Where-Object paymentMethodReference -eq $paymentMethodReference |
            Select-Object -First 1))
}
catch {
    $script:failed++
    $script:failureMessages.Add("Unhandled test interruption: $($_.Exception.Message)")
    Write-Host "TEST INTERRUPTED: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Section "Summary"
    "- Passed assertions: $script:passed" |
        Out-File $ReportFile -Append -Encoding utf8
    "- Failed assertions: $script:failed" |
        Out-File $ReportFile -Append -Encoding utf8
    if ($script:failureMessages.Count -gt 0) {
        "`n### Failures`n" | Out-File $ReportFile -Append -Encoding utf8
        foreach ($failure in $script:failureMessages) {
            "- $(Protect-ReportText $failure)" |
                Out-File $ReportFile -Append -Encoding utf8
        }
    }
    Write-Host "`nHTTP assertions: $script:passed passed, $script:failed failed"
    Write-Host "Report: $ReportFile"
}

if ($script:failed -gt 0) { exit 1 }
