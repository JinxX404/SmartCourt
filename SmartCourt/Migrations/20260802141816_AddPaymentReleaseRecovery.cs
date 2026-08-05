using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReleaseRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderAttemptCount",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualAction",
                table: "PaymentTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE [PaymentTransactions]
                SET [ProviderAttemptCount] = 1,
                    [NextRetryAt] = SYSUTCDATETIME()
                WHERE [OperationType] = 1
                  AND [Status] = 2;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReleaseRecovery",
                table: "PaymentTransactions",
                columns: new[] { "Status", "OperationType", "RequiresManualAction", "NextRetryAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentTransactions_ProviderAttemptCount_NonNegative",
                table: "PaymentTransactions",
                sql: "[ProviderAttemptCount] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ReleaseRecovery",
                table: "PaymentTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentTransactions_ProviderAttemptCount_NonNegative",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderAttemptCount",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RequiresManualAction",
                table: "PaymentTransactions");
        }
    }
}
