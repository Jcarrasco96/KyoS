using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_ReviewWith_FloridaSHS4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeScreenings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeScreenings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
