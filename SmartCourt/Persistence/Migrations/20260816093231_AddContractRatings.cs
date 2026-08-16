using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalRatingCount",
                table: "LawyerProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRatingSum",
                table: "LawyerProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContractRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaterRole = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRatings", x => x.Id);
                    table.CheckConstraint("CK_ContractRatings_RaterRole_Range", "[RaterRole] IN (0, 1)");
                    table.CheckConstraint("CK_ContractRatings_Stars_Range", "[Stars] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_ContractRatings_AspNetUsers_RatedUserId",
                        column: x => x.RatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractRatings_AspNetUsers_RaterUserId",
                        column: x => x.RaterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractRatings_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractRatings_RatedUser_ClientRatings",
                table: "ContractRatings",
                column: "RatedUserId",
                filter: "[RaterRole] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRatings_RaterUserId",
                table: "ContractRatings",
                column: "RaterUserId");

            migrationBuilder.CreateIndex(
                name: "UX_ContractRatings_Contract_RaterRole",
                table: "ContractRatings",
                columns: new[] { "ContractId", "RaterRole" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractRatings");

            migrationBuilder.DropColumn(
                name: "TotalRatingCount",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "TotalRatingSum",
                table: "LawyerProfile");
        }
    }
}
