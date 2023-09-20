using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class PrimaryDoctorPsychiatrists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Psychiatrists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaxNumber",
                table: "Psychiatrists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Psychiatrists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Psychiatrists",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Psychiatrists");

            migrationBuilder.DropColumn(
                name: "FaxNumber",
                table: "Psychiatrists");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Psychiatrists");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Psychiatrists");
        }
    }
}
