using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMSupervisionTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMSupervisionTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMSupervisorId = table.Column<int>(type: "int", nullable: true),
                    CaseManagerId = table.Column<int>(type: "int", nullable: true),
                    DateSupervision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMSupervisionTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMSupervisionTimes_CaseManagers_CaseManagerId",
                        column: x => x.CaseManagerId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMSupervisionTimes_TCMSupervisors_TCMSupervisorId",
                        column: x => x.TCMSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMSupervisionTimes_CaseManagerId",
                table: "TCMSupervisionTimes",
                column: "CaseManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMSupervisionTimes_TCMSupervisorId",
                table: "TCMSupervisionTimes",
                column: "TCMSupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMSupervisionTimes");
        }
    }
}
