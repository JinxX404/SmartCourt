param(
    [string]$BaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [string]$SqlServer = ".",
    [string]$SqlDatabase = "SmartCourt_dev"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ApiLogPath) {
    $ApiLogPath = Join-Path $scriptDir "..\..\SmartCourt\api_log.txt"
}
if (-not $ReportFile) {
    $ReportFile = Join-Path $scriptDir "PaymentsNotifications_Report.md"
}

. "$scriptDir\ContractsNotifications_Test.ps1" `
    -BaseUrl $BaseUrl -ApiLogPath $ApiLogPath -ReportFile $ReportFile `
    -SqlServer $SqlServer -SqlDatabase $SqlDatabase -FunctionsOnly

$script:passed = 0
$script:failed = 0
$script:failureMessages = [System.Collections.Generic.List[string]]::new()
"# Payments Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
    Out-File $ReportFile -Encoding utf8

function Find-PaymentNotification {
    param(
        [string]$Title,
        [string]$Token,
        [string]$Type,
        [hashtable]$ExpectedData,
        [int]$TimeoutSeconds = 75
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $response = Invoke-TestRequest $Title GET `
            "/api/notifications?pageSize=50" -Token $Token
        if ($response.Status -eq 200 -and $response.Json.data.items) {
            $match = @($response.Json.data.items) | Where-Object {
                if ($_.type -ne $Type) { return $false }
                foreach ($key in $ExpectedData.Keys) {
                    $actualProperty = $_.data.PSObject.Properties[$key]
                    if ($null -eq $actualProperty -or
                        [string]$actualProperty.Value -ne
                        [string]$ExpectedData[$key]) {
                        return $false
                    }
                }
                return $true
            } | Select-Object -First 1
            if ($match) { return $match }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Assert-PaymentNotification {
    param(
        [string]$Name,
        $Notification,
        [string]$Type,
        [string]$Severity,
        [string]$Title,
        [string]$Body,
        [hashtable]$ExpectedData
    )

    $valid = $null -ne $Notification -and
        $Notification.type -eq $Type -and
        $Notification.severity -eq $Severity -and
        $Notification.title -eq $Title -and
        $Notification.body -eq $Body -and
        $null -eq $Notification.actionUrl
    if ($valid) {
        foreach ($key in $ExpectedData.Keys) {
            $actualProperty = $Notification.data.PSObject.Properties[$key]
            if ($null -eq $actualProperty -or
                [string]$actualProperty.Value -ne
                [string]$ExpectedData[$key]) {
                $valid = $false
                break
            }
        }
    }
    if ($valid) {
        $forbiddenKeys = @(
            "amount", "currency", "reason", "failureReason",
            "destinationReference", "paymentMethodReference",
            "providerTransactionId", "idempotencyKey")
        foreach ($key in $forbiddenKeys) {
            if ($Notification.data.PSObject.Properties.Name -contains $key) {
                $valid = $false
                break
            }
        }
    }
    Assert-Test $Name $valid
}

function New-ReadyPaymentFoundation {
    param(
        [string]$Label,
        [string]$ClientToken,
        [string]$LawyerToken,
        [string]$LawyerId
    )

    $foundation = New-CaseProposal $Label $ClientToken $LawyerToken $LawyerId
    $contract = New-Contract $Label $foundation.ProposalId $LawyerToken
    $milestoneId = New-ApprovedMilestone $Label $contract.Id `
        $ClientToken $LawyerToken
    Accept-ContractBoth $Label $contract.Id $ClientToken $LawyerToken
    $list = Invoke-TestRequest "$Label - list milestone before ready" GET `
        "/api/contracts/$($contract.Id)/milestones" -Token $LawyerToken
    Require-Api "$Label - list milestone before ready" $list
    $version = [string](@($list.Json.data) |
        Where-Object id -eq $milestoneId).version
    $ready = Invoke-TestRequest "$Label - mark ready for funding" POST `
        "/api/milestones/$milestoneId/ready-for-funding" `
        -Token $LawyerToken -Body "{}" `
        -ExtraHeaders @{ "If-Match" = $version }
    Require-Api "$Label - mark ready for funding" $ready
    return [pscustomobject]@{
        CaseId = [string]$foundation.CaseId
        ProposalId = [string]$foundation.ProposalId
        ContractId = [string]$contract.Id
        MilestoneId = [string]$milestoneId
    }
}

function Get-PaymentData {
    param(
        $Foundation,
        [string]$Token,
        [string]$Label
    )

    $history = Invoke-TestRequest "$Label - get contract payments" GET `
        "/api/contracts/$($Foundation.ContractId)/payments" -Token $Token
    Require-Api "$Label - get contract payments" $history
    return $history.Json.data
}

function Promote-SuperAdministrator {
    param(
        [string]$UserId,
        [string]$Email,
        [string]$Password
    )

    $newRoleId = [guid]::NewGuid()
    $sql = @"
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @RoleId uniqueidentifier;
SELECT @RoleId = Id FROM AspNetRoles
WHERE NormalizedName = 'SUPERADMINISTRATOR';
IF @RoleId IS NULL
BEGIN
    SET @RoleId = '$newRoleId';
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@RoleId, 'SuperAdministrator', 'SUPERADMINISTRATOR', CONVERT(nvarchar(36), NEWID()));
END;
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles
    WHERE UserId = '$UserId' AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES ('$UserId', @RoleId);
END;
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $sql | Out-Null
    Assert-Test "Disposable SuperAdministrator fixture created" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) {
        throw "SuperAdministrator role fixture creation failed."
    }
    return Login-User "Refresh disposable SuperAdministrator token" `
        $Email $Password
}

function Add-LawyerFileFixture {
    param([string]$LawyerId, [string]$Label)

    $fileId = [guid]::NewGuid()
    $fileName = "payments-$Label-$($fileId.ToString('N')).pdf"
    $sql = @"
SET NOCOUNT ON;
INSERT INTO StoredFiles
    (Id, StoredFileName, OriginalFileName, FileUrl, ContentType, Extension, SizeInBytes, IsDeleted)
VALUES
    ('$fileId', '$fileName', '$fileName', 'https://mock.local/$fileName', 'application/pdf', '.pdf', 1024, 0);
INSERT INTO UserVerificationDocuments
    (Id, UserId, StoredFileId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate)
VALUES
    (NEWID(), '$LawyerId', '$fileId', 0, 1, 1, 0, DATEADD(year, 1, GETUTCDATE()));
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $sql | Out-Null
    Assert-Test "$Label lawyer-owned stored-file fixture created" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) { throw "Stored-file fixture failed." }
    return [string]$fileId
}

