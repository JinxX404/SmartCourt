# 03_LawyerProfile_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\03_LawyerProfile_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Lawyer Profile CRUD Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$lawyerEmail = "lawyer_crud_${randomNum}@test.com"

# Setup: Register, Confirm, and Login a Lawyer
$body = @{ FullName = "Lawyer Crud"; Email = $lawyerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "0. Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $body -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$loginRes = Invoke-Api -title "0. Setup - Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$lawyerToken = $loginRes.Data.AccessToken

$nationalNumPrefix = "2900101"
$nationalNumSuffix = Get-Random -Minimum 1000000 -Maximum 9999999
$nationalNum = "$nationalNumPrefix$nationalNumSuffix"

# 1. Complete Profile - Missing Fields (NationalNumber, Bio, Level)
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Law Firm 1" } | ConvertTo-Json
Invoke-Api -title "1. Lawyer Complete - Missing NationalNumber & Bio" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken -reportFile $reportFile

# 2. Complete Profile - Invalid National Number Length
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Law Firm 1"; Bio = "Hello"; Level = 1; NationalNumber = "123" } | ConvertTo-Json
Invoke-Api -title "2. Lawyer Complete - Invalid National Number Length" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken -reportFile $reportFile

# 3. Complete Profile - Invalid Lawyer Level
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Law Firm 1"; Bio = "Hello"; Level = 999; NationalNumber = $nationalNum } | ConvertTo-Json
Invoke-Api -title "3. Lawyer Complete - Invalid Lawyer Level" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken -reportFile $reportFile

# 4. Complete Profile - Valid Data
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Law Firm 1"; Bio = "Hello"; Level = 1; NationalNumber = $nationalNum } | ConvertTo-Json
Invoke-Api -title "4. Lawyer Complete - Valid Data" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken -reportFile $reportFile

# 5. Re-Login to Get PendingReview Token
$loginRes = Invoke-Api -title "5. Re-Login Lawyer (Token Refresh)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$lawyerToken = $loginRes.Data.AccessToken
$lawyerId = $loginRes.Data.User.Id

# 6. Get Private Profile
Invoke-Api -title "6. Lawyer GET Private Profile" -method "GET" -endpoint "/api/lawyers/profile" -token $lawyerToken -reportFile $reportFile

# 7. Get Public Profile (Anonymous)
Invoke-Api -title "7. Lawyer GET Public Profile (Anonymous)" -method "GET" -endpoint "/api/lawyers/public/$lawyerId" -reportFile $reportFile

# 8. Update Profile - Bio Exceeds Max Length
$longBio = "A" * 501
$body = @{ PhoneNumber = "+201222222222"; Address = "New Address"; Bio = $longBio; Level = 2 } | ConvertTo-Json
Invoke-Api -title "8. Lawyer Update - Bio Exceeds Max Length" -method "PUT" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken -reportFile $reportFile

# 9. Update Profile - Valid Data
$body = @{ PhoneNumber = "+201222222222"; Address = "New Address"; Bio = "Updated Bio"; Level = 2 } | ConvertTo-Json
Invoke-Api -title "9. Lawyer Update - Valid Data" -method "PUT" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken -reportFile $reportFile

# 9b. Re-Login to Get New Token (Because updating PhoneNumber resets SecurityStamp)
$loginRes = Invoke-Api -title "9b. Re-Login Lawyer (Token Refresh)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$lawyerToken = $loginRes.Data.AccessToken

# 10. Get Private Profile After Update
Invoke-Api -title "10. Lawyer GET Private Profile (Verify Update)" -method "GET" -endpoint "/api/lawyers/profile" -token $lawyerToken -reportFile $reportFile

# 11. Delete Account - Invalid Password
$body = @{ CurrentPassword = "WrongPassword!" } | ConvertTo-Json
Invoke-Api -title "11. Lawyer Delete Account - Wrong Password" -method "DELETE" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken -reportFile $reportFile

# 12. Delete Account - Valid Password
$body = @{ CurrentPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "12. Lawyer Delete Account - Success" -method "DELETE" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken -reportFile $reportFile

# 13. Get Public Profile After Delete (Should be Not Found or specific error)
Invoke-Api -title "13. Lawyer GET Public Profile (After Delete)" -method "GET" -endpoint "/api/lawyers/public/$lawyerId" -reportFile $reportFile

"Tests complete. Results saved to $reportFile`n" | Write-Host
