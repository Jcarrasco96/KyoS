using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeScreening : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeScreeningEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    InformationGatheredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureClient = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureWitness = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientIsStatus = table.Column<int>(type: "int", nullable: false),
                    BehaviorIsStatus = table.Column<int>(type: "int", nullable: false),
                    SpeechIsStatus = table.Column<int>(type: "int", nullable: false),
                    DoesClientKnowHisName = table.Column<bool>(type: "bit", nullable: false),
                    DoesClientKnowTodayDate = table.Column<bool>(type: "bit", nullable: false),
                    DoesClientKnowWhereIs = table.Column<bool>(type: "bit", nullable: false),
                    DoesClientKnowTimeOfDay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeScreeningEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeScreeningEntity_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeScreeningEntity_Client_FK",
                table: "IntakeScreeningEntity",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeScreeningEntity");
        }
    }
}
