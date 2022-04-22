using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentForRelease1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizeStaff",
                table: "IntakeConsentForRelease");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AuthorizeStaff",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
