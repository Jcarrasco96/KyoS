using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddPlanEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Notes_DailySessions_Clients",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlanEntity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plan_Classification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PlanId = table.Column<int>(nullable: true),
                    ClassificationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan_Classification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plan_Classification_Classifications_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "Classifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plan_Classification_PlanEntity_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PlanEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DailySessions_Clients_PlanId",
                table: "Notes_DailySessions_Clients",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_Classification_ClassificationId",
                table: "Plan_Classification",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_Classification_PlanId",
                table: "Plan_Classification",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_DailySessions_Clients_PlanEntity_PlanId",
                table: "Notes_DailySessions_Clients",
                column: "PlanId",
                principalTable: "PlanEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_DailySessions_Clients_PlanEntity_PlanId",
                table: "Notes_DailySessions_Clients");

            migrationBuilder.DropTable(
                name: "Plan_Classification");

            migrationBuilder.DropTable(
                name: "PlanEntity");

            migrationBuilder.DropIndex(
                name: "IX_Notes_DailySessions_Clients_PlanId",
                table: "Notes_DailySessions_Clients");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Notes_DailySessions_Clients");
        }
    }
}
