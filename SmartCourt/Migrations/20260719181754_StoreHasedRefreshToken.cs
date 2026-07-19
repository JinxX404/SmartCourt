using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class StoreHasedRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshTokens",
                newName: "HashedToken");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Government",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Government",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "HashedToken",
                table: "RefreshTokens",
                newName: "Token");
        }
    }
}
