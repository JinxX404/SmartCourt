param(
    [Alias("BaseUrl")]
    [string]$ApiBaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [switch]$UseExistingApi
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..\..")).Path
$apiProjectDir = Join-Path $repoRoot "SmartCourt"
$apiDllPath = Join-Path $apiProjectDir "bin\Debug\net8.0\SmartCourt.dll"
$script:localStoragePath = Join-Path $apiProjectDir ("uploads\gate7-" + [guid]::NewGuid().ToString("N"))
if ([string]::IsNullOrWhiteSpace($ApiLogPath)) { $ApiLogPath = Join-Path $apiProjectDir "api_log.txt" }
if ([string]::IsNullOrWhiteSpace($ReportFile)) { $ReportFile = Join-Path $scriptDir "AuthSecurityNotifications_Report.md" }

Import-Module (Join-Path $scriptDir "TestHelpers.psm1") -Force
Add-Type -AssemblyName System.Net.Http

$script:reportFile = $ReportFile
$script:apiBaseUrl = $ApiBaseUrl.TrimEnd("/")
$script:apiLogPath = $ApiLogPath
$script:useExistingApi = $UseExistingApi
$script:passed = 0
$script:failed = 0
$script:skipped = 0
$script:fatal = [System.Collections.Generic.List[string]]::new()
$script:apiProcess = $null
$script:ownsApi = $false
$script:apiLogStartLength = 0
$script:apiStdOut = Join-Path ([IO.Path]::GetTempPath()) ("smartcourt-gate7-" + [guid]::NewGuid().ToString("N") + ".out.log")
$script:apiStdErr = Join-Path ([IO.Path]::GetTempPath()) ("smartcourt-gate7-" + [guid]::NewGuid().ToString("N") + ".err.log")
$script:confirmedEmails = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$script:resetEmails = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

("# Authentication/Security Notifications HTTP Test Report" + [Environment]::NewLine + [Environment]::NewLine + "Generated at: " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")) | Out-File $script:reportFile -Encoding utf8

function Add-Report { param([string]$Text) $Text | Out-File $script:reportFile -Append -Encoding utf8 }
function Section { param([string]$Title) Add-Report ([Environment]::NewLine + "## " + $Title + [Environment]::NewLine) }

function Protect-Text {
    param([AllowNull()][string]$Text)
    if ($null -eq $Text) { return "" }
    $value = $Text
    $value = [regex]::Replace($value, '(?i)(accessToken|refreshToken|confirmationToken|resetToken|token|password|secret|apiKey)=([^&\s]+)', '$1=[REDACTED]')
    $value = [regex]::Replace($value, '(?i)Bearer\s+[A-Za-z0-9\-_\.]+', 'Bearer [REDACTED]')
    $value = [regex]::Replace($value, '(?i)([a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,})', '[REDACTED_EMAIL]')
    $value = [regex]::Replace($value, '(?<![0-9])[0-9]{10,14}(?![0-9])', '[REDACTED_NUMBER]')
    return $value
}

function Sensitive-Key {
    param([string]$Key)
    return $Key -match '(?i)(password|token|secret|api.?key|email|phone|national|authorization|cookie|ip.?address|device.?fingerprint|security.?stamp|file.?url|storage.?path|provider.?id|idempotency.?key)'
}

function Redact-Value {
    param($Value, [string]$Key = "")
    if (Sensitive-Key $Key) { return "[REDACTED]" }
    if ($null -eq $Value) { return $null }
    if ($Value -is [string]) { return Protect-Text $Value }
    if ($Value -is [System.Collections.IDictionary]) {
        $result = [ordered]@{}
        foreach ($entry in $Value.GetEnumerator()) { $result[$entry.Key] = Redact-Value $entry.Value ([string]$entry.Key) }
        return $result
    }
    if ($Value -is [pscustomobject]) {
        $result = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) { $result[$property.Name] = Redact-Value $property.Value $property.Name }
        return $result
    }
    if ($Value -is [System.Collections.IEnumerable]) { return ,@($Value | ForEach-Object { Redact-Value $_ }) }
    return $Value
}

function Redact-Text {
    param([AllowNull()][string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return "" }
    try { return (Redact-Value ($Text | ConvertFrom-Json -ErrorAction Stop) | ConvertTo-Json -Depth 100) }
    catch { return Protect-Text $Text }
}

function Parse-Json {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return $null }
    try { return $Text | ConvertFrom-Json -ErrorAction Stop } catch { return $null }
}

function Assert-Test {
    param([string]$Name, [bool]$Condition, [string]$Details = "")
    if ($Condition) {
        [void]($script:passed++)
        Add-Report ("- [PASS] **" + (Protect-Text $Name) + "**" + $(if ($Details) { " (" + $Details + ")" } else { "" }))
    }
    else {
        [void]($script:failed++)
        Add-Report ("- [FAIL] **" + (Protect-Text $Name) + "**" + $(if ($Details) { " (" + $Details + ")" } else { "" }))
    }
}

function Skip-Test { param([string]$Name, [string]$Reason) [void]($script:skipped++); Add-Report ("- [SKIP] **" + (Protect-Text $Name) + "** — " + (Protect-Text $Reason)) }

function Write-Exchange {
    param([string]$Title, [string]$Method, [string]$Url, [string]$Body, [int]$Status, [string]$ResponseBody)
    Add-Report ("### " + (Protect-Text $Title))
    Add-Report ("**Request:** " + $Method + " " + (Protect-Text $Url))
    if (-not [string]::IsNullOrWhiteSpace($Body)) { Add-Report ("<pre>" + (Redact-Text $Body) + "</pre>") }
    Add-Report ("**Response Status:** " + $Status)
    if ([string]::IsNullOrWhiteSpace($ResponseBody)) { Add-Report "**Response Body:** (Empty)" }
    else { Add-Report ("<pre>" + (Redact-Text $ResponseBody) + "</pre>") }
    Add-Report "---"
}

function Request {
    param([string]$Title, [string]$Method, [string]$Path, [string]$Token = "", [string]$Body = "", [string]$ContentType = "application/json", [switch]$NoReport)
    $url = if ($Path -match "^https?://") { $Path } else { $script:apiBaseUrl + $Path }
    $client = [Net.Http.HttpClient]::new()
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($Method.ToUpperInvariant()), $url)
    try {
        if ($Token) { $request.Headers.Authorization = [Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token) }
        if ($Body) {
            if ([string]::IsNullOrWhiteSpace($ContentType)) {
                $request.Content = [Net.Http.StringContent]::new($Body, [Text.Encoding]::UTF8)
                $request.Content.Headers.ContentType = $null
            }
            else {
                $request.Content = [Net.Http.StringContent]::new($Body, [Text.Encoding]::UTF8, $ContentType)
            }
        }
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $raw = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        if (-not $NoReport) { Write-Exchange $Title $Method $url $Body $status $raw }
        return [pscustomobject]@{ Status = $status; Json = (Parse-Json $raw); Raw = $raw }
    }
    catch {
        if (-not $NoReport) { Write-Exchange $Title $Method $url $Body 0 $_.Exception.Message }
        return [pscustomobject]@{ Status = 0; Json = $null; Raw = $_.Exception.Message }
    }
    finally {
        if ($request.Content) { $request.Content.Dispose() }
        $request.Dispose()
        $client.Dispose()
    }
}

