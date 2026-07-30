using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proposals_ClientUserId_LawyerUserId_Status",
                table: "Proposals");

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                table: "Proposals",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Proposals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "Proposals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ClientUserId",
                table: "Proposals",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_LegalCaseId",
                table: "Proposals",
                column: "LegalCaseId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_LegalCaseId_LawyerUserId",
                table: "Proposals",
                columns: new[] { "LegalCaseId", "LawyerUserId" },
                unique: true,
                filter: "[Status] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proposals_ClientUserId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_LegalCaseId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_LegalCaseId_LawyerUserId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "Proposals");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ClientUserId_LawyerUserId_Status",
                table: "Proposals",
                columns: new[] { "ClientUserId", "LawyerUserId", "Status" });
        }
    }
}
