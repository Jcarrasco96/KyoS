using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Review_BIOandMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensedPractitioner",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "UnlicensedTherapist",
                table: "Bio");

            migrationBuilder.AddColumn<int>(
                name: "DocumentsAssistantId",
                table: "Bio",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Bio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bio_DocumentsAssistantId",
                table: "Bio",
                column: "DocumentsAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Bio_SupervisorId",
                table: "Bio",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bio_DocumentsAssistant_DocumentsAssistantId",
                table: "Bio",
                column: "DocumentsAssistantId",
                principalTable: "DocumentsAssistant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bio_Supervisors_SupervisorId",
                table: "Bio",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bio_DocumentsAssistant_DocumentsAssistantId",
                table: "Bio");

            migrationBuilder.DropForeignKey(
                name: "FK_Bio_Supervisors_SupervisorId",
                table: "Bio");

            migrationBuilder.DropIndex(
                name: "IX_Bio_DocumentsAssistantId",
                table: "Bio");

            migrationBuilder.DropIndex(
                name: "IX_Bio_SupervisorId",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "DocumentsAssistantId",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Bio");

            migrationBuilder.AddColumn<string>(
                name: "LicensedPractitioner",
                table: "Bio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnlicensedTherapist",
                table: "Bio",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
