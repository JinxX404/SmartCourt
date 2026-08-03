using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class MakeNationalNumberOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecommendations_lawyerProfile_LawyerId",
                table: "CaseRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_lawyerProfile_AspNetUsers_UserId",
                table: "lawyerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerSpecializations_lawyerProfile_LawyerProfileUserId",
                table: "LawyerSpecializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LegalCases_Status_Range",
                table: "LegalCases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lawyerProfile",
                table: "lawyerProfile");

            migrationBuilder.RenameTable(
                name: "lawyerProfile",
                newName: "LawyerProfile");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NationalNumber",
                table: "AspNetUsers",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers",
                column: "NationalNumber",
                unique: true,
                filter: "[NationalNumber] IS NOT NULL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LawyerProfile",
                table: "LawyerProfile",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LegalCases_Status_Range",
                table: "LegalCases",
                sql: "[Status] BETWEEN 0 AND 6");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecommendations_LawyerProfile_LawyerId",
                table: "CaseRecommendations",
                column: "LawyerId",
                principalTable: "LawyerProfile",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerProfile_AspNetUsers_UserId",
                table: "LawyerProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerSpecializations_LawyerProfile_LawyerProfileUserId",
                table: "LawyerSpecializations",
                column: "LawyerProfileUserId",
                principalTable: "LawyerProfile",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecommendations_LawyerProfile_LawyerId",
                table: "CaseRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerProfile_AspNetUsers_UserId",
                table: "LawyerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerSpecializations_LawyerProfile_LawyerProfileUserId",
                table: "LawyerSpecializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LegalCases_Status_Range",
                table: "LegalCases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LawyerProfile",
                table: "LawyerProfile");

            migrationBuilder.RenameTable(
                name: "LawyerProfile",
                newName: "lawyerProfile");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NationalNumber",
                table: "AspNetUsers",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NationalNumber",
                table: "AspNetUsers",
                column: "NationalNumber",
                unique: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_lawyerProfile",
                table: "lawyerProfile",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LegalCases_Status_Range",
                table: "LegalCases",
                sql: "[Status] BETWEEN 0 AND 4");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecommendations_lawyerProfile_LawyerId",
                table: "CaseRecommendations",
                column: "LawyerId",
                principalTable: "lawyerProfile",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lawyerProfile_AspNetUsers_UserId",
                table: "lawyerProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerSpecializations_lawyerProfile_LawyerProfileUserId",
                table: "LawyerSpecializations",
                column: "LawyerProfileUserId",
                principalTable: "lawyerProfile",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
