# SeedMarketplaceData.ps1
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appsettingsPath = "$scriptDir\..\..\SmartCourt\appsettings.Development.json"
$conn = $null

if (Test-Path $appsettingsPath) {
    try {
        $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $cs = $json.ConnectionStrings.DefaultConnection
        if ($cs) {
            $testConn = New-Object System.Data.SqlClient.SqlConnection($cs)
            $testConn.Open()
            $conn = $testConn
            Write-Host "Connected using appsettings.Development.json DefaultConnection: $cs" -ForegroundColor Green
        }
    } catch {
        # Fallback to predefined DBs
    }
}

if (-not $conn) {
    $dbNames = @("SmartCourt_Dev2", "SmartCourt_Dev", "SmartCourtDB", "SmartCourt_Graduation")
    foreach ($db in $dbNames) {
        try {
            $cs = "Server=.;Database=$db;Integrated Security=True;TrustServerCertificate=True;"
            $testConn = New-Object System.Data.SqlClient.SqlConnection($cs)
            $testConn.Open()
            $conn = $testConn
            Write-Host "Connected to database: $db" -ForegroundColor Green
            break
        } catch {
            # Try next
        }
    }
}

if (-not $conn) {
    Write-Error "Could not connect to any SmartCourt database instance."
    exit 1
}

# Common PasswordHash for "Password123!" using Identity default format
$passwordHash = "AQAAAAIAAYagAAAAEN8XOwL/qYjo/Be70VCIGQJsYJPWiJ6Z7P4Qfq+79W11ePMiozRR8aL9hAssu91+DA=="

# Get Role IDs
function Get-RoleId($roleName) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id FROM AspNetRoles WHERE Name = '$roleName'"
    $val = $cmd.ExecuteScalar()
    if (-not $val) {
        $newRoleId = [Guid]::NewGuid().ToString()
        $normRole = $roleName.ToUpper()
        $createRoleCmd = $conn.CreateCommand()
        $createRoleCmd.CommandText = "INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES ('$newRoleId', '$roleName', '$normRole', '$newRoleId')"
        $createRoleCmd.ExecuteNonQuery() | Out-Null
        return $newRoleId
    }
    return $val.ToString()
}

$lawyerRoleId = Get-RoleId "Lawyer"
$clientRoleId = Get-RoleId "Client"

Write-Host "Lawyer Role ID: $lawyerRoleId"
Write-Host "Client Role ID: $clientRoleId"

# Helper function to insert lawyer user & profile directly
function Add-LawyerUser {
    param (
        [string]$id,
        [string]$fullName,
        [string]$email,
        [string]$phoneNumber,
        [string]$nationalNumber,
        [int]$gender,
        [string]$dateOfBirth,
        [string]$address,
        [string]$governorate,
        [string]$city,
        [int]$level,
        [string]$bio,
        [decimal]$rating,
        [int]$ratingCount,
        [decimal]$responseTime,
        [bool]$isAvailable,
        [int[]]$specializations
    )

    $normalizedEmail = $email.ToUpper()
    $secStamp = [Guid]::NewGuid().ToString()
    $conStamp = [Guid]::NewGuid().ToString()
    $ratingSum = [int]($rating * $ratingCount)

    # 1. Insert or Update AspNetUsers
    $sqlUser = @"
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = '$email')
BEGIN
    INSERT INTO AspNetUsers (
        Id, FullName, Email, NormalizedEmail, UserName, NormalizedUserName, 
        PhoneNumber, PhoneNumberConfirmed, NationalNumber, Gender, DateOfBirth, 
        Address, Governorate, City, EmailConfirmed, Status, 
        SecurityStamp, ConcurrencyStamp, PasswordHash, AccessFailedCount, LockoutEnabled, TwoFactorEnabled
    )
    VALUES (
        '$id', N'$fullName', '$email', '$normalizedEmail', '$email', '$normalizedEmail',
        '$phoneNumber', 1, '$nationalNumber', $gender, '$dateOfBirth',
        N'$address', N'$governorate', N'$city', 1, 2,
        '$secStamp', '$conStamp', '$passwordHash', 0, 1, 0
    )
END
ELSE
BEGIN
    UPDATE AspNetUsers 
    SET FullName = N'$fullName',
        PhoneNumber = '$phoneNumber',
        PhoneNumberConfirmed = 1,
        NationalNumber = '$nationalNumber',
        Gender = $gender,
        DateOfBirth = '$dateOfBirth',
        Address = N'$address',
        Governorate = N'$governorate',
        City = N'$city',
        Status = 2, 
        EmailConfirmed = 1, 
        PasswordHash = '$passwordHash'
    WHERE Email = '$email'
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlUser
    $cmd.ExecuteNonQuery() | Out-Null

    # 2. Get User Id
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id FROM AspNetUsers WHERE Email = '$email'"
    $userId = $cmd.ExecuteScalar().ToString()

    # 3. Add to AspNetUserRoles
    if ($lawyerRoleId) {
        $sqlRole = "IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = '$userId' AND RoleId = '$lawyerRoleId') INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('$userId', '$lawyerRoleId')"
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlRole
        $cmd.ExecuteNonQuery() | Out-Null
    }

    # 4. Insert or Update LawyerProfile
    $availBit = if ($isAvailable) { 1 } else { 0 }
    $sqlProfile = @"
