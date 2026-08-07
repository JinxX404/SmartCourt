$ErrorActionPreference = "Continue"

$baseUrl = "https://localhost:7119" # Or HTTP depending on launchSettings.json, let's use HTTP to avoid SSL warnings
$httpUrl = "http://localhost:5049"

$reportFile = "$PSScriptRoot\ProfileEndpoints_Report.md"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# Profile Endpoints API Test Report`n" | Out-File $reportFile -Encoding utf8

function Log-Test {
    param([string]$title, [string]$method, [string]$url, [string]$body, [string]$responseStatus, [string]$responseBody)
    "### $title`n" | Out-File $reportFile -Append -Encoding utf8
    "**Request:** $method $url`n" | Out-File $reportFile -Append -Encoding utf8
    if ($body) {
        "**Body:**`n$body`n" | Out-File $reportFile -Append -Encoding utf8
    }
    "**Response Status:** $responseStatus`n" | Out-File $reportFile -Append -Encoding utf8
    "**Response Body:**`n$responseBody`n---`n" | Out-File $reportFile -Append -Encoding utf8
}

function Invoke-Api {
    param([string]$title, [string]$method, [string]$endpoint, [string]$body, [string]$token)
    $headers = @{
        "Content-Type" = "application/json"
    }
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    $url = "$httpUrl$endpoint"
    try {
        if ([string]::IsNullOrWhiteSpace($body) -or $method -eq "GET") {
            $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -UseBasicParsing
        } else {
            $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -Body $body -UseBasicParsing
        }
        $status = $response.StatusCode
        $responseBody = $response.Content
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $responseBody
        return ($responseBody | ConvertFrom-Json)
    } catch {
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorResponse = $reader.ReadToEnd()
            $status = $_.Exception.Response.StatusCode.value__
        } else {
            $errorResponse = $_.Exception.Message
            $status = "Error"
        }
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $errorResponse
        return ($errorResponse | ConvertFrom-Json -ErrorAction SilentlyContinue)
    }
}

