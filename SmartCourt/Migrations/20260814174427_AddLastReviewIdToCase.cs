using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReviewIdToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastReviewId",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LastReviewId",
                table: "Cases",
                column: "LastReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_CaseReviewReports_LastReviewId",
                table: "Cases",
                column: "LastReviewId",
                principalTable: "CaseReviewReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_CaseReviewReports_LastReviewId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LastReviewId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LastReviewId",
                table: "Cases");
        }
    }
}
