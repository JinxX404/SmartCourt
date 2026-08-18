using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUsageDateToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DailyUsages",
                table: "DailyUsages");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UsageDate",
                table: "DailyUsages",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DailyUsages",
                table: "DailyUsages",
                columns: new[] { "ClientId", "UsageDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DailyUsages",
                table: "DailyUsages");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "UsageDate",
                table: "DailyUsages",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DailyUsages",
                table: "DailyUsages",
                columns: new[] { "ClientId", "UsageDate" });
        }
    }
}
