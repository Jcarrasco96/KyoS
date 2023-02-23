using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class improvesGroupNotesAcitity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes_Activities_GroupNotes2_GroupNote2EntityId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes2_Workdays_Clients_Workday_CientId",
                table: "GroupNotes2");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes2_Workday_CientId",
                table: "GroupNotes2");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes_Activities_GroupNote2EntityId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropColumn(
                name: "Workday_CientId",
                table: "GroupNotes2");

            migrationBuilder.DropColumn(
                name: "GroupNote2EntityId",
                table: "GroupNotes_Activities");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Workday_Client_FK",
                table: "GroupNotes2",
                column: "Workday_Client_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes2_Workdays_Clients_Workday_Client_FK",
                table: "GroupNotes2",
                column: "Workday_Client_FK",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes2_Workdays_Clients_Workday_Client_FK",
                table: "GroupNotes2");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes2_Workday_Client_FK",
                table: "GroupNotes2");

            migrationBuilder.AddColumn<int>(
                name: "Workday_CientId",
                table: "GroupNotes2",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupNote2EntityId",
                table: "GroupNotes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Workday_CientId",
                table: "GroupNotes2",
                column: "Workday_CientId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_GroupNote2EntityId",
                table: "GroupNotes_Activities",
                column: "GroupNote2EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes_Activities_GroupNotes2_GroupNote2EntityId",
                table: "GroupNotes_Activities",
                column: "GroupNote2EntityId",
                principalTable: "GroupNotes2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes2_Workdays_Clients_Workday_CientId",
                table: "GroupNotes2",
                column: "Workday_CientId",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
