using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addFieldProviderNumberInCaseManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "CaseManagers",
                newName: "ProviderNumber");

            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "CaseManagers");

            migrationBuilder.RenameColumn(
                name: "ProviderNumber",
                table: "CaseManagers",
                newName: "Codigo");
        }
    }
}
