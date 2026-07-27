using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class ContractAndPaymentV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AcceptedByClientAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedByLawyerAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TerminatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.CheckConstraint("CK_Contracts_Currency_EGP", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_Contracts_Status_Range", "[Status] BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_Contracts_AspNetUsers_ClientUserId",
                        column: x => x.ClientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_AspNetUsers_LawyerUserId",
                        column: x => x.LawyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_AspNetUsers_TerminatedByUserId",
                        column: x => x.TerminatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Operation = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ResourceType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestHash = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: true),
                    ResultReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                    table.CheckConstraint("CK_IdempotencyRecords_Status_Range", "[Status] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_IdempotencyRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawyerWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    PendingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerWallets", x => x.Id);
                    table.CheckConstraint("CK_LawyerWallets_Balances_NonNegative", "[PendingBalance] >= 0 AND [AvailableBalance] >= 0");
                    table.CheckConstraint("CK_LawyerWallets_Currency_EGP", "[Currency] = 'EGP'");
                    table.ForeignKey(
                        name: "FK_LawyerWallets_AspNetUsers_LawyerUserId",
                        column: x => x.LawyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    AggregateType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AvailableAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                    table.CheckConstraint("CK_OutboxMessages_Attempts_NonNegative", "[Attempts] >= 0");
                    table.CheckConstraint("CK_OutboxMessages_EventVersion_Positive", "[EventVersion] > 0");
                    table.CheckConstraint("CK_OutboxMessages_Status_Range", "[Status] BETWEEN 0 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.CheckConstraint("CK_WithdrawalRequests_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_WithdrawalRequests_Currency_EGP", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_WithdrawalRequests_Status_Range", "[Status] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_AspNetUsers_LawyerUserId",
                        column: x => x.LawyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractStateHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractStateHistories", x => x.Id);
                    table.CheckConstraint("CK_ContractStateHistories_NewStatus_Range", "[NewStatus] BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_ContractStateHistories_PreviousStatus_Range", "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_ContractStateHistories_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractStateHistories_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscrowAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    TotalDeposited = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalReleased = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalRefunded = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFees = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowAccounts", x => x.Id);
                    table.CheckConstraint("CK_EscrowAccounts_Currency_EGP", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_EscrowAccounts_NonNegativeTotals", "[TotalDeposited] >= 0 AND [TotalReleased] >= 0 AND [TotalRefunded] >= 0 AND [TotalFees] >= 0");
                    table.CheckConstraint("CK_EscrowAccounts_Status_Range", "[Status] BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_EscrowAccounts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AcceptedByClientAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedByLawyerAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadyForFundingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoAcceptEligibleAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoAcceptJobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptanceSource = table.Column<int>(type: "int", nullable: true),
                    HoldStartsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoldExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmissionVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                    table.CheckConstraint("CK_Milestones_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_Milestones_DurationDays_Range", "[DurationDays] IS NULL OR [DurationDays] BETWEEN 1 AND 365");
                    table.CheckConstraint("CK_Milestones_OrderNumber_Positive", "[OrderNumber] > 0");
                    table.CheckConstraint("CK_Milestones_Status_Range", "[Status] BETWEEN 0 AND 9");
                    table.CheckConstraint("CK_Milestones_SubmissionVersion_Positive", "[SubmissionVersion] >= 0");
                    table.ForeignKey(
                        name: "FK_Milestones_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaisedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedModeratorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedOutcome = table.Column<int>(type: "int", nullable: false),
                    ResolutionType = table.Column<int>(type: "int", nullable: true),
                    ResolutionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ResolutionSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.CheckConstraint("CK_Disputes_Category_Range", "[Category] BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_Disputes_RequestedOutcome_Range", "[RequestedOutcome] BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_Disputes_ResolutionType_Range", "[ResolutionType] IS NULL OR [ResolutionType] BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_Disputes_Status_Range", "[Status] BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_Disputes_AspNetUsers_AssignedModeratorUserId",
                        column: x => x.AssignedModeratorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputes_AspNetUsers_RaisedByUserId",
                        column: x => x.RaisedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputes_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputes_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputes_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscrowHolds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscrowAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FundedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoldStartsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoldExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FrozenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettlementType = table.Column<int>(type: "int", nullable: true),
                    ProviderDepositTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderReleaseTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderRefundTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowHolds", x => x.Id);
                    table.CheckConstraint("CK_EscrowHolds_FeesAndNet_NonNegative", "[PlatformFeeAmount] >= 0 AND [NetAmount] >= 0");
                    table.CheckConstraint("CK_EscrowHolds_FundedStateRequiresTimestamp", "[Status] <> 0 OR [FundedAt] IS NOT NULL");
                    table.CheckConstraint("CK_EscrowHolds_GrossAmount_Positive", "[GrossAmount] > 0");
                    table.CheckConstraint("CK_EscrowHolds_Reconciliation", "[GrossAmount] = [PlatformFeeAmount] + [NetAmount]");
                    table.CheckConstraint("CK_EscrowHolds_Status_Range", "[Status] BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_EscrowHolds_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowHolds_EscrowAccounts_EscrowAccountId",
                        column: x => x.EscrowAccountId,
                        principalTable: "EscrowAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowHolds_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposedDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    ProposedDurationDays = table.Column<int>(type: "int", nullable: true),
                    ProposedDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneChangeRequests", x => x.Id);
                    table.CheckConstraint("CK_MilestoneChangeRequests_DurationDays_Range", "[ProposedDurationDays] IS NULL OR [ProposedDurationDays] BETWEEN 1 AND 365");
                    table.CheckConstraint("CK_MilestoneChangeRequests_Status_Range", "[Status] BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_MilestoneChangeRequests_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestoneChangeRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestoneChangeRequests_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneStateHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneStateHistories", x => x.Id);
                    table.CheckConstraint("CK_MilestoneStateHistories_NewStatus_Range", "[NewStatus] BETWEEN 0 AND 9");
                    table.CheckConstraint("CK_MilestoneStateHistories_PreviousStatus_Range", "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 9");
                    table.ForeignKey(
                        name: "FK_MilestoneStateHistories_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestoneStateHistories_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisputeEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisputeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeEvidence", x => x.Id);
                    table.CheckConstraint("CK_DisputeEvidence_FileOrContent", "[StoredFileId] IS NOT NULL OR [Content] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_DisputeEvidence_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisputeEvidence_Disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "Disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisputeEvidence_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisputeResolutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisputeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolutionType = table.Column<int>(type: "int", nullable: false),
                    GrossHoldAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClientRefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LawyerReleaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeResolutions", x => x.Id);
                    table.CheckConstraint("CK_DisputeResolutions_Amounts_NonNegative", "[GrossHoldAmount] >= 0 AND [ClientRefundAmount] >= 0 AND [LawyerReleaseAmount] >= 0 AND [PlatformFeeAmount] >= 0");
                    table.CheckConstraint("CK_DisputeResolutions_Reconciliation", "[GrossHoldAmount] = [ClientRefundAmount] + [LawyerReleaseAmount] + [PlatformFeeAmount]");
                    table.CheckConstraint("CK_DisputeResolutions_ResolutionType_Range", "[ResolutionType] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_DisputeResolutions_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisputeResolutions_Disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "Disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawyerPenalties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisputeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PenaltyType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerPenalties", x => x.Id);
                    table.CheckConstraint("CK_LawyerPenalties_EndAfterStart", "[EndsAt] IS NULL OR [EndsAt] >= [StartsAt]");
                    table.CheckConstraint("CK_LawyerPenalties_Type_Range", "[PenaltyType] BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_LawyerPenalties_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LawyerPenalties_AspNetUsers_LawyerUserId",
                        column: x => x.LawyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LawyerPenalties_Disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "Disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscrowHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneSubmissions", x => x.Id);
                    table.CheckConstraint("CK_MilestoneSubmissions_Version_Positive", "[Version] > 0");
                    table.ForeignKey(
                        name: "FK_MilestoneSubmissions_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestoneSubmissions_EscrowHolds_EscrowHoldId",
                        column: x => x.EscrowHoldId,
                        principalTable: "EscrowHolds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestoneSubmissions_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EscrowHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.CheckConstraint("CK_PaymentTransactions_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_PaymentTransactions_CompletedDepositRequiresHold", "NOT ([OperationType] = 0 AND [Status] = 1) OR [EscrowHoldId] IS NOT NULL");
                    table.CheckConstraint("CK_PaymentTransactions_Currency_EGP", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_PaymentTransactions_MilestoneRequiredForMoneyOperations", "[OperationType] = 3 OR [MilestoneId] IS NOT NULL");
                    table.CheckConstraint("CK_PaymentTransactions_OperationType_Range", "[OperationType] BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_PaymentTransactions_Status_Range", "[Status] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_EscrowHolds_EscrowHoldId",
                        column: x => x.EscrowHoldId,
                        principalTable: "EscrowHolds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneSubmissionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneSubmissionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilestoneSubmissionAttachments_MilestoneSubmissions_MilestoneSubmissionId",
                        column: x => x.MilestoneSubmissionId,
                        principalTable: "MilestoneSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MilestoneSubmissionAttachments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscrowLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscrowAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscrowHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RunningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "EGP"),
                    ReferenceType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowLedgerEntries", x => x.Id);
                    table.CheckConstraint("CK_EscrowLedgerEntries_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_EscrowLedgerEntries_Currency_EGP", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_EscrowLedgerEntries_RunningBalance_NonNegative", "[RunningBalance] >= 0");
                    table.CheckConstraint("CK_EscrowLedgerEntries_TransactionType_Range", "[TransactionType] BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_EscrowLedgerEntries_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowLedgerEntries_EscrowAccounts_EscrowAccountId",
                        column: x => x.EscrowAccountId,
                        principalTable: "EscrowAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowLedgerEntries_EscrowHolds_EscrowHoldId",
                        column: x => x.EscrowHoldId,
                        principalTable: "EscrowHolds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowLedgerEntries_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_ContractId",
                table: "ContractAttachments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_StoredFileId",
                table: "ContractAttachments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_UploadedByUserId",
                table: "ContractAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientUserId",
                table: "Contracts",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_LawyerUserId",
                table: "Contracts",
                column: "LawyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Status",
                table: "Contracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TerminatedByUserId",
                table: "Contracts",
                column: "TerminatedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_Contracts_ProposalId",
                table: "Contracts",
                column: "ProposalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractStateHistories_ActorUserId",
                table: "ContractStateHistories",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStateHistories_ContractId_CreatedAt",
                table: "ContractStateHistories",
                columns: new[] { "ContractId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DisputeEvidence_DisputeId",
                table: "DisputeEvidence",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeEvidence_StoredFileId",
                table: "DisputeEvidence",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeEvidence_UploadedByUserId",
                table: "DisputeEvidence",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeResolutions_ResolvedByUserId",
                table: "DisputeResolutions",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_DisputeResolutions_DisputeId",
                table: "DisputeResolutions",
                column: "DisputeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_AssignedModeratorUserId",
                table: "Disputes",
                column: "AssignedModeratorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ContractId",
                table: "Disputes",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_RaisedByUserId",
                table: "Disputes",
                column: "RaisedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ResolvedByUserId",
                table: "Disputes",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_Status_CreatedAt",
                table: "Disputes",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Disputes_OpenPerMilestone",
                table: "Disputes",
                columns: new[] { "MilestoneId", "Status" },
                unique: true,
                filter: "[Status] IN (0, 1, 2)");

            migrationBuilder.CreateIndex(
                name: "UX_EscrowAccounts_ContractId",
                table: "EscrowAccounts",
                column: "ContractId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHolds_ContractId",
                table: "EscrowHolds",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHolds_EscrowAccountId",
                table: "EscrowHolds",
                column: "EscrowAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHolds_HoldExpiresAt_Status",
                table: "EscrowHolds",
                columns: new[] { "HoldExpiresAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_EscrowHolds_MilestoneId",
                table: "EscrowHolds",
                column: "MilestoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscrowLedgerEntries_AccountId_CreatedAt",
                table: "EscrowLedgerEntries",
                columns: new[] { "EscrowAccountId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EscrowLedgerEntries_CreatedByUserId",
                table: "EscrowLedgerEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowLedgerEntries_EscrowHoldId",
                table: "EscrowLedgerEntries",
                column: "EscrowHoldId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowLedgerEntries_PaymentTransactionId",
                table: "EscrowLedgerEntries",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_Status_ExpiresAt",
                table: "IdempotencyRecords",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "UX_IdempotencyRecords_UserId_Key",
                table: "IdempotencyRecords",
                columns: new[] { "UserId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPenalties_CreatedByUserId",
                table: "LawyerPenalties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPenalties_DisputeId",
                table: "LawyerPenalties",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPenalties_LawyerUserId_StartsAt",
                table: "LawyerPenalties",
                columns: new[] { "LawyerUserId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "UX_LawyerWallets_LawyerUserId",
                table: "LawyerWallets",
                column: "LawyerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneChangeRequests_DecidedByUserId",
                table: "MilestoneChangeRequests",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneChangeRequests_RequestedByUserId",
                table: "MilestoneChangeRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_MilestoneChangeRequests_Pending",
                table: "MilestoneChangeRequests",
                columns: new[] { "MilestoneId", "Status" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_ContractId_Status",
                table: "Milestones",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_Status_AutoAcceptEligibleAt",
                table: "Milestones",
                columns: new[] { "Status", "AutoAcceptEligibleAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Milestones_ContractId_OrderNumber",
                table: "Milestones",
                columns: new[] { "ContractId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneStateHistories_ActorUserId",
                table: "MilestoneStateHistories",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneStateHistories_MilestoneId_CreatedAt",
                table: "MilestoneStateHistories",
                columns: new[] { "MilestoneId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneSubmissionAttachments_MilestoneSubmissionId",
                table: "MilestoneSubmissionAttachments",
                column: "MilestoneSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneSubmissionAttachments_StoredFileId",
                table: "MilestoneSubmissionAttachments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneSubmissions_EscrowHoldId",
                table: "MilestoneSubmissions",
                column: "EscrowHoldId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneSubmissions_SubmittedByUserId",
                table: "MilestoneSubmissions",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_MilestoneSubmissions_MilestoneId_Version",
                table: "MilestoneSubmissions",
                columns: new[] { "MilestoneId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Aggregate",
                table: "OutboxMessages",
                columns: new[] { "AggregateType", "AggregateId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_AvailableAt",
                table: "OutboxMessages",
                columns: new[] { "Status", "AvailableAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ContractId_Status",
                table: "PaymentTransactions",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_EscrowHoldId",
                table: "PaymentTransactions",
                column: "EscrowHoldId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MilestoneId_Status",
                table: "PaymentTransactions",
                columns: new[] { "MilestoneId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_PaymentTransactions_IdempotencyKey",
                table: "PaymentTransactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PaymentTransactions_ProviderTransaction",
                table: "PaymentTransactions",
                columns: new[] { "ProviderName", "ProviderTransactionId" },
                unique: true,
                filter: "[ProviderTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_LawyerUserId_Status",
                table: "WithdrawalRequests",
                columns: new[] { "LawyerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_WithdrawalRequests_IdempotencyKey",
                table: "WithdrawalRequests",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAttachments");

            migrationBuilder.DropTable(
                name: "ContractStateHistories");

            migrationBuilder.DropTable(
                name: "DisputeEvidence");

            migrationBuilder.DropTable(
                name: "DisputeResolutions");

            migrationBuilder.DropTable(
                name: "EscrowLedgerEntries");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "LawyerPenalties");

            migrationBuilder.DropTable(
                name: "LawyerWallets");

            migrationBuilder.DropTable(
                name: "MilestoneChangeRequests");

            migrationBuilder.DropTable(
                name: "MilestoneStateHistories");

            migrationBuilder.DropTable(
                name: "MilestoneSubmissionAttachments");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "MilestoneSubmissions");

            migrationBuilder.DropTable(
                name: "EscrowHolds");

            migrationBuilder.DropTable(
                name: "EscrowAccounts");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
