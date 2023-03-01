using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ScheduleInIndividualNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubScheduleId",
                table: "IndividualNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualNotes_SubScheduleId",
                table: "IndividualNotes",
                column: "SubScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualNotes_SubSchedule_SubScheduleId",
                table: "IndividualNotes",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualNotes_SubSchedule_SubScheduleId",
                table: "IndividualNotes");

            migrationBuilder.DropIndex(
                name: "IX_IndividualNotes_SubScheduleId",
                table: "IndividualNotes");

            migrationBuilder.DropColumn(
                name: "SubScheduleId",
                table: "IndividualNotes");
        }
    }
}
