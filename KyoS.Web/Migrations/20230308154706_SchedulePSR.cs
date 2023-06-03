using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class SchedulePSR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubScheduleId",
                table: "NotesP_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubScheduleId",
                table: "Notes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Activities_SubScheduleId",
                table: "NotesP_Activities",
                column: "SubScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Activities_SubScheduleId",
                table: "Notes_Activities",
                column: "SubScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Activities_SubSchedule_SubScheduleId",
                table: "Notes_Activities",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotesP_Activities_SubSchedule_SubScheduleId",
                table: "NotesP_Activities",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Activities_SubSchedule_SubScheduleId",
                table: "Notes_Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_NotesP_Activities_SubSchedule_SubScheduleId",
                table: "NotesP_Activities");

            migrationBuilder.DropIndex(
                name: "IX_NotesP_Activities_SubScheduleId",
                table: "NotesP_Activities");

            migrationBuilder.DropIndex(
                name: "IX_Notes_Activities_SubScheduleId",
                table: "Notes_Activities");

            migrationBuilder.DropColumn(
                name: "SubScheduleId",
                table: "NotesP_Activities");

            migrationBuilder.DropColumn(
                name: "SubScheduleId",
                table: "Notes_Activities");
        }
    }
}
