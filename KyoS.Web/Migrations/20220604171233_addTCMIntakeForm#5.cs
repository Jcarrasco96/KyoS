using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMIntakeForm5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryOfBirth",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmergencyContact",
                table: "TCMIntakeForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StatusOther",
                table: "TCMIntakeForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StatusOther_Explain",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusResident",
                table: "TCMIntakeForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StausCitizen",
                table: "TCMIntakeForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "YearEnterUsa",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryOfBirth",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "StatusOther",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "StatusOther_Explain",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "StatusResident",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "StausCitizen",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "YearEnterUsa",
                table: "TCMIntakeForms");
        }
    }
}
