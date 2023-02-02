using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class improvesMessage_includeBrief : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Brief_BriefEntityId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "BriefEntityId",
                table: "Messages",
                newName: "BriefId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_BriefEntityId",
                table: "Messages",
                newName: "IX_Messages_BriefId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Brief_BriefId",
                table: "Messages",
                column: "BriefId",
                principalTable: "Brief",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Brief_BriefId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "BriefId",
                table: "Messages",
                newName: "BriefEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_BriefId",
                table: "Messages",
                newName: "IX_Messages_BriefEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Brief_BriefEntityId",
                table: "Messages",
                column: "BriefEntityId",
                principalTable: "Brief",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
