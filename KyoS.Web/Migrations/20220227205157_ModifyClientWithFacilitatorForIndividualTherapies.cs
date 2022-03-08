using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyClientWithFacilitatorForIndividualTherapies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndividualTherapyFacilitatorId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IndividualTherapyFacilitatorId",
                table: "Clients",
                column: "IndividualTherapyFacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Facilitators_IndividualTherapyFacilitatorId",
                table: "Clients",
                column: "IndividualTherapyFacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Facilitators_IndividualTherapyFacilitatorId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IndividualTherapyFacilitatorId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IndividualTherapyFacilitatorId",
                table: "Clients");
        }
    }
}
