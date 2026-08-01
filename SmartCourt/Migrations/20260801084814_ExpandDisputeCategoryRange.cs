using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDisputeCategoryRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Disputes_Category_Range",
                table: "Disputes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Disputes_Category_Range",
                table: "Disputes",
                sql: "[Category] BETWEEN 0 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Disputes_Category_Range",
                table: "Disputes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Disputes_Category_Range",
                table: "Disputes",
                sql: "[Category] BETWEEN 0 AND 3");
        }
    }
}
