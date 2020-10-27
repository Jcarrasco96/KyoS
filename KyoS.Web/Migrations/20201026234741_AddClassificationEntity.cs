using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddClassificationEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Classifications_ClassificationEntity_ClassificationId",
                table: "Objetives_Classifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassificationEntity",
                table: "ClassificationEntity");

            migrationBuilder.RenameTable(
                name: "ClassificationEntity",
                newName: "Classifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classifications",
                table: "Classifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Classifications_Classifications_ClassificationId",
                table: "Objetives_Classifications",
                column: "ClassificationId",
                principalTable: "Classifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Classifications_Classifications_ClassificationId",
                table: "Objetives_Classifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classifications",
                table: "Classifications");

            migrationBuilder.RenameTable(
                name: "Classifications",
                newName: "ClassificationEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassificationEntity",
                table: "ClassificationEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Classifications_ClassificationEntity_ClassificationId",
                table: "Objetives_Classifications",
                column: "ClassificationId",
                principalTable: "ClassificationEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
