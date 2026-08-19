$ErrorActionPreference = 'Stop'
$BaseUrl = "http://localhost:5049/api"
$TestRunId = [guid]::NewGuid().ToString().Substring(0, 8)
$Email = "lawyertest_$TestRunId@example.com"
$Password = "TestPass123!"

# Utility function
function Invoke-Api {
    param(
        [string]$Path,
        [string]$Method = 'GET',
        [hashtable]$Body = $null,
        [string]$Token = $null
    )
    
    $headers = @{
        "Accept" = "application/json"
    }
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
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
    }
    catch {
        Write-Error "Request failed: $_"
        return $null
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
Write-Host "Register Response: $($registerRes | ConvertTo-Json -Depth 3)"

Write-Host "2. Confirming Email via DB..."
$sql = "SET QUOTED_IDENTIFIER ON; UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE Email = '$Email'"
sqlcmd -S "." -d "SmartCourt_Dev" -Q $sql -C
Write-Host "Email confirmed in DB."

Write-Host "3. Logging In..."
$loginBody = @{
    Email = $Email
    Password = $Password
    UserType = "Lawyer"
}
$loginRes = Invoke-Api -Path "/auth/login" -Method "POST" -Body $loginBody

$Token = $loginRes.data.accessToken
if (-not $Token) {
    Write-Host "No token obtained. Response: $($loginRes | ConvertTo-Json -Depth 3)"
    exit 1
}

Write-Host "4. Fetching Lawyer Subscription Info..."
$subInfo = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Subscription Info: $($subInfo | ConvertTo-Json -Depth 4)"

Write-Host "5. Creating Agent Conversation..."
$convBody = @{
    CaseId = $null
}
$convRes = Invoke-Api -Path "/agent/conversations" -Method "POST" -Body $convBody -Token $Token
$ConversationId = $convRes.Data.Id
Write-Host "Conversation ID: $ConversationId"

Write-Host "6. Sending Message to Agent..."
$msgBody = @{
    Content = "What is the supreme court ruling?"
}
$msgRes = Invoke-Api -Path "/agent/conversations/$ConversationId/messages" -Method "POST" -Body $msgBody -Token $Token
Write-Host "Message Sent. Output: $($msgRes | ConvertTo-Json -Depth 4)"
Start-Sleep -Seconds 5

Write-Host "7. Fetching Lawyer Subscription Info Again (Checking Quota Deductions)..."
$subInfo2 = Invoke-Api -Path "/lawyer/subscription" -Method "GET" -Token $Token
Write-Host "Subscription Info After Chat: $($subInfo2 | ConvertTo-Json -Depth 4)"

Write-Host "8. Fetching Lawyer Plans..."
$plansRes = Invoke-Api -Path "/lawyer/subscription/plans" -Method "GET" -Token $Token
Write-Host "Plans: $($plansRes | ConvertTo-Json -Depth 4)"

Write-Host "9. Fetching Bundles..."
$bundlesRes = Invoke-Api -Path "/lawyer/bundles" -Method "GET" -Token $Token
Write-Host "Bundles: $($bundlesRes | ConvertTo-Json -Depth 4)"

Write-Host "E2E Lawyer Quota Test Completed successfully!"