function Status-Is { param($Response, [int[]]$Expected) return $Expected -contains [int]$Response.Status }
function Assert-Status { param([string]$Name, $Response, [int[]]$Expected) Assert-Test $Name (Status-Is $Response $Expected) ("status=" + $Response.Status) }
function Assert-4xx { param([string]$Name, $Response) Assert-Test $Name ($Response.Status -ge 400 -and $Response.Status -lt 500) ("status=" + $Response.Status) }

function Full-Log {
    $paths = @($script:apiLogPath, $script:apiStdOut, $script:apiStdErr)
    $parts = foreach ($path in $paths) {
        if (-not (Test-Path $path)) { continue }
        try {
            $stream = [IO.FileStream]::new($path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
            try {
                $reader = [IO.StreamReader]::new($stream)
                try { $reader.ReadToEnd() }
                finally { $reader.Dispose() }
            }
            finally { $stream.Dispose() }
        }
        catch { }
    }
    return ($parts -join [Environment]::NewLine)
}

function Latest-Link {
    param([string]$Email, [string]$Fragment)
    $pattern = '(?is)To:\s*' + [regex]::Escape($Email) + '.*?href=[''"]([^''"]*' + [regex]::Escape($Fragment) + '[^''"]*)[''"]'
    $deadline = (Get-Date).AddSeconds(45)
    do {
        $matches = [regex]::Matches((Full-Log), $pattern)
        if ($matches.Count -gt 0) { return [Net.WebUtility]::HtmlDecode($matches[$matches.Count - 1].Groups[1].Value) }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    return ""
}

function Query-Values {
    param([string]$Url)
    $values = @{}
    foreach ($part in ([Uri]$Url).Query.TrimStart("?").Split("&")) {
        if (-not $part) { continue }
        $separator = $part.IndexOf("=")
        if ($separator -lt 0) { continue }
        $values[[Uri]::UnescapeDataString($part.Substring(0, $separator))] = [Uri]::UnescapeDataString($part.Substring($separator + 1))
    }
    return $values
}

function Confirm-AccountEmail {
    param([string]$Email, [string]$ExpectedUserId, [string]$Label)
    $link = Latest-Link $Email "verify-email"
    Assert-Test ($Label + " confirmation link is in mock Email log") ($link -ne "")
    if (-not $link) { throw "No Email confirmation link found." }
    $query = Query-Values $link
    $userId = if ($query.ContainsKey("userId")) { [string]$query["userId"] } else { $ExpectedUserId }
    $token = [string]$query["token"]
    $response = Request ($Label + " confirms Email from mock log") GET ("/api/auth/confirm-email?userId=" + $userId + "&token=" + [Uri]::EscapeDataString($token))
    Assert-Status ($Label + " Email confirmation returns 200") $response @(200)
    if ($response.Status -ne 200) { throw "Email confirmation failed." }
    [void]$script:confirmedEmails.Add($Email)
    return [pscustomobject]@{ Url = $link; UserId = $userId; Token = $token }
}

function Register-Account {
    param([string]$Kind, [string]$Label, [switch]$SkipConfirmation)
    $suffix = [guid]::NewGuid().ToString("N").Substring(0, 12)
    $email = "gate7_" + $Kind.ToLowerInvariant() + "_" + $suffix + "@example.com"
    $password = "Gate7-" + $suffix + "!Aa9"
    $endpoint = if ($Kind -eq "Lawyer") { "/api/auth/register/lawyer" } else { "/api/auth/register/client" }
    $body = @{ FullName = "Gate 7 " + $Label; Email = $email; Password = $password; ConfirmPassword = $password } | ConvertTo-Json
    $registered = Request ($Label + " registration") POST $endpoint -Body $body
    Assert-Status ($Label + " registration returns 201") $registered @(201)
    $id = [string]$registered.Json.data.userId
    Assert-Test ($Label + " registration returns user id") ($registered.Status -eq 201 -and $id)
    if ($registered.Status -ne 201 -or -not $id) { throw "Disposable registration failed." }
    $account = [pscustomobject]@{ Id = $id; Email = $email; Password = $password; Confirmation = $null; Token = ""; RefreshToken = "" }
    if (-not $SkipConfirmation) {
        $account.Confirmation = Confirm-AccountEmail $email $id $Label
        Login-Account $Label $account | Out-Null
    }
    return $account
}

function Login-Account {
    param([string]$Label, $Account)
    $body = @{ Email = $Account.Email; Password = $Account.Password } | ConvertTo-Json
    $login = Request ($Label + " login") POST "/api/auth/login" -Body $body
    Assert-Status ($Label + " login returns 200") $login @(200)
    if ($login.Status -ne 200) { throw "Login failed." }
    $Account.Token = [string]$login.Json.data.accessToken
    $Account.RefreshToken = [string]$login.Json.data.refreshToken
    return $login
}

function Sms-Code {
    param([string]$Phone)
    $pattern = '(?is)--- MOCK SMS ---.*?To:\s*' + [regex]::Escape($Phone) + '.*?Message:.*?:\s*(\d+)'
    $deadline = (Get-Date).AddSeconds(45)
    do {
        $matches = [regex]::Matches((Full-Log), $pattern)
        if ($matches.Count -gt 0) { return $matches[$matches.Count - 1].Groups[1].Value }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    return ""
}

function Set-ApiEnvironment {
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

function Api-Healthy { return ((Request "health probe" GET "/health" -NoReport).Status -eq 200) }

function Start-Api {
    if (Api-Healthy) {
        if (-not $script:useExistingApi) { throw "An API is already listening on the test port." }
        Skip-Test "API lifecycle" "An existing API was explicitly supplied."
        return
    }
    if (-not (Test-Path $apiDllPath)) { throw "Build SmartCourt before running the HTTP test." }
    $names = @("ASPNETCORE_ENVIRONMENT","ConnectionStrings__DefaultConnection","AuthEmail__PublicBaseUrl","FileStorage__Provider","FileStorage__BasePath","Supabase__Url","Supabase__ApiKey","Supabase__Bucket","SmtpSettings__Server","SmtpSettings__Port","OutboxDispatch__Enabled")
    $old = @{}
    foreach ($name in $names) { $old[$name] = [Environment]::GetEnvironmentVariable($name) }
    try {
        Set-ApiEnvironment
        $parameters = @{ FilePath = "dotnet"; ArgumentList = ('"' + $apiDllPath + '" --urls ' + $script:apiBaseUrl); WorkingDirectory = $apiProjectDir; RedirectStandardOutput = $script:apiStdOut; RedirectStandardError = $script:apiStdErr; WindowStyle = "Hidden"; PassThru = $true }
        $script:apiProcess = Start-Process @parameters
        $script:ownsApi = $true
    }
    finally { foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $old[$name]) } }
    $deadline = (Get-Date).AddSeconds(90)
    do { if (Api-Healthy) { return }; Start-Sleep -Milliseconds 750 } while ((Get-Date) -lt $deadline)
    throw "API did not become healthy."
}

function Stop-Api {
    if ($script:ownsApi -and $null -ne $script:apiProcess -and -not $script:apiProcess.HasExited) {
        Stop-Process -Id $script:apiProcess.Id -Force -ErrorAction SilentlyContinue
        $script:apiProcess.WaitForExit(15000)
    }
}

function Port-Free {
    try {
        $uri = [Uri]$script:apiBaseUrl
        $tcp = [Net.Sockets.TcpClient]::new()
        try { $task = $tcp.ConnectAsync($uri.Host, $uri.Port); return -not ($task.Wait(750) -and $tcp.Connected) }
        finally { $tcp.Dispose() }
    }
    catch { return $true }
}

function Notification-Items {
    param([string]$Token, [string]$Label, [string]$Query = "?pageSize=50")
    $response = Request $Label GET ("/api/notifications" + $Query) -Token $Token
    Assert-Status ($Label + " returns 200") $response @(200)
    if ($response.Status -ne 200) { return @() }
    return @($response.Json.data.items)
}

function Wait-Notification {
    param([string]$Token, [string]$Type, [string]$UserId)
    $deadline = (Get-Date).AddSeconds(75)
    do {
        $response = Request ("poll " + $Type) GET "/api/notifications?pageSize=50" -Token $Token -NoReport
        if ($response.Status -eq 200) {
            foreach ($item in @($response.Json.data.items)) {
                if ($item.type -eq $Type -and $null -ne $item.data -and [string]$item.data.userId -eq $UserId) { return $item }
            }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Assert-SecurityNotification {
    param([string]$Label, $Item, [string]$Type, [string]$Title, [string]$Body, [string]$UserId)
    $valid = $null -ne $Item -and $Item.type -eq $Type -and $Item.severity -eq "Critical" -and $Item.title -eq $Title -and $Item.body -eq $Body -and $null -eq $Item.actionUrl
    if ($valid) { $valid = @($Item.data.PSObject.Properties.Name).Count -eq 1 -and [string]$Item.data.userId -eq $UserId }
    $forbidden = @("email","phone","ipAddress","deviceFingerprint","securityStamp","accessToken","refreshToken","token","password","secret","providerId","idempotencyKey","resetToken")
    $keys = if ($null -ne $Item -and $null -ne $Item.data) { @($Item.data.PSObject.Properties.Name) } else { @() }
    Assert-Test $Label ($valid -and @($keys | Where-Object { $_ -in $forbidden }).Count -eq 0)
}

function Logs-Clean {
    $parts = @()
    if (Test-Path $script:apiLogPath) {
        $log = Get-Content $script:apiLogPath -Raw -ErrorAction SilentlyContinue
        $parts += if ($log.Length -ge $script:apiLogStartLength) { $log.Substring($script:apiLogStartLength) } else { $log }
    }
    if (Test-Path $script:apiStdOut) { $parts += Get-Content $script:apiStdOut -Raw -ErrorAction SilentlyContinue }
    if (Test-Path $script:apiStdErr) { $parts += Get-Content $script:apiStdErr -Raw -ErrorAction SilentlyContinue }
    $log = $parts -join [Environment]::NewLine
    $patterns = @('(?i)notification\s+(dispatch|outbox)[^\r\n]*(error|fail|exception)','(?i)outbox\s+(dispatch|handler)[^\r\n]*(error|fail|exception)','(?i)no outbox handler is registered','(?i)failed to send (email|sms)','(?i)(provider|smtp)[^\r\n]*(error|fail|exception)')
    $bad = @()
    $expected = '(?i)(ValidationException|AuthenticationException|BusinessException|NotFoundException|ConflictException|ForbiddenAccessException|PreconditionFailedException|TooManyRequestsException|PayloadTooLargeException)'
    foreach ($match in [regex]::Matches($log, '(?is)An unhandled exception has occurred\..{0,2500}')) { if ($match.Value -notmatch $expected) { $bad += $match.Value } }
    foreach ($pattern in $patterns) { $bad += [regex]::Matches($log, $pattern) | ForEach-Object { $_.Value } }
    Assert-Test "API/outbox/notification/provider logs are clean" ($bad.Count -eq 0) ("violations=" + $bad.Count)
    foreach ($email in $script:confirmedEmails) { Assert-Test "Mock Email confirmation was recorded" ($log -match "(?is)To:\s*$([regex]::Escape($email)).*?/verify-email") }
    foreach ($email in $script:resetEmails) { Assert-Test "Mock Email reset receipt was recorded" ($log -match "(?is)To:\s*$([regex]::Escape($email)).*?/auth/reset-password") }
}

try {
    if (-not (Test-Path $script:apiLogPath)) { New-Item -Path $script:apiLogPath -ItemType File -Force | Out-Null }
    $script:apiLogStartLength = (Get-Content $script:apiLogPath -Raw -ErrorAction SilentlyContinue).Length
    Start-Api

    Section "Health and anonymous/authenticated boundaries"
    Assert-Status "Health returns 200" (Request "GET health" GET "/health") @(200)
    Assert-Status "Health ping returns 200" (Request "GET health ping" GET "/api/health/ping") @(200)
    Assert-4xx "Missing content type is rejected" (Request "Missing content type" POST "/api/auth/login" -ContentType "" -Body (@{ Email = "invalid@example.com"; Password = "Gate7-Invalid123!" } | ConvertTo-Json))
    Assert-4xx "Invalid content type is rejected" (Request "Invalid content type" POST "/api/auth/login" -ContentType "text/plain" -Body (@{ Email = "invalid@example.com"; Password = "Gate7-Invalid123!" } | ConvertTo-Json))
    $changeAnon = Request "Anonymous change-password" POST "/api/auth/change-password" -Body (@{ CurrentPassword = "x"; NewPassword = "Gate7-New123!"; ConfirmNewPassword = "Gate7-New123!" } | ConvertTo-Json)
    Assert-Status "Anonymous change-password returns 401" $changeAnon @(401)
    $phoneAnon = Request "Anonymous phone send-token" POST "/api/auth/phone/send-token" -Body (@{ PhoneNumber = "+201011111111" } | ConvertTo-Json)
    Assert-Status "Anonymous phone send-token returns 401" $phoneAnon @(401)
    $refreshMissing = Request "Refresh missing token" POST "/api/auth/refresh" -Body "{}"
    Assert-Status "Refresh missing token returns 401" $refreshMissing @(401)
    Assert-4xx "Revoke missing tokens returns a controlled client error" (Request "Revoke missing tokens" POST "/api/auth/revoke" -Body "{}")

    Section "Registration, confirmation, resend, and login"
    $badClient = @(
        @{ Name = "Client registration missing name"; Body = @{ Email = "gate7_validation_" + [guid]::NewGuid().ToString("N") + "@example.com"; Password = "Gate7-Abc123!"; ConfirmPassword = "Gate7-Abc123!" } },
        @{ Name = "Client registration invalid Email"; Body = @{ FullName = "Valid Client"; Email = "bad"; Password = "Gate7-Abc123!"; ConfirmPassword = "Gate7-Abc123!" } },
        @{ Name = "Client registration weak password"; Body = @{ FullName = "Valid Client"; Email = "gate7_weak_" + [guid]::NewGuid().ToString("N") + "@example.com"; Password = "weak"; ConfirmPassword = "weak" } },
        @{ Name = "Client registration mismatched password"; Body = @{ FullName = "Valid Client"; Email = "gate7_mismatch_" + [guid]::NewGuid().ToString("N") + "@example.com"; Password = "Gate7-Abc123!"; ConfirmPassword = "Gate7-Abc124!" } },
        @{ Name = "Client registration hostile SQL/XSS name"; Body = @{ FullName = "'; DROP TABLE Users; -- <script>alert(1)</script>"; Email = "gate7_hostile_" + [guid]::NewGuid().ToString("N") + "@example.com"; Password = "Gate7-Abc123!"; ConfirmPassword = "Gate7-Abc123!" } }
    )
    foreach ($case in $badClient) {
        $caseResponse = Request $case.Name POST "/api/auth/register/client" -Body ($case.Body | ConvertTo-Json)
        if ($case.Name -eq "Client registration hostile SQL/XSS name") {
            Assert-Test "Hostile registration text is handled without server error" ($caseResponse.Status -ge 200 -and $caseResponse.Status -lt 500) ("status=" + $caseResponse.Status)
        }
        else { Assert-4xx $case.Name $caseResponse }
    }
    Assert-4xx "Lawyer registration validation rejects malformed payload" (Request "Malformed lawyer registration" POST "/api/auth/register/lawyer" -Body (@{ FullName = "x"; Email = "bad"; Password = "weak"; ConfirmPassword = "different" } | ConvertTo-Json))

    $client = Register-Account "Client" "primary client"
    $lawyer = Register-Account "Lawyer" "lawyer resend" -SkipConfirmation
    $resend = Request "Resend verification before confirmation" POST "/api/auth/resend-verification" -Body (@{ Email = $lawyer.Email } | ConvertTo-Json)
    Assert-Status "Resend verification returns 200" $resend @(200)
    $lawyer.Confirmation = Confirm-AccountEmail $lawyer.Email $lawyer.Id "lawyer resend"
    Login-Account "Lawyer after resend" $lawyer | Out-Null
    $unconfirmed = Register-Account "Client" "unconfirmed account" -SkipConfirmation
    Assert-Status "Unconfirmed login returns 403" (Request "Login before Email confirmation" POST "/api/auth/login" -Body (@{ Email = $unconfirmed.Email; Password = $unconfirmed.Password } | ConvertTo-Json)) @(403)
    Assert-Status "Unconfirmed resend returns 200" (Request "Resend unconfirmed account" POST "/api/auth/resend-verification" -Body (@{ Email = $unconfirmed.Email } | ConvertTo-Json)) @(200)
    $unconfirmed.Confirmation = Confirm-AccountEmail $unconfirmed.Email $unconfirmed.Id "unconfirmed account"
    Login-Account "Confirmed account" $unconfirmed | Out-Null
    Assert-Status "Duplicate registration returns 409" (Request "Duplicate client registration" POST "/api/auth/register/client" -Body (@{ FullName = "Duplicate"; Email = $client.Email; Password = "Gate7-Dup123!"; ConfirmPassword = "Gate7-Dup123!" } | ConvertTo-Json)) @(409)
    $replayConfirm = Request "Replay Email confirmation" GET ("/api/auth/confirm-email?userId=" + $client.Confirmation.UserId + "&token=" + [Uri]::EscapeDataString($client.Confirmation.Token))
    Assert-4xx "Replayed Email confirmation is rejected" $replayConfirm
    Assert-Status "Confirmed resend remains 200" (Request "Resend confirmed account" POST "/api/auth/resend-verification" -Body (@{ Email = $client.Email } | ConvertTo-Json)) @(200)
    Assert-4xx "Malformed confirmation query is rejected" (Request "Malformed confirmation query" GET "/api/auth/confirm-email?userId=bad&token=%%%")

    Section "Login and password-change notification"
    Assert-Status "Invalid login returns 401" (Request "Invalid login" POST "/api/auth/login" -Body (@{ Email = $client.Email; Password = "Wrong-Gate7-123!" } | ConvertTo-Json)) @(401)
    Assert-Status "Login validator returns 400" (Request "Login validator" POST "/api/auth/login" -Body (@{ Email = "bad"; Password = "x" } | ConvertTo-Json)) @(400)
    $beforeChange = @(Notification-Items $client.Token "Inbox before change" | Where-Object { $_.type -eq "security.password-changed" })
    Assert-4xx "Wrong current password is rejected" (Request "Wrong current password" POST "/api/auth/change-password" -Token $client.Token -Body (@{ CurrentPassword = "Wrong-Gate7-123!"; NewPassword = "Gate7-New123!"; ConfirmNewPassword = "Gate7-New123!" } | ConvertTo-Json))
    Assert-4xx "Password reuse is rejected" (Request "Password reuse" POST "/api/auth/change-password" -Token $client.Token -Body (@{ CurrentPassword = $client.Password; NewPassword = $client.Password; ConfirmNewPassword = $client.Password } | ConvertTo-Json))
    Assert-Status "Weak new password is rejected" (Request "Weak change password" POST "/api/auth/change-password" -Token $client.Token -Body (@{ CurrentPassword = $client.Password; NewPassword = "weak"; ConfirmNewPassword = "weak" } | ConvertTo-Json)) @(400)
    Assert-Status "Mismatched new password is rejected" (Request "Mismatched change password" POST "/api/auth/change-password" -Token $client.Token -Body (@{ CurrentPassword = $client.Password; NewPassword = "Gate7-New123!"; ConfirmNewPassword = "Gate7-New124!" } | ConvertTo-Json)) @(400)
    $oldChangeAccess = $client.Token
    $oldChangeRefresh = $client.RefreshToken
    $oldChangePassword = $client.Password
    $client.Password = "Gate7-New123!"
    Assert-Status "Successful change-password returns 200" (Request "Successful change password" POST "/api/auth/change-password" -Token $oldChangeAccess -Body (@{ CurrentPassword = $oldChangePassword; NewPassword = $client.Password; ConfirmNewPassword = $client.Password } | ConvertTo-Json)) @(200)
    Assert-Status "Password-change old refresh token is revoked" (Request "Refresh after password change" POST "/api/auth/refresh" -Body (@{ RefreshToken = $oldChangeRefresh } | ConvertTo-Json)) @(401)
    Assert-Status "Password-change old access token is revoked" (Request "Old access after password change" GET "/api/notifications?pageSize=50" -Token $oldChangeAccess) @(401)
    Login-Account "Client after password change" $client | Out-Null
    $changed = Wait-Notification $client.Token "security.password-changed" $client.Id
    $changedItems = @(Notification-Items $client.Token "Password-change notification list" | Where-Object { $_.type -eq "security.password-changed" })
    Assert-Test "Successful password change creates exactly one notification" ($changedItems.Count -eq 1) ("count=" + $changedItems.Count)
    if ($changedItems.Count -eq 1) { Assert-SecurityNotification "Password-change notification exact Arabic/safe contract" $changedItems[0] "security.password-changed" "تم تغيير كلمة المرور" "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم." $client.Id }
    if ($changedItems.Count -eq 0) { throw "Password-change notification was not persisted." }
    $changedId = [string]$changedItems[0].id
    $unread = Request "Unread count after password change" GET "/api/notifications/unread-count" -Token $client.Token
    Assert-Status "Unread count after password change returns 200" $unread @(200)
    Assert-Test "Password-change notification starts unread" ([int]$unread.Json.data.unreadCount -ge 1)
    Assert-Status "Read password-change notification returns 200" (Request "Read password-change notification" PATCH ("/api/notifications/" + $changedId + "/read") -Token $client.Token) @(200)
    Assert-Status "Replay read password-change notification returns 200" (Request "Replay read password-change notification" PATCH ("/api/notifications/" + $changedId + "/read") -Token $client.Token) @(200)
    Assert-Status "Read-all after password change returns 200" (Request "Read-all password-change notification" PATCH "/api/notifications/read-all" -Token $client.Token) @(200)
    $readItems = Notification-Items $client.Token "Read notification filter" "?pageSize=50&isRead=true"
    Assert-Test "Read filter contains password-change notification" (@($readItems | Where-Object { $_.id -eq $changedId }).Count -eq 1)
    Assert-4xx "Malformed notification id is rejected" (Request "Malformed notification read id" PATCH "/api/notifications/not-a-guid/read" -Token $client.Token)
    Assert-Status "Unknown notification id returns 404" (Request "Unknown notification read id" PATCH "/api/notifications/00000000-0000-0000-0000-000000000000/read" -Token $client.Token) @(404)
    Assert-Test "Unrelated lawyer receives no password-change notification" (@(Notification-Items $lawyer.Token "Lawyer isolation inbox" | Where-Object { $_.type -eq "security.password-changed" }).Count -eq 0)

    Section "Forgot-password and password-reset notification"
    Assert-Status "Forgot-password validator returns 400" (Request "Forgot validator" POST "/api/auth/forgot-password" -Body (@{ Email = "bad" } | ConvertTo-Json)) @(400)
    Assert-Status "Forgot-password unknown account returns generic 200" (Request "Forgot unknown account" POST "/api/auth/forgot-password" -Body (@{ Email = "missing-" + [guid]::NewGuid().ToString("N") + "@example.com" } | ConvertTo-Json)) @(200)
    Assert-Status "Forgot-password valid account returns 200" (Request "Forgot valid account" POST "/api/auth/forgot-password" -Body (@{ Email = $client.Email } | ConvertTo-Json)) @(200)
    [void]$script:resetEmails.Add($client.Email)
    $resetLink = Latest-Link $client.Email "auth/reset-password"
    Assert-Test "Reset link is present in mock Email log" ($resetLink -ne "")
    if (-not $resetLink) { throw "Reset link not found." }
    $resetToken = [string](Query-Values $resetLink)["token"]
    Assert-Test "Reset link contains a token" ($resetToken -ne "")
    Assert-4xx "Malformed reset token is rejected" (Request "Malformed reset token" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = "%%%"; NewPassword = "Gate7-Reset123!"; ConfirmNewPassword = "Gate7-Reset123!" } | ConvertTo-Json))
    Assert-4xx "Oversized reset token is rejected" (Request "Oversized reset token" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = ("a" * 2049); NewPassword = "Gate7-Reset123!"; ConfirmNewPassword = "Gate7-Reset123!" } | ConvertTo-Json))
    Assert-Status "Weak reset password is rejected" (Request "Weak reset password" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = $resetToken; NewPassword = "weak"; ConfirmNewPassword = "weak" } | ConvertTo-Json)) @(400)
    Assert-Status "Mismatched reset password is rejected" (Request "Mismatched reset password" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = $resetToken; NewPassword = "Gate7-Reset123!"; ConfirmNewPassword = "Gate7-Reset124!" } | ConvertTo-Json)) @(400)
    $oldResetAccess = $client.Token
    $oldResetRefresh = $client.RefreshToken
    $client.Password = "Gate7-Reset123!"
    Assert-Status "Successful reset-password returns 200" (Request "Successful reset password" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = $resetToken; NewPassword = $client.Password; ConfirmNewPassword = $client.Password } | ConvertTo-Json)) @(200)
    Assert-Status "Password-reset old refresh token is revoked" (Request "Refresh after password reset" POST "/api/auth/refresh" -Body (@{ RefreshToken = $oldResetRefresh } | ConvertTo-Json)) @(401)
    Assert-Status "Password-reset old access token is revoked" (Request "Old access after password reset" GET "/api/notifications?pageSize=50" -Token $oldResetAccess) @(401)
    Assert-4xx "Replayed reset token is rejected" (Request "Replay reset token" POST "/api/auth/reset-password" -Body (@{ Email = $client.Email; Token = $resetToken; NewPassword = "Gate7-Replay123!"; ConfirmNewPassword = "Gate7-Replay123!" } | ConvertTo-Json))
    Login-Account "Client after password reset" $client | Out-Null
    $resetItem = Wait-Notification $client.Token "security.password-reset" $client.Id
    $resetItems = @(Notification-Items $client.Token "Password-reset notification list" | Where-Object { $_.type -eq "security.password-reset" })
    Assert-Test "Successful password reset creates exactly one notification" ($resetItems.Count -eq 1) ("count=" + $resetItems.Count)
    if ($resetItems.Count -eq 1) { Assert-SecurityNotification "Password-reset notification exact Arabic/safe contract" $resetItems[0] "security.password-reset" "تمت إعادة تعيين كلمة المرور" "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم." $client.Id }
    if ($resetItems.Count -eq 0) { throw "Password-reset notification was not persisted." }
    Assert-Test "Replayed reset token creates no second notification" (@(Notification-Items $client.Token "Reset notification replay list" | Where-Object { $_.type -eq "security.password-reset" }).Count -eq 1)
    Skip-Test "True expired reset token" "Identity lifespan is fixed at one hour and no safe HTTP time-advance control exists; invalid, oversized, and replayed paths are covered."

    Section "Refresh rotation and revoke"
    $oldRotationRefresh = $client.RefreshToken
    $rotation = Request "Successful refresh rotation" POST "/api/auth/refresh" -Body (@{ RefreshToken = $oldRotationRefresh } | ConvertTo-Json)
    Assert-Status "Refresh rotation returns 200" $rotation @(200)
    $rotationAccess = [string]$rotation.Json.data.accessToken
    $rotationRefresh = [string]$rotation.Json.data.refreshToken
    Assert-Test "Refresh rotation returns new access and refresh values" ($rotationAccess -and $rotationRefresh)
    $revoke = Request "Revoke active refresh" POST "/api/auth/revoke" -Body (@{ Token = $rotationAccess; RefreshToken = $rotationRefresh } | ConvertTo-Json)
    Assert-Status "Revoke active refresh returns 200" $revoke @(200)
    Assert-Test "Revoke active refresh returns true" ($revoke.Json.data -eq $true)
    Assert-Status "Refresh after explicit revoke is rejected" (Request "Refresh after revoke" POST "/api/auth/refresh" -Body (@{ RefreshToken = $rotationRefresh } | ConvertTo-Json)) @(401)
    Assert-Status "Rotated-away refresh token is rejected" (Request "Replay old rotated refresh" POST "/api/auth/refresh" -Body (@{ RefreshToken = $oldRotationRefresh } | ConvertTo-Json)) @(401)
    Assert-Status "Invalid refresh token is rejected" (Request "Invalid refresh value" POST "/api/auth/refresh" -Body (@{ RefreshToken = "not-real" } | ConvertTo-Json)) @(401)
    $revokeReplay = Request "Replay explicit revoke" POST "/api/auth/revoke" -Body (@{ Token = $rotationAccess; RefreshToken = $rotationRefresh } | ConvertTo-Json)
    Assert-Status "Revoke replay returns 200" $revokeReplay @(200)
    Assert-Test "Revoke replay returns false" ($revokeReplay.Json.data -eq $false)
    Assert-4xx "Malformed revoke is rejected" (Request "Malformed revoke" POST "/api/auth/revoke" -Body (@{ Token = "not-jwt"; RefreshToken = "not-refresh" } | ConvertTo-Json))

    Section "Legacy phone verification endpoints"
    Login-Account "Client before phone verification" $client | Out-Null
    $phone = "+2010" + (Get-Random -Minimum 10000000 -Maximum 99999999)
    $sendPhone = Request "Phone send-token" POST "/api/auth/phone/send-token" -Token $client.Token -Body (@{ PhoneNumber = $phone } | ConvertTo-Json)
    Assert-Status "Phone send-token returns 200" $sendPhone @(200)
    $sms = Sms-Code $phone
    Assert-Test "Mock SMS code is recorded" ($sms -ne "")
    if ($sms) {
        Assert-Status "Phone confirm returns 200" (Request "Phone confirm" POST "/api/auth/phone/confirm" -Token $client.Token -Body (@{ PhoneNumber = $phone; Token = $sms } | ConvertTo-Json)) @(200)
        Login-Account "Client after phone confirmation" $client | Out-Null
        Assert-4xx "Phone confirmation replay is rejected" (Request "Phone confirm replay" POST "/api/auth/phone/confirm" -Token $client.Token -Body (@{ PhoneNumber = $phone; Token = $sms } | ConvertTo-Json))
    }
    Assert-4xx "Invalid phone token is rejected" (Request "Invalid phone token" POST "/api/auth/phone/confirm" -Token $client.Token -Body (@{ PhoneNumber = $phone; Token = "000000" } | ConvertTo-Json))
    $emptyPhone = Request "Empty phone send payload" POST "/api/auth/phone/send-token" -Token $client.Token -Body "{}"
    Assert-Test "Empty phone payload does not produce a server error" ($emptyPhone.Status -ge 200 -and $emptyPhone.Status -lt 500) ("status=" + $emptyPhone.Status)
    Assert-Test "Phone actions create no extra password-change notification" (@(Notification-Items $client.Token "Phone final security list" | Where-Object { $_.type -eq "security.password-changed" }).Count -eq 1)

    Section "Hostile input and final recipient isolation"
    Assert-4xx "Hostile forgot-password input is rejected" (Request "Hostile forgot input" POST "/api/auth/forgot-password" -Body (@{ Email = "' OR 1=1 -- <script>alert(1)</script>" } | ConvertTo-Json))
    Assert-4xx "Hostile reset-password input is rejected" (Request "Hostile reset input" POST "/api/auth/reset-password" -Body (@{ Email = "' OR 1=1 --"; Token = "bad"; NewPassword = "Gate7-Hostile123!"; ConfirmNewPassword = "Gate7-Hostile123!" } | ConvertTo-Json))
    $hugeEmail = ("a" * 6000) + "@example.com"
    Assert-Status "Extreme Email input returns generic forgot-password response" (Request "Extreme forgot input" POST "/api/auth/forgot-password" -Body (@{ Email = $hugeEmail } | ConvertTo-Json)) @(200)
    Assert-4xx "Empty reset body is rejected" (Request "Empty reset body" POST "/api/auth/reset-password" -Body "{}")
    Assert-4xx "Empty register body is rejected" (Request "Empty register body" POST "/api/auth/register/client" -Body "{}")
    $finalClient = Notification-Items $client.Token "Final client inbox"
    Assert-Test "Final client inbox has one password-change notification" (@($finalClient | Where-Object { $_.type -eq "security.password-changed" }).Count -eq 1)
    Assert-Test "Final client inbox has one password-reset notification" (@($finalClient | Where-Object { $_.type -eq "security.password-reset" }).Count -eq 1)
    $finalLawyer = Notification-Items $lawyer.Token "Final unrelated lawyer inbox"
    Assert-Test "Unrelated lawyer has no password-change notification" (@($finalLawyer | Where-Object { $_.type -eq "security.password-changed" }).Count -eq 0)
    Assert-Test "Unrelated lawyer has no password-reset notification" (@($finalLawyer | Where-Object { $_.type -eq "security.password-reset" }).Count -eq 0)
}
catch {
    $message = Protect-Text $_.Exception.Message
    [void]$script:fatal.Add($message)
    [void]($script:failed++)
    Add-Report ([Environment]::NewLine + "### Fatal test error" + [Environment]::NewLine + $message)
}
finally {
    Section "API and mock provider log monitoring"
    try { Logs-Clean } catch { [void]($script:failed++); Add-Report ("- [FAIL] **Log monitor execution** (" + (Protect-Text $_.Exception.Message) + ")") }
    Stop-Api
    Assert-Test "API test port is released after owned process shutdown" ((-not $script:ownsApi) -or (Port-Free))
    if (-not $script:ownsApi) { Skip-Test "API shutdown ownership" "An externally running API was supplied." }
    if (Test-Path $script:apiStdOut) { Remove-Item -LiteralPath $script:apiStdOut -Force -ErrorAction SilentlyContinue }
    if (Test-Path $script:apiStdErr) { Remove-Item -LiteralPath $script:apiStdErr -Force -ErrorAction SilentlyContinue }
    $storage = [IO.Path]::GetFullPath($script:localStoragePath)
    $uploads = [IO.Path]::GetFullPath((Join-Path $apiProjectDir "uploads"))
    if ((Test-Path $storage) -and $storage.StartsWith(($uploads + [IO.Path]::DirectorySeparatorChar), [StringComparison]::OrdinalIgnoreCase)) { Remove-Item -LiteralPath $storage -Recurse -Force -ErrorAction SilentlyContinue }
    Section "Execution summary"
    Add-Report ("| Metric | Count |" + [Environment]::NewLine + "|---|---:|" + [Environment]::NewLine + "| Passed assertions | " + $script:passed + " |" + [Environment]::NewLine + "| Failed assertions | " + $script:failed + " |" + [Environment]::NewLine + "| Documented skips | " + $script:skipped + " |")
    if ($script:fatal.Count -gt 0) { Add-Report ([Environment]::NewLine + "### Fatal errors"); foreach ($item in $script:fatal) { Add-Report ("- " + $item) } }
    Write-Host ("Auth security notification HTTP tests complete: " + $script:passed + " passed, " + $script:failed + " failed, " + $script:skipped + " skipped.")
}
if ($script:failed -gt 0) { exit 1 }
