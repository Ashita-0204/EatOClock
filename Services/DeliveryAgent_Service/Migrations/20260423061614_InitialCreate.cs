using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryAgent_Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryAgents",
                columns: table => new
                {
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    VehicleNumber = table.Column<string>(type: "text", nullable: false),
                    CurrentLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "double precision", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: false),
                    TotalDeliveries = table.Column<int>(type: "integer", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAgents", x => x.AgentId);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRecords",
                columns: table => new
                {
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    PickupAddress = table.Column<string>(type: "text", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: false),
                    EarningsForDelivery = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    RatingNote = table.Column<string>(type: "text", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRecords", x => x.DeliveryId);
                    table.ForeignKey(
                        name: "FK_DeliveryRecords_DeliveryAgents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "DeliveryAgents",
                        principalColumn: "AgentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_Phone",
                table: "DeliveryAgents",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_UserId",
                table: "DeliveryAgents",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRecords_AgentId",
                table: "DeliveryRecords",
                column: "AgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryRecords");

            migrationBuilder.DropTable(
                name: "DeliveryAgents");
        }
    }
}
