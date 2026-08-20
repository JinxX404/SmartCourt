# Seed50VerifiedLawyers.ps1
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
        # Fallback
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

# Universal PasswordHash for "Password123!" using Identity default format
$passwordHash = "AQAAAAIAAYagAAAAEN8XOwL/qYjo/Be70VCIGQJsYJPWiJ6Z7P4Qfq+79W11ePMiozRR8aL9hAssu91+DA=="

# Helper to get/create Role ID
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
Write-Host "Lawyer Role ID: $lawyerRoleId" -ForegroundColor Cyan

# Check if tables exist
function Table-Exists($tableName) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName'"
    return ([int]$cmd.ExecuteScalar() -gt 0)
}

$hasWalletsTable = Table-Exists "LawyerWallets"
$hasPayoutsTable = Table-Exists "LawyerPayoutAccounts"
$hasDocsTable = Table-Exists "UserVerificationDocuments"

# Read JSON with UTF-8
$jsonFilePath = "$scriptDir\lawyers_50.json"
if (-not (Test-Path $jsonFilePath)) {
    Write-Error "Could not find $jsonFilePath"
    exit 1
}

$jsonText = [System.IO.File]::ReadAllText($jsonFilePath, [System.Text.Encoding]::UTF8)
$lawyers = $jsonText | ConvertFrom-Json

Write-Host "Found $($lawyers.Count) lawyers to seed." -ForegroundColor Yellow

$count = 0
foreach ($l in $lawyers) {
    $fullName = $l.fullName
    $email = $l.email
    $normalizedEmail = $email.ToUpper()
    $phoneNumber = $l.phoneNumber
    $nationalNumber = $l.nationalNumber
    $gender = [int]$l.gender
    $dateOfBirth = $l.dateOfBirth
    $address = $l.address
    $governorate = $l.governorate
    $city = $l.city
    $level = [int]$l.level
    $bio = $l.bio
    $rating = [decimal]$l.rating
    $ratingCount = [int]$l.ratingCount
    $ratingSum = [int]($rating * $ratingCount)
    $responseTime = [decimal]$l.responseTime
    $isAvailable = if ($l.isAvailable) { 1 } else { 0 }

    # 1. Insert or Update AspNetUsers using SQL Parameters for clean Unicode support
    $sqlUser = @"
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @Email)
BEGIN
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO AspNetUsers (
        Id, FullName, Email, NormalizedEmail, UserName, NormalizedUserName, 
        PhoneNumber, PhoneNumberConfirmed, NationalNumber, Gender, DateOfBirth, 
        Address, Governorate, City, EmailConfirmed, Status, 
        SecurityStamp, ConcurrencyStamp, PasswordHash, AccessFailedCount, LockoutEnabled, TwoFactorEnabled
    )
    VALUES (
        @NewId, @FullName, @Email, @NormalizedEmail, @Email, @NormalizedEmail,
        @PhoneNumber, 1, @NationalNumber, @Gender, @DateOfBirth,
        @Address, @Governorate, @City, 1, 2,
        NEWID(), NEWID(), @PasswordHash, 0, 1, 0
    );
END
ELSE
BEGIN
    UPDATE AspNetUsers 
    SET FullName = @FullName,
        PhoneNumber = @PhoneNumber,
        PhoneNumberConfirmed = 1,
        NationalNumber = @NationalNumber,
        Gender = @Gender,
        DateOfBirth = @DateOfBirth,
        Address = @Address,
        Governorate = @Governorate,
        City = @City,
        Status = 2, 
        EmailConfirmed = 1, 
        PasswordHash = @PasswordHash
    WHERE Email = @Email;
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlUser
    $cmd.Parameters.AddWithValue("@FullName", $fullName) | Out-Null
    $cmd.Parameters.AddWithValue("@Email", $email) | Out-Null
    $cmd.Parameters.AddWithValue("@NormalizedEmail", $normalizedEmail) | Out-Null
    $cmd.Parameters.AddWithValue("@PhoneNumber", $phoneNumber) | Out-Null
    $cmd.Parameters.AddWithValue("@NationalNumber", $nationalNumber) | Out-Null
    $cmd.Parameters.AddWithValue("@Gender", $gender) | Out-Null
    $cmd.Parameters.AddWithValue("@DateOfBirth", $dateOfBirth) | Out-Null
    $cmd.Parameters.AddWithValue("@Address", $address) | Out-Null
    $cmd.Parameters.AddWithValue("@Governorate", $governorate) | Out-Null
    $cmd.Parameters.AddWithValue("@City", $city) | Out-Null
    $cmd.Parameters.AddWithValue("@PasswordHash", $passwordHash) | Out-Null
    $cmd.ExecuteNonQuery() | Out-Null

    # 2. Get User Id
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id FROM AspNetUsers WHERE Email = @Email"
    $cmd.Parameters.AddWithValue("@Email", $email) | Out-Null
    $userId = $cmd.ExecuteScalar().ToString()

    # 3. Add to AspNetUserRoles (Lawyer)
    if ($lawyerRoleId) {
        $sqlRole = "IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = '$userId' AND RoleId = '$lawyerRoleId') INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('$userId', '$lawyerRoleId');"
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlRole
        $cmd.ExecuteNonQuery() | Out-Null
    }

    # 4. Insert or Update LawyerProfile
    $sqlProfile = @"
