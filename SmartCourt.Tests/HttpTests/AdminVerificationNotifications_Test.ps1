param(
    [Alias("BaseUrl")]
    [string]$ApiBaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [switch]$UseExistingApi,
    [string]$SuperAdministratorEmail = "",
    [string]$SuperAdministratorPassword = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..\..")).Path
$apiProjectDir = Join-Path $repoRoot "SmartCourt"
$apiDllPath = Join-Path $apiProjectDir "bin\Debug\net8.0\SmartCourt.dll"
$script:localStoragePath = Join-Path `
    $apiProjectDir `
    "uploads\gate5-$([guid]::NewGuid().ToString('N'))"

if ([string]::IsNullOrWhiteSpace($ApiLogPath)) {
    $ApiLogPath = Join-Path $apiProjectDir "api_log.txt"
}
if ([string]::IsNullOrWhiteSpace($ReportFile)) {
    $ReportFile = Join-Path $scriptDir "AdminVerificationNotifications_Report.md"
}

Import-Module (Join-Path $scriptDir "TestHelpers.psm1") -Force
Add-Type -AssemblyName System.Net.Http

$script:reportFile = $ReportFile
$script:passed = 0
$script:failed = 0
$script:skipped = 0
$script:fatal = [System.Collections.Generic.List[string]]::new()
$script:apiProcess = $null
$script:ownsApi = $false
$script:apiLogStartLength = 0
$script:apiStdOut = Join-Path ([System.IO.Path]::GetTempPath()) "smartcourt-gate5-api-$([guid]::NewGuid().ToString('N')).out.log"
$script:apiStdErr = Join-Path ([System.IO.Path]::GetTempPath()) "smartcourt-gate5-api-$([guid]::NewGuid().ToString('N')).err.log"
$script:confirmedEmails = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)

"# Administrative Verification Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
    Out-File $script:reportFile -Encoding utf8

function Add-Report {
    param([string]$Text)
    $Text | Out-File $script:reportFile -Append -Encoding utf8
}

function Write-Section {
    param([string]$Title)
    Add-Report "`n## $Title`n"
}

function Protect-ReportText {
    param([AllowNull()][string]$Text)
    if ($null -eq $Text) { return "" }

    $protected = $Text
    $protected = [regex]::Replace(
        $protected,
        '(?i)(accessToken|refreshToken|confirmationToken|resetToken|token|password|secret|apiKey)=([^&\s]+)',
        '$1=[REDACTED]')
    $protected = [regex]::Replace(
        $protected,
        '(?i)([a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,})',
        '[REDACTED_EMAIL]')
    $protected = [regex]::Replace(
        $protected,
        '(?<![0-9])[0-9]{10,14}(?![0-9])',
        '[REDACTED_NUMBER]')
    return $protected
}

function Test-SensitiveKey {
    param([string]$Key)
    return $Key -match '(?i)(password|token|secret|api.?key|email|phone|national|rejection.?reason|file.?url|storage.?path|download.?url|document.?content|content.?url|file.?name|provider.?id|idempotency.?key)'
}

function Redact-JsonValue {
    param(
        $Value,
        [string]$Key = ""
    )

    if (Test-SensitiveKey $Key) { return "[REDACTED]" }
    if ($null -eq $Value) { return $null }
    if ($Value -is [string]) { return Protect-ReportText $Value }

    if ($Value -is [System.Collections.IDictionary]) {
        $result = [ordered]@{}
        foreach ($entry in $Value.GetEnumerator()) {
            $result[$entry.Key] = Redact-JsonValue $entry.Value ([string]$entry.Key)
        }
        return $result
    }

    if ($Value -is [pscustomobject]) {
        $result = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) {
            $result[$property.Name] = Redact-JsonValue $property.Value $property.Name
        }
        return $result
    }

    if ($Value -is [System.Collections.IEnumerable]) {
        $items = @($Value | ForEach-Object { Redact-JsonValue $_ })
        return ,$items
    }

    return $Value
}

function Redact-JsonText {
    param([AllowNull()][string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return "" }
    try {
        $json = $Text | ConvertFrom-Json -ErrorAction Stop
        return (Redact-JsonValue $json | ConvertTo-Json -Depth 100)
    }
    catch {
        return Protect-ReportText $Text
    }
}

function Assert-Test {
    param(
        [string]$Name,
        [bool]$Condition,
        [string]$Details = ""
    )

    if ($Condition) {
        [void]($script:passed++)
        $suffix = if ([string]::IsNullOrWhiteSpace($Details)) { "" } else { " ($Details)" }
        Add-Report "- [PASS] **$(Protect-ReportText $Name)**$suffix"
    }
    else {
        [void]($script:failed++)
        $suffix = if ([string]::IsNullOrWhiteSpace($Details)) { "" } else { " ($Details)" }
        Add-Report "- [FAIL] **$(Protect-ReportText $Name)**$suffix"
    }
}

function Add-Skip {
    param([string]$Name, [string]$Reason)
    [void]($script:skipped++)
    Add-Report "- [SKIP] **$(Protect-ReportText $Name)** — $(Protect-ReportText $Reason)"
}

function Parse-Json {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return $null }
    try { return $Text | ConvertFrom-Json -ErrorAction Stop }
    catch { return $null }
}

function Write-HttpExchange {
    param(
        [string]$Title,
        [string]$Method,
        [string]$Url,
        [string]$Body,
        [int]$Status,
        [string]$ResponseBody
    )

    $safeBody = Redact-JsonText $Body
    $safeResponse = Redact-JsonText $ResponseBody
    Add-Report "### $(Protect-ReportText $Title)`n"
    Add-Report "**Request:** $Method $(Protect-ReportText $Url)`n"
    if (-not [string]::IsNullOrWhiteSpace($safeBody)) {
        Add-Report "**Body:**`n``````json`n$safeBody`n``````n"
    }
    Add-Report "**Response Status:** $Status`n"
    if ([string]::IsNullOrWhiteSpace($safeResponse)) {
        Add-Report "**Response Body:** (Empty)`n---`n"
    }
    else {
        Add-Report "**Response Body:**`n``````json`n$safeResponse`n``````n---`n"
    }
}

function Invoke-TestRequest {
    param(
        [string]$Title,
        [string]$Method,
        [string]$Path,
        [string]$Token = "",
        [string]$Body = "",
        [hashtable]$ExtraHeaders = @{},
        [switch]$NoReport
    )

    $url = if ($Path -match '^https?://') { $Path } else { "$($ApiBaseUrl.TrimEnd('/'))$Path" }
    $client = [System.Net.Http.HttpClient]::new()
    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::new($Method.ToUpperInvariant()),
        $url)
    try {
        if (-not [string]::IsNullOrWhiteSpace($Token)) {
            $request.Headers.Authorization =
                [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
        }
        foreach ($header in $ExtraHeaders.GetEnumerator()) {
            [void]$request.Headers.TryAddWithoutValidation(
                [string]$header.Key,
                [string]$header.Value)
        }

        if (-not [string]::IsNullOrWhiteSpace($Body)) {
            $request.Content = [System.Net.Http.StringContent]::new(
                $Body,
                [System.Text.Encoding]::UTF8,
                "application/json")
        }

        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        if (-not $NoReport) {
            Write-HttpExchange $Title $Method $url $Body $status $responseBody
        }
        return [pscustomobject]@{
            Status = $status
            Json = Parse-Json $responseBody
            Raw = $responseBody
        }
    }
    catch {
        if (-not $NoReport) {
            Write-HttpExchange $Title $Method $url $Body 0 $_.Exception.Message
        }
        return [pscustomobject]@{
            Status = 0
            Json = $null
            Raw = $_.Exception.Message
        }
    }
    finally {
        if ($request.Content) { $request.Content.Dispose() }
        $request.Dispose()
        $client.Dispose()
    }
}

