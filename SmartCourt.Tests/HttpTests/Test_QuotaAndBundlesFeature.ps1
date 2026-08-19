# Test_QuotaAndBundlesFeature.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force
Add-Type -AssemblyName System.Net.Http

$reportFile = "$PSScriptRoot\QuotaAndBundles_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"

# Ensure log directory/file exists
if (!(Test-Path $apiLogPath)) {
    New-Item -Path $apiLogPath -ItemType File -Force | Out-Null
}

"# Quota and Token Bundles E2E Test Report`n`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random -Maximum 999999999
$adminEmail = "moatazmohammed2392003@gmail.com"
$adminPassword = "Admin@123"

$clientEmail = "client_quota_${randomNum}@test.com"
$clientPassword = "Password123!"

# --- SETUP: ADMIN LOGIN ---
$loginBodyAdmin = @{
    Email = $adminEmail
    Password = $adminPassword
} | ConvertTo-Json

$loginResAdmin = Invoke-Api -title "0a. Setup - Login Admin" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
if (-not $loginResAdmin -or -not $loginResAdmin.Data.AccessToken) {
    # Fallback admin
    $adminEmail = "admin@smartcourt.com"
    $loginBodyAdmin = @{
        Email = $adminEmail
        Password = $adminPassword
    } | ConvertTo-Json
    $loginResAdmin = Invoke-Api -title "0a. Setup - Login Admin Fallback" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAdmin -reportFile $reportFile
    if (-not $loginResAdmin -or -not $loginResAdmin.Data.AccessToken) {
        Write-Host "Failed to login as Admin."
        exit
    }
}
$adminToken = $loginResAdmin.Data.AccessToken

# --- SETUP: REGISTER & LOGIN CLIENT ---
$bodyClient = @{
    Email = $clientEmail
    FullName = "Client QuotaTest"
    Password = $clientPassword
    ConfirmPassword = $clientPassword
} | ConvertTo-Json

$regClientRes = Invoke-Api -title "0b. Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $bodyClient -reportFile $reportFile
if (-not $regClientRes -or -not $regClientRes.Data.UserId) {
    Write-Host "Failed to register Client."
    exit
}
$clientId = $regClientRes.Data.UserId

# Confirm Email
Start-Sleep -Seconds 2
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginBodyClient = @{
    Email = $clientEmail
    Password = $clientPassword
} | ConvertTo-Json

$loginResClient = Invoke-Api -title "0c. Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginBodyClient -reportFile $reportFile
if (-not $loginResClient -or -not $loginResClient.Data.AccessToken) {
    Write-Host "Failed to login as Client."
    exit
}
$clientToken = $loginResClient.Data.AccessToken

# ==========================================
# CLIENT SCENARIOS
# ==========================================

# 1. Get Available Bundles (Success)
$bundlesRes = Invoke-Api -title "1. Client - Get Available Bundles" -method "GET" -endpoint "/api/chat-agent/bundles" -token $clientToken -reportFile $reportFile
$bundleId = $null
if ($bundlesRes -and $bundlesRes.Data -and $bundlesRes.Data.Length -gt 0) {
    $bundleId = $bundlesRes.Data[0].Id
}

# 2. Purchase Bundle (Success)
if ($bundleId) {
    $purchaseBody = @{ BundleId = $bundleId } | ConvertTo-Json
    Invoke-Api -title "2. Client - Purchase Bundle" -method "POST" -endpoint "/api/chat-agent/bundles/purchase" -body $purchaseBody -token $clientToken -reportFile $reportFile | Out-Null
}

# 3. Purchase Bundle (Fail - Invalid Bundle Id)
$purchaseInvalidBody = @{ BundleId = 99999 } | ConvertTo-Json
Invoke-Api -title "3. Client - Purchase Bundle (Invalid ID)" -method "POST" -endpoint "/api/chat-agent/bundles/purchase" -body $purchaseInvalidBody -token $clientToken -reportFile $reportFile | Out-Null

