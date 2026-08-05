# TestHelpers.psm1
$global:baseUrl = "https://localhost:7119"
$global:httpUrl = "http://localhost:5049"

function Log-Test {
    param([string]$title, [string]$method, [string]$url, [string]$body, [string]$responseStatus, [string]$responseBody, [string]$reportFile)
    
    $logOutput = "### $title`n`n"
    $logOutput += "**Request:** $method $url`n`n"
    
    if (-not [string]::IsNullOrWhiteSpace($body)) {
        # Format JSON body if possible
        try {
            $formattedBody = $body | ConvertFrom-Json | ConvertTo-Json -Depth 10
            $logOutput += "**Body:**`n```json`n$formattedBody`n````n`n"
        } catch {
            $logOutput += "**Body:**`n$body`n`n"
        }
    }
    
    $logOutput += "**Response Status:** $responseStatus`n`n"
    
    if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
        $logOutput += "**Response Body:**`n"
        try {
            $formattedResponse = $responseBody | ConvertFrom-Json -ErrorAction Stop | ConvertTo-Json -Depth 10 -Compress:$false
            $logOutput += "```json`n$formattedResponse`n````n"
        } catch {
            $logOutput += "$responseBody`n"
        }
    } else {
        $logOutput += "**Response Body:** (Empty)`n"
    }
    
    $logOutput += "---`n`n"
    $logOutput | Out-File $reportFile -Append -Encoding utf8
}

function Invoke-Api {
    param([string]$title, [string]$method, [string]$endpoint, [string]$body = "", [string]$token = "", [string]$reportFile)
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    $url = "$global:httpUrl$endpoint"
    try {
        if ([string]::IsNullOrWhiteSpace($body) -or $method -eq "GET" -or $method -eq "DELETE") {
            # DELETE might have body, but let's check
            if ($method -eq "DELETE" -and -not [string]::IsNullOrWhiteSpace($body)) {
                $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -Body $body -UseBasicParsing
            } else {
                $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -UseBasicParsing
            }
        } else {
            $response = Invoke-WebRequest -Method $method -Uri $url -Headers $headers -Body $body -UseBasicParsing
        }
        $status = $response.StatusCode
        $responseBody = $response.Content
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $responseBody -reportFile $reportFile
        
        return ($responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue)
    } catch {
        $status = "Error"
        if ($_.Exception.Response.StatusCode) {
            $status = [int]$_.Exception.Response.StatusCode
        } elseif ($_.Exception.Response.StatusCode.value__) {
            $status = $_.Exception.Response.StatusCode.value__
        }
        
        $errorResponse = ""
        if ($_.ErrorDetails -and -not [string]::IsNullOrEmpty($_.ErrorDetails.Message)) {
            $errorResponse = $_.ErrorDetails.Message
        } elseif ($_.Exception.Response -and -not ($_.Exception.Response -is [System.Net.Http.HttpResponseMessage])) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $errorResponse = $reader.ReadToEnd()
            } catch {
                $errorResponse = $_.Exception.Message
            }
        } else {
            $errorResponse = $_.Exception.Message
        }
        
        Log-Test -title $title -method $method -url $url -body $body -responseStatus $status -responseBody $errorResponse -reportFile $reportFile
        
        return ($errorResponse | ConvertFrom-Json -ErrorAction SilentlyContinue)
    }
}

function Confirm-EmailFromLog {
    param([string]$email, [string]$reportFile, [string]$apiLogPath)
    
    Start-Sleep -Seconds 3 # Wait for hangfire job to log the email
    
    $fullLog = Get-Content $apiLogPath -Raw -ErrorAction SilentlyContinue
    if (-not $fullLog) {
        "Failed to read api_log.txt for $email`n" | Out-File $reportFile -Append -Encoding utf8
        return
    }

    $escapedEmail = [regex]::Escape($email)
    if ($fullLog -match "(?s)To: ${escapedEmail}.*?href='([^']*)'") {
        $confirmationUrl = $matches[1] -replace "`r`n", "" -replace "`n", "" -replace "&amp;", "&"
        
        "Found confirmation URL for ${email}: ${confirmationUrl}`n" | Out-File $reportFile -Append -Encoding utf8
        
        if ($confirmationUrl -match "userId=(.*?)&token=(.*)") {
            $userId = $matches[1]
            $token = $matches[2]
            Invoke-Api -title "Confirm Email for $email" -method "GET" -endpoint "/api/auth/confirm-email?userId=$userId&token=$token" -body "" -reportFile $reportFile | Out-Null
        }
    } else {
        "Could not find confirmation URL for $email in log.`n" | Out-File $reportFile -Append -Encoding utf8
    }
}

function Get-ResetTokenFromLog {
    param([string]$email, [string]$apiLogPath)
    
    Start-Sleep -Seconds 3
    $fullLog = Get-Content $apiLogPath -Raw -ErrorAction SilentlyContinue
    if (-not $fullLog) { return "" }

    $escapedEmail = [regex]::Escape($email)
    # The reset token email will have href='.../auth/reset-password?email=X&token=Y'
    if ($fullLog -match "(?s)To: ${escapedEmail}.*?href='([^']*/auth/reset-password[^']*)'") {
        $resetUrl = $matches[1] -replace "`r`n", "" -replace "`n", "" -replace "&amp;", "&"
        if ($resetUrl -match "token=(.*)") {
            return $matches[1]
        }
    }
    return ""
}

Export-ModuleMember -Function Log-Test, Invoke-Api, Confirm-EmailFromLog, Get-ResetTokenFromLog
