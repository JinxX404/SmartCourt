$ErrorActionPreference = 'Stop'
$BaseUrl = "http://localhost:5049/api"
$TestRunId = [guid]::NewGuid().ToString().Substring(0, 8)
$Email = "lawyerupdates_$TestRunId@example.com"
$Password = "TestPass123!"

function Invoke-Api {
    param(
        [string]$Path,
        [string]$Method = 'GET',
        [hashtable]$Body = $null,
        [string]$Token = $null,
        [string]$IdempotencyKey = $null,
        [string]$PaymentMethodReference = $null
    )
    
    $headers = @{
        "Accept" = "application/json"
    }
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    if ($IdempotencyKey) {
        $headers["Idempotency-Key"] = $IdempotencyKey
    }
    if ($PaymentMethodReference) {
        $headers["Payment-Method-Reference"] = $PaymentMethodReference
    }
    
    $params = @{
        Uri = "$BaseUrl$Path"
        Method = $Method
        Headers = $headers
    }
    
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
        $params.ContentType = "application/json"
    }
    
    try {
        $response = Invoke-RestMethod @params
        return $response
    } catch {
        Write-Host "API Error: $_"
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errorBody = $reader.ReadToEnd()
            Write-Host "Error Body: $errorBody"
        }
        throw
    }
}

Write-Host "1. Registering Lawyer..."
$registerBody = @{
    FullName = "Test Lawyer"
    Email = $Email
    Password = $Password
    ConfirmPassword = $Password
}
$registerRes = Invoke-Api -Path "/auth/register/lawyer" -Method "POST" -Body $registerBody

Write-Host "2. Confirming Email via DB..."
$sql = "SET QUOTED_IDENTIFIER ON; UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE Email = '$Email'"
sqlcmd -S "." -d "SmartCourt_Dev" -Q $sql -C

Write-Host "3. Logging In..."
$loginBody = @{
    Email = $Email
    Password = $Password
    UserType = "Lawyer"
}
$loginRes = Invoke-Api -Path "/auth/login" -Method "POST" -Body $loginBody
$Token = $loginRes.data.accessToken

Write-Host "4. Checking Initial Quota (Should be Free plan with 100 credits/1M tokens)..."
$subInfo = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Plan Name: $($subInfo.data.planName)"
Write-Host "Total Remaining Credits (Daily + Ledger): $($subInfo.data.totalRemainingCredits)"

Write-Host "5. Fetching Lawyer Token Bundles..."
$bundles = Invoke-Api -Path "/lawyer/bundles" -Method "GET" -Token $Token
Write-Host "Returned Bundles Count: $($bundles.data.bundles.Length)"
$bundles.data.bundles | ForEach-Object { Write-Host " - $($_.name): $($_.priceEgp) EGP for $($_.creditAmount) credits" }

Write-Host "6. Initializing Subscription Purchase (Basic Plan)..."
$purchaseBody = @{
    PlanType = "Basic"
}
$purchaseRes = Invoke-Api -Path "/lawyer/subscription/change" -Method "POST" -Body $purchaseBody -Token $Token -IdempotencyKey ([guid]::NewGuid().ToString()) -PaymentMethodReference "pm_card_visa"
$InternalTransactionId = $purchaseRes.data.transactionId

Write-Host "7. Fetching ProviderTransactionId from Database..."
$sqlGetTx = "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; SELECT ProviderTransactionId FROM LawyerPaymentTransactions WHERE Id = '$InternalTransactionId'"
$ProviderTransactionId = (sqlcmd -S "." -d "SmartCourt_Dev" -Q $sqlGetTx -h -1 -W -C).Trim()

Write-Host "8. Simulating Stripe Webhook (charge.succeeded)..."
$webhookRes = Invoke-Api -Path "/debug/webhooks/simulate?providerTransactionId=$ProviderTransactionId&eventType=charge.succeeded" -Method "POST"

Write-Host "9. Checking Final Quota (Should be Basic Plan)..."
$subInfo2 = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Final Plan: $($subInfo2.data.planName)"
Write-Host "Total Remaining Credits (Daily + Ledger): $($subInfo2.data.totalRemainingCredits)"

if ($subInfo2.data.planName -eq "Basic" -and $subInfo2.data.totalRemainingCredits -ge 100) {
    Write-Host "SUCCESS! E2E Test Passed." -ForegroundColor Green
} else {
    Write-Host "FAILED! Plan did not update." -ForegroundColor Red
}
