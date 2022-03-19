using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlanReview_V11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMServicePlanReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateServicePlanReview = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOpending = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SummaryProgress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recomendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TcmServicePlanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMServicePlanReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMServicePlanReviews_TCMServicePlans_TcmServicePlanId",
                        column: x => x.TcmServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMServicePlanReviewDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangesUpdate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TcmServicePlanId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TCMServicePlanReviewEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMServicePlanReviewDomains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMServicePlanReviewDomains_TCMServicePlanReviews_TCMServicePlanReviewEntityId",
                        column: x => x.TCMServicePlanReviewEntityId,
                        principalTable: "TCMServicePlanReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMServicePlanReviewDomains_TCMServicePlans_TcmServicePlanId",
                        column: x => x.TcmServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviewDomains_Id",
                table: "TCMServicePlanReviewDomains",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviewDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviewDomains_TCMServicePlanReviewEntityId",
                table: "TCMServicePlanReviewDomains",
                column: "TCMServicePlanReviewEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_Id",
                table: "TCMServicePlanReviews",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_TcmServicePlanId",
                table: "TCMServicePlanReviews",
                column: "TcmServicePlanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMServicePlanReviewDomains");

            migrationBuilder.DropTable(
                name: "TCMServicePlanReviews");
        }
    }
}
