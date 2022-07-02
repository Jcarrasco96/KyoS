using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class includeFieldInTCMAddendum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LongTerm",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NeedsIdentified",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongTerm",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "NeedsIdentified",
                table: "TCMAdendums");
        }
    }
}
