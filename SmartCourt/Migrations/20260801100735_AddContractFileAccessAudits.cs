using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddContractFileAccessAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractFileAccessAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModeratorAccess = table.Column<bool>(type: "bit", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractFileAccessAudits", x => x.Id);
                    table.CheckConstraint("CK_ContractFileAccessAudits_Purpose_Range", "[Purpose] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_ContractFileAccessAudits_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractFileAccessAudits_ActorUserId",
                table: "ContractFileAccessAudits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractFileAccessAudits_File_Entity_Time",
                table: "ContractFileAccessAudits",
                columns: new[] { "StoredFileId", "RelatedEntityId", "AccessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractFileAccessAudits");
        }
    }
}
