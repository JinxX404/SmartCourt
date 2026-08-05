using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCriticalFinancialUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT [MilestoneId]
                    FROM [Disputes]
                    WHERE [Status] IN (0, 1, 2)
                    GROUP BY [MilestoneId]
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, N'توجد نزاعات نشطة مكررة على المرحلة نفسها ويجب تسويتها قبل تطبيق قيد التفرد.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "UX_Disputes_OpenPerMilestone",
                table: "Disputes");

            migrationBuilder.CreateIndex(
                name: "UX_Disputes_OpenPerMilestone",
                table: "Disputes",
                column: "MilestoneId",
                unique: true,
                filter: "[Status] IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Disputes_OpenPerMilestone",
                table: "Disputes");

            migrationBuilder.CreateIndex(
                name: "UX_Disputes_OpenPerMilestone",
                table: "Disputes",
                columns: new[] { "MilestoneId", "Status" },
                unique: true,
                filter: "[Status] IN (0, 1, 2)");
        }
    }
}
