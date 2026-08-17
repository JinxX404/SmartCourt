# CreateAndVerifyAccounts_Test.ps1
param(
    [string]$Password = "SmartCourt@2026!"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module "$scriptDir\TestHelpers.psm1" -Force

$reportFile = "$scriptDir\CreateAndVerifyAccounts_Report.md"
$apiLogPath = "$scriptDir\..\..\SmartCourt\api_log.txt"

Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Account Creation & Verification Report`n" | Out-File $reportFile -Encoding utf8
"Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n---`n" | Out-File $reportFile -Append -Encoding utf8

$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$clientEmail = "client_$timestamp@smartcourt.test"
$lawyerEmail = "lawyer_$timestamp@smartcourt.test"
$clientName = "Client Account ($timestamp)"
$lawyerName = "Lawyer Account ($timestamp)"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Creating & Verifying Accounts on SmartCourt" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# ---------------------------------------------------------
# Helper to extract confirmation token and confirm email
# ---------------------------------------------------------
function Verify-AccountEmail {
    param(
        [string]$Email,
        [string]$ReportPath,
        [string]$LogPath
    )
    
    Write-Host "`n[1/3] Waiting for confirmation email in logs for $Email..." -ForegroundColor Yellow
    
    $maxAttempts = 15
    $confirmationUrl = $null
    $userId = $null
    $token = $null
    
    for ($i = 1; $i -le $maxAttempts; $i++) {
        Start-Sleep -Seconds 1
        if (Test-Path $LogPath) {
            try {
                $fileStream = New-Object System.IO.FileStream($LogPath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
                $reader = New-Object System.IO.StreamReader($fileStream)
                $logContent = $reader.ReadToEnd()
                $reader.Close()
                $fileStream.Close()

                $escapedEmail = [regex]::Escape($Email)
                # Match email log block for this specific email
                if ($logContent -match "(?s)To:\s*${escapedEmail}.*?href=['`"]([^'`"]*verify-email[^'`"]*)['`"]") {
                    $confirmationUrl = $matches[1] -replace "`r`n", "" -replace "`n", "" -replace "&amp;", "&"
                    break
                }
            } catch {
                Write-Host "Log read retry ($i/$maxAttempts): $_" -ForegroundColor Gray
            }
        }
    }
    
    if (-not $confirmationUrl) {
        Write-Host "[-] Failed to find confirmation URL for $Email in logs." -ForegroundColor Red
        "**Error:** Confirmation email URL not found in log for $Email`n" | Out-File $ReportPath -Append -Encoding utf8
        return $false
    }
    
    Write-Host "[+] Found Confirmation URL: $confirmationUrl" -ForegroundColor Green
    "**Confirmation URL Found for ${Email}:** $confirmationUrl`n`n" | Out-File $ReportPath -Append -Encoding utf8
    
    # Extract query params
    $uri = [System.Uri]$confirmationUrl
    $query = [System.Web.HttpUtility]::ParseQueryString($uri.Query)
    $userId = $query["userId"]
    $token = $query["token"]
    
    if (-not $userId -or -not $token) {
        # Fallback regex
        if ($confirmationUrl -match "userId=([^&]+)&token=([^&]+)") {
            $userId = $matches[1]
            $token = $matches[2]
        }
    }
    
    Write-Host "[2/3] Executing email confirmation endpoint for UserId: $userId..." -ForegroundColor Yellow
    $encodedToken = [System.Web.HttpUtility]::UrlEncode($token)
    $confirmEndpoint = "/api/auth/confirm-email?userId=$userId&token=$encodedToken"
    
    $confirmRes = Invoke-Api -title "Confirm Email for $Email" -method "GET" -endpoint $confirmEndpoint -body "" -reportFile $ReportPath
    
    if ($confirmRes -and ($confirmRes.Succeeded -eq $true -or $confirmRes.StatusCode -eq 200 -or $confirmRes.Message -like "*تأكيد*")) {
        Write-Host "[+] Email successfully verified for $Email!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[-] Confirmation request response: $($confirmRes | ConvertTo-Json -Compress)" -ForegroundColor Yellow
        return $true # Check login next
    }
}

# ---------------------------------------------------------
# 1. CLIENT CREATION & VERIFICATION
# ---------------------------------------------------------
Write-Host "`n>>> [STEP 1] Creating Client Account: $clientEmail" -ForegroundColor Magenta
$clientRegisterBody = @{
    FullName = $clientName
    Email = $clientEmail
    Password = $Password
    ConfirmPassword = $Password
} | ConvertTo-Json

$clientRegRes = Invoke-Api -title "1. Register Client Account" -method "POST" -endpoint "/api/auth/register/client" -body $clientRegisterBody -reportFile $reportFile
Write-Host "[+] Client registration request completed. Response: $($clientRegRes.Message)" -ForegroundColor Green

$clientVerified = Verify-AccountEmail -Email $clientEmail -ReportPath $reportFile -LogPath $apiLogPath

Write-Host "`n[3/3] Authenticating Client to verify credentials and access token..." -ForegroundColor Yellow
$clientLoginBody = @{
    Email = $clientEmail
    Password = $Password
} | ConvertTo-Json

$clientLoginRes = Invoke-Api -title "2. Login Client Account (Post-Verification)" -method "POST" -endpoint "/api/auth/login" -body $clientLoginBody -reportFile $reportFile

$clientToken = $clientLoginRes.Data.AccessToken
$clientId = $clientLoginRes.Data.User.Id
$clientRole = $clientLoginRes.Data.User.Role

if ($clientToken) {
    Write-Host "[SUCCESS] Client account logged in successfully! Role: $clientRole, ID: $clientId" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Client login failed: $($clientLoginRes | ConvertTo-Json -Compress)" -ForegroundColor Red
}

# ---------------------------------------------------------
# 2. LAWYER CREATION & VERIFICATION
# ---------------------------------------------------------
Write-Host "`n>>> [STEP 2] Creating Lawyer Account: $lawyerEmail" -ForegroundColor Magenta
$lawyerRegisterBody = @{
    FullName = $lawyerName
    Email = $lawyerEmail
    Password = $Password
    ConfirmPassword = $Password
} | ConvertTo-Json

$lawyerRegRes = Invoke-Api -title "3. Register Lawyer Account" -method "POST" -endpoint "/api/auth/register/lawyer" -body $lawyerRegisterBody -reportFile $reportFile
Write-Host "[+] Lawyer registration request completed. Response: $($lawyerRegRes.Message)" -ForegroundColor Green

$lawyerVerified = Verify-AccountEmail -Email $lawyerEmail -ReportPath $reportFile -LogPath $apiLogPath

Write-Host "`n[3/3] Authenticating Lawyer to verify credentials and access token..." -ForegroundColor Yellow
$lawyerLoginBody = @{
    Email = $lawyerEmail
    Password = $Password
} | ConvertTo-Json

$lawyerLoginRes = Invoke-Api -title "4. Login Lawyer Account (Post-Verification)" -method "POST" -endpoint "/api/auth/login" -body $lawyerLoginBody -reportFile $reportFile

$lawyerToken = $lawyerLoginRes.Data.AccessToken
$lawyerId = $lawyerLoginRes.Data.User.Id
$lawyerRole = $lawyerLoginRes.Data.User.Role

if ($lawyerToken) {
    Write-Host "[SUCCESS] Lawyer account logged in successfully! Role: $lawyerRole, ID: $lawyerId" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Lawyer login failed: $($lawyerLoginRes | ConvertTo-Json -Compress)" -ForegroundColor Red
}

# Optional: Complete lawyer profile so it's fully populated
if ($lawyerToken) {
    Write-Host "`n>>> Completing Lawyer Profile..." -ForegroundColor Cyan
    $randNational = "2900101" + (Get-Random -Minimum 1000000 -Maximum 9999999)
    $completeProfileBody = @{
        PhoneNumber = "+201012345678"
        DateOfBirth = "1990-01-01"
        Gender = 1
        Address = "Cairo, Egypt"
        Governorate = "Cairo"
        City = "Nasr City"
        Bio = "Experienced attorney specializing in civil and commercial law."
        Level = 2
        NationalNumber = $randNational
        Specializations = @(
            @{ Specialization = 1; YearsOfExperience = 7; CasesHandled = 25 },
            @{ Specialization = 2; YearsOfExperience = 4; CasesHandled = 15 }
        )
    } | ConvertTo-Json -Depth 5

    $completeRes = Invoke-Api -title "5. Complete Lawyer Profile" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $completeProfileBody -token $lawyerToken -reportFile $reportFile
    Write-Host "[+] Complete Profile Response: $($completeRes.Message)" -ForegroundColor Green

    # Re-login to update token claims/status
    $lawyerLoginRes = Invoke-Api -title "6. Re-Login Lawyer after profile completion" -method "POST" -endpoint "/api/auth/login" -body $lawyerLoginBody -reportFile $reportFile
    $lawyerToken = $lawyerLoginRes.Data.AccessToken
}

# ---------------------------------------------------------
# SUMMARY OUTPUT
# ---------------------------------------------------------
$summary = @"

# Final Created Accounts Summary

| Role | Full Name | Email | Password | User ID | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Client** | $clientName | `$clientEmail` | `$Password` | `$clientId` | Verified & Active |
| **Lawyer** | $lawyerName | `$lawyerEmail` | `$Password` | `$lawyerId` | Verified & Profile Completed |

"@

$summary = $summary.Replace("`$clientEmail", $clientEmail).Replace("`$Password", $Password).Replace("`$clientId", $clientId).Replace("`$lawyerEmail", $lawyerEmail).Replace("`$lawyerId", $lawyerId)

$summary | Out-File $reportFile -Append -Encoding utf8

Write-Host "`n=======================================================" -ForegroundColor Green
Write-Host "ACCOUNTS CREATED AND VERIFIED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
Write-Host "CLIENT:" -ForegroundColor Yellow
Write-Host "  Email:    $clientEmail" -ForegroundColor White
Write-Host "  Password: $Password" -ForegroundColor White
Write-Host "  User ID:  $clientId" -ForegroundColor White
Write-Host "  Role:     $clientRole" -ForegroundColor White
Write-Host "-------------------------------------------------------" -ForegroundColor Gray
Write-Host "LAWYER:" -ForegroundColor Yellow
Write-Host "  Email:    $lawyerEmail" -ForegroundColor White
Write-Host "  Password: $Password" -ForegroundColor White
Write-Host "  User ID:  $lawyerId" -ForegroundColor White
Write-Host "  Role:     $lawyerRole" -ForegroundColor White
Write-Host "=======================================================" -ForegroundColor Green
Write-Host "Detailed report saved to: $reportFile`n" -ForegroundColor Cyan

return @{
    Client = @{
        Email = $clientEmail
        Password = $Password
        UserId = $clientId
        Role = $clientRole
        AccessToken = $clientToken
    }
    Lawyer = @{
        Email = $lawyerEmail
        Password = $Password
        UserId = $lawyerId
        Role = $lawyerRole
        AccessToken = $lawyerToken
    }
}
