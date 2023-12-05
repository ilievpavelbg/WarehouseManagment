using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class NotesAddedToSalesAndCourierTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Sales",
                type: "nvarchar(1000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Couriers",
                type: "nvarchar(1000)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Couriers");
        }
    }
}
