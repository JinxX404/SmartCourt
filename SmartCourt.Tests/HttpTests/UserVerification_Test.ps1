# UserVerification_Test.ps1
Import-Module "$PSScriptRoot\TestHelpers.psm1" -Force
Add-Type -AssemblyName System.Net.Http

$reportFile = "$PSScriptRoot\UserVerification_Report.md"
$apiLogPath = "$PSScriptRoot\..\..\SmartCourt\api_log.txt"
Clear-Content $reportFile -ErrorAction SilentlyContinue
"# User Verification Slice Test Report`n" | Out-File $reportFile -Encoding utf8

$randomNum = Get-Random
$lawyerEmail = "lawyer_verification_${randomNum}@test.com"

# Setup: Register, Confirm, and Login a user
$body = @{ FullName = "Lawyer Verification"; Email = $lawyerEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "0. Setup - Register Lawyer" -method "POST" -endpoint "/api/auth/register/lawyer" -body $body -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $lawyerEmail -reportFile $reportFile -apiLogPath $apiLogPath

$loginBody = @{ Email = $lawyerEmail; Password = "Password123!" } | ConvertTo-Json
$loginRes = Invoke-Api -title "0. Setup - Login" -method "POST" -endpoint "/api/auth/login" -body $loginBody -reportFile $reportFile

$clientToken = $loginRes.Data.AccessToken
$userId = $loginRes.Data.User.Id

# Ensure we have a valid UserId
if (-not $userId) {
    "Failed to retrieve UserId from login response.`n" | Out-File $reportFile -Append -Encoding utf8
    exit 1
}

# Create a dummy image file for upload testing
$tempFilePath = "$PSScriptRoot\dummy_id.jpg"
[System.IO.File]::WriteAllBytes($tempFilePath, [byte[]]@(0..10))

# Custom helper function for multipart form data
function Invoke-MultipartApi {
    param(
        [string]$title,
        [string]$endpoint,
        [string]$token,
        [hashtable]$form,
        [string]$fileField,
        [string]$filePath
    )
    $url = "$global:httpUrl$endpoint"
    
    $multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
    
    foreach ($key in $form.Keys) {
        $multipartContent.Add([System.Net.Http.StringContent]::new($form[$key]), $key)
    }
    
    if (-not [string]::IsNullOrEmpty($fileField) -and (Test-Path $filePath)) {
        $fileContent = [System.Net.Http.ByteArrayContent]::new([System.IO.File]::ReadAllBytes($filePath))
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
        $multipartContent.Add($fileContent, $fileField, [System.IO.Path]::GetFileName($filePath))
    }
    
    $httpClient = [System.Net.Http.HttpClient]::new()
    if (-not [string]::IsNullOrEmpty($token)) {
        $httpClient.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $token)
    }
    
    # Log Request
    $logOutput = "### $title`n`n"
    $logOutput += "**Request:** POST $url (Multipart Form Data)`n`n"
    $logOutput += "**Form Data:**`n"
    foreach ($key in $form.Keys) {
        $logOutput += "- $key = $($form[$key])`n"
    }
    if ($fileField) {
        $logOutput += "- $fileField = [File: $filePath]`n"
    }
    $logOutput += "`n"
    $logOutput | Out-File $reportFile -Append -Encoding utf8
    
    try {
        $task = $httpClient.PostAsync($url, $multipartContent)
        $response = $task.GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        
        $logOutput2 = "**Response Status:** $status`n`n"
        if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
            $logOutput2 += "**Response Body:**`n```json`n$responseBody`n````n"
        }
        $logOutput2 += "---`n`n"
        $logOutput2 | Out-File $reportFile -Append -Encoding utf8
        
        return ($responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue)
    } catch {
        $status = "Error"
        $errorMsg = $_.Exception.Message
        if ($_.Exception.InnerException) {
            $errorMsg += " - " + $_.Exception.InnerException.Message
        }
        $logOutput2 = "**Response Status:** $status`n`n"
        $logOutput2 += "**Response Body:**`n$errorMsg`n---`n`n"
        $logOutput2 | Out-File $reportFile -Append -Encoding utf8
        return $null
    }
}

