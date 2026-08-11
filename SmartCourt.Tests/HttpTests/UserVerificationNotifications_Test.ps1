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
    "uploads\gate6-$([guid]::NewGuid().ToString('N'))"

if ([string]::IsNullOrWhiteSpace($ApiLogPath)) {
    $ApiLogPath = Join-Path $apiProjectDir "api_log.txt"
}
if ([string]::IsNullOrWhiteSpace($ReportFile)) {
    $ReportFile = Join-Path $scriptDir "UserVerificationNotifications_Report.md"
}

Import-Module (Join-Path $scriptDir "TestHelpers.psm1") -Force
Add-Type -AssemblyName System.Net.Http

$script:reportFile = $ReportFile
$script:apiBaseUrl = $ApiBaseUrl.TrimEnd('/')
$script:apiLogPath = $ApiLogPath
$script:useExistingApi = $UseExistingApi
$script:passed = 0
$script:failed = 0
$script:skipped = 0
$script:fatal = [System.Collections.Generic.List[string]]::new()
$script:apiProcess = $null
$script:ownsApi = $false
$script:apiLogStartLength = 0
$script:apiStdOut = Join-Path ([System.IO.Path]::GetTempPath()) "smartcourt-gate6-api-$([guid]::NewGuid().ToString('N')).out.log"
$script:apiStdErr = Join-Path ([System.IO.Path]::GetTempPath()) "smartcourt-gate6-api-$([guid]::NewGuid().ToString('N')).err.log"
$script:confirmedEmails = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)
$script:temporaryFiles = [System.Collections.Generic.List[string]]::new()

"# User Verification Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
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
    param($Value, [string]$Key = "")

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

