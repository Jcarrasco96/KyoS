using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Create_adendum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdendumEntityId",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Adendum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MtpId = table.Column<int>(type: "int", nullable: true),
                    Dateidentified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProblemStatement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Frecuency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adendum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adendum_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Adendum_MTPs_MtpId",
                        column: x => x.MtpId,
                        principalTable: "MTPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Adendum_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_AdendumEntityId",
                table: "Goals",
                column: "AdendumEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Adendum_FacilitatorId",
                table: "Adendum",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Adendum_MtpId",
                table: "Adendum",
                column: "MtpId");

            migrationBuilder.CreateIndex(
                name: "IX_Adendum_SupervisorId",
                table: "Adendum",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Adendum_AdendumEntityId",
                table: "Goals",
                column: "AdendumEntityId",
                principalTable: "Adendum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Adendum_AdendumEntityId",
                table: "Goals");

            migrationBuilder.DropTable(
                name: "Adendum");

            migrationBuilder.DropIndex(
                name: "IX_Goals_AdendumEntityId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "AdendumEntityId",
                table: "Goals");
        }
    }
}
