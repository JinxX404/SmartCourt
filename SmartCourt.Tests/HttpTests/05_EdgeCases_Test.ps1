# 05_EdgeCases_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\05_EdgeCases_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Extreme Edge Cases & Security Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$baseEmail = "EdgeCase_${randomNum}"
$clientEmail = "${baseEmail}@TeSt.cOm" # Intentionally mixed case
$normalizedEmail = "${baseEmail}@test.com"
$pass = "Password123!"

# 1. Case Sensitivity & Normalization Attacks
$body = @{ FullName = "Client Edge"; Email = $clientEmail; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "1. Register Client (Mixed Case Email)" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile | Out-Null

$body = @{ FullName = "Client Duplicate"; Email = $normalizedEmail; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "1b. Register Client (Lowercase Email - Duplicate)" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile | Out-Null

# 2. Cross-Role Email Conflict
$body = @{ FullName = "Lawyer Conflict"; Email = $normalizedEmail; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "2. Cross-Role Registration Conflict (Lawyer with Client Email)" -method "POST" -endpoint "/api/auth/register/lawyer" -body $body -reportFile $reportFile | Out-Null

# Confirm Email so we can test Login
Confirm-EmailFromLog -email $normalizedEmail -reportFile $reportFile -apiLogPath $apiLogPath | Out-Null

# 4. Login with Normalized Email (UPPERCASE)
$body = @{ Email = "${baseEmail}@TEST.COM"; Password = $pass } | ConvertTo-Json
$loginRes = Invoke-Api -title "4. Login (UPPERCASE Email)" -method "POST" -endpoint "/api/auth/login" -body $body -reportFile $reportFile
$clientToken = $loginRes.Data.AccessToken

# 5. Invalid Auth Tokens
Invoke-Api -title "5a. Invalid Confirm Email Token" -method "GET" -endpoint "/api/auth/confirm-email?userId=fake-id&token=fake-token" -reportFile $reportFile | Out-Null
$body = @{ Email = $normalizedEmail; Token = "fake-token"; NewPassword = $pass; ConfirmNewPassword = $pass } | ConvertTo-Json
Invoke-Api -title "5b. Invalid Reset Password Token" -method "POST" -endpoint "/api/auth/reset-password" -body $body -reportFile $reportFile | Out-Null

# 6. Invalid/Spoofed JWTs
$spoofedToken = $clientToken + "fake"
Invoke-Api -title "6a. Spoofed JWT Token" -method "GET" -endpoint "/api/clients/profile" -token $spoofedToken -reportFile $reportFile | Out-Null
Invoke-Api -title "6b. Malformed Authorization Header" -method "GET" -endpoint "/api/clients/profile" -headers @{ "Authorization" = "Bearer" } -reportFile $reportFile | Out-Null

# 7. RBAC Violations
Invoke-Api -title "7. RBAC: Client attempting to access Lawyer endpoint" -method "GET" -endpoint "/api/lawyers/profile" -token $clientToken -reportFile $reportFile | Out-Null

# 8. Malformed JSON & Content-Type
Invoke-Api -title "8a. Empty Body" -method "POST" -endpoint "/api/auth/login" -body "" -reportFile $reportFile | Out-Null
Invoke-Api -title "8b. Malformed JSON" -method "POST" -endpoint "/api/auth/login" -body "{ `"Email`": `"test`"" -reportFile $reportFile | Out-Null
try {
    Invoke-RestMethod -Uri "http://localhost:5049/api/auth/login" -Method POST -Body '{"Email": "a@a.com"}' -Headers @{ "Content-Type" = "text/plain" } -ErrorAction Stop
} catch {
    $status = $_.Exception.Response.StatusCode.value__
}
"### 8c. Missing Content-Type`n**Response Status:** $status`n---`n" | Out-File -Append $reportFile -Encoding utf8

# 9. Malicious Payloads (SQLi, XSS, Unicode, Null Byte)
$maliciousBody = @{ FullName = "'; DROP TABLE Users; --"; Email = "sqli_${randomNum}@test.com"; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "9a. Malicious Payload (SQLi)" -method "POST" -endpoint "/api/auth/register/client" -body $maliciousBody -reportFile $reportFile | Out-Null

$maliciousBody = @{ FullName = "<script>alert('XSS')</script>"; Email = "xss_${randomNum}@test.com"; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "9b. Malicious Payload (XSS)" -method "POST" -endpoint "/api/auth/register/client" -body $maliciousBody -reportFile $reportFile | Out-Null

$maliciousBody = @{ FullName = "👨‍👩‍👧‍👦 Zalgo T̵e̷x̵t̵"; Email = "unicode_${randomNum}@test.com"; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "9c. Malicious Payload (Unicode & Emojis)" -method "POST" -endpoint "/api/auth/register/client" -body $maliciousBody -reportFile $reportFile | Out-Null

$maliciousBody = @{ FullName = "Null\u0000Byte"; Email = "null_${randomNum}@test.com"; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "9d. Malicious Payload (Null Byte)" -method "POST" -endpoint "/api/auth/register/client" -body $maliciousBody -reportFile $reportFile | Out-Null

# 10. CPU Exhaustion & Massive Payloads
$massivePassword = "a" * 15000 + "B1!"
$massiveBody = @{ FullName = "CPU Test"; Email = "cpu_${randomNum}@test.com"; Password = $massivePassword; ConfirmPassword = $massivePassword } | ConvertTo-Json
Invoke-Api -title "10a. CPU Exhaustion (15,000 char password)" -method "POST" -endpoint "/api/auth/register/client" -body $massiveBody -reportFile $reportFile | Out-Null

$massiveBio = "a" * 60000
$massiveBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = $massiveBio } | ConvertTo-Json
Invoke-Api -title "10b. Massive Payload (60,000 char Address)" -method "POST" -endpoint "/api/clients/profile/complete" -body $massiveBody -token $clientToken -reportFile $reportFile | Out-Null

# 11. Type Mismatch & Impossible Dates
$invalidBody = '{ "PhoneNumber": "+201011111111", "DateOfBirth": "1990-01-01", "Gender": "NotAnInteger", "Address": "Test" }'
Invoke-Api -title "11a. Type Mismatch (Gender string instead of enum/int)" -method "POST" -endpoint "/api/clients/profile/complete" -body $invalidBody -token $clientToken -reportFile $reportFile | Out-Null

$invalidBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "3000-01-01"; Gender = 1; Address = "Test" } | ConvertTo-Json
Invoke-Api -title "11b. Impossible Date (Year 3000)" -method "POST" -endpoint "/api/clients/profile/complete" -body $invalidBody -token $clientToken -reportFile $reportFile | Out-Null

$invalidBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = (Get-Date).AddYears(-2).ToString("yyyy-MM-dd"); Gender = 1; Address = "Test" } | ConvertTo-Json
Invoke-Api -title "11c. Impossible Date (2 Years Old)" -method "POST" -endpoint "/api/clients/profile/complete" -body $invalidBody -token $clientToken -reportFile $reportFile | Out-Null

# Complete Profile Properly to proceed
$validBody = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Test" } | ConvertTo-Json
Invoke-Api -title "Setup: Complete Client Profile" -method "POST" -endpoint "/api/clients/profile/complete" -body $validBody -token $clientToken -reportFile $reportFile | Out-Null

# 12. Double Profile Completion
Invoke-Api -title "12. Double Profile Completion" -method "POST" -endpoint "/api/clients/profile/complete" -body $validBody -token $clientToken -reportFile $reportFile | Out-Null

# 13. Deleted Account Login
$deleteBody = @{ Password = $pass } | ConvertTo-Json
Invoke-Api -title "Setup: Delete Client Account" -method "DELETE" -endpoint "/api/clients/profile" -body $deleteBody -token $clientToken -reportFile $reportFile | Out-Null

$body = @{ Email = $normalizedEmail; Password = $pass } | ConvertTo-Json
Invoke-Api -title "13. Deleted Account Login" -method "POST" -endpoint "/api/auth/login" -body $body -reportFile $reportFile | Out-Null

# Setup new user for Brute Force test
$bruteEmail = "brute_$randomNum@test.com"
$body = @{ FullName = "Brute Force User"; Email = $bruteEmail; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
Invoke-Api -title "Setup: Register Brute Force User" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $bruteEmail -reportFile $reportFile -apiLogPath $apiLogPath | Out-Null

# 13b. Brute Force Lockout
for ($i = 1; $i -le 6; $i++) {
    $body = @{ Email = $bruteEmail; Password = "WrongPassword!" } | ConvertTo-Json
    Invoke-Api -title "13b. Brute Force Login Attempt $i" -method "POST" -endpoint "/api/auth/login" -body $body -reportFile $reportFile | Out-Null
}

# 13c. Login with Normalized Email (UPPERCASE) - Should fail if locked out!
$body = @{ Email = "BRUTE_$randomNum@TEST.COM"; Password = $pass } | ConvertTo-Json
Invoke-Api -title "13c. Login (UPPERCASE Email - Locked Out)" -method "POST" -endpoint "/api/auth/login" -body $body -reportFile $reportFile | Out-Null

# 14. Rate Limiting Bombardment
"### 14. Rate Limiting Bombardment`n" | Out-File -Append $reportFile -Encoding utf8
$rateLimitScript = {
    param($url, $body)
    try {
        $res = Invoke-RestMethod -Uri $url -Method POST -Body $body -Headers @{ "Content-Type" = "application/json" } -ErrorAction Stop
        return 200
    } catch {
        return $_.Exception.Response.StatusCode.value__
    }
}
$url = "http://localhost:5049/api/auth/login"
$body = @{ Email = "rate_$randomNum@test.com"; Password = "Password123!" } | ConvertTo-Json
$jobs = @()
for ($i = 0; $i -lt 50; $i++) {
    $jobs += Start-Job -ScriptBlock $rateLimitScript -ArgumentList $url, $body
}
$results = Wait-Job -Job $jobs | Receive-Job
$counts = $results | Group-Object
foreach ($group in $counts) {
    "**Status Code:** $($group.Name) - Count: $($group.Count)`n" | Out-File -Append $reportFile -Encoding utf8
}
"---`n" | Out-File -Append $reportFile -Encoding utf8

# 15. Concurrent Registration (Race Condition)
"### 15. Concurrent Registration (Race Condition)`n" | Out-File -Append $reportFile -Encoding utf8
$raceEmail = "race_${randomNum}@test.com"
$raceBody = @{ FullName = "Race Condition"; Email = $raceEmail; Password = $pass; ConfirmPassword = $pass } | ConvertTo-Json
$raceUrl = "http://localhost:5049/api/auth/register/client"
$jobs = @()
for ($i = 0; $i -lt 10; $i++) {
    $jobs += Start-Job -ScriptBlock $rateLimitScript -ArgumentList $raceUrl, $raceBody
}
$results = Wait-Job -Job $jobs | Receive-Job
$counts = $results | Group-Object
foreach ($group in $counts) {
    "**Status Code:** $($group.Name) - Count: $($group.Count)`n" | Out-File -Append $reportFile -Encoding utf8
}
"---`n" | Out-File -Append $reportFile -Encoding utf8

"Tests complete. Results saved to $reportFile"
