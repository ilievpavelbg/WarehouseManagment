using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddWarehouseSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarehouseSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultMaterialWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DefaultWipWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DefaultFinishedGoodsWarehouseId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseSettings_Warehouses_DefaultFinishedGoodsWarehouseId",
                        column: x => x.DefaultFinishedGoodsWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseSettings_Warehouses_DefaultMaterialWarehouseId",
                        column: x => x.DefaultMaterialWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseSettings_Warehouses_DefaultWipWarehouseId",
                        column: x => x.DefaultWipWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseSettings_DefaultFinishedGoodsWarehouseId",
                table: "WarehouseSettings",
                column: "DefaultFinishedGoodsWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseSettings_DefaultMaterialWarehouseId",
                table: "WarehouseSettings",
                column: "DefaultMaterialWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseSettings_DefaultWipWarehouseId",
                table: "WarehouseSettings",
                column: "DefaultWipWarehouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseSettings");
        }
    }
}
