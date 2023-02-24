using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ScheduleDinamicGroupNotesActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubScheduleId",
                table: "GroupNotes2_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubScheduleId",
                table: "GroupNotes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Activities_SubScheduleId",
                table: "GroupNotes2_Activities",
                column: "SubScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_SubScheduleId",
                table: "GroupNotes_Activities",
                column: "SubScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes_Activities_SubSchedule_SubScheduleId",
                table: "GroupNotes_Activities",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes2_Activities_SubSchedule_SubScheduleId",
                table: "GroupNotes2_Activities",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes_Activities_SubSchedule_SubScheduleId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes2_Activities_SubSchedule_SubScheduleId",
                table: "GroupNotes2_Activities");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes2_Activities_SubScheduleId",
                table: "GroupNotes2_Activities");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes_Activities_SubScheduleId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropColumn(
                name: "SubScheduleId",
                table: "GroupNotes2_Activities");

            migrationBuilder.DropColumn(
                name: "SubScheduleId",
                table: "GroupNotes_Activities");
        }
    }
}