# 4. Purchase Bundle (Fail - Malformed Body)
$purchaseMalformedBody = "{ `"invalid_json`" }"
Invoke-Api -title "4. Client - Purchase Bundle (Malformed Body)" -method "POST" -endpoint "/api/chat-agent/bundles/purchase" -body $purchaseMalformedBody -token $clientToken -reportFile $reportFile | Out-Null

# 5. Get Client Quota (Success)
Invoke-Api -title "5. Client - Get Quota" -method "GET" -endpoint "/api/agent/quota" -token $clientToken -reportFile $reportFile | Out-Null

# 6. Get Client Transactions (Success)
Invoke-Api -title "6. Client - Get Transactions" -method "GET" -endpoint "/api/agent/quota/transactions?PageNumber=1&PageSize=10" -token $clientToken -reportFile $reportFile | Out-Null

# 7. Client - Get Bundle Purchases (Success)
Invoke-Api -title "7. Client - Get Bundle Purchases" -method "GET" -endpoint "/api/chat-agent/bundles/purchases?PageNumber=1&PageSize=10" -token $clientToken -reportFile $reportFile | Out-Null

# 7b. Client - Get Default Limit (Success)
Invoke-Api -title "7b. Client - Get Default Limit" -method "GET" -endpoint "/api/agent/quota/default" -token $clientToken -reportFile $reportFile | Out-Null

# ==========================================
# ADMIN SCENARIOS
# ==========================================

# 8. Admin - Get Client Quota (Success)
Invoke-Api -title "8. Admin - Get Client Quota" -method "GET" -endpoint "/api/admin/quotas/clients/$clientId" -token $adminToken -reportFile $reportFile | Out-Null

# 9. Admin - Get Client Quota (Fail - Not Found / Bad ID)
$fakeId = [guid]::NewGuid().ToString()
Invoke-Api -title "9. Admin - Get Client Quota (Not Found)" -method "GET" -endpoint "/api/admin/quotas/clients/$fakeId" -token $adminToken -reportFile $reportFile | Out-Null

# 10. Admin - Adjust Quota (Success)
$adjustBody = @{ Amount = 50000; Reason = "Bonus grant" } | ConvertTo-Json
Invoke-Api -title "10. Admin - Adjust Quota" -method "POST" -endpoint "/api/admin/quotas/clients/$clientId/adjust" -body $adjustBody -token $adminToken -reportFile $reportFile | Out-Null

# 11. Admin - Adjust Quota (Fail - Zero Amount)
$adjustInvalidBody = @{ Amount = 0; Reason = "Test" } | ConvertTo-Json
Invoke-Api -title "11. Admin - Adjust Quota (Zero Amount)" -method "POST" -endpoint "/api/admin/quotas/clients/$clientId/adjust" -body $adjustInvalidBody -token $adminToken -reportFile $reportFile | Out-Null

# 12. Admin - Adjust Quota (Fail - Missing Reason)
$adjustNoReasonBody = @{ Amount = 1000 } | ConvertTo-Json
Invoke-Api -title "12. Admin - Adjust Quota (Missing Reason)" -method "POST" -endpoint "/api/admin/quotas/clients/$clientId/adjust" -body $adjustNoReasonBody -token $adminToken -reportFile $reportFile | Out-Null

# 13. Admin - Adjust Quota (Fail - Extreme Amount)
$adjustExtremeBody = @{ Amount = 999999999; Reason = "Extreme grant" } | ConvertTo-Json
Invoke-Api -title "13. Admin - Adjust Quota (Extreme Amount)" -method "POST" -endpoint "/api/admin/quotas/clients/$clientId/adjust" -body $adjustExtremeBody -token $adminToken -reportFile $reportFile | Out-Null

# 14. Admin - Set Client Daily Limit (Success)
$limitBody = @{ DailyLimit = 20000 } | ConvertTo-Json
Invoke-Api -title "14. Admin - Set Client Daily Limit" -method "PUT" -endpoint "/api/admin/quotas/clients/$clientId/limit" -body $limitBody -token $adminToken -reportFile $reportFile | Out-Null

# 15. Admin - Set Client Daily Limit (Fail - Negative limit)
$limitNegBody = @{ DailyLimit = -500 } | ConvertTo-Json
Invoke-Api -title "15. Admin - Set Client Daily Limit (Negative)" -method "PUT" -endpoint "/api/admin/quotas/clients/$clientId/limit" -body $limitNegBody -token $adminToken -reportFile $reportFile | Out-Null

# 16. Admin - Set Default Limit (Success)
$globalLimitBody = @{ DailyLimit = 15000 } | ConvertTo-Json
Invoke-Api -title "16. Admin - Set Default Daily Limit" -method "PUT" -endpoint "/api/admin/quotas/default-limit" -body $globalLimitBody -token $adminToken -reportFile $reportFile | Out-Null

# 17. Admin - Set Default Limit (Fail - Invalid JSON)
$globalLimitBadBody = "{ `"DailyLimit`": `"not-a-number`" }"
Invoke-Api -title "17. Admin - Set Default Daily Limit (Bad Body)" -method "PUT" -endpoint "/api/admin/quotas/default-limit" -body $globalLimitBadBody -token $adminToken -reportFile $reportFile | Out-Null

# 18. Admin - Get Client Transactions (Success)
Invoke-Api -title "18. Admin - Get Client Transactions" -method "GET" -endpoint "/api/admin/quotas/clients/$clientId/transactions?PageNumber=1&PageSize=10" -token $adminToken -reportFile $reportFile | Out-Null

# 19. Admin - Get All Purchases (Success)
Invoke-Api -title "19. Admin - Get All Purchases" -method "GET" -endpoint "/api/admin/quotas/purchases?PageNumber=1&PageSize=10" -token $adminToken -reportFile $reportFile | Out-Null

# 19b. Admin - Get All Clients Quota Summary (Success)
Invoke-Api -title "19b. Admin - Get All Clients Quota Summary" -method "GET" -endpoint "/api/admin/quotas/clients?page=1&pageSize=10" -token $adminToken -reportFile $reportFile | Out-Null

# ==========================================
# SECURITY SCENARIOS
# ==========================================

# 20. Client trying to access Admin endpoints (Unauthorized)
Invoke-Api -title "20. Client accessing Admin Adjust Quota" -method "POST" -endpoint "/api/admin/quotas/clients/$clientId/adjust" -body $adjustBody -token $clientToken -reportFile $reportFile | Out-Null
Invoke-Api -title "21. Client accessing Admin Default Limit" -method "PUT" -endpoint "/api/admin/quotas/default-limit" -body $globalLimitBody -token $clientToken -reportFile $reportFile | Out-Null

# 22. Unauthenticated access
Invoke-Api -title "22. Unauthenticated accessing Quota" -method "GET" -endpoint "/api/agent/quota" -token "" -reportFile $reportFile | Out-Null

# 23. Unauthenticated accessing Default Limit (Success)
Invoke-Api -title "23. Unauthenticated - Get Default Limit" -method "GET" -endpoint "/api/agent/quota/default" -token "" -reportFile $reportFile | Out-Null

"Tests complete. Results saved to $reportFile`n" | Write-Host
