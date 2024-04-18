using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class improvesSafetyPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyPlan_Clients_Client_FK",
                table: "SafetyPlan");

            migrationBuilder.DropIndex(
                name: "IX_SafetyPlan_Client_FK",
                table: "SafetyPlan");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "SafetyPlan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_ClientId",
                table: "SafetyPlan",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyPlan_Clients_ClientId",
                table: "SafetyPlan",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyPlan_Clients_ClientId",
                table: "SafetyPlan");

            migrationBuilder.DropIndex(
                name: "IX_SafetyPlan_ClientId",
                table: "SafetyPlan");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "SafetyPlan");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_Client_FK",
                table: "SafetyPlan",
                column: "Client_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyPlan_Clients_Client_FK",
                table: "SafetyPlan",
                column: "Client_FK",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
