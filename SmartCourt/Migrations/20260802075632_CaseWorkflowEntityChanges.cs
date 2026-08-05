using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class CaseWorkflowEntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LawyerProfile_AspNetUsers_UserId",
                table: "LawyerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerProfile_LegalSpecializations_SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LawyerProfile",
                table: "LawyerProfile");

            migrationBuilder.DropIndex(
                name: "IX_LawyerProfile_SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "LawyerProfile");

            migrationBuilder.DropColumn(
                name: "CaseComplexity",
                table: "CaseReviewReports");

            migrationBuilder.RenameTable(
                name: "LawyerProfile",
                newName: "lawyerProfile");

            migrationBuilder.RenameColumn(
                name: "Government",
                table: "AspNetUsers",
                newName: "Governorate");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "lawyerProfile",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageResponseTimeHours",
                table: "lawyerProfile",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Cases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "Cases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Specialization",
                table: "CaseProfiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "RequiredLawyerLevelId",
                table: "CaseProfiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "Complexity",
                table: "CaseProfiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lawyerProfile",
                table: "lawyerProfile",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "CaseRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    LocationScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    ExperienceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    RatingScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    ResponseTimeScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseRecommendations_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseRecommendations_lawyerProfile_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "lawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawyerSpecializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LawyerProfileUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Specialization = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CasesHandled = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerSpecializations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerSpecializations_lawyerProfile_LawyerProfileUserId",
                        column: x => x.LawyerProfileUserId,
                        principalTable: "lawyerProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseRecommendation_CaseId_Rank",
                table: "CaseRecommendations",
                columns: new[] { "CaseId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseRecommendations_LawyerId",
                table: "CaseRecommendations",
                column: "LawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerSpecialization_LawyerId_Specialization",
                table: "LawyerSpecializations",
                columns: new[] { "LawyerProfileUserId", "Specialization" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_lawyerProfile_AspNetUsers_UserId",
                table: "lawyerProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lawyerProfile_AspNetUsers_UserId",
                table: "lawyerProfile");

            migrationBuilder.DropTable(
                name: "CaseRecommendations");

            migrationBuilder.DropTable(
                name: "LawyerSpecializations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lawyerProfile",
                table: "lawyerProfile");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "lawyerProfile");

            migrationBuilder.DropColumn(
                name: "AverageResponseTimeHours",
                table: "lawyerProfile");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "Cases");

            migrationBuilder.RenameTable(
                name: "lawyerProfile",
                newName: "LawyerProfile");

            migrationBuilder.RenameColumn(
                name: "Governorate",
                table: "AspNetUsers",
                newName: "Government");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "LawyerProfile",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecializationId",
                table: "LawyerProfile",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "LawyerProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "CaseComplexity",
                table: "CaseReviewReports",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<byte>(
                name: "Specialization",
                table: "CaseProfiles",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RequiredLawyerLevelId",
                table: "CaseProfiles",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Complexity",
                table: "CaseProfiles",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LawyerProfile",
                table: "LawyerProfile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerProfile_SpecializationId",
                table: "LawyerProfile",
                column: "SpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerProfile_AspNetUsers_UserId",
                table: "LawyerProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerProfile_LegalSpecializations_SpecializationId",
                table: "LawyerProfile",
                column: "SpecializationId",
                principalTable: "LegalSpecializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
