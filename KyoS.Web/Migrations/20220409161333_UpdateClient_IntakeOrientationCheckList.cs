using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeOrientationCheckList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeOrientationCheckList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TourFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rights = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoliceGrievancce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Insent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Services = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Access = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confidentiality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Methods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoliceTobacco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoliceIllicit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoliceWeapons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndividualPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgencyPolice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgencyExpectation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheAbove = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeOrientationCheckList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeOrientationCheckList_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeOrientationCheckList_Client_FK",
                table: "IntakeOrientationCheckList",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeOrientationCheckList");
        }
    }
}
