using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddMilestoneTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Milestones_Status_Range",
                table: "Milestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilestoneStateHistories_NewStatus_Range",
                table: "MilestoneStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilestoneStateHistories_PreviousStatus_Range",
                table: "MilestoneStateHistories");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Milestones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_Type_Status_FundedAt",
                table: "Milestones",
                columns: new[] { "Type", "Status", "FundedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Milestones_ExpenseFields",
                table: "Milestones",
                sql: "[Type] <> 1 OR ([Deliverables] IS NULL AND [DurationDays] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Milestones_Status_Range",
                table: "Milestones",
                sql: "[Status] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Milestones_Type_Range",
                table: "Milestones",
                sql: "[Type] BETWEEN 0 AND 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilestoneStateHistories_NewStatus_Range",
                table: "MilestoneStateHistories",
                sql: "[NewStatus] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilestoneStateHistories_PreviousStatus_Range",
                table: "MilestoneStateHistories",
                sql: "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Milestones_Type_Status_FundedAt",
                table: "Milestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Milestones_ExpenseFields",
                table: "Milestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Milestones_Status_Range",
                table: "Milestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Milestones_Type_Range",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Milestones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilestoneStateHistories_NewStatus_Range",
                table: "MilestoneStateHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MilestoneStateHistories_PreviousStatus_Range",
                table: "MilestoneStateHistories");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Milestones_Status_Range",
                table: "Milestones",
                sql: "[Status] BETWEEN 0 AND 9");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilestoneStateHistories_NewStatus_Range",
                table: "MilestoneStateHistories",
                sql: "[NewStatus] BETWEEN 0 AND 9");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MilestoneStateHistories_PreviousStatus_Range",
                table: "MilestoneStateHistories",
                sql: "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 9");
        }
    }
}
