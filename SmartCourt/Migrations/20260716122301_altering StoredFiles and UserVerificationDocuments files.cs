using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class alteringStoredFilesandUserVerificationDocumentsfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "StoredFiles",
                newName: "StoredFileName");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ExpirationDate",
                table: "UserVerificationDocuments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "StoredFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "StoredFiles");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                table: "StoredFiles",
                newName: "FileName");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "UserVerificationDocuments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
