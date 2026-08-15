using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLawyerConsultations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultationOfferings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mode = table.Column<byte>(type: "tinyint", nullable: false),
                    Specialization = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    OfficeLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationOfferings", x => x.Id);
                    table.CheckConstraint("CK_ConsultationOfferings_Currency", "[Currency] = 'EGP'");
                    table.CheckConstraint("CK_ConsultationOfferings_Duration", "[DurationMinutes] BETWEEN 15 AND 240");
                    table.CheckConstraint("CK_ConsultationOfferings_Price", "[Price] > 0 AND [Price] <= 100000");
                    table.ForeignKey(
                        name: "FK_ConsultationOfferings_LawyerProfile_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "LawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawyerConsultationSettings",
                columns: table => new
                {
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinimumBookingNoticeHours = table.Column<int>(type: "int", nullable: false),
                    MaximumAdvanceBookingDays = table.Column<int>(type: "int", nullable: false),
                    BufferMinutes = table.Column<int>(type: "int", nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerConsultationSettings", x => x.LawyerId);
                    table.CheckConstraint("CK_LawyerConsultationSettings_Advance", "[MaximumAdvanceBookingDays] BETWEEN 1 AND 365");
                    table.CheckConstraint("CK_LawyerConsultationSettings_Buffer", "[BufferMinutes] BETWEEN 0 AND 120");
                    table.CheckConstraint("CK_LawyerConsultationSettings_Notice", "[MinimumBookingNoticeHours] BETWEEN 0 AND 168");
                    table.ForeignKey(
                        name: "FK_LawyerConsultationSettings_LawyerProfile_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "LawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationAvailabilitySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ReservedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationAvailabilitySlots", x => x.Id);
                    table.CheckConstraint("CK_ConsultationSlots_TimeRange", "[EndAtUtc] > [StartAtUtc]");
                    table.ForeignKey(
                        name: "FK_ConsultationAvailabilitySlots_ConsultationOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "ConsultationOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationAvailabilitySlots_LawyerProfile_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "LawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationOfferingInclusions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationOfferingInclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationOfferingInclusions_ConsultationOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "ConsultationOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mode = table.Column<byte>(type: "tinyint", nullable: false),
                    Specialization = table.Column<byte>(type: "tinyint", nullable: false),
                    OfferingTitle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    OfferingDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InclusionsJson = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LawyerNetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MatterSummary = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    OfficeLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetingUrl = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisputeReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationBookings", x => x.Id);
                    table.CheckConstraint("CK_ConsultationBookings_Amounts", "[GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [LawyerNetAmount]");
                    table.CheckConstraint("CK_ConsultationBookings_Currency", "[Currency] = 'EGP'");
                    table.ForeignKey(
                        name: "FK_ConsultationBookings_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationBookings_ConsultationAvailabilitySlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "ConsultationAvailabilitySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationBookings_ConsultationOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "ConsultationOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationBookings_LawyerProfile_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "LawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationPaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    RelatedProviderTransactionId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    ProviderStatus = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequiresManualAction = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationPaymentTransactions", x => x.Id);
                    table.CheckConstraint("CK_ConsultationPaymentTransactions_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_ConsultationPaymentTransactions_Currency", "[Currency] = 'EGP'");
                    table.ForeignKey(
                        name: "FK_ConsultationPaymentTransactions_ConsultationBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "ConsultationBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationEscrowHolds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepositTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FundedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoldStartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoldExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FrozenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationEscrowHolds", x => x.Id);
                    table.CheckConstraint("CK_ConsultationEscrowHolds_Amounts", "[GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [NetAmount]");
                    table.ForeignKey(
                        name: "FK_ConsultationEscrowHolds_ConsultationBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "ConsultationBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationEscrowHolds_ConsultationPaymentTransactions_DepositTransactionId",
                        column: x => x.DepositTransactionId,
                        principalTable: "ConsultationPaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RunningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationLedgerEntries", x => x.Id);
                    table.CheckConstraint("CK_ConsultationLedgerEntries_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_ConsultationLedgerEntries_ConsultationBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "ConsultationBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationLedgerEntries_ConsultationPaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "ConsultationPaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAvailabilitySlots_LawyerId_StartAtUtc_EndAtUtc",
                table: "ConsultationAvailabilitySlots",
                columns: new[] { "LawyerId", "StartAtUtc", "EndAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAvailabilitySlots_OfferingId_StartAtUtc",
                table: "ConsultationAvailabilitySlots",
                columns: new[] { "OfferingId", "StartAtUtc" },
                unique: true,
                filter: "[Status] <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationBookings_ClientId_Status_StartAtUtc",
                table: "ConsultationBookings",
                columns: new[] { "ClientId", "Status", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationBookings_LawyerId_Status_StartAtUtc",
                table: "ConsultationBookings",
                columns: new[] { "LawyerId", "Status", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationBookings_OfferingId",
                table: "ConsultationBookings",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationBookings_SlotId",
                table: "ConsultationBookings",
                column: "SlotId",
                unique: true,
                filter: "[Status] IN (0,1,2,3,6)");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationEscrowHolds_BookingId",
                table: "ConsultationEscrowHolds",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationEscrowHolds_DepositTransactionId",
                table: "ConsultationEscrowHolds",
                column: "DepositTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationLedgerEntries_BookingId_CreatedAt",
                table: "ConsultationLedgerEntries",
                columns: new[] { "BookingId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationLedgerEntries_PaymentTransactionId",
                table: "ConsultationLedgerEntries",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationOfferingInclusions_OfferingId_SortOrder",
                table: "ConsultationOfferingInclusions",
                columns: new[] { "OfferingId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationOfferings_LawyerId_IsActive",
                table: "ConsultationOfferings",
                columns: new[] { "LawyerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationOfferings_Mode_Specialization_IsActive",
                table: "ConsultationOfferings",
                columns: new[] { "Mode", "Specialization", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPaymentTransactions_BookingId_OperationType_Status",
                table: "ConsultationPaymentTransactions",
                columns: new[] { "BookingId", "OperationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPaymentTransactions_ProviderName_IdempotencyKey",
                table: "ConsultationPaymentTransactions",
                columns: new[] { "ProviderName", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPaymentTransactions_ProviderTransactionId",
                table: "ConsultationPaymentTransactions",
                column: "ProviderTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationEscrowHolds");

            migrationBuilder.DropTable(
                name: "ConsultationLedgerEntries");

            migrationBuilder.DropTable(
                name: "ConsultationOfferingInclusions");

            migrationBuilder.DropTable(
                name: "LawyerConsultationSettings");

            migrationBuilder.DropTable(
                name: "ConsultationPaymentTransactions");

            migrationBuilder.DropTable(
                name: "ConsultationBookings");

            migrationBuilder.DropTable(
                name: "ConsultationAvailabilitySlots");

            migrationBuilder.DropTable(
                name: "ConsultationOfferings");
        }
    }
}
