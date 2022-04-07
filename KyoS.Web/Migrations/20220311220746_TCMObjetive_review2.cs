using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMObjetive_review2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TCMObjetives",
                newName: "Task");

            migrationBuilder.AddColumn<string>(
                name: "Long_Term",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Long_Term",
                table: "TCMObjetives");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "TCMObjetives",
                newName: "Description");
        }
    }
}
