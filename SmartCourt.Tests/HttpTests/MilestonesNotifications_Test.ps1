param(
    [string]$BaseUrl = "http://localhost:5049",
    [string]$ApiLogPath = "",
    [string]$ReportFile = "",
    [string]$SqlServer = ".",
    [string]$SqlDatabase = "SmartCourt_dev"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ApiLogPath) {
    $ApiLogPath = Join-Path $scriptDir "..\..\SmartCourt\api_log.txt"
}
if (-not $ReportFile) {
    $ReportFile = Join-Path $scriptDir "MilestonesNotifications_Report.md"
}

. "$scriptDir\ContractsNotifications_Test.ps1" `
    -BaseUrl $BaseUrl -ApiLogPath $ApiLogPath -ReportFile $ReportFile `
    -SqlServer $SqlServer -SqlDatabase $SqlDatabase -FunctionsOnly

$script:passed = 0
$script:failed = 0
$script:failureMessages = [System.Collections.Generic.List[string]]::new()
"# Milestones Notifications HTTP Test Report`n`nGenerated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')`n" |
    Out-File $ReportFile -Encoding utf8

function Find-MilestoneNotification {
    param(
        [string]$Title,
        [string]$Token,
        [string]$Type,
        [string]$MilestoneId,
        [string]$ChangeRequestId = "",
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $response = Invoke-TestRequest $Title GET `
            "/api/notifications?pageSize=50" -Token $Token
        if ($response.Status -eq 200 -and $response.Json.data.items) {
            $match = @($response.Json.data.items) | Where-Object {
                $_.type -eq $Type -and
                $_.data.milestoneId -eq $MilestoneId -and
                (-not $ChangeRequestId -or
                    $_.data.changeRequestId -eq $ChangeRequestId)
            } | Select-Object -First 1
            if ($match) { return $match }
        }
        Start-Sleep -Milliseconds 750
    } while ((Get-Date) -lt $deadline)
    return $null
}

function Assert-MilestoneNotification {
    param(
        [string]$Name,
        $Notification,
        [string]$Type,
        [string]$Severity,
        [string]$Title,
        [string]$Body,
        [string]$MilestoneId,
        [string]$ContractId,
        [string]$ProposalId,
        [string]$CaseId,
        [string]$ChangeRequestId = ""
    )

    $valid = $null -ne $Notification -and
        $Notification.type -eq $Type -and
        $Notification.severity -eq $Severity -and
        $Notification.title -eq $Title -and
        $Notification.body -eq $Body -and
        $null -eq $Notification.actionUrl -and
        $Notification.data.milestoneId -eq $MilestoneId -and
        $Notification.data.contractId -eq $ContractId -and
        $Notification.data.proposalId -eq $ProposalId -and
        $Notification.data.legalCaseId -eq $CaseId
    if ($ChangeRequestId) {
        $valid = $valid -and
            $Notification.data.changeRequestId -eq $ChangeRequestId
    }
    Assert-Test $Name $valid
}

function Get-Milestone {
    param(
        [string]$Title,
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$Token
    )
    $response = Invoke-TestRequest $Title GET `
        "/api/contracts/$ContractId/milestones" -Token $Token
    Require-Api $Title $response
    $milestone = @($response.Json.data) |
        Where-Object id -eq $MilestoneId |
        Select-Object -First 1
    Assert-Test "$Title contains target milestone" ($null -ne $milestone)
    if (-not $milestone) { throw "Milestone $MilestoneId was not listed." }
    return $milestone
}

function Get-ChangeRequestETag {
    param([string]$ChangeRequestId)
    $hex = (& sqlcmd -S $SqlServer -d $SqlDatabase -b -W -h -1 -Q `
        "SET NOCOUNT ON; SELECT master.dbo.fn_varbintohexstr([RowVersion]) FROM MilestoneChangeRequests WHERE Id = '$ChangeRequestId';") |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1
    if ($LASTEXITCODE -ne 0 -or -not $hex) {
        throw "Could not read the change-request rowversion test fixture."
    }
    $hex = $hex.Trim() -replace '^0x', ''
    $bytes = [byte[]]::new($hex.Length / 2)
    for ($index = 0; $index -lt $bytes.Length; $index++) {
        $bytes[$index] = [Convert]::ToByte(
            $hex.Substring($index * 2, 2), 16)
    }
    return '"' + [Convert]::ToBase64String($bytes) + '"'
}

function Add-LawyerFileFixture {
    param([string]$LawyerId, [string]$Label)
    $fileId = [guid]::NewGuid()
    $fileName = "milestones-notifications-$Label-$stamp.pdf"
    $sql = @"
SET NOCOUNT ON;
INSERT INTO StoredFiles
    (Id, StoredFileName, OriginalFileName, FileUrl, ContentType, Extension, SizeInBytes, IsDeleted)
VALUES
    ('$fileId', '$fileName', '$fileName', 'https://mock.local/$fileName', 'application/pdf', '.pdf', 1024, 0);
INSERT INTO UserVerificationDocuments
    (Id, UserId, StoredFileId, DocumentType, Status, IsCurrent, IsDeleted, ExpirationDate)
VALUES
    (NEWID(), '$LawyerId', '$fileId', 0, 1, 1, 0, DATEADD(year, 1, GETUTCDATE()));
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $sql | Out-Null
    Assert-Test "$Label lawyer-owned stored-file fixture created" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) { throw "Stored-file fixture failed." }
    return [string]$fileId
}

function Create-ChangeRequest {
    param(
        [string]$Label,
        [string]$ContractId,
        [string]$MilestoneId,
        [string]$Token,
        [int]$DurationDays,
        [string]$Description
    )
    $milestone = Get-Milestone "$Label list for milestone ETag" `
        $ContractId $MilestoneId $Token
    $response = Invoke-TestRequest "$Label create change request" POST `
        "/api/milestones/$MilestoneId/change-requests" -Token $Token `
        -ExtraHeaders @{ "If-Match" = $milestone.version } -Body (
        @{
            ProposedDescription = $Description
            ProposedDurationDays = $DurationDays
            Reason = "سبب واضح ومحدد لطلب تعديل شروط المرحلة."
        } | ConvertTo-Json)
    Require-Api "$Label create change request" $response @(201)
    return [string]$response.Json.data.entityId
}

try {
    Write-Section "Health and Milestones authorization boundary"
    $health = Invoke-TestRequest "API is healthy" GET "/health"
    Assert-Test "API is healthy" ($health.Status -eq 200) `
        "(status=$($health.Status))"
    $unknownId = [guid]::NewGuid()
    foreach ($test in @(
        @{ Name = "Add requires authentication"; Method = "POST"; Path = "/api/contracts/$unknownId/milestones"; Body = "{}" },
        @{ Name = "List requires authentication"; Method = "GET"; Path = "/api/contracts/$unknownId/milestones"; Body = "" },
        @{ Name = "Update requires authentication"; Method = "PUT"; Path = "/api/contracts/$unknownId/milestones/$unknownId"; Body = "{}" },
        @{ Name = "Approve requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/approve"; Body = "{}" },
        @{ Name = "Ready requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/ready-for-funding"; Body = "{}" },
        @{ Name = "Submit requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/submit"; Body = "{}" },
        @{ Name = "Accept requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/accept"; Body = "{}" },
        @{ Name = "Request changes requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/request-changes"; Body = "{}" },
        @{ Name = "Create change request requires authentication"; Method = "POST"; Path = "/api/milestones/$unknownId/change-requests"; Body = "{}" },
        @{ Name = "Approve change request requires authentication"; Method = "POST"; Path = "/api/change-requests/$unknownId/approve"; Body = "{}" },
        @{ Name = "Reject change request requires authentication"; Method = "POST"; Path = "/api/change-requests/$unknownId/reject"; Body = "{}" },
        @{ Name = "Cancel change request requires authentication"; Method = "POST"; Path = "/api/change-requests/$unknownId/cancel"; Body = "{}" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path `
            -Body $test.Body
        Assert-Test $test.Name ($response.Status -eq 401) `
            "(status=$($response.Status))"
    }

    Write-Section "Zero-assumption accounts with mock Email confirmation"
    $stamp = Get-Date -Format "yyyyMMddHHmmssfff"
    $password = "Password123!"
    $adminLogin = Login-User "Login admin" "admin@smartcourt.com" "Admin@123"
    $adminToken = [string]$adminLogin.accessToken
    $client = Register-And-PrepareUser "client" `
        "milestones_client_$stamp@example.com" $password $adminToken 20
    $lawyer = Register-And-PrepareUser "lawyer" `
        "milestones_lawyer_$stamp@example.com" $password $adminToken 21
    $attacker = Register-And-PrepareUser "attacker" `
        "milestones_attacker_$stamp@example.com" $password $adminToken 22

    Write-Section "Foundation and add endpoint"
    $foundation = New-CaseProposal "milestones-primary" `
        $client.Token $lawyer.Token $lawyer.Id
    $contract = New-Contract "milestones-primary" `
        $foundation.ProposalId $lawyer.Token
    foreach ($case in @(
        @{ Name = "Add empty body"; Body = "{}" },
        @{ Name = "Add negative and past values"; Body = (@{ Title = "Bad"; OrderNumber = -1; Amount = -10; DurationDays = 0; DueDate = "2000-01-01T00:00:00Z" } | ConvertTo-Json) },
        @{ Name = "Add extreme title"; Body = (@{ Title = ("x" * 201); Description = "valid"; OrderNumber = 1; Amount = 10; DurationDays = 1 } | ConvertTo-Json) },
        @{ Name = "Add type mismatch"; Body = '{"title":"مرحلة","orderNumber":"bad","amount":"bad"}' },
        @{ Name = "Add hostile payload"; Body = (@{ Title = "<script>alert(1)</script>'' OR 1=1--"; Description = "Valid hostile-looking text"; OrderNumber = 0; Amount = 1.001; DurationDays = 366 } | ConvertTo-Json) }
    )) {
        $response = Invoke-TestRequest $case.Name POST `
            "/api/contracts/$($contract.Id)/milestones" `
            -Token $lawyer.Token -Body $case.Body
        Assert-Test "$($case.Name) returns 400" ($response.Status -eq 400) `
            "(status=$($response.Status))"
    }
    $addBody = @{
        Title = "المرحلة الأولى لتنفيذ الأعمال"
        Description = "وصف عربي شامل للمرحلة الأولى."
        OrderNumber = 1
        Amount = 1000.00
        DurationDays = 10
        DueDate = (Get-Date).ToUniversalTime().AddDays(20).ToString("o")
    } | ConvertTo-Json
    $attackerAdd = Invoke-TestRequest "Unrelated user cannot add milestone" POST `
        "/api/contracts/$($contract.Id)/milestones" `
        -Token $attacker.Token -Body $addBody
    Assert-Test "Unrelated add is forbidden" ($attackerAdd.Status -eq 403) `
        "(status=$($attackerAdd.Status))"
    $add = Invoke-TestRequest "Lawyer adds milestone" POST `
        "/api/contracts/$($contract.Id)/milestones" `
        -Token $lawyer.Token -Body $addBody
    Require-Api "Lawyer adds milestone" $add @(201)
    $milestoneId = [string]$add.Json.data.id
    $createdNotification = Find-MilestoneNotification `
        "Poll client for milestone creation" $client.Token `
        "milestone.created" $milestoneId
    Assert-MilestoneNotification "Client receives exact milestone creation" `
        $createdNotification "milestone.created" "Information" `
        "مرحلة تعاقدية جديدة" `
        "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId

    Write-Section "List, update, ownership, concurrency, and draft notifications"
    $clientList = Invoke-TestRequest "Client lists milestones" GET `
        "/api/contracts/$($contract.Id)/milestones" -Token $client.Token
    Require-Api "Client lists milestones" $clientList
    Assert-Test "Client list contains milestone" `
        ($null -ne (@($clientList.Json.data) | Where-Object id -eq $milestoneId))
    $attackerList = Invoke-TestRequest "Unrelated user cannot list milestones" GET `
        "/api/contracts/$($contract.Id)/milestones" -Token $attacker.Token
    Assert-Test "Unrelated list is forbidden" ($attackerList.Status -eq 403) `
        "(status=$($attackerList.Status))"
    $milestone = Get-Milestone "Get milestone for update" `
        $contract.Id $milestoneId $client.Token
    $missingIfMatch = Invoke-TestRequest "Update missing If-Match" PUT `
        "/api/contracts/$($contract.Id)/milestones/$milestoneId" `
        -Token $client.Token -Body (@{ Title = "عنوان صالح"; DurationDays = 12 } | ConvertTo-Json)
    Assert-Test "Update missing If-Match returns 412" `
        ($missingIfMatch.Status -in @(400, 412)) "(status=$($missingIfMatch.Status))"
    $attackerUpdate = Invoke-TestRequest "Unrelated user cannot update milestone" PUT `
        "/api/contracts/$($contract.Id)/milestones/$milestoneId" `
        -Token $attacker.Token -Body (@{ Title = "عنوان صالح"; DurationDays = 12 } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = $milestone.version }
    Assert-Test "Unrelated update is forbidden" ($attackerUpdate.Status -eq 403) `
        "(status=$($attackerUpdate.Status))"
    $update = Invoke-TestRequest "Client updates milestone draft" PUT `
        "/api/contracts/$($contract.Id)/milestones/$milestoneId" `
        -Token $client.Token -ExtraHeaders @{ "If-Match" = $milestone.version } `
        -Body (@{
            Title = "المرحلة الأولى بعد التحديث"
            Description = "وصف عربي محدث وآمن للمرحلة."
            DurationDays = 12
            DueDate = (Get-Date).ToUniversalTime().AddDays(25).ToString("o")
        } | ConvertTo-Json)
    Require-Api "Client updates milestone draft" $update
    $updatedNotification = Find-MilestoneNotification `
        "Poll lawyer for milestone update" $lawyer.Token `
        "milestone.draft-updated" $milestoneId
    Assert-MilestoneNotification "Lawyer receives exact milestone update" `
        $updatedNotification "milestone.draft-updated" "Warning" `
        "تم تحديث المرحلة" `
        "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId
    $staleUpdate = Invoke-TestRequest "Stale update is rejected" PUT `
        "/api/contracts/$($contract.Id)/milestones/$milestoneId" `
        -Token $client.Token -ExtraHeaders @{ "If-Match" = $milestone.version } `
        -Body (@{ Title = "عنوان آخر"; DurationDays = 13 } | ConvertTo-Json)
    Assert-Test "Stale update returns 412" ($staleUpdate.Status -in @(409, 412)) `
        "(status=$($staleUpdate.Status))"

    Write-Section "Participant approval and approved notifications"
    $milestone = Get-Milestone "Get milestone for lawyer approval" `
        $contract.Id $milestoneId $lawyer.Token
    $approveLawyer = Invoke-TestRequest "Lawyer approves milestone" POST `
        "/api/milestones/$milestoneId/approve" -Token $lawyer.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = $milestone.version }
    Require-Api "Lawyer approves milestone" $approveLawyer
    $acceptanceNotification = Find-MilestoneNotification `
        "Poll client for milestone acceptance" $client.Token `
        "milestone.acceptance-recorded" $milestoneId
    Assert-MilestoneNotification "Client receives first milestone approval" `
        $acceptanceNotification "milestone.acceptance-recorded" "Information" `
        "موافقة جديدة على المرحلة" `
        "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId
    $milestone = Get-Milestone "Get milestone for client approval" `
        $contract.Id $milestoneId $client.Token
    $attackerApprove = Invoke-TestRequest "Attacker cannot approve milestone" POST `
        "/api/milestones/$milestoneId/approve" -Token $attacker.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = $milestone.version }
    Assert-Test "Attacker approval is forbidden" ($attackerApprove.Status -eq 403) `
        "(status=$($attackerApprove.Status))"
    $approveClient = Invoke-TestRequest "Client completes milestone approval" POST `
        "/api/milestones/$milestoneId/approve" -Token $client.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = $milestone.version }
    Require-Api "Client completes milestone approval" $approveClient
    foreach ($entry in @(
        @{ Name = "Client receives milestone approved"; Token = $client.Token },
        @{ Name = "Lawyer receives milestone approved"; Token = $lawyer.Token }
    )) {
        $notification = Find-MilestoneNotification `
            "Poll for $($entry.Name)" $entry.Token `
            "milestone.approved" $milestoneId
        Assert-MilestoneNotification $entry.Name $notification `
            "milestone.approved" "Success" "تم اعتماد المرحلة" `
            "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل." `
            $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId
    }

    Write-Section "Activation, ready-for-funding, and funded execution"
    Accept-ContractBoth "milestones-primary" $contract.Id `
        $client.Token $lawyer.Token
    $milestone = Get-Milestone "Get milestone for ready funding" `
        $contract.Id $milestoneId $lawyer.Token
    $clientReady = Invoke-TestRequest "Client cannot mark ready" POST `
        "/api/milestones/$milestoneId/ready-for-funding" `
        -Token $client.Token -Body "{}" `
        -ExtraHeaders @{ "If-Match" = $milestone.version }
    Assert-Test "Client ready-for-funding is forbidden" `
        ($clientReady.Status -eq 403) "(status=$($clientReady.Status))"
    Mark-ReadyAndFund "milestones-primary" $contract.Id $milestoneId `
        $client.Token $lawyer.Token
    $readyNotification = Find-MilestoneNotification `
        "Poll client for ready funding" $client.Token `
        "milestone.ready-for-funding" $milestoneId
    Assert-MilestoneNotification "Client receives ready-for-funding" `
        $readyNotification "milestone.ready-for-funding" "Information" `
        "المرحلة جاهزة للتمويل" `
        "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId

    Write-Section "Formal change-request endpoints and notifications"
    $invalidCr = Invoke-TestRequest "Empty change request is invalid" POST `
        "/api/milestones/$milestoneId/change-requests" -Token $client.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = (Get-Milestone "Get for invalid CR" $contract.Id $milestoneId $client.Token).version }
    Assert-Test "Empty change request returns 400" ($invalidCr.Status -eq 400) `
        "(status=$($invalidCr.Status))"
    $crApprovedId = Create-ChangeRequest "client CR for approval" `
        $contract.Id $milestoneId $client.Token 20 `
        "وصف جديد بعد موافقة الطرف الآخر."
    $crCreatedForLawyer = Find-MilestoneNotification `
        "Poll lawyer for created CR" $lawyer.Token `
        "milestone.change-request-created" $milestoneId $crApprovedId
    Assert-MilestoneNotification "Lawyer receives created change request" `
        $crCreatedForLawyer "milestone.change-request-created" "Information" `
        "طلب تعديل جديد للمرحلة" `
        "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId `
        $crApprovedId
    $approveCr = Invoke-TestRequest "Lawyer approves change request" POST `
        "/api/change-requests/$crApprovedId/approve" -Token $lawyer.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = (Get-ChangeRequestETag $crApprovedId) }
    Require-Api "Lawyer approves change request" $approveCr
    $approvedCrNotification = Find-MilestoneNotification `
        "Poll client for approved CR" $client.Token `
        "milestone.change-request-approved" $milestoneId $crApprovedId
    Assert-MilestoneNotification "Client receives approved change request" `
        $approvedCrNotification "milestone.change-request-approved" "Success" `
        "تمت الموافقة على طلب التعديل" `
        "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId `
        $crApprovedId

    $crRejectedId = Create-ChangeRequest "lawyer CR for rejection" `
        $contract.Id $milestoneId $lawyer.Token 25 `
        "وصف يقترحه المحامي ليختبر الرفض."
    $rejectMissing = Invoke-TestRequest "Reject CR missing reason" POST `
        "/api/change-requests/$crRejectedId/reject" -Token $client.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = (Get-ChangeRequestETag $crRejectedId) }
    Assert-Test "Reject CR missing reason returns 400" `
        ($rejectMissing.Status -eq 400) "(status=$($rejectMissing.Status))"
    $rejectCr = Invoke-TestRequest "Client rejects change request" POST `
        "/api/change-requests/$crRejectedId/reject" -Token $client.Token `
        -Body (@{ Reason = "لا تتوافق التعديلات المقترحة مع نطاق المرحلة." } | ConvertTo-Json) `
        -ExtraHeaders @{ "If-Match" = (Get-ChangeRequestETag $crRejectedId) }
    Require-Api "Client rejects change request" $rejectCr
    $rejectedCrNotification = Find-MilestoneNotification `
        "Poll lawyer for rejected CR" $lawyer.Token `
        "milestone.change-request-rejected" $milestoneId $crRejectedId
    Assert-MilestoneNotification "Lawyer receives rejected change request" `
        $rejectedCrNotification "milestone.change-request-rejected" "Warning" `
        "تم رفض طلب تعديل المرحلة" `
        "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId `
        $crRejectedId

    $crCancelledId = Create-ChangeRequest "client CR for cancellation" `
        $contract.Id $milestoneId $client.Token 30 `
        "وصف مؤقت سيقوم العميل بإلغاء طلبه."
    $cancelOther = Invoke-TestRequest "Counterparty cannot cancel CR" POST `
        "/api/change-requests/$crCancelledId/cancel" -Token $lawyer.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = (Get-ChangeRequestETag $crCancelledId) }
    Assert-Test "Counterparty cancellation is forbidden" `
        ($cancelOther.Status -eq 403) "(status=$($cancelOther.Status))"
    $cancelCr = Invoke-TestRequest "Client cancels own change request" POST `
        "/api/change-requests/$crCancelledId/cancel" -Token $client.Token `
        -Body "{}" -ExtraHeaders @{ "If-Match" = (Get-ChangeRequestETag $crCancelledId) }
    Require-Api "Client cancels own change request" $cancelCr
    $cancelledCrNotification = Find-MilestoneNotification `
        "Poll lawyer for cancelled CR" $lawyer.Token `
        "milestone.change-request-cancelled" $milestoneId $crCancelledId
    Assert-MilestoneNotification "Lawyer receives cancelled change request" `
        $cancelledCrNotification "milestone.change-request-cancelled" "Information" `
        "تم إلغاء طلب تعديل المرحلة" `
        "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId `
        $crCancelledId

    Write-Section "Submission, requested changes, resubmission, and manual acceptance"
    $fileId = Add-LawyerFileFixture $lawyer.Id "manual"
    $submitInvalid = Invoke-TestRequest "Submit missing files" POST `
        "/api/milestones/$milestoneId/submit" -Token $lawyer.Token `
        -Body (@{ Notes = "ملاحظات صالحة"; StoredFileIds = @() } | ConvertTo-Json)
    Assert-Test "Submit missing files returns 400" `
        ($submitInvalid.Status -eq 400) "(status=$($submitInvalid.Status))"
    $clientSubmit = Invoke-TestRequest "Client cannot submit work" POST `
        "/api/milestones/$milestoneId/submit" -Token $client.Token `
        -Body (@{ Notes = "محاولة غير مصرح بها"; StoredFileIds = @($fileId) } | ConvertTo-Json)
    Assert-Test "Client submit is forbidden" ($clientSubmit.Status -eq 403) `
        "(status=$($clientSubmit.Status))"
    $submit = Invoke-TestRequest "Lawyer submits milestone work" POST `
        "/api/milestones/$milestoneId/submit" -Token $lawyer.Token `
        -Body (@{ Notes = "اكتملت أعمال المرحلة وأصبحت جاهزة للمراجعة."; StoredFileIds = @($fileId) } | ConvertTo-Json)
    Require-Api "Lawyer submits milestone work" $submit
    $submittedNotification = Find-MilestoneNotification `
        "Poll client for submission" $client.Token `
        "milestone.submitted" $milestoneId
    Assert-MilestoneNotification "Client receives submission" `
        $submittedNotification "milestone.submitted" "Information" `
        "تم تسليم أعمال المرحلة" `
        "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId
    $requestMissing = Invoke-TestRequest "Request changes missing reason" POST `
        "/api/milestones/$milestoneId/request-changes" `
        -Token $client.Token -Body "{}"
    Assert-Test "Request changes missing reason returns 400" `
        ($requestMissing.Status -eq 400) "(status=$($requestMissing.Status))"
    $requestChanges = Invoke-TestRequest "Client requests work changes" POST `
        "/api/milestones/$milestoneId/request-changes" `
        -Token $client.Token -Body (@{ Reason = "يرجى استكمال المستند الختامي." } | ConvertTo-Json)
    Require-Api "Client requests work changes" $requestChanges
    $changesNotification = Find-MilestoneNotification `
        "Poll lawyer for requested changes" $lawyer.Token `
        "milestone.changes-requested" $milestoneId
    Assert-MilestoneNotification "Lawyer receives requested changes" `
        $changesNotification "milestone.changes-requested" "Warning" `
        "طُلبت تعديلات على المرحلة" `
        "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId
    $resubmit = Invoke-TestRequest "Lawyer resubmits milestone work" POST `
        "/api/milestones/$milestoneId/submit" -Token $lawyer.Token `
        -Body (@{ Notes = "تم استكمال التعديلات المطلوبة."; StoredFileIds = @($fileId) } | ConvertTo-Json)
    Require-Api "Lawyer resubmits milestone work" $resubmit
    $lawyerAccept = Invoke-TestRequest "Lawyer cannot accept submission" POST `
        "/api/milestones/$milestoneId/accept" -Token $lawyer.Token -Body "{}"
    Assert-Test "Lawyer acceptance is forbidden" ($lawyerAccept.Status -eq 403) `
        "(status=$($lawyerAccept.Status))"
    $accept = Invoke-TestRequest "Client accepts milestone work" POST `
        "/api/milestones/$milestoneId/accept" -Token $client.Token -Body "{}"
    Require-Api "Client accepts milestone work" $accept
    $acceptedNotification = Find-MilestoneNotification `
        "Poll lawyer for manual acceptance" $lawyer.Token `
        "milestone.accepted" $milestoneId
    Assert-MilestoneNotification "Lawyer receives manual acceptance" `
        $acceptedNotification "milestone.accepted" "Success" `
        "تم قبول أعمال المرحلة" `
        "قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف." `
        $milestoneId $contract.Id $foundation.ProposalId $foundation.CaseId

    Write-Section "Automatic acceptance through accelerated Hangfire schedule"
    $autoFoundation = New-CaseProposal "milestones-auto" `
        $client.Token $lawyer.Token $lawyer.Id
    $autoContract = New-Contract "milestones-auto" `
        $autoFoundation.ProposalId $lawyer.Token
    $autoMilestoneId = New-ApprovedMilestone "milestones-auto" `
        $autoContract.Id $client.Token $lawyer.Token
    Accept-ContractBoth "milestones-auto" $autoContract.Id `
        $client.Token $lawyer.Token
    Mark-ReadyAndFund "milestones-auto" $autoContract.Id $autoMilestoneId `
        $client.Token $lawyer.Token
    $autoFileId = Add-LawyerFileFixture $lawyer.Id "auto"
    $autoSubmit = Invoke-TestRequest "Submit auto-accept milestone" POST `
        "/api/milestones/$autoMilestoneId/submit" -Token $lawyer.Token `
        -Body (@{ Notes = "تسليم لاختبار القبول التلقائي."; StoredFileIds = @($autoFileId) } | ConvertTo-Json)
    Require-Api "Submit auto-accept milestone" $autoSubmit
    $scheduledJobId = ""
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        $scheduledJobLine = (& sqlcmd -S $SqlServer -d $SqlDatabase -b -W -h -1 -Q `
            "SET NOCOUNT ON; SELECT AutoAcceptJobId FROM Milestones WHERE Id = '$autoMilestoneId';") |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -First 1
        $candidateJobId = if ($scheduledJobLine) {
            $scheduledJobLine.Trim()
        } else { "" }
        $scheduledJobId = if ($candidateJobId -match '^\d+$') {
            $candidateJobId
        } else { "" }
        if ($scheduledJobId) { break }
        Start-Sleep -Milliseconds 750
    }
    Assert-Test "Auto-accept Hangfire job was scheduled" `
        (-not [string]::IsNullOrWhiteSpace($scheduledJobId))
    if (-not $scheduledJobId) { throw "Auto-accept job was not scheduled." }
    $accelerateSql = @"
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY
DECLARE @ScheduledJobId nvarchar(100);
SELECT @ScheduledJobId = AutoAcceptJobId
FROM Milestones
WHERE Id = '$autoMilestoneId';
UPDATE Milestones
SET AutoAcceptEligibleAt = DATEADD(second, -5, GETUTCDATE())
WHERE Id = '$autoMilestoneId';
UPDATE [HangFire].[Set]
SET Score = 0
WHERE [Key] = N'schedule' AND [Value] = @ScheduledJobId;
IF @@ROWCOUNT <> 1
    THROW 51000, 'Expected exactly one disposable Hangfire schedule row.', 1;
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
"@
    & sqlcmd -S $SqlServer -d $SqlDatabase -b -Q $accelerateSql | Out-Null
    Assert-Test "Auto-accept schedule accelerated by scoped test fixture" `
        ($LASTEXITCODE -eq 0)
    if ($LASTEXITCODE -ne 0) { throw "Could not accelerate auto-accept job." }
    $autoClientNotification = Find-MilestoneNotification `
        "Poll client for automatic acceptance" $client.Token `
        "milestone.auto-accepted" $autoMilestoneId "" 120
    $autoLawyerNotification = Find-MilestoneNotification `
        "Poll lawyer for automatic acceptance" $lawyer.Token `
        "milestone.auto-accepted" $autoMilestoneId "" 120
    Assert-MilestoneNotification "Client receives role-specific auto acceptance" `
        $autoClientNotification "milestone.auto-accepted" "Warning" `
        "تم قبول المرحلة تلقائيًا" `
        "انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض." `
        $autoMilestoneId $autoContract.Id $autoFoundation.ProposalId `
        $autoFoundation.CaseId
    Assert-MilestoneNotification "Lawyer receives role-specific auto acceptance" `
        $autoLawyerNotification "milestone.auto-accepted" "Success" `
        "تم قبول المرحلة تلقائيًا" `
        "قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ." `
        $autoMilestoneId $autoContract.Id $autoFoundation.ProposalId `
        $autoFoundation.CaseId

    Write-Section "Unsupported methods and recipient isolation"
    foreach ($test in @(
        @{ Name = "DELETE milestone collection unsupported"; Method = "DELETE"; Path = "/api/contracts/$($contract.Id)/milestones" },
        @{ Name = "PATCH milestone unsupported"; Method = "PATCH"; Path = "/api/contracts/$($contract.Id)/milestones/$milestoneId" },
        @{ Name = "DELETE change request unsupported"; Method = "DELETE"; Path = "/api/change-requests/$crApprovedId" }
    )) {
        $response = Invoke-TestRequest $test.Name $test.Method $test.Path `
            -Token $client.Token
        Assert-Test $test.Name ($response.Status -in @(404, 405)) `
            "(status=$($response.Status))"
    }
    $attackerNotifications = Invoke-TestRequest `
        "Get unrelated user notifications" GET `
        "/api/notifications?pageSize=50" -Token $attacker.Token
    $leaked = @($attackerNotifications.Json.data.items | Where-Object {
        $_.type -like "milestone.*" -and
        $_.data.milestoneId -in @($milestoneId, $autoMilestoneId)
    })
    Assert-Test "No Milestone notification leaks to unrelated user" `
        ($attackerNotifications.Status -eq 200 -and $leaked.Count -eq 0)
}
catch {
    $script:failed++
    $script:failureMessages.Add("FATAL: $($_.Exception.Message)")
    Write-Host "FATAL: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Section "Execution summary"
    "| Metric | Count |`n|---|---:|`n| Passed assertions | $script:passed |`n| Failed assertions | $script:failed |" |
        Out-File $ReportFile -Append -Encoding utf8
    if ($script:failureMessages.Count -gt 0) {
        "`n### Failures`n" | Out-File $ReportFile -Append -Encoding utf8
        foreach ($failure in $script:failureMessages) {
            "- $(Protect-ReportText $failure)" |
                Out-File $ReportFile -Append -Encoding utf8
        }
    }
    Write-Host "`nMilestones notification HTTP tests complete: $script:passed passed, $script:failed failed."
}

if ($script:failed -gt 0) { exit 1 }
