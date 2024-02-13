using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class NutritionalScreenANDpersonalWellbeingInTCMintake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeNutritionalScreen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    ClientHasIllnes = table.Column<bool>(type: "bit", nullable: false),
                    ClientHasIllnes_Value = table.Column<int>(type: "int", nullable: false),
                    ClientHasHistory = table.Column<bool>(type: "bit", nullable: false),
                    ClientHasHistory_Value = table.Column<int>(type: "int", nullable: false),
                    ClientEatsFewer = table.Column<bool>(type: "bit", nullable: false),
                    ClientEatsFewer_Value = table.Column<int>(type: "int", nullable: false),
                    ClientEatsFew = table.Column<bool>(type: "bit", nullable: false),
                    ClientEatsFew_Value = table.Column<int>(type: "int", nullable: false),
                    ClientHasTooth = table.Column<bool>(type: "bit", nullable: false),
                    ClientHasTooth_Value = table.Column<int>(type: "int", nullable: false),
                    ClientEatsAlone = table.Column<bool>(type: "bit", nullable: false),
                    ClientEatsAlone_Value = table.Column<int>(type: "int", nullable: false),
                    ClientTakes = table.Column<bool>(type: "bit", nullable: false),
                    ClientTakes_Value = table.Column<int>(type: "int", nullable: false),
                    WithoutWanting = table.Column<bool>(type: "bit", nullable: false),
                    WithoutWanting_Value = table.Column<int>(type: "int", nullable: false),
                    ClientAlwaysHungry = table.Column<bool>(type: "bit", nullable: false),
                    ClientAlwaysHungry_Value = table.Column<int>(type: "int", nullable: false),
                    ClientAlwaysThirsty = table.Column<bool>(type: "bit", nullable: false),
                    ClientAlwaysThirsty_Value = table.Column<int>(type: "int", nullable: false),
                    ClientVomits = table.Column<bool>(type: "bit", nullable: false),
                    ClientVomits_Value = table.Column<int>(type: "int", nullable: false),
                    ClientDiarrhea = table.Column<bool>(type: "bit", nullable: false),
                    ClientDiarrhea_Value = table.Column<int>(type: "int", nullable: false),
                    ClientBinges = table.Column<bool>(type: "bit", nullable: false),
                    ClientBinges_Value = table.Column<int>(type: "int", nullable: false),
                    ClientAppetiteGood = table.Column<bool>(type: "bit", nullable: false),
                    ClientAppetiteGood_Value = table.Column<int>(type: "int", nullable: false),
                    ClientAppetiteFair = table.Column<bool>(type: "bit", nullable: false),
                    ClientAppetiteFair_Value = table.Column<int>(type: "int", nullable: false),
                    ClientAppetitepoor = table.Column<bool>(type: "bit", nullable: false),
                    ClientAppetitepoor_Value = table.Column<int>(type: "int", nullable: false),
                    ClientFoodAllergies = table.Column<bool>(type: "bit", nullable: false),
                    ClientFoodAllergies_Value = table.Column<int>(type: "int", nullable: false),
                    ReferredTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfReferral = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeNutritionalScreen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeNutritionalScreen_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMIntakePersonalWellbeing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    CurrentPainScore = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Life = table.Column<int>(type: "int", nullable: false),
                    Relationships = table.Column<int>(type: "int", nullable: false),
                    Feel = table.Column<int>(type: "int", nullable: false),
                    Community = table.Column<int>(type: "int", nullable: false),
                    Security = table.Column<int>(type: "int", nullable: false),
                    Religion = table.Column<int>(type: "int", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakePersonalWellbeing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakePersonalWellbeing_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeNutritionalScreen_TcmClient_FK",
                table: "TCMIntakeNutritionalScreen",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakePersonalWellbeing_TcmClient_FK",
                table: "TCMIntakePersonalWellbeing",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeNutritionalScreen");

            migrationBuilder.DropTable(
                name: "TCMIntakePersonalWellbeing");
        }
    }
}
