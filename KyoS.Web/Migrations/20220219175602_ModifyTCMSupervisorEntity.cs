using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyTCMSupervisorEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMsupervisors_Clinics_ClinicId",
                table: "TCMsupervisors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMsupervisors",
                table: "TCMsupervisors");

            migrationBuilder.DropColumn(
                name: "TCMActive",
                table: "TCMsupervisors");

            migrationBuilder.RenameTable(
                name: "TCMsupervisors",
                newName: "TCMSupervisors");

            migrationBuilder.RenameIndex(
                name: "IX_TCMsupervisors_Name",
                table: "TCMSupervisors",
                newName: "IX_TCMSupervisors_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TCMsupervisors_ClinicId",
                table: "TCMSupervisors",
                newName: "IX_TCMSupervisors_ClinicId");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TCMSupervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMSupervisors",
                table: "TCMSupervisors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMSupervisors_Clinics_ClinicId",
                table: "TCMSupervisors",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMSupervisors_Clinics_ClinicId",
                table: "TCMSupervisors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMSupervisors",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TCMSupervisors");

            migrationBuilder.RenameTable(
                name: "TCMSupervisors",
                newName: "TCMsupervisors");

            migrationBuilder.RenameIndex(
                name: "IX_TCMSupervisors_Name",
                table: "TCMsupervisors",
                newName: "IX_TCMsupervisors_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TCMSupervisors_ClinicId",
                table: "TCMsupervisors",
                newName: "IX_TCMsupervisors_ClinicId");

            migrationBuilder.AddColumn<bool>(
                name: "TCMActive",
                table: "TCMsupervisors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMsupervisors",
                table: "TCMsupervisors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMsupervisors_Clinics_ClinicId",
                table: "TCMsupervisors",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
