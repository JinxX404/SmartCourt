using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialManualActionEscalation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ManualActionRequiredAt",
                table: "WithdrawalRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualAction",
                table: "WithdrawalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualActionRequiredAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [PaymentTransactions]
                SET [ManualActionRequiredAt] = [UpdatedAt]
                WHERE [RequiresManualAction] = 1
                  AND [ManualActionRequiredAt] IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_ReconciliationQueue",
                table: "WithdrawalRequests",
                columns: new[] { "Status", "RequiresManualAction", "RequestedAt", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawalRequests_ManualActionTimestamp",
                table: "WithdrawalRequests",
                sql: "[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReconciliationQueue",
                table: "PaymentTransactions",
                columns: new[] { "Status", "RequiresManualAction", "CreatedAt", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentTransactions_ManualActionTimestamp",
                table: "PaymentTransactions",
                sql: "[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_ReconciliationQueue",
                table: "WithdrawalRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WithdrawalRequests_ManualActionTimestamp",
                table: "WithdrawalRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ReconciliationQueue",
                table: "PaymentTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentTransactions_ManualActionTimestamp",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ManualActionRequiredAt",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "RequiresManualAction",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ManualActionRequiredAt",
                table: "PaymentTransactions");
        }
    }
}
