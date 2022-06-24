using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMFarsForm_TCMClient_TCMClientId",
                table: "TCMFarsForm");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Medication",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonPurpose",
                table: "Medication",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TcmAssessmentId",
                table: "Medication",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMAssessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    DateAssessment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientInput = table.Column<bool>(type: "bit", nullable: false),
                    Family = table.Column<bool>(type: "bit", nullable: false),
                    Referring = table.Column<bool>(type: "bit", nullable: false),
                    School = table.Column<bool>(type: "bit", nullable: false),
                    Treating = table.Column<bool>(type: "bit", nullable: false),
                    Caregiver = table.Column<bool>(type: "bit", nullable: false),
                    Review = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    OtherExplain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentingProblems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfOnSetPresentingProblem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PresentingProblemPrevious = table.Column<bool>(type: "bit", nullable: false),
                    ChildMother = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildFather = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Married = table.Column<bool>(type: "bit", nullable: false),
                    Divorced = table.Column<bool>(type: "bit", nullable: false),
                    Separated = table.Column<bool>(type: "bit", nullable: false),
                    NeverMarried = table.Column<bool>(type: "bit", nullable: false),
                    AreChild = table.Column<bool>(type: "bit", nullable: false),
                    AreChildName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreChildPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreChildAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreChildCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MayWe = table.Column<bool>(type: "bit", nullable: false),
                    MayWeNA = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesByFollowing = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesPill = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesFamily = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesCalendar = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesElectronic = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesRNHHA = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesKeeping = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesDaily = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesOther = table.Column<bool>(type: "bit", nullable: false),
                    HowDoesOtherExplain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HowWeelWithNo = table.Column<bool>(type: "bit", nullable: false),
                    HowWeelWithALot = table.Column<bool>(type: "bit", nullable: false),
                    HowWeelWithSome = table.Column<bool>(type: "bit", nullable: false),
                    HowWeelEnable = table.Column<bool>(type: "bit", nullable: false),
                    HasTheClient = table.Column<bool>(type: "bit", nullable: false),
                    WhatPharmacy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PharmacyPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnyOther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessment_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentHouseComposition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    RelationShip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Supporting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentHouseComposition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentHouseComposition_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentIndividualAgency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Agency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationShip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentIndividualAgency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentIndividualAgency_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentPastCurrentService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    TypeService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderAgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Efectiveness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentPastCurrentService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentPastCurrentService_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medication_TcmAssessmentId",
                table: "Medication",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessment_TcmClient_FK",
                table: "TCMAssessment",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentHouseComposition_TcmAssessmentId",
                table: "TCMAssessmentHouseComposition",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentIndividualAgency_TcmAssessmentId",
                table: "TCMAssessmentIndividualAgency",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentPastCurrentService_TcmAssessmentId",
                table: "TCMAssessmentPastCurrentService",
                column: "TcmAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_TCMAssessment_TcmAssessmentId",
                table: "Medication",
                column: "TcmAssessmentId",
                principalTable: "TCMAssessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMFarsForm_TCMClient_TCMClientId",
                table: "TCMFarsForm",
                column: "TCMClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_TCMAssessment_TcmAssessmentId",
                table: "Medication");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMFarsForm_TCMClient_TCMClientId",
                table: "TCMFarsForm");

            migrationBuilder.DropTable(
                name: "TCMAssessmentHouseComposition");

            migrationBuilder.DropTable(
                name: "TCMAssessmentIndividualAgency");

            migrationBuilder.DropTable(
                name: "TCMAssessmentPastCurrentService");

            migrationBuilder.DropTable(
                name: "TCMAssessment");

            migrationBuilder.DropIndex(
                name: "IX_Medication_TcmAssessmentId",
                table: "Medication");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Medication");

            migrationBuilder.DropColumn(
                name: "ReasonPurpose",
                table: "Medication");

            migrationBuilder.DropColumn(
                name: "TcmAssessmentId",
                table: "Medication");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMFarsForm_TCMClient_TCMClientId",
                table: "TCMFarsForm",
                column: "TCMClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
