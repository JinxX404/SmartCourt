using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLawyerSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LawyerDailyUsages",
                columns: table => new
                {
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsageDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConsumedTokens = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerDailyUsages", x => new { x.LawyerId, x.UsageDate });
                });

            migrationBuilder.CreateTable(
                name: "LawyerPaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PriceEgp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RelatedProviderTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerPaymentTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LawyerQuotaLedgers",
                columns: table => new
                {
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasedTokenBalance = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerQuotaLedgers", x => x.LawyerId);
                });

            migrationBuilder.CreateTable(
                name: "LawyerQuotaTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerQuotaTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LawyerSubscriptions",
                columns: table => new
                {
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    DailyTokenLimit = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerSubscriptions", x => x.LawyerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPaymentTransactions_ProviderName_IdempotencyKey",
                table: "LawyerPaymentTransactions",
                columns: new[] { "ProviderName", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPaymentTransactions_ProviderTransactionId",
                table: "LawyerPaymentTransactions",
                column: "ProviderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerQuotaTransactions_LawyerId",
                table: "LawyerQuotaTransactions",
                column: "LawyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawyerDailyUsages");

            migrationBuilder.DropTable(
                name: "LawyerPaymentTransactions");

            migrationBuilder.DropTable(
                name: "LawyerQuotaLedgers");

            migrationBuilder.DropTable(
                name: "LawyerQuotaTransactions");

            migrationBuilder.DropTable(
                name: "LawyerSubscriptions");
        }
    }
}
