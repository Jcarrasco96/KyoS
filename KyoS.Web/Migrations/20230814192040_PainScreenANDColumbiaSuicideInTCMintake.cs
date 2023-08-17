using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class PainScreenANDColumbiaSuicideInTCMintake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeColumbiaSuicide",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    HaveYouWishedPastMonth = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouWishedPastMonth_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouWishedLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouWishedLifeTime_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouActuallyPastMonth = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouActuallyPastMonth_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouActuallyLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouActuallyLifeTime_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouBeenPastMonth = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouBeenPastMonth_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouBeenLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouBeenLifeTime_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouHadPastMonth = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouHadPastMonth_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouHadLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouHadLifeTime_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouStartedPastMonth = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouStartedPastMonth_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouStartedLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouStartedLifeTime_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouEver = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEver_Value = table.Column<int>(type: "int", nullable: false),
                    HaveYouEverIfYes = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverIfYes_Value = table.Column<int>(type: "int", nullable: false),
                    CurrentPainScore = table.Column<int>(type: "int", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeColumbiaSuicide", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeColumbiaSuicide_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMIntakePainScreen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    DoYouSuffer = table.Column<bool>(type: "bit", nullable: false),
                    DidYouUse = table.Column<bool>(type: "bit", nullable: false),
                    WereYourDrugs = table.Column<bool>(type: "bit", nullable: false),
                    DoYouFell = table.Column<bool>(type: "bit", nullable: false),
                    DoYouBelieve = table.Column<bool>(type: "bit", nullable: false),
                    WhereIs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatCauses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoesYourPainEffect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlwayasThere = table.Column<bool>(type: "bit", nullable: false),
                    ComesAndGoes = table.Column<bool>(type: "bit", nullable: false),
                    CurrentPainScore = table.Column<int>(type: "int", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakePainScreen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakePainScreen_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeColumbiaSuicide_TcmClient_FK",
                table: "TCMIntakeColumbiaSuicide",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakePainScreen_TcmClient_FK",
                table: "TCMIntakePainScreen",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeColumbiaSuicide");

            migrationBuilder.DropTable(
                name: "TCMIntakePainScreen");
        }
    }
}
