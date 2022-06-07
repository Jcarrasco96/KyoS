using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMOrientationCheckList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeOrientationCheckList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TourFacility = table.Column<bool>(type: "bit", nullable: false),
                    Rights = table.Column<bool>(type: "bit", nullable: false),
                    PoliceGrievancce = table.Column<bool>(type: "bit", nullable: false),
                    Insent = table.Column<bool>(type: "bit", nullable: false),
                    Services = table.Column<bool>(type: "bit", nullable: false),
                    Access = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<bool>(type: "bit", nullable: false),
                    Confidentiality = table.Column<bool>(type: "bit", nullable: false),
                    Methods = table.Column<bool>(type: "bit", nullable: false),
                    Explanation = table.Column<bool>(type: "bit", nullable: false),
                    Fire = table.Column<bool>(type: "bit", nullable: false),
                    PoliceTobacco = table.Column<bool>(type: "bit", nullable: false),
                    PoliceIllicit = table.Column<bool>(type: "bit", nullable: false),
                    PoliceWeapons = table.Column<bool>(type: "bit", nullable: false),
                    Identification = table.Column<bool>(type: "bit", nullable: false),
                    Program = table.Column<bool>(type: "bit", nullable: false),
                    Purpose = table.Column<bool>(type: "bit", nullable: false),
                    IndividualPlan = table.Column<bool>(type: "bit", nullable: false),
                    Discharge = table.Column<bool>(type: "bit", nullable: false),
                    AgencyPolice = table.Column<bool>(type: "bit", nullable: false),
                    AgencyExpectation = table.Column<bool>(type: "bit", nullable: false),
                    Education = table.Column<bool>(type: "bit", nullable: false),
                    TheAbove = table.Column<bool>(type: "bit", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeOrientationCheckList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeOrientationCheckList_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeOrientationCheckList_TcmClient_FK",
                table: "TCMIntakeOrientationCheckList",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeOrientationCheckList");
        }
    }
}
