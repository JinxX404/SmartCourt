# SeedMarketplaceData.ps1
$connectionString = "Server=localhost;Database=SmartCourtDB;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()

# Common PasswordHash for "Password123!" using Identity default format
$passwordHash = "AQAAAAIAAYagAAAAEN8XOwL/qYjo/Be70VCIGQJsYJPWiJ6Z7P4Qfq+79W11ePMiozRR8aL9hAssu91+DA=="

# Get Role IDs
function Get-RoleId($roleName) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id FROM AspNetRoles WHERE Name = '$roleName'"
    $val = $cmd.ExecuteScalar()
    return $val
}

$lawyerRoleId = Get-RoleId "Lawyer"
$clientRoleId = Get-RoleId "Client"

Write-Host "Lawyer Role ID: $lawyerRoleId"
Write-Host "Client Role ID: $clientRoleId"

# Helper function to insert user & profile directly
function Add-LawyerUser {
    param (
        [string]$id,
        [string]$fullName,
        [string]$email,
        [int]$level,
        [string]$governorate,
        [string]$bio,
        [decimal]$rating,
        [decimal]$responseTime,
        [bool]$isAvailable,
        [int[]]$specializations
    )

    $normalizedEmail = $email.ToUpper()
    $secStamp = [Guid]::NewGuid().ToString()
    $conStamp = [Guid]::NewGuid().ToString()

    # 1. Insert AspNetUsers if not exists
    $sqlUser = @"
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = '$email')
BEGIN
    INSERT INTO AspNetUsers (Id, FullName, Email, NormalizedEmail, UserName, NormalizedUserName, EmailConfirmed, Status, Governorate, SecurityStamp, ConcurrencyStamp, PasswordHash, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
    VALUES ('$id', N'$fullName', '$email', '$normalizedEmail', '$email', '$normalizedEmail', 1, 2, N'$governorate', '$secStamp', '$conStamp', '$passwordHash', 0, 1, 1, 0)
END
ELSE
BEGIN
    UPDATE AspNetUsers 
    SET FullName = N'$fullName', Status = 2, EmailConfirmed = 1, Governorate = N'$governorate', PasswordHash = '$passwordHash'
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
    INSERT INTO LawyerProfile (UserId, Level, Bio, IsAvailable, AverageRating, AverageResponseTimeHours)
    VALUES ('$userId', $level, N'$bio', $availBit, $rating, $responseTime)
END
ELSE
BEGIN
    UPDATE LawyerProfile
    SET Level = $level, Bio = N'$bio', IsAvailable = $availBit, AverageRating = $rating, AverageResponseTimeHours = $responseTime
    WHERE UserId = '$userId'
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlProfile
    $cmd.ExecuteNonQuery() | Out-Null

    # 5. Insert Specializations
    foreach ($spec in $specializations) {
        $specId = [Guid]::NewGuid().ToString()
        $sqlSpec = @"
IF NOT EXISTS (SELECT 1 FROM LawyerSpecializations WHERE LawyerProfileUserId = '$userId' AND Specialization = $spec)
BEGIN
    INSERT INTO LawyerSpecializations (Id, LawyerProfileUserId, Specialization, YearsOfExperience, CasesHandled)
    VALUES ('$specId', '$userId', $spec, 5, 20)
END
"@
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlSpec
        $cmd.ExecuteNonQuery() | Out-Null
    }

    Write-Host "Seeded Lawyer: $fullName ($email)" -ForegroundColor Green
}

# Seed Client User for Search API Authentication
function Add-ClientUser {
    param ([string]$email, [string]$fullName)
    
    $normalizedEmail = $email.ToUpper()
    $secStamp = [Guid]::NewGuid().ToString()
    $conStamp = [Guid]::NewGuid().ToString()
    $id = [Guid]::NewGuid().ToString()

    $sqlClient = @"
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = '$email')
BEGIN
    INSERT INTO AspNetUsers (Id, FullName, Email, NormalizedEmail, UserName, NormalizedUserName, EmailConfirmed, Status, SecurityStamp, ConcurrencyStamp, PasswordHash, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
    VALUES ('$id', N'$fullName', '$email', '$normalizedEmail', '$email', '$normalizedEmail', 1, 2, '$secStamp', '$conStamp', '$passwordHash', 0, 1, 1, 0)
END
ELSE
BEGIN
    UPDATE AspNetUsers SET Status = 2, EmailConfirmed = 1, PasswordHash = '$passwordHash' WHERE Email = '$email'
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

    Write-Host "Seeded Client User: $fullName ($email)" -ForegroundColor Green
}

# ==============================================================================
# EXECUTE SEEDING
# ==============================================================================
Add-ClientUser -email "mkt_search_client@test.com" -fullName "Marketplace Search Client"

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Ahmed El-Sayed" `
    -email "ahmed.sayed.lawyer@test.com" `
    -level 4 `
    -governorate "Cairo" `
    -bio "Senior Cassation attorney specializing in commercial disputes and corporate law" `
    -rating 4.90 `
    -responseTime 1.50 `
    -isAvailable $true `
    -specializations @(2, 1)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Fatima Al-Zahraa" `
    -email "fatima.zahraa.lawyer@test.com" `
    -level 3 `
    -governorate "Alexandria" `
    -bio "Appeal Court specialist for family law, alimony, and custody cases" `
    -rating 4.80 `
    -responseTime 3.00 `
    -isAvailable $true `
    -specializations @(0)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Mahmoud Hassan" `
    -email "mahmoud.hassan.lawyer@test.com" `
    -level 2 `
    -governorate "Giza" `
    -bio "Primary Court litigation expert handling civil law and contract drafting" `
    -rating 4.20 `
    -responseTime 6.00 `
    -isAvailable $true `
    -specializations @(1)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Mostafa Mansour" `
    -email "mostafa.mansour.lawyer@test.com" `
    -level 4 `
    -governorate "Cairo" `
    -bio "Cassation level criminal law defender and constitutional advisor" `
    -rating 5.00 `
    -responseTime 0.50 `
    -isAvailable $true `
    -specializations @(4)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Nouran Ibrahim" `
    -email "nouran.ibrahim.lawyer@test.com" `
    -level 3 `
    -governorate "Mansoura" `
    -bio "Labor law consultant handling employment contracts and State Council disputes" `
    -rating 3.80 `
    -responseTime 12.00 `
    -isAvailable $false `
    -specializations @(5, 3)

Add-LawyerUser -id ([Guid]::NewGuid().ToString()) `
    -fullName "Youssef Nabil" `
    -email "youssef.nabil.lawyer@test.com" `
    -level 1 `
    -governorate "Aswan" `
    -bio "Junior attorney specializing in general civil consultations" `
    -rating 3.50 `
    -responseTime 24.00 `
    -isAvailable $true `
    -specializations @(1)

$conn.Close()
Write-Host "Database Seeding Completed Successfully!" -ForegroundColor Cyan
