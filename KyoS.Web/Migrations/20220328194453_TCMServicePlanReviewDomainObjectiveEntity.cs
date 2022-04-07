using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlanReviewDomainObjectiveEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMServicePlanReviewDomainObjectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdObjective = table.Column<int>(type: "int", nullable: false),
                    DateEndObjective = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCMServicePlanReviewDomainEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMServicePlanReviewDomainObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomains_TCMServicePlanReviewDomainEntityId",
                        column: x => x.TCMServicePlanReviewDomainEntityId,
                        principalTable: "TCMServicePlanReviewDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviewDomainObjectives_Id",
                table: "TCMServicePlanReviewDomainObjectives",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomainEntityId",
                table: "TCMServicePlanReviewDomainObjectives",
                column: "TCMServicePlanReviewDomainEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMServicePlanReviewDomainObjectives");
        }
    }
}
