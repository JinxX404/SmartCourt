using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencySettlementKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_IdempotencyRecords_HoldSettlement",
                table: "IdempotencyRecords",
                columns: new[] { "ResourceType", "ResourceId" },
                unique: true,
                filter: "[ResourceType] = 'EscrowHoldSettlement'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_IdempotencyRecords_HoldSettlement",
                table: "IdempotencyRecords");
        }
    }
}
