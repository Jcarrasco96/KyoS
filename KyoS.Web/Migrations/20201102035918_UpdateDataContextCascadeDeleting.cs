using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateDataContextCascadeDeleting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Classifications_Objetives_ObjetiveId",
                table: "Objetives_Classifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Classifications_Objetives_ObjetiveId",
                table: "Objetives_Classifications",
                column: "ObjetiveId",
                principalTable: "Objetives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Classifications_Objetives_ObjetiveId",
                table: "Objetives_Classifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Classifications_Objetives_ObjetiveId",
                table: "Objetives_Classifications",
                column: "ObjetiveId",
                principalTable: "Objetives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
