using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ScheduleDinamicGroupTherapy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Service = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSchedule_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubSchedule_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ScheduleId",
                table: "Groups",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSchedule_ClinicId",
                table: "SubSchedule",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSchedule_ScheduleId",
                table: "SubSchedule",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Schedule_ScheduleId",
                table: "Groups",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Schedule_ScheduleId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "SubSchedule");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ScheduleId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Groups");
        }
    }
}
