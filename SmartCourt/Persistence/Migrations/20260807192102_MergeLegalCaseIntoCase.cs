using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MergeLegalCaseIntoCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_LegalCases_LegalCaseId",
                table: "ChatConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_LegalCases_LegalCaseId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_LegalCases_LegalCaseId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "LegalCases");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_Cases_LegalCaseId",
                table: "ChatConversations",
                column: "LegalCaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Cases_LegalCaseId",
                table: "Contracts",
                column: "LegalCaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Cases_LegalCaseId",
                table: "Proposals",
                column: "LegalCaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_Cases_LegalCaseId",
                table: "ChatConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Cases_LegalCaseId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Cases_LegalCaseId",
                table: "Proposals");

            migrationBuilder.CreateTable(
                name: "LegalCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    FinalSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalCases", x => x.Id);
                    table.CheckConstraint("CK_LegalCases_Status_Range", "[Status] BETWEEN 0 AND 6");
                    table.ForeignKey(
                        name: "FK_LegalCases_AspNetUsers_ClientUserId",
                        column: x => x.ClientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalCases_ClientUserId_Status",
                table: "LegalCases",
                columns: new[] { "ClientUserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_LegalCases_LegalCaseId",
                table: "ChatConversations",
                column: "LegalCaseId",
                principalTable: "LegalCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_LegalCases_LegalCaseId",
                table: "Contracts",
                column: "LegalCaseId",
                principalTable: "LegalCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_LegalCases_LegalCaseId",
                table: "Proposals",
                column: "LegalCaseId",
                principalTable: "LegalCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
