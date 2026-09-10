using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalTickets = table.Column<int>(type: "integer", nullable: false),
                    Sold = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Confirmed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId",
                table: "Orders",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PurchasedAt",
                table: "Orders",
                column: "PurchasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
