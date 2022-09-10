using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Review_MTPandBIO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MTPs_MTPEntityId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "MTPEntityId",
                table: "Messages",
                newName: "MtpId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_MTPEntityId",
                table: "Messages",
                newName: "IX_Messages_MtpId");

            migrationBuilder.AddColumn<int>(
                name: "BioId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_BioId",
                table: "Messages",
                column: "BioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Bio_BioId",
                table: "Messages",
                column: "BioId",
                principalTable: "Bio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MTPs_MtpId",
                table: "Messages",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Bio_BioId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MTPs_MtpId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_BioId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "BioId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "MtpId",
                table: "Messages",
                newName: "MTPEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_MtpId",
                table: "Messages",
                newName: "IX_Messages_MTPEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MTPs_MTPEntityId",
                table: "Messages",
                column: "MTPEntityId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
