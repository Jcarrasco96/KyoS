using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAdendumVersion7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateIdentified",
                table: "TCMAdendums",
                newName: "DateAdendum");

            migrationBuilder.AddColumn<int>(
                name: "Approved",
                table: "TCMAdendums",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "TCMAdendums");

            migrationBuilder.RenameColumn(
                name: "DateAdendum",
                table: "TCMAdendums",
                newName: "DateIdentified");
        }
    }
}
