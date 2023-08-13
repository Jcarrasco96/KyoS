using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class DATOSfarsCASEMANAGER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaterEducation",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RaterFMHCertification",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaterEducation",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "RaterFMHCertification",
                table: "CaseManagers");
        }
    }
}