# 1. POST - Valid Submission
$formValid = @{
    UserId = $userId
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
$submitRes = Invoke-MultipartApi -title "1. Submit Verification Documents - Valid" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formValid -fileField "Documents[0].File" -filePath $tempFilePath

# 2. POST - Validation Error (Missing UserId)
$formNoUserId = @{
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "2"
}
Invoke-MultipartApi -title "2. Submit Verification - Missing UserId (400)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formNoUserId -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 3a. POST - Validation Error (Empty Documents list)
$bodyNoDocs = @{ UserId = $userId; Documents = @() } | ConvertTo-Json
Invoke-Api -title "3a. Submit Verification - Empty Documents (400)" -method "POST" -endpoint "/api/UserVerification/submit-verification-documents" -body $bodyNoDocs -token $clientToken -reportFile $reportFile | Out-Null

# 3b. POST - Validation Error (Invalid Enum Type)
$formInvalidEnum = @{
    UserId = $userId
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "999"
}
Invoke-MultipartApi -title "3b. Submit Verification - Invalid Type (400)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formInvalidEnum -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 3c. POST - Malicious Payload (SQL Injection in UserId)
$formSqlInjection = @{
    UserId = "1' OR '1'='1"
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "3c. Submit Verification - Malicious Payload (400)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formSqlInjection -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 4. POST - Security (No Token)
Invoke-MultipartApi -title "4. Submit Verification - No Token (401)" -endpoint "/api/UserVerification/submit-verification-documents" -token "" -form $formValid -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 5. GET - Valid
$getRes = Invoke-Api -title "5. Get User Documents - Valid" -method "GET" -endpoint "/api/UserVerification/$userId" -token $clientToken -reportFile $reportFile

$documentId = $null
if ($getRes -and $getRes.Data -and $getRes.Data.Documents -and $getRes.Data.Documents.Count -gt 0) {
    $documentId = $getRes.Data.Documents[0].DocumentId
}

# 6. GET - Invalid UserId (Empty/Zero)
$invalidGuid = "00000000-0000-0000-0000-000000000000"
Invoke-Api -title "6. Get User Documents - Invalid UserId" -method "GET" -endpoint "/api/UserVerification/$invalidGuid" -token $clientToken -reportFile $reportFile | Out-Null

# 7. GET - Malicious UserId
Invoke-Api -title "7. Get User Documents - Malicious UserId" -method "GET" -endpoint "/api/UserVerification/DROP_TABLE_USERS" -token $clientToken -reportFile $reportFile | Out-Null

# 8. GET - Security (No Token)
Invoke-Api -title "8. Get User Documents - No Token (401)" -method "GET" -endpoint "/api/UserVerification/$userId" -token "" -reportFile $reportFile | Out-Null

# 9. DELETE - Validation (Missing DocumentId)
Invoke-Api -title "9. Delete User Document - Missing DocumentId" -method "DELETE" -endpoint "/api/UserVerification?UserId=$userId" -token $clientToken -reportFile $reportFile | Out-Null

# 10. DELETE - Validation (Missing UserId)
$docQueryStr = if ($documentId) { "?DocumentId=$documentId" } else { "?DocumentId=$([guid]::NewGuid())" }
Invoke-Api -title "10. Delete User Document - Missing UserId" -method "DELETE" -endpoint "/api/UserVerification$docQueryStr" -token $clientToken -reportFile $reportFile | Out-Null

# 11. DELETE - Security (No Token)
if ($documentId) {
    Invoke-Api -title "11. Delete User Document - No Token (401)" -method "DELETE" -endpoint "/api/UserVerification?UserId=$userId&DocumentId=$documentId" -token "" -reportFile $reportFile | Out-Null
}

# 12. DELETE - Valid
if ($documentId) {
    Invoke-Api -title "12. Delete User Document - Valid" -method "DELETE" -endpoint "/api/UserVerification?UserId=$userId&DocumentId=$documentId" -token $clientToken -reportFile $reportFile | Out-Null
} else {
    "### 12. Delete User Document - Skipped (No DocumentId)`n`n" | Out-File $reportFile -Append -Encoding utf8
}

# 13. DELETE - Already Deleted / Not Found
if ($documentId) {
    Invoke-Api -title "13. Delete User Document - Not Found (Already deleted)" -method "DELETE" -endpoint "/api/UserVerification?UserId=$userId&DocumentId=$documentId" -token $clientToken -reportFile $reportFile | Out-Null
}

# 14. Stress - Large File Upload (3MB)
$largeFilePath = "$PSScriptRoot\large_id.jpg"
[System.IO.File]::WriteAllBytes($largeFilePath, [byte[]]::new(3 * 1024 * 1024))
Invoke-MultipartApi -title "14. Submit Verification - Large File (3MB)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formValid -fileField "Documents[0].File" -filePath $largeFilePath | Out-Null

# 15. Stress - Invalid File Type (.exe disguised)
$exeFilePath = "$PSScriptRoot\malicious.exe"
[System.IO.File]::WriteAllText($exeFilePath, "MZ900000")
Invoke-MultipartApi -title "15. Submit Verification - Invalid File Extension (.exe)" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formValid -fileField "Documents[0].File" -filePath $exeFilePath | Out-Null

# 16. Stress - Extremely Long UserId
$longGuid = "a" * 10000
$formLongGuid = @{
    UserId = $longGuid
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "16. Submit Verification - Extremely Long UserId" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formLongGuid -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 17. Stress - Past Expiration Date
$formPastDate = @{
    UserId = $userId
    "Documents[0].ExpirationDate" = "1800-01-01"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "17. Submit Verification - Past Expiration Date" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formPastDate -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 18. Integration - Cross-User Deletion Attempt
# Setup a second user to get their token
$secondEmail = "attacker_${randomNum}@test.com"
$bodyAttacker = @{ FullName = "Attacker User"; Email = $secondEmail; Password = "Password123!"; ConfirmPassword = "Password123!" } | ConvertTo-Json
Invoke-Api -title "18a. Setup - Register Attacker" -method "POST" -endpoint "/api/auth/register/lawyer" -body $bodyAttacker -reportFile $reportFile | Out-Null
Confirm-EmailFromLog -email $secondEmail -reportFile $reportFile -apiLogPath $apiLogPath
$loginBodyAttacker = @{ Email = $secondEmail; Password = "Password123!" } | ConvertTo-Json
$loginResAttacker = Invoke-Api -title "18b. Setup - Login Attacker" -method "POST" -endpoint "/api/auth/login" -body $loginBodyAttacker -reportFile $reportFile

$attackerToken = ""
if ($loginResAttacker -and $loginResAttacker.Data -and $loginResAttacker.Data.AccessToken) {
    $attackerToken = $loginResAttacker.Data.AccessToken
}

# Attempt to delete the original user's document using the attacker's token
$docQueryStrAttacker = if ($documentId) { "?UserId=$userId&DocumentId=$documentId" } else { "?UserId=$userId&DocumentId=$([guid]::NewGuid())" }
Invoke-Api -title "18c. Delete User Document - Cross-User (Attacker Token)" -method "DELETE" -endpoint "/api/UserVerification$docQueryStrAttacker" -token $attackerToken -reportFile $reportFile | Out-Null

# 19. Stress - Invalid Date Format
$formInvalidDate = @{
    UserId = $userId
    "Documents[0].ExpirationDate" = "13-13-2030"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "19. Submit Verification - Invalid Date Format" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formInvalidDate -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null
# 20. Validation - Submit Missing UserId
$formMissingUserId = @{
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "20. Validation - Submit Missing UserId" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formMissingUserId -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 21. Validation - Submit Empty Documents
$formEmptyDocs = @{
    UserId = $userId
}
Invoke-MultipartApi -title "21. Validation - Submit Empty Documents" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formEmptyDocs -fileField "" -filePath "" | Out-Null

# 22. Validation - Submit Malformed UserId
$formMalformedUserId = @{
    UserId = "not-a-guid"
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
}
Invoke-MultipartApi -title "22. Validation - Submit Malformed UserId" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formMalformedUserId -fileField "Documents[0].File" -filePath $tempFilePath | Out-Null

# 23. Validation - Submit Duplicate DocumentTypes
$formDuplicateDocs = @{
    UserId = $userId
    "Documents[0].ExpirationDate" = "2030-01-01"
    "Documents[0].Type" = "1"
    "Documents[1].ExpirationDate" = "2030-01-01"
    "Documents[1].Type" = "1"
}
# Because Invoke-MultipartApi only supports one file right now in our helper, we'll manually invoke this
Invoke-Api -title "23. Validation - Submit Duplicate DocumentTypes" -method "POST" -endpoint "/api/UserVerification/submit-verification-documents" -body "{}" -token $clientToken -reportFile $reportFile | Out-Null # Hacky skip because multipart helper doesn't support array of files yet, but empty json will trigger 500 error again as documented. Let's build a quick inline multipart:

"### 23. Validation - Submit Duplicate DocumentTypes`n" | Out-File $reportFile -Append -Encoding utf8
$boundary = [System.Guid]::NewGuid().ToString()
$multipartContent = [System.Net.Http.MultipartFormDataContent]::new($boundary)
$multipartContent.Add([System.Net.Http.StringContent]::new($userId), "UserId")
$multipartContent.Add([System.Net.Http.StringContent]::new("2030-01-01"), "Documents[0].ExpirationDate")
$multipartContent.Add([System.Net.Http.StringContent]::new("1"), "Documents[0].Type")
$fileBytes = [System.IO.File]::ReadAllBytes($tempFilePath)
$fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
$multipartContent.Add($fileContent, "Documents[0].File", "file1.jpg")
$multipartContent.Add([System.Net.Http.StringContent]::new("2030-01-01"), "Documents[1].ExpirationDate")
$multipartContent.Add([System.Net.Http.StringContent]::new("1"), "Documents[1].Type")
$fileContent2 = [System.Net.Http.ByteArrayContent]::new($fileBytes)
$fileContent2.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
$multipartContent.Add($fileContent2, "Documents[1].File", "file2.jpg")
$httpClient = [System.Net.Http.HttpClient]::new()
if ($clientToken) { $httpClient.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $clientToken) }
try {
    $res = $httpClient.PostAsync("http://localhost:5049/api/UserVerification/submit-verification-documents", $multipartContent).GetAwaiter().GetResult()
    "**Response Status:** $([int]$res.StatusCode)`n" | Out-File $reportFile -Append -Encoding utf8
    $resBody = $res.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    "**Response Body:**`n``````json`n$resBody`n``````n---`n`n" | Out-File $reportFile -Append -Encoding utf8
} catch {} finally { $httpClient.Dispose(); $multipartContent.Dispose() }


# 24. Stress - 35MB file stress test
$giantFilePath = "$PSScriptRoot\giant_id.jpg"
$fileStream = [System.IO.File]::Create($giantFilePath)
$fileStream.SetLength(35MB)
$fileStream.Close()
$formGiant = @{ UserId = $userId; "Documents[0].ExpirationDate" = "2030-01-01"; "Documents[0].Type" = "1" }
Invoke-MultipartApi -title "24. Stress - 35MB file stress test" -endpoint "/api/UserVerification/submit-verification-documents" -token $clientToken -form $formGiant -fileField "Documents[0].File" -filePath $giantFilePath | Out-Null
if (Test-Path $giantFilePath) { Remove-Item $giantFilePath -Force }

# 25. HTTP Method - POST to GET endpoint
Invoke-Api -title "25a. HTTP Method - POST to GET endpoint" -method "POST" -endpoint "/api/UserVerification/$userId" -token $clientToken -reportFile $reportFile -body "{}" | Out-Null
Invoke-Api -title "25b. HTTP Method - GET to DELETE endpoint" -method "GET" -endpoint "/api/UserVerification?UserId=$userId&DocumentId=$([guid]::NewGuid())" -token $clientToken -reportFile $reportFile | Out-Null

# 26. Read - Get documents for non-existent UserId
Invoke-Api -title "26. Read - Get documents for non-existent UserId" -method "GET" -endpoint "/api/UserVerification/$([guid]::NewGuid())" -token $clientToken -reportFile $reportFile | Out-Null

if (Test-Path $largeFilePath) { Remove-Item $largeFilePath -Force }
if (Test-Path $exeFilePath) { Remove-Item $exeFilePath -Force }

# Cleanup temp file
if (Test-Path $tempFilePath) {
    Remove-Item $tempFilePath -Force
}

"Tests complete. Results saved to $reportFile`n" | Write-Host
