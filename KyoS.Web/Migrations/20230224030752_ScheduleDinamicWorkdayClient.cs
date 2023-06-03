using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ScheduleDinamicWorkdayClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubSchedule_Clinics_ClinicId",
                table: "SubSchedule");

            migrationBuilder.DropIndex(
                name: "IX_SubSchedule_ClinicId",
                table: "SubSchedule");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "SubSchedule");

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Schedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_ScheduleId",
                table: "Workdays_Clients",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_ClinicId",
                table: "Schedule",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Clinics_ClinicId",
                table: "Schedule",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_Clients_Schedule_ScheduleId",
                table: "Workdays_Clients",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Clinics_ClinicId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_Clients_Schedule_ScheduleId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_Workdays_Clients_ScheduleId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_Schedule_ClinicId",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Schedule");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "SubSchedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSchedule_ClinicId",
                table: "SubSchedule",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubSchedule_Clinics_ClinicId",
                table: "SubSchedule",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
