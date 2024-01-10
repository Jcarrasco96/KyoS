using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class separated_address : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZidCode",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Referreds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Referreds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZidCode",
                table: "Referreds",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "State",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "ZidCode",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Referreds");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Referreds");

            migrationBuilder.DropColumn(
                name: "ZidCode",
                table: "Referreds");
        }
    }
}
