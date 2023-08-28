using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CiteEntityV11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Schedule_ScheduleId",
                table: "Cites");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Cites",
                newName: "SubScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_ScheduleId",
                table: "Cites",
                newName: "IX_Cites_SubScheduleId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Cites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_SubSchedule_SubScheduleId",
                table: "Cites",
                column: "SubScheduleId",
                principalTable: "SubSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cites_SubSchedule_SubScheduleId",
                table: "Cites");

            migrationBuilder.RenameColumn(
                name: "SubScheduleId",
                table: "Cites",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_SubScheduleId",
                table: "Cites",
                newName: "IX_Cites_ScheduleId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Cites",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Schedule_ScheduleId",
                table: "Cites",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
