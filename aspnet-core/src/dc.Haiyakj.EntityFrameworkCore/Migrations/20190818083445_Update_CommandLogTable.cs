using Microsoft.EntityFrameworkCore.Migrations;

namespace dc.Haiyakj.Migrations
{
    public partial class Update_CommandLogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResultMessage",
                table: "Command_Log",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultMessage",
                table: "Command_Log");
        }
    }
}