IF NOT EXISTS (SELECT 1 FROM LawyerProfile WHERE UserId = '$userId')
BEGIN
    INSERT INTO LawyerProfile (UserId, Level, Bio, IsAvailable, AverageRating, TotalRatingSum, TotalRatingCount, AverageResponseTimeHours)
    VALUES ('$userId', $level, N'$bio', $availBit, $rating, $ratingSum, $ratingCount, $responseTime)
END
ELSE
BEGIN
    UPDATE LawyerProfile
    SET Level = $level, 
        Bio = N'$bio', 
        IsAvailable = $availBit, 
        AverageRating = $rating, 
        TotalRatingSum = $ratingSum, 
        TotalRatingCount = $ratingCount, 
        AverageResponseTimeHours = $responseTime
    WHERE UserId = '$userId'
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlProfile
    $cmd.ExecuteNonQuery() | Out-Null

    # 5. Insert Specializations
    $cmdDel = $conn.CreateCommand()
    $cmdDel.CommandText = "DELETE FROM LawyerSpecializations WHERE LawyerProfileUserId = '$userId'"
    $cmdDel.ExecuteNonQuery() | Out-Null

    foreach ($spec in $specializations) {
        $specId = [Guid]::NewGuid().ToString()
        $sqlSpec = @"
INSERT INTO LawyerSpecializations (Id, LawyerProfileUserId, Specialization, YearsOfExperience, CasesHandled)
VALUES ('$specId', '$userId', $spec, 7, 30)
"@
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlSpec
        $cmd.ExecuteNonQuery() | Out-Null
    }

    Write-Host "Seeded & Verified Lawyer: $fullName ($email)" -ForegroundColor Green
}

# Helper function to insert client user directly
function Add-ClientUser {
    param (
        [string]$email, 
        [string]$fullName,
        [string]$phoneNumber,
        [string]$nationalNumber,
        [int]$gender,
        [string]$dob,
        [string]$address,
        [string]$governorate,
        [string]$city
    )
    
    $normalizedEmail = $email.ToUpper()
    $secStamp = [Guid]::NewGuid().ToString()
    $conStamp = [Guid]::NewGuid().ToString()
    $id = [Guid]::NewGuid().ToString()

    $sqlClient = @"
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = '$email')
BEGIN
    INSERT INTO AspNetUsers (
        Id, FullName, Email, NormalizedEmail, UserName, NormalizedUserName, 
        PhoneNumber, PhoneNumberConfirmed, NationalNumber, Gender, DateOfBirth, 
        Address, Governorate, City, EmailConfirmed, Status, 
        SecurityStamp, ConcurrencyStamp, PasswordHash, AccessFailedCount, LockoutEnabled, TwoFactorEnabled
    )
    VALUES (
        '$id', N'$fullName', '$email', '$normalizedEmail', '$email', '$normalizedEmail', 
        '$phoneNumber', 1, '$nationalNumber', $gender, '$dob',
        N'$address', N'$governorate', N'$city', 1, 2, 
        '$secStamp', '$conStamp', '$passwordHash', 0, 1, 0
    )
END
ELSE
BEGIN
    UPDATE AspNetUsers 
    SET FullName = N'$fullName', 
        PhoneNumber = '$phoneNumber',
        PhoneNumberConfirmed = 1,
        NationalNumber = '$nationalNumber',
        Gender = $gender,
        DateOfBirth = '$dob',
        Address = N'$address',
        Governorate = N'$governorate',
        City = N'$city',
        Status = 2, 
        EmailConfirmed = 1, 
        PasswordHash = '$passwordHash' 
    WHERE Email = '$email'
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlClient
    $cmd.ExecuteNonQuery() | Out-Null

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id FROM AspNetUsers WHERE Email = '$email'"
    $userId = $cmd.ExecuteScalar().ToString()

    if ($clientRoleId) {
        $sqlRole = "IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = '$userId' AND RoleId = '$clientRoleId') INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('$userId', '$clientRoleId')"
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlRole
        $cmd.ExecuteNonQuery() | Out-Null
    }

    $sqlClientProf = "IF NOT EXISTS (SELECT 1 FROM ClientProfile WHERE UserId = '$userId') INSERT INTO ClientProfile (UserId) VALUES ('$userId')"
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlClientProf
    $cmd.ExecuteNonQuery() | Out-Null

    Write-Host "Seeded & Verified Client: $fullName ($email)" -ForegroundColor Green
}

# ==============================================================================
# EXECUTE SEEDING
# ==============================================================================

# 1. Clients
Add-ClientUser -email "client@smartcourt.com" `
    -fullName "Test Client" `
    -phoneNumber "01100000000" `
    -nationalNumber "29001011234500" `
    -gender 1 `
    -dob "1992-04-10" `
    -address "El Nasr Street" `
    -governorate "Cairo" `
    -city "Maadi"

