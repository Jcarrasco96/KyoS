using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class improvesTCMMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRead = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TCMFarsFormId = table.Column<int>(type: "int", nullable: true),
                    TCMNoteId = table.Column<int>(type: "int", nullable: true),
                    TCMAssessmentId = table.Column<int>(type: "int", nullable: true),
                    TCMServicePlanId = table.Column<int>(type: "int", nullable: true),
                    TCMServicePlanReviewId = table.Column<int>(type: "int", nullable: true),
                    TCMAddendumId = table.Column<int>(type: "int", nullable: true),
                    TCMDischargeId = table.Column<int>(type: "int", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notification = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMAdendums_TCMAddendumId",
                        column: x => x.TCMAddendumId,
                        principalTable: "TCMAdendums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMAssessment_TCMAssessmentId",
                        column: x => x.TCMAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMDischarge_TCMDischargeId",
                        column: x => x.TCMDischargeId,
                        principalTable: "TCMDischarge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMFarsForm_TCMFarsFormId",
                        column: x => x.TCMFarsFormId,
                        principalTable: "TCMFarsForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMNote_TCMNoteId",
                        column: x => x.TCMNoteId,
                        principalTable: "TCMNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMServicePlanReviews_TCMServicePlanReviewId",
                        column: x => x.TCMServicePlanReviewId,
                        principalTable: "TCMServicePlanReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMMessages_TCMServicePlans_TCMServicePlanId",
                        column: x => x.TCMServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMAddendumId",
                table: "TCMMessages",
                column: "TCMAddendumId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMAssessmentId",
                table: "TCMMessages",
                column: "TCMAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMDischargeId",
                table: "TCMMessages",
                column: "TCMDischargeId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMFarsFormId",
                table: "TCMMessages",
                column: "TCMFarsFormId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMNoteId",
                table: "TCMMessages",
                column: "TCMNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMServicePlanId",
                table: "TCMMessages",
                column: "TCMServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMMessages_TCMServicePlanReviewId",
                table: "TCMMessages",
                column: "TCMServicePlanReviewId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMMessages");
        }
    }
}
