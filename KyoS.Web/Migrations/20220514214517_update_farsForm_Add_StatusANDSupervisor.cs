using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_farsForm_Add_StatusANDSupervisor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FarsForm",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "FarsForm",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarsForm_SupervisorId",
                table: "FarsForm",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarsForm_Supervisors_SupervisorId",
                table: "FarsForm",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarsForm_Supervisors_SupervisorId",
                table: "FarsForm");

            migrationBuilder.DropIndex(
                name: "IX_FarsForm_SupervisorId",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "FarsForm");
        }
    }
}
