using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateDataContextPlanEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_DailySessions_Clients_PlanEntity_PlanId",
                table: "Notes_DailySessions_Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Classification_Classifications_ClassificationId",
                table: "Plan_Classification");

            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Classification_PlanEntity_PlanId",
                table: "Plan_Classification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanEntity",
                table: "PlanEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Plan_Classification",
                table: "Plan_Classification");

            migrationBuilder.RenameTable(
                name: "PlanEntity",
                newName: "Plans");

            migrationBuilder.RenameTable(
                name: "Plan_Classification",
                newName: "Plans_Classifications");

            migrationBuilder.RenameIndex(
                name: "IX_Plan_Classification_PlanId",
                table: "Plans_Classifications",
                newName: "IX_Plans_Classifications_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Plan_Classification_ClassificationId",
                table: "Plans_Classifications",
                newName: "IX_Plans_Classifications_ClassificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Plans",
                table: "Plans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Plans_Classifications",
                table: "Plans_Classifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_DailySessions_Clients_Plans_PlanId",
                table: "Notes_DailySessions_Clients",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Classifications_Classifications_ClassificationId",
                table: "Plans_Classifications",
                column: "ClassificationId",
                principalTable: "Classifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Classifications_Plans_PlanId",
                table: "Plans_Classifications",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_DailySessions_Clients_Plans_PlanId",
                table: "Notes_DailySessions_Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Classifications_Classifications_ClassificationId",
                table: "Plans_Classifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Classifications_Plans_PlanId",
                table: "Plans_Classifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Plans_Classifications",
                table: "Plans_Classifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Plans",
                table: "Plans");

            migrationBuilder.RenameTable(
                name: "Plans_Classifications",
                newName: "Plan_Classification");

            migrationBuilder.RenameTable(
                name: "Plans",
                newName: "PlanEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Plans_Classifications_PlanId",
                table: "Plan_Classification",
                newName: "IX_Plan_Classification_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Plans_Classifications_ClassificationId",
                table: "Plan_Classification",
                newName: "IX_Plan_Classification_ClassificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Plan_Classification",
                table: "Plan_Classification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanEntity",
                table: "PlanEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_DailySessions_Clients_PlanEntity_PlanId",
                table: "Notes_DailySessions_Clients",
                column: "PlanId",
                principalTable: "PlanEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Classification_Classifications_ClassificationId",
                table: "Plan_Classification",
                column: "ClassificationId",
                principalTable: "Classifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Classification_PlanEntity_PlanId",
                table: "Plan_Classification",
                column: "PlanId",
                principalTable: "PlanEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
