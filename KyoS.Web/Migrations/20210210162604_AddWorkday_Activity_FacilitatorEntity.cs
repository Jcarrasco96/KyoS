using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddWorkday_Activity_FacilitatorEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workdays_Activities_Facilitators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkdayId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays_Activities_Facilitators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workdays_Activities_Facilitators_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workdays_Activities_Facilitators_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workdays_Activities_Facilitators_Workdays_WorkdayId",
                        column: x => x.WorkdayId,
                        principalTable: "Workdays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Activities_Facilitators_ActivityId",
                table: "Workdays_Activities_Facilitators",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Activities_Facilitators_FacilitatorId",
                table: "Workdays_Activities_Facilitators",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Activities_Facilitators_WorkdayId",
                table: "Workdays_Activities_Facilitators",
                column: "WorkdayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workdays_Activities_Facilitators");
        }
    }
}
