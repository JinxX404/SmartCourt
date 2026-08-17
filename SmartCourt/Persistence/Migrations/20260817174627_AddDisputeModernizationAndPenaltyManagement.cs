using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeModernizationAndPenaltyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Disputes_Status_Range",
                table: "Disputes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractStateHistories_NewStatus_Range",
                table: "ContractStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractStateHistories_PreviousStatus_Range",
                table: "ContractStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Contracts_Status_Range",
                table: "Contracts");

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "LawyerPenalties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RevocationReason",
                table: "LawyerPenalties",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedAt",
                table: "LawyerPenalties",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevokedByUserId",
                table: "LawyerPenalties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Disputes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "Disputes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Disputes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousContractStatus",
                table: "Disputes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousMilestoneStatus",
                table: "Disputes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPenalties_RevokedByUserId",
                table: "LawyerPenalties",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_CancelledByUserId",
                table: "Disputes",
                column: "CancelledByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Disputes_Status_Range",
                table: "Disputes",
                sql: "[Status] BETWEEN 0 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractStateHistories_NewStatus_Range",
                table: "ContractStateHistories",
                sql: "[NewStatus] BETWEEN 0 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractStateHistories_PreviousStatus_Range",
                table: "ContractStateHistories",
                sql: "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Contracts_Status_Range",
                table: "Contracts",
                sql: "[Status] BETWEEN 0 AND 5");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_AspNetUsers_CancelledByUserId",
                table: "Disputes",
                column: "CancelledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerPenalties_AspNetUsers_RevokedByUserId",
                table: "LawyerPenalties",
                column: "RevokedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_AspNetUsers_CancelledByUserId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerPenalties_AspNetUsers_RevokedByUserId",
                table: "LawyerPenalties");

            migrationBuilder.DropIndex(
                name: "IX_LawyerPenalties_RevokedByUserId",
                table: "LawyerPenalties");

            migrationBuilder.DropIndex(
                name: "IX_Disputes_CancelledByUserId",
                table: "Disputes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Disputes_Status_Range",
                table: "Disputes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractStateHistories_NewStatus_Range",
                table: "ContractStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractStateHistories_PreviousStatus_Range",
                table: "ContractStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Contracts_Status_Range",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "LawyerPenalties");

            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "LawyerPenalties");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "LawyerPenalties");

            migrationBuilder.DropColumn(
                name: "RevokedByUserId",
                table: "LawyerPenalties");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "PreviousContractStatus",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "PreviousMilestoneStatus",
                table: "Disputes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Disputes_Status_Range",
                table: "Disputes",
                sql: "[Status] BETWEEN 0 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractStateHistories_NewStatus_Range",
                table: "ContractStateHistories",
                sql: "[NewStatus] BETWEEN 0 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractStateHistories_PreviousStatus_Range",
                table: "ContractStateHistories",
                sql: "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Contracts_Status_Range",
                table: "Contracts",
                sql: "[Status] BETWEEN 0 AND 4");
        }
    }
}
