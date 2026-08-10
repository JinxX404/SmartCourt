# 04_AuthExtended_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force

$reportFile = "$PSScriptRoot\04_AuthExtended_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Authentication Extended Flow Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$lawyerEmail = "lawyer_ext_${randomNum}@test.com"
$lawyerPass = "Password123!"

# 1. RegisterLawyer - Missing Name and Weak Password
$body = @{ Email = $lawyerEmail; Password = "password"; ConfirmPassword = "password" } | ConvertTo-Json
Invoke-Api -title "1. Register Lawyer - Missing Name & Weak Password" -method "POST" -endpoint "/api/auth/register/lawyer" -body $body -reportFile $reportFile

# 2. RegisterLawyer - Valid
$body = @{ FullName = "Test Lawyer Ext"; Email = $lawyerEmail; Password = $lawyerPass; ConfirmPassword = $lawyerPass } | ConvertTo-Json
Invoke-Api -title "2. Register Lawyer - Valid" -method "POST" -endpoint "/api/auth/register/lawyer" -body $body -reportFile $reportFile

# 3. Resend Verification
$body = @{ Email = $lawyerEmail } | ConvertTo-Json
Invoke-Api -title "3. Resend Verification" -method "POST" -endpoint "/api/auth/resend-verification" -body $body -reportFile $reportFile

# 4. Confirm Email
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

# 5. Login
$loginBody = @{ Email = $lawyerEmail; Password = $lawyerPass } | ConvertTo-Json
$loginRes = Invoke-Api -title "5. Login" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$token = $loginRes.Data.AccessToken
$refreshToken = $loginRes.Data.RefreshToken

# 6. Complete Profile (To become AccessEligible for ChangePassword and ForgotPassword)
$nationalNumPrefix = "2900101"
$nationalNumSuffix = Get-Random -Minimum 1000000 -Maximum 9999999
$nationalNum = "$nationalNumPrefix$nationalNumSuffix"
$body = @{ PhoneNumber = "+201011111111"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Law Firm 1"; Bio = "Hello"; Level = 1; NationalNumber = $nationalNum; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "6. Complete Lawyer Profile" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $token -reportFile $reportFile | Out-Null

# 6b. Re-Login to Get New Token (Because completing profile resets SecurityStamp)
$loginRes = Invoke-Api -title "6b. Re-Login Lawyer (Token Refresh)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$token = $loginRes.Data.AccessToken
$refreshToken = $loginRes.Data.RefreshToken

# 7. Change Password - Invalid Current Password
$body = @{ CurrentPassword = "WrongPassword!"; NewPassword = "NewPassword123!"; ConfirmNewPassword = "NewPassword123!" } | ConvertTo-Json
Invoke-Api -title "7. Change Password - Invalid Current Password" -method "POST" -endpoint "/api/auth/change-password" -body $body -token $token -reportFile $reportFile

# 8. Change Password - Valid
$body = @{ CurrentPassword = $lawyerPass; NewPassword = "NewPassword123!"; ConfirmNewPassword = "NewPassword123!" } | ConvertTo-Json
Invoke-Api -title "8. Change Password - Valid" -method "POST" -endpoint "/api/auth/change-password" -body $body -token $token -reportFile $reportFile
$lawyerPass = "NewPassword123!"

# 8b. Re-Login with New Password
$loginBody = @{ Email = $lawyerEmail; Password = $lawyerPass } | ConvertTo-Json
$loginRes = Invoke-Api -title "8b. Re-Login Lawyer (New Password)" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile
$token = $loginRes.Data.AccessToken
$refreshToken = $loginRes.Data.RefreshToken

# 9. Refresh Token
$body = @{ AccessToken = $token; RefreshToken = $refreshToken } | ConvertTo-Json
$refreshRes = Invoke-Api -title "9. Refresh Token" -method "POST" -endpoint "/api/auth/refresh" -body $body -reportFile $reportFile
if ($refreshRes.Success -eq $true) {
    $token = $refreshRes.Data.AccessToken
    $refreshToken = $refreshRes.Data.RefreshToken
}

# 10. Revoke Refresh Token
$body = @{ Token = $token; RefreshToken = $refreshToken } | ConvertTo-Json
Invoke-Api -title "10. Revoke Refresh Token" -method "POST" -endpoint "/api/auth/revoke" -body $body -reportFile $reportFile

# 11. Try to Refresh Token after Revoke (Should Fail)
$body = @{ AccessToken = $token; RefreshToken = $refreshToken } | ConvertTo-Json
Invoke-Api -title "11. Refresh Token - After Revocation (Should Fail)" -method "POST" -endpoint "/api/auth/refresh" -body $body -reportFile $reportFile

# 11. Forgot Password
$body = @{ Email = $lawyerEmail } | ConvertTo-Json
Invoke-Api -title "11. Forgot Password" -method "POST" -endpoint "/api/auth/forgot-password" -body $body -reportFile $reportFile

# 12. Retrieve Reset Token
$resetToken = Get-ResetTokenFromLog -email $lawyerEmail -apiLogPath $apiLogPath
if (-not $resetToken) {
    "Failed to find Reset Token in log for $lawyerEmail`n" | Out-File $reportFile -Append -Encoding utf8
}

# 13. Reset Password
$body = @{ Email = $lawyerEmail; Token = $resetToken; NewPassword = "ResetPassword123!"; ConfirmNewPassword = "ResetPassword123!" } | ConvertTo-Json
Invoke-Api -title "13. Reset Password" -method "POST" -endpoint "/api/auth/reset-password" -body $body -reportFile $reportFile

# 14. Login with Reset Password
$loginBody = @{ Email = $lawyerEmail; Password = "ResetPassword123!" } | ConvertTo-Json
Invoke-Api -title "14. Login - With Reset Password" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile

"Tests complete. Results saved to $reportFile`n" | Write-Host
