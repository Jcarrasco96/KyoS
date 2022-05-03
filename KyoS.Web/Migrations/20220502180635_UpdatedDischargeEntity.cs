using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdatedDischargeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GivingToClient",
                table: "Discharge");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Discharge",
                newName: "DateSignatureSupervisor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateSignatureSupervisor",
                table: "Discharge",
                newName: "Time");

            migrationBuilder.AddColumn<bool>(
                name: "GivingToClient",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
