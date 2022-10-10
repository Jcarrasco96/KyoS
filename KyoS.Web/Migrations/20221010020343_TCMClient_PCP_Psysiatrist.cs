using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMClient_PCP_Psysiatrist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PCP_Address",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PCP_CityStateZip",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PCP_Name",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PCP_Phone",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PCP_Place",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Psychiatrist_Address",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Psychiatrist_CityStateZip",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Psychiatrist_Name",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Psychiatrist_Phone",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PCP_Address",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "PCP_CityStateZip",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "PCP_Name",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "PCP_Phone",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "PCP_Place",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "Psychiatrist_Address",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "Psychiatrist_CityStateZip",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "Psychiatrist_Name",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "Psychiatrist_Phone",
                table: "TCMIntakeForms");
        }
    }
}