function New-WebhookHeaders {
    param(
        [string]$Body,
        [string]$EventId,
        [long]$Timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    )

    $timestampText = $Timestamp.ToString(
        [System.Globalization.CultureInfo]::InvariantCulture)
    $key = [Text.Encoding]::UTF8.GetBytes(
        "local-mock-payment-webhook-secret")
    $signed = [Text.Encoding]::UTF8.GetBytes("$timestampText.$Body")
    $hmac = [Security.Cryptography.HMACSHA256]::new($key)
    try {
        $signature = [Convert]::ToBase64String($hmac.ComputeHash($signed))
    }
    finally {
        $hmac.Dispose()
    }
    return @{
        "X-Payment-Event-Id" = $EventId
        "X-Payment-Timestamp" = $timestampText
        "X-Payment-Signature" = "v1=$signature"
    }
}

function Get-WithdrawalId {
    param(
        [string]$LawyerId,
        [int]$Status
    )

    $lines = & sqlcmd -S $SqlServer -d $SqlDatabase -b -W -h -1 -Q `
        "SET NOCOUNT ON; SELECT TOP (1) Id FROM WithdrawalRequests WHERE LawyerUserId = '$LawyerId' AND Status = $Status ORDER BY RequestedAt DESC;"
    $candidate = @($lines | Where-Object {
        -not [string]::IsNullOrWhiteSpace($_) -and $_.Trim() -ne "NULL"
    } | Select-Object -First 1)
    if ($candidate.Count -eq 0) { return "" }
    return $candidate[0].Trim()
}

function Accelerate-WithdrawalReconciliation {
    param([string]$WithdrawalId)

    $sql = @"
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY
    UPDATE WithdrawalRequests
    SET RequestedAt = DATEADD(day, -2, GETUTCDATE())
    WHERE Id = '$WithdrawalId' AND Status = 0 AND RequiresManualAction = 0;
    IF @@ROWCOUNT <> 1
        THROW 51000, 'Expected one disposable processing withdrawal.', 1;
    UPDATE [HangFire].[Set]
    SET Score = 0
    WHERE [Key] = N'recurring-jobs'
      AND [Value] = N'contract-payment-wallet-reconciliation';
    IF @@ROWCOUNT <> 1
        THROW 51001, 'Expected the wallet reconciliation recurring job.', 1;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $sql | Out-Null
    Assert-Test "Delayed withdrawal reconciliation accelerated" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) {
        throw "Could not accelerate withdrawal reconciliation."
    }
}

try {
    Write-Section "Health and complete Payments authorization boundary"
    $health = Invoke-TestRequest "API health" GET "/health"
    Assert-Test "API is healthy" ($health.Status -eq 200) `
        "(status=$($health.Status))"
    $unknownId = [guid]::NewGuid().ToString()
    foreach ($test in @(
        @{ Name = "Fund requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/fund"; Body = "{}" },
        @{ Name = "Contract payments require authentication"; Method = "GET"; Path = "/api/contracts/$unknownId/payments"; Body = "" },
        @{ Name = "Milestone payment requires authentication"; Method = "GET"; Path = "/api/milestones/$unknownId/payment"; Body = "" },
        @{ Name = "Retry requires authentication"; Method = "POST"; Path = "/api/payments/$unknownId/retry"; Body = "{}" },
        @{ Name = "Wallet requires authentication"; Method = "GET"; Path = "/api/wallet"; Body = "" },
        @{ Name = "Withdrawal requires authentication"; Method = "POST"; Path = "/api/wallet/withdrawals"; Body = "{}" },
        @{ Name = "Admin release requires authentication"; Method = "POST"; Path = "/api/admin/milestones/$unknownId/release"; Body = "{}" },
        @{ Name = "Admin adjustment requires authentication"; Method = "POST"; Path = "/api/admin/wallets/$unknownId/adjustments"; Body = "{}" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path `
            -Body $test.Body
        Assert-Test $test.Name ($response.Status -eq 401) `
            "(status=$($response.Status))"
    }
    $emptyWebhook = Invoke-TestRequest "Anonymous empty webhook rejected" POST `
        "/api/payments/webhook" -Body "{}"
    Assert-Test "Empty webhook returns 400" ($emptyWebhook.Status -eq 400) `
        "(status=$($emptyWebhook.Status))"

    Write-Section "Zero-assumption accounts with mock Email confirmation"
    $stamp = Get-Date -Format "yyyyMMddHHmmssfff"
    $password = "Password123!"
    $attackerEmail = "payments_attacker_$stamp@example.com"
    $adminLogin = Login-User "Login admin" "admin@smartcourt.com" "Admin@123"
    $adminToken = [string]$adminLogin.accessToken
    $client = Register-And-PrepareUser "client" `
        "payments_client_$stamp@example.com" $password $adminToken 20
    $lawyer = Register-And-PrepareUser "lawyer" `
        "payments_lawyer_$stamp@example.com" $password $adminToken 21
    $attacker = Register-And-PrepareUser "attacker" `
        $attackerEmail $password $adminToken 22

    Write-Section "Funding validation, hostile input, and role boundaries"
    $validation = New-ReadyPaymentFoundation "payment-validation" `
        $client.Token $lawyer.Token $lawyer.Id
    $emptyFund = Invoke-TestRequest "Fund empty body" POST `
        "/api/milestones/$($validation.MilestoneId)/fund" `
        -Token $client.Token -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Empty funding body returns 400" ($emptyFund.Status -eq 400) `
        "(status=$($emptyFund.Status))"
    $missingFundKey = Invoke-TestRequest "Fund missing idempotency key" POST `
        "/api/milestones/$($validation.MilestoneId)/fund" `
        -Token $client.Token -Body (
        @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json)
    Assert-Test "Missing funding idempotency key returns 400" `
        ($missingFundKey.Status -eq 400) "(status=$($missingFundKey.Status))"
    $longPaymentReference = "<script>☠' OR 1=1;--</script>" + ("م" * 220)
    $hostileFund = Invoke-TestRequest "Fund hostile extreme reference" POST `
        "/api/milestones/$($validation.MilestoneId)/fund" `
        -Token $client.Token -Body (
        @{ PaymentMethodReference = $longPaymentReference } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Hostile extreme funding reference returns 400" `
        ($hostileFund.Status -eq 400) "(status=$($hostileFund.Status))"
    $typeMismatchFund = Invoke-TestRequest "Fund type mismatch" POST `
        "/api/milestones/$($validation.MilestoneId)/fund" `
        -Token $client.Token -Body '{"paymentMethodReference":12345}' `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Funding type mismatch returns 400" `
        ($typeMismatchFund.Status -eq 400) "(status=$($typeMismatchFund.Status))"
    $lawyerFund = Invoke-TestRequest "Lawyer cannot fund" POST `
        "/api/milestones/$($validation.MilestoneId)/fund" `
        -Token $lawyer.Token -Body (
        @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Lawyer funding is forbidden" ($lawyerFund.Status -eq 403) `
        "(status=$($lawyerFund.Status))"
    $clientWallet = Invoke-TestRequest "Client cannot get lawyer wallet" GET `
        "/api/wallet" -Token $client.Token
    Assert-Test "Client wallet access is forbidden" `
        ($clientWallet.Status -eq 403) "(status=$($clientWallet.Status))"
    $prePromotionRetry = Invoke-TestRequest `
        "Ordinary user cannot retry payment" POST `
        "/api/payments/$unknownId/retry" -Token $attacker.Token -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Ordinary user retry is forbidden" `
        ($prePromotionRetry.Status -eq 403) `
        "(status=$($prePromotionRetry.Status))"
    $prePromotionRelease = Invoke-TestRequest `
        "Ordinary user cannot force release" POST `
        "/api/admin/milestones/$unknownId/release" `
        -Token $attacker.Token -Body "{}"
    Assert-Test "Ordinary user release is forbidden" `
        ($prePromotionRelease.Status -eq 403) `
        "(status=$($prePromotionRelease.Status))"
    $prePromotionAdjustment = Invoke-TestRequest `
        "Ordinary user cannot adjust wallet" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $attacker.Token -Body "{}"
    Assert-Test "Ordinary user adjustment is forbidden" `
        ($prePromotionAdjustment.Status -eq 403) `
        "(status=$($prePromotionAdjustment.Status))"

    Write-Section "Direct funding success, idempotency, queries, and Arabic notifications"
    $success = New-ReadyPaymentFoundation "payment-success" `
        $client.Token $lawyer.Token $lawyer.Id
    $fundKey = "payment-success-$([guid]::NewGuid())"
    $fundBody = @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json
    $fund = Invoke-TestRequest "Fund milestone successfully" POST `
        "/api/milestones/$($success.MilestoneId)/fund" `
        -Token $client.Token -Body $fundBody `
        -ExtraHeaders @{ "Idempotency-Key" = $fundKey }
    Require-Api "Fund milestone successfully" $fund
    $fundReplay = Invoke-TestRequest "Replay funding idempotency key" POST `
        "/api/milestones/$($success.MilestoneId)/fund" `
        -Token $client.Token -Body $fundBody `
        -ExtraHeaders @{ "Idempotency-Key" = $fundKey }
    Require-Api "Replay funding idempotency key" $fundReplay
    Assert-Test "Funding replay returns same hold" `
        ($fund.Json.data.id -eq $fundReplay.Json.data.id)
    $successData = @{
        milestoneId = $success.MilestoneId
        contractId = $success.ContractId
        proposalId = $success.ProposalId
        legalCaseId = $success.CaseId
    }
    $fundingStarted = Find-PaymentNotification `
        "Poll lawyer for funding started" $lawyer.Token `
        "milestone.funding-started" $successData
    Assert-PaymentNotification "Lawyer receives exact funding-started notification" `
        $fundingStarted "milestone.funding-started" "Information" `
        "بدأ تمويل المرحلة" `
        "بدأت معالجة تمويل المرحلة. انتظر تأكيد اكتمال التمويل قبل بدء العمل." `
        $successData
    $fundedClient = Find-PaymentNotification "Poll client for funded" `
        $client.Token "milestone.funded" $successData
    $fundedLawyer = Find-PaymentNotification "Poll lawyer for funded" `
        $lawyer.Token "milestone.funded" $successData
    Assert-PaymentNotification "Client receives exact funded notification" `
        $fundedClient "milestone.funded" "Success" "تم تمويل المرحلة" `
        "اكتمل تمويل المرحلة وحُفظ المبلغ في حساب الضمان." $successData
    Assert-PaymentNotification "Lawyer receives exact funded notification" `
        $fundedLawyer "milestone.funded" "Success" "تم تمويل المرحلة" `
        "اكتمل تمويل المرحلة، ويمكنك الآن بدء العمل عليها." $successData

    $successHistory = Get-PaymentData $success $client.Token "success"
    Assert-Test "Contract payment history contains funded hold" `
        (@($successHistory.payments).Count -eq 1 -and
        @($successHistory.attempts).Count -ge 1)
    $paymentDetail = Invoke-TestRequest "Get milestone payment" GET `
        "/api/milestones/$($success.MilestoneId)/payment" `
        -Token $lawyer.Token -ExtraHeaders @{ Accept = "text/plain" }
    Require-Api "Get milestone payment" $paymentDetail
    Assert-Test "Invalid Accept header does not corrupt JSON contract" `
        ($paymentDetail.Json.data.milestoneId -eq $success.MilestoneId)
    $participantHistory = Invoke-TestRequest "Participant queries payments" GET `
        "/api/contracts/$($success.ContractId)/payments" -Token $client.Token
    Assert-Test "Contract participant can query payment history" `
        ($participantHistory.Status -eq 200)
    $unrelatedHistory = Invoke-TestRequest "Unrelated user cannot query payments" GET `
        "/api/contracts/$($success.ContractId)/payments" -Token $attacker.Token
    Assert-Test "Unrelated user payment history is forbidden" `
        ($unrelatedHistory.Status -eq 403) `
        "(status=$($unrelatedHistory.Status))"
    $missingPayment = Invoke-TestRequest "Unknown milestone payment rejected" GET `
        "/api/milestones/$unknownId/payment" -Token $client.Token
    Assert-Test "Unknown milestone payment returns 400 or 404" `
        ($missingPayment.Status -in @(400, 404)) `
        "(status=$($missingPayment.Status))"

    Write-Section "Confirmed funding failure and explicit admin retry"
    $super = Promote-SuperAdministrator $attacker.Id $attackerEmail $password
    $superToken = [string]$super.accessToken
    $retryFoundation = New-ReadyPaymentFoundation "payment-retry" `
        $client.Token $lawyer.Token $lawyer.Id
    $retryKey = "payment-retry-failure-$([guid]::NewGuid())"
    $failedFund = Invoke-TestRequest "Mock funding confirmed failure" POST `
        "/api/milestones/$($retryFoundation.MilestoneId)/fund" `
        -Token $client.Token -Body (
        @{ PaymentMethodReference = "mock-fail" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = $retryKey }
    Assert-Test "Confirmed funding failure returns 400" `
        ($failedFund.Status -eq 400) "(status=$($failedFund.Status))"
    $retryData = @{
        milestoneId = $retryFoundation.MilestoneId
        contractId = $retryFoundation.ContractId
        proposalId = $retryFoundation.ProposalId
        legalCaseId = $retryFoundation.CaseId
    }
    $failedNotification = Find-PaymentNotification `
        "Poll client for failed funding" $client.Token `
        "milestone.funding-failed" $retryData
    Assert-PaymentNotification "Client receives exact funding-failed notification" `
        $failedNotification "milestone.funding-failed" "Critical" `
        "فشل تمويل المرحلة" `
        "لم تكتمل عملية تمويل المرحلة. يمكنك مراجعة وسيلة الدفع والمحاولة مرة أخرى." `
        $retryData
    $retryHistory = Get-PaymentData $retryFoundation $client.Token `
        "failed funding"
    $failedAttempt = @($retryHistory.attempts | Where-Object {
        $_.status -eq "Failed" -or [int]$_.status -eq 2
    } | Select-Object -First 1)
    Assert-Test "Failed payment attempt is queryable" ($failedAttempt.Count -eq 1)
    $failedTransactionId = [string]$failedAttempt[0].id
    $missingRetryKey = Invoke-TestRequest "Retry missing idempotency key" POST `
        "/api/payments/$failedTransactionId/retry" `
        -Token $superToken -Body "{}"
    Assert-Test "Missing retry key returns 400" `
        ($missingRetryKey.Status -eq 400) `
        "(status=$($missingRetryKey.Status))"
    $clientRetry = Invoke-TestRequest "Client cannot retry payment" POST `
        "/api/payments/$failedTransactionId/retry" `
        -Token $client.Token -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Client retry is forbidden" ($clientRetry.Status -eq 403) `
        "(status=$($clientRetry.Status))"
    $unknownRetry = Invoke-TestRequest "Retry unknown transaction" POST `
        "/api/payments/$unknownId/retry" -Token $superToken -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Unknown retry returns 400 or 404" `
        ($unknownRetry.Status -in @(400, 404)) `
        "(status=$($unknownRetry.Status))"
    $retry = Invoke-TestRequest "Admin retries failed payment" POST `
        "/api/payments/$failedTransactionId/retry" `
        -Token $superToken -Body "{}" `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Confirmed-failure retry returns safe 400" `
        ($retry.Status -eq 400) "(status=$($retry.Status))"
    $afterRetryHistory = Get-PaymentData $retryFoundation $client.Token `
        "after failed retry"
    $processingAttemptsAfterRetry = @($afterRetryHistory.attempts | Where-Object {
        $_.status -eq "Processing" -or [int]$_.status -eq 0
    })
    Assert-Test "Unconfirmed mock retry remains safely processing" `
        ($processingAttemptsAfterRetry.Count -eq 1) `
        "(processingAttempts=$($processingAttemptsAfterRetry.Count))"

    Write-Section "Signed webhook validation, completion, and replay"
    $webhookFoundation = New-ReadyPaymentFoundation "payment-webhook" `
        $client.Token $lawyer.Token $lawyer.Id
    $webhookFund = Invoke-TestRequest "Create processing funding attempt" POST `
        "/api/milestones/$($webhookFoundation.MilestoneId)/fund" `
        -Token $client.Token -Body (
        @{ PaymentMethodReference = "mock-timeout" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = "webhook-$([guid]::NewGuid())" }
    Assert-Test "Unknown provider result remains processing" `
        ($webhookFund.Status -eq 400) "(status=$($webhookFund.Status))"
    $webhookHistory = Get-PaymentData $webhookFoundation $client.Token `
        "webhook processing"
    $processingAttempt = @($webhookHistory.attempts | Where-Object {
        $_.status -eq "Processing" -or [int]$_.status -eq 0
    } | Select-Object -First 1)
    Assert-Test "Processing payment attempt is queryable" `
        ($processingAttempt.Count -eq 1)
    $webhookTransactionId = [string]$processingAttempt[0].id
    $webhookEventId = "payment-http-$([guid]::NewGuid())"
    $providerTransactionId = "mock-webhook-$([guid]::NewGuid())"
    $webhookBody = @{
        EventId = $webhookEventId
        PaymentTransactionId = $webhookTransactionId
        ProviderTransactionId = $providerTransactionId
        Status = 1
        Amount = 1000.00
        Currency = "EGP"
        ProcessedAt = [DateTime]::UtcNow.ToString("o")
        FailureReason = $null
    } | ConvertTo-Json -Compress
    $badSignature = Invoke-TestRequest "Webhook invalid signature" POST `
        "/api/payments/webhook" -Body $webhookBody -ExtraHeaders @{
            "X-Payment-Event-Id" = $webhookEventId
            "X-Payment-Timestamp" = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
            "X-Payment-Signature" = "v1=invalid"
        }
    Assert-Test "Invalid webhook signature returns 400" `
        ($badSignature.Status -eq 400) `
        "(status=$($badSignature.Status))"
    $staleEventId = "payment-stale-$([guid]::NewGuid())"
    $staleBody = $webhookBody -replace $webhookEventId, $staleEventId
    $staleHeaders = New-WebhookHeaders $staleBody $staleEventId `
        ([DateTimeOffset]::UtcNow.AddMinutes(-10).ToUnixTimeSeconds())
    $staleWebhook = Invoke-TestRequest "Webhook stale timestamp" POST `
        "/api/payments/webhook" -Body $staleBody -ExtraHeaders $staleHeaders
    Assert-Test "Stale webhook returns 400" ($staleWebhook.Status -eq 400) `
        "(status=$($staleWebhook.Status))"
    $webhookHeaders = New-WebhookHeaders $webhookBody $webhookEventId
    $validWebhook = Invoke-TestRequest "Complete funding by signed webhook" POST `
        "/api/payments/webhook" -Body $webhookBody `
        -ExtraHeaders $webhookHeaders
    Require-Api "Complete funding by signed webhook" $validWebhook
    $duplicateWebhook = Invoke-TestRequest "Replay signed webhook" POST `
        "/api/payments/webhook" -Body $webhookBody `
        -ExtraHeaders $webhookHeaders
    Require-Api "Replay signed webhook" $duplicateWebhook
    Assert-Test "Webhook replay is reported as duplicate" `
        ($duplicateWebhook.Json.data.status -eq "Duplicate")
    $webhookData = @{
        milestoneId = $webhookFoundation.MilestoneId
        contractId = $webhookFoundation.ContractId
        proposalId = $webhookFoundation.ProposalId
        legalCaseId = $webhookFoundation.CaseId
    }
    Assert-Test "Webhook completion notifies client" ($null -ne (
        Find-PaymentNotification "Poll client after webhook" $client.Token `
            "milestone.funded" $webhookData 90))
    Assert-Test "Webhook completion notifies lawyer" ($null -ne (
        Find-PaymentNotification "Poll lawyer after webhook" $lawyer.Token `
            "milestone.funded" $webhookData 90))

    Write-Section "Escrow release and role-specific settlement notifications"
    $fileId = Add-LawyerFileFixture $lawyer.Id "release"
    $submit = Invoke-TestRequest "Lawyer submits funded milestone" POST `
        "/api/milestones/$($success.MilestoneId)/submit" `
        -Token $lawyer.Token -Body (
        @{
            Notes = "اكتملت أعمال المرحلة لاختبار تحرير الأموال."
            StoredFileIds = @($fileId)
        } | ConvertTo-Json)
    Require-Api "Lawyer submits funded milestone" $submit
    $accept = Invoke-TestRequest "Client accepts funded milestone" POST `
        "/api/milestones/$($success.MilestoneId)/accept" `
        -Token $client.Token -Body "{}"
    Require-Api "Client accepts funded milestone" $accept
    $clientRelease = Invoke-TestRequest "Client cannot force escrow release" POST `
        "/api/admin/milestones/$($success.MilestoneId)/release" `
        -Token $client.Token -Body "{}"
    Assert-Test "Client escrow release is forbidden" `
        ($clientRelease.Status -eq 403) "(status=$($clientRelease.Status))"
    $release = Invoke-TestRequest "SuperAdministrator releases escrow" POST `
        "/api/admin/milestones/$($success.MilestoneId)/release" `
        -Token $superToken -Body "{}"
    Require-Api "SuperAdministrator releases escrow" $release
    $releasedClient = Find-PaymentNotification `
        "Poll client for released funds" $client.Token `
        "funds.released" @{ milestoneId = $success.MilestoneId } 90
    $releasedLawyer = Find-PaymentNotification `
        "Poll lawyer for released funds" $lawyer.Token `
        "funds.released" @{ milestoneId = $success.MilestoneId } 90
    $settlementData = @{
        milestoneId = $success.MilestoneId
        contractId = $success.ContractId
        proposalId = $success.ProposalId
        legalCaseId = $success.CaseId
        escrowHoldId = [string]$releasedClient.data.escrowHoldId
        paymentTransactionId = [string]$releasedClient.data.paymentTransactionId
    }
    Assert-PaymentNotification "Client receives exact funds-released confirmation" `
        $releasedClient "funds.released" "Success" `
        "تم تحرير أموال المرحلة" `
        "انتهت مدة الحجز وتم تحرير مستحقات المحامي عن المرحلة." `
        $settlementData
    Assert-PaymentNotification "Lawyer receives exact released receipt" `
        $releasedLawyer "funds.released" "Success" `
        "أصبحت مستحقات المرحلة متاحة" `
        "تم تحويل مستحقات المرحلة إلى رصيد محفظتك المتاح." `
        $settlementData
    $wallet = Invoke-TestRequest "Lawyer gets released wallet" GET `
        "/api/wallet" -Token $lawyer.Token
    Require-Api "Lawyer gets released wallet" $wallet
    Assert-Test "Release creates available lawyer balance" `
        ([decimal]$wallet.Json.data.availableBalance -gt 0)

    Write-Section "Administrative wallet adjustment and notification"
    $zeroAdjustmentBody = @{
        ContractId = $success.ContractId
        PendingBalanceDelta = 0
        AvailableBalanceDelta = 0
        Reason = "سبب تصحيح مالي واضح لكنه لا يحتوي على أي تغيير فعلي."
    } | ConvertTo-Json
    $zeroAdjustment = Invoke-TestRequest "Adjustment with zero deltas" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body $zeroAdjustmentBody `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Zero wallet adjustment returns 400" `
        ($zeroAdjustment.Status -eq 400) `
        "(status=$($zeroAdjustment.Status))"
    $shortReason = Invoke-TestRequest "Adjustment short reason" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body (
        @{
            ContractId = $success.ContractId
            PendingBalanceDelta = 0
            AvailableBalanceDelta = 10
            Reason = "قصير"
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Short adjustment reason returns 400" `
        ($shortReason.Status -eq 400) "(status=$($shortReason.Status))"
    $extremeAdjustment = Invoke-TestRequest "Adjustment extreme delta" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body (
        @{
            ContractId = $success.ContractId
            PendingBalanceDelta = 0
            AvailableBalanceDelta = 1000001
            Reason = "تصحيح متطرف يجب أن يرفضه المدقق دون تغيير المحفظة."
        } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Extreme adjustment returns 400" `
        ($extremeAdjustment.Status -eq 400) `
        "(status=$($extremeAdjustment.Status))"
    $validAdjustmentBody = @{
        ContractId = $success.ContractId
        PendingBalanceDelta = 0
        AvailableBalanceDelta = 100
        Reason = "تصحيح مالي اختباري بعد مراجعة سجل تحرير أموال المرحلة."
    } | ConvertTo-Json
    $missingAdjustmentKey = Invoke-TestRequest `
        "Adjustment missing idempotency key" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body $validAdjustmentBody
    Assert-Test "Missing adjustment key returns 400" `
        ($missingAdjustmentKey.Status -eq 400) `
        "(status=$($missingAdjustmentKey.Status))"
    $adjustmentKey = "adjust-$([guid]::NewGuid())"
    $adjustment = Invoke-TestRequest "Apply wallet adjustment" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body $validAdjustmentBody `
        -ExtraHeaders @{ "Idempotency-Key" = $adjustmentKey }
    Require-Api "Apply wallet adjustment" $adjustment
    $adjustmentReplay = Invoke-TestRequest "Replay wallet adjustment" POST `
        "/api/admin/wallets/$($lawyer.Id)/adjustments" `
        -Token $superToken -Body $validAdjustmentBody `
        -ExtraHeaders @{ "Idempotency-Key" = $adjustmentKey }
    Require-Api "Replay wallet adjustment" $adjustmentReplay
    Assert-Test "Adjustment replay returns same record" `
        ($adjustment.Json.data.id -eq $adjustmentReplay.Json.data.id)
    $adjustmentData = @{
        walletAdjustmentId = [string]$adjustment.Json.data.id
        contractId = $success.ContractId
    }
    $adjustedNotification = Find-PaymentNotification `
        "Poll lawyer for wallet adjustment" $lawyer.Token `
        "wallet.adjusted" $adjustmentData
    Assert-PaymentNotification "Lawyer receives exact wallet-adjusted warning" `
        $adjustedNotification "wallet.adjusted" "Warning" `
        "تم تصحيح رصيد المحفظة" `
        "أجرى مسؤول النظام تصحيحًا ماليًا على محفظتك. راجع الرصيد الحالي والتفاصيل مع الدعم عند الحاجة." `
        $adjustmentData

    Write-Section "Withdrawal validation and completed/failed/delayed outcomes"
    foreach ($case in @(
        @{ Name = "Withdrawal negative amount"; Amount = -1; Destination = "mock-success" },
        @{ Name = "Withdrawal excessive precision"; Amount = 1.234; Destination = "mock-success" },
        @{ Name = "Withdrawal extreme destination"; Amount = 1; Destination = ("<script>☠</script>" + ("س" * 210)) }
    )) {
        $response = Invoke-TestRequest $case.Name POST `
            "/api/wallet/withdrawals" -Token $lawyer.Token -Body (
            @{
                Amount = $case.Amount
                DestinationReference = $case.Destination
            } | ConvertTo-Json) `
            -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
        Assert-Test "$($case.Name) returns 400" ($response.Status -eq 400) `
            "(status=$($response.Status))"
    }
    $clientWithdrawal = Invoke-TestRequest "Client cannot withdraw" POST `
        "/api/wallet/withdrawals" -Token $client.Token -Body (
        @{ Amount = 10; DestinationReference = "mock-success" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Assert-Test "Client withdrawal is forbidden" `
        ($clientWithdrawal.Status -eq 403) `
        "(status=$($clientWithdrawal.Status))"
    $missingWithdrawalKey = Invoke-TestRequest `
        "Withdrawal missing idempotency key" POST `
        "/api/wallet/withdrawals" -Token $lawyer.Token -Body (
        @{ Amount = 10; DestinationReference = "mock-success" } | ConvertTo-Json)
    Assert-Test "Missing withdrawal key returns 400" `
        ($missingWithdrawalKey.Status -eq 400) `
        "(status=$($missingWithdrawalKey.Status))"
    $withdrawKey = "withdraw-success-$([guid]::NewGuid())"
    $withdrawBody = @{
        Amount = 100
        DestinationReference = "mock-success"
    } | ConvertTo-Json
    $withdraw = Invoke-TestRequest "Complete mock withdrawal" POST `
        "/api/wallet/withdrawals" -Token $lawyer.Token `
        -Body $withdrawBody `
        -ExtraHeaders @{ "Idempotency-Key" = $withdrawKey }
    Require-Api "Complete mock withdrawal" $withdraw
    $withdrawReplay = Invoke-TestRequest "Replay completed withdrawal" POST `
        "/api/wallet/withdrawals" -Token $lawyer.Token `
        -Body $withdrawBody `
        -ExtraHeaders @{ "Idempotency-Key" = $withdrawKey }
    Require-Api "Replay completed withdrawal" $withdrawReplay
    Assert-Test "Withdrawal replay returns same entity" `
        ($withdraw.Json.data.entityId -eq $withdrawReplay.Json.data.entityId)
    $withdrawalCompletedData = @{
        withdrawalId = [string]$withdraw.Json.data.entityId
    }
    $completedWithdrawal = Find-PaymentNotification `
        "Poll lawyer for completed withdrawal" $lawyer.Token `
        "wallet.withdrawal-completed" $withdrawalCompletedData
    Assert-PaymentNotification "Lawyer receives exact completed withdrawal" `
        $completedWithdrawal "wallet.withdrawal-completed" "Success" `
        "اكتمل طلب السحب" `
        "اكتمل طلب سحب الرصيد من محفظتك بنجاح." `
        $withdrawalCompletedData

    $failedWithdrawalResponse = Invoke-TestRequest "Fail mock withdrawal" POST `
        "/api/wallet/withdrawals" -Token $lawyer.Token -Body (
        @{ Amount = 50; DestinationReference = "mock-fail" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = "withdraw-fail-$([guid]::NewGuid())" }
    Assert-Test "Confirmed withdrawal failure returns 400" `
        ($failedWithdrawalResponse.Status -eq 400) `
        "(status=$($failedWithdrawalResponse.Status))"
    $failedWithdrawalId = Get-WithdrawalId $lawyer.Id 2
    Assert-Test "Failed withdrawal fixture is queryable" `
        (-not [string]::IsNullOrWhiteSpace($failedWithdrawalId))
    $failedWithdrawalData = @{ withdrawalId = $failedWithdrawalId }
    $failedWithdrawal = Find-PaymentNotification `
        "Poll lawyer for failed withdrawal" $lawyer.Token `
        "wallet.withdrawal-failed" $failedWithdrawalData
    Assert-PaymentNotification "Lawyer receives exact failed withdrawal" `
        $failedWithdrawal "wallet.withdrawal-failed" "Warning" `
        "فشل طلب السحب" `
        "لم يكتمل طلب السحب، وأُعيد المبلغ إلى رصيد محفظتك المتاح." `
        $failedWithdrawalData

    $unknownWithdrawalResponse = Invoke-TestRequest `
        "Create uncertain mock withdrawal" POST `
        "/api/wallet/withdrawals" -Token $lawyer.Token -Body (
        @{ Amount = 25; DestinationReference = "mock-timeout" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = "withdraw-timeout-$([guid]::NewGuid())" }
    Assert-Test "Uncertain withdrawal returns 400" `
        ($unknownWithdrawalResponse.Status -eq 400) `
        "(status=$($unknownWithdrawalResponse.Status))"
    $delayedWithdrawalId = Get-WithdrawalId $lawyer.Id 0
    Assert-Test "Processing withdrawal fixture is queryable" `
        (-not [string]::IsNullOrWhiteSpace($delayedWithdrawalId))
    Accelerate-WithdrawalReconciliation $delayedWithdrawalId
    $delayedWithdrawalData = @{ withdrawalId = $delayedWithdrawalId }
    $delayedWithdrawal = Find-PaymentNotification `
        "Poll lawyer for delayed withdrawal" $lawyer.Token `
        "wallet.withdrawal-delayed" $delayedWithdrawalData 150
    Assert-PaymentNotification "Lawyer receives exact delayed withdrawal" `
        $delayedWithdrawal "wallet.withdrawal-delayed" "Warning" `
        "طلب السحب يحتاج إلى مراجعة" `
        "تأخر حسم طلب السحب ويجري التعامل معه يدويًا. لا تنشئ طلبًا بديلًا." `
        $delayedWithdrawalData

    Write-Section "Funded contract refund and role-specific notifications"
    $refundFoundation = New-ReadyPaymentFoundation "payment-refund" `
        $client.Token $lawyer.Token $lawyer.Id
    $refundFund = Invoke-TestRequest "Fund refundable milestone" POST `
        "/api/milestones/$($refundFoundation.MilestoneId)/fund" `
        -Token $client.Token -Body (
        @{ PaymentMethodReference = "mock-success" } | ConvertTo-Json) `
        -ExtraHeaders @{ "Idempotency-Key" = [guid]::NewGuid().ToString() }
    Require-Api "Fund refundable milestone" $refundFund
    $refundDetail = Get-ContractDetail "Refresh refundable contract" `
        $refundFoundation.ContractId $client.Token
    $termination = Invoke-ContractIfMatch `
        "Terminate funded contract with refund" POST `
        "/api/contracts/$($refundFoundation.ContractId)/terminate" `
        $client.Token $refundDetail.version `
        (@{ Reason = "إنهاء العقد ورد تمويل المرحلة وفق التسوية." } | ConvertTo-Json)
    Require-Api "Terminate funded contract with refund" $termination
    $refundedClient = Find-PaymentNotification `
        "Poll client for refunded funds" $client.Token `
        "funds.refunded" @{ milestoneId = $refundFoundation.MilestoneId } 120
    $refundedLawyer = Find-PaymentNotification `
        "Poll lawyer for refunded funds" $lawyer.Token `
        "funds.refunded" @{ milestoneId = $refundFoundation.MilestoneId } 120
    $refundData = @{
        milestoneId = $refundFoundation.MilestoneId
        contractId = $refundFoundation.ContractId
        proposalId = $refundFoundation.ProposalId
        legalCaseId = $refundFoundation.CaseId
        escrowHoldId = [string]$refundedClient.data.escrowHoldId
        paymentTransactionId = [string]$refundedClient.data.paymentTransactionId
    }
    Assert-PaymentNotification "Client receives exact refunded receipt" `
        $refundedClient "funds.refunded" "Success" `
        "تم رد أموال المرحلة" `
        "اكتملت تسوية المرحلة وتم رد الأموال إلى العميل." $refundData
    Assert-PaymentNotification "Lawyer receives exact refund confirmation" `
        $refundedLawyer "funds.refunded" "Information" `
        "تم رد أموال المرحلة" `
        "اكتملت تسوية المرحلة برد الأموال إلى العميل." $refundData

    Write-Section "Unsupported methods and recipient isolation"
    foreach ($test in @(
        @{ Name = "DELETE payments history unsupported"; Method = "DELETE"; Path = "/api/contracts/$($success.ContractId)/payments"; Token = $client.Token },
        @{ Name = "PATCH milestone payment unsupported"; Method = "PATCH"; Path = "/api/milestones/$($success.MilestoneId)/payment"; Token = $client.Token },
        @{ Name = "DELETE wallet unsupported"; Method = "DELETE"; Path = "/api/wallet"; Token = $lawyer.Token },
        @{ Name = "PATCH withdrawal unsupported"; Method = "PATCH"; Path = "/api/wallet/withdrawals"; Token = $lawyer.Token },
        @{ Name = "DELETE wallet adjustment unsupported"; Method = "DELETE"; Path = "/api/admin/wallets/$($lawyer.Id)/adjustments"; Token = $superToken }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path `
            -Token $test.Token -Body "{}"
        Assert-Test $test.Name ($response.Status -in @(404, 405)) `
            "(status=$($response.Status))"
    }
    $superNotifications = Invoke-TestRequest `
        "Get unrelated administrator notifications" GET `
        "/api/notifications?pageSize=50" -Token $superToken
    $targetMilestones = @(
        $success.MilestoneId,
        $retryFoundation.MilestoneId,
        $webhookFoundation.MilestoneId,
        $refundFoundation.MilestoneId)
    $leaked = @($superNotifications.Json.data.items | Where-Object {
        ($_.type -like "milestone.*" -or $_.type -like "funds.*" -or
            $_.type -like "wallet.*") -and
        ($_.data.milestoneId -in $targetMilestones -or
            $_.data.withdrawalId -in @(
                $withdrawalCompletedData.withdrawalId,
                $failedWithdrawalId,
                $delayedWithdrawalId) -or
            $_.data.walletAdjustmentId -eq $adjustmentData.walletAdjustmentId)
    })
    Assert-Test "No Payment notification leaks to unrelated administrator" `
        ($superNotifications.Status -eq 200 -and $leaked.Count -eq 0)
}
catch {
    $script:failed++
    $script:failureMessages.Add("FATAL: $($_.Exception.Message)")
    Write-Host "FATAL: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Section "Execution summary"
    "| Metric | Count |`n|---|---:|`n| Passed assertions | $script:passed |`n| Failed assertions | $script:failed |" |
        Out-File $ReportFile -Append -Encoding utf8
    if ($script:failureMessages.Count -gt 0) {
        "`n### Failures`n" | Out-File $ReportFile -Append -Encoding utf8
        foreach ($failure in $script:failureMessages) {
            "- $(Protect-ReportText $failure)" |
                Out-File $ReportFile -Append -Encoding utf8
        }
    }
    Write-Host "`nPayments notification HTTP tests complete: $script:passed passed, $script:failed failed."
}

if ($script:failed -gt 0) { exit 1 }
