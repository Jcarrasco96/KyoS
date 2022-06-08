using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMIntakeForm2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReligionOrEspiritual",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtherLanguage_Read",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OtherLanguage_Speak",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OtherLanguage_Understand",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "ReligionOrEspiritual",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "OtherLanguage_Read",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OtherLanguage_Speak",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OtherLanguage_Understand",
                table: "Clients");
        }
    }
}
