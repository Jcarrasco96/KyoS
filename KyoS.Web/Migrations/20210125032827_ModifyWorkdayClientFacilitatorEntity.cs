using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyWorkdayClientFacilitatorEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacilitatorId",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_FacilitatorId",
                table: "Workdays_Clients",
                column: "FacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_Clients_Facilitators_FacilitatorId",
                table: "Workdays_Clients",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_Clients_Facilitators_FacilitatorId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_Workdays_Clients_FacilitatorId",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "FacilitatorId",
                table: "Workdays_Clients");
        }
    }
}