Add-ClientUser -email "mkt_search_client@test.com" `
    -fullName "Marketplace Search Client" `
    -phoneNumber "01199998888" `
    -nationalNumber "29001011234501" `
    -gender 1 `
    -dob "1994-08-15" `
    -address "Dokki Street" `
    -governorate "Giza" `
    -city "Dokki"

# 2. System Default Lawyer
Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Test Lawyer" `
    -email "lawyer@smartcourt.com" `
    -phoneNumber "01000000000" `
    -nationalNumber "28001011234500" `
    -gender 1 `
    -dateOfBirth "1980-01-01" `
    -address "123 Legal Street" `
    -governorate "Cairo" `
    -city "Nasr City" `
    -level 4 `
    -bio "Senior corporate lawyer with over 15 years experience in commercial arbitration and corporate agreements." `
    -rating 4.95 `
    -ratingCount 24 `
    -responseTime 1.00 `
    -isAvailable $true `
    -specializations @(2, 9, 10)

# 3. Marketplace Diverse Lawyers
Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Ahmed El-Sayed" `
    -email "ahmed.sayed.lawyer@test.com" `
    -phoneNumber "01012345671" `
    -nationalNumber "28205121234561" `
    -gender 1 `
    -dateOfBirth "1982-05-12" `
    -address "45 Mossadak Street" `
    -governorate "Cairo" `
    -city "Nasr City" `
    -level 4 `
    -bio "Senior Cassation attorney specializing in commercial disputes and corporate law." `
    -rating 4.90 `
    -ratingCount 35 `
    -responseTime 1.50 `
    -isAvailable $true `
    -specializations @(2, 1)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Fatima Al-Zahraa" `
    -email "fatima.zahraa.lawyer@test.com" `
    -phoneNumber "01012345672" `
    -nationalNumber "28608221234562" `
    -gender 2 `
    -dateOfBirth "1986-08-22" `
    -address "12 Army Road" `
    -governorate "Alexandria" `
    -city "Smouha" `
    -level 3 `
    -bio "Appeal Court specialist for family law, alimony, and custody cases." `
    -rating 4.80 `
    -ratingCount 28 `
    -responseTime 3.00 `
    -isAvailable $true `
    -specializations @(0)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Mahmoud Hassan" `
    -email "mahmoud.hassan.lawyer@test.com" `
    -phoneNumber "01012345673" `
    -nationalNumber "29011151234563" `
    -gender 1 `
    -dateOfBirth "1990-11-15" `
    -address "88 Tahrir Street" `
    -governorate "Giza" `
    -city "Dokki" `
    -level 2 `
    -bio "Primary Court litigation expert handling civil law and contract drafting." `
    -rating 4.20 `
    -ratingCount 14 `
    -responseTime 6.00 `
    -isAvailable $true `
    -specializations @(1, 10)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Mostafa Mansour" `
    -email "mostafa.mansour.lawyer@test.com" `
    -phoneNumber "01012345674" `
    -nationalNumber "27903101234564" `
    -gender 1 `
    -dateOfBirth "1979-03-10" `
    -address "15 Ramses Square" `
    -governorate "Cairo" `
    -city "Downtown" `
    -level 4 `
    -bio "Cassation level criminal law defender and constitutional advisor." `
    -rating 5.00 `
    -ratingCount 42 `
    -responseTime 0.50 `
    -isAvailable $true `
    -specializations @(4, 6)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Nouran Ibrahim" `
    -email "nouran.ibrahim.lawyer@test.com" `
    -phoneNumber "01012345675" `
    -nationalNumber "28807181234565" `
    -gender 2 `
    -dateOfBirth "1988-07-18" `
    -address "22 El Gomhoria Street" `
    -governorate "Mansoura" `
    -city "El Mansoura" `
    -level 3 `
    -bio "Labor law consultant handling employment contracts and State Council disputes." `
    -rating 3.80 `
    -ratingCount 10 `
    -responseTime 12.00 `
    -isAvailable $false `
    -specializations @(5, 3)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Youssef Nabil" `
    -email "youssef.nabil.lawyer@test.com" `
    -phoneNumber "01012345676" `
    -nationalNumber "29609051234566" `
    -gender 1 `
    -dateOfBirth "1996-09-05" `
    -address "5 Corniche El Nile" `
    -governorate "Aswan" `
    -city "Aswan City" `
    -level 1 `
    -bio "Junior attorney specializing in general civil consultations and property registration." `
    -rating 3.50 `
    -ratingCount 6 `
    -responseTime 24.00 `
    -isAvailable $true `
    -specializations @(1, 15)

$conn.Close()

Write-Host "Triggering 50 Lawyers Extended Dataset Seeder..." -ForegroundColor Cyan
powershell -ExecutionPolicy Bypass -File "$scriptDir\Seed50VerifiedLawyers.ps1"

Write-Host "Database Seeding and Verification Completed Successfully!" -ForegroundColor Cyan
