using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddTCMInterventionLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeInterventionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeInterventionLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeInterventionLog_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMIntakeIntervention",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmInterventionLogId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeIntervention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeIntervention_TCMIntakeInterventionLog_TcmInterventionLogId",
                        column: x => x.TcmInterventionLogId,
                        principalTable: "TCMIntakeInterventionLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeIntervention_TcmInterventionLogId",
                table: "TCMIntakeIntervention",
                column: "TcmInterventionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeInterventionLog_TcmClient_FK",
                table: "TCMIntakeInterventionLog",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeIntervention");

            migrationBuilder.DropTable(
                name: "TCMIntakeInterventionLog");
        }
    }
}
