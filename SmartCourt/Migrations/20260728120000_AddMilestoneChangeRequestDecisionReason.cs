using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCourt.Persistence;

#nullable disable

namespace SmartCourt.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260728120000_AddMilestoneChangeRequestDecisionReason")]
public partial class AddMilestoneChangeRequestDecisionReason : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DecisionReason",
            table: "MilestoneChangeRequests",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DecisionReason",
            table: "MilestoneChangeRequests");
    }
}
