using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class improvesTCMServiceActivity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "TCMServiceActivity",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "TCMServiceActivity",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
