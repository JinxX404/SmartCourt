$ErrorActionPreference = 'Stop'
$BaseUrl = "http://localhost:5049/api"
$TestRunId = [guid]::NewGuid().ToString().Substring(0, 8)
$Email = "lawyerpayment_$TestRunId@example.com"
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
    FullName = "Test Payment Lawyer"
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

Write-Host "4. Checking Initial Quota..."
$subInfo1 = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Initial Quota: $($subInfo1.data.availableAdditionalCredits) additional credits"

Write-Host "5. Initializing Bundle Purchase (Mostashar Pro - 1000 Credits)..."
$purchaseBody = @{
    BundleId = "bundle_pro"
}
$purchaseRes = Invoke-Api -Path "/lawyer/bundles/purchase" -Method "POST" -Body $purchaseBody -Token $Token -IdempotencyKey ([guid]::NewGuid().ToString()) -PaymentMethodReference "pm_card_visa"
$InternalTransactionId = $purchaseRes.data.transactionId
Write-Host "Checkout Initialized! Internal Transaction ID: $InternalTransactionId"
Write-Host "Client Secret (for frontend Stripe Elements): $($purchaseRes.data.clientSecret)"

Write-Host "6. Fetching ProviderTransactionId (PaymentIntent ID) from Database..."
$sqlGetTx = "SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON; SELECT ProviderTransactionId FROM LawyerPaymentTransactions WHERE Id = '$InternalTransactionId'"
$ProviderTransactionId = (sqlcmd -S "." -d "SmartCourt_Dev" -Q $sqlGetTx -h -1 -W -C).Trim()
Write-Host "Provider Transaction ID: $ProviderTransactionId"

Write-Host "7. Simulating Stripe Webhook (charge.succeeded)..."
$webhookRes = Invoke-Api -Path "/debug/webhooks/simulate?providerTransactionId=$ProviderTransactionId&eventType=charge.succeeded" -Method "POST"
Write-Host "Webhook Processed! Result: $($webhookRes | ConvertTo-Json -Depth 3)"

Write-Host "8. Checking Final Quota (Should have +1000 additional credits)..."
$subInfo2 = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Final Quota: $($subInfo2.data.availableAdditionalCredits) additional credits"

if ($subInfo2.data.availableAdditionalCredits -eq 1000) {
    Write-Host "SUCCESS! The payment was successfully reconciled and tokens were added to the lawyer's balance." -ForegroundColor Green
} else {
    Write-Host "FAILED! The tokens were not added." -ForegroundColor Red
}
