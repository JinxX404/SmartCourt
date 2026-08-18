using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenBundlePurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenBundlePaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TokenAmount = table.Column<int>(type: "int", nullable: false),
                    PriceEgp = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelatedProviderTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBundlePaymentTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenBundlePaymentTransactions_ProviderName_IdempotencyKey",
                table: "TokenBundlePaymentTransactions",
                columns: new[] { "ProviderName", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenBundlePaymentTransactions_ProviderTransactionId",
                table: "TokenBundlePaymentTransactions",
                column: "ProviderTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenBundlePaymentTransactions");
        }
    }
}
