using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ImprovesIncidentAndSendMenssage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IncidentId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAsignedId",
                table: "Incidents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "clientId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IncidentId",
                table: "Messages",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_clientId",
                table: "Incidents",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_UserAsignedId",
                table: "Incidents",
                column: "UserAsignedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_AspNetUsers_UserAsignedId",
                table: "Incidents",
                column: "UserAsignedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Clients_clientId",
                table: "Incidents",
                column: "clientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Incidents_IncidentId",
                table: "Messages",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_AspNetUsers_UserAsignedId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Clients_clientId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Incidents_IncidentId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_IncidentId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_clientId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_UserAsignedId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserAsignedId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "clientId",
                table: "Incidents");
        }
    }
}
