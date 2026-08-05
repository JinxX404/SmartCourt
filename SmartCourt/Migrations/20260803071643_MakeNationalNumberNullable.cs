using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class MakeNationalNumberNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers",
                column: "NationalNumber",
                unique: true,
                filter: "[NationalNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers",
                column: "NationalNumber",
                unique: true);
        }
    }
}