# 1. Register Client
$clientEmail = "client_test_$(Get-Random)@test.com"
$clientRegBody = @{
    FullName = "Test Client"
    Email = $clientEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

Invoke-Api -title "1. Register Client" -method "POST" -endpoint "/api/auth/register/client" -body $clientRegBody

# 2. Register Lawyer
$lawyerEmail = "lawyer_test_$(Get-Random)@test.com"
$lawyerRegBody = @{
    FullName = "Test Lawyer"
    Email = $lawyerEmail
    Password = "Password123!"
    ConfirmPassword = "Password123!"
} | ConvertTo-Json

Invoke-Api -title "2. Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $lawyerRegBody

# 3. Confirm Emails using API
Start-Sleep -Seconds 3 # Wait for hangfire job to log the email

function Confirm-EmailFromLog([string]$email) {
    $apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
    $fullLog = Get-Content $apiLogPath -Raw -ErrorAction SilentlyContinue
    if (-not $fullLog) {
        "Failed to read api_log.txt for $email`n" | Out-File $reportFile -Append -Encoding utf8
        return
    }

    $escapedEmail = [regex]::Escape($email)
    # Match To: <email> and then capture the href up to the closing quote
    if ($fullLog -match "(?s)To: ${escapedEmail}.*?href='([^']*)'") {
        $confirmationUrl = $matches[1] -replace "`r`n", "" -replace "`n", "" -replace "&amp;", "&"
        
        "Found confirmation URL for ${email}: ${confirmationUrl}`n" | Out-File $reportFile -Append -Encoding utf8
        
        if ($confirmationUrl -match "userId=(.*?)&token=(.*)") {
            $userId = $matches[1]
            $token = $matches[2]
            Invoke-Api -title "Confirm Email for $email" -method "GET" -endpoint "/api/auth/confirm-email?userId=$userId&token=$token" -body ""
        }
    } else {
        "Could not find confirmation URL for $email in log.`n" | Out-File $reportFile -Append -Encoding utf8
    }
}

Confirm-EmailFromLog -email $clientEmail
Confirm-EmailFromLog -email $lawyerEmail


# 4. Login Client
$loginClientBody = @{
    Email = $clientEmail
    Password = "Password123!"
} | ConvertTo-Json
$clientLoginRes = Invoke-Api -title "3. Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody
$clientToken = $clientLoginRes.Data.AccessToken

# 5. Login Lawyer
$loginLawyerBody = @{
    Email = $lawyerEmail
    Password = "Password123!"
} | ConvertTo-Json
$lawyerLoginRes = Invoke-Api -title "4. Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody
$lawyerToken = $lawyerLoginRes.Data.AccessToken

# CLIENT TESTS
# Test 5: Client Complete - Missing Phone Number
$body = @{ DateOfBirth = "1990-01-01"; Gender = 1; Address = "Test Address" } | ConvertTo-Json
Invoke-Api -title "5. Client Complete Profile - Missing Phone Number" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken

# Test 6: Client Complete - Invalid Phone Number Format
$body = @{ PhoneNumber = "01000000000"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Test Address"; NationalNumber = "29001014304533" } | ConvertTo-Json
Invoke-Api -title "6. Client Complete Profile - Invalid Phone Format (Needs +20)" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken

# Test 7: Client Complete - Future Date of Birth
$body = @{ PhoneNumber = "+201000000000"; DateOfBirth = "2050-01-01"; Gender = 1; Address = "Test Address"; NationalNumber = "29001014304533" } | ConvertTo-Json
Invoke-Api -title "7. Client Complete Profile - Future DOB" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken

# Test 8: Client Complete - Valid Data
$body = @{ PhoneNumber = "+201000000000"; DateOfBirth = "1990-01-01"; Gender = 1; Address = "Test Address"; NationalNumber = "29001014304533" } | ConvertTo-Json
Invoke-Api -title "8. Client Complete Profile - Valid Data" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken

# Test 9: Client Complete - Try Again (Should Fail)
Invoke-Api -title "9. Client Complete Profile - Try Again After Completion" -method "POST" -endpoint "/api/clients/profile/complete" -body $body -token $clientToken

# Re-Login Client because SecurityStamp changed when setting PhoneNumber
$clientLoginRes2 = Invoke-Api -title "9b. Re-Login Client" -method "POST" -endpoint "/api/auth/login" -body $loginClientBody
$clientToken = $clientLoginRes2.Data.AccessToken

# Test 10: Client Update - Valid Data (Phone and Address only)
$body = @{ PhoneNumber = "+201111111111"; Address = "Updated Address" } | ConvertTo-Json
Invoke-Api -title "10. Client Update Profile - Valid Data" -method "PUT" -endpoint "/api/clients/profile" -body $body -token $clientToken

# Test 11: Client Update - Invalid Phone Number
$body = @{ PhoneNumber = "123"; Address = "Updated Address" } | ConvertTo-Json
Invoke-Api -title "11. Client Update Profile - Invalid Phone Number" -method "PUT" -endpoint "/api/clients/profile" -body $body -token $clientToken

# LAWYER TESTS
# Test 12: Lawyer Complete - Missing National Number
$body = @{ PhoneNumber = "+201000000000"; DateOfBirth = "1990-01-01"; Gender = 1; Level = 1 } | ConvertTo-Json
Invoke-Api -title "12. Lawyer Complete Profile - Missing National Number" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken

# Test 13: Lawyer Complete - Invalid National Number length
$body = @{ PhoneNumber = "+201000000000"; NationalNumber = "123456"; DateOfBirth = "1990-01-01"; Gender = 1; Level = 1; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "13. Lawyer Complete Profile - Invalid National Number Length" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken

# Test 14: Lawyer Complete - Invalid Level (Not in Enum)
$body = @{ PhoneNumber = "+201000000000"; NationalNumber = "12345678901234"; DateOfBirth = "1990-01-01"; Gender = 1; Level = 999; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "14. Lawyer Complete Profile - Invalid Lawyer Level" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken

# Generate random 14-digit National Number
$nationalNumPrefix = "2900101"
$nationalNumSuffix = Get-Random -Minimum 1000000 -Maximum 9999999
$nationalNum = "$nationalNumPrefix$nationalNumSuffix"

# Test 15: Lawyer Complete - Valid Data
$body = @{ Bio = "Hello I am a lawyer"; Level = 1; DateOfBirth = "1990-01-01"; Gender = 1; PhoneNumber = "+201000000000"; NationalNumber = $nationalNum; Address = "Law Firm 1"; Specializations = @(@{ Specialization = 1; YearsOfExperience = 5; CasesHandled = 10 }) } | ConvertTo-Json
Invoke-Api -title "15. Lawyer Complete Profile - Valid Data" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken

# Test 16: Lawyer Complete - Try Again (Should Fail)
Invoke-Api -title "16. Lawyer Complete Profile - Try Again After Completion" -method "POST" -endpoint "/api/lawyers/profile/complete" -body $body -token $lawyerToken

# Re-Login Lawyer because SecurityStamp changed when setting PhoneNumber
$lawyerLoginRes2 = Invoke-Api -title "16b. Re-Login Lawyer" -method "POST" -endpoint "/api/auth/login" -body $loginLawyerBody
$lawyerToken = $lawyerLoginRes2.Data.AccessToken

# Test 17: Lawyer Update - Valid Data (Phone, Address, Bio, Level)
$body = @{ PhoneNumber = "+201111111111"; Level = 2; Bio = "Updated Bio"; Address = "Updated Address" } | ConvertTo-Json
Invoke-Api -title "17. Lawyer Update Profile - Valid Data" -method "PUT" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken

# Test 18: Lawyer Update - Invalid Bio Length (500+ chars)
$longBio = "A" * 501
$body = @{ PhoneNumber = "+201111111111"; Level = 2; Bio = $longBio; Address = "Updated Address" } | ConvertTo-Json
Invoke-Api -title "18. Lawyer Update Profile - Invalid Bio Length" -method "PUT" -endpoint "/api/lawyers/profile" -body $body -token $lawyerToken

# Test 19: Check GET Client Profile
Invoke-Api -title "19. Client GET Profile - Ensure fields are correct" -method "GET" -endpoint "/api/clients/profile" -body "" -token $clientToken

# Test 20: Check GET Lawyer Profile
Invoke-Api -title "20. Lawyer GET Profile - Ensure fields are correct" -method "GET" -endpoint "/api/lawyers/profile" -body "" -token $lawyerToken

Write-Host "Tests complete. Results saved to $reportFile"
