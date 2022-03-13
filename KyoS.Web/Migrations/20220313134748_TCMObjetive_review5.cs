using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMObjetive_review5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Long_Term",
                table: "TCMObjetives");

            migrationBuilder.AddColumn<string>(
                name: "LongTerm",
                table: "TCMDomains",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongTerm",
                table: "TCMDomains");

            migrationBuilder.AddColumn<string>(
                name: "Long_Term",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
