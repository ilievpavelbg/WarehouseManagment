using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductionWorkReporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProductionOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProductionOrderOperations",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "ProductionWorkEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderOperationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserNameSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReportedCompletedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReportedRejectedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkStartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkEndedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionWorkEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionWorkEntries_ProductionOrderOperations_ProductionOrderOperationId",
                        column: x => x.ProductionOrderOperationId,
                        principalTable: "ProductionOrderOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkEntries_CreatedOn",
                table: "ProductionWorkEntries",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkEntries_ProductionOrderOperationId",
                table: "ProductionWorkEntries",
                column: "ProductionOrderOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkEntries_ProductionOrderOperationId_CreatedOn",
                table: "ProductionWorkEntries",
                columns: new[] { "ProductionOrderOperationId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkEntries_UserId",
                table: "ProductionWorkEntries",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionWorkEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductionOrderOperations");
        }
    }
}
