using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddTCMAppendixJImproves1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeAppendixJ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    IsAwaiting = table.Column<bool>(type: "bit", nullable: false),
                    HasBeen = table.Column<bool>(type: "bit", nullable: false),
                    HasHad = table.Column<bool>(type: "bit", nullable: false),
                    IsAt = table.Column<bool>(type: "bit", nullable: false),
                    IsExperiencing = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorSignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    TcmSupervisorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeAppendixJ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeAppendixJ_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TCMIntakeAppendixJ_TCMSupervisors_TcmSupervisorId",
                        column: x => x.TcmSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixJ_TcmClient_FK",
                table: "TCMIntakeAppendixJ",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixJ_TcmSupervisorId",
                table: "TCMIntakeAppendixJ",
                column: "TcmSupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeAppendixJ");
        }
    }
}
