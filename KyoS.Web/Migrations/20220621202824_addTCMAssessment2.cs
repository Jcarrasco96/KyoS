using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_TCMAssessment_TcmAssessmentId",
                table: "Medication");

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

            migrationBuilder.CreateTable(
                name: "TCMAssessmentMedication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    ReasonPurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prescriber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentMedication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentMedication_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentMedication_TcmAssessmentId",
                table: "TCMAssessmentMedication",
                column: "TcmAssessmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMAssessmentMedication");

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

            migrationBuilder.CreateIndex(
                name: "IX_Medication_TcmAssessmentId",
                table: "Medication",
                column: "TcmAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_TCMAssessment_TcmAssessmentId",
                table: "Medication",
                column: "TcmAssessmentId",
                principalTable: "TCMAssessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
