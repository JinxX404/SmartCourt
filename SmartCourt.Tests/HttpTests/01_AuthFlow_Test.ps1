# 01_AuthFlow_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\01_AuthFlow_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Authentication Flow Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$clientEmail = "client_auth_${randomNum}@test.com"

# 1. Register Client - Missing FullName
$body = @{ Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "1. Register Client - Missing FullName" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 2. Register Client - Missing Email
$body = @{ FullName = "Test Client"; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "2. Register Client - Missing Email" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 3. Register Client - Invalid Email Format
$body = @{ FullName = "Test Client"; Email = "invalid_email"; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "3. Register Client - Invalid Email Format" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 4. Register Client - Weak Password (No Uppercase/Lowercase/Number)
$body = @{ FullName = "Test Client"; Email = $clientEmail; Password = "password"; ConfirmPassword = "password" } | ConvertTo-Json
Invoke-Api -title "4. Register Client - Weak Password" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 5. Register Client - Mismatched ConfirmPassword
$body = @{ FullName = "Test Client"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password1234!" } | ConvertTo-Json
Invoke-Api -title "5. Register Client - Mismatched ConfirmPassword" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 6. Register Client - Valid
$body = @{ FullName = "Test Client"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "6. Register Client - Valid Data" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile

# 7. Login before Email Confirmation (Should fail 403)
$loginBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
Invoke-Api -title "7. Login Client - Unconfirmed Email" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile

# 8. Confirm Email
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

# 9. Login after Email Confirmation (Should succeed)
$loginRes = Invoke-Api -title "9. Login Client - Confirmed Email" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile

"Tests complete. Results saved to $reportFile`n" | Write-Host
