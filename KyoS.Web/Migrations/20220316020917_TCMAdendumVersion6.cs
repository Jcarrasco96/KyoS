using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAdendumVersion6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Longterm",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "NeedsIdentified",
                table: "TCMAdendums");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Longterm",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NeedsIdentified",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
