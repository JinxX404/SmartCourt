using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2TokenQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DailyLimit",
                table: "QuotaProfiles",
                newName: "DailyTokenLimit");

            migrationBuilder.RenameColumn(
                name: "AdditionalBalance",
                table: "QuotaLedgers",
                newName: "AdditionalTokenBalance");

            migrationBuilder.RenameColumn(
                name: "ConsumedRequests",
                table: "DailyUsages",
                newName: "ConsumedTokens");

            // Data Migration: Convert existing request-based data into tokens
            // Assume 10,000 tokens per request * 10 multiplier = 100,000 tokens per request. Let's use 20,000 as a safer average.
            migrationBuilder.Sql("UPDATE QuotaProfiles SET DailyTokenLimit = DailyTokenLimit * 20000;");
            migrationBuilder.Sql("UPDATE QuotaLedgers SET AdditionalTokenBalance = AdditionalTokenBalance * 20000;");
            migrationBuilder.Sql("UPDATE DailyUsages SET ConsumedTokens = ConsumedTokens * 20000;");

            migrationBuilder.CreateTable(
                name: "TokenUsageHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenUsageHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenUsageHistories_ClientId",
                table: "TokenUsageHistories",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenUsageHistories_ConversationId",
                table: "TokenUsageHistories",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse Data Migration
            migrationBuilder.Sql("UPDATE DailyUsages SET ConsumedTokens = ConsumedTokens / 20000;");
            migrationBuilder.Sql("UPDATE QuotaLedgers SET AdditionalTokenBalance = AdditionalTokenBalance / 20000;");
            migrationBuilder.Sql("UPDATE QuotaProfiles SET DailyTokenLimit = DailyTokenLimit / 20000;");

            migrationBuilder.DropTable(
                name: "TokenUsageHistories");

            migrationBuilder.RenameColumn(
                name: "DailyTokenLimit",
                table: "QuotaProfiles",
                newName: "DailyLimit");

            migrationBuilder.RenameColumn(
                name: "AdditionalTokenBalance",
                table: "QuotaLedgers",
                newName: "AdditionalBalance");

            migrationBuilder.RenameColumn(
                name: "ConsumedTokens",
                table: "DailyUsages",
                newName: "ConsumedRequests");
        }
    }
}