function Invoke-MultipartRequest {
    param(
        [string]$Title,
        [string]$Path,
        [string]$Token,
        [hashtable]$Fields,
        [string]$FilePath,
        [string]$FileField = "Documents[0].File",
        [string]$FileName = "gate5-verification.jpg",
        [switch]$NoReport
    )

    $url = "$($ApiBaseUrl.TrimEnd('/'))$Path"
    $client = [System.Net.Http.HttpClient]::new()
    $content = [System.Net.Http.MultipartFormDataContent]::new()
    try {
        foreach ($field in $Fields.GetEnumerator()) {
            $content.Add(
                [System.Net.Http.StringContent]::new([string]$field.Value),
                [string]$field.Key)
        }

        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        $fileContent = [System.Net.Http.ByteArrayContent]::new($bytes)
        $fileContent.Headers.ContentType =
            [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
        $content.Add($fileContent, $FileField, $FileName)

        if (-not [string]::IsNullOrWhiteSpace($Token)) {
            $client.DefaultRequestHeaders.Authorization =
                [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
        }
        $response = $client.PostAsync($url, $content).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        if (-not $NoReport) {
            $safeFields = [ordered]@{}
            foreach ($field in $Fields.GetEnumerator()) {
                $safeFields[$field.Key] = $field.Value
            }
            $safeFields[$FileField] = "[REDACTED_FILE]"
            Write-HttpExchange `
                $Title `
                "POST" `
                $url `
                ($safeFields | ConvertTo-Json -Depth 10) `
                $status `
                $responseBody
        }
        return [pscustomobject]@{
            Status = $status
            Json = Parse-Json $responseBody
            Raw = $responseBody
        }
    }
    catch {
        if (-not $NoReport) {
            Write-HttpExchange $Title "POST" $url "[REDACTED_MULTIPART]" 0 $_.Exception.Message
        }
        return [pscustomobject]@{
            Status = 0
            Json = $null
            Raw = $_.Exception.Message
        }
    }
    finally {
        $content.Dispose()
        $client.Dispose()
    }
}

function Test-ApiHealth {
    $probe = Invoke-TestRequest "API health probe" GET "/health" -NoReport
    return $probe.Status -eq 200
}

function Set-TestApiEnvironment {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ConnectionStrings__DefaultConnection = `
        "Data Source=.;Initial Catalog=SmartCourt_Dev;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;"
    $env:FileStorage__Provider = "Local"
    $env:FileStorage__BasePath = $script:localStoragePath
    $env:Supabase__Url = $null
    $env:Supabase__ApiKey = $null
    $env:Supabase__Bucket = $null
    $env:SmtpSettings__Server = ""
}

function Start-ApiIfNeeded {
    if (Test-ApiHealth) {
        if (-not $UseExistingApi) {
            throw "An API is already listening on the requested test URL. Stop it or rerun with -UseExistingApi to make external-process ownership explicit."
        }
        Add-Skip "API lifecycle" "An API was already listening on the requested test URL; the script will not terminate an external process."
        return
    }

    if (-not (Test-Path $apiDllPath)) {
        throw "Built API assembly was not found at the expected project path."
    }

    $environmentNames = @(
        "ASPNETCORE_ENVIRONMENT",
        "ConnectionStrings__DefaultConnection",
        "FileStorage__Provider",
        "FileStorage__BasePath",
        "Supabase__Url",
        "Supabase__ApiKey",
        "Supabase__Bucket",
        "SmtpSettings__Server"
    )
    $oldEnvironment = @{}
    foreach ($name in $environmentNames) {
        $oldEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }

    try {
        Set-TestApiEnvironment
        $apiArguments = '"' + $apiDllPath + '" --urls ' + $ApiBaseUrl
        $script:apiProcess = Start-Process `
            -FilePath "dotnet" `
            -ArgumentList $apiArguments `
            -WorkingDirectory $apiProjectDir `
            -RedirectStandardOutput $script:apiStdOut `
            -RedirectStandardError $script:apiStdErr `
            -WindowStyle Hidden `
            -PassThru
        $script:ownsApi = $true
    }
    finally {
        foreach ($name in $environmentNames) {
            [Environment]::SetEnvironmentVariable($name, $oldEnvironment[$name])
        }
    }

    $deadline = (Get-Date).AddSeconds(90)
    do {
        if (Test-ApiHealth) { return }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)

    throw "The API did not become healthy before the startup deadline."
}

function Stop-ApiIfOwned {
    if (-not $script:ownsApi -or $null -eq $script:apiProcess) { return }
    if (-not $script:apiProcess.HasExited) {
        Stop-Process -Id $script:apiProcess.Id -Force -ErrorAction SilentlyContinue
        $script:apiProcess.WaitForExit(15000)
    }
}

function Test-PortReleased {
    try {
        $uri = [Uri]$ApiBaseUrl
        $port = $uri.Port
        $tcp = [System.Net.Sockets.TcpClient]::new()
        try {
            $task = $tcp.ConnectAsync($uri.Host, $port)
            if ($task.Wait(750) -and $tcp.Connected) { return $false }
            return $true
        }
        finally { $tcp.Dispose() }
    }
    catch { return $true }
}

function Extract-EmailConfirmationUrl {
    param([string]$Email)
    $escaped = [regex]::Escape($Email)
    $deadline = (Get-Date).AddSeconds(30)
    do {
        $fullLog = Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue
        $pattern = '(?is)To:\s*' + $escaped + '.*?href=[''\"]([^''\"]*/verify-email[^''\"]*)[''\"]'
        $match = if ($fullLog) {
            [regex]::Match($fullLog, $pattern)
        }
        else {
            $null
        }
        if ($null -ne $match -and $match.Success) {
            return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    return ""
}

function Confirm-DisposableEmail {
    param(
        [string]$Email,
        [string]$UserId,
        [string]$Label
    )

    $confirmationUrl = Extract-EmailConfirmationUrl $Email
    if ([string]::IsNullOrWhiteSpace($confirmationUrl)) {
        Assert-Test "$Label confirmation link was found in mock Email log" $false
        throw "No confirmation link was found for the disposable account."
    }

    $uri = [Uri]$confirmationUrl
    $values = @{}
    foreach ($part in $uri.Query.TrimStart('?').Split('&')) {
        if ([string]::IsNullOrWhiteSpace($part)) { continue }
        $separator = $part.IndexOf('=')
        if ($separator -lt 0) { continue }
        $key = [Uri]::UnescapeDataString($part.Substring(0, $separator))
        $value = [Uri]::UnescapeDataString($part.Substring($separator + 1))
        $values[$key] = $value
    }
    $actualUserId = if ($values.ContainsKey("userId")) { $values["userId"] } else { $UserId }
    $token = if ($values.ContainsKey("token")) {
        [string]$values["token"]
    }
    else {
        ""
    }
    if ([string]::IsNullOrWhiteSpace($actualUserId) -or
        [string]::IsNullOrWhiteSpace($token)) {
        Assert-Test "$Label confirmation link contains required query values" $false
        throw "The mock Email confirmation link did not contain userId and token."
    }

    $endpoint = "/api/auth/confirm-email?userId=$actualUserId&token=$([Uri]::EscapeDataString($token))"
    $response = Invoke-TestRequest "$Label confirm Email from mock log" GET $endpoint
    Assert-Test "$Label Email confirmation succeeds" ($response.Status -eq 200) "status=$($response.Status)"
    if ($response.Status -ne 200) { throw "Disposable Email confirmation failed." }
    [void]$script:confirmedEmails.Add($Email)
}

function New-DisposableUser {
    param(
        [string]$Kind,
        [string]$Label,
        [string]$AdminToken
    )

    $suffix = [guid]::NewGuid().ToString('N').Substring(0, 12)
    $email = "gate5_$($Kind.ToLowerInvariant())_$suffix@example.com"
    $password = "Gate5-$suffix!Aa9"
    $endpoint = if ($Kind -eq "Lawyer") {
        "/api/auth/register/lawyer"
    }
    else {
        "/api/auth/register/client"
    }
    $body = @{
        Email = $email
        FullName = "Gate 5 $Label"
        Password = $password
        ConfirmPassword = $password
    } | ConvertTo-Json
    $registered = Invoke-TestRequest "Setup $Label registration" POST $endpoint -Body $body
    Assert-Test "$Label registration uses Created response" ($registered.Status -eq 201) "status=$($registered.Status)"
    $userId = [string]$registered.Json.data.userId
    if ($registered.Status -ne 201 -or [string]::IsNullOrWhiteSpace($userId)) {
        throw "Could not create the disposable $Kind account through the registration endpoint."
    }

    Confirm-DisposableEmail $email $userId $Label
    $loginBody = @{ Email = $email; Password = $password } | ConvertTo-Json
    $login = Invoke-TestRequest "Setup $Label login" POST "/api/auth/login" -Body $loginBody
    Assert-Test "$Label login succeeds after Email confirmation" ($login.Status -eq 200) "status=$($login.Status)"
    $accessToken = [string]$login.Json.data.accessToken
    if ($login.Status -ne 200 -or [string]::IsNullOrWhiteSpace($accessToken)) {
        throw "Could not log in the disposable $Kind account."
    }

    return [pscustomobject]@{
        Id = $userId
        Email = $email
        Password = $password
        Token = $accessToken
    }
}

function New-VerificationFile {
    $path = Join-Path ([System.IO.Path]::GetTempPath()) "smartcourt-gate5-$([guid]::NewGuid().ToString('N')).jpg"
    [System.IO.File]::WriteAllBytes($path, [byte[]](0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0xFF, 0xD9))
    return $path
}

function Submit-VerificationDocument {
    param(
        [string]$Label,
        $User,
        [int]$DocumentType,
        [string]$ExpirationDate,
        [string]$FilePath,
        [switch]$RequirePersistedDocument
    )
    $fields = @{
        UserId = $User.Id
        "Documents[0].ExpirationDate" = $ExpirationDate
        "Documents[0].Type" = [string]$DocumentType
    }
    $response = Invoke-MultipartRequest `
        "$Label submit verification document" `
        "/api/UserVerification/submit-verification-documents" `
        $User.Token `
        $fields `
        $FilePath
    Assert-Test "$Label document upload succeeds" ($response.Status -eq 200) "status=$($response.Status)"
    if ($response.Status -ne 200) { throw "The $Label verification upload failed." }
    if ($RequirePersistedDocument) {
        $uploadedDocuments = @($response.Json.data.uploadedDocuments)
        Assert-Test `
            "$Label response contains a persisted uploaded document" `
            ($uploadedDocuments.Count -eq 1) `
            "uploaded=$($uploadedDocuments.Count)"
        if ($uploadedDocuments.Count -ne 1) {
            throw "The $Label upload returned no persisted document."
        }
    }
    return $response
}

function Get-AdminDetails {
    param([string]$Label, [string]$UserId, [string]$AdminToken)
    return Invoke-TestRequest "$Label admin verification details" GET "/api/admin/verifications/$UserId" -Token $AdminToken
}

function Find-AdminDocument {
    param($DetailsResponse, [string]$DocumentType)
    return @($DetailsResponse.Json.data.documents |
        Where-Object { $_.documentType -eq $DocumentType } |
        Select-Object -First 1)
}

function Require-AdminDocument {
    param($DetailsResponse, [string]$DocumentType, [string]$Label)
    $documents = @(Find-AdminDocument $DetailsResponse $DocumentType)
    if ($documents.Count -ne 1 -or [string]::IsNullOrWhiteSpace([string]$documents[0].documentId)) {
        throw "The $Label admin details response did not contain exactly one current $DocumentType document."
    }
    return $documents[0]
}

function Age-LocalVerificationDocument {
    param([string]$DocumentId)

    $parsedId = [guid]::Empty
    if (-not [guid]::TryParse($DocumentId, [ref]$parsedId)) {
        throw "Cannot age a verification document with an invalid ID."
    }

    $sql = "SET NOCOUNT ON; UPDATE UserVerificationDocuments SET ExpirationDate = DATEADD(day, -1, CONVERT(date, GETUTCDATE())) WHERE Id = '$parsedId'; IF @@ROWCOUNT <> 1 THROW 51000, 'The disposable verification fixture was not found.', 1;"
    $output = & sqlcmd -S . -d SmartCourt_Dev -E -C -b -Q $sql 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "The local SQL database could not age the disposable verification fixture."
    }
    Add-Report "- [PASS] **Expired fixture prepared in the local disposable database**"
}

function Find-Notification {
    param(
        [string]$Token,
        [string]$Type,
        [hashtable]$ExpectedData,
        [int]$TimeoutSeconds = 75
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $page = Invoke-TestRequest "Notification poll for $Type" GET "/api/notifications?pageSize=50" -Token $Token -NoReport
        if ($page.Status -eq 200 -and $null -ne $page.Json) {
            foreach ($item in @($page.Json.data.items)) {
                if ($item.type -ne $Type) { continue }
                $matches = $true
                foreach ($key in $ExpectedData.Keys) {
                    $property = $item.data.PSObject.Properties[$key]
                    if ($null -eq $property -or [string]$property.Value -ne [string]$ExpectedData[$key]) {
                        $matches = $false
                        break
                    }
                }
                if ($matches) { return $item }
            }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Get-NotificationItems {
    param([string]$Token, [string]$Label = "Get notifications")
    $response = Invoke-TestRequest $Label GET "/api/notifications?pageSize=50" -Token $Token
    Assert-Test "$Label returns 200" ($response.Status -eq 200) "status=$($response.Status)"
    if ($response.Status -ne 200) { return @() }
    return @($response.Json.data.items)
}

function Get-NotificationCountFor {
    param(
        [string]$Token,
        [string]$Type,
        [hashtable]$ExpectedData
    )
    $items = @(Get-NotificationItems $Token "Count notifications for $Type")
    return @($items | Where-Object {
        if ($_.type -ne $Type) { return $false }
        foreach ($key in $ExpectedData.Keys) {
            $property = $_.data.PSObject.Properties[$key]
            if ($null -eq $property -or [string]$property.Value -ne [string]$ExpectedData[$key]) {
                return $false
            }
        }
        return $true
    }).Count
}

function Assert-ExactNotification {
    param(
        [string]$Label,
        $Notification,
        [string]$Type,
        [string]$Severity,
        [string]$Title,
        [string]$Body,
        [hashtable]$ExpectedData
    )

    $valid = $null -ne $Notification -and
        $Notification.type -eq $Type -and
        $Notification.severity -eq $Severity -and
        $Notification.title -eq $Title -and
        $Notification.body -eq $Body -and
        $null -eq $Notification.actionUrl
    if ($valid) {
        foreach ($key in $ExpectedData.Keys) {
            $property = $Notification.data.PSObject.Properties[$key]
            if ($null -eq $property -or [string]$property.Value -ne [string]$ExpectedData[$key]) {
                $valid = $false
                break
            }
        }
    }
    $dataKeys = if ($null -eq $Notification -or $null -eq $Notification.data) {
        @()
    } else { @($Notification.data.PSObject.Properties.Name) }
    $forbidden = @(
        "storagePath", "fileUrl", "downloadUrl", "content", "contentUrl",
        "documentContent", "rejectionReason", "privateReviewComment", "reason",
        "email", "phone", "phoneNumber", "nationalNumber", "accessToken",
        "refreshToken", "providerId", "idempotencyKey", "token")
    $hasForbidden = @($dataKeys | Where-Object { $_ -in $forbidden }).Count -gt 0
    Assert-Test $Label ($valid -and -not $hasForbidden)
}

function Invoke-ConcurrentReview {
    param(
        [string]$DocumentId,
        [string]$AdminToken,
        [string]$FirstBody,
        [string]$SecondBody
    )

    if ([string]::IsNullOrWhiteSpace($DocumentId)) {
        throw "Concurrent review requires a persisted document ID."
    }

    $clients = @([System.Net.Http.HttpClient]::new(), [System.Net.Http.HttpClient]::new())
    $requests = @()
    $url = "$($ApiBaseUrl.TrimEnd('/'))/api/admin/verifications/documents/$DocumentId"
    try {
        foreach ($client in $clients) {
            $client.DefaultRequestHeaders.Authorization =
                [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $AdminToken)
        }
        $bodies = @($FirstBody, $SecondBody)
        foreach ($body in $bodies) {
            $request = [System.Net.Http.HttpRequestMessage]::new(
                [System.Net.Http.HttpMethod]::new("PATCH"),
                $url)
            $request.Content = [System.Net.Http.StringContent]::new(
                $body,
                [System.Text.Encoding]::UTF8,
                "application/json")
            $requests += $request
        }
        $tasks = [System.Threading.Tasks.Task[]]@(
            $clients[0].SendAsync($requests[0]),
            $clients[1].SendAsync($requests[1]))
        [System.Threading.Tasks.Task]::WaitAll($tasks)
        $responses = @($tasks | ForEach-Object { $_.GetAwaiter().GetResult() })
        $results = @()
        for ($index = 0; $index -lt $responses.Length; $index++) {
            $body = $responses[$index].Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $status = [int]$responses[$index].StatusCode
            Write-HttpExchange "Concurrent review request $($index + 1)" "PATCH" $url $bodies[$index] $status $body
            $results += [pscustomobject]@{ Status = $status; Json = Parse-Json $body; Raw = $body }
        }
        return $results
    }
    finally {
        foreach ($request in $requests) {
            if ($request.Content) { $request.Content.Dispose() }
            $request.Dispose()
        }
        foreach ($client in $clients) { $client.Dispose() }
    }
}

function Get-LogTextSinceStart {
    $contents = @()
    if (Test-Path $ApiLogPath) {
        $apiLog = Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue
        if ($apiLog.Length -ge $script:apiLogStartLength) {
            $contents += $apiLog.Substring($script:apiLogStartLength)
        }
        else {
            $contents += $apiLog
        }
    }
    if (Test-Path $script:apiStdOut) { $contents += Get-Content $script:apiStdOut -Raw -ErrorAction SilentlyContinue }
    if (Test-Path $script:apiStdErr) { $contents += Get-Content $script:apiStdErr -Raw -ErrorAction SilentlyContinue }
    return ($contents -join "`n")
}

function Assert-LogsClean {
    $log = Get-LogTextSinceStart
    $patterns = @(
        '(?i)notification\s+(dispatch|outbox)[^\r\n]*(error|fail|exception)',
        '(?i)outbox\s+(dispatch|handler)[^\r\n]*(error|fail|exception)',
        '(?i)no outbox handler is registered',
        '(?i)failed to send email',
        '(?i)(provider|smtp)[^\r\n]*(error|fail|exception)'
    )
    $violations = @()

    # The API's exception middleware logs expected domain/validation 4xx responses
    # as "unhandled" errors. Keep those out of the unexpected-failure signal while
    # still failing the gate for an unclassified exception.
    $expectedExceptionTypes = '(?i)(ValidationException|AuthenticationException|BusinessException|NotFoundException|ConflictException|ForbiddenAccessException|PreconditionFailedException|TooManyRequestsException|PayloadTooLargeException)'
    foreach ($match in [regex]::Matches($log, '(?is)An unhandled exception has occurred\..{0,2500}')) {
        if ($match.Value -notmatch $expectedExceptionTypes) {
            $violations += $match.Value
        }
    }
    foreach ($pattern in $patterns) {
        $violations += [regex]::Matches($log, $pattern) | ForEach-Object { $_.Value }
    }
    Assert-Test "API, outbox, notification, and provider logs are clean" ($violations.Count -eq 0) "violations=$($violations.Count)"
    foreach ($email in $script:confirmedEmails) {
        $found = $log -match "(?is)To:\s*$([regex]::Escape($email)).*?/verify-email"
        Assert-Test "Mock Email confirmation was recorded for disposable account" $found
    }
}

try {
    Write-Section "Health, lifecycle, and anonymous authorization boundaries"
    if (-not (Test-Path $ApiLogPath)) {
        New-Item -Path $ApiLogPath -ItemType File -Force | Out-Null
    }
    $script:apiLogStartLength = (Get-Content $ApiLogPath -Raw -ErrorAction SilentlyContinue).Length
    Start-ApiIfNeeded
    $health = Invoke-TestRequest "GET /health" GET "/health"
    Assert-Test "Health endpoint returns 200" ($health.Status -eq 200) "status=$($health.Status)"
    $healthPing = Invoke-TestRequest "GET /api/health/ping" GET "/api/health/ping"
    Assert-Test "Health ping endpoint returns 200" ($healthPing.Status -eq 200) "status=$($healthPing.Status)"

    # The shared helper is used for a JSON compatibility probe; sensitive flows use the redacting wrapper above.
    $global:httpUrl = $ApiBaseUrl.TrimEnd('/')
    Invoke-Api -title "TestHelpers health compatibility probe" -method "GET" -endpoint "/api/health/ping" -reportFile $script:reportFile | Out-Null

    $unknownId = [guid]::NewGuid().ToString()
    $anonymousAdminRequests = @(
        @{ Name = "Anonymous pending verification list"; Method = "GET"; Path = "/api/admin/verifications"; Body = "" },
        @{ Name = "Anonymous verification details"; Method = "GET"; Path = "/api/admin/verifications/$unknownId"; Body = "" },
        @{ Name = "Anonymous document content"; Method = "GET"; Path = "/api/admin/verifications/documents/$unknownId/content"; Body = "" },
        @{ Name = "Anonymous document review"; Method = "PATCH"; Path = "/api/admin/verifications/documents/$unknownId"; Body = (@{ Decision = 1 } | ConvertTo-Json) },
        @{ Name = "Anonymous account approval"; Method = "PATCH"; Path = "/api/admin/verifications/$unknownId/approve-account"; Body = "" },
        @{ Name = "Anonymous account rejection"; Method = "PATCH"; Path = "/api/admin/verifications/$unknownId/reject-account"; Body = (@{ RejectionReason = "مرفوض" } | ConvertTo-Json) }
    )
    foreach ($test in $anonymousAdminRequests) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path -Body $test.Body
        Assert-Test "$($test.Name) returns 401" ($response.Status -eq 401) "status=$($response.Status)"
    }
    $anonymousNotificationRequests = @(
        @{ Name = "Anonymous notification list"; Method = "GET"; Path = "/api/notifications?pageSize=50"; Body = "" },
        @{ Name = "Anonymous unread count"; Method = "GET"; Path = "/api/notifications/unread-count"; Body = "" },
        @{ Name = "Anonymous mark read"; Method = "PATCH"; Path = "/api/notifications/$unknownId/read"; Body = "" },
        @{ Name = "Anonymous mark all read"; Method = "PATCH"; Path = "/api/notifications/read-all"; Body = "" }
    )
    foreach ($test in $anonymousNotificationRequests) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path -Body $test.Body
        Assert-Test "$($test.Name) returns 401" ($response.Status -eq 401) "status=$($response.Status)"
    }

    Write-Section "Admin and role boundaries with zero-assumption disposable accounts"
    $adminLoginBody = @{ Email = "admin@smartcourt.com"; Password = "Admin@123" } | ConvertTo-Json
    $adminLogin = Invoke-TestRequest "Seeded Admin login" POST "/api/auth/login" -Body $adminLoginBody
    Assert-Test "Seeded Admin login succeeds" ($adminLogin.Status -eq 200) "status=$($adminLogin.Status)"
    $adminToken = [string]$adminLogin.Json.data.accessToken
    if ($adminLogin.Status -ne 200 -or [string]::IsNullOrWhiteSpace($adminToken)) {
        throw "The seeded Admin account could not be used for the administrative verification test."
    }

    $adminUnknownReview = Invoke-TestRequest "Admin reviews unknown document" PATCH "/api/admin/verifications/documents/$unknownId" -Token $adminToken -Body (@{ Decision = 1 } | ConvertTo-Json)
    Assert-Test "Unknown document review returns 404" ($adminUnknownReview.Status -eq 404) "status=$($adminUnknownReview.Status)"
    $adminUnknownApprove = Invoke-TestRequest "Admin approves unknown account" PATCH "/api/admin/verifications/$unknownId/approve-account" -Token $adminToken
    Assert-Test "Unknown account approval returns 404" ($adminUnknownApprove.Status -eq 404) "status=$($adminUnknownApprove.Status)"
    $adminUnknownReject = Invoke-TestRequest "Admin rejects unknown account" PATCH "/api/admin/verifications/$unknownId/reject-account" -Token $adminToken -Body (@{ RejectionReason = "مستخدم غير موجود" } | ConvertTo-Json)
    Assert-Test "Unknown account rejection returns 404" ($adminUnknownReject.Status -eq 404) "status=$($adminUnknownReject.Status)"

    $documentOwner = New-DisposableUser "Lawyer" "document owner" $adminToken
    $accountApprover = New-DisposableUser "Client" "account approval owner" $adminToken
    $accountRejectee = New-DisposableUser "Client" "account rejection owner" $adminToken
    $unrelatedUser = New-DisposableUser "Client" "unrelated recipient" $adminToken

    foreach ($test in $anonymousAdminRequests) {
        $response = Invoke-TestRequest "Lawyer role boundary - $($test.Name)" $test.Method $test.Path -Token $documentOwner.Token -Body $test.Body
        Assert-Test "Lawyer cannot use $($test.Name)" ($response.Status -eq 403) "status=$($response.Status)"
    }

    if ([string]::IsNullOrWhiteSpace($SuperAdministratorEmail) -or
        [string]::IsNullOrWhiteSpace($SuperAdministratorPassword)) {
        Add-Skip "SuperAdministrator role boundary" "The repository seeds only Client, Lawyer, and Admin; no supported HTTP endpoint creates or assigns SuperAdministrator. Optional credentials were not supplied."
    }
    else {
        $superLoginBody = @{ Email = $SuperAdministratorEmail; Password = $SuperAdministratorPassword } | ConvertTo-Json
        $superLogin = Invoke-TestRequest "Optional SuperAdministrator login" POST "/api/auth/login" -Body $superLoginBody
        Assert-Test "Optional SuperAdministrator login succeeds" ($superLogin.Status -eq 200) "status=$($superLogin.Status)"
        if ($superLogin.Status -eq 200) {
            $superToken = [string]$superLogin.Json.data.accessToken
            $superList = Invoke-TestRequest "SuperAdministrator verification list" GET "/api/admin/verifications" -Token $superToken
            Assert-Test "SuperAdministrator is not granted the Admin-only verification route" ($superList.Status -eq 403) "status=$($superList.Status)"
        }
    }

    Write-Section "Admin verification read endpoints, validation, hostile input, and content authorization"
    $filePath = New-VerificationFile
    $ownerUpload = Submit-VerificationDocument "Approved-document fixture" $documentOwner 2 "2035-01-01" $filePath -RequirePersistedDocument
    $ownerDetails = Get-AdminDetails "Owner" $documentOwner.Id $adminToken
    Assert-Test "Admin can read verification details" ($ownerDetails.Status -eq 200) "status=$($ownerDetails.Status)"
    $approvedDocument = Require-AdminDocument $ownerDetails "NationalIdBack" "approved-document fixture"
    $approvedDocumentId = [string]$approvedDocument.documentId

    $pending = Invoke-TestRequest "Admin pending verification list" GET "/api/admin/verifications?PageNumber=1&PageSize=10" -Token $adminToken
    Assert-Test "Admin can list pending verifications" ($pending.Status -eq 200) "status=$($pending.Status)"
    $pendingSearch = [Uri]::EscapeDataString("Gate 5 document owner")
    $pendingOwner = Invoke-TestRequest "Admin pending list searches the disposable document owner" GET "/api/admin/verifications?PageNumber=1&PageSize=10&Search=$pendingSearch" -Token $adminToken
    $pendingMatch = @($pendingOwner.Json.data | Where-Object { [string]$_.lawyerId -eq $documentOwner.Id })
    Assert-Test "Pending list includes the disposable document owner" ($pendingOwner.Status -eq 200 -and $pendingMatch.Count -ge 1) "status=$($pendingOwner.Status)"
    $pendingStatus = Invoke-TestRequest "Admin pending list filtered by Pending status" GET "/api/admin/verifications?PageNumber=1&PageSize=10&Status=1" -Token $adminToken
    Assert-Test "Pending status filter returns 200" ($pendingStatus.Status -eq 200) "status=$($pendingStatus.Status)"
    $longSearch = [Uri]::EscapeDataString(("x" * 101))
    $invalidSearch = Invoke-TestRequest "Admin pending list overlong search" GET "/api/admin/verifications?PageNumber=1&PageSize=10&Search=$longSearch" -Token $adminToken
    Assert-Test "Overlong verification search returns 400" ($invalidSearch.Status -eq 400) "status=$($invalidSearch.Status)"
    $invalidStatus = Invoke-TestRequest "Admin pending list invalid status enum" GET "/api/admin/verifications?PageNumber=1&PageSize=10&Status=99" -Token $adminToken
    Assert-Test "Invalid verification status returns 400" ($invalidStatus.Status -eq 400) "status=$($invalidStatus.Status)"
    $invalidPage = Invoke-TestRequest "Admin pending list invalid pagination" GET "/api/admin/verifications?PageNumber=0&PageSize=51" -Token $adminToken
    Assert-Test "Invalid verification pagination returns 400" ($invalidPage.Status -eq 400) "status=$($invalidPage.Status)"

    $content = Invoke-TestRequest "Admin reads current document content" GET "/api/admin/verifications/documents/$approvedDocumentId/content" -Token $adminToken
    Assert-Test "Admin can read current document content" ($content.Status -eq 200) "status=$($content.Status)"
    $missingDetails = Invoke-TestRequest "Admin details unknown user" GET "/api/admin/verifications/$unknownId" -Token $adminToken
    Assert-Test "Unknown verification user returns 404" ($missingDetails.Status -eq 404) "status=$($missingDetails.Status)"
    $malformedDetails = Invoke-TestRequest "Admin details malformed user id" GET "/api/admin/verifications/not-a-guid" -Token $adminToken
    Assert-Test "Malformed verification user id returns 404 or 400" ($malformedDetails.Status -in @(400, 404)) "status=$($malformedDetails.Status)"
    $missingContent = Invoke-TestRequest "Admin content unknown document" GET "/api/admin/verifications/documents/$unknownId/content" -Token $adminToken
    Assert-Test "Unknown document content returns 404" ($missingContent.Status -eq 404) "status=$($missingContent.Status)"

    $ownerDocuments = Invoke-TestRequest "Owner reads own verification documents" GET "/api/UserVerification/$($documentOwner.Id)" -Token $documentOwner.Token
    Assert-Test "Owner can read own verification documents" ($ownerDocuments.Status -eq 200) "status=$($ownerDocuments.Status)"
    Add-Skip "UserVerification cross-user document route authorization" "The existing UserVerification read handler accepts the route UserId without an ownership check; Gate 5 does not alter that unrelated slice rule. Notification recipient isolation is tested below."

    Write-Section "Document approval, rejection, expiry, replay, and notification isolation"
    $approveBody = @{ Decision = 1; RejectionReason = $null } | ConvertTo-Json
    $approve = Invoke-TestRequest "Admin approves current document" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body $approveBody
    Assert-Test "Document approval returns 200" ($approve.Status -eq 200) "status=$($approve.Status)"
    $approvedData = @{ documentId = $approvedDocumentId; documentType = "NationalIdBack" }
    $approvedNotification = Find-Notification $documentOwner.Token "verification.document-approved" $approvedData
    Assert-ExactNotification `
        "Owner receives exact document-approved notification" `
        $approvedNotification `
        "verification.document-approved" `
        "Success" `
        "تم اعتماد مستند التحقق" `
        "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك." `
        $approvedData
    $repeatApprove = Invoke-TestRequest "Replay document approval" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body $approveBody
    Assert-Test "Repeated document approval preserves existing endpoint success" ($repeatApprove.Status -eq 200) "status=$($repeatApprove.Status)"
    Start-Sleep -Seconds 2
    Assert-Test "Repeated document approval does not duplicate notification" ((Get-NotificationCountFor $documentOwner.Token "verification.document-approved" $approvedData) -eq 1)

    Submit-VerificationDocument "Rejected-document fixture" $documentOwner 3 "2035-01-01" $filePath -RequirePersistedDocument | Out-Null
    $rejectedDetails = Get-AdminDetails "Rejected document" $documentOwner.Id $adminToken
    $rejectedDocument = Require-AdminDocument $rejectedDetails "BarAssociationCardFront" "rejected-document fixture"
    $rejectedDocumentId = [string]$rejectedDocument.documentId
    $hostileRejectionReason = '<script>alert(''x'')</script> OR 1=1 -- ' + ("ر" * 40)
    $rejectBody = @{ Decision = 2; RejectionReason = $hostileRejectionReason } | ConvertTo-Json
    $reject = Invoke-TestRequest "Admin rejects current document" PATCH "/api/admin/verifications/documents/$rejectedDocumentId" -Token $adminToken -Body $rejectBody
    Assert-Test "Document rejection returns 200" ($reject.Status -eq 200) "status=$($reject.Status)"
    $rejectedData = @{ documentId = $rejectedDocumentId; documentType = "BarAssociationCardFront" }
    $rejectedNotification = Find-Notification $documentOwner.Token "verification.document-rejected" $rejectedData
    Assert-ExactNotification `
        "Owner receives exact document-rejected notification without reason metadata" `
        $rejectedNotification `
        "verification.document-rejected" `
        "Warning" `
        "تم رفض مستند التحقق" `
        "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة." `
        $rejectedData
    Assert-Test "Document rejection notification does not contain full rejection reason" `
        (-not ([string]$rejectedNotification.body).Contains($hostileRejectionReason))
    $repeatReject = Invoke-TestRequest "Replay document rejection with a changed reason" PATCH "/api/admin/verifications/documents/$rejectedDocumentId" -Token $adminToken -Body (@{ Decision = 2; RejectionReason = "سبب ثانٍ لا يجب أن يظهر في الإشعار" } | ConvertTo-Json)
    Assert-Test "Repeated document rejection preserves existing endpoint success" ($repeatReject.Status -eq 200) "status=$($repeatReject.Status)"
    Assert-Test "Repeated document rejection does not duplicate notification" ((Get-NotificationCountFor $documentOwner.Token "verification.document-rejected" $rejectedData) -eq 1)

    Submit-VerificationDocument "Expired-document fixture" $documentOwner 5 "2035-01-01" $filePath -RequirePersistedDocument | Out-Null
    $expiredDetails = Get-AdminDetails "Expired document" $documentOwner.Id $adminToken
    $expiredDocument = Require-AdminDocument $expiredDetails "SelfieWithId" "expired-document fixture"
    $expiredDocumentId = [string]$expiredDocument.documentId
    Age-LocalVerificationDocument $expiredDocumentId
    $expiredReview = Invoke-TestRequest "Admin reviews expired document" PATCH "/api/admin/verifications/documents/$expiredDocumentId" -Token $adminToken -Body $approveBody
    Assert-Test "Expired document returns the existing 409 conflict outcome" ($expiredReview.Status -eq 409) "status=$($expiredReview.Status)"
    $expiredData = @{ documentId = $expiredDocumentId; documentType = "SelfieWithId" }
    $expiredNotification = Find-Notification $documentOwner.Token "verification.document-expired" $expiredData
    Assert-ExactNotification `
        "Owner receives exact document-expired notification" `
        $expiredNotification `
        "verification.document-expired" `
        "Warning" `
        "انتهت صلاحية مستند التحقق" `
        "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول." `
        $expiredData
    $repeatExpired = Invoke-TestRequest "Replay expired document review" PATCH "/api/admin/verifications/documents/$expiredDocumentId" -Token $adminToken -Body $approveBody
    Assert-Test "Repeated expired review preserves existing 409 outcome" ($repeatExpired.Status -eq 409) "status=$($repeatExpired.Status)"
    Assert-Test "Repeated expired review does not duplicate notification" ((Get-NotificationCountFor $documentOwner.Token "verification.document-expired" $expiredData) -eq 1)

    $missingRejectReason = Invoke-TestRequest "Reject document without reason" PATCH "/api/admin/verifications/documents/$expiredDocumentId" -Token $adminToken -Body (@{ Decision = 2; RejectionReason = "" } | ConvertTo-Json)
    Assert-Test "Reject without reason returns 400" ($missingRejectReason.Status -eq 400) "status=$($missingRejectReason.Status)"
    $tooLongRejectReason = Invoke-TestRequest "Reject document with overlong hostile reason" PATCH "/api/admin/verifications/documents/$expiredDocumentId" -Token $adminToken -Body (@{ Decision = 2; RejectionReason = ("x" * 501) } | ConvertTo-Json)
    Assert-Test "Overlong rejection reason returns 400" ($tooLongRejectReason.Status -eq 400) "status=$($tooLongRejectReason.Status)"
    $approveWithReason = Invoke-TestRequest "Approve document with forbidden reason field" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body (@{ Decision = 1; RejectionReason = "لا يجب قبول سبب مع الاعتماد" } | ConvertTo-Json)
    Assert-Test "Approve with rejection reason returns 400" ($approveWithReason.Status -eq 400) "status=$($approveWithReason.Status)"
    $invalidDecision = Invoke-TestRequest "Review document with invalid decision enum" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body (@{ Decision = 99 } | ConvertTo-Json)
    Assert-Test "Invalid review decision returns 400" ($invalidDecision.Status -eq 400) "status=$($invalidDecision.Status)"
    $typeMismatchDecision = Invoke-TestRequest "Review document with decision type mismatch" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body '{"Decision":"approve"}'
    Assert-Test "Review decision type mismatch returns 400" ($typeMismatchDecision.Status -eq 400) "status=$($typeMismatchDecision.Status)"

    Write-Section "Account approval/rejection transitions and deduplication"
    $accountApprove = Invoke-TestRequest "Admin approves account on actual Active transition" PATCH "/api/admin/verifications/$($accountApprover.Id)/approve-account" -Token $adminToken
    Assert-Test "Account approval returns 200" ($accountApprove.Status -eq 200) "status=$($accountApprove.Status)"
    $accountData = @{ userId = $accountApprover.Id }
    $accountApprovedNotification = Find-Notification $accountApprover.Token "account.approved" $accountData
    Assert-ExactNotification `
        "Account owner receives exact account-approved notification" `
        $accountApprovedNotification `
        "account.approved" `
        "Success" `
        "تم اعتماد حسابك" `
        "تم اعتماد حسابك وأصبح جاهزًا للاستخدام." `
        $accountData
    $repeatAccountApprove = Invoke-TestRequest "Replay account approval" PATCH "/api/admin/verifications/$($accountApprover.Id)/approve-account" -Token $adminToken
    Assert-Test "Repeated account approval preserves existing endpoint success" ($repeatAccountApprove.Status -eq 200) "status=$($repeatAccountApprove.Status)"
    Assert-Test "Account approval notification is emitted only on Active transition" ((Get-NotificationCountFor $accountApprover.Token "account.approved" $accountData) -eq 1)

    $accountRejectBody = @{ RejectionReason = "سبب إداري خاص لا يجب أن يدخل في بيانات الإشعار." } | ConvertTo-Json
    $accountReject = Invoke-TestRequest "Admin rejects account" PATCH "/api/admin/verifications/$($accountRejectee.Id)/reject-account" -Token $adminToken -Body $accountRejectBody
    Assert-Test "Account rejection returns 200" ($accountReject.Status -eq 200) "status=$($accountReject.Status)"
    $accountRejectedNotification = Find-Notification $accountRejectee.Token "account.rejected" @{ userId = $accountRejectee.Id }
    Assert-ExactNotification `
        "Account owner receives exact account-rejected notification without reason" `
        $accountRejectedNotification `
        "account.rejected" `
        "Critical" `
        "تم رفض الحساب" `
        "تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب." `
        @{ userId = $accountRejectee.Id }
    Assert-Test "Account rejection notification does not contain full rejection reason" `
        (-not ([string]$accountRejectedNotification.body).Contains("سبب إداري خاص"))
    $repeatAccountReject = Invoke-TestRequest "Replay account rejection" PATCH "/api/admin/verifications/$($accountRejectee.Id)/reject-account" -Token $adminToken -Body (@{ RejectionReason = "سبب إداري محدث لا يجب أن يظهر." } | ConvertTo-Json)
    Assert-Test "Repeated account rejection preserves existing endpoint success" ($repeatAccountReject.Status -eq 200) "status=$($repeatAccountReject.Status)"
    Assert-Test "Account rejection notification is emitted once per transition" ((Get-NotificationCountFor $accountRejectee.Token "account.rejected" @{ userId = $accountRejectee.Id }) -eq 1)

    Write-Section "Version conflicts and concurrent review behavior"
    Submit-VerificationDocument "Concurrency fixture" $documentOwner 6 "2035-01-01" $filePath -RequirePersistedDocument | Out-Null
    $concurrencyDetails = Get-AdminDetails "Concurrency fixture" $documentOwner.Id $adminToken
    $concurrencyDocument = Require-AdminDocument $concurrencyDetails "Other" "concurrency fixture"
    $concurrencyDocumentId = [string]$concurrencyDocument.documentId
    $concurrentResults = Invoke-ConcurrentReview `
        -DocumentId $concurrencyDocumentId `
        -AdminToken $adminToken `
        -FirstBody (@{ Decision = 1; RejectionReason = $null } | ConvertTo-Json) `
        -SecondBody (@{ Decision = 2; RejectionReason = "تنافس مراجعة متزامن" } | ConvertTo-Json)
    $concurrentStatuses = @($concurrentResults | ForEach-Object { $_.Status })
    Assert-Test "Concurrent review requests return only success or conflict" (@($concurrentStatuses | Where-Object { $_ -notin @(200, 409) }).Count -eq 0)
    Assert-Test "Concurrent review produces at least one committed decision" (@($concurrentStatuses | Where-Object { $_ -eq 200 }).Count -ge 1)
    Add-Skip "Deterministic row-version winner" "The HTTP race is timing-dependent; the existing current-version conflict path is tested deterministically below."

    Submit-VerificationDocument "Replacement-version fixture" $documentOwner 2 "2035-01-01" $filePath -RequirePersistedDocument | Out-Null
    $replacementDetails = Get-AdminDetails "Replacement version" $documentOwner.Id $adminToken
    $replacementDocument = Require-AdminDocument $replacementDetails "NationalIdBack" "replacement-version fixture"
    $replacementDocumentId = [string]$replacementDocument.documentId
    $staleReview = Invoke-TestRequest "Review superseded document version" PATCH "/api/admin/verifications/documents/$approvedDocumentId" -Token $adminToken -Body $approveBody
    Assert-Test "Superseded document review returns 409 conflict" ($staleReview.Status -eq 409) "status=$($staleReview.Status)"
    $staleContent = Invoke-TestRequest "Read superseded document content" GET "/api/admin/verifications/documents/$approvedDocumentId/content" -Token $adminToken
    Assert-Test "Superseded document content is not exposed as current" ($staleContent.Status -eq 404) "status=$($staleContent.Status)"
    Assert-Test "Replacement version has a distinct document id" ($replacementDocumentId -ne $approvedDocumentId)

    Write-Section "Notification list/count/read/read-all contracts and recipient isolation"
    $ownerItems = Get-NotificationItems $documentOwner.Token "Owner notification list"
    $ownerTypes = @($ownerItems | ForEach-Object { $_.type })
    Assert-Test "Owner notification list contains approved, rejected, and expired document types" `
        ($ownerTypes -contains "verification.document-approved" -and
         $ownerTypes -contains "verification.document-rejected" -and
         $ownerTypes -contains "verification.document-expired")
    foreach ($notification in @($ownerItems | Where-Object { $_.type -like "verification.*" })) {
        $forbiddenKeys = @($notification.data.PSObject.Properties.Name | Where-Object {
            $_ -in @("storagePath", "fileUrl", "downloadUrl", "content", "contentUrl", "rejectionReason", "privateReviewComment", "reason", "email", "phone", "nationalNumber", "token")
        })
        Assert-Test "Verification notification has no forbidden metadata fields" ($forbiddenKeys.Count -eq 0)
    }
    $ownerCount = Invoke-TestRequest "Owner unread notification count" GET "/api/notifications/unread-count" -Token $documentOwner.Token
    Assert-Test "Owner unread count returns 200" ($ownerCount.Status -eq 200) "status=$($ownerCount.Status)"
    Assert-Test "Owner has unread notifications before read" ([int]$ownerCount.Json.data.unreadCount -ge 3)
    $readTarget = $approvedNotification
    $readResponse = Invoke-TestRequest "Owner marks approved notification read" PATCH "/api/notifications/$($readTarget.id)/read" -Token $documentOwner.Token
    Assert-Test "Mark read returns 200" ($readResponse.Status -eq 200) "status=$($readResponse.Status)"
    Assert-Test "Mark read response contains a read timestamp" ($null -ne $readResponse.Json.data.readAtUtc)
    $readReplay = Invoke-TestRequest "Owner replays mark read" PATCH "/api/notifications/$($readTarget.id)/read" -Token $documentOwner.Token
    Assert-Test "Repeated mark read remains idempotent" ($readReplay.Status -eq 200) "status=$($readReplay.Status)"
    $ownerCountAfterRead = Invoke-TestRequest "Owner unread count after mark read" GET "/api/notifications/unread-count" -Token $documentOwner.Token
    Assert-Test "Unread count decreases after mark read" ([int]$ownerCountAfterRead.Json.data.unreadCount -lt [int]$ownerCount.Json.data.unreadCount)
    $readAll = Invoke-TestRequest "Owner marks all notifications read" PATCH "/api/notifications/read-all" -Token $documentOwner.Token
    Assert-Test "Mark all read returns 200" ($readAll.Status -eq 200) "status=$($readAll.Status)"
    $ownerCountFinal = Invoke-TestRequest "Owner unread count after read-all" GET "/api/notifications/unread-count" -Token $documentOwner.Token
    Assert-Test "Read-all leaves no unread owner notifications" ([int]$ownerCountFinal.Json.data.unreadCount -eq 0)
    $readList = Invoke-TestRequest "Owner lists read notifications" GET "/api/notifications?pageSize=50&isRead=true" -Token $documentOwner.Token
    Assert-Test "Read notification filter returns 200" ($readList.Status -eq 200) "status=$($readList.Status)"
    $badPageSize = Invoke-TestRequest "Notification list invalid page size" GET "/api/notifications?pageSize=0" -Token $documentOwner.Token
    Assert-Test "Notification invalid page size returns 400" ($badPageSize.Status -eq 400) "status=$($badPageSize.Status)"
    $badCursor = Invoke-TestRequest "Notification list invalid cursor" GET "/api/notifications?pageSize=10&cursor=not-a-valid-cursor" -Token $documentOwner.Token
    Assert-Test "Notification invalid cursor returns 400" ($badCursor.Status -eq 400) "status=$($badCursor.Status)"
    $emptyRead = Invoke-TestRequest "Notification empty id read" PATCH "/api/notifications/00000000-0000-0000-0000-000000000000/read" -Token $documentOwner.Token
    Assert-Test "Notification empty id read returns 404" ($emptyRead.Status -eq 404) "status=$($emptyRead.Status)"

    $unrelatedItems = Get-NotificationItems $unrelatedUser.Token "Unrelated notification list"
    $targetIds = @($approvedDocumentId, $rejectedDocumentId, $expiredDocumentId, $documentOwner.Id)
    $unrelatedLeak = @($unrelatedItems | Where-Object {
        $_.data -and (
            [string]$_.data.documentId -in $targetIds -or
            [string]$_.data.userId -in $targetIds)
    })
    Assert-Test "Unrelated user receives no verification notification leakage" ($unrelatedLeak.Count -eq 0)
    $adminItems = Get-NotificationItems $adminToken "Unrelated Admin notification list"
    $adminLeak = @($adminItems | Where-Object {
        $_.data -and (
            [string]$_.data.documentId -in $targetIds -or
            [string]$_.data.userId -in $targetIds)
    })
    Assert-Test "Admin inbox is not blindly broadcast verification work" ($adminLeak.Count -eq 0)
    $unrelatedRead = Invoke-TestRequest "Unrelated user reads owner notification" PATCH "/api/notifications/$($approvedNotification.id)/read" -Token $unrelatedUser.Token
    Assert-Test "Unrelated user cannot mark owner notification read" ($unrelatedRead.Status -eq 404) "status=$($unrelatedRead.Status)"
    $unrelatedAll = Invoke-TestRequest "Unrelated user marks all read" PATCH "/api/notifications/read-all" -Token $unrelatedUser.Token
    Assert-Test "Unrelated user read-all remains isolated" ($unrelatedAll.Status -eq 200) "status=$($unrelatedAll.Status)"
}
catch {
    $message = Protect-ReportText $_.Exception.Message
    $script:fatal.Add($message)
    $script:failed++
    Add-Report "`n### Fatal test error`n$message`n"
}
finally {
    Write-Section "API and mock Email log monitoring"
    try { Assert-LogsClean } catch {
        $script:failed++
        Add-Report "- [FAIL] **Log monitor execution** ($(Protect-ReportText $_.Exception.Message))"
    }

    Stop-ApiIfOwned
    $released = if ($script:ownsApi) { Test-PortReleased } else { $true }
    Assert-Test "API test port is released after owned process shutdown" $released
    if (-not $script:ownsApi) {
        Add-Skip "API shutdown ownership" "An externally running API was detected; no external process was terminated."
    }

    if (Test-Path $script:apiStdOut) { Remove-Item -LiteralPath $script:apiStdOut -Force -ErrorAction SilentlyContinue }
    if (Test-Path $script:apiStdErr) { Remove-Item -LiteralPath $script:apiStdErr -Force -ErrorAction SilentlyContinue }
    if ($null -ne $filePath -and (Test-Path -LiteralPath $filePath)) {
        Remove-Item -LiteralPath $filePath -Force -ErrorAction SilentlyContinue
    }

    $localStorageFullPath = [IO.Path]::GetFullPath($script:localStoragePath)
    $uploadsRoot = [IO.Path]::GetFullPath(
        (Join-Path $apiProjectDir "uploads"))
    $localStorageIsScoped = $localStorageFullPath.StartsWith(
        "$uploadsRoot$([IO.Path]::DirectorySeparatorChar)",
        [StringComparison]::OrdinalIgnoreCase)
    if ((Test-Path -LiteralPath $localStorageFullPath) -and $localStorageIsScoped) {
        Remove-Item -LiteralPath $localStorageFullPath `
            -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Section "Execution summary"
    Add-Report "| Metric | Count |`n|---|---:|`n| Passed assertions | $script:passed |`n| Failed assertions | $script:failed |`n| Documented skips | $script:skipped |"
    if ($script:fatal.Count -gt 0) {
        Add-Report "`n### Fatal errors`n"
        foreach ($errorText in $script:fatal) { Add-Report "- $errorText" }
    }
    Write-Host "Administrative verification notification HTTP tests complete: $script:passed passed, $script:failed failed, $script:skipped skipped."
}

if ($script:failed -gt 0) { exit 1 }