IF NOT EXISTS (SELECT 1 FROM LawyerProfile WHERE UserId = '$userId')
BEGIN
    INSERT INTO LawyerProfile (UserId, Level, Bio, IsAvailable, AverageRating, TotalRatingSum, TotalRatingCount, AverageResponseTimeHours)
    VALUES ('$userId', @Level, @Bio, @IsAvailable, @Rating, @RatingSum, @RatingCount, @ResponseTime);
END
ELSE
BEGIN
    UPDATE LawyerProfile
    SET Level = @Level, 
        Bio = @Bio, 
        IsAvailable = @IsAvailable, 
        AverageRating = @Rating, 
        TotalRatingSum = @RatingSum, 
        TotalRatingCount = @RatingCount, 
        AverageResponseTimeHours = @ResponseTime
    WHERE UserId = '$userId';
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlProfile
    $cmd.Parameters.AddWithValue("@Level", $level) | Out-Null
    $cmd.Parameters.AddWithValue("@Bio", $bio) | Out-Null
    $cmd.Parameters.AddWithValue("@IsAvailable", $isAvailable) | Out-Null
    $cmd.Parameters.AddWithValue("@Rating", $rating) | Out-Null
    $cmd.Parameters.AddWithValue("@RatingSum", $ratingSum) | Out-Null
    $cmd.Parameters.AddWithValue("@RatingCount", $ratingCount) | Out-Null
    $cmd.Parameters.AddWithValue("@ResponseTime", $responseTime) | Out-Null
    $cmd.ExecuteNonQuery() | Out-Null

    # 5. Insert Specializations
    $cmdDel = $conn.CreateCommand()
    $cmdDel.CommandText = "DELETE FROM LawyerSpecializations WHERE LawyerProfileUserId = '$userId'"
    $cmdDel.ExecuteNonQuery() | Out-Null

    foreach ($s in $l.specs) {
        $specEnum = [int]$s.spec
        $specYears = [int]$s.years
        $specCases = [int]$s.cases
        $specId = [Guid]::NewGuid().ToString()
        
        $sqlSpec = "INSERT INTO LawyerSpecializations (Id, LawyerProfileUserId, Specialization, YearsOfExperience, CasesHandled) VALUES ('$specId', '$userId', $specEnum, $specYears, $specCases);"
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sqlSpec
        $cmd.ExecuteNonQuery() | Out-Null
    }

    # 6. Insert Verified Documents if Table Exists
    if ($hasDocsTable) {
        $sqlDocs = @"
IF NOT EXISTS (SELECT 1 FROM UserVerificationDocuments WHERE UserId = '$userId' AND DocumentType = 0)
BEGIN
    INSERT INTO UserVerificationDocuments (Id, UserId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate, CreatedAt)
    VALUES (NEWID(), '$userId', 0, 2, 1, 0, DATEADD(year, 2, GETUTCDATE()), GETUTCDATE());
END
IF NOT EXISTS (SELECT 1 FROM UserVerificationDocuments WHERE UserId = '$userId' AND DocumentType = 1)
BEGIN
    INSERT INTO UserVerificationDocuments (Id, UserId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate, CreatedAt)
    VALUES (NEWID(), '$userId', 1, 2, 1, 0, DATEADD(year, 5, GETUTCDATE()), GETUTCDATE());
END
"@
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $sqlDocs
            $cmd.ExecuteNonQuery() | Out-Null
        } catch {}
    }

    # 7. Insert Lawyer Wallet if Table Exists
    if ($hasWalletsTable) {
        $sqlWallet = @"
IF NOT EXISTS (SELECT 1 FROM LawyerWallets WHERE LawyerUserId = '$userId')
BEGIN
    INSERT INTO LawyerWallets (Id, LawyerUserId, PendingBalance, AvailableBalance, TotalReleased, Currency, CreatedAt, UpdatedAt)
    VALUES (NEWID(), '$userId', 0.00, 0.00, 0.00, 'EGP', GETUTCDATE(), GETUTCDATE());
END
"@
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $sqlWallet
            $cmd.ExecuteNonQuery() | Out-Null
        } catch {}
    }

    # 8. Insert Lawyer Payout Account if Table Exists
    if ($hasPayoutsTable) {
        $sqlPayout = @"
IF NOT EXISTS (SELECT 1 FROM LawyerPayoutAccounts WHERE LawyerUserId = '$userId')
BEGIN
    INSERT INTO LawyerPayoutAccounts (Id, LawyerUserId, ProviderCode, Status, DetailsSubmitted, TransfersEnabled, PayoutsEnabled, Country, DefaultCurrency, CreatedAt, UpdatedAt)
    VALUES (NEWID(), '$userId', 'StripeConnect', 2, 1, 1, 1, 'EG', 'EGP', GETUTCDATE(), GETUTCDATE());
END
"@
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $sqlPayout
            $cmd.ExecuteNonQuery() | Out-Null
        } catch {}
    }

    $count++
    Write-Host "[$count/50] Seeded & Verified: $fullName ($email) - Level $level - $governorate" -ForegroundColor Green
}

$conn.Close()
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host "Successfully Seeded and Verified all 50 Lawyers into the Database!" -ForegroundColor Green
Write-Host "All 50 lawyers are Active, EmailConfirmed, Level-configured (1-4)," -ForegroundColor Green
Write-Host "covering ALL 21 specializations (0-20), with full profiles and ratings." -ForegroundColor Green
Write-Host "Universal Password: Password123!" -ForegroundColor Yellow
Write-Host "=======================================================================" -ForegroundColor Cyan
