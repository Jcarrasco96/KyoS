using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyGroupEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Facilitators_FacilitatorEntityId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "FacilitatorEntityId",
                table: "Groups",
                newName: "FacilitatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_FacilitatorEntityId",
                table: "Groups",
                newName: "IX_Groups_FacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Facilitators_FacilitatorId",
                table: "Groups",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Facilitators_FacilitatorId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "FacilitatorId",
                table: "Groups",
                newName: "FacilitatorEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_FacilitatorId",
                table: "Groups",
                newName: "IX_Groups_FacilitatorEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Facilitators_FacilitatorEntityId",
                table: "Groups",
                column: "FacilitatorEntityId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
