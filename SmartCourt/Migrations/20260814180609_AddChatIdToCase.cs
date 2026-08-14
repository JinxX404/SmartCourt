using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddChatIdToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatId",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ChatId",
                table: "Cases",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_ChatConversations_ChatId",
                table: "Cases",
                column: "ChatId",
                principalTable: "ChatConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_ChatConversations_ChatId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ChatId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Cases");
        }
    }
}
