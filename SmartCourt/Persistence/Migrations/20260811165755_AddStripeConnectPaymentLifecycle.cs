using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeConnectPaymentLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LawyerPayoutAccountId",
                table: "WithdrawalRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderAccountId",
                table: "WithdrawalRequests",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProviderAmountMinor",
                table: "WithdrawalRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCurrency",
                table: "WithdrawalRequests",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderStatus",
                table: "WithdrawalRequests",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentTransactionId",
                table: "PaymentWebhookEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ConnectedAccountId",
                table: "PaymentWebhookEvents",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "PaymentWebhookEvents",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "PaymentWebhookEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingError",
                table: "PaymentWebhookEvents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCode",
                table: "PaymentWebhookEvents",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderObjectId",
                table: "PaymentWebhookEvents",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProviderAmountMinor",
                table: "PaymentTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCurrency",
                table: "PaymentTransactions",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderObjectType",
                table: "PaymentTransactions",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderRelatedTransactionId",
                table: "PaymentTransactions",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderStatus",
                table: "PaymentTransactions",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LawyerPayoutAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ProviderAccountId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DetailsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    TransfersEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PayoutsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsLive = table.Column<bool>(type: "bit", nullable: false),
                    Country = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    DefaultCurrency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    AvailableProviderAmountMinor = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    MaskedDestination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastProviderStatus = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    LastProviderErrorCode = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    LastSynchronizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerPayoutAccounts", x => x.Id);
                    table.CheckConstraint("CK_LawyerPayoutAccounts_ProviderBalance_NonNegative", "[AvailableProviderAmountMinor] >= 0");
                    table.CheckConstraint("CK_LawyerPayoutAccounts_Status_Range", "[Status] BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_LawyerPayoutAccounts_AspNetUsers_LawyerUserId",
                        column: x => x.LawyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_LawyerPayoutAccountId",
                table: "WithdrawalRequests",
                column: "LawyerPayoutAccountId");

            migrationBuilder.CreateIndex(
                name: "UX_LawyerPayoutAccounts_Lawyer_Provider",
                table: "LawyerPayoutAccounts",
                columns: new[] { "LawyerUserId", "ProviderCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_LawyerPayoutAccounts_ProviderAccount",
                table: "LawyerPayoutAccounts",
                columns: new[] { "ProviderCode", "ProviderAccountId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_LawyerPayoutAccounts_LawyerPayoutAccountId",
                table: "WithdrawalRequests",
                column: "LawyerPayoutAccountId",
                principalTable: "LawyerPayoutAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_LawyerPayoutAccounts_LawyerPayoutAccountId",
                table: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "LawyerPayoutAccounts");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_LawyerPayoutAccountId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "LawyerPayoutAccountId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ProviderAccountId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ProviderAmountMinor",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ProviderCurrency",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ProviderStatus",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ConnectedAccountId",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ProcessingError",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ProviderCode",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ProviderObjectId",
                table: "PaymentWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ProviderAmountMinor",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderCurrency",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderObjectType",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderRelatedTransactionId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderStatus",
                table: "PaymentTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentTransactionId",
                table: "PaymentWebhookEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
