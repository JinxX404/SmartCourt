using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddClientPaymentCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientPaymentCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    IsLive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPaymentCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPaymentCustomers_AspNetUsers_ClientUserId",
                        column: x => x.ClientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_ClientPaymentCustomers_Client_Provider",
                table: "ClientPaymentCustomers",
                columns: new[] { "ClientUserId", "ProviderCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ClientPaymentCustomers_ProviderCustomer",
                table: "ClientPaymentCustomers",
                columns: new[] { "ProviderCode", "ProviderCustomerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientPaymentCustomers");
        }
    }
}
