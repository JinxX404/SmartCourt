# 02_ClientProfile_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\02_ClientProfile_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Client Profile CRUD Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$clientEmail = "client_crud_${randomNum}@test.com"

# Setup: Register, Confirm, and Login a Client
$body = @{ FullName = "Client Crud"; Email = $clientEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "0. Setup - Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $body -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $clientEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginBody = @{ Email = $clientEmail; Password = "Password123!" } | ConvertTo-Json
$loginRes = Invoke-Api -title "0. Setup - Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$clientToken = $loginRes.Data.AccessToken

# 1. Complete Profile - Missing Fields (Phone, DateOfBirth)
$body = @{ Gender = 1; Address = "Cairo" } | ConvertTo-Json
Invoke-Api -title "1. Client Complete - Missing Phone & DOB" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken -reportFile $reportFile

# 2. Complete Profile - Invalid Phone Format
$body = @{ PhoneNumber = "123456789"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo" } | ConvertTo-Json
Invoke-Api -title "2. Client Complete - Invalid Phone Format" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken -reportFile $reportFile

# 3. Complete Profile - Future DOB
$futureDate = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = $futureDate; Gender = 1; Address = "Cairo" } | ConvertTo-Json
Invoke-Api -title "3. Client Complete - Future Date of Birth" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken -reportFile $reportFile

# 4. Complete Profile - Valid Data
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Cairo" } | ConvertTo-Json
Invoke-Api -title "4. Client Complete - Valid Data" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken -reportFile $reportFile

# 5. Re-Login to Get Active Token
$loginRes = Invoke-Api -title "5. Re-Login Client (Token Refresh)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$clientToken = $loginRes.Data.AccessToken

# 6. Get Private Profile
Invoke-Api -title "6. Client GET Private Profile" -method "GET" -endpoint "/api/clients/profile" -token $clientToken -reportFile $reportFile

# 7. Update Profile - Invalid Phone
$body = @{ PhoneNumber = "invalid_phone"; Address = "Alexandria" } | ConvertTo-Json
Invoke-Api -title "7. Client Update - Invalid Phone Format" -method "PUT" -endpoint "/api/clients/profile" -body $body -token $clientToken -reportFile $reportFile

# 8. Update Profile - Valid Data
$body = @{ PhoneNumber = "+201222222222"; Address = "Alexandria" } | ConvertTo-Json
Invoke-Api -title "8. Client Update - Valid Data" -method "PUT" -endpoint "/api/clients/profile" -body $body -token $clientToken -reportFile $reportFile

# 8b. Re-Login to Get New Token (Because updating PhoneNumber resets SecurityStamp)
$loginRes = Invoke-Api -title "8b. Re-Login Client (Token Refresh)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$clientToken = $loginRes.Data.AccessToken

# 9. Get Private Profile After Update
Invoke-Api -title "9. Client GET Private Profile (Verify Update)" -method "GET" -endpoint "/api/clients/profile" -token $clientToken -reportFile $reportFile

# 10. Delete Account - Invalid Password
$body = @{ CurrentPassword = "WrongPassword!" } | ConvertTo-Json
Invoke-Api -title "10. Client Delete Account - Wrong Password" -method "DELETE" -endpoint "/api/clients/profile" -body $body -token $clientToken -reportFile $reportFile

# 11. Delete Account - Valid Password
$body = @{ CurrentPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "11. Client Delete Account - Success" -method "DELETE" -endpoint "/api/clients/profile" -body $body -token $clientToken -reportFile $reportFile

"Tests complete. Results saved to $reportFile`n" | Write-Host