function Parse-Json {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return $null }
    try { return $Text | ConvertFrom-Json -ErrorAction Stop }
    catch { return $null }
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
    $lineBreak = [Environment]::NewLine
    $codeFence = '```'
    Add-Report "### $(Protect-ReportText $Title)`n"
    Add-Report "**Request:** $Method $(Protect-ReportText $Url)`n"
    if (-not [string]::IsNullOrWhiteSpace($safeBody)) {
        Add-Report ("**Body:**" + $lineBreak + $codeFence + "json" + $lineBreak + $safeBody + $lineBreak + $codeFence + $lineBreak)
    }
    Add-Report "**Response Status:** $Status`n"
    if ([string]::IsNullOrWhiteSpace($safeResponse)) {
        Add-Report "**Response Body:** (Empty)`n---`n"
    }
    else {
        Add-Report ("**Response Body:**" + $lineBreak + $codeFence + "json" + $lineBreak + $safeResponse + $lineBreak + $codeFence + $lineBreak + "---" + $lineBreak)
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

    $url = if ($Path -match '^https?://') { $Path } else { "$script:apiBaseUrl$Path" }
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

function Invoke-MultipartDocuments {
    param(
        [string]$Title,
        [string]$Token,
        [string]$UserId,
        [object[]]$Documents,
        [switch]$NoReport
    )

    $url = "$script:apiBaseUrl/api/UserVerification/submit-verification-documents"
    $client = [System.Net.Http.HttpClient]::new()
    $content = [System.Net.Http.MultipartFormDataContent]::new()
    try {
        $content.Add([System.Net.Http.StringContent]::new($UserId), "UserId")
        $reportFields = [ordered]@{ UserId = $UserId }
        for ($index = 0; $index -lt $Documents.Count; $index++) {
            $document = $Documents[$index]
            $expirationField = "Documents[$index].ExpirationDate"
            $typeField = "Documents[$index].Type"
            $fileField = "Documents[$index].File"
            $content.Add([System.Net.Http.StringContent]::new([string]$document.ExpirationDate), $expirationField)
            $content.Add([System.Net.Http.StringContent]::new([string]$document.Type), $typeField)
            $reportFields[$expirationField] = $document.ExpirationDate
            $reportFields[$typeField] = $document.Type

            if (-not [string]::IsNullOrWhiteSpace([string]$document.Path)) {
                $bytes = [System.IO.File]::ReadAllBytes([string]$document.Path)
                $fileContent = [System.Net.Http.ByteArrayContent]::new($bytes)
                if (-not [string]::IsNullOrWhiteSpace([string]$document.ContentType)) {
                    $fileContent.Headers.ContentType =
                        [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse([string]$document.ContentType)
                }
                $content.Add(
                    $fileContent,
                    $fileField,
                    [string]$document.FileName)
            }
            $reportFields[$fileField] = "[REDACTED_FILE]"
        }

        if (-not [string]::IsNullOrWhiteSpace($Token)) {
            $client.DefaultRequestHeaders.Authorization =
                [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
        }
        $response = $client.PostAsync($url, $content).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        if (-not $NoReport) {
            Write-HttpExchange `
                $Title `
                "POST" `
                $url `
                ($reportFields | ConvertTo-Json -Depth 10) `
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

function New-VerificationFile {
    param([string]$Name = "smartcourt-gate6.jpg")
    $path = Join-Path ([System.IO.Path]::GetTempPath()) "$([guid]::NewGuid().ToString('N'))-$Name"
    [System.IO.File]::WriteAllBytes(
        $path,
        [byte[]](0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0xFF, 0xD9))
    [void]$script:temporaryFiles.Add($path)
    return $path
}

function Extract-EmailConfirmationUrl {
    param([string]$Email)
    $escaped = [regex]::Escape($Email)
    $deadline = (Get-Date).AddSeconds(45)
    do {
        $fullLog = Get-Content $script:apiLogPath -Raw -ErrorAction SilentlyContinue
        $pattern = '(?is)To:\s*' + $escaped + '.*?href=[''\"]([^''\"]*/verify-email[^''\"]*)[''\"]'
        if ($fullLog) {
            $match = [regex]::Match($fullLog, $pattern)
            if ($match.Success) {
                return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
            }
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    return ""
}

function Confirm-DisposableEmail {
    param([string]$Email, [string]$UserId, [string]$Label)

    $confirmationUrl = Extract-EmailConfirmationUrl $Email
    if ([string]::IsNullOrWhiteSpace($confirmationUrl)) {
        Assert-Test "$Label confirmation link is present in the mock Email log" $false
        throw "No mock Email confirmation link was found."
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
    $token = if ($values.ContainsKey("token")) { [string]$values["token"] } else { "" }
    if ([string]::IsNullOrWhiteSpace($actualUserId) -or [string]::IsNullOrWhiteSpace($token)) {
        Assert-Test "$Label confirmation link contains userId and token" $false
        throw "The mock Email confirmation link was incomplete."
    }

    $endpoint = "/api/auth/confirm-email?userId=$actualUserId&token=$([Uri]::EscapeDataString($token))"
    $confirmed = Invoke-TestRequest "$Label confirms Email from mock log" GET $endpoint
    Assert-Test "$Label Email confirmation succeeds" ($confirmed.Status -eq 200) "status=$($confirmed.Status)"
    if ($confirmed.Status -ne 200) { throw "Disposable Email confirmation failed." }
    [void]$script:confirmedEmails.Add($Email)
}

function New-DisposableUser {
    param([string]$Kind, [string]$Label)

    $suffix = [guid]::NewGuid().ToString('N').Substring(0, 12)
    $email = "gate6_$($Kind.ToLowerInvariant())_$suffix@example.com"
    $password = "Gate6-$suffix!Aa9"
    $endpoint = if ($Kind -eq "Lawyer") { "/api/auth/register/lawyer" } else { "/api/auth/register/client" }
    $body = @{
        Email = $email
        FullName = "Gate 6 $Label"
        Password = $password
        ConfirmPassword = $password
    } | ConvertTo-Json
    $registered = Invoke-TestRequest "Setup $Label registration" POST $endpoint -Body $body
    Assert-Test "$Label registration returns 201" ($registered.Status -eq 201) "status=$($registered.Status)"
    $userId = [string]$registered.Json.data.userId
    if ($registered.Status -ne 201 -or [string]::IsNullOrWhiteSpace($userId)) {
        throw "Could not create the disposable $Kind account."
    }

    Confirm-DisposableEmail $email $userId $Label
    $login = Invoke-TestRequest "Setup $Label login" POST "/api/auth/login" -Body (@{ Email = $email; Password = $password } | ConvertTo-Json)
    Assert-Test "$Label login succeeds after Email confirmation" ($login.Status -eq 200) "status=$($login.Status)"
    $accessToken = [string]$login.Json.data.accessToken
    if ($login.Status -ne 200 -or [string]::IsNullOrWhiteSpace($accessToken)) {
        throw "Could not log in the disposable $Kind account."
    }

    return [pscustomobject]@{
        Id = $userId
        Token = $accessToken
    }
}

function New-AdminSession {
    param([string]$Label, [string]$Email, [string]$Password)
    $login = Invoke-TestRequest "Setup $Label Admin login" POST "/api/auth/login" -Body (@{ Email = $Email; Password = $Password } | ConvertTo-Json)
    Assert-Test "$Label Admin login succeeds" ($login.Status -eq 200) "status=$($login.Status)"
    $token = [string]$login.Json.data.accessToken
    if ($login.Status -ne 200 -or [string]::IsNullOrWhiteSpace($token)) {
        throw "$Label Admin login failed."
    }
    return [pscustomobject]@{ Label = $Label; Token = $token; Id = [string]$login.Json.data.user.id }
}

function Test-ApiHealth {
    return (Invoke-TestRequest "Health probe" GET "/health" -NoReport).Status -eq 200
}

function Set-TestApiEnvironment {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ConnectionStrings__DefaultConnection = "Data Source=.;Initial Catalog=SmartCourt_Dev;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;"
    $env:AuthEmail__PublicBaseUrl = "http://localhost:5173"
    $env:FileStorage__Provider = "Local"
    $env:FileStorage__BasePath = $script:localStoragePath
    $env:Supabase__Url = $null
    $env:Supabase__ApiKey = $null
    $env:Supabase__Bucket = $null
    $env:SmtpSettings__Server = ""
    $env:SmtpSettings__Port = "1025"
    $env:OutboxDispatch__Enabled = "true"
}

function Start-ApiIfNeeded {
    if (Test-ApiHealth) {
        if (-not $script:useExistingApi) {
            throw "An API is already listening on the requested test URL. Stop it or rerun with -UseExistingApi."
        }
        Add-Skip "API lifecycle" "An existing API was explicitly supplied; the script will not terminate it."
        return
    }

    if (-not (Test-Path $apiDllPath)) {
        throw "Built API assembly was not found. Build SmartCourt before running the HTTP test."
    }

    $environmentNames = @(
        "ASPNETCORE_ENVIRONMENT",
        "ConnectionStrings__DefaultConnection",
        "AuthEmail__PublicBaseUrl",
        "FileStorage__Provider",
        "FileStorage__BasePath",
        "Supabase__Url",
        "Supabase__ApiKey",
        "Supabase__Bucket",
        "SmtpSettings__Server",
        "SmtpSettings__Port",
        "OutboxDispatch__Enabled"
    )
    $oldEnvironment = @{}
    foreach ($name in $environmentNames) {
        $oldEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }

    try {
        Set-TestApiEnvironment
        $apiArguments = '"' + $apiDllPath + '" --urls ' + $script:apiBaseUrl
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
        $uri = [Uri]$script:apiBaseUrl
        $tcp = [System.Net.Sockets.TcpClient]::new()
        try {
            $task = $tcp.ConnectAsync($uri.Host, $uri.Port)
            if ($task.Wait(750) -and $tcp.Connected) { return $false }
            return $true
        }
        finally { $tcp.Dispose() }
    }
    catch { return $true }
}

function Find-Notification {
    param([string]$Token, [string]$Type, [hashtable]$ExpectedData, [int]$TimeoutSeconds = 75)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $page = Invoke-TestRequest "Poll notification $Type" GET "/api/notifications?pageSize=50" -Token $Token -NoReport
        if ($page.Status -eq 200 -and $null -ne $page.Json) {
            foreach ($item in @($page.Json.data.items)) {
                if ($item.type -ne $Type) { continue }
                $matches = $true
                foreach ($key in $ExpectedData.Keys) {
                    $property = if ($null -ne $item.data) { $item.data.PSObject.Properties[$key] } else { $null }
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
    param([string]$Token, [string]$Label)
    $response = Invoke-TestRequest $Label GET "/api/notifications?pageSize=50" -Token $Token
    Assert-Test "$Label returns 200" ($response.Status -eq 200) "status=$($response.Status)"
    if ($response.Status -ne 200) { return @() }
    return @($response.Json.data.items)
}

function Get-NotificationCountFor {
    param([string]$Token, [string]$Type, [hashtable]$ExpectedData, [string]$Label)
    $items = @(Get-NotificationItems $Token $Label)
    $matches = @($items | Where-Object {
        if ($_.type -ne $Type) { return $false }
        foreach ($key in $ExpectedData.Keys) {
            $property = if ($null -ne $_.data) { $_.data.PSObject.Properties[$key] } else { $null }
            if ($null -eq $property -or [string]$property.Value -ne [string]$ExpectedData[$key]) { return $false }
        }
        return $true
    })
    return $matches.Count
}

function Assert-ExactReviewNotification {
    param([string]$Label, $Notification, [string]$UserId, [int]$DocumentCount)
    $expectedData = @{ userId = $UserId; documentCount = [string]$DocumentCount }
    $valid = $null -ne $Notification -and
        $Notification.type -eq "verification.review-requested" -and
        $Notification.severity -eq "Information" -and
        $Notification.title -eq "طلب مراجعة مستندات التحقق" -and
        $Notification.body -eq "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب." -and
        $null -eq $Notification.actionUrl
    if ($valid) {
        $keys = @($Notification.data.PSObject.Properties.Name)
        $valid = $keys.Count -eq 2
        foreach ($key in $expectedData.Keys) {
            $property = $Notification.data.PSObject.Properties[$key]
            if ($null -eq $property -or [string]$property.Value -ne [string]$expectedData[$key]) { $valid = $false }
        }
    }
    $dataKeys = if ($null -eq $Notification -or $null -eq $Notification.data) { @() } else { @($Notification.data.PSObject.Properties.Name) }
    $forbidden = @("storagePath", "fileUrl", "downloadUrl", "content", "contentUrl", "documentContent", "rejectionReason", "privateReviewComment", "reason", "email", "phone", "phoneNumber", "nationalNumber", "accessToken", "refreshToken", "providerId", "idempotencyKey", "token")
    $hasForbidden = @($dataKeys | Where-Object { $_ -in $forbidden }).Count -gt 0
    Assert-Test $Label ($valid -and -not $hasForbidden)
}

function Get-CurrentDocument {
    param($Documents)
    return @($Documents | Where-Object { $_.isCurrent -eq $true } | Select-Object -First 1)
}

function Get-DocumentByType {
    param($Documents, [int]$Type)
    $numeric = [string]$Type
    return @($Documents | Where-Object {
        $value = [string]$_.documentType
        $value -eq $numeric -or $value -eq @("", "NationalIdFront", "NationalIdBack", "BarAssociationCardFront", "BarAssociationCardBack", "SelfieWithId", "Other", "OfficialProfilePicture")[$Type]
    })
}

function Get-StoredFileId {
    param([string]$VerificationDocumentId)
    $parsedId = [guid]::Empty
    if (-not [guid]::TryParse($VerificationDocumentId, [ref]$parsedId)) {
        throw "The verification document ID was invalid before the local fixture query."
    }
    $sql = "SET NOCOUNT ON; SELECT CAST(StoredFileId AS varchar(36)) FROM UserVerificationDocuments WHERE Id = '$parsedId';"
    $output = & sqlcmd -S . -d SmartCourt_Dev -E -C -h -1 -W -b -Q $sql 2>&1
    if ($LASTEXITCODE -ne 0) { throw "The local SQL database could not read the disposable stored-file fixture." }
    $storedFileId = [string]($output | Select-Object -Last 1)
    $storedFileId = $storedFileId.Trim()
    if ([string]::IsNullOrWhiteSpace($storedFileId)) { throw "The disposable stored-file fixture was not found." }
    return $storedFileId
}

function Get-LogTextSinceStart {
    $contents = @()
    if (Test-Path $script:apiLogPath) {
        $apiLog = Get-Content $script:apiLogPath -Raw -ErrorAction SilentlyContinue
        if ($apiLog.Length -ge $script:apiLogStartLength) { $contents += $apiLog.Substring($script:apiLogStartLength) } else { $contents += $apiLog }
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
    $expectedExceptionTypes = '(?i)(ValidationException|AuthenticationException|BusinessException|NotFoundException|ConflictException|ForbiddenAccessException|PreconditionFailedException|TooManyRequestsException|PayloadTooLargeException)'
    foreach ($match in [regex]::Matches($log, '(?is)An unhandled exception has occurred\..{0,2500}')) {
        if ($match.Value -notmatch $expectedExceptionTypes) { $violations += $match.Value }
    }
    foreach ($pattern in $patterns) { $violations += [regex]::Matches($log, $pattern) | ForEach-Object { $_.Value } }
    Assert-Test "API, outbox, notification, and provider logs are clean" ($violations.Count -eq 0) "violations=$($violations.Count)"
    foreach ($email in $script:confirmedEmails) {
        $found = $log -match "(?is)To:\s*$([regex]::Escape($email)).*?/verify-email"
        Assert-Test "Mock Email confirmation was recorded for each disposable account" $found
    }
}

try {
    if (-not (Test-Path $script:apiLogPath)) { New-Item -Path $script:apiLogPath -ItemType File -Force | Out-Null }
    $script:apiLogStartLength = (Get-Content $script:apiLogPath -Raw -ErrorAction SilentlyContinue).Length
    Start-ApiIfNeeded

    Write-Section "Health, anonymous access, and notification authorization boundaries"
    $health = Invoke-TestRequest "GET /health" GET "/health"
    Assert-Test "Health returns 200" ($health.Status -eq 200) "status=$($health.Status)"
    $ping = Invoke-TestRequest "GET /api/health/ping" GET "/api/health/ping"
    Assert-Test "Health ping returns 200" ($ping.Status -eq 200) "status=$($ping.Status)"
    $global:httpUrl = $script:apiBaseUrl

    $unknownId = [guid]::NewGuid().ToString()
    $anonymousRequests = @(
        @{ Label = "Anonymous submit verification"; Method = "POST"; Path = "/api/UserVerification/submit-verification-documents"; Body = "{}" },
        @{ Label = "Anonymous list user documents"; Method = "GET"; Path = "/api/UserVerification/$unknownId"; Body = "" },
        @{ Label = "Anonymous user document content"; Method = "GET"; Path = "/api/UserVerification/documents/$unknownId/content"; Body = "" },
        @{ Label = "Anonymous delete document"; Method = "DELETE"; Path = "/api/UserVerification?UserId=$unknownId&DocumentId=$unknownId"; Body = "" },
        @{ Label = "Anonymous notification list"; Method = "GET"; Path = "/api/notifications?pageSize=50"; Body = "" },
        @{ Label = "Anonymous unread count"; Method = "GET"; Path = "/api/notifications/unread-count"; Body = "" },
        @{ Label = "Anonymous notification read"; Method = "PATCH"; Path = "/api/notifications/$unknownId/read"; Body = "" },
        @{ Label = "Anonymous notification read-all"; Method = "PATCH"; Path = "/api/notifications/read-all"; Body = "" }
    )
    foreach ($test in $anonymousRequests) {
        $response = Invoke-TestRequest $test.Label $test.Method $test.Path -Body $test.Body
        Assert-Test "$($test.Label) returns 401" ($response.Status -eq 401) "status=$($response.Status)"
    }

    Write-Section "Admin-only recipient setup and role boundaries"
    $adminSessions = @(
        (New-AdminSession "Primary" "admin@smartcourt.com" "Admin@123"),
        (New-AdminSession "Secondary" "kokker@gmail.com" "Kokker@123"),
        (New-AdminSession "Tertiary" "moatazmohammed2392003@gmail.com" "Admin@123")
    )
    $primaryAdmin = $adminSessions[0]

    $owner = New-DisposableUser "Lawyer" "submission owner"
    $partialOwner = New-DisposableUser "Lawyer" "partial submission owner"
    $multiOwner = New-DisposableUser "Lawyer" "multi-document owner"
    $unrelated = New-DisposableUser "Client" "unrelated user"

    $adminUnknown = Invoke-TestRequest "Admin reads unknown verification user" GET "/api/admin/verifications/$unknownId" -Token $primaryAdmin.Token
    Assert-Test "Admin unknown verification user returns 404" ($adminUnknown.Status -eq 404) "status=$($adminUnknown.Status)"

    $lawyerAdminList = Invoke-TestRequest "Lawyer accesses Admin verification list" GET "/api/admin/verifications" -Token $owner.Token
    Assert-Test "Lawyer cannot access Admin verification list" ($lawyerAdminList.Status -eq 403) "status=$($lawyerAdminList.Status)"
    $lawyerNotificationList = Invoke-TestRequest "Lawyer notification list remains authenticated" GET "/api/notifications?pageSize=50" -Token $owner.Token
    Assert-Test "Authenticated non-Admin can access own notification list" ($lawyerNotificationList.Status -eq 200) "status=$($lawyerNotificationList.Status)"

    if ([string]::IsNullOrWhiteSpace($SuperAdministratorEmail) -or [string]::IsNullOrWhiteSpace($SuperAdministratorPassword)) {
        Add-Skip "SuperAdministrator role boundary" "The repository seeds Admin but no supported HTTP endpoint creates or assigns SuperAdministrator; optional credentials were not supplied."
    }
    else {
        $superLogin = Invoke-TestRequest "Optional SuperAdministrator login" POST "/api/auth/login" -Body (@{ Email = $SuperAdministratorEmail; Password = $SuperAdministratorPassword } | ConvertTo-Json)
        Assert-Test "Optional SuperAdministrator login succeeds" ($superLogin.Status -eq 200) "status=$($superLogin.Status)"
        if ($superLogin.Status -eq 200) {
            $superToken = [string]$superLogin.Json.data.accessToken
            $superList = Invoke-TestRequest "SuperAdministrator reads Admin verification list" GET "/api/admin/verifications" -Token $superToken
            Assert-Test "SuperAdministrator is not granted Admin-only verification routes" ($superList.Status -eq 403) "status=$($superList.Status)"
        }
    }

    Write-Section "Successful upload, pending/detail/content endpoints, and causal review notification"
    $validFile = New-VerificationFile
    $successfulUpload = Invoke-MultipartDocuments `
        "Owner submits one valid verification document" `
        $owner.Token `
        $owner.Id `
        @([pscustomobject]@{ Type = 1; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "owner-front.jpg" })
    Assert-Test "Successful upload returns 200" ($successfulUpload.Status -eq 200) "status=$($successfulUpload.Status)"
    $uploadedCount = @($successfulUpload.Json.data.uploadedDocuments).Count
    $failedCount = @($successfulUpload.Json.data.failedDocuments).Count
    Assert-Test "Successful upload persists exactly one document in the response" ($uploadedCount -eq 1 -and $failedCount -eq 0) "uploaded=$uploadedCount failed=$failedCount"

    $ownerDocs = Invoke-TestRequest "Owner lists submitted documents" GET "/api/UserVerification/$($owner.Id)" -Token $owner.Token
    Assert-Test "Owner document list returns 200" ($ownerDocs.Status -eq 200) "status=$($ownerDocs.Status)"
    $ownerDocument = Get-CurrentDocument @($ownerDocs.Json.data.documents)
    $ownerDocumentId = [string]$ownerDocument.documentId
    Assert-Test "Owner list contains a current document ID" (-not [string]::IsNullOrWhiteSpace($ownerDocumentId))

    $adminPending = Invoke-TestRequest "Admin lists pending verifications" GET "/api/admin/verifications?PageNumber=1&PageSize=10" -Token $primaryAdmin.Token
    Assert-Test "Admin pending verification list returns 200" ($adminPending.Status -eq 200) "status=$($adminPending.Status)"
    $adminDetails = Invoke-TestRequest "Admin reads owner verification details" GET "/api/admin/verifications/$($owner.Id)" -Token $primaryAdmin.Token
    Assert-Test "Admin verification details return 200" ($adminDetails.Status -eq 200) "status=$($adminDetails.Status)"
    $adminDocumentId = [string](@($adminDetails.Json.data.documents | Select-Object -First 1).documentId)
    Assert-Test "Admin details expose the current document ID" (-not [string]::IsNullOrWhiteSpace($adminDocumentId))
    $adminContent = Invoke-TestRequest "Admin reads submitted document content" GET "/api/admin/verifications/documents/$adminDocumentId/content" -Token $primaryAdmin.Token
    Assert-Test "Admin document content returns 200" ($adminContent.Status -eq 200) "status=$($adminContent.Status)"
    $ownerContent = Invoke-TestRequest "Owner reads own current document content" GET "/api/UserVerification/documents/$ownerDocumentId/content" -Token $owner.Token
    Assert-Test "Owner document content returns 200" ($ownerContent.Status -eq 200) "status=$($ownerContent.Status)"

    $ownerReviewNotifications = @()
    foreach ($admin in $adminSessions) {
        $notification = Find-Notification $admin.Token "verification.review-requested" @{ userId = $owner.Id; documentCount = "1" }
        Assert-ExactReviewNotification "$($admin.Label) receives one review-requested notification after the upload" $notification $owner.Id 1
        if ($null -ne $notification) { $ownerReviewNotifications += $notification }
    }
    Assert-Test "The upload creates one logical review notification per Admin" ($ownerReviewNotifications.Count -eq $adminSessions.Count)

    $ownerInbox = Get-NotificationItems $owner.Token "Owner notification inbox after submission"
    Assert-Test "Uploader does not receive the Admin review-requested notification" (@($ownerInbox | Where-Object { $_.type -eq "verification.review-requested" }).Count -eq 0)
    $unrelatedInbox = Get-NotificationItems $unrelated.Token "Unrelated user inbox before submission notification checks"
    Assert-Test "Unrelated user receives no review-requested notification" (@($unrelatedInbox | Where-Object { $_.type -eq "verification.review-requested" }).Count -eq 0)

    Write-Section "Partial uploads, multiple-file coalescing, and notification counts"
    $partialBefore = @{}
    foreach ($admin in $adminSessions) {
        $partialBefore[$admin.Label] = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $partialOwner.Id } "$($admin.Label) partial-before notification list"
    }
    $partialUpload = Invoke-MultipartDocuments `
        "Partial submission with one valid and one expired document" `
        $partialOwner.Token `
        $partialOwner.Id `
        @(
            [pscustomobject]@{ Type = 2; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "partial-valid.jpg" },
            [pscustomobject]@{ Type = 3; ExpirationDate = "2000-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "partial-expired.jpg" }
        )
    Assert-Test "Partial upload request returns 200" ($partialUpload.Status -eq 200) "status=$($partialUpload.Status)"
    Assert-Test "Partial upload persists one and reports one failed document" (@($partialUpload.Json.data.uploadedDocuments).Count -eq 1 -and @($partialUpload.Json.data.failedDocuments).Count -eq 1)
    foreach ($admin in $adminSessions) {
        $notification = Find-Notification $admin.Token "verification.review-requested" @{ userId = $partialOwner.Id; documentCount = "1" }
        Assert-ExactReviewNotification "$($admin.Label) receives one notification for a partial successful upload" $notification $partialOwner.Id 1
        $after = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $partialOwner.Id } "$($admin.Label) partial-after notification list"
        Assert-Test "$($admin.Label) receives only one partial-upload notification" ($after -eq ($partialBefore[$admin.Label] + 1)) "before=$($partialBefore[$admin.Label]) after=$after"
    }

    $multiUpload = Invoke-MultipartDocuments `
        "User submits two valid documents in one request" `
        $multiOwner.Token `
        $multiOwner.Id `
        @(
            [pscustomobject]@{ Type = 1; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "multi-front.jpg" },
            [pscustomobject]@{ Type = 2; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "multi-back.jpg" }
        )
    Assert-Test "Two-document upload returns 200" ($multiUpload.Status -eq 200) "status=$($multiUpload.Status)"
    Assert-Test "Two-document upload persists both documents" (@($multiUpload.Json.data.uploadedDocuments).Count -eq 2 -and @($multiUpload.Json.data.failedDocuments).Count -eq 0)
    foreach ($admin in $adminSessions) {
        $notification = Find-Notification $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id; documentCount = "2" }
        Assert-ExactReviewNotification "$($admin.Label) receives one notification for two uploaded documents" $notification $multiOwner.Id 2
        $count = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id; documentCount = "2" } "$($admin.Label) two-document notification count"
        Assert-Test "$($admin.Label) receives one, not two, notifications for the multi-file request" ($count -eq 1) "count=$count"
    }

    Write-Section "Replacement versions, deletion, ownership, and no-notification outcomes"
    $beforeReplacement = Invoke-TestRequest "Multi-document owner lists versions before replacement" GET "/api/UserVerification/$($multiOwner.Id)" -Token $multiOwner.Token
    $typeOneBefore = @(Get-DocumentByType @($beforeReplacement.Json.data.documents) 1 | Where-Object { $_.isCurrent -eq $true } | Select-Object -First 1)
    $oldTypeOneId = [string]$typeOneBefore.documentId
    $replacementBeforeCounts = @{}
    foreach ($admin in $adminSessions) {
        $replacementBeforeCounts[$admin.Label] = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id } "$($admin.Label) replacement-before notification list"
    }
    $replacementUpload = Invoke-MultipartDocuments `
        "User replaces a current verification document" `
        $multiOwner.Token `
        $multiOwner.Id `
        @([pscustomobject]@{ Type = 1; ExpirationDate = "2036-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "replacement-front.jpg" })
    Assert-Test "Replacement upload returns 200" ($replacementUpload.Status -eq 200) "status=$($replacementUpload.Status)"
    $afterReplacement = Invoke-TestRequest "Multi-document owner lists versions after replacement" GET "/api/UserVerification/$($multiOwner.Id)" -Token $multiOwner.Token
    $typeOneAfter = @(Get-DocumentByType @($afterReplacement.Json.data.documents) 1)
    $newCurrentTypeOne = @($typeOneAfter | Where-Object { $_.isCurrent -eq $true } | Select-Object -First 1)
    $oldVersion = @($typeOneAfter | Where-Object { [string]$_.documentId -eq $oldTypeOneId })
    Assert-Test "Replacement creates a distinct current document version" ($newCurrentTypeOne.Count -eq 1 -and [string]$newCurrentTypeOne[0].documentId -ne $oldTypeOneId)
    Assert-Test "Replacement marks the previous version non-current" ($oldVersion.Count -eq 1 -and $oldVersion[0].isCurrent -eq $false)
    foreach ($admin in $adminSessions) {
        $notification = Find-Notification $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id; documentCount = "1" }
        Assert-ExactReviewNotification "$($admin.Label) receives one notification for the replacement upload" $notification $multiOwner.Id 1
        $afterCount = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id } "$($admin.Label) replacement-after notification list"
        Assert-Test "$($admin.Label) receives exactly one additional replacement notification" ($afterCount -eq ($replacementBeforeCounts[$admin.Label] + 1)) "before=$($replacementBeforeCounts[$admin.Label]) after=$afterCount"
    }

    $replacementDocumentId = [string]$newCurrentTypeOne[0].documentId
    $attackerContent = Invoke-TestRequest "Unrelated user reads owner document content" GET "/api/UserVerification/documents/$replacementDocumentId/content" -Token $unrelated.Token
    Assert-Test "Unrelated user cannot read the owner document" ($attackerContent.Status -eq 404) "status=$($attackerContent.Status)"
    $ownerStoredFileId = Get-StoredFileId $replacementDocumentId
    $crossUserDelete = Invoke-TestRequest "Unrelated user deletes using unrelated UserId" DELETE "/api/UserVerification?UserId=$($unrelated.Id)&DocumentId=$ownerStoredFileId" -Token $unrelated.Token
    Assert-Test "Cross-user delete with the attacker UserId returns 404" ($crossUserDelete.Status -eq 404) "status=$($crossUserDelete.Status)"
    $delete = Invoke-TestRequest "Owner deletes current verification document" DELETE "/api/UserVerification?UserId=$($multiOwner.Id)&DocumentId=$ownerStoredFileId" -Token $multiOwner.Token
    Assert-Test "Owner deletion returns 200" ($delete.Status -eq 200) "status=$($delete.Status)"
    $deletedContent = Invoke-TestRequest "Owner reads deleted document content" GET "/api/UserVerification/documents/$replacementDocumentId/content" -Token $multiOwner.Token
    Assert-Test "Deleted document content returns 404" ($deletedContent.Status -eq 404) "status=$($deletedContent.Status)"
    foreach ($admin in $adminSessions) {
        $deleteCount = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $multiOwner.Id } "$($admin.Label) post-delete notification list"
        Assert-Test "$($admin.Label) receives no notification for deletion" ($deleteCount -eq ($replacementBeforeCounts[$admin.Label] + 1))
    }

    Write-Section "Validation, hostile input, malformed identifiers, and no-event failures"
    $missingUserUpload = Invoke-MultipartDocuments "Submit without UserId" $owner.Token "" @([pscustomobject]@{ Type = 1; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "missing-user.jpg" })
    Assert-Test "Submit without UserId returns 400" ($missingUserUpload.Status -eq 400) "status=$($missingUserUpload.Status)"
    $jsonToMultipart = Invoke-TestRequest "JSON sent to multipart submit endpoint" POST "/api/UserVerification/submit-verification-documents" -Token $owner.Token -Body "{}"
    Assert-Test "JSON sent to multipart endpoint returns validation/media failure" ($jsonToMultipart.Status -in @(400, 415)) "status=$($jsonToMultipart.Status)"
    $duplicateUpload = Invoke-MultipartDocuments `
        "Submit duplicate document types" `
        $owner.Token `
        $owner.Id `
        @(
            [pscustomobject]@{ Type = 2; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "duplicate-one.jpg" },
            [pscustomobject]@{ Type = 2; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "duplicate-two.jpg" }
        )
    Assert-Test "Duplicate document types return 400" ($duplicateUpload.Status -eq 400) "status=$($duplicateUpload.Status)"
    $invalidDateUpload = Invoke-MultipartDocuments "Submit invalid date format" $owner.Token $owner.Id @([pscustomobject]@{ Type = 3; ExpirationDate = "not-a-date"; Path = $validFile; ContentType = "image/jpeg"; FileName = "invalid-date.jpg" })
    Assert-Test "Invalid expiration date returns 400" ($invalidDateUpload.Status -eq 400) "status=$($invalidDateUpload.Status)"
    $invalidTypeUpload = Invoke-MultipartDocuments "Submit invalid enum type" $owner.Token $owner.Id @([pscustomobject]@{ Type = 999; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "invalid-type.jpg" })
    Assert-Test "Invalid document type returns 400" ($invalidTypeUpload.Status -eq 400) "status=$($invalidTypeUpload.Status)"
    $expiredUpload = Invoke-MultipartDocuments "Submit expired document" $partialOwner.Token $partialOwner.Id @([pscustomobject]@{ Type = 4; ExpirationDate = "2000-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "expired.jpg" })
    Assert-Test "Expired document is reported as a failed upload" ($expiredUpload.Status -eq 200 -and @($expiredUpload.Json.data.uploadedDocuments).Count -eq 0 -and @($expiredUpload.Json.data.failedDocuments).Count -eq 1)
    $wrongContentTypeUpload = Invoke-MultipartDocuments "Submit unsupported content type" $partialOwner.Token $partialOwner.Id @([pscustomobject]@{ Type = 5; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "application/octet-stream"; FileName = "hostile.exe" })
    Assert-Test "Unsupported content type is reported as a failed upload" ($wrongContentTypeUpload.Status -eq 200 -and @($wrongContentTypeUpload.Json.data.uploadedDocuments).Count -eq 0 -and @($wrongContentTypeUpload.Json.data.failedDocuments).Count -eq 1)
    foreach ($admin in $adminSessions) {
        $failedOnlyCount = Get-NotificationCountFor $admin.Token "verification.review-requested" @{ userId = $partialOwner.Id } "$($admin.Label) failed-only upload notification list"
        Assert-Test "$($admin.Label) receives no notification for failed-only uploads" ($failedOnlyCount -eq ($partialBefore[$admin.Label] + 1))
    }
    $longUserId = Invoke-MultipartDocuments "Submit extremely long UserId" $owner.Token ("x" * 10000) @([pscustomobject]@{ Type = 6; ExpirationDate = "2035-01-01"; Path = $validFile; ContentType = "image/jpeg"; FileName = "long-user.jpg" })
    Assert-Test "Extremely long UserId returns 400" ($longUserId.Status -eq 400) "status=$($longUserId.Status)"
    $malformedGet = Invoke-TestRequest "Get documents with malformed UserId" GET "/api/UserVerification/not-a-guid" -Token $owner.Token
    Assert-Test "Malformed UserId route returns 400 or 404" ($malformedGet.Status -in @(400, 404)) "status=$($malformedGet.Status)"
    $missingGet = Invoke-TestRequest "Get documents for unknown UserId" GET "/api/UserVerification/$unknownId" -Token $owner.Token
    Assert-Test "Unknown UserId returns 404" ($missingGet.Status -eq 404) "status=$($missingGet.Status)"
    $missingDelete = Invoke-TestRequest "Delete without document identifiers" DELETE "/api/UserVerification" -Token $owner.Token
    Assert-Test "Delete without identifiers returns 400" ($missingDelete.Status -eq 400) "status=$($missingDelete.Status)"
    $unknownDelete = Invoke-TestRequest "Delete unknown document" DELETE "/api/UserVerification?UserId=$($owner.Id)&DocumentId=$unknownId" -Token $owner.Token
    Assert-Test "Delete unknown document returns 404" ($unknownDelete.Status -eq 404) "status=$($unknownDelete.Status)"

    Write-Section "Notification list, unread count, read/read-all, exact payload, and recipient isolation"
    $primaryReview = Find-Notification $primaryAdmin.Token "verification.review-requested" @{ userId = $owner.Id; documentCount = "1" }
    $secondaryReview = Find-Notification $adminSessions[1].Token "verification.review-requested" @{ userId = $owner.Id; documentCount = "1" }
    Assert-Test "Primary Admin notification exists before read tests" ($null -ne $primaryReview)
    Assert-Test "Secondary Admin notification exists before isolation tests" ($null -ne $secondaryReview)
    if ($null -ne $primaryReview) {
        $primaryReadBySecondary = Invoke-TestRequest "Secondary Admin reads Primary Admin notification" PATCH "/api/notifications/$($primaryReview.id)/read" -Token $adminSessions[1].Token
        Assert-Test "Notification read is isolated between Admin recipients" ($primaryReadBySecondary.Status -eq 404) "status=$($primaryReadBySecondary.Status)"
    }
    $primaryItems = Get-NotificationItems $primaryAdmin.Token "Primary Admin notification list"
    $primaryReviewItems = @($primaryItems | Where-Object { $_.type -eq "verification.review-requested" })
    Assert-Test "Primary Admin inbox contains review-requested notifications" ($primaryReviewItems.Count -ge 1)
    foreach ($notification in $primaryReviewItems) {
        Assert-ExactReviewNotification "Primary Admin review notification has exact Arabic snapshot and safe data" $notification ([string]$notification.data.userId) ([int]$notification.data.documentCount)
    }
    $unread = Invoke-TestRequest "Primary Admin unread count" GET "/api/notifications/unread-count" -Token $primaryAdmin.Token
    Assert-Test "Unread count returns 200" ($unread.Status -eq 200) "status=$($unread.Status)"
    $beforeUnread = [int]$unread.Json.data.unreadCount
    if ($null -ne $primaryReview) {
        $read = Invoke-TestRequest "Primary Admin marks review notification read" PATCH "/api/notifications/$($primaryReview.id)/read" -Token $primaryAdmin.Token
        Assert-Test "Mark review notification read returns 200" ($read.Status -eq 200) "status=$($read.Status)"
        Assert-Test "Mark read returns a timestamp" ($null -ne $read.Json.data.readAtUtc)
        $readReplay = Invoke-TestRequest "Primary Admin repeats mark-read" PATCH "/api/notifications/$($primaryReview.id)/read" -Token $primaryAdmin.Token
        Assert-Test "Repeated mark-read remains 200" ($readReplay.Status -eq 200) "status=$($readReplay.Status)"
    }
    $afterRead = Invoke-TestRequest "Primary Admin unread count after mark-read" GET "/api/notifications/unread-count" -Token $primaryAdmin.Token
    Assert-Test "Unread count does not increase after mark-read" ([int]$afterRead.Json.data.unreadCount -le $beforeUnread)
    $readAll = Invoke-TestRequest "Primary Admin marks all notifications read" PATCH "/api/notifications/read-all" -Token $primaryAdmin.Token
    Assert-Test "Read-all returns 200" ($readAll.Status -eq 200) "status=$($readAll.Status)"
    $afterReadAll = Invoke-TestRequest "Primary Admin unread count after read-all" GET "/api/notifications/unread-count" -Token $primaryAdmin.Token
    Assert-Test "Read-all leaves no unread notifications" ([int]$afterReadAll.Json.data.unreadCount -eq 0)
    $readList = Invoke-TestRequest "Primary Admin lists read notifications" GET "/api/notifications?pageSize=50&isRead=true" -Token $primaryAdmin.Token
    Assert-Test "Read notification filter returns 200" ($readList.Status -eq 200) "status=$($readList.Status)"
    $badPage = Invoke-TestRequest "Notification list invalid page size" GET "/api/notifications?pageSize=0" -Token $primaryAdmin.Token
    Assert-Test "Invalid notification page size returns 400" ($badPage.Status -eq 400) "status=$($badPage.Status)"
    $badCursor = Invoke-TestRequest "Notification list invalid cursor" GET "/api/notifications?pageSize=10&cursor=not-a-valid-cursor" -Token $primaryAdmin.Token
    Assert-Test "Invalid notification cursor returns 400" ($badCursor.Status -eq 400) "status=$($badCursor.Status)"
    $emptyRead = Invoke-TestRequest "Notification read empty ID" PATCH "/api/notifications/00000000-0000-0000-0000-000000000000/read" -Token $primaryAdmin.Token
    Assert-Test "Notification read empty ID returns 404" ($emptyRead.Status -eq 404) "status=$($emptyRead.Status)"

    $ownerFinalInbox = Get-NotificationItems $owner.Token "Owner final notification list"
    Assert-Test "Owner final inbox has no Admin review request" (@($ownerFinalInbox | Where-Object { $_.type -eq "verification.review-requested" }).Count -eq 0)
    $unrelatedFinalInbox = Get-NotificationItems $unrelated.Token "Unrelated final notification list"
    Assert-Test "Unrelated final inbox has no review request" (@($unrelatedFinalInbox | Where-Object { $_.type -eq "verification.review-requested" }).Count -eq 0)
}
catch {
    $message = Protect-ReportText $_.Exception.Message
    [void]$script:fatal.Add($message)
    [void]($script:failed++)
    Add-Report "`n### Fatal test error`n$message`n"
}
finally {
    Write-Section "API and mock Email log monitoring"
    try { Assert-LogsClean } catch {
        [void]($script:failed++)
        Add-Report "- [FAIL] **Log monitor execution** ($(Protect-ReportText $_.Exception.Message))"
    }

    Stop-ApiIfOwned
    $released = if ($script:ownsApi) { Test-PortReleased } else { $true }
    Assert-Test "API test port is released after owned process shutdown" $released
    if (-not $script:ownsApi) {
        Add-Skip "API shutdown ownership" "An externally running API was detected; no external process was terminated."
    }

    foreach ($path in $script:temporaryFiles) {
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
        }
    }
    if (Test-Path $script:apiStdOut) { Remove-Item -LiteralPath $script:apiStdOut -Force -ErrorAction SilentlyContinue }
    if (Test-Path $script:apiStdErr) { Remove-Item -LiteralPath $script:apiStdErr -Force -ErrorAction SilentlyContinue }

    $localStorageFullPath = [IO.Path]::GetFullPath($script:localStoragePath)
    $uploadsRoot = [IO.Path]::GetFullPath((Join-Path $apiProjectDir "uploads"))
    $localStorageIsScoped = $localStorageFullPath.StartsWith(
        "$uploadsRoot$([IO.Path]::DirectorySeparatorChar)",
        [StringComparison]::OrdinalIgnoreCase)
    if ((Test-Path -LiteralPath $localStorageFullPath) -and $localStorageIsScoped) {
        Remove-Item -LiteralPath $localStorageFullPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Section "Execution summary"
    Add-Report "| Metric | Count |`n|---|---:|`n| Passed assertions | $script:passed |`n| Failed assertions | $script:failed |`n| Documented skips | $script:skipped |"
    if ($script:fatal.Count -gt 0) {
        Add-Report "`n### Fatal errors`n"
        foreach ($errorText in $script:fatal) { Add-Report "- $errorText" }
    }
    Write-Host "User verification notification HTTP tests complete: $script:passed passed, $script:failed failed, $script:skipped skipped."
}

if ($script:failed -gt 0) { exit 1 }
