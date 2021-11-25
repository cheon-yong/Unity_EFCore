using Microsoft.EntityFrameworkCore.Migrations;

namespace MMO_EFCore.Migrations
{
    public partial class ItemGrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemGrade",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemGrade",
                table: "Items");
        }
    }
}
