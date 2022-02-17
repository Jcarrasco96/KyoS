using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifySettingEntity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseMannager_Clinics_ClinicId",
                table: "CaseMannager");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseMannager",
                table: "CaseMannager");

            migrationBuilder.RenameTable(
                name: "CaseMannager",
                newName: "CaseManagers");

            migrationBuilder.RenameIndex(
                name: "IX_CaseMannager_Name",
                table: "CaseManagers",
                newName: "IX_CaseManagers_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CaseMannager_ClinicId",
                table: "CaseManagers",
                newName: "IX_CaseManagers_ClinicId");

            migrationBuilder.AddColumn<bool>(
                name: "MentalHealthClinic",
                table: "Settings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TCMClinic",
                table: "Settings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseManagers",
                table: "CaseManagers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseManagers_Clinics_ClinicId",
                table: "CaseManagers",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseManagers_Clinics_ClinicId",
                table: "CaseManagers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseManagers",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "MentalHealthClinic",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TCMClinic",
                table: "Settings");

            migrationBuilder.RenameTable(
                name: "CaseManagers",
                newName: "CaseMannager");

            migrationBuilder.RenameIndex(
                name: "IX_CaseManagers_Name",
                table: "CaseMannager",
                newName: "IX_CaseMannager_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CaseManagers_ClinicId",
                table: "CaseMannager",
                newName: "IX_CaseMannager_ClinicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseMannager",
                table: "CaseMannager",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseMannager_Clinics_ClinicId",
                table: "CaseMannager",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
