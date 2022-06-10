using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddCoordinationCareInOpenBinderSectio3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeCoordinationCare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false),
                    InformationToRelease = table.Column<bool>(type: "bit", nullable: false),
                    InformationTorequested = table.Column<bool>(type: "bit", nullable: false),
                    PCP = table.Column<bool>(type: "bit", nullable: false),
                    Specialist = table.Column<bool>(type: "bit", nullable: false),
                    SpecialistText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InformationVerbal = table.Column<bool>(type: "bit", nullable: false),
                    InformationWrited = table.Column<bool>(type: "bit", nullable: false),
                    InformationFascimile = table.Column<bool>(type: "bit", nullable: false),
                    InformationElectronic = table.Column<bool>(type: "bit", nullable: false),
                    InformationAllBefore = table.Column<bool>(type: "bit", nullable: false),
                    InformationNonKnown = table.Column<bool>(type: "bit", nullable: false),
                    IAuthorize = table.Column<bool>(type: "bit", nullable: false),
                    IRefuse = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeCoordinationCare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeCoordinationCare_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeCoordinationCare_TcmClient_FK",
                table: "TCMIntakeCoordinationCare",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeCoordinationCare");
        }
    }
}
