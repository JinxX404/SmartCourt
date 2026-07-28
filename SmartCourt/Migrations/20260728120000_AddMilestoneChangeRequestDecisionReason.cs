using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations;

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
