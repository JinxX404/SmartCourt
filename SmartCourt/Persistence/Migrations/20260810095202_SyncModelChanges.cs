using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proposals_LegalCaseId",
                table: "Proposals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_LegalCaseId",
                table: "Contracts");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Proposals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByUserId",
                table: "Proposals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Proposals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "LawyerId",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ClosedByUserId",
                table: "Proposals",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Status_ExpiresAt",
                table: "Proposals",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals",
                sql: "[Status] BETWEEN 0 AND 6");

            migrationBuilder.CreateIndex(
                name: "UX_Contracts_ActiveCase",
                table: "Contracts",
                column: "LegalCaseId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LawyerId",
                table: "Cases",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_LawyerProfile_LawyerId",
                table: "Cases",
                column: "LawyerId",
                principalTable: "LawyerProfile",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_AspNetUsers_ClosedByUserId",
                table: "Proposals",
                column: "ClosedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_LawyerProfile_LawyerId",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_AspNetUsers_ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_Status_ExpiresAt",
                table: "Proposals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "UX_Contracts_ActiveCase",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LawyerId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "Cases");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_LegalCaseId",
                table: "Proposals",
                column: "LegalCaseId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals",
                sql: "[Status] BETWEEN 0 AND 2");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_LegalCaseId",
                table: "Contracts",
                column: "LegalCaseId");
        }
    }
}
