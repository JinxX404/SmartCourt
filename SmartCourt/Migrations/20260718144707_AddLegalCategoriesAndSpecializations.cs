using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalCategoriesAndSpecializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "LawyerProfile");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "LawyerProfile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecializationId",
                table: "LawyerProfile",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LegalCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalSpecializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalSpecializations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalSpecializations_LegalCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LegalCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerProfile_SpecializationId",
                table: "LawyerProfile",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalSpecializations_CategoryId",
                table: "LegalSpecializations",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerProfile_LegalSpecializations_SpecializationId",
                table: "LawyerProfile",
                column: "SpecializationId",
                principalTable: "LegalSpecializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LawyerProfile_LegalSpecializations_SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropTable(
                name: "LegalSpecializations");

            migrationBuilder.DropTable(
                name: "LegalCategories");

            migrationBuilder.DropIndex(
                name: "IX_LawyerProfile_SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "LawyerProfile",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
